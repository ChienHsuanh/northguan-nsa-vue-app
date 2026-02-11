<template>
  <Dialog :open="isOpen" @update:open="(value) => !value && $emit('close')">
    <DialogContent class="sm:max-w-[425px]">
      <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100">
        <TrashIcon class="h-6 w-6 text-red-600" />
      </div>
      <div>
        <h3 class="text-lg font-medium text-gray-900">確認刪除</h3>
        <p class="text-sm text-gray-500 mt-2">
          您確定要刪除用戶「{{ account?.name }}」嗎？<br>
          此操作無法復原。
        </p>
      </div>
      <div class="flex justify-center space-x-3">
        <Button variant="outline" @click="$emit('close')">
          取消
        </Button>
        <Button variant="destructive" @click="handleDelete" :disabled="isDeleting">
          <span v-if="isDeleting" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></span>
          {{ isDeleting ? '刪除中...' : '確認刪除' }}
        </Button>
      </div>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { UserResponse, AccountService } from '@/services'
import { handleApiError, ERROR_CODES, type ApiErrorResponse } from '@/utils/errorHandler'
import { useToast } from '@/composables/useToast'
import Button from '@/components/ui/button/Button.vue'
import {
  Dialog,
  DialogContent,
} from '@/components/ui/dialog'
import { Trash as TrashIcon } from 'lucide-vue-next'

interface Props {
  isOpen: boolean
  account?: UserResponse | null
}

interface Emits {
  (e: 'close'): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()
const toast = useToast()

const isDeleting = ref(false)

const handleDelete = async () => {
  if (!props.account?.id) return

  try {
    isDeleting.value = true
    await AccountService.deleteAccount(props.account.id.toString())
    toast.success('帳戶刪除成功')
    emit('success')
    emit('close')
  } catch (error) {
    console.error('刪除帳戶失敗:', error)
    const apiError: ApiErrorResponse = await handleApiError(error)

    // 根據錯誤類型顯示不同的錯誤訊息
    if (apiError.errorCode === ERROR_CODES.RESOURCE_NOT_FOUND) {
      toast.error('找不到指定的帳戶')
    } else if (apiError.errorCode === ERROR_CODES.RESOURCE_IN_USE) {
      toast.error('該帳戶正在使用中，無法刪除')
    } else if (apiError.errorCode === ERROR_CODES.OPERATION_NOT_ALLOWED) {
      toast.error('沒有權限刪除此帳戶')
    } else {
      toast.error(`刪除帳戶失敗: ${apiError.message}`)
    }
  } finally {
    isDeleting.value = false
  }
}
</script>