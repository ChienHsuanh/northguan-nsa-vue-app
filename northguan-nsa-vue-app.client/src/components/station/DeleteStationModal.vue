<template>
  <Dialog :open="isOpen" @update:open="$emit('close')">
    <DialogContent class="sm:max-w-[425px]">
      <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100">
        <TrashIcon class="h-6 w-6 text-red-600" />
      </div>
      <div>
        <h3 class="text-lg font-medium text-gray-900">確認刪除</h3>
        <p class="text-sm text-gray-500 mt-2">
          您確定要刪除分站「{{ station?.name }}」嗎？<br>
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
  StationService,
  StationResponse
} from '@/services'
import { useToast } from '@/composables/useToast'
import Button from '@/components/ui/button/Button.vue'
import {
  Dialog,
  DialogContent
} from '@/components/ui/dialog'
import { Trash as TrashIcon } from 'lucide-vue-next'

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

// State
const deleting = ref(false)

const handleDelete = async () => {
  if (!props.station) return

  try {
    deleting.value = true
    await StationService.deleteStation(props.station.id || 0)

    emit('close')
    toast.success('刪除成功', '分站已刪除')
    emit('success')
  } catch (error) {
    console.error('刪除分站失敗:', error)
    toast.error('刪除失敗', '無法刪除分站')
  } finally {
    deleting.value = false
  }
}
</script>