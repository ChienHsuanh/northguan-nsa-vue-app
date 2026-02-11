import { handleApiError, type ApiErrorResponse } from '@/utils/errorHandler'
import {
  authClient,
  LoginRequest,
  ProfileResponse,
  type FileResponse,
  type FileParameter,
  LoginResponse
} from '@/api/apiClients'

export class AuthService {
  static async login(request: LoginRequest): Promise<LoginResponse> {
    try {
      const response: LoginResponse = await authClient.login(request)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async getProfile(): Promise<ProfileResponse> {
    try {
      const response: ProfileResponse = await authClient.getProfile()
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }

  static async updateProfile(
    name?: string | null,
    phone?: string | null,
    employeeId?: string | null,
    password?: string | null,
    avatarFile?: FileParameter | null
  ): Promise<FileResponse> {
    try {
      const response: FileResponse = await authClient.updateProfile(name, phone, employeeId, password, avatarFile)
      return response
    } catch (error) {
      throw handleApiError(error)
    }
  }
}

// 導出 NSwag 生成的類型
export {
  LoginRequest,
  ProfileResponse,
  type FileResponse,
  type FileParameter
}
