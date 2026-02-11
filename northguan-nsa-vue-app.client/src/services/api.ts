import axios, { AxiosError } from 'axios'
import { useAuthStore } from '@/stores/auth'
import { 
  addPendingRequest, 
  removePendingRequest, 
  type DebounceConfig,
  defaultDebounceConfig 
} from '@/utils/requestDebounce'

// 創建 axios 實例
export const apiClient = axios.create({
  baseURL: '',
  timeout: 10000,
})

// 請求攔截器
apiClient.interceptors.request.use(
  (config) => {
    // 添加認證 token
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    // 防重複提交邏輯
    const debounceConfig: DebounceConfig = {
      ...defaultDebounceConfig,
      ...config.metadata?.debounce
    }

    // 只對 POST, PUT, PATCH, DELETE 請求進行防重複檢查
    const shouldDebounce = debounceConfig.enableDebounce && 
      ['post', 'put', 'patch', 'delete'].includes(config.method?.toLowerCase() || '')

    if (shouldDebounce) {
      const canProceed = addPendingRequest(config)
      if (!canProceed) {
        // 創建一個特殊的錯誤來阻止重複請求
        const cancelError = new Error('重複請求已被阻止')
        cancelError.name = 'DuplicateRequestError'
        // 添加特殊標記，讓後續處理知道這是重複請求
        ;(cancelError as any).isDuplicateRequest = true
        return Promise.reject(cancelError)
      }
    }

    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 響應攔截器
apiClient.interceptors.response.use(
  (response) => {
    // 請求成功，從待處理列表中移除
    if (response.config) {
      removePendingRequest(response.config)
    }
    return response
  },
  (error: AxiosError | any) => {
    // 如果是重複請求錯誤，直接靜默處理，不進行任何其他操作
    if (error.name === 'DuplicateRequestError' || error.isDuplicateRequest) {
      console.log('Axios攔截器: 重複請求被靜默處理')
      return Promise.reject(error)
    }

    // 請求失敗，從待處理列表中移除
    if (error.config) {
      removePendingRequest(error.config)
    }

    // 處理 401 未授權錯誤
    if (error.response?.status === 401 && error.config && error.config.url !== '/api/login') {
      const authStore = useAuthStore()
      authStore.logout()
      window.location.href = '/login'
    }

    return Promise.reject(error)
  }
)
