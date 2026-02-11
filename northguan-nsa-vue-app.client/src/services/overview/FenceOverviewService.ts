import { handleApiError } from '@/utils/errorHandler'
import {
  fenceClient,
  FenceRecordListResponse,
  FenceRecordHistoryResponse,
  FenceRecentRecordDetailResponse,
  FenceLatestRecordResponse,
  type FileResponse
} from '@/api/apiClients'

export class FenceOverviewService {
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
    eventTypes?: string[],
    groupByDevice?: boolean,
    latestOnly?: boolean
  }): Promise<{
    data: FenceRecordListResponse[],
    totalCount: number,
    page: number,
    size: number,
    totalPages: number,
    hasNextPage: boolean,
    hasPreviousPage: boolean
  }> {
    try {
      console.log('FenceOverviewService.getRecordsList 調用參數:', parameters)

      // 如果是總覽模式且需要最新記錄，使用不同的 API
      if (parameters?.latestOnly && parameters?.groupByDevice) {
        try {
          // 嘗試使用 getLatestRecord API
          const latestResponse = await fenceClient.getLatestRecord(
            parameters.stationIds?.[0]
          )

          if (latestResponse && Array.isArray(latestResponse.data)) {
            return {
              data: latestResponse.data,
              totalCount: latestResponse.data.length,
              page: 1,
              size: latestResponse.data.length,
              totalPages: 1,
              hasNextPage: false,
              hasPreviousPage: false
            }
          }
        } catch (latestError) {
          console.warn('getLatestRecord 失敗，使用普通列表 API:', latestError)
        }
      }

      // 將參數展開為查詢字符串參數
      const response = await fenceClient.getRecordsList(
        parameters?.page,
        parameters?.size,
        parameters?.keyword,
        parameters?.stationIds
      )

      console.log('FenceOverviewService.getRecordsList 響應:', response)

      // 如果API返回的是數組，我們需要包裝成分頁響應格式
      if (Array.isArray(response)) {
        let processedData = response

        // 如果需要按設備分組並只取最新記錄
        if (parameters?.groupByDevice && parameters?.latestOnly) {
          const deviceMap = new Map<string, FenceRecordListResponse>()

          response.forEach(record => {
            const deviceKey = record.deviceSerial || `station_${record.stationId}`
            const existing = deviceMap.get(deviceKey)

            if (!existing || new Date(record.timestamp) > new Date(existing.timestamp)) {
              deviceMap.set(deviceKey, record)
            }
          })

          processedData = Array.from(deviceMap.values())
        }

        return {
          data: processedData,
          totalCount: processedData.length,
          page: parameters?.page || 1,
          size: parameters?.size || 20,
          totalPages: Math.ceil(processedData.length / (parameters?.size || 20)),
          hasNextPage: false,
          hasPreviousPage: false
        }
      }

      return response
    } catch (error) {
      console.error('FenceOverviewService.getRecordsList 錯誤:', error)
      throw handleApiError(error)
    }
  }

  static async getRecordsCount(keyword?: string, availableStationIds?: number[]): Promise<number> {
    try {
      const count: number = await fenceClient.getRecordsCount(keyword, availableStationIds)
      return count
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async exportRecords(keyword?: string): Promise<Blob> {
    try {
      const response: FileResponse = await fenceClient.exportRecords(keyword)
      // FileResponse.data 是 Blob 類型，直接返回
      return response.data
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecentRecordHistory(stationID?: number, timeRange?: number): Promise<FenceRecordHistoryResponse> {
    try {
      const response: FenceRecordHistoryResponse = await fenceClient.getRecentRecordHistory(stationID, timeRange)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecentRecord(stationID?: number, timeRange?: number, limit?: number): Promise<FenceRecentRecordDetailResponse> {
    try {
      const response: FenceRecentRecordDetailResponse = await fenceClient.getRecentRecord(stationID, timeRange, limit)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getLatestRecord(stationID?: number): Promise<FenceLatestRecordResponse> {
    try {
      const response: FenceLatestRecordResponse = await fenceClient.getLatestRecord(stationID)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  FenceRecordListResponse,
  FenceRecordHistoryResponse,
  FenceRecentRecordDetailResponse,
  FenceLatestRecordResponse
}
