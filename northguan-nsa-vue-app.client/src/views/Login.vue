<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50">
    <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div class="max-w-md w-full space-y-8">
        <!-- Logo 和標題 -->
        <div class="text-center">
          <div
            class="mx-auto h-20 w-20 bg-gradient-to-br from-blue-600 to-purple-600 rounded-full flex items-center justify-center mb-6">
            <MapIcon class="h-10 w-10 text-white" />
          </div>
          <h2 class="text-3xl font-bold text-gray-900 mb-2">北觀智慧管理平台</h2>
          <p class="text-gray-600">智慧觀光管理系統</p>
        </div>

        <!-- 登入表單卡片 -->
        <Card class="shadow-xl border-0">
          <CardContent class="p-8">
            <Form @submit="handleLogin" :validation-schema="loginSchema" v-slot="{ errors, isSubmitting }"
              autocomplete="off">
              <div class="space-y-6">
                <div class="text-center mb-6">
                  <h3 class="text-xl font-semibold text-gray-900">登入您的帳號</h3>
                  <p class="text-sm text-gray-500 mt-1">請輸入您的登入資訊</p>
                </div>

                <!-- 帳號輸入 -->
                <FormField name="username" label="帳號 / 電子郵件">
                  <Field name="username" v-slot="{ field }">
                    <div class="relative">
                      <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                        <UserIcon class="h-5 w-5 text-gray-400" />
                      </div>
                      <Input v-bind="field" type="text" autocomplete="username email" placeholder="請輸入帳號或電子郵件"
                        class="pl-10"
                        :class="{ 'border-red-500 focus:border-red-500 focus:ring-red-500': errors.username }"
                        :disabled="isSubmitting" />
                    </div>
                  </Field>
                  <ErrorMessage name="username" class="text-sm text-red-600 mt-1" />
                </FormField>

                <!-- 密碼輸入 -->
                <FormField name="password" label="密碼">
                  <Field name="password" v-slot="{ field }">
                    <div class="relative">
                      <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                        <LockIcon class="h-5 w-5 text-gray-400" />
                      </div>
                      <Input v-bind="field" :type="showPassword ? 'text' : 'password'" autocomplete="current-password"
                        placeholder="請輸入密碼" class="pl-10 pr-10"
                        :class="{ 'border-red-500 focus:border-red-500 focus:ring-red-500': errors.password }"
                        :disabled="isSubmitting" />
                      <button type="button" class="absolute inset-y-0 right-0 pr-3 flex items-center"
                        @click="togglePasswordVisibility" :disabled="isSubmitting">
                        <EyeIcon v-if="showPassword" class="h-5 w-5 text-gray-400 hover:text-gray-600" />
                        <EyeOffIcon v-else class="h-5 w-5 text-gray-400 hover:text-gray-600" />
                      </button>
                    </div>
                  </Field>
                  <ErrorMessage name="password" class="text-sm text-red-600 mt-1" />
                </FormField>

                <!-- 記住我 -->
                <!-- <div class="flex items-center justify-between">
                  <div class="flex items-center">
                    <Field name="rememberMe" v-slot="{ field }">
                      <input
                        v-bind="field"
                        type="checkbox"
                        class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                        :disabled="isSubmitting"
                      />
                    </Field>
                    <Label class="ml-2 text-sm text-gray-600">記住我</Label>
                  </div>
                  <div class="text-sm">
                    <a href="#" class="font-medium text-blue-600 hover:text-blue-500" @click.prevent="showForgotPassword">
                      忘記密碼？
                    </a>
                  </div>
                </div> -->

                <!-- 登入按鈕 -->
                <Button type="submit" class="w-full h-12 text-base font-medium" :disabled="isSubmitting">
                  <div v-if="isSubmitting" class="flex items-center justify-center">
                    <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-2"></div>
                    登入中...
                  </div>
                  <div v-else class="flex items-center justify-center">
                    <LogInIcon class="h-5 w-5 mr-2" />
                    登入
                  </div>
                </Button>

                <!-- 錯誤訊息顯示 -->
                <div v-if="loginError" class="rounded-md bg-red-50 p-4">
                  <div class="flex">
                    <div class="flex-shrink-0">
                      <AlertCircleIcon class="h-5 w-5 text-red-400" />
                    </div>
                    <div class="ml-3">
                      <h3 class="text-sm font-medium text-red-800">登入失敗</h3>
                      <div class="mt-2 text-sm text-red-700">
                        {{ loginError }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </Form>
          </CardContent>
        </Card>

        <!-- 底部資訊 -->
        <div class="text-center">
          <p class="text-xs text-gray-500">
            © 2024 北海岸及觀音山國家風景區管理處. 版權所有.
          </p>
        </div>
      </div>
    </div>

    <!-- 忘記密碼對話框 -->
    <Dialog v-model:open="showForgotPasswordModal">
      <DialogContent class="sm:max-w-[425px]">
        <div class="text-center">
          <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-blue-100 mb-4">
            <MailIcon class="h-6 w-6 text-blue-600" />
          </div>
          <h3 class="text-lg font-medium text-gray-900">重設密碼</h3>
          <p class="text-sm text-gray-500 mt-2">
            請輸入您的電子郵件地址，我們將發送重設密碼的連結給您。
          </p>
        </div>

        <Form @submit="handleForgotPassword" :validation-schema="forgotPasswordSchema"
          v-slot="{ errors, isSubmitting }">
          <div class="space-y-4">
            <FormField name="email" label="電子郵件">
              <Field name="email" v-slot="{ field }">
                <Input v-bind="field" type="email" placeholder="請輸入您的電子郵件"
                  :class="{ 'border-red-500': errors.email }" />
              </Field>
              <ErrorMessage name="email" class="text-sm text-red-600 mt-1" />
            </FormField>
          </div>

          <div class="flex justify-end space-x-3 pt-4">
            <Button type="button" variant="outline" @click="showForgotPasswordModal = false">
              取消
            </Button>
            <Button type="submit" :disabled="isSubmitting">
              <span v-if="isSubmitting" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></span>
              {{ isSubmitting ? '發送中...' : '發送重設連結' }}
            </Button>
          </div>
        </Form>
      </DialogContent>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Form, Field, ErrorMessage } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/yup'
import * as yup from 'yup'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { AuthService, LoginRequest } from '@/services'
import { handleApiError, ERROR_CODES, type ApiErrorResponse } from '@/utils/errorHandler'
import Card from '@/components/ui/card/Card.vue'
import CardContent from '@/components/ui/card/CardContent.vue'
import Button from '@/components/ui/button/Button.vue'
import Input from '@/components/ui/input/Input.vue'
import Label from '@/components/ui/label/Label.vue'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter
} from '@/components/ui/dialog'
import { FormControl, FormDescription, FormField, FormItem, FormMessage } from '@/components/ui/form'
import {
  Map as MapIcon,
  User as UserIcon,
  Lock as LockIcon,
  Eye as EyeIcon,
  EyeOff as EyeOffIcon,
  LogIn as LogInIcon,
  AlertCircle as AlertCircleIcon,
  Mail as MailIcon
} from 'lucide-vue-next'

// Composables
const router = useRouter()
const authStore = useAuthStore()
const toast = useToast()

// State
const showPassword = ref(false)
const loginError = ref('')
const showForgotPasswordModal = ref(false)

// Validation schemas
const loginSchema = toTypedSchema(
  yup.object({
    username: yup.string().required('請輸入帳號或電子郵件'),
    password: yup.string().min(6, '密碼至少需要6個字符').required('請輸入密碼'),
    rememberMe: yup.boolean().default(false)
  })
)

const forgotPasswordSchema = toTypedSchema(
  yup.object({
    email: yup.string().email('請輸入有效的電子郵件地址').required('請輸入電子郵件')
  })
)

// Methods
const togglePasswordVisibility = () => {
  showPassword.value = !showPassword.value
}

const handleLogin = async (values: any) => {
  try {
    loginError.value = ''

    // 使用 NSwag AuthService
    const loginRequest = new LoginRequest()
    loginRequest.username = values.username
    loginRequest.password = values.password

    const response = await AuthService.login(loginRequest)
    const success = await authStore.handleLoginResponse(response, values.rememberMe)

    if (success) {
      // 立即跳轉，不等待 toast 或其他操作
      const redirect = new URLSearchParams(window.location.search).get('redirect')
      router.push(redirect || '/dashboard')

      // 在跳轉後顯示成功訊息
      toast.success('登入成功', '歡迎回到智慧觀光管理系統')
    } else {
      loginError.value = '帳號或密碼錯誤，請重新輸入'
    }
  } catch (error: any) {
    console.error('登入失敗:', error)

    // 使用新的錯誤處理機制
    // const apiError: ApiErrorResponse = handleApiError(error)

    // 根據錯誤代碼設定特定的錯誤訊息
    switch (error.errorCode) {
      case ERROR_CODES.INVALID_CREDENTIALS:
        loginError.value = '帳號或密碼錯誤，請重新輸入'
        break
      case ERROR_CODES.ACCOUNT_LOCKED:
        loginError.value = '帳號已被鎖定，請聯繫管理員'
        break
      case ERROR_CODES.TOKEN_EXPIRED:
        loginError.value = '登入已過期，請重新輸入'
        break
      case ERROR_CODES.VALIDATION_FAILED:
        // 顯示驗證錯誤的詳細資訊
        const validationErrors = error.validationErrors
        if (validationErrors) {
          const errorMessages = Object.entries(validationErrors)
            .map(([field, errors]) => `${field}: ${(errors as string[]).join(', ')}`)
            .join('\n')
          loginError.value = errorMessages
        } else {
          loginError.value = error.message
        }
        break
      case 'NETWORK_ERROR':
        loginError.value = '網路連線錯誤，請檢查您的網路連線'
        break
      default:
        loginError.value = error.message || '登入失敗，請稍後再試或聯繫管理員'
    }
  }
}

const showForgotPassword = () => {
  showForgotPasswordModal.value = true
}

const handleForgotPassword = async (values: any) => {
  try {
    // 這裡應該調用忘記密碼的 API
    // await authStore.forgotPassword(values.email)

    toast.success('重設連結已發送', `重設密碼的連結已發送至 ${values.email}`)
    showForgotPasswordModal.value = false
  } catch (error) {
    console.error('發送重設連結失敗:', error)
    toast.error('發送失敗', '無法發送重設連結，請稍後再試')
  }
}

onMounted(() => {
  // 如果已經登入，直接跳轉到首頁
  if (authStore.isAuthenticated) {
    const redirect = new URLSearchParams(window.location.search).get('redirect')
    router.push(redirect || '/')
  }

  // 清除之前的錯誤訊息
  loginError.value = ''
})
</script>
