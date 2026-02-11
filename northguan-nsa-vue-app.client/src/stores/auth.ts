import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiClient } from '@/services/api'
import { AuthService, ProfileResponse, type FileResponse } from '@/services'
import type { LoginResponse } from '@/api/apiClients'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const user = ref<any>(null)

  const isAuthenticated = computed(() => !!token.value)

  const login = async (username: string, password: string, rememberMe: boolean = false) => {
    try {
      const response = await apiClient.post('/api/login', {
        username,
        password,
        rememberMe
      })

      if (response.data.token) {
        token.value = response.data.token
        user.value = response.data.user

        // 根據 rememberMe 決定存儲方式
        if (rememberMe) {
          localStorage.setItem('token', response.data.token)
          localStorage.setItem('rememberMe', 'true')
        } else {
          sessionStorage.setItem('token', response.data.token)
          localStorage.removeItem('rememberMe')
        }

        // 設置 axios 默認 header
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`

        return true
      }
      return false
    } catch (error) {
      console.error('Login failed:', error)
      throw error
    }
  }

  const logout = () => {
    token.value = null
    user.value = null

    // 清除所有存儲的認證資訊
    localStorage.removeItem('token')
    localStorage.removeItem('rememberMe')
    sessionStorage.removeItem('token')

    delete apiClient.defaults.headers.common['Authorization']
  }

  const initializeAuth = () => {
    // 檢查 localStorage 和 sessionStorage 中的 token
    const localToken = localStorage.getItem('token')
    const sessionToken = sessionStorage.getItem('token')
    const rememberMe = localStorage.getItem('rememberMe') === 'true'

    // 優先使用 localStorage 中的 token（記住我功能）
    const storedToken = localToken || sessionToken

    if (storedToken) {
      token.value = storedToken
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${storedToken}`

      // 如果是從 sessionStorage 載入且沒有 rememberMe，則清除 localStorage
      if (sessionToken && !rememberMe) {
        localStorage.removeItem('token')
      }
    }
  }

  const handleLoginResponse = async (response: LoginResponse, rememberMe: boolean = false) => {
    try {
      const token = response.token;
      if (!token) {
        throw new Error('Token is missing in login response')
      }

      // 根據 rememberMe 決定存儲方式
      if (rememberMe) {
        localStorage.setItem('token', token)
        localStorage.setItem('rememberMe', 'true')
      } else {
        sessionStorage.setItem('token', token)
        localStorage.removeItem('rememberMe')
      }

      // 設置 axios 默認 header
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`

      // 非阻塞獲取用戶資料，不等待完成
      getProfile().catch(error => {
        console.error('Failed to get profile after login:', error)
      })

      return true
    } catch (error) {
      console.error('Handle login response failed:', error)
      throw error
    }
  }

  const getProfile = async (): Promise<ProfileResponse> => {
    try {
      const response: ProfileResponse = await AuthService.getProfile()
      user.value = response
      return response
    } catch (error) {
      console.error('Failed to get profile:', error)
      throw error
    }
  }

  const updateProfile = async (
    name?: string | null,
    phone?: string | null,
    employeeId?: string | null,
    password?: string | null,
    avatarFile?: any
  ): Promise<FileResponse> => {
    try {
      const response = await AuthService.updateProfile(name, phone, employeeId, password, avatarFile)

      // 更新本地用戶資料
      if (user.value) {
        if (name) user.value.name = name
        if (phone) user.value.phone = phone
        if (employeeId) user.value.employeeID = employeeId
      }

      return response
    } catch (error) {
      console.error('Failed to update profile:', error)
      throw error
    }
  }

  return {
    token,
    user,
    isAuthenticated,
    login,
    logout,
    initializeAuth,
    getProfile,
    updateProfile,
    handleLoginResponse
  }
})
