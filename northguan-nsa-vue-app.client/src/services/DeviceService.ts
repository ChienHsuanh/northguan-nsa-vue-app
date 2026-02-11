import { handleApiError } from '@/utils/errorHandler'
import {
  deviceClient,
  DeviceListResponse,
  CreateDeviceRequest,
  UpdateDeviceRequest,
  DeviceStatusResponse,
  DeviceStatusLogResponse,
  CountResponse,
  type FileResponse
} from '@/api/apiClients'

export class DeviceService {
  static async getDevices(type?: string, keyword?: string, page?: number, size?: number): Promise<DeviceListResponse[]> {
    try {
      const response: DeviceListResponse[] = await deviceClient.getDevices(type, keyword, page, size)
      return response
    } catch (error) {
      throw await handleApiError(error)
    }
  }

  static async getDeviceCount(type?: string, keyword?: string): Promise<number> {
    try {
      const response: CountResponse = await deviceClient.getDeviceCount(type, keyword)
      return response.count || 0
    } catch (error) {
      throw await handleApiError(error)
    }
  }

  static async createDevice(request: CreateDeviceRequest): Promise<FileResponse> {
    try {
      const response: FileResponse = await deviceClient.createDevice(request)
      return response
    } catch (error) {
      throw await handleApiError(error)
    }
  }

  static async updateDevice(id: number, request: UpdateDeviceRequest): Promise<FileResponse> {
    try {
      const response: FileResponse = await deviceClient.updateDevice(id, request)
      return response
    } catch (error) {
      throw await handleApiError(error)
    }
  }

  static async deleteDevice(id: number, type?: string): Promise<FileResponse> {
    try {
      const response: FileResponse = await deviceClient.deleteDevice(id, type)
      return response
    } catch (error) {
      throw await handleApiError(error)
    }
  }

  static async getDevicesStatus(page?: number, size?: number, keyword?: string): Promise<DeviceStatusResponse[]> {
    try {
      const response: DeviceStatusResponse[] = await deviceClient.getDevicesStatus(page, size, keyword)
      return response
    } catch (error) {
      throw await handleApiError(error)
    }
  }

  static async getDevicesStatusCount(): Promise<number> {
    try {
      const response: CountResponse = await deviceClient.getDevicesStatusCount()
      return response.count || 0
    } catch (error) {
      throw await handleApiError(error)
    }
  }

  static async getDeviceStatusLogs(page?: number, size?: number, keyword?: string): Promise<DeviceStatusLogResponse[]> {
    try {
      const response: DeviceStatusLogResponse[] = await deviceClient.getDeviceStatusLogs(page, size, keyword)
      return response
    } catch (error) {
      throw await handleApiError(error)
    }
  }

  static async getDeviceStatusLogCount(keyword?: string): Promise<number> {
    try {
      const response: CountResponse = await deviceClient.getDeviceStatusLogCount(keyword)
      return response.count || 0
    } catch (error) {
      throw await handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  DeviceListResponse,
  CreateDeviceRequest,
  UpdateDeviceRequest,
  DeviceStatusResponse,
  DeviceStatusLogResponse,
  CountResponse,
  type FileResponse
}
