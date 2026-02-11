import { handleApiError } from '@/utils/errorHandler'
import {
  parkingClient,
  ParkingRecordListResponse,
  ParkingConversionHistoryResponse,
  ParkingRateResponse,
  type FileResponse
} from '@/api/apiClients'

export class ParkingOverviewService {
  static async getRecordsList(parameters?: {
    page?: number,
    size?: number,
    keyword?: string,
    sortBy?: string,
    sortOrder?: string,
    startDate?: string,
    endDate?: string,
    stationIds?: number[],
    deviceSerials?: string[],
    minTotalSpaces?: number,
    maxTotalSpaces?: number,
    minOccupancyRate?: number,
    maxOccupancyRate?: number
  }): Promise<{
    data: ParkingRecordListResponse[],
    totalCount: number,
    page: number,
    size: number,
    totalPages: number,
    hasNextPage: boolean,
    hasPreviousPage: boolean
  }> {
    try {
      // 將參數展開為查詢字符串參數
      const response = await parkingClient.getRecordsList(
        parameters?.page,
        parameters?.size,
        parameters?.keyword,
        parameters?.stationIds
      )
      
      // 如果API返回的是數組，我們需要包裝成分頁響應格式
      if (Array.isArray(response)) {
        return {
          data: response,
          totalCount: response.length,
          page: parameters?.page || 1,
          size: parameters?.size || 20,
          totalPages: Math.ceil(response.length / (parameters?.size || 20)),
          hasNextPage: false,
          hasPreviousPage: false
        }
      }
      
      // 確保返回正確的格式
      if (response && response.success && response.data) {
        return response
      }

      // 如果響應格式不正確，返回空數據
      return {
        data: [],
        totalCount: 0,
        page: parameters?.page || 1,
        size: parameters?.size || 20,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
      }
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecordsCount(keyword?: string, availableStationIds?: number[]): Promise<number> {
    try {
      const count: number = await parkingClient.getRecordsCount(keyword, availableStationIds)
      return count
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async exportRecords(keyword?: string): Promise<Blob> {
    try {
      const response: FileResponse = await parkingClient.exportRecords(keyword)
      // FileResponse.data 是 Blob 類型，直接返回
      return response.data
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecentConversionHistory(stationID?: number, timeRange?: number): Promise<ParkingConversionHistoryResponse> {
    try {
      const response: ParkingConversionHistoryResponse = await parkingClient.getRecentConversionHistory(stationID, timeRange)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecentParkingRate(stationID?: number, timeRange?: number, limit?: number): Promise<ParkingRateResponse> {
    try {
      const response: ParkingRateResponse = await parkingClient.getRecentParkingRate(stationID, timeRange, limit)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  ParkingRecordListResponse,
  ParkingConversionHistoryResponse,
  ParkingRateResponse
}
