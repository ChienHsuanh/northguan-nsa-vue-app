import { handleApiError } from '@/utils/errorHandler'
import {
  systemSettingClient,
  SystemSettingResponse,
  UpdateSystemSettingRequest,
  type FileResponse
} from '@/api/apiClients'

export class SystemSettingService {
  static async getSystemSetting(): Promise<SystemSettingResponse> {
    try {
      const response: SystemSettingResponse = await systemSettingClient.getSystemSetting()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async updateSystemSetting(request: UpdateSystemSettingRequest): Promise<FileResponse> {
    try {
      const response: FileResponse = await systemSettingClient.updateSystemSetting(request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  SystemSettingResponse,
  UpdateSystemSettingRequest,
  type FileResponse
}
