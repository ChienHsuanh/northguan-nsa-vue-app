import { ref } from "vue";
import {
  StationService,
  ParkingOverviewService,
  CrowdOverviewService,
  TrafficOverviewService,
  FenceOverviewService,
} from "@/services";
import type {
  CrowdRateData,
  FenceRecentRecordDetail,
  ParkingRateData,
  StationResponse,
  TrafficConditionData,
} from "@/api/client";

export interface HotspotItem {
  id: string | number;
  deviceName: string;
  status: string;
  rate: number;
}

export interface FenceEvent {
  id: string | number;
  deviceName: string;
  eventType: number;
  event: string;
  date: string;
  time: string;
  imageUrl?: string;
}

export interface Station {
  id: number;
  name: string;
}

export function useHomeData() {
  // Loading states
  const loading = ref(false);
  const stationsLoading = ref(false);

  // Data
  const selectedStation = ref("all");
  const stations = ref<Station[]>([]);
  const parkingHotspots = ref<HotspotItem[]>([]);
  const crowdHotspots = ref<HotspotItem[]>([]);
  const trafficHotspots = ref<HotspotItem[]>([]);
  const fenceEvents = ref<FenceEvent[]>([]);

  // Load stations
  const loadStations = async () => {
    stationsLoading.value = true;
    try {
      const response = await StationService.getStations();
      stations.value = response.map((station: StationResponse) => ({
        id: station.id ?? -1,
        name: station.name ?? "",
      }));
    } catch (error) {
      console.error("載入站點失敗:", error);
    } finally {
      stationsLoading.value = false;
    }
  };

  // Load dashboard data
  const loadDashboardData = async () => {
    loading.value = true;
    try {
      const stationId =
        selectedStation.value && selectedStation.value !== "all"
          ? parseInt(selectedStation.value)
          : undefined;

      // Load parking hotspots
      try {
        const parkingData = await ParkingOverviewService.getRecentParkingRate(stationId, 10080, 5);
        console.log("Parking data loaded successfully:", parkingData);

        const parkingRecords = parkingData?.data ?? [];

        parkingHotspots.value = parkingRecords.map((record: ParkingRateData, index: number) => ({
          id: record.deviceId ?? `parking-${index}`,
          deviceName: record.deviceName ?? `停車場設備 ${index + 1}`,
          status: record.status ?? "online",
          rate: record.rate ?? 0,
        }));
      } catch (error) {
        console.error("載入停車場數據失敗:", error);
        parkingHotspots.value = [];
      }

      // Load crowd hotspots
      try {
        const crowdData = await CrowdOverviewService.getRecentCapacityRate(stationId, 10080, 5);
        console.log("Crowd data loaded successfully:", crowdData);

        const crowdRecords = crowdData?.data ?? [];

        crowdHotspots.value = crowdRecords.map((record: CrowdRateData, index: number) => ({
          id: record.deviceId ?? `crowd-${index}`,
          deviceName: record.deviceName ?? `人流設備 ${index + 1}`,
          status: record.status ?? "online",
          rate: record.rate ?? 0,
        }));
      } catch (error) {
        console.error("載入人流數據失敗:", error);
        crowdHotspots.value = [];
      }

      // Load traffic hotspots
      try {
        const trafficData = await TrafficOverviewService.getRecentRoadCondition(
          stationId,
          10080,
          5
        );
        console.log("Traffic data loaded successfully:", trafficData);

        const trafficRecords = trafficData?.data ?? [];

        trafficHotspots.value = trafficRecords.map(
          (record: TrafficConditionData, index: number) => ({
            id: record.deviceId ?? `traffic-${index}`,
            deviceName: record.deviceName ?? `交通設備 ${index + 1}`,
            status: record.status ?? "online",
            rate: record.rate ?? 0,
          })
        );
      } catch (error) {
        console.error("載入交通數據失敗:", error);
        trafficHotspots.value = [];
      }

      // Load fence events
      try {
        const fenceData = await FenceOverviewService.getRecentRecord(stationId, 10080, 5);
        console.log("Fence data loaded successfully:", fenceData);

        const fenceRecords = Array.isArray(fenceData) ? fenceData : fenceData?.data ?? [];
        console.log("Fence records:", fenceRecords);

        fenceEvents.value = fenceRecords.map((record: FenceRecentRecordDetail, index: number) => {
          return {
            id: `fence-${index}`,
            deviceName: record.deviceName || `圍欄設備 ${index + 1}`,
            eventType: record.eventType || 0,
            event: record.event || "",
            time: record.time ?? "",
            timestamp: record.timestamp ?? 0,
            imageUrl: record.imageUrl || "",
          };
        });
      } catch (error) {
        console.error("載入圍欄數據失敗:", error);
        fenceEvents.value = [];
      }

      console.log("Final hotspots data:", {
        parking: parkingHotspots.value,
        crowd: crowdHotspots.value,
        traffic: trafficHotspots.value,
        fence: fenceEvents.value,
      });
    } catch (error) {
      console.error("載入儀表板資料失敗:", error);
    } finally {
      loading.value = false;
    }
  };

  // Refresh all data
  const refreshData = async () => {
    await loadDashboardData();
  };

  // Station change handler
  const onStationChange = async () => {
    await loadDashboardData();
  };

  return {
    // State
    loading,
    stationsLoading,
    selectedStation,
    stations,
    parkingHotspots,
    crowdHotspots,
    trafficHotspots,
    fenceEvents,

    // Methods
    loadStations,
    loadDashboardData,
    refreshData,
    onStationChange,
  };
}
