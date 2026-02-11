<template>
  <Dialog :open="showFenceModal" @update:open="$emit('close')">
    <DialogContent class="max-w-2xl">
      <DialogHeader>
        <DialogTitle>電子圍籬事件詳情</DialogTitle>
      </DialogHeader>
      
      <div class="mt-4">
        <div v-if="selectedFenceEvent?.imageUrl" class="text-center relative">
          <!-- Loading state -->
          <div 
            v-if="imageLoading" 
            class="flex items-center justify-center bg-gray-100 rounded-lg min-h-[300px] w-full"
          >
            <div class="flex flex-col items-center">
              <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-2"></div>
              <p class="text-sm text-gray-600">載入圖片中...</p>
            </div>
          </div>
          
          <!-- Image -->
          <img 
            v-if="!imageError"
            :src="selectedFenceEvent.imageUrl" 
            alt="事件圖片" 
            class="max-w-full h-auto rounded-lg shadow-lg transition-opacity duration-300"
            :class="{ 'opacity-0': imageLoading, 'opacity-100': !imageLoading }"
            @load="handleImageLoad"
            @error="handleImageError"
          >
          
          <!-- Error state -->
          <div 
            v-if="imageError" 
            class="text-center py-12 text-gray-500 bg-gray-50 rounded-lg"
          >
            <ImageOff class="h-12 w-12 mx-auto mb-2 text-gray-400" />
            <p class="text-sm">圖片載入失敗</p>
          </div>
        </div>
        
        <!-- No image state -->
        <div 
          v-else-if="selectedFenceEvent && !selectedFenceEvent.imageUrl" 
          class="text-center py-12 text-gray-500 bg-gray-50 rounded-lg"
        >
          <ImageOff class="h-12 w-12 mx-auto mb-2 text-gray-400" />
          <p class="text-sm">此事件無圖片資料</p>
        </div>
        
        <div v-if="selectedFenceEvent" class="mt-4 space-y-2">
          <div class="flex justify-between">
            <span class="font-medium">設備名稱:</span>
            <span>{{ selectedFenceEvent.deviceName }}</span>
          </div>
          <div class="flex justify-between">
            <span class="font-medium">事件類型:</span>
            <span :class="selectedFenceEvent.eventType === 1 ? 'text-red-600' : 'text-blue-600'">
              {{ selectedFenceEvent.eventType === 1 ? '闖入事件' : '離開事件' }}
            </span>
          </div>
          <div class="flex justify-between">
            <span class="font-medium">發生時間:</span>
            <span>{{ selectedFenceEvent.date }} {{ selectedFenceEvent.time }}</span>
          </div>
        </div>
      </div>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { ImageOff } from 'lucide-vue-next'
import type { FenceEvent } from '@/composables/useHomeData'

interface Props {
  showFenceModal: boolean
  selectedFenceEvent: FenceEvent | null
}

const props = defineProps<Props>()

defineEmits<{
  close: []
}>()

// 圖片加載狀態
const imageLoading = ref(false)
const imageError = ref(false)

// 處理圖片加載完成
const handleImageLoad = () => {
  imageLoading.value = false
  imageError.value = false
}

// 處理圖片加載錯誤
const handleImageError = () => {
  imageLoading.value = false
  imageError.value = true
}

// 監聽選中的事件變化，重置圖片狀態
watch(() => props.selectedFenceEvent, (newEvent) => {
  if (newEvent?.imageUrl) {
    imageLoading.value = true
    imageError.value = false
  } else {
    imageLoading.value = false
    imageError.value = false
  }
}, { immediate: true })

// 監聽模態框開關狀態
watch(() => props.showFenceModal, (isOpen) => {
  if (isOpen && props.selectedFenceEvent?.imageUrl) {
    imageLoading.value = true
    imageError.value = false
  }
})
</script>