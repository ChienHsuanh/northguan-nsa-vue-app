import { handleApiError } from '@/utils/errorHandler'
import {
  highResolutionClient,
  HighResolutionOverviewResponse
} from '@/api/apiClients'

export class HighResolutionService {
  static async getOverviewInfo(stationID?: number): Promise<HighResolutionOverviewResponse> {
    try {
      const response: HighResolutionOverviewResponse = await highResolutionClient.getOverviewInfo(stationID)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  HighResolutionOverviewResponse
}
