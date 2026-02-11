import { handleApiError } from '@/utils/errorHandler'
import {
  stationClient,
  StationResponse,
  CreateStationRequest,
  UpdateStationRequest,
  CountResponse,
  type FileResponse
} from '@/api/apiClients'

export class StationService {
  static async getStations(page?: number, size?: number, keyword?: string): Promise<StationResponse[]> {
    try {
      const response: StationResponse[] = await stationClient.getStations(page, size, keyword)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getStationDetail(id: number): Promise<StationResponse> {
    try {
      const response: StationResponse = await stationClient.getStationDetail(id)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getStationCount(keyword?: string): Promise<number> {
    try {
      const response: CountResponse = await stationClient.getStationCount(keyword)
      return response.count || 0
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async createStation(request: CreateStationRequest): Promise<FileResponse> {
    try {
      const response: FileResponse = await stationClient.createStation(request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async updateStation(id: number, request: UpdateStationRequest): Promise<FileResponse> {
    try {
      const response: FileResponse = await stationClient.updateStation(id, request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async deleteStation(id: number): Promise<FileResponse> {
    try {
      const response: FileResponse = await stationClient.deleteStation(id)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  StationResponse,
  CreateStationRequest,
  UpdateStationRequest,
  CountResponse,
  type FileResponse
}
