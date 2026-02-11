<template>
  <Dialog :open="isOpen" @update:open="(value) => !value && $emit('close')">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle class="text-center">更新使用者</DialogTitle>
        <DialogDescription class="text-center">修改用戶資訊</DialogDescription>
      </DialogHeader>

      <form @submit="onSubmit">
        <div class="space-y-4">
          <div>
            <Label class="text-sm font-medium">使用者帳號</Label>
            <Input v-model="username" autocomplete="off" type="email" disabled class="bg-gray-50" />
            <p class="text-xs text-gray-500 mt-1">帳號無法修改</p>
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
            <Label class="text-sm font-medium">新密碼</Label>
            <Input v-model="password" type="password" autocomplete="new-password" placeholder="留空則不修改密碼"
              :class="{ 'border-red-500': errors.password }" />
            <p v-if="passwordStrengthMessages.length === 0 && errors.password" class="text-sm text-red-600 mt-1">{{
              errors.password }}</p>
            <div v-if="passwordStrengthMessages.length > 0" class="mt-2 text-sm text-red-600">
              <p>密碼不符合以下條件:</p>
              <ul class="list-disc list-inside">
                <li v-for="message in passwordStrengthMessages" :key="message">{{ message }}</li>
              </ul>
            </div>
            <p class="text-xs text-gray-500 mt-1">留空則不修改現有密碼</p>
          </div>

          <div v-if="showPasswordConfirm">
            <Label class="text-sm font-medium">確認新密碼 <span class="text-red-500">*</span></Label>
            <Input v-model="confirmPassword" type="password" autocomplete="new-password" placeholder="請再次輸入新密碼"
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
            <Checkbox id="update-readonly" v-model:checked="isReadOnly" />
            <Label for="update-readonly" class="text-sm">設為唯讀帳戶</Label>
          </div>
        </div>

        <div class="flex justify-end space-x-3 pt-6">
          <Button type="button" variant="outline" @click="$emit('close')">
            取消
          </Button>
          <Button type="submit" :disabled="isSubmitting">
            <span v-if="isSubmitting" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></span>
            {{ isSubmitting ? '更新中...' : '更新帳戶' }}
          </Button>
        </div>
      </form>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { watch, ref, nextTick } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import * as z from 'zod'
import { StationResponse, UserResponse, AccountService, UpdateAccountRequest } from '@/services'
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
} from '@/components/ui/dialog'
import { FormField } from '@/components/ui/form'
import { Checkbox } from "@/components/ui/checkbox"

interface Props {
  isOpen: boolean
  stations: StationResponse[]
  account?: UserResponse | null
}

interface Emits {
  (e: 'close'): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()
const toast = useToast()

const showPasswordConfirm = ref(false)

// Fixed validation schema - password is optional for updates
const updateSchema = toTypedSchema(z.object({
  username: z.string().min(1, '請輸入帳號'),
  name: z.string().min(1, '請輸入姓名'),
  employeeID: z.string().min(1, '請輸入員工編號'),
  roleType: z.string().min(1, '請選擇角色'),
  password: z.string().optional()
    .refine((val) => !val || val.length >= 6, '密碼長度至少為6個字符')
    .refine((val) => !val || /(?=.*\d)/.test(val), '密碼必須包含至少一個數字')
    .refine((val) => !val || /(?=.*[a-z])/.test(val), '密碼必須包含至少一個小寫字母')
    .refine((val) => !val || /(?=.*[A-Z])/.test(val), '密碼必須包含至少一個大寫字母'),
  confirmPassword: z.string().optional(),
  isReadOnly: z.boolean().default(false)
}).refine((data) => {
  // 如果有輸入密碼，則確認密碼必須一致
  if (data.password && data.password.trim().length > 0) {
    return data.password === data.confirmPassword
  }
  return true
}, {
  message: '密碼確認不一致',
  path: ['confirmPassword']
}))



// Use VeeValidate form
const { handleSubmit, setValues, errors, isSubmitting, defineField } = useForm({
  validationSchema: updateSchema
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
  if (!newPassword || newPassword.trim().length === 0) return

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
  showPasswordConfirm.value = !!(newVal && newVal.trim().length > 0)
  if (!showPasswordConfirm.value) {
    confirmPassword.value = ''
  }
})

// Watch for account changes to populate form
watch(() => [props.account, props.isOpen], ([account, isOpen]) => {
  if (account && isOpen && typeof account === 'object' && 'username' in account) {
    nextTick(() => {
      // Reset form and populate with account data
      setValues({
        username: account.username || '',
        name: account.name || '',
        employeeID: account.employeeId || '',
        roleType: account.role === 'Admin' ? 'Admin' : 'User',
        isReadOnly: account.isReadOnly || false,
        password: '',
        confirmPassword: ''
      })

      stationIds.value = [...(account.stationIds || [])]

      // Reset password confirmation display
      showPasswordConfirm.value = false
    })
  } else if (isOpen) {
    nextTick(() => {
      // Reset form
      setValues({
        username: '',
        name: '',
        employeeID: '',
        roleType: '',
        password: '',
        confirmPassword: '',
        isReadOnly: false
      })
      stationIds.value = []
      showPasswordConfirm.value = false
    })
  }
}, { immediate: true })

const onSubmit = handleSubmit(async (values) => {
  if (!props.account?.id) return

  try {
    // 驗證分站管理員必須選擇分站
    if (values.roleType === 'User' && stationIds.value.length === 0) {
      toast.error('分站管理員必須至少選擇一個管理分站')
      return
    }

    const request = new UpdateAccountRequest()
    request.name = values.name
    request.employeeId = values.employeeID
    request.role = values.roleType
    request.isReadOnly = values.isReadOnly || false

    // 如果有輸入新密碼，則更新密碼
    if (values.password && values.password.trim() !== '') {
      request.password = values.password
    }

    // 如果是分站管理員，設置分站ID
    if (values.roleType === 'User') {
      request.stationIds = stationIds.value
    }

    await AccountService.updateAccount(props.account.id.toString(), request)
    toast.success('帳戶更新成功')
    emit('success')
    emit('close')
  } catch (error) {
    console.error('更新帳戶失敗:', error)
    const apiError: ApiErrorResponse = await handleApiError(error)

    // 根據錯誤類型顯示不同的錯誤訊息
    if (apiError.errorCode === ERROR_CODES.VALIDATION_FAILED && apiError.validationErrors) {
      const errorMessages = Object.entries(apiError.validationErrors)
        .map(([field, errors]) => `${field}: ${errors.join(', ')}`)
        .join('\n')
      toast.error(`驗證失敗: ${errorMessages}`)
    } else if (apiError.errorCode === ERROR_CODES.RESOURCE_NOT_FOUND) {
      toast.error('找不到指定的帳戶')
    } else {
      toast.error(`更新帳戶失敗: ${apiError.message}`)
    }
  }
})
</script>
