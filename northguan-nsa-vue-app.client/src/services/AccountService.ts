import { handleApiError } from '@/utils/errorHandler'
import {
  accountClient,
  UserResponse,
  CreateAccountRequest,
  UpdateAccountRequest,
  CountResponse,
  type FileResponse
} from '@/api/apiClients'

export class AccountService {
  static async getAccountCount(keyword?: string): Promise<number> {
    try {
      const response: CountResponse = await accountClient.getAccountCount(keyword)
      return response.count || 0
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getAccountList(page?: number, size?: number, keyword?: string): Promise<UserResponse[]> {
    try {
      const response: UserResponse[] = await accountClient.getAccountList(page, size, keyword)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async createAccount(request: CreateAccountRequest): Promise<FileResponse> {
    try {
      const response: FileResponse = await accountClient.createAccount(request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async updateAccount(id: string, request: UpdateAccountRequest): Promise<FileResponse> {
    try {
      const response: FileResponse = await accountClient.updateAccount(id, request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async deleteAccount(id: string): Promise<FileResponse> {
    try {
      const response: FileResponse = await accountClient.deleteAccount(id)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export { UserResponse, CreateAccountRequest, UpdateAccountRequest, CountResponse, type FileResponse }
