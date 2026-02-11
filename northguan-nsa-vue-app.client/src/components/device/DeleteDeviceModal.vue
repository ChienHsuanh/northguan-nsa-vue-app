<template>
  <Dialog :open="isOpen" @update:open="$emit('close')">
    <DialogContent class="sm:max-w-[425px]">
      <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100">
        <TrashIcon class="h-6 w-6 text-red-600" />
      </div>
      <div>
        <h3 class="text-lg font-medium text-gray-900">確認刪除</h3>
        <p class="text-sm text-gray-500 mt-2">
          您確定要刪除設備「{{ device?.name }}」嗎？<br>
          此操作無法復原。
        </p>
      </div>
      <div class="flex justify-center space-x-3">
        <Button variant="outline" @click="$emit('close')">
          取消
        </Button>
        <Button variant="destructive" @click="handleDelete" :disabled="deleting">
          <span v-if="deleting" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></span>
          {{ deleting ? '刪除中...' : '確認刪除' }}
        </Button>
      </div>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import {
  DeviceService,
  DeviceListResponse
} from '@/services'
import { useToast } from '@/composables/useToast'
import { handleApiError, ERROR_CODES, type ApiErrorResponse } from '@/utils/errorHandler'
import Button from '@/components/ui/button/Button.vue'
import {
  Dialog,
  DialogContent
} from '@/components/ui/dialog'
import { Trash as TrashIcon } from 'lucide-vue-next'

interface Props {
  isOpen: boolean
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
const deleting = ref(false)

const handleDelete = async () => {
  if (!props.device) return

  try {
    deleting.value = true
    await DeviceService.deleteDevice(props.device.id || 0, props.device.type)

    emit('close')
    toast.success('刪除成功', '設備已刪除')
    emit('success')
  } catch (error) {
    console.error('刪除設備失敗:', error)
    const apiError: ApiErrorResponse = await handleApiError(error)

    // 根據錯誤類型顯示不同的錯誤訊息
    if (apiError.errorCode === ERROR_CODES.RESOURCE_NOT_FOUND) {
      toast.error('刪除失敗', '找不到指定的設備')
    } else if (apiError.errorCode === ERROR_CODES.RESOURCE_IN_USE) {
      toast.error('刪除失敗', '該設備正在使用中，無法刪除')
    } else if (apiError.errorCode === ERROR_CODES.OPERATION_NOT_ALLOWED) {
      toast.error('刪除失敗', '沒有權限刪除此設備')
    } else {
      toast.error('刪除失敗', `無法刪除設備: ${apiError.message}`)
    }
  } finally {
    deleting.value = false
  }
}
</script>