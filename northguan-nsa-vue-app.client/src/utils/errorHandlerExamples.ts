// 錯誤處理使用範例
import { handleApiError, ERROR_CODES, type ApiErrorResponse } from '@/utils/errorHandler'
import { useToast } from '@/composables/useToast'

/**
 * 基本錯誤處理範例
 */
export function basicErrorHandling() {
  const toast = useToast()

  // 在 try-catch 中使用
  const handleOperation = async () => {
    try {
      // API 調用
      await someApiCall()
    } catch (error) {
      const apiError: ApiErrorResponse = await handleApiError(error)

      // 基本錯誤顯示
      toast.error('操作失敗', apiError.message)
    }
  }
}

/**
 * 詳細錯誤處理範例
 */
export function detailedErrorHandling() {
  const toast = useToast()

  const handleCreateUser = async (userData: any) => {
    try {
      await createUser(userData)
      toast.success('創建成功', '用戶已成功創建')
    } catch (error) {
      const apiError: ApiErrorResponse = await handleApiError(error)

      // 根據錯誤代碼進行不同處理
      switch (apiError.errorCode) {
        case ERROR_CODES.VALIDATION_FAILED:
          // 處理驗證錯誤
          if (apiError.validationErrors) {
            const errorMessages = Object.entries(apiError.validationErrors)
              .map(([field, errors]) => `${field}: ${errors.join(', ')}`)
              .join('\n')
            toast.error('驗證失敗', errorMessages)
          } else {
            toast.error('驗證失敗', apiError.message)
          }
          break

        case ERROR_CODES.RESOURCE_ALREADY_EXISTS:
          toast.error('創建失敗', '該用戶已存在')
          break

        case ERROR_CODES.UNAUTHORIZED:
          toast.error('權限不足', '您沒有權限執行此操作')
          // 可能需要重新登入
          break

        case ERROR_CODES.INTERNAL_SERVER_ERROR:
          toast.error('系統錯誤', '伺服器發生內部錯誤，請稍後再試')
          break

        default:
          toast.error('操作失敗', apiError.message)
      }
    }
  }
}

/**
 * 在 Vue 組件中使用的範例
 */
export function vueComponentExample() {
  return `
<template>
  <div>
    <!-- 使用 ErrorDisplay 組件顯示錯誤 -->
    <ErrorDisplay
      :error="currentError"
      title="載入失敗"
      :show-details="isDevelopment"
    />

    <!-- 其他內容 -->
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { handleApiError, type ApiErrorResponse } from '@/utils/errorHandler'
import ErrorDisplay from '@/components/ErrorDisplay.vue'

const currentError = ref<ApiErrorResponse | null>(null)
const isDevelopment = process.env.NODE_ENV === 'development'

const loadData = async () => {
  try {
    currentError.value = null
    const data = await fetchData()
    // 處理成功的情況
  } catch (error) {
    currentError.value = await handleApiError(error)
  }
}
</script>
  `
}

/**
 * 表單驗證錯誤處理範例
 */
export function formValidationExample() {
  const toast = useToast()

  const handleFormSubmit = async (formData: any) => {
    try {
      await submitForm(formData)
      toast.success('提交成功', '表單已成功提交')
    } catch (error) {
      const apiError: ApiErrorResponse = await handleApiError(error)

      if (apiError.errorCode === ERROR_CODES.VALIDATION_FAILED && apiError.validationErrors) {
        // 將驗證錯誤映射到表單欄位
        const fieldErrors: Record<string, string[]> = {}

        Object.entries(apiError.validationErrors).forEach(([field, errors]) => {
          // 將後端欄位名稱映射到前端欄位名稱
          const frontendFieldName = mapBackendFieldToFrontend(field)
          fieldErrors[frontendFieldName] = errors
        })

        // 顯示欄位級別的錯誤
        Object.entries(fieldErrors).forEach(([field, errors]) => {
          // 這裡可以設定表單驗證錯誤
          setFieldError(field, errors.join(', '))
        })

        toast.error('驗證失敗', '請檢查表單中的錯誤')
      } else {
        toast.error('提交失敗', apiError.message)
      }
    }
  }
}

// 輔助函數
function mapBackendFieldToFrontend(backendField: string): string {
  const fieldMapping: Record<string, string> = {
    'Username': 'username',
    'Name': 'name',
    'Email': 'email',
    'Password': 'password',
    // 添加更多映射
  }

  return fieldMapping[backendField] || backendField.toLowerCase()
}

function setFieldError(field: string, message: string) {
  // 實際的表單錯誤設定邏輯
  console.log(`Setting error for field ${field}: ${message}`)
}

// 模擬的 API 函數
async function someApiCall() {
  // 模擬 API 調用
}

async function createUser(userData: any) {
  // 模擬創建用戶 API
}

async function fetchData() {
  // 模擬獲取數據 API
}

async function submitForm(formData: any) {
  // 模擬提交表單 API
}
