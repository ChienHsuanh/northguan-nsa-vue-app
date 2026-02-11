import { handleApiError } from '@/utils/errorHandler'
import {
  thirdPartyApiClient,
  CreateFenceRecordRequest,
  UpdateFenceHeartbeatRequest,
  CreateCrowdRecordRequest,
  CreateParkingRecordRequest,
  ThirdPartyStationInfo,
  ThirdPartyFenceDeviceInfo,
  ThirdPartyCrowdDeviceInfo,
  ThirdPartyParkingDeviceInfo,
  ThirdPartyTrafficDeviceInfo,
  CreateFenceRecordResponse,
  UpdateFenceHeartbeatResponse,
  CreateCrowdRecordResponse,
  CreateParkingRecordResponse,
  type FileResponse
} from '@/api/apiClients'

export class ThirdPartyApiService {
  static async createFenceRecord(request: CreateFenceRecordRequest): Promise<CreateFenceRecordResponse> {
    try {
      const response: CreateFenceRecordResponse = await thirdPartyApiClient.createFenceRecord(request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async updateFenceHeartbeat(request: UpdateFenceHeartbeatRequest): Promise<UpdateFenceHeartbeatResponse> {
    try {
      const response: UpdateFenceHeartbeatResponse = await thirdPartyApiClient.updateFenceHeartbeat(request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async createCrowdRecord(request: CreateCrowdRecordRequest): Promise<CreateCrowdRecordResponse> {
    try {
      const response: CreateCrowdRecordResponse = await thirdPartyApiClient.createCrowdRecord(request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async createApParkingRecord(request: CreateParkingRecordRequest): Promise<CreateParkingRecordResponse> {
    try {
      const response: CreateParkingRecordResponse = await thirdPartyApiClient.createApParkingRecord(request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getStationList(): Promise<ThirdPartyStationInfo[]> {
    try {
      const response: ThirdPartyStationInfo[] = await thirdPartyApiClient.getStationList()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getFenceDeviceList(): Promise<ThirdPartyFenceDeviceInfo[]> {
    try {
      const response: ThirdPartyFenceDeviceInfo[] = await thirdPartyApiClient.getFenceDeviceList()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getCrowdDeviceList(): Promise<ThirdPartyCrowdDeviceInfo[]> {
    try {
      const response: ThirdPartyCrowdDeviceInfo[] = await thirdPartyApiClient.getCrowdDeviceList()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getParkingDeviceList(): Promise<ThirdPartyParkingDeviceInfo[]> {
    try {
      const response: ThirdPartyParkingDeviceInfo[] = await thirdPartyApiClient.getParkingDeviceList()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getTrafficDeviceList(): Promise<ThirdPartyTrafficDeviceInfo[]> {
    try {
      const response: ThirdPartyTrafficDeviceInfo[] = await thirdPartyApiClient.getTrafficDeviceList()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async zeroTouchFileMaker(): Promise<FileResponse> {
    try {
      const response: FileResponse = await thirdPartyApiClient.zeroTouchFileMaker()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  CreateFenceRecordRequest,
  UpdateFenceHeartbeatRequest,
  CreateCrowdRecordRequest,
  CreateParkingRecordRequest,
  ThirdPartyStationInfo,
  ThirdPartyFenceDeviceInfo,
  ThirdPartyCrowdDeviceInfo,
  ThirdPartyParkingDeviceInfo,
  ThirdPartyTrafficDeviceInfo,
  CreateFenceRecordResponse,
  UpdateFenceHeartbeatResponse,
  CreateCrowdRecordResponse,
  CreateParkingRecordResponse,
  type FileResponse
}
