import { handleApiError } from "@/utils/errorHandler";
import { apiClient } from "./api";
import type { DateValue } from "@internationalized/date";

// API 查詢參數介面
export interface RecordQueryParameters {
  page?: number;
  size?: number;
  keyword?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  startDate?: string;
  endDate?: string;
  stationIds?: number[];
  deviceSerials?: string[];
}

// 分頁響應介面
export interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  size: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  success: boolean;
  message?: string;
}

// 人流記錄查詢參數
export interface CrowdRecordQueryParameters extends RecordQueryParameters {
  minPeopleCount?: number;
  maxPeopleCount?: number;
  minDensity?: number;
  maxDensity?: number;
  minArea?: number;
  maxArea?: number;
}

// 圍籬記錄查詢參數
export interface FenceRecordQueryParameters extends RecordQueryParameters {
  eventTypes?: string[];
}

// 停車記錄查詢參數
export interface ParkingRecordQueryParameters extends RecordQueryParameters {
  minTotalSpaces?: number;
  maxTotalSpaces?: number;
  minOccupancyRate?: number;
  maxOccupancyRate?: number;
}

// 車流記錄查詢參數
export interface TrafficRecordQueryParameters extends RecordQueryParameters {
  minVehicleCount?: number;
  maxVehicleCount?: number;
  minAverageSpeed?: number;
  maxAverageSpeed?: number;
  speedStatuses?: string[];
  cities?: string[];
}

// 響應 DTO 介面
export interface FenceRecordResponse {
  id: number;
  deviceSerial: string;
  deviceName: string;
  stationName: string;
  event: string;
  time: string;
  timestamp: number;
}

export interface CrowdRecordResponse {
  id: number;
  deviceSerial: string;
  deviceName: string;
  stationName: string;
  area: number;
  peopleCount: number;
  density: number;
  time: string;
  timestamp: number;
}

export interface ParkingRecordResponse {
  id: number;
  deviceSerial: string;
  deviceName: string;
  stationName: string;
  totalSpaces: number;
  parkedNum: number;
  availableSpaces: number;
  occupancyRate: number;
  time: string;
  timestamp: number;
}

export interface TrafficRecordResponse {
  id: number;
  deviceSerial: string;
  deviceName: string;
  stationName: string;
  city: string;
  eTagNumber: string;
  speedLimit: number;
  vehicleCount: number;
  averageSpeed: number;
  speedStatus: string;
  time: string;
  timestamp: number;
}

// 工具函數：將 DateValue 轉換為 API 格式
const formatDateForApi = (dateValue: DateValue): string => {
  return `${dateValue.year}-${dateValue.month.toString().padStart(2, "0")}-${dateValue.day
    .toString()
    .padStart(2, "0")}`;
};

// 工具函數：格式化本地時間為檔案名使用
const formatLocalDatetimeForFilename = (): string => {
  const now = new Date();
  const year = now.getFullYear();
  const month = (now.getMonth() + 1).toString().padStart(2, "0");
  const day = now.getDate().toString().padStart(2, "0");
  const hours = now.getHours().toString().padStart(2, "0");
  const minutes = now.getMinutes().toString().padStart(2, "0");
  const seconds = now.getSeconds().toString().padStart(2, "0");
  return `${year}-${month}-${day}_${hours}-${minutes}-${seconds}`;
};

// 工具函數：構建查詢參數
const buildQueryParams = (params: RecordQueryParameters): URLSearchParams => {
  const queryParams = new URLSearchParams();

  if (params.page) queryParams.append("page", params.page.toString());
  if (params.size) queryParams.append("size", params.size.toString());
  if (params.keyword) queryParams.append("keyword", params.keyword);
  if (params.sortBy) queryParams.append("sortBy", params.sortBy);
  if (params.sortOrder) queryParams.append("sortOrder", params.sortOrder);
  if (params.startDate) queryParams.append("startDate", params.startDate);
  if (params.endDate) queryParams.append("endDate", params.endDate);

  // 處理陣列參數
  if (params.stationIds?.length) {
    params.stationIds.forEach((id) => queryParams.append("stationIds", id.toString()));
  }
  if (params.deviceSerials?.length) {
    params.deviceSerials.forEach((serial) => queryParams.append("deviceSerials", serial));
  }

  return queryParams;
};

// Record 服務類
export class RecordService {
  // 人流記錄服務
  static async getCrowdRecords(
    params: CrowdRecordQueryParameters
  ): Promise<PagedResponse<CrowdRecordResponse>> {
    try {
      const queryParams = buildQueryParams(params);

      // 添加人流記錄特有的參數
      if (params.minPeopleCount !== undefined)
        queryParams.append("minPeopleCount", params.minPeopleCount.toString());
      if (params.maxPeopleCount !== undefined)
        queryParams.append("maxPeopleCount", params.maxPeopleCount.toString());
      if (params.minDensity !== undefined)
        queryParams.append("minDensity", params.minDensity.toString());
      if (params.maxDensity !== undefined)
        queryParams.append("maxDensity", params.maxDensity.toString());
      if (params.minArea !== undefined) queryParams.append("minArea", params.minArea.toString());
      if (params.maxArea !== undefined) queryParams.append("maxArea", params.maxArea.toString());

      const response = await apiClient.get(`/api/crowd-records?${queryParams}`);
      // 處理後端返回的Result<PagedResponse<T>>結構
      if (response.data.success && response.data.data) {
        return response.data.data;
      }

      return response.data;
    } catch (error) {
      throw handleApiError(error);
    }
  }

  static async exportCrowdRecords(params: {
    startDate: DateValue;
    endDate: DateValue;
    stationIds?: number[];
  }): Promise<void> {
    const queryParams = new URLSearchParams({
      startDate: formatDateForApi(params.startDate),
      endDate: formatDateForApi(params.endDate),
    });

    if (params.stationIds?.length) {
      params.stationIds.forEach((id) => queryParams.append("stationIds", id.toString()));
    }

    const response = await apiClient.get(`/api/crowd-records-export?${queryParams}`, {
      responseType: "blob",
      timeout: 0, // 匯出大筆資料可能會花較久的時間，因此設為無限等待
    });

    const blob = new Blob([response.data], {
      type: "text/csv",
    });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;

    const start = formatDateForApi(params.startDate);
    const end = formatDateForApi(params.endDate);
    const timestamp = formatLocalDatetimeForFilename();
    link.download = `人流記錄_${start}_${end}_${timestamp}.csv`;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  // 圍籬記錄服務
  static async getFenceRecords(
    params: FenceRecordQueryParameters
  ): Promise<PagedResponse<FenceRecordResponse>> {
    const queryParams = buildQueryParams(params);

    // 添加圍籬記錄特有的參數
    if (params.eventTypes?.length) {
      params.eventTypes.forEach((type) => queryParams.append("eventTypes", type));
    }

    const response = await apiClient.get(`/api/fence-records?${queryParams}`);
    // 處理後端返回的Result<PagedResponse<T>>結構
    if (response.data.success && response.data.data) {
      return response.data.data;
    }
    return response.data;
  }

  static async exportFenceRecords(params: {
    startDate: DateValue;
    endDate: DateValue;
    stationIds?: number[];
  }): Promise<void> {
    const queryParams = new URLSearchParams({
      startDate: formatDateForApi(params.startDate),
      endDate: formatDateForApi(params.endDate),
    });

    if (params.stationIds?.length) {
      params.stationIds.forEach((id) => queryParams.append("stationIds", id.toString()));
    }

    const response = await apiClient.get(`/api/fence-records-export?${queryParams}`, {
      responseType: "blob",
      timeout: 0, // 匯出大筆資料可能會花較久的時間，因此設為無限等待
    });

    const blob = new Blob([response.data], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;

    const start = formatDateForApi(params.startDate);
    const end = formatDateForApi(params.endDate);
    const timestamp = formatLocalDatetimeForFilename();
    link.download = `圍籬記錄_${start}_${end}_${timestamp}.xlsx`;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  // 停車記錄服務
  static async getParkingRecords(
    params: ParkingRecordQueryParameters
  ): Promise<PagedResponse<ParkingRecordResponse>> {
    const queryParams = buildQueryParams(params);

    // 添加停車記錄特有的參數
    if (params.minTotalSpaces !== undefined)
      queryParams.append("minTotalSpaces", params.minTotalSpaces.toString());
    if (params.maxTotalSpaces !== undefined)
      queryParams.append("maxTotalSpaces", params.maxTotalSpaces.toString());
    if (params.minOccupancyRate !== undefined)
      queryParams.append("minOccupancyRate", params.minOccupancyRate.toString());
    if (params.maxOccupancyRate !== undefined)
      queryParams.append("maxOccupancyRate", params.maxOccupancyRate.toString());

    const response = await apiClient.get(`/api/parking-records?${queryParams}`);
    // 處理後端返回的Result<PagedResponse<T>>結構
    if (response.data.success && response.data.data) {
      return response.data.data;
    }
    return response.data;
  }

  static async exportParkingRecords(params: {
    startDate: DateValue;
    endDate: DateValue;
    stationIds?: number[];
  }): Promise<void> {
    const queryParams = new URLSearchParams({
      startDate: formatDateForApi(params.startDate),
      endDate: formatDateForApi(params.endDate),
    });

    if (params.stationIds?.length) {
      params.stationIds.forEach((id) => queryParams.append("stationIds", id.toString()));
    }

    const response = await apiClient.get(`/api/parking-records-export?${queryParams}`, {
      responseType: "blob",
      timeout: 0, // 匯出大筆資料可能會花較久的時間，因此設為無限等待
    });

    const blob = new Blob([response.data], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;

    const start = formatDateForApi(params.startDate);
    const end = formatDateForApi(params.endDate);
    const timestamp = formatLocalDatetimeForFilename();
    link.download = `停車記錄_${start}_${end}_${timestamp}.csv`;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  // 車流記錄服務
  static async getTrafficRecords(
    params: TrafficRecordQueryParameters
  ): Promise<PagedResponse<TrafficRecordResponse>> {
    const queryParams = buildQueryParams(params);

    // 添加車流記錄特有的參數
    if (params.minVehicleCount !== undefined)
      queryParams.append("minVehicleCount", params.minVehicleCount.toString());
    if (params.maxVehicleCount !== undefined)
      queryParams.append("maxVehicleCount", params.maxVehicleCount.toString());
    if (params.minAverageSpeed !== undefined)
      queryParams.append("minAverageSpeed", params.minAverageSpeed.toString());
    if (params.maxAverageSpeed !== undefined)
      queryParams.append("maxAverageSpeed", params.maxAverageSpeed.toString());

    if (params.speedStatuses?.length) {
      params.speedStatuses.forEach((status) => queryParams.append("speedStatuses", status));
    }
    if (params.cities?.length) {
      params.cities.forEach((city) => queryParams.append("cities", city));
    }

    const response = await apiClient.get(`/api/traffic-records?${queryParams}`);
    // 處理後端返回的Result<PagedResponse<T>>結構
    if (response.data.success && response.data.data) {
      return response.data.data;
    }
    return response.data;
  }

  static async exportTrafficRecords(params: {
    startDate: DateValue;
    endDate: DateValue;
    stationIds?: number[];
  }): Promise<void> {
    const queryParams = new URLSearchParams({
      startDate: formatDateForApi(params.startDate),
      endDate: formatDateForApi(params.endDate),
    });

    if (params.stationIds?.length) {
      params.stationIds.forEach((id) => queryParams.append("stationIds", id.toString()));
    }

    const response = await apiClient.get(`/api/traffic-records-export?${queryParams}`, {
      responseType: "blob",
      timeout: 0, // 匯出大筆資料可能會花較久的時間，因此設為無限等待
    });

    const blob = new Blob([response.data], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;

    const start = formatDateForApi(params.startDate);
    const end = formatDateForApi(params.endDate);
    const timestamp = formatLocalDatetimeForFilename();
    link.download = `車流記錄_${start}_${end}_${timestamp}.csv`;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}
