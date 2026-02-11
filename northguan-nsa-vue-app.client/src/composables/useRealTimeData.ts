import { ref, onMounted, onUnmounted } from "vue";

import {
  StationService,
  ParkingOverviewService,
  CrowdOverviewService,
  TrafficOverviewService,
  FenceOverviewService,
  MapService,
} from "@/services";
import type {
  ICrowdHistoryData,
  ICrowdRateData,
  IFenceHistoryData,
  IFenceRecentRecordDetail,
  ILandmarkItem,
  IParkingHistoryData,
  IParkingRateData,
  ITrafficConditionData,
  ITrafficHistoryData,
  LandmarkItem,
  StationResponse,
} from "@/api/client";

export type ParkingHotspotItem = IParkingRateData[];
export type CrowdHotspotItem = ICrowdRateData[];
export type TrafficHotspotItem = ITrafficConditionData[];
export type FenceEventItem = IFenceRecentRecordDetail[];
export type HighResolutionDeviceItem = ILandmarkItem[];

export interface Station {
  id: number;
  name: string;
}

export interface RealTimeDataState {
  loading: boolean;
  lastUpdateTime: string;
  updateInterval: number;
  autoRefresh: boolean;
}

export function useRealTimeData() {
  const state = ref<RealTimeDataState>({
    loading: true,
    lastUpdateTime: "",
    updateInterval: 30000,
    autoRefresh: true,
  });

  const selectedStation = ref("all");
  const stations = ref<Station[]>([]);
  const parkingHotspots = ref<IParkingRateData[]>([]);
  const crowdHotspots = ref<ICrowdRateData[]>([]);
  const trafficHotspots = ref<ITrafficConditionData[]>([]);
  const fenceEvents = ref<IFenceRecentRecordDetail[]>([]);
  const highResolutionDevices = ref<ILandmarkItem[]>([]);

  // 歷史數據 - 只在點擊標點時載入
  const parkingHistory = ref<IParkingHistoryData[]>([]);
  const crowdHistory = ref<ICrowdHistoryData[]>([]);
  const trafficHistory = ref<ITrafficHistoryData[]>([]);
  const fenceHistory = ref<IFenceHistoryData[]>([]);

  let refreshTimer: any | null = null;

  const validateStatus = (status: unknown): "online" | "offline" | null => {
    if (typeof status !== "string") return null;
    const validStatuses = ["online", "offline"] as const;
    return validStatuses.includes(status as any) ? (status as any) : null;
  };

  const loadStations = async (): Promise<void> => {
    try {
      const response = await StationService.getStations();
      stations.value = response
        .filter((station: StationResponse) => station.id != null && station.name != null)
        .map((station: StationResponse) => ({
          id: station.id!,
          name: station.name!,
        }));
    } catch (error) {
      console.error("載入站點失敗:", error);
      stations.value = [];
    }
  };

  // 共享的 landmarks 數據，避免重複呼叫
  let cachedLandmarks: LandmarkItem[] | null = null;
  let landmarksLoadTime = 0;
  let currentLandmarksRequest: Promise<LandmarkItem[]> | null = null;
  const LANDMARKS_CACHE_DURATION = 30000; // 30秒快取

  const getLandmarksData = async (): Promise<LandmarkItem[]> => {
    const now = Date.now();

    // 如果有快取且未過期，直接返回
    if (cachedLandmarks && now - landmarksLoadTime < LANDMARKS_CACHE_DURATION) {
      console.log("使用快取的 landmarks 數據");
      return cachedLandmarks;
    }

    // 如果已經有正在進行的請求，等待該請求完成
    if (currentLandmarksRequest) {
      console.log("等待正在進行的 landmarks 請求");
      return await currentLandmarksRequest;
    }

    // 創建新的請求
    console.log("發起新的 landmarks API 請求");
    currentLandmarksRequest = (async () => {
      try {
        const landmarksResponse = await MapService.getLandmarks();
        cachedLandmarks = landmarksResponse?.landmarks ?? [];
        landmarksLoadTime = now;
        console.log("Landmarks API 請求完成，載入了", cachedLandmarks.length, "個標點");
        return cachedLandmarks;
      } catch (error) {
        console.error("載入 landmarks 失敗:", error);
        return [];
      } finally {
        // 請求完成後清除請求標記
        currentLandmarksRequest = null;
      }
    })();

    return await currentLandmarksRequest;
  };

  // 從 recent API 載入停車場熱點數據
  const loadParkingHotspots = async (stationId?: number): Promise<void> => {
    try {
      const response = await ParkingOverviewService.getRecentParkingRate(stationId, 10080, 10);

      if (response && response.data && Array.isArray(response.data)) {
        parkingHotspots.value = response.data.map((record) => ({
          id: record.deviceId || 0,
          deviceName: record.deviceName || "未知設備",
          rate: record.rate || record.averageOccupancyRate || 0,
        }));
      } else {
        parkingHotspots.value = [];
      }

      console.log("停車場熱點數據載入完成:", parkingHotspots.value.length);
    } catch (error) {
      console.error("載入停車場熱點數據失敗:", error);
      parkingHotspots.value = [];
    }
  };

  // 從 recent API 載入人流熱點數據
  const loadCrowdHotspots = async (stationId?: number): Promise<void> => {
    try {
      const response = await CrowdOverviewService.getRecentCapacityRate(stationId, 10080, 10);

      if (response && response.data && Array.isArray(response.data)) {
        crowdHotspots.value = response.data.map((record) => ({
          id: record.deviceId || 0,
          deviceName: record.deviceName || "未知設備",
          status: validateStatus(record.status) ?? "offline",
          rate: record.rate || (record.averageDensity ? record.averageDensity * 100 : 0) || 0,
          peopleNum: record.averagePeopleCount ?? 0,
          density: record.averageDensity ?? 0,
        }));
      } else {
        crowdHotspots.value = [];
      }

      console.log("人流熱點數據載入完成:", crowdHotspots.value.length);
    } catch (error) {
      console.error("載入人流熱點數據失敗:", error);
      crowdHotspots.value = [];
    }
  };

  // 從 recent API 載入交通熱點數據
  const loadTrafficHotspots = async (stationId?: number): Promise<void> => {
    try {
      const response = await TrafficOverviewService.getRecentRoadCondition(stationId, 10080, 10);

      if (response && response.data && Array.isArray(response.data)) {
        trafficHotspots.value = response.data.map((record) => ({
          id: record.deviceId || 0,
          deviceName: record.deviceName || "未知設備",
          status: validateStatus(record.status) ?? "offline",
          rate: record.rate || record.averageSpeed || 0,
          speed: record.averageSpeed ?? 0,
          stationName: record.stationName || "",
        }));
      } else {
        trafficHotspots.value = [];
      }

      console.log("交通熱點數據載入完成:", trafficHotspots.value.length);
    } catch (error) {
      console.error("載入交通熱點數據失敗:", error);
      trafficHotspots.value = [];
    }
  };

  // 從 recent API 載入圍籬事件數據
  const loadFenceHotspots = async (stationId?: number): Promise<void> => {
    try {
      const response = await FenceOverviewService.getRecentRecord(stationId, 10080, 10);

      if (response && response.data && Array.isArray(response.data)) {
        fenceEvents.value = response.data.map((record) => ({
          id: record.id || Math.random(),
          deviceName: record.deviceName || "未知設備",
          event: record.event || "",
          date: record.date,
          time: record.time,
          imageUrl: record.imageUrl || "",
          eventType: record.eventType || 0,
        }));
      } else {
        fenceEvents.value = [];
      }

      console.log("圍籬事件數據載入完成:", fenceEvents.value.length);
    } catch (error) {
      console.error("載入圍籬事件數據失敗:", error);
      fenceEvents.value = [];
    }
  };

  // 載入高解析度設備數據
  const loadHighResolutionDevices = async (stationId?: number): Promise<void> => {
    try {
      // 獲取 landmarks 數據
      const landmarks = await getLandmarksData();

      // 根據 stationId 過濾 landmarks 並取得高解析度設備
      const filteredLandmarks = stationId
        ? landmarks.filter((l) => l.stationID === stationId)
        : landmarks;

      const highResolutionLandmarks = filteredLandmarks.filter((l) => l.type === "highResolution");

      // 賦值給 highResolutionDevices
      highResolutionDevices.value = highResolutionLandmarks || [];

      console.log("高解析度設備數據載入完成:", highResolutionDevices.value.length);
    } catch (error) {
      console.error("載入高解析度設備數據失敗:", error);
      highResolutionDevices.value = [];
    }
  };

  // 載入特定設備的歷史數據（點擊標點時調用）
  const loadDeviceHistory = async (
    deviceName: string,
    deviceType: "parking" | "crowd" | "traffic" | "fence",
    stationId?: number
  ): Promise<void> => {
    try {
      console.log(`載入 ${deviceType} 設備 ${deviceName} 的歷史數據`);

      const timeRange = 86400; // 24小時的秒數

      switch (deviceType) {
        case "parking":
          const parkingResponse = await ParkingOverviewService.getRecentConversionHistory(
            stationId,
            timeRange
          );
          if (parkingResponse && parkingResponse.data) {
            parkingHistory.value = parkingResponse.data.filter(
              (record) => record.deviceName === deviceName
            );
          }
          break;

        case "crowd":
          const crowdResponse = await CrowdOverviewService.getRecentCapacityHistory(
            stationId,
            timeRange
          );
          if (crowdResponse && crowdResponse.data) {
            crowdHistory.value = crowdResponse.data.filter(
              (record) => record.deviceName === deviceName
            );
          }
          break;

        case "traffic":
          const trafficResponse = await TrafficOverviewService.getRecentRoadConditionHistory(
            stationId,
            timeRange
          );
          if (trafficResponse && trafficResponse.data) {
            trafficHistory.value = trafficResponse.data.filter(
              (record) => record.deviceName === deviceName
            );
          }
          break;

        case "fence":
          const fenceResponse = await FenceOverviewService.getRecentRecordHistory(
            stationId,
            timeRange
          );
          if (fenceResponse && fenceResponse.data) {
            fenceHistory.value = fenceResponse.data.filter(
              (record) => record.deviceName === deviceName
            );
          }
          break;
      }
    } catch (error) {
      console.error(`載入 ${deviceType} 設備歷史數據失敗:`, error);
    }
  };

  // 清空歷史數據
  const clearHistoryData = (): void => {
    parkingHistory.value = [];
    crowdHistory.value = [];
    trafficHistory.value = [];
    fenceHistory.value = [];
  };

  // 添加一個標記來區分初次載入和刷新
  const isInitialLoad = ref(true);

  const loadDashboardData = async (): Promise<void> => {
    // 只在初次載入時顯示 loading 狀態
    if (isInitialLoad.value) {
      state.value.loading = true;
    }

    try {
      const stationId =
        selectedStation.value && selectedStation.value !== "all"
          ? parseInt(selectedStation.value)
          : undefined;

      // 載入熱點數據（用於 HomeIntegrated 頁面的熱點卡片）
      await Promise.all([
        loadParkingHotspots(stationId),
        loadCrowdHotspots(stationId),
        loadTrafficHotspots(stationId),
        loadFenceHotspots(stationId),
        loadHighResolutionDevices(stationId),
      ]);

      console.log("熱點數據載入完成:", {
        parking: parkingHotspots.value.length,
        crowd: crowdHotspots.value.length,
        traffic: trafficHotspots.value.length,
        fence: fenceEvents.value.length,
        highResolution: highResolutionDevices.value.length,
      });

      state.value.lastUpdateTime = new Date().toLocaleString();

      // 標記初次載入完成
      if (isInitialLoad.value) {
        isInitialLoad.value = false;
      }
    } catch (error) {
      console.error("載入儀表板資料失敗:", error);
    } finally {
      // 只在初次載入時關閉 loading 狀態
      if (state.value.loading) {
        state.value.loading = false;
      }
    }
  };

  // 新增：載入地圖標點數據（用於 HomeMap 組件）
  const loadMapLandmarks = async (
    stationId?: number
  ): Promise<{
    parkingLandmarks: ILandmarkItem[];
    crowdLandmarks: ILandmarkItem[];
    trafficLandmarks: ILandmarkItem[];
    fenceLandmarks: ILandmarkItem[];
    highResolutionLandmarks: ILandmarkItem[];
  }> => {
    try {
      // 只呼叫一次 landmarks API，然後分發給各個設備類型
      const landmarks = await getLandmarksData();

      // 根據 stationId 過濾 landmarks (注意後端使用 StationID)
      const filteredLandmarks = stationId
        ? landmarks.filter((l) => l.stationID === stationId)
        : landmarks;

      const parkingLandmarks = filteredLandmarks.filter((l) => l.type === "parking");
      const crowdLandmarks = filteredLandmarks.filter((l) => l.type === "crowd");
      const trafficLandmarks = filteredLandmarks.filter((l) => l.type === "traffic");
      const fenceLandmarks = filteredLandmarks.filter((l) => l.type === "fence");
      const highResolutionLandmarks = filteredLandmarks.filter((l) => l.type === "highResolution");

      return {
        parkingLandmarks: parkingLandmarks || [],
        crowdLandmarks: crowdLandmarks || [],
        trafficLandmarks: trafficLandmarks || [],
        fenceLandmarks: fenceLandmarks || [],
        highResolutionLandmarks: highResolutionLandmarks || [],
      };
    } catch (error) {
      console.error("載入地圖標點數據失敗:", error);
      return {
        parkingLandmarks: [],
        crowdLandmarks: [],
        trafficLandmarks: [],
        fenceLandmarks: [],
        highResolutionLandmarks: [],
      };
    }
  };

  const startAutoRefresh = (): void => {
    if (refreshTimer) clearInterval(refreshTimer);

    if (state.value.autoRefresh && state.value.updateInterval > 0) {
      refreshTimer = setInterval(() => {
        loadDashboardData();
      }, state.value.updateInterval);
    }
  };

  const stopAutoRefresh = (): void => {
    if (refreshTimer) {
      clearInterval(refreshTimer);
      refreshTimer = null;
    }
  };

  const refreshData = async (): Promise<void> => {
    await loadDashboardData();
  };

  // 清除 landmarks 快取
  const clearLandmarksCache = (): void => {
    cachedLandmarks = null;
    landmarksLoadTime = 0;
    currentLandmarksRequest = null;
    console.log("Landmarks 快取已清除");
  };

  const onStationChange = async (): Promise<void> => {
    // 站點切換時清除快取並重置為初次載入狀態，顯示 loading
    clearLandmarksCache();
    isInitialLoad.value = true;
    await loadDashboardData();
  };

  const setUpdateInterval = (interval: number): void => {
    state.value.updateInterval = interval;

    // 如果設置為 0，表示永不刷新
    if (interval === 0) {
      state.value.autoRefresh = false;
      stopAutoRefresh();
    } else {
      // 確保最小間隔為 5 秒（除了 0）
      state.value.updateInterval = Math.max(5000, interval);
      // 從永不刷新切換回來時，重新啟動自動刷新
      state.value.autoRefresh = true;
      startAutoRefresh();
    }
  };

  const toggleAutoRefresh = (): void => {
    state.value.autoRefresh = !state.value.autoRefresh;
    if (state.value.autoRefresh) {
      startAutoRefresh();
    } else {
      stopAutoRefresh();
    }
  };

  onMounted(() => {
    startAutoRefresh();
  });

  onUnmounted(() => {
    stopAutoRefresh();
  });

  return {
    // 狀態
    state,
    selectedStation,
    stations,

    // 即時數據
    parkingHotspots,
    crowdHotspots,
    trafficHotspots,
    fenceEvents,
    highResolutionDevices,

    // 歷史數據（按需載入）
    parkingHistory,
    crowdHistory,
    trafficHistory,
    fenceHistory,

    // 方法
    loadStations,
    loadDashboardData,
    loadMapLandmarks,
    loadDeviceHistory,
    clearHistoryData,
    clearLandmarksCache,
    refreshData,
    onStationChange,
    setUpdateInterval,
    toggleAutoRefresh,
    startAutoRefresh,
    stopAutoRefresh,
  };
}
