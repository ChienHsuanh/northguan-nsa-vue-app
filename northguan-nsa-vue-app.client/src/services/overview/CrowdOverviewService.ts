import { handleApiError } from '@/utils/errorHandler'
import {
  crowdClient,
  CrowdRecordListResponse,
  CrowdCapacityHistoryResponse,
  CrowdCapacityRateResponse,
  type FileResponse
} from '@/api/apiClients'

export class CrowdOverviewService {
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
    minPeopleCount?: number,
    maxPeopleCount?: number,
    minDensity?: number,
    maxDensity?: number,
    minArea?: number,
    maxArea?: number
  }): Promise<{
    data: CrowdRecordListResponse[],
    totalCount: number,
    page: number,
    size: number,
    totalPages: number,
    hasNextPage: boolean,
    hasPreviousPage: boolean
  }> {
    try {
      // 將參數展開為查詢字符串參數
      const response = await crowdClient.getRecordsList(
        parameters?.minPeopleCount,
        parameters?.maxPeopleCount,
        parameters?.minDensity,
        parameters?.maxDensity,
        parameters?.minArea,
        parameters?.maxArea,
        parameters?.page,
        parameters?.size,
        parameters?.keyword,
        parameters?.sortBy,
        parameters?.sortOrder,
        parameters?.startDate && new Date(parameters.startDate),
        parameters?.endDate && new Date(parameters.endDate),
        parameters?.stationIds,
        parameters?.deviceSerials
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
      const count: number = await crowdClient.getRecordsCount(keyword, availableStationIds)
      return count
    } catch (error) {
      throw handleApiError(error)
    }
  }

  // 保持向後兼容的簡化方法
  static async getRecordsListSimple(page?: number, size?: number, keyword?: string, availableStationIds?: number[]): Promise<CrowdRecordListResponse[]> {
    try {
      const response = await this.getRecordsList({
        page,
        size,
        keyword,
        stationIds: availableStationIds
      })
      return response.data
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async exportRecords(keyword?: string, availableStationIds?: number[], format?: string): Promise<Blob> {
    try {
      const response: FileResponse = await crowdClient.exportRecords(keyword)
      // FileResponse.data 是 Blob 類型，直接返回
      return response.data
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecentCapacityHistory(stationID?: number, timeRange?: number): Promise<CrowdCapacityHistoryResponse> {
    try {
      const response: CrowdCapacityHistoryResponse = await crowdClient.getRecentCapacityHistory(stationID, timeRange)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecentCapacityRate(stationID?: number, timeRange?: number, limit?: number): Promise<CrowdCapacityRateResponse> {
    try {
      const response: CrowdCapacityRateResponse = await crowdClient.getRecentCapacityRate(stationID, timeRange, limit)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  CrowdRecordListResponse,
  CrowdCapacityHistoryResponse,
  CrowdCapacityRateResponse
}
