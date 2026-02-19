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
                    <SelectItem value="water">水域監控</SelectItem>
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

        <!-- 水域監控特定欄位 -->
        <div v-if="selectedType === 'water'" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 border-b pb-2">水域監控設定</h3>

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

        <!-- 攝影機設定（所有類型共用） -->
        <div v-if="selectedType" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 border-b pb-2">攝影機設定</h3>

          <div class="grid grid-cols-2 gap-4">
            <FormField v-slot="{ componentField }" name="cameraType">
              <FormItem>
                <FormLabel>攝影機類型</FormLabel>
                <FormControl>
                  <Select v-bind="componentField" @update:model-value="handleCameraTypeChange">
                    <SelectTrigger>
                      <SelectValue placeholder="- 請選擇攝影機類型 -" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="fixed">固定式</SelectItem>
                      <SelectItem value="ptz">球機 (PTZ)</SelectItem>
                      <SelectItem value="panoramic">全景聯動</SelectItem>
                    </SelectContent>
                  </Select>
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>

            <FormField v-if="selectedCameraType === 'ptz'" v-slot="{ componentField }" name="cameraBrand">
              <FormItem>
                <FormLabel>廠牌</FormLabel>
                <FormControl>
                  <Select v-bind="componentField" @update:model-value="handleCameraBrandChange">
                    <SelectTrigger>
                      <SelectValue placeholder="- 請選擇廠牌 -" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="axis">Axis</SelectItem>
                      <SelectItem value="bosch">Bosch</SelectItem>
                      <SelectItem value="onvif">ONVIF</SelectItem>
                    </SelectContent>
                  </Select>
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
          </div>

          <FormField v-if="selectedCameraType" v-slot="{ componentField }" name="camRtspUrl">
            <FormItem>
              <FormLabel>RTSP URL <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="rtsp://admin:password@192.168.1.1/stream"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <FormField v-if="selectedCameraType" v-slot="{ componentField }" name="camOutputSize">
            <FormItem>
              <FormLabel>輸出解析度</FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="1280x720"
                  v-bind="componentField"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <!-- Axis 專屬欄位 -->
          <div v-if="selectedCameraType === 'ptz' && selectedCameraBrand === 'axis'" class="space-y-4 p-4 bg-gray-50 rounded-md">
            <h4 class="text-sm font-medium text-gray-700">Axis VAPIX 設定</h4>
            <FormField v-slot="{ componentField }" name="axisBaseUrl">
              <FormItem>
                <FormLabel>Axis Base URL</FormLabel>
                <FormControl>
                  <Input type="text" placeholder="http://61.219.173.73" v-bind="componentField" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
            <div class="grid grid-cols-2 gap-4">
              <FormField v-slot="{ componentField }" name="axisUser">
                <FormItem>
                  <FormLabel>帳號</FormLabel>
                  <FormControl>
                    <Input type="text" placeholder="root" v-bind="componentField" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>
              <FormField v-slot="{ componentField }" name="axisPassword">
                <FormItem>
                  <FormLabel>密碼</FormLabel>
                  <FormControl>
                    <Input type="password" placeholder="密碼" v-bind="componentField" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>
            </div>
            <FormField v-slot="{ componentField }" name="axisCamera">
              <FormItem>
                <FormLabel>Camera 編號</FormLabel>
                <FormControl>
                  <Input type="number" min="1" placeholder="1" v-bind="componentField" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
          </div>

          <!-- Bosch 專屬欄位 -->
          <div v-if="selectedCameraType === 'ptz' && selectedCameraBrand === 'bosch'" class="space-y-4 p-4 bg-gray-50 rounded-md">
            <h4 class="text-sm font-medium text-gray-700">Bosch 設定</h4>
            <FormField v-slot="{ componentField }" name="boschBaseUrl">
              <FormItem>
                <FormLabel>Bosch Base URL</FormLabel>
                <FormControl>
                  <Input type="text" placeholder="http://61.222.158.251:10562" v-bind="componentField" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
            <div class="grid grid-cols-2 gap-4">
              <FormField v-slot="{ componentField }" name="boschUser">
                <FormItem>
                  <FormLabel>帳號</FormLabel>
                  <FormControl>
                    <Input type="text" placeholder="service" v-bind="componentField" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>
              <FormField v-slot="{ componentField }" name="boschPassword">
                <FormItem>
                  <FormLabel>密碼</FormLabel>
                  <FormControl>
                    <Input type="password" placeholder="密碼" v-bind="componentField" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>
            </div>
          </div>

          <!-- ONVIF 專屬欄位 -->
          <div v-if="selectedCameraType === 'ptz' && selectedCameraBrand === 'onvif'" class="space-y-4 p-4 bg-gray-50 rounded-md">
            <h4 class="text-sm font-medium text-gray-700">ONVIF 設定</h4>
            <FormField v-slot="{ componentField }" name="onvifUrl">
              <FormItem>
                <FormLabel>ONVIF URL</FormLabel>
                <FormControl>
                  <Input type="text" placeholder="http://192.168.1.1/onvif/device_service" v-bind="componentField" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
            <div class="grid grid-cols-2 gap-4">
              <FormField v-slot="{ componentField }" name="onvifUser">
                <FormItem>
                  <FormLabel>帳號</FormLabel>
                  <FormControl>
                    <Input type="text" placeholder="admin" v-bind="componentField" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>
              <FormField v-slot="{ componentField }" name="onvifPassword">
                <FormItem>
                  <FormLabel>密碼</FormLabel>
                  <FormControl>
                    <Input type="password" placeholder="密碼" v-bind="componentField" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>
            </div>
            <FormField v-slot="{ componentField }" name="ptzHFov">
              <FormItem>
                <FormLabel>PTZ 水平視角 (度)</FormLabel>
                <FormControl>
                  <Input type="number" step="0.1" placeholder="60" v-bind="componentField" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
          </div>

          <!-- 全景聯動專屬欄位 -->
          <div v-if="selectedCameraType === 'panoramic'" class="space-y-4 p-4 bg-gray-50 rounded-md">
            <h4 class="text-sm font-medium text-gray-700">全景聯動設定</h4>
            <FormField v-slot="{ componentField }" name="ptzTarget">
              <FormItem>
                <FormLabel>聯動球機名稱</FormLabel>
                <FormControl>
                  <Input type="text" placeholder="例如: ptz1" v-bind="componentField" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>
            <div class="grid grid-cols-3 gap-4">
              <FormField v-slot="{ componentField }" name="panPerPixelX">
                <FormItem>
                  <FormLabel>PanPerPixelX</FormLabel>
                  <FormControl>
                    <Input type="number" step="0.00001" placeholder="0.15703" v-bind="componentField" />
                  </FormControl>
                </FormItem>
              </FormField>
              <FormField v-slot="{ componentField }" name="panPerPixelY">
                <FormItem>
                  <FormLabel>PanPerPixelY</FormLabel>
                  <FormControl>
                    <Input type="number" step="0.00001" placeholder="0" v-bind="componentField" />
                  </FormControl>
                </FormItem>
              </FormField>
              <FormField v-slot="{ componentField }" name="panOffset">
                <FormItem>
                  <FormLabel>PanOffset</FormLabel>
                  <FormControl>
                    <Input type="number" step="0.1" placeholder="94.0" v-bind="componentField" />
                  </FormControl>
                </FormItem>
              </FormField>
            </div>
            <div class="grid grid-cols-3 gap-4">
              <FormField v-slot="{ componentField }" name="tiltPerPixelX">
                <FormItem>
                  <FormLabel>TiltPerPixelX</FormLabel>
                  <FormControl>
                    <Input type="number" step="0.00001" placeholder="0" v-bind="componentField" />
                  </FormControl>
                </FormItem>
              </FormField>
              <FormField v-slot="{ componentField }" name="tiltPerPixelY">
                <FormItem>
                  <FormLabel>TiltPerPixelY</FormLabel>
                  <FormControl>
                    <Input type="number" step="0.00001" placeholder="-0.1148" v-bind="componentField" />
                  </FormControl>
                </FormItem>
              </FormField>
              <FormField v-slot="{ componentField }" name="tiltOffset">
                <FormItem>
                  <FormLabel>TiltOffset</FormLabel>
                  <FormControl>
                    <Input type="number" step="0.1" placeholder="20.0" v-bind="componentField" />
                  </FormControl>
                </FormItem>
              </FormField>
            </div>
          </div>
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
const selectedCameraType = ref<string>('')
const selectedCameraBrand = ref<string>('')
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
  observingTimeEnd: z.string().optional(),
  // Camera config fields
  cameraType: z.string().optional(),
  cameraBrand: z.string().optional(),
  camRtspUrl: z.string().optional(),
  camOutputSize: z.string().optional(),
  axisBaseUrl: z.string().optional(),
  axisUser: z.string().optional(),
  axisPassword: z.string().optional(),
  axisCamera: z.coerce.number().optional(),
  boschBaseUrl: z.string().optional(),
  boschUser: z.string().optional(),
  boschPassword: z.string().optional(),
  onvifUrl: z.string().optional(),
  onvifUser: z.string().optional(),
  onvifPassword: z.string().optional(),
  ptzHFov: z.coerce.number().optional(),
  ptzTarget: z.string().optional(),
  panPerPixelX: z.coerce.number().optional(),
  panPerPixelY: z.coerce.number().optional(),
  panOffset: z.coerce.number().optional(),
  tiltPerPixelX: z.coerce.number().optional(),
  tiltPerPixelY: z.coerce.number().optional(),
  tiltOffset: z.coerce.number().optional()
}))

// Use VeeValidate form
const { handleSubmit, setValues, setFieldError } = useForm({
  validationSchema: deviceSchema
})

const handleTypeChange = (type: string) => {
  selectedType.value = type
  selectedCameraType.value = ''
  selectedCameraBrand.value = ''
}

const handleCameraTypeChange = (type: string | number | boolean | null) => {
  if (typeof type === 'string') {
    selectedCameraType.value = type
    selectedCameraBrand.value = ''
  }
}

const handleCameraBrandChange = (brand: string | number | boolean | null) => {
  if (typeof brand === 'string') {
    selectedCameraBrand.value = brand
  }
}

const parseCameraConfig = (configJson: string | null | undefined): Record<string, any> => {
  if (!configJson) return {}
  try {
    return JSON.parse(configJson)
  } catch {
    return {}
  }
}

const buildCameraConfigJson = (values: any): string | undefined => {
  if (!values.cameraType || !values.camRtspUrl) return undefined

  const config: Record<string, any> = {
    CameraType: values.cameraType === 'ptz' ? `ptz_${values.cameraBrand || 'onvif'}` : values.cameraType,
    RtspUrl: values.camRtspUrl,
    OutputSize: values.camOutputSize || '1280x720'
  }

  if (values.cameraType === 'ptz' && values.cameraBrand === 'axis') {
    config.AxisBaseUrl = values.axisBaseUrl || ''
    config.AxisUser = values.axisUser || ''
    config.AxisPassword = values.axisPassword || ''
    config.AxisCamera = values.axisCamera || 1
  } else if (values.cameraType === 'ptz' && values.cameraBrand === 'bosch') {
    config.BoschBaseUrl = values.boschBaseUrl || ''
    config.BoschUser = values.boschUser || ''
    config.BoschPassword = values.boschPassword || ''
  } else if (values.cameraType === 'ptz' && values.cameraBrand === 'onvif') {
    config.OnvifUrl = values.onvifUrl || ''
    config.OnvifUser = values.onvifUser || ''
    config.OnvifPassword = values.onvifPassword || ''
    config.PtzHFov = values.ptzHFov || 60
  } else if (values.cameraType === 'panoramic') {
    config.PtzTarget = values.ptzTarget || ''
    config.PanPerPixelX = values.panPerPixelX || 0
    config.PanPerPixelY = values.panPerPixelY || 0
    config.PanOffset = values.panOffset || 0
    config.TiltPerPixelX = values.tiltPerPixelX || 0
    config.TiltPerPixelY = values.tiltPerPixelY || 0
    config.TiltOffset = values.tiltOffset || 0
  }

  return JSON.stringify(config)
}

// Watch for device changes and populate form
watch(() => [props.device, props.isOpen], ([device, isOpen]) => {
  if (device && isOpen) {
    selectedType.value = device.type || ''
    // 清除錯誤訊息
    submitError.value = ''

    // 解析攝影機設定
    const camConfig = parseCameraConfig(device.cameraConfig)
    let camType = ''
    let camBrand = ''
    if (camConfig.CameraType) {
      if (camConfig.CameraType === 'fixed') {
        camType = 'fixed'
      } else if (camConfig.CameraType === 'panoramic') {
        camType = 'panoramic'
      } else if (camConfig.CameraType?.startsWith('ptz_')) {
        camType = 'ptz'
        camBrand = camConfig.CameraType.replace('ptz_', '')
      }
    }
    selectedCameraType.value = camType
    selectedCameraBrand.value = camBrand

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
        observingTimeEnd: device.observingTimeEnd || '',
        // Camera config fields
        cameraType: camType || undefined,
        cameraBrand: camBrand || undefined,
        camRtspUrl: camConfig.RtspUrl || '',
        camOutputSize: camConfig.OutputSize || '',
        axisBaseUrl: camConfig.AxisBaseUrl || '',
        axisUser: camConfig.AxisUser || '',
        axisPassword: camConfig.AxisPassword || '',
        axisCamera: camConfig.AxisCamera || undefined,
        boschBaseUrl: camConfig.BoschBaseUrl || '',
        boschUser: camConfig.BoschUser || '',
        boschPassword: camConfig.BoschPassword || '',
        onvifUrl: camConfig.OnvifUrl || '',
        onvifUser: camConfig.OnvifUser || '',
        onvifPassword: camConfig.OnvifPassword || '',
        ptzHFov: camConfig.PtzHFov || undefined,
        ptzTarget: camConfig.PtzTarget || '',
        panPerPixelX: camConfig.PanPerPixelX || undefined,
        panPerPixelY: camConfig.PanPerPixelY || undefined,
        panOffset: camConfig.PanOffset || undefined,
        tiltPerPixelX: camConfig.TiltPerPixelX || undefined,
        tiltPerPixelY: camConfig.TiltPerPixelY || undefined,
        tiltOffset: camConfig.TiltOffset || undefined
      })
    })
  } else if (!isOpen) {
    // Reset error state when modal closes
    submitError.value = ''
    selectedCameraType.value = ''
    selectedCameraBrand.value = ''
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
    } else if (values.type === 'water') {
      request.videoUrl = values.videoUrl || ''
    }

    // 攝影機設定（所有類型共用）
    request.cameraConfig = buildCameraConfigJson(values)

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
