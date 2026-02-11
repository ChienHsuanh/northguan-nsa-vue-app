import { handleApiError } from '@/utils/errorHandler'
import {
  ResultOfPagedResponseOfTrafficRecordListResponse,
  trafficClient,
  TrafficRecordListResponse,
  TrafficRoadConditionHistoryResponse,
  TrafficRoadConditionResponse,
  type FileResponse
} from '@/api/apiClients'

export class TrafficOverviewService {
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
    minVehicleCount?: number,
    maxVehicleCount?: number,
    minAverageSpeed?: number,
    maxAverageSpeed?: number,
    speedStatuses?: string[],
    cities?: string[],
    groupByDevice?: boolean,
    latestOnly?: boolean
  }): Promise<{
    data: TrafficRecordListResponse[],
    totalCount: number,
    page: number,
    size: number,
    totalPages: number,
    hasNextPage: boolean,
    hasPreviousPage: boolean
  }> {
    try {
      let response: ResultOfPagedResponseOfTrafficRecordListResponse
      try {
        response = await trafficClient.getRecordsList(
          parameters?.minVehicleCount,
          parameters?.maxVehicleCount,
          parameters?.minAverageSpeed,
          parameters?.maxAverageSpeed,
          parameters?.speedStatuses,
          parameters?.cities,
          parameters?.page,
          parameters?.size,
          parameters?.keyword,
          parameters?.sortBy,
          parameters?.sortOrder,
          parameters?.startDate && new Date(parameters.startDate),
          parameters?.endDate && new Date(parameters.endDate),
          parameters?.stationIds,
          parameters?.deviceSerials,
          undefined,
        )
      } catch (err) {
        console.error('TrafficOverviewService.getRecordsList 調用 API 失敗:', err)
        throw err
      }

      // 如果API返回的是數組，我們需要包裝成分頁響應格式
      if (Array.isArray(response)) {
        let processedData = response

        // 如果需要按設備分組並只取最新記錄
        if (parameters?.groupByDevice && parameters?.latestOnly) {
          const deviceMap = new Map<string, TrafficRecordListResponse>()

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
      console.error('TrafficOverviewService.getRecordsList 錯誤:', error)
      throw handleApiError(error)
    }
  }

  static async getRecordsCount(keyword?: string, availableStationIds?: number[]): Promise<number> {
    try {
      const count: number = await trafficClient.getRecordsCount(keyword, availableStationIds)
      return count
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async exportRecords(keyword?: string): Promise<Blob> {
    try {
      const response: FileResponse = await trafficClient.exportRecords(keyword)
      // FileResponse.data 是 Blob 類型，直接返回
      return response.data
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecentRoadConditionHistory(stationID?: number, timeRange?: number): Promise<TrafficRoadConditionHistoryResponse> {
    try {
      const response: TrafficRoadConditionHistoryResponse = await trafficClient.getRecentRoadConditionHistory(stationID, timeRange)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getRecentRoadCondition(stationID?: number, timeRange?: number, limit?: number): Promise<TrafficRoadConditionResponse> {
    try {
      const response: TrafficRoadConditionResponse = await trafficClient.getRecentRoadCondition(stationID, timeRange, limit)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  TrafficRecordListResponse,
  TrafficRoadConditionHistoryResponse,
  TrafficRoadConditionResponse
}
