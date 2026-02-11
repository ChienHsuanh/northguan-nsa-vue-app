import { toast } from 'vue-sonner'

export interface ApiErrorResponse {
  errorCode: string
  message: string
  details?: string | null
  validationErrors?: { [key: string]: string[] } | null
  timestamp: Date
  path?: string | null
  statusCode: number
}

export const useToast = () => {
  const success = (title: string, description?: string) => {
    return toast.success(title, {
      description,
      duration: 4000,
    })
  }

  const error = (title: string, description?: string) => {
    return toast.error(title, {
      description,
      duration: 5000,
    })
  }

  // 專門處理統一錯誤格式的方法
  const apiError = (apiErrorResponse: ApiErrorResponse) => {
    let title = '錯誤'
    let description = apiErrorResponse.message

    // 根據錯誤代碼設置更友善的標題
    switch (apiErrorResponse.errorCode) {
      case 'VALIDATION_FAILED':
        title = '驗證失敗'
        // 如果有驗證錯誤，格式化顯示
        if (apiErrorResponse.validationErrors) {
          // const validationMessages: string[] = []
          // for (const [field, errors] of Object.entries(apiErrorResponse.validationErrors)) {
          //   if (errors && errors.length > 0) {
          //     validationMessages.push(`${field}: ${errors.join(', ')}`)
          //   }
          // }
          const validationMessages = Object.values(apiErrorResponse.validationErrors)
          description = validationMessages.length > 0 ? validationMessages.join('\n') : apiErrorResponse.message
        }
        break
      case 'UNAUTHORIZED':
        title = '權限不足'
        description = '您沒有權限執行此操作'
        break
      case 'RESOURCE_NOT_FOUND':
        title = '資源未找到'
        break
      case 'RESOURCE_ALREADY_EXISTS':
        title = '資源已存在'
        break
      case 'INTERNAL_SERVER_ERROR':
        title = '系統錯誤'
        description = '伺服器發生內部錯誤，請稍後再試'
        break
      case 'INVALID_CREDENTIALS':
        title = '登入失敗'
        description = '帳號或密碼錯誤'
        break
      case 'ACCOUNT_LOCKED':
        title = '帳號已鎖定'
        description = '請聯繫管理員解鎖帳號'
        break
      case 'TOKEN_EXPIRED':
        title = '登入已過期'
        description = '請重新登入'
        break
      default:
        title = '操作失敗'
    }

    return toast.error(title, {
      description,
      duration: 5000,
    })
  }

  const warning = (title: string, description?: string) => {
    return toast.warning(title, {
      description,
      duration: 4000,
    })
  }

  const info = (title: string, description?: string) => {
    return toast.info(title, {
      description,
      duration: 4000,
    })
  }

  const loading = (title: string, description?: string) => {
    return toast.loading(title, {
      description,
    })
  }

  const promise = <T>(
    promise: Promise<T>,
    options: {
      loading: string
      success: string | ((data: T) => string)
      error: string | ((error: any) => string)
    }
  ) => {
    return toast.promise(promise, options)
  }

  const dismiss = (id?: string | number) => {
    return toast.dismiss(id)
  }

  return {
    success,
    error,
    apiError,
    warning,
    info,
    loading,
    promise,
    dismiss
  }
}
