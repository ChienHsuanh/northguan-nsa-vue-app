<template>
  <div class="min-h-screen bg-black text-white flex flex-col">
    <!-- Header -->
    <div class="bg-gray-900 p-4 border-b border-gray-700">
      <div class="container mx-auto flex items-center justify-between">
        <h1 class="text-xl font-bold">{{ deviceName }}</h1>
        <div class="flex items-center gap-4">
          <div class="flex items-center gap-2">
            <div class="w-3 h-3 rounded-full animate-pulse" 
                 :class="isStreaming ? 'bg-green-500' : 'bg-red-500'"></div>
            <span class="text-sm">{{ isStreaming ? 'LIVE' : '離線' }}</span>
          </div>
          <button @click="closeWindow" 
                  class="px-3 py-1 bg-gray-700 hover:bg-gray-600 rounded text-sm">
            關閉
          </button>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="flex-1 flex items-center justify-center p-4">
      <div class="w-full max-w-6xl">
        <!-- Image Container -->
        <div class="bg-gray-900 rounded-lg overflow-hidden">
          <img v-if="imageUrl" 
               :src="imageUrl"
               @load="onImageLoad"
               @error="onImageError"
               class="w-full h-auto max-h-[80vh] object-contain"
               :alt="`人流設備 ${deviceId} 即時影像`"
               draggable="false" />
          
          <!-- Loading State -->
          <div v-if="isLoading" class="flex items-center justify-center h-96">
            <div class="text-center">
              <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-green-500 mx-auto mb-4"></div>
              <p class="text-gray-300">載入影像中...</p>
            </div>
          </div>
          
          <!-- Error State -->
          <div v-if="hasError && !isLoading" class="flex items-center justify-center h-96">
            <div class="text-center text-gray-400">
              <svg class="h-16 w-16 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                      d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
              </svg>
              <p class="text-lg mb-2">影像載入失敗</p>
              <p class="text-sm">正在重試連接...</p>
            </div>
          </div>
        </div>

        <!-- Controls -->
        <div class="mt-4 flex items-center justify-between bg-gray-900 rounded-lg p-4">
          <div class="flex items-center gap-4">
            <span class="text-sm text-gray-400">更新間隔：</span>
            <select v-model="refreshInterval" @change="restartStream" 
                    class="bg-gray-800 text-white px-3 py-1 rounded text-sm">
              <option :value="30">30ms (快)</option>
              <option :value="100">100ms (中)</option>
              <option :value="200">200ms (慢)</option>
              <option :value="500">500ms (很慢)</option>
            </select>
          </div>
          
          <div class="flex items-center gap-4 text-sm text-gray-400">
            <span>設備ID: {{ deviceId }}</span>
            <span v-if="lastUpdateTime">最後更新: {{ lastUpdateTime }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

// Get URL parameters
const urlParams = new URLSearchParams(window.location.search)
const deviceId = urlParams.get('deviceId') || ''
const deviceType = urlParams.get('deviceType') || 'crowd'
const deviceName = urlParams.get('deviceName') || `人流設備 ${deviceId}`
const channelId = urlParams.get('channelId') || 'cam1'

// Reactive state
const imageUrl = ref<string>('')
const isLoading = ref(false)
const hasError = ref(false)
const isStreaming = ref(false)
const refreshInterval = ref(200)
const lastUpdateTime = ref<string>('')
const intervalId = ref<number | null>(null)
const viewCount = ref(0)
const inInterval = ref(false)

// Reload image with cache-busting timestamp
const reloadImage = () => {
  if (inInterval.value && viewCount.value < 10) {
    viewCount.value++
    return
  }
  
  viewCount.value = 0
  inInterval.value = true
  
  const timestamp = new Date().getTime()
  
  // Use backend API to securely get device image by device ID and type
  const secureImageUrl = `/api/devices/${deviceId}/stream?type=${deviceType}&channelid=${channelId}&t=${timestamp}`
  
  // Only show loading on first load
  if (!imageUrl.value) {
    isLoading.value = true
  }
  hasError.value = false
  imageUrl.value = secureImageUrl
}

// Handle successful image load
const onImageLoad = () => {
  inInterval.value = false
  isLoading.value = false
  hasError.value = false
  isStreaming.value = true
  lastUpdateTime.value = new Date().toLocaleTimeString()
}

// Handle image load error
const onImageError = () => {
  inInterval.value = false
  isLoading.value = false
  hasError.value = true
  isStreaming.value = false
}

// Start streaming
const startStream = () => {
  if (deviceId) {
    reloadImage()
    intervalId.value = window.setInterval(reloadImage, refreshInterval.value)
  }
}

// Stop streaming
const stopStream = () => {
  if (intervalId.value) {
    clearInterval(intervalId.value)
    intervalId.value = null
  }
  isStreaming.value = false
}

// Restart streaming with new interval
const restartStream = () => {
  stopStream()
  startStream()
}

// Close window function
const closeWindow = () => {
  stopStream()
  window.close()
}

// Lifecycle hooks
onMounted(() => {
  if (deviceId) {
    startStream()
  } else {
    hasError.value = true
    console.error('No device ID provided')
  }
})

onUnmounted(() => {
  stopStream()
})

// Handle window beforeunload
window.addEventListener('beforeunload', () => {
  stopStream()
})
</script>