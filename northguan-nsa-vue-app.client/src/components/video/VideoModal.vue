<template>
  <Dialog v-model:open="isOpen">
    <DialogContent class="max-w-5xl w-full p-0 bg-slate-900 border-slate-700">
      <!-- Header -->
      <DialogHeader class="p-6 pb-4 border-b border-slate-700">
        <DialogTitle class="flex items-center justify-between text-white">
          <div class="flex items-center gap-3">
            <div class="relative">
              <div class="w-3 h-3 bg-red-500 rounded-full animate-pulse"></div>
              <div class="absolute inset-0 w-3 h-3 bg-red-500 rounded-full animate-ping opacity-75"></div>
            </div>
            <div class="flex flex-col">
              <span class="text-lg font-semibold">{{ location?.name }} - 即時直播</span>
              <span class="text-sm text-slate-400 font-normal">Live Stream</span>
            </div>
          </div>

          <div class="flex items-center gap-2">
            <!-- New Window Button -->
            <Button @click="openInNewWindow" variant="ghost" size="sm"
              class="h-8 px-2 text-slate-300 hover:text-white hover:bg-slate-700" :disabled="!hasStreamCapability">
              <ExternalLinkIcon class="w-4 h-4 mr-1" />
              <span class="hidden sm:inline">新視窗</span>
            </Button>
          </div>
        </DialogTitle>
      </DialogHeader>

      <!-- Video Container -->
      <div class="relative bg-black" ref="videoContainer">
        <div class="aspect-video relative overflow-hidden">
          <!-- GetAIJpeg Image Stream -->
          <CrowdImageStream v-if="device?.apiUrl && device.apiUrl.includes('GetAIJpeg') && !location?.videoUrl"
            :device-id="device.id?.toString() || ''" :device-type="device.type || 'crowd'" channel-id="cam1" :refresh-interval="200"
            class="w-full h-full" />

          <!-- YouTube Video -->
          <iframe v-else-if="location?.videoUrl && isYouTubeUrl(location.videoUrl)"
            :src="processYouTubeUrl(location.videoUrl)" class="w-full h-full" frameborder="0" allowfullscreen
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
            referrerpolicy="strict-origin-when-cross-origin"
            sandbox="allow-scripts allow-same-origin allow-presentation" />

          <!-- Regular Video -->
          <video v-else-if="location?.videoUrl && isVideoFile(location.videoUrl)" :src="location.videoUrl" class="w-full h-full object-cover" controls
            autoplay muted playsinline ref="videoElement">
            您的瀏覽器不支援影片播放
          </video>

          <!-- Generic iframe for other URLs -->
          <iframe v-else-if="location?.videoUrl"
            :src="location.videoUrl" class="w-full h-full" frameborder="0"
            referrerpolicy="strict-origin-when-cross-origin"
            sandbox="allow-scripts allow-same-origin allow-forms allow-presentation allow-popups allow-top-navigation-by-user-activation">
          </iframe>

          <!-- No Signal State -->
          <div v-else
            class="w-full h-full flex items-center justify-center bg-gradient-to-br from-slate-800 to-slate-900">
            <div class="text-center text-slate-400 p-8">
              <div class="relative mb-6">
                <VideoOffIcon class="h-16 w-16 mx-auto text-slate-500" />
                <div class="absolute -top-1 -right-1">
                  <AlertTriangleIcon class="h-6 w-6 text-amber-500" />
                </div>
              </div>
              <h3 class="text-xl font-semibold text-slate-300 mb-2">暫無影像訊號</h3>
              <p class="text-slate-500 mb-4">請檢查設備連線狀態</p>
              <div class="flex items-center justify-center gap-2 text-sm text-slate-600">
                <WifiOffIcon class="h-4 w-4" />
                <span>設備離線或訊號中斷</span>
              </div>
            </div>
          </div>

          <!-- Loading Overlay -->
          <div v-if="isLoading" class="absolute inset-0 bg-black/50 flex items-center justify-center">
            <div class="text-center text-white">
              <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto mb-4"></div>
              <p class="text-sm">載入中...</p>
            </div>
          </div>
        </div>

        <!-- Video Controls Overlay (for custom video) -->
        <div v-if="location?.videoUrl && !isYouTubeUrl(location.videoUrl) && isVideoFile(location.videoUrl)" class="absolute bottom-4 left-4 right-4">
          <div class="bg-black/70 backdrop-blur-sm rounded-lg p-3">
            <div class="flex items-center justify-between text-white text-sm">
              <div class="flex items-center gap-3">
                <Button @click="togglePlay" variant="ghost" size="sm" class="h-8 w-8 p-0 hover:bg-white/20">
                  <PlayIcon v-if="!isPlaying" class="h-4 w-4" />
                  <PauseIcon v-else class="h-4 w-4" />
                </Button>
                <span class="text-xs">{{ formatTime(currentTime) }} / {{ formatTime(duration) }}</span>
              </div>

              <div class="flex items-center gap-2">
                <Button @click="toggleMute" variant="ghost" size="sm" class="h-8 w-8 p-0 hover:bg-white/20">
                  <VolumeXIcon v-if="isMuted" class="h-4 w-4" />
                  <Volume2Icon v-else class="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer Info -->
      <div class="p-4 bg-slate-800 border-t border-slate-700">
        <div class="flex items-center justify-between text-sm">
          <div class="flex items-center gap-4 text-slate-400">
            <div class="flex items-center gap-2">
              <MapPinIcon class="h-4 w-4" />
              <span>{{ location?.name }}</span>
            </div>
            <div class="flex items-center gap-2">
              <ClockIcon class="h-4 w-4" />
              <span>{{ currentDateTime }}</span>
            </div>
          </div>

          <div class="flex items-center gap-2">
            <div class="flex items-center gap-1 text-green-400">
              <div class="w-2 h-2 bg-green-400 rounded-full"></div>
              <span class="text-xs">在線</span>
            </div>
          </div>
        </div>
      </div>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import CrowdImageStream from '@/components/device/CrowdImageStream.vue'

// Icons
import {
  ExternalLink as ExternalLinkIcon,
  VideoOff as VideoOffIcon,
  AlertTriangle as AlertTriangleIcon,
  WifiOff as WifiOffIcon,
  MapPin as MapPinIcon,
  Clock as ClockIcon,
  Play as PlayIcon,
  Pause as PauseIcon,
  VolumeX as VolumeXIcon,
  Volume2 as Volume2Icon
} from 'lucide-vue-next'

// Utils
import { isYouTubeUrl, processYouTubeUrl, isVideoFile } from '@/utils/statusUtils'

// Props
interface Location {
  name: string
  url: string
  videoUrl?: string
}

interface Device {
  id?: number | string
  name: string
  type?: string
  apiUrl?: string
  videoUrl?: string
}

interface Props {
  location: Location | null
  device?: Device | null
  isOpen: boolean
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  'update:isOpen': [value: boolean]
  'openInNewWindow': [location: Location]
}>()

// Reactive state
const isLoading = ref(false)
const videoElement = ref<HTMLVideoElement | null>(null)
const currentTime = ref(0)
const duration = ref(0)
const isPlaying = ref(false)
const isMuted = ref(true)
const currentDateTime = ref('')

// Computed
const isOpen = computed({
  get: () => props.isOpen,
  set: (value) => emit('update:isOpen', value)
})

const hasStreamCapability = computed(() => {
  return props.location?.videoUrl || (props.device?.apiUrl && props.device.apiUrl.includes('GetAIJpeg'))
})

// Methods
const openInNewWindow = () => {
  // Handle GetAIJpeg devices
  if (props.device?.apiUrl && props.device.apiUrl.includes('GetAIJpeg')) {
    const params = new URLSearchParams({
      deviceId: props.device.id?.toString() || '',
      deviceType: props.device.type || 'crowd',
      deviceName: props.device.name || props.location?.name || '',
      channelId: 'cam1'
    })

    const streamUrl = `/crowd-stream?${params.toString()}`
    window.open(streamUrl, '_blank', 'width=1200,height=800,resizable=yes,scrollbars=yes')
  } else if (props.location) {
    // Handle traditional video streams
    emit('openInNewWindow', props.location)
  }
}


const togglePlay = () => {
  if (!videoElement.value) return

  if (videoElement.value.paused) {
    videoElement.value.play()
  } else {
    videoElement.value.pause()
  }
}

const toggleMute = () => {
  if (!videoElement.value) return

  videoElement.value.muted = !videoElement.value.muted
  isMuted.value = videoElement.value.muted
}

const formatTime = (seconds: number): string => {
  const mins = Math.floor(seconds / 60)
  const secs = Math.floor(seconds % 60)
  return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
}

const updateDateTime = () => {
  const now = new Date()
  currentDateTime.value = now.toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false
  })
}

// Video event handlers
const handleVideoEvents = () => {
  if (!videoElement.value) return

  const video = videoElement.value

  // 影片開始載入時
  video.addEventListener('loadstart', () => {
    isLoading.value = true
  })

  // 影片載入足夠數據，可以開始播放時
  video.addEventListener('canplay', () => {
    isLoading.value = false
  })

  // 影片播放中但需要等待更多數據時（緩衝中）
  video.addEventListener('waiting', () => {
    // 只有在影片沒有暫停的情況下才顯示載入
    if (!video.paused) {
      isLoading.value = true
    }
  })

  // 影片開始或恢復播放時（不再等待）
  video.addEventListener('playing', () => {
    isLoading.value = false
  })

  // 取得影片的基本資料，如持續時間
  video.addEventListener('loadeddata', () => {
    duration.value = video.duration
  })

  video.addEventListener('timeupdate', () => {
    currentTime.value = video.currentTime
  })

  video.addEventListener('play', () => {
    isPlaying.value = true
  })

  video.addEventListener('pause', () => {
    isPlaying.value = false
    // 當影片暫停時，移除載入狀態
    isLoading.value = false
  })

  video.addEventListener('volumechange', () => {
    isMuted.value = video.muted
  })
}

// Watchers
watch(() => props.isOpen, (newValue) => {
  if (newValue) {
    // 當對話框開啟且有普通影片 URL 時，預設開始載入狀態
    if (props.location?.videoUrl && !isYouTubeUrl(props.location.videoUrl) && isVideoFile(props.location.videoUrl)) {
      // 確保在 videoElement 被設定前，載入狀態為 true
      isLoading.value = true
    }
  } else {
    // 對話框關閉時重置所有相關狀態
    isLoading.value = false
    isPlaying.value = false
  }
})

watch(() => videoElement.value, (newElement) => {
  if (newElement) {
    handleVideoEvents()
  }
})

// Lifecycle
let dateTimeInterval: NodeJS.Timeout

onMounted(() => {
  updateDateTime()
  dateTimeInterval = setInterval(updateDateTime, 1000)
})

onUnmounted(() => {
  if (dateTimeInterval) {
    clearInterval(dateTimeInterval)
  }
})
</script>