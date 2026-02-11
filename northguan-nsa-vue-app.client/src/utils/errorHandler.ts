import { ApiException } from '@/api/client'
import { useToast, type ApiErrorResponse } from '@/composables/useToast'

// 錯誤代碼常數
export const ERROR_CODES = {
  // 認證相關
  UNAUTHORIZED: 'UNAUTHORIZED',
  INVALID_CREDENTIALS: 'INVALID_CREDENTIALS',
  ACCOUNT_LOCKED: 'ACCOUNT_LOCKED',
  TOKEN_EXPIRED: 'TOKEN_EXPIRED',

  // 資源相關
  RESOURCE_NOT_FOUND: 'RESOURCE_NOT_FOUND',
  RESOURCE_ALREADY_EXISTS: 'RESOURCE_ALREADY_EXISTS',
  RESOURCE_IN_USE: 'RESOURCE_IN_USE',

  // 驗證相關
  VALIDATION_FAILED: 'VALIDATION_FAILED',
  INVALID_INPUT: 'INVALID_INPUT',
  MISSING_REQUIRED_FIELD: 'MISSING_REQUIRED_FIELD',

  // 業務邏輯
  BUSINESS_RULE_VIOLATION: 'BUSINESS_RULE_VIOLATION',
  OPERATION_NOT_ALLOWED: 'OPERATION_NOT_ALLOWED',

  // 系統錯誤
  INTERNAL_SERVER_ERROR: 'INTERNAL_SERVER_ERROR',
  DATABASE_ERROR: 'DATABASE_ERROR',
  EXTERNAL_SERVICE_ERROR: 'EXTERNAL_SERVICE_ERROR'
} as const

// 重新導出 ApiErrorResponse 介面以保持向後兼容
export type { ApiErrorResponse } from '@/composables/useToast'

// 解析 API 錯誤
export async function parseApiError(error: any): Promise<ApiErrorResponse | null> {
  console.debug('parseApiError input:', error)

  // 處理 NSwag 生成的 ApiException
  if (error instanceof ApiException) {
    console.debug('Processing NSwag ApiException:', error)
    console.debug('ApiException response:', error.response)

    // 嘗試解析 response 中的 JSON 數據
    let responseData = null
    if (error.response) {
      try {
        // 如果 response 是 Blob，需要讀取其文本內容
        if (error.response instanceof Blob) {
          const responseText = await error.response.text()
          // console.debug('Blob response text:', responseText)
          // 解析 JSON 並處理 Unicode 轉義序列
          responseData = JSON.parse(responseText)
        } else if (typeof error.response === 'string') {
          responseData = JSON.parse(error.response)
        } else if (typeof error.response === 'object') {
          responseData = error.response
        }
        console.debug('Parsed response data:', responseData)
      } catch (e) {
        console.debug('Failed to parse response as JSON:', e)
      }
    }

    // 如果解析出的數據符合統一錯誤格式
    if (responseData && responseData.errorCode && responseData.message) {
      return {
        errorCode: responseData.errorCode,
        message: responseData.message,
        details: responseData.details || null,
        validationErrors: responseData.validationErrors || null,
        timestamp: responseData.timestamp ? new Date(responseData.timestamp) : new Date(),
        path: responseData.path || null,
        statusCode: error.status || responseData.statusCode || 500
      }
    }

    // 檢查 result 屬性是否包含統一錯誤格式
    if (error.result) {
      const result = error.result
      console.debug('ApiException result:', result)

      // 檢查是否符合統一錯誤格式
      if (result.errorCode && result.message) {
        return {
          errorCode: result.errorCode,
          message: result.message,
          details: result.details || null,
          validationErrors: result.validationErrors || null,
          timestamp: result.timestamp ? new Date(result.timestamp) : new Date(),
          path: result.path || null,
          statusCode: error.status || result.statusCode || 500
        }
      }
    }

    // 如果 result 不包含統一錯誤格式，但有基本錯誤信息
    if (error.message && error.status) {
      return {
        errorCode: `HTTP_${error.status}`,
        message: error.message,
        details: error.response || null,
        validationErrors: null,
        timestamp: new Date(),
        path: null,
        statusCode: error.status
      }
    }
  }

  // 處理 Axios 錯誤回應 - 統一錯誤格式
  if (error.response?.data) {
    const errorData = error.response.data

    // 檢查是否符合統一錯誤格式
    if (errorData.errorCode && errorData.message) {
      return {
        errorCode: errorData.errorCode,
        message: errorData.message,
        details: errorData.details || null,
        validationErrors: errorData.validationErrors || null,
        timestamp: errorData.timestamp ? new Date(errorData.timestamp) : new Date(),
        path: errorData.path || null,
        statusCode: errorData.statusCode || error.response.status
      }
    }
  }

  // 處理其他可能的錯誤格式
  if (error.response) {
    const response = error.response

    // 嘗試從 response 直接獲取錯誤信息
    if (response.errorCode && response.message) {
      return {
        errorCode: response.errorCode,
        message: response.message,
        details: response.details || null,
        validationErrors: response.validationErrors || null,
        timestamp: response.timestamp ? new Date(response.timestamp) : new Date(),
        path: response.path || null,
        statusCode: response.statusCode || response.status
      }
    }

    // 處理標準 HTTP 錯誤狀態碼
    if (response.status) {
      let errorCode = 'HTTP_ERROR'
      let message = '發生錯誤'

      switch (response.status) {
        case 400:
          errorCode = 'BAD_REQUEST'
          message = '請求格式錯誤'
          break
        case 401:
          errorCode = 'UNAUTHORIZED'
          message = '未授權訪問'
          break
        case 403:
          errorCode = 'FORBIDDEN'
          message = '禁止訪問'
          break
        case 404:
          errorCode = 'RESOURCE_NOT_FOUND'
          message = '資源未找到'
          break
        case 409:
          errorCode = 'RESOURCE_ALREADY_EXISTS'
          message = '資源已存在'
          break
        case 422:
          errorCode = 'VALIDATION_FAILED'
          message = '驗證失敗'
          break
        case 500:
          errorCode = 'INTERNAL_SERVER_ERROR'
          message = '伺服器內部錯誤'
          break
        default:
          message = `HTTP ${response.status} 錯誤`
      }

      return {
        errorCode,
        message: response.data?.message || message,
        details: response.data?.details || null,
        validationErrors: response.data?.validationErrors || null,
        timestamp: new Date(),
        path: response.config?.url || null,
        statusCode: response.status
      }
    }
  }

  // 處理網路錯誤或其他錯誤
  if (error.code === 'NETWORK_ERROR' || error.message?.includes('Network Error')) {
    return {
      errorCode: 'NETWORK_ERROR',
      message: '網路連線錯誤，請檢查網路連接',
      details: error.message || null,
      validationErrors: null,
      timestamp: new Date(),
      path: null,
      statusCode: 0
    }
  }

  // 處理超時錯誤
  if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
    return {
      errorCode: 'TIMEOUT_ERROR',
      message: '請求超時，請稍後再試',
      details: error.message || null,
      validationErrors: null,
      timestamp: new Date(),
      path: null,
      statusCode: 0
    }
  }

  console.debug('parseApiError: Unable to parse error, returning null')
  return null
}

// 錯誤處理器類別
export class ErrorHandler {
  private toast = useToast()

  // 處理 API 錯誤
  async handleApiError(error: any): Promise<ApiErrorResponse> {
    console.debug('handleError', error)

    // 避免重複處理已經是 ApiErrorResponse 的錯誤
    if (error && typeof error === 'object' && error.errorCode && error.message && error.timestamp) {
      console.debug('Error is already an ApiErrorResponse, skipping processing')
      this.showErrorMessage(error)
      return error
    }

    // 解析 API 錯誤
    const parsedError = await parseApiError(error)

    if (parsedError) {
      this.showErrorMessage(parsedError)
      return parsedError
    }

    // 處理網路錯誤或其他未知錯誤
    const fallbackError: ApiErrorResponse = {
      errorCode: 'NETWORK_ERROR',
      message: '網路連線錯誤，請稍後再試',
      timestamp: new Date(),
      statusCode: error.response?.status || 0
    }

    this.showErrorMessage(fallbackError)
    return fallbackError
  }

  // 顯示錯誤訊息
  private showErrorMessage(error: ApiErrorResponse) {
    // 使用新的 apiError 方法來統一處理錯誤顯示
    this.toast.apiError(error)
  }

  // 檢查錯誤類型
  isErrorType(error: ApiErrorResponse, errorCode: string): boolean {
    return error.errorCode === errorCode
  }

  // 獲取友善的錯誤訊息
  getFriendlyMessage(error: ApiErrorResponse): string {
    return error.message
  }

  // 獲取驗證錯誤
  getValidationErrors(error: ApiErrorResponse): { [key: string]: string[] } {
    return error.validationErrors || {}
  }

  // 關閉 API 錯誤提示
  dissmissApiErrorToast() {
    this.toast.dismiss()
  }
}

// 創建全域錯誤處理器實例
export const errorHandler = new ErrorHandler()

// 便利函數
export async function handleApiError(error: any): Promise<ApiErrorResponse> {
  return await errorHandler.handleApiError(error)
}

export function isErrorType(error: ApiErrorResponse, errorCode: string): boolean {
  return errorHandler.isErrorType(error, errorCode)
}

export function getFriendlyMessage(error: ApiErrorResponse): string {
  return errorHandler.getFriendlyMessage(error)
}

export function getValidationErrors(error: ApiErrorResponse): { [key: string]: string[] } {
  return errorHandler.getValidationErrors(error)
}

export function dissmissApiErrorToast() {
  errorHandler.dissmissApiErrorToast()
}
