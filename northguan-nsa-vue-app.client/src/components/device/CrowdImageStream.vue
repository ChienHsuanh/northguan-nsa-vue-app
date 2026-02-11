<template>
  <div ref="containerRef" class="aspect-video bg-black rounded-b relative">
    <img v-if="imageUrl" :src="imageUrl" @load="onImageLoad" @error="onImageError"
      class="w-full h-full object-cover rounded-b" :alt="`人流設備 ${deviceId} 即時影像`" draggable="false" />

    <!-- Loading indicator - only show when no image is loaded yet -->
    <div v-if="isLoading && !imageUrl" class="absolute inset-0 flex items-center justify-center bg-black bg-opacity-50">
      <div class="text-center text-white">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-white mx-auto mb-2"></div>
        <p class="text-sm">載入影像中...</p>
      </div>
    </div>

    <!-- Error state - only show when no image is loaded yet -->
    <div v-if="hasError && !isLoading && !imageUrl"
      class="absolute inset-0 flex items-center justify-center bg-gray-800">
      <div class="text-center text-gray-400">
        <CameraIcon class="h-8 w-8 mx-auto mb-2" />
        <p class="text-sm">影像載入失敗</p>
        <p class="text-xs mt-1">正在重試連接...</p>
      </div>
    </div>

    <!-- Status indicator -->
    <div class="absolute top-2 left-2 flex items-center gap-2 bg-black bg-opacity-50 rounded px-2 py-1">
      <div class="w-2 h-2 rounded-full" :class="isStreaming ? 'bg-green-500 animate-pulse' : 'bg-red-500'"></div>
      <span class="text-white text-xs">{{ isStreaming ? 'LIVE' : '離線' }}</span>
    </div>

    <!-- Fullscreen button -->
    <div class="absolute top-2 right-2">
      <button @click="toggleFullscreen"
        class="bg-black bg-opacity-50 hover:bg-opacity-75 text-white p-2 rounded transition-all duration-200"
        title="全螢幕">
        <ExpandIcon v-if="!isFullscreen" class="w-4 h-4" />
        <MinimizeIcon v-else class="w-4 h-4" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { Camera as CameraIcon, Expand as ExpandIcon, Minimize as MinimizeIcon } from 'lucide-vue-next'

interface Props {
  deviceId: string | number
  deviceType?: string
  channelId?: string
  refreshInterval?: number
}

const props = withDefaults(defineProps<Props>(), {
  deviceType: 'crowd',
  channelId: 'cam1',
  refreshInterval: 200 // 200ms for better performance and visual experience
})

// Reactive state
const imageUrl = ref<string>('')
const isLoading = ref(false)
const hasError = ref(false)
const isStreaming = ref(false)
const intervalId = ref<number | null>(null)
const viewCount = ref(0)
const inInterval = ref(false)
const isFullscreen = ref(false)
const containerRef = ref<HTMLElement | null>(null)

// Reload image with cache-busting timestamp
const reloadImage = () => {
  if (inInterval.value && viewCount.value < 10) {
    viewCount.value++
    return
  }

  viewCount.value = 0
  inInterval.value = true

  const timestamp = new Date().getTime()

  // Use secure backend API endpoint
  const secureImageUrl = `/api/devices/${props.deviceId}/stream?type=${props.deviceType}&channelid=${props.channelId}&t=${timestamp}`

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
}

// Handle image load error
const onImageError = () => {
  inInterval.value = false
  isLoading.value = false
  hasError.value = true
  isStreaming.value = false
}

// Start streaming
const startStreaming = () => {
  if (props.deviceId) {
    reloadImage()
    intervalId.value = window.setInterval(reloadImage, props.refreshInterval)
  }
}

// Stop streaming
const stopStreaming = () => {
  if (intervalId.value) {
    clearInterval(intervalId.value)
    intervalId.value = null
  }
  isStreaming.value = false
}

// Fullscreen functionality
const toggleFullscreen = async () => {
  if (!containerRef.value) return

  try {
    if (!isFullscreen.value) {
      // Enter fullscreen
      if (containerRef.value.requestFullscreen) {
        await containerRef.value.requestFullscreen()
      } else if ((containerRef.value as any).webkitRequestFullscreen) {
        await (containerRef.value as any).webkitRequestFullscreen()
      } else if ((containerRef.value as any).msRequestFullscreen) {
        await (containerRef.value as any).msRequestFullscreen()
      }
    } else {
      // Exit fullscreen
      if (document.exitFullscreen) {
        await document.exitFullscreen()
      } else if ((document as any).webkitExitFullscreen) {
        await (document as any).webkitExitFullscreen()
      } else if ((document as any).msExitFullscreen) {
        await (document as any).msExitFullscreen()
      }
    }
  } catch (error) {
    console.error('Fullscreen toggle failed:', error)
  }
}

// Handle fullscreen change events
const handleFullscreenChange = () => {
  const fullscreenElement = document.fullscreenElement ||
    (document as any).webkitFullscreenElement ||
    (document as any).msFullscreenElement

  isFullscreen.value = fullscreenElement === containerRef.value
}

// Watch for deviceId changes
watch(() => props.deviceId, (newDeviceId) => {
  stopStreaming()
  if (newDeviceId) {
    startStreaming()
  }
}, { immediate: true })

// Lifecycle hooks
onMounted(() => {
  if (props.deviceId) {
    startStreaming()
  }

  // Add fullscreen event listeners
  document.addEventListener('fullscreenchange', handleFullscreenChange)
  document.addEventListener('webkitfullscreenchange', handleFullscreenChange)
  document.addEventListener('msfullscreenchange', handleFullscreenChange)
})

onUnmounted(() => {
  stopStreaming()

  // Remove fullscreen event listeners
  document.removeEventListener('fullscreenchange', handleFullscreenChange)
  document.removeEventListener('webkitfullscreenchange', handleFullscreenChange)
  document.removeEventListener('msfullscreenchange', handleFullscreenChange)
})
</script>