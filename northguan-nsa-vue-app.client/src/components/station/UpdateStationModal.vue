<template>
  <Dialog :open="isOpen" @update:open="$emit('close')">
    <DialogContent class="sm:max-w-[425px]">
      <DialogHeader>
        <DialogTitle class="text-center">編輯分站</DialogTitle>
        <DialogDescription class="text-center">修改分站資訊</DialogDescription>
      </DialogHeader>

      <form @submit="onSubmit">
        <div class="space-y-4">
          <div>
            <Label class="text-sm font-medium">分站名稱 <span class="text-red-500">*</span></Label>
            <Input
              v-model="name"
              placeholder="請輸入分站名稱"
              :class="{ 'border-red-500': errors.name }"
            />
            <p v-if="errors.name" class="text-sm text-red-600 mt-1">{{ errors.name }}</p>
          </div>

          <div class="space-y-3">
            <div class="flex items-center space-x-2">
              <input
                id="enableNotify"
                v-model="enableNotify"
                type="checkbox"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <Label for="enableNotify" class="text-sm font-medium">啟用通知功能</Label>
            </div>
            
            <div v-if="enableNotify" class="ml-6">
              <Label class="text-sm font-medium">Line Token</Label>
              <Input
                v-model="lineToken"
                placeholder="請輸入 Line Token"
                :class="{ 'border-red-500': errors.lineToken }"
              />
              <p v-if="errors.lineToken" class="text-sm text-red-600 mt-1">{{ errors.lineToken }}</p>
              <p class="text-xs text-gray-500 mt-1">啟用通知功能時需要提供 Line Token</p>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <Label class="text-sm font-medium">緯度</Label>
              <Input
                v-model="latitude"
                type="number"
                step="0.000001"
                placeholder="25.0330"
                :class="{ 'border-red-500': errors.latitude }"
              />
              <p v-if="errors.latitude" class="text-sm text-red-600 mt-1">{{ errors.latitude }}</p>
            </div>

            <div>
              <Label class="text-sm font-medium">經度</Label>
              <Input
                v-model="longitude"
                type="number"
                step="0.000001"
                placeholder="121.5654"
                :class="{ 'border-red-500': errors.longitude }"
              />
              <p v-if="errors.longitude" class="text-sm text-red-600 mt-1">{{ errors.longitude }}</p>
            </div>
          </div>
        </div>

        <DialogFooter class="flex justify-end space-x-3 pt-6 border-t">
          <Button type="button" variant="outline" @click="$emit('close')">
            取消
          </Button>
          <Button type="submit" :disabled="isSubmitting">
            <span v-if="isSubmitting" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></span>
            {{ isSubmitting ? '更新中...' : '更新分站' }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { watch, nextTick } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import * as z from 'zod'
import {
  StationService,
  UpdateStationRequest,
  StationResponse
} from '@/services'
import { useToast } from '@/composables/useToast'
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

interface Props {
  isOpen: boolean
  station: StationResponse | null
}

interface Emits {
  (e: 'close'): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const toast = useToast()

// Validation schema
const stationSchema = toTypedSchema(z.object({
  name: z.string().min(1, '請輸入分站名稱'),
  enableNotify: z.boolean().optional(),
  lineToken: z.string().optional(),
  latitude: z.number().optional(),
  longitude: z.number().optional()
}).refine((data) => {
  // 如果啟用通知，則需要提供 Line Token
  if (data.enableNotify && !data.lineToken?.trim()) {
    return false
  }
  return true
}, {
  message: '啟用通知功能時必須提供 Line Token',
  path: ['lineToken']
}))

// Use VeeValidate form
const { handleSubmit, setValues, errors, isSubmitting, defineField } = useForm({
  validationSchema: stationSchema
})

// Define form fields
const [name] = defineField('name')
const [enableNotify] = defineField('enableNotify')
const [lineToken] = defineField('lineToken')
const [latitude] = defineField('latitude')
const [longitude] = defineField('longitude')

// Watch for station changes and populate form
watch(() => [props.station, props.isOpen], ([station, isOpen]) => {
  if (station && isOpen) {
    nextTick(() => {
      setValues({
        name: station.name || '',
        enableNotify: station.enableNotify || false,
        lineToken: station.lineToken || '',
        latitude: station.lat || null,
        longitude: station.lng || null
      })
    })
  }
}, { immediate: true })

const onSubmit = handleSubmit(async (values) => {
  if (!props.station) return

  try {
    const request = new UpdateStationRequest()
    request.name = values.name
    request.enableNotify = values.enableNotify || false
    request.lineToken = values.enableNotify ? values.lineToken : undefined
    request.lat = values.latitude ? Number(values.latitude) : undefined
    request.lng = values.longitude ? Number(values.longitude) : undefined

    await StationService.updateStation(props.station.id || 0, request)

    emit('close')
    toast.success('更新成功', '分站資訊已更新')
    emit('success')
  } catch (error) {
    console.error('更新分站失敗:', error)
    toast.error('更新失敗', '無法更新分站')
  }
})
</script>