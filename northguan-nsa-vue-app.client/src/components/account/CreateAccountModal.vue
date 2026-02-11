<template>
  <Dialog :open="isOpen" @update:open="(value) => !value && $emit('close')">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle class="text-center">創建使用者</DialogTitle>
        <DialogDescription class="text-center">請填寫新用戶的基本資訊</DialogDescription>
      </DialogHeader>

      <form @submit="onSubmit">
        <div class="space-y-4">
          <div>
            <Label class="text-sm font-medium">使用者帳號 <span class="text-red-500">*</span></Label>
            <Input v-model="username" type="email" autocomplete="email" placeholder="user@example.com"
              :class="{ 'border-red-500': errors.username }" />
            <p v-if="errors.username" class="text-sm text-red-600 mt-1">{{ errors.username }}</p>
          </div>

          <div>
            <Label class="text-sm font-medium">姓名 <span class="text-red-500">*</span></Label>
            <Input v-model="name" placeholder="請輸入姓名" :class="{ 'border-red-500': errors.name }" />
            <p v-if="errors.name" class="text-sm text-red-600 mt-1">{{ errors.name }}</p>
          </div>

          <div>
            <Label class="text-sm font-medium">員工編號 <span class="text-red-500">*</span></Label>
            <Input v-model="employeeID" placeholder="請輸入員工編號" :class="{ 'border-red-500': errors.employeeID }" />
            <p v-if="errors.employeeID" class="text-sm text-red-600 mt-1">{{ errors.employeeID }}</p>
          </div>

          <div>
            <Label class="text-sm font-medium">角色設定 <span class="text-red-500">*</span></Label>
            <Select v-model="roleType" :class="{ 'border-red-500': errors.roleType }">
              <SelectTrigger>
                <SelectValue placeholder="- 請選擇 -" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Admin">北觀管理員</SelectItem>
                <SelectItem value="User">分站管理員</SelectItem>
              </SelectContent>
            </Select>
            <p v-if="errors.roleType" class="text-sm text-red-600 mt-1">{{ errors.roleType }}</p>
          </div>

          <div>
            <Label class="text-sm font-medium">密碼 <span class="text-red-500">*</span></Label>
            <Input v-model="password" type="password" autocomplete="new-password" placeholder="請輸入密碼"
              :class="{ 'border-red-500': errors.password }" />
            <p v-if="passwordStrengthMessages.length === 0 && errors.password" class="text-sm text-red-600 mt-1">{{
              errors.password }}</p>
            <div v-if="passwordStrengthMessages.length > 0" class="mt-2 text-sm text-red-600">
              <p>密碼不符合以下條件:</p>
              <ul class="list-disc list-inside">
                <li v-for="message in passwordStrengthMessages" :key="message">{{ message }}</li>
              </ul>
            </div>
          </div>

          <div>
            <Label class="text-sm font-medium">確認密碼 <span class="text-red-500">*</span></Label>
            <Input v-model="confirmPassword" type="password" autocomplete="new-password" placeholder="請再次輸入密碼"
              :class="{ 'border-red-500': errors.confirmPassword }" />
            <p v-if="errors.confirmPassword" class="text-sm text-red-600 mt-1">{{ errors.confirmPassword }}</p>
          </div>

          <div v-if="roleType === 'User'" class="space-y-3">
            <Label class="text-sm font-medium">管理分站 <span class="text-red-500">*</span></Label>
            <div class="grid grid-cols-1 gap-2 max-h-32 overflow-y-auto border rounded-md p-3 bg-gray-50">
              <div v-if="stations.length === 0" class="text-sm text-gray-500 text-center py-2">
                載入分站中...
              </div>
              <label v-for="station in stations" :key="station.id"
                class="flex items-center space-x-2 text-sm hover:bg-white rounded p-2 transition-colors">
                <input type="checkbox" :value="station.id" v-model="stationIds"
                  class="rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
                <span class="flex-1">{{ station.name }}</span>
              </label>
            </div>
            <div v-if="roleType === 'User' && stationIds.length === 0" class="text-xs text-orange-600">
              分站管理員必須至少選擇一個管理分站
            </div>
          </div>

          <div class="flex items-center space-x-2 pt-2">
            <Checkbox id="create-readonly" v-model:checked="isReadOnly" />
            <Label for="create-readonly" class="text-sm">設為唯讀帳戶</Label>
          </div>
        </div>

        <DialogFooter class="flex justify-end space-x-3 pt-6">
          <Button type="button" variant="outline" @click="$emit('close')">
            取消
          </Button>
          <Button type="submit" :disabled="isSubmitting">
            <span v-if="isSubmitting" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></span>
            {{ isSubmitting ? '建立中...' : '建立帳戶' }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { watch, ref, nextTick } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import * as z from 'zod'
import { StationResponse, AccountService, CreateAccountRequest } from '@/services'
import { handleApiError, ERROR_CODES, type ApiErrorResponse } from '@/utils/errorHandler'
import { useToast } from '@/composables/useToast'
import Button from '@/components/ui/button/Button.vue'
import Input from '@/components/ui/input/Input.vue'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import Label from '@/components/ui/label/Label.vue'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter
} from '@/components/ui/dialog'
import { Checkbox } from "@/components/ui/checkbox"

interface Props {
  isOpen: boolean
  stations: StationResponse[]
}

interface Emits {
  (e: 'close'): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()
const toast = useToast()

// Validation schema
const createSchema = toTypedSchema(z.object({
  username: z.string().email('請輸入有效的電子郵件地址').min(1, '請輸入使用者帳號'),
  name: z.string().min(1, '請輸入姓名'),
  employeeID: z.string().min(1, '請輸入員工編號'),
  roleType: z.string().min(1, '請選擇角色'),
  password: z.string()
    .min(6, '密碼長度至少為6個字符')
    .regex(/(?=.*\d)/, '密碼必須包含至少一個數字')
    .regex(/(?=.*[a-z])/, '密碼必須包含至少一個小寫字母')
    .regex(/(?=.*[A-Z])/, '密碼必須包含至少一個大寫字母'),
  confirmPassword: z.string().min(1, '請確認密碼'),
  isReadOnly: z.boolean().default(false)
}).refine((data) => data.password === data.confirmPassword, {
  message: '密碼確認不一致',
  path: ['confirmPassword']
}))

// Use VeeValidate form
const { handleSubmit, resetForm, errors, isSubmitting, defineField } = useForm({
  validationSchema: createSchema
})

// Define form fields
const [username] = defineField('username')
const [name] = defineField('name')
const [employeeID] = defineField('employeeID')
const [roleType] = defineField('roleType')
const [password] = defineField('password')
const [confirmPassword] = defineField('confirmPassword')
const [isReadOnly] = defineField('isReadOnly')

// Password strength messages
const passwordStrengthMessages = ref<string[]>([])

watch(password, (newPassword) => {
  passwordStrengthMessages.value = []
  if (!newPassword) return

  if (newPassword.length < 6) {
    passwordStrengthMessages.value.push('密碼長度至少為6個字符')
  }
  if (!/(?=.*\d)/.test(newPassword)) {
    passwordStrengthMessages.value.push('密碼必須包含至少一個數字')
  }
  if (!/(?=.*[a-z])/.test(newPassword)) {
    passwordStrengthMessages.value.push('密碼必須包含至少一個小寫字母')
  }
  if (!/(?=.*[A-Z])/.test(newPassword)) {
    passwordStrengthMessages.value.push('密碼必須包含至少一個大寫字母')
  }
}, { immediate: true })

// Station IDs - handled separately since it's not a simple field
const stationIds = ref<number[]>([])

// Watchers
watch(() => roleType.value, (newVal) => {
  if (newVal === 'Admin') {
    stationIds.value = []
  }
})

watch(() => password.value, (newVal) => {
  // 當密碼清空時，也清空確認密碼
  if (!newVal || newVal.trim().length === 0) {
    confirmPassword.value = ''
  }
})

// Watch for modal open/close to reset form
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    nextTick(() => {
      // Reset form when modal opens
      resetForm()
      stationIds.value = []
    })
  }
})

const onSubmit = handleSubmit(async (values) => {
  try {
    // 驗證分站管理員必須選擇分站
    if (values.roleType === 'User' && stationIds.value.length === 0) {
      toast.error('分站管理員必須至少選擇一個管理分站')
      return
    }

    const request = new CreateAccountRequest()
    request.username = values.username
    request.name = values.name
    request.employeeId = values.employeeID
    request.role = values.roleType
    request.password = values.password
    request.isReadOnly = values.isReadOnly || false

    // 如果是分站管理員，設置分站ID
    if (values.roleType === 'User') {
      request.stationIds = stationIds.value
    }

    await AccountService.createAccount(request)
    toast.success('帳戶創建成功')
    emit('success')
    emit('close')
  } catch (error) {
    console.error('創建帳戶失敗:', error)
    const apiError: ApiErrorResponse = await handleApiError(error)

    // 根據錯誤類型顯示不同的錯誤訊息
    if (apiError.errorCode === ERROR_CODES.VALIDATION_FAILED && apiError.validationErrors) {
      const errorMessages = Object.entries(apiError.validationErrors)
        .map(([field, errors]) => `${field}: ${errors.join(', ')}`)
        .join('\n')
      toast.error(`驗證失敗: ${errorMessages}`)
    } else if (apiError.errorCode === ERROR_CODES.RESOURCE_ALREADY_EXISTS) {
      toast.error('該帳號已存在')
    } else {
      toast.error(`創建帳戶失敗: ${apiError.message}`)
    }
  }
})
</script>
