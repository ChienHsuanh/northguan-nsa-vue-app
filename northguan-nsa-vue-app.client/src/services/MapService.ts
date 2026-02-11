import { handleApiError } from '@/utils/errorHandler'
import {
  mapClient,
  LandmarksResponse,
  ParkingLandmarkResponse,
  TrafficLandmarkResponse,
  CrowdLandmarkResponse,
  FenceLandmarkResponse,
  HighResolutionLandmarkResponse
} from '@/api/apiClients'

export class MapService {
  static async getLandmarks(): Promise<LandmarksResponse> {
    try {
      const response: LandmarksResponse = await mapClient.getLandmarks()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getLandmarkParking(id: number): Promise<ParkingLandmarkResponse> {
    try {
      const response: ParkingLandmarkResponse = await mapClient.getLandmarkParking(id)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getLandmarkTraffic(id: number): Promise<TrafficLandmarkResponse> {
    try {
      const response: TrafficLandmarkResponse = await mapClient.getLandmarkTraffic(id)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getLandmarkCrowd(id: number): Promise<CrowdLandmarkResponse> {
    try {
      const response: CrowdLandmarkResponse = await mapClient.getLandmarkCrowd(id)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getLandmarkFence(id: number): Promise<FenceLandmarkResponse> {
    try {
      const response: FenceLandmarkResponse = await mapClient.getLandmarkFence(id)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getLandmarkHighResolution(id: number): Promise<HighResolutionLandmarkResponse> {
    try {
      const response: HighResolutionLandmarkResponse = await mapClient.getLandmarkHighResolution(id)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  LandmarksResponse,
  ParkingLandmarkResponse,
  TrafficLandmarkResponse,
  CrowdLandmarkResponse,
  FenceLandmarkResponse,
  HighResolutionLandmarkResponse
}
