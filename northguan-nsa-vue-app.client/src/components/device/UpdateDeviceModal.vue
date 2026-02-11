<template>
  <Dialog :open="isOpen" @update:open="$emit('close')">
    <DialogContent class="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
      <DialogHeader>
        <DialogTitle class="text-center">編輯設備</DialogTitle>
        <DialogDescription class="text-center">修改設備資訊</DialogDescription>
      </DialogHeader>

      <form class="space-y-6" @submit="onSubmit">
        <!-- 基本資訊 -->
        <div class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 border-b pb-2">基本資訊</h3>

          <FormField v-slot="{ componentField }" name="name">
            <FormItem>
              <FormLabel>設備名稱 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="請輸入設備名稱"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField v-slot="{ componentField }" name="stationId">
            <FormItem>
              <FormLabel>分站 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Select v-bind="componentField">
                  <SelectTrigger>
                    <SelectValue placeholder="- 請選擇分站 -" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="station in stations" :key="station.id" :value="station.id">
                      {{ station.name }}
                    </SelectItem>
                  </SelectContent>
                </Select>
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField v-slot="{ componentField }" name="type">
            <FormItem>
              <FormLabel>設備類型 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Select v-bind="componentField" @update:model-value="handleTypeChange" disabled="{{ !!componentField.type }}">
                  <SelectTrigger>
                    <SelectValue placeholder="- 請選擇設備類型 -" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="crowd">人流辨識</SelectItem>
                    <SelectItem value="parking">停車場</SelectItem>
                    <SelectItem value="traffic">車流</SelectItem>
                    <SelectItem value="fence">電子圍籬</SelectItem>
                    <SelectItem value="highResolution">4K影像</SelectItem>
                  </SelectContent>
                </Select>
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <div class="grid grid-cols-2 gap-4">
            <FormField v-slot="{ componentField }" name="latitude">
              <FormItem>
                <FormLabel>座標 緯度 <span class="text-red-500">*</span></FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.000001"
                    placeholder="25.0330"
                    v-bind="componentField"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>

            <FormField v-slot="{ componentField }" name="longitude">
              <FormItem>
                <FormLabel>座標 經度 <span class="text-red-500">*</span></FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.000001"
                    placeholder="121.5654"
                    v-bind="componentField"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
          </div>

          <FormField v-slot="{ componentField }" name="serial">
            <FormItem>
              <FormLabel>設備序號 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="請輸入設備序號"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        </div>

        <!-- 人流辨識特定欄位 -->
        <div v-if="selectedType === 'crowd'" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 border-b pb-2">人流辨識設定</h3>

          <FormField v-slot="{ componentField }" name="videoUrl">
            <FormItem>
              <FormLabel>串流影片URL</FormLabel>
              <FormControl>
                <Input
                  type="url"
                  placeholder="請輸入串流影片URL"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField v-slot="{ componentField }" name="apiUrl">
            <FormItem>
              <FormLabel>API URL</FormLabel>
              <FormControl>
                <Input
                  type="url"
                  placeholder="請輸入API URL"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField v-slot="{ componentField }" name="area">
            <FormItem>
              <FormLabel>面積(平方公尺)</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min="0"
                  placeholder="請輸入面積"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        </div>

        <!-- 停車場特定欄位 -->
        <div v-if="selectedType === 'parking'" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 border-b pb-2">停車場設定</h3>

          <FormField v-slot="{ componentField }" name="numberOfParking">
            <FormItem>
              <FormLabel>總車位數</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min="0"
                  placeholder="請輸入總車位數"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField v-slot="{ componentField }" name="apiUrl">
            <FormItem>
              <FormLabel>API URL</FormLabel>
              <FormControl>
                <Input
                  type="url"
                  placeholder="請輸入API URL"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        </div>

        <!-- 車流特定欄位 -->
        <div v-if="selectedType === 'traffic'" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 border-b pb-2">車流設定</h3>

          <FormField v-slot="{ componentField }" name="speedLimit">
            <FormItem>
              <FormLabel>道路速限</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min="0"
                  placeholder="請輸入道路速限(km/h)"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField v-slot="{ componentField }" name="city">
            <FormItem>
              <FormLabel>縣市</FormLabel>
              <FormControl>
                <Select v-bind="componentField">
                  <SelectTrigger>
                    <SelectValue placeholder="- 請選擇縣市 -" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Taipei">臺北市</SelectItem>
                    <SelectItem value="Taoyuao">桃園市</SelectItem>
                    <SelectItem value="Keelung">基隆市</SelectItem>
                  </SelectContent>
                </Select>
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField v-slot="{ componentField }" name="eTagNumber">
            <FormItem>
              <FormLabel>eTag配對路段代碼</FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="請輸入eTag配對路段代碼"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        </div>

        <!-- 電子圍籬特定欄位 -->
        <div v-if="selectedType === 'fence'" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 border-b pb-2">電子圍籬設定</h3>

          <div class="grid grid-cols-2 gap-4">
            <FormField v-slot="{ componentField }" name="observingTimeStart">
              <FormItem>
                <FormLabel>監管時間 開始</FormLabel>
                <FormControl>
                  <Input
                    type="time"
                    v-bind="componentField"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>

            <FormField v-slot="{ componentField }" name="observingTimeEnd">
              <FormItem>
                <FormLabel>監管時間 結束</FormLabel>
                <FormControl>
                  <Input
                    type="time"
                    v-bind="componentField"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
          </div>
        </div>

        <!-- 4K影像特定欄位 -->
        <div v-if="selectedType === 'highResolution'" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 border-b pb-2">4K影像設定</h3>

          <FormField v-slot="{ componentField }" name="videoUrl">
            <FormItem>
              <FormLabel>串流影片URL</FormLabel>
              <FormControl>
                <Input
                  type="url"
                  placeholder="請輸入串流影片URL"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        </div>

        <!-- 錯誤訊息顯示區域 -->
        <div v-if="submitError" class="p-4 bg-red-50 border border-red-200 rounded-md">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-red-800">更新失敗</h3>
              <div class="mt-2 text-sm text-red-700 whitespace-pre-line">
                {{ submitError }}
              </div>
            </div>
          </div>
        </div>

        <DialogFooter class="flex justify-end space-x-3 pt-6 border-t">
          <Button type="button" variant="outline" @click="$emit('close')" :disabled="isSubmitting">
            取消
          </Button>
          <Button type="submit" :disabled="isSubmitting">
            <div v-if="isSubmitting" class="flex items-center">
              <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              更新中...
            </div>
            <span v-else>更新設備</span>
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import { toTypedSchema } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import * as z from 'zod'
import {
  DeviceService,
  UpdateDeviceRequest,
  DeviceListResponse,
  StationResponse
} from '@/services'
import { useToast } from '@/composables/useToast'
import { handleApiError, ERROR_CODES, type ApiErrorResponse } from '@/utils/errorHandler'
import Button from '@/components/ui/button/Button.vue'
import Input from '@/components/ui/input/Input.vue'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter
} from '@/components/ui/dialog'
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'

interface Props {
  isOpen: boolean
  stations: StationResponse[]
  device: DeviceListResponse | null
}

interface Emits {
  (e: 'close'): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const toast = useToast()

// State
const selectedType = ref<string>('')
const submitError = ref<string>('')
const isSubmitting = ref<boolean>(false)

// Validation schema
const deviceSchema = toTypedSchema(z.object({
  name: z.string().min(1, '請輸入設備名稱'),
  type: z.string().min(1, '請選擇設備類型'),
  stationId: z.number({ required_error: '請選擇所屬分站' }),
  latitude: z.number({ required_error: '請輸入緯度' }),
  longitude: z.number({ required_error: '請輸入經度' }),
  serial: z.string().min(1, '請輸入設備序號'),
  // Type-specific fields
  videoUrl: z.string().optional(),
  apiUrl: z.string().optional(),
  area: z.number().optional(),
  numberOfParking: z.number().optional(),
  speedLimit: z.number().optional(),
  city: z.string().optional(),
  eTagNumber: z.string().optional(),
  observingTimeStart: z.string().optional(),
  observingTimeEnd: z.string().optional()
}))

// Use VeeValidate form
const { handleSubmit, setValues, setFieldError } = useForm({
  validationSchema: deviceSchema
})

const handleTypeChange = (type: string) => {
  selectedType.value = type
}

// Watch for device changes and populate form
watch(() => [props.device, props.isOpen], ([device, isOpen]) => {
  if (device && isOpen) {
    selectedType.value = device.type || ''
    // 清除錯誤訊息
    submitError.value = ''
    nextTick(() => {
      setValues({
        name: device.name || '',
        type: device.type || '',
        stationId: device.stationID || null,
        latitude: device.lat || null,
        longitude: device.lng || null,
        serial: device.serial || '',
        // Type-specific fields
        videoUrl: device.videoUrl || '',
        apiUrl: device.apiUrl || '',
        area: device.area || 0,
        numberOfParking: device.numberOfParking || 0,
        speedLimit: device.speedLimit || 0,
        city: device.city || '',
        eTagNumber: device.eTagNumber || '',
        observingTimeStart: device.observingTimeStart || '',
        observingTimeEnd: device.observingTimeEnd || ''
      })
    })
  } else if (!isOpen) {
    // Reset error state when modal closes
    submitError.value = ''
  }
}, { immediate: true })

const onSubmit = handleSubmit(async (values) => {
  if (!props.device) return

  // 防止重複提交
  if (isSubmitting.value) {
    console.log('正在提交中，忽略重複點擊')
    return
  }

  // 清除之前的錯誤訊息
  submitError.value = ''

  try {
    isSubmitting.value = true

    const request = new UpdateDeviceRequest()
    request.name = values.name
    request.type = values.type
    request.lat = Number(values.latitude)
    request.lng = Number(values.longitude)
    request.serial = values.serial

    // Type-specific properties
    if (values.type === 'crowd') {
      request.videoUrl = values.videoUrl || ''
      request.apiUrl = values.apiUrl || ''
      request.area = values.area || 0
    } else if (values.type === 'parking') {
      request.numberOfParking = values.numberOfParking || 0
      request.apiUrl = values.apiUrl || ''
    } else if (values.type === 'traffic') {
      request.speedLimit = values.speedLimit || 0
      request.city = values.city || ''
      request.eTagNumber = values.eTagNumber || ''
    } else if (values.type === 'fence') {
      request.observingTimeStart = values.observingTimeStart || ''
      request.observingTimeEnd = values.observingTimeEnd || ''
    } else if (values.type === 'highResolution') {
      request.videoUrl = values.videoUrl || ''
    }

    await DeviceService.updateDevice(props.device.id || 0, request)

    // 成功後清理並關閉
    submitError.value = ''
    toast.success('更新成功', '設備資訊已更新')
    emit('success')
    emit('close')
  } catch (error: any) {
    console.error('更新設備失敗:', error)

    // 如果是重複請求錯誤，靜默處理
    if (error.name === 'DuplicateRequestError' || error.isDuplicateRequest) {
      console.log('UpdateDeviceModal: 重複請求被靜默處理')
      return
    }

    const apiError: ApiErrorResponse = await handleApiError(error)

    // 在 modal 內顯示錯誤訊息
    if (apiError.errorCode === ERROR_CODES.VALIDATION_FAILED) {
      if (apiError.validationErrors && Object.keys(apiError.validationErrors).length > 0) {
        // 直接使用後端返回的字段名稱設置錯誤，因為後端已經返回前端字段名稱
        for (const [fieldName, errors] of Object.entries(apiError.validationErrors)) {
          if (fieldName in values) {
            setFieldError(fieldName as keyof typeof values, errors.join(', '))
          }
        }
      } else {
        submitError.value = apiError.message || '驗證失敗，請檢查輸入的資料'
      }
    } else if (apiError.errorCode === ERROR_CODES.RESOURCE_ALREADY_EXISTS) {
      submitError.value = '設備名稱已存在，請使用不同的名稱'
    } else {
      submitError.value = `無法更新設備: ${apiError.message}`
    }
  } finally {
    isSubmitting.value = false
  }
})
</script>
