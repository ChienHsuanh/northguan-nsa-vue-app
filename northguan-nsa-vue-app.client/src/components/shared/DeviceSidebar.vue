<template>
  <!-- Modern Sidebar -->
  <div v-if="showSidebar" class="absolute top-0 right-0 w-full md:w-[45%] h-full bg-white shadow-2xl z-[1001] flex flex-col"
    style="backdrop-filter: blur(10px); background: rgba(255, 255, 255, 0.95);">
    <!-- Sidebar Header -->
    <div class="flex-shrink-0 text-white p-6"
      :style="{ backgroundColor: colors[sidebarType as keyof typeof colors] || '#6c757d' }">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center">
            <component :is="getSidebarIcon(sidebarType)" class="w-5 h-5 text-white" />
          </div>
          <div>
            <h3 class="font-bold text-lg text-white mb-1">{{ sidebarTitle }}</h3>
            <p class="text-white/80 text-sm mt-1">{{ sidebarData.name }}</p>
          </div>
        </div>
        <Button variant="ghost" size="sm" @click="closeSidebar"
          class="text-white hover:bg-white/20 rounded-full w-8 h-8 p-0" title="關閉">
          <XIcon class="h-4 w-4" />
        </Button>
      </div>

      <!-- Device Status Indicator -->
      <div class="mt-4 flex items-center gap-2">
        <div class="w-3 h-3 rounded-full" :class="getStatusIndicatorClass(sidebarData.status)"></div>
        <span class="text-sm text-white/80 py-1">
          {{ getStatusText(sidebarData.status) }}
        </span>
        <span class="text-xs text-white/60 ml-auto py-1">
          ID: {{ selectedDeviceId }}
        </span>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="sidebarLoading" class="flex-1 flex items-center justify-center">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
        <p class="text-gray-600">載入設備資料中...</p>
      </div>
    </div>

    <!-- Sidebar Content -->
    <div v-else class="flex-1 overflow-y-auto bg-white"
      style="scrollbar-width: thin; scrollbar-color: #cbd5e1 transparent;">
      <div class="p-6 space-y-6">

        <!-- Parking Sidebar -->
        <div v-if="sidebarType === 'parking' && 'currentParked' in sidebarData">
          <!-- Usage Overview -->
          <div class="mb-6">
            <div class="flex items-center justify-between mb-2">
              <h4 class="font-semibold text-gray-800">使用率</h4>
              <span class="text-xl font-bold text-blue-600">{{ sidebarData?.currentParked !== undefined &&
                sidebarData?.numberOfParking !== undefined && sidebarData.currentParked > 0
                ? Math.round((sidebarData.currentParked / sidebarData.numberOfParking) * 100) : 0 }}%</span>
            </div>

            <!-- Progress Bar -->
            <div class="w-full bg-gray-200 rounded-full h-2 mb-2">
              <div class="bg-blue-600 h-2 rounded-full transition-all duration-500" :style="{
                width: `${sidebarData?.currentParked !== undefined &&
                  sidebarData?.numberOfParking !== undefined && sidebarData.currentParked > 0
                  ? Math.round((sidebarData.currentParked / sidebarData.numberOfParking) * 100) : 0}%`
              }"></div>
            </div>

            <!-- Stats -->
            <div class="grid grid-cols-3 gap-2 text-center text-sm">
              <div class="space-y-1">
                <div class="font-bold text-red-600">{{ sidebarData.currentParked ?? 0 }}</div>
                <div class="text-xs text-gray-500">已使用</div>
              </div>
              <div class="space-y-1">
                <div class="font-bold text-green-600">{{ Math.max((sidebarData.numberOfParking ?? 0) -
                  (sidebarData.currentParked ?? 0), 0) }}</div>
                <div class="text-xs text-gray-500">可用</div>
              </div>
              <div class="space-y-1">
                <div class="font-bold text-blue-600">{{ sidebarData.numberOfParking ?? 0 }}</div>
                <div class="text-xs text-gray-500">總計</div>
              </div>
            </div>
          </div>

          <!-- Chart -->
          <div class="mb-3" v-if="chartOptions && chartOptions.parking">
            <EChartsWrapper :option="chartOptions.parking" :loading="false" height="180px" />
          </div>
        </div>

        <!-- Traffic Sidebar -->
        <div v-if="sidebarType === 'traffic' && 'currentAverageSpeed' in sidebarData">
          <!-- Current Status -->
          <div class="mb-6">
            <div class="flex items-center justify-between mb-4">
              <h4 class="font-semibold text-gray-800">交通狀況</h4>
              <span class="text-xl font-bold text-orange-600">{{ sidebarData.currentAverageSpeed ?? 0 }} km/h</span>
            </div>

            <!-- Speed vs Limit -->
            <div v-if="sidebarData.speedLimit && sidebarData.speedLimit > 0" class="mb-4">
              <div class="flex justify-between text-sm mb-2">
                <span>速度 / 限速</span>
                <span>{{ sidebarData.currentAverageSpeed ?? 0 }} / {{ sidebarData.speedLimit }} km/h</span>
              </div>
              <div class="w-full bg-gray-200 rounded-full h-2">
                <div class="h-2 rounded-full transition-all duration-500"
                  :class="getSpeedBarClass(sidebarData.currentAverageSpeed ?? 0, sidebarData.speedLimit)"
                  :style="{ width: `${Math.max(Math.min((sidebarData.currentAverageSpeed ?? 0) / sidebarData.speedLimit * 100, 100), 0)}%` }">
                </div>
              </div>
            </div>

            <!-- Stats -->
            <div class="grid grid-cols-2 gap-2 text-center text-sm">
              <div class="space-y-1">
                <div class="font-bold text-blue-600">{{ sidebarData.currentVehicleCount ?? 0 }}</div>
                <div class="text-xs text-gray-500">車輛數</div>
              </div>
              <div class="space-y-1">
                <div class="font-bold text-purple-600">{{ sidebarData.speedLimit ?? 0 }}</div>
                <div class="text-xs text-gray-500">速限</div>
              </div>
            </div>
          </div>

          <!-- Device Info -->
          <div v-if="sidebarData.city || sidebarData.eTagNumber" class="mb-3 pb-3 border-b border-gray-200">
            <h5 class="font-semibold text-gray-800 mb-2">設備資訊</h5>
            <div class="text-sm space-y-1">
              <div v-if="sidebarData.stationName" class="text-gray-600 py-1">{{ sidebarData.stationName }}</div>
              <div v-if="sidebarData.city" class="flex justify-between py-1">
                <span>城市</span>
                <span class="font-medium">{{ sidebarData.city }}</span>
              </div>
              <div v-if="sidebarData.eTagNumber" class="flex justify-between py-1">
                <span>ETag編號</span>
                <span class="font-mono text-xs">{{ sidebarData.eTagNumber }}</span>
              </div>
            </div>
          </div>

          <!-- Chart -->
          <div class="mb-3" v-if="chartOptions && chartOptions.traffic">
            <EChartsWrapper :option="chartOptions.traffic" :loading="false" height="180px" />
          </div>
        </div>

        <!-- Crowd Sidebar -->
        <div
          v-if="sidebarType === 'crowd' && 'crowdDensity' in sidebarData && 'currentPeopleCount' in sidebarData && 'totalIn' in sidebarData">
          <!-- Video Stream / Image Stream -->
          <div
            v-if="('videoUrl' in sidebarData && sidebarData.videoUrl) || ('apiUrl' in sidebarData && sidebarData.apiUrl && sidebarData.apiUrl.includes('GetAIJpeg'))"
            class="mb-6">
            <div class="bg-gray-800 p-2 flex items-center justify-between rounded-t">
              <div class="flex items-center gap-2">
                <div class="w-2 h-2 bg-red-500 rounded-full animate-pulse"></div>
                <span class="text-white text-sm font-medium">
                  {{ ('apiUrl' in sidebarData && sidebarData.apiUrl && sidebarData.apiUrl.includes('GetAIJpeg') &&
                    !('videoUrl' in sidebarData && sidebarData.videoUrl)) ? '即時影像串流' : '即時影像' }}
                </span>
              </div>
              <Button @click="openVideoInNewWindow" variant="ghost" size="sm"
                class="text-white hover:bg-gray-700 h-6 px-2">
                <ExternalLinkIcon class="w-3 h-3 mr-1" />
                {{ ('apiUrl' in sidebarData && sidebarData.apiUrl && sidebarData.apiUrl.includes('GetAIJpeg') &&
                  !('videoUrl' in sidebarData && sidebarData.videoUrl)) ? '新視窗' : '全螢幕' }}
              </Button>
            </div>

            <!-- Image Stream for devices with GetAIJpeg API -->
            <CrowdImageStream
              v-if="'apiUrl' in sidebarData && sidebarData.apiUrl && sidebarData.apiUrl.includes('GetAIJpeg') && !('videoUrl' in sidebarData && sidebarData.videoUrl)"
              :api-url="sidebarData.apiUrl" :device-id="selectedDeviceId" channel-id="cam1" :refresh-interval="30" />

            <!-- Traditional Video Stream -->
            <div v-else-if="'videoUrl' in sidebarData && sidebarData.videoUrl" class="aspect-video bg-black rounded-b">
              <!-- YouTube Video -->
              <iframe v-if="isYouTubeUrl(sidebarData.videoUrl)"
                :src="processYouTubeUrl(sidebarData.videoUrl)" class="w-full h-full rounded-b"
                frameborder="0" allowfullscreen
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                referrerpolicy="strict-origin-when-cross-origin"
                sandbox="allow-scripts allow-same-origin allow-presentation" />

              <!-- Regular Video -->
              <video v-else-if="isVideoFile(sidebarData.videoUrl)" :src="sidebarData.videoUrl" class="w-full h-full object-cover rounded-b" controls
                autoplay muted playsinline>
                您的瀏覽器不支援影片播放
              </video>

              <!-- Generic iframe for other URLs -->
              <iframe v-else
                :src="sidebarData.videoUrl" class="w-full h-full rounded-b" frameborder="0"
                referrerpolicy="strict-origin-when-cross-origin"
                sandbox="allow-scripts allow-same-origin allow-forms allow-presentation allow-popups allow-top-navigation-by-user-activation">
              </iframe>
            </div>
          </div>

          <!-- Current Status -->
          <div class="mb-6">
            <div class="flex items-center justify-between mb-2">
              <h4 class="font-semibold text-gray-800">人流狀況</h4>
              <span class="text-xl font-bold text-green-600">{{ (sidebarData.currentPeopleCount ?? 0) }} 人</span>
            </div>

            <!-- Crowd Density Status -->
            <div class="mb-4 p-3 rounded-lg border" :class="getCrowdStatus((sidebarData.crowdDensity ?? 0)).bgColor">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-2">
                  <div class="w-3 h-3 rounded-full"
                    :class="getCrowdStatus((sidebarData.crowdDensity ?? 0)).level === 'smooth' ? 'bg-green-500' : getCrowdStatus((sidebarData.crowdDensity ?? 0)).level === 'moderate' ? 'bg-yellow-500' : 'bg-red-500'">
                  </div>
                  <span class="font-medium text-sm" :class="getCrowdStatus((sidebarData.crowdDensity ?? 0)).color">
                    {{ getCrowdStatus((sidebarData.crowdDensity ?? 0)).text }}
                  </span>
                </div>
                <span class="text-xs text-gray-600">
                  {{ (sidebarData.crowdDensity) && (sidebarData.crowdDensity ??
                    sidebarData.CrowdDensity) > 99999 ? '∞' : ((sidebarData.crowdDensity ??
                      0)).toFixed(2) }} 人/m²
                </span>
              </div>
              <p class="text-xs text-gray-600 mt-1">
                {{ getCrowdStatus((sidebarData.crowdDensity ?? 0)).description }}
              </p>
            </div>

            <!-- Stats -->
            <div class="grid grid-cols-2 gap-2 text-center text-sm">
              <div class="space-y-1">
                <div class="font-bold text-blue-600">
                  {{ (sidebarData.crowdDensity) && (sidebarData.crowdDensity) > 99999 ? '∞' : ((sidebarData.crowdDensity
                    ?? 0)).toFixed(2) }}
                </div>
                <div class="text-xs text-gray-500">密度</div>
              </div>
              <div class="space-y-1">
                <div class="font-bold text-purple-600">{{ (sidebarData.totalIn ?? 0) }}</div>
                <div class="text-xs text-gray-500">累計入園</div>
              </div>
            </div>
          </div>

          <!-- Chart -->
          <div class="mb-3" v-if="chartOptions && chartOptions.crowd">
            <EChartsWrapper :option="chartOptions.crowd" :loading="false" height="180px" />
          </div>
        </div>

        <!-- Fence Sidebar -->
        <div v-if="sidebarType === 'fence'">
          <!-- Event Image -->
          <div class="mb-6">
            <div class="bg-blue-600 p-2 flex items-center gap-2 rounded-t">
              <ShieldIcon class="w-4 h-4 text-white" />
              <span class="text-white text-sm font-medium">事件圖片</span>
            </div>
            <div class="aspect-video bg-black rounded-b">
              <img :src="fenceImageUrl" alt="圍籬事件圖片" class="w-full h-full object-cover rounded-b">
            </div>
          </div>

          <!-- Events List -->
          <div v-if="'events' in sidebarData && sidebarData.events && sidebarData.events.length > 0"
            class="mb-6 pb-3 border-b border-gray-200">
            <h5 class="font-semibold text-gray-800 mb-2">事件列表</h5>
            <div class="max-h-32 overflow-y-auto">
              <table class="w-full text-sm">
                <thead class="bg-gray-50 sticky top-0">
                  <tr>
                    <th class="px-4 py-2 text-left text-xs font-medium text-gray-600">時間</th>
                    <th class="px-4 py-2 text-left text-xs font-medium text-gray-600">事件</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="event in sidebarData.events" :key="event.id"
                    @click="changeFenceImage(event.image ? event.image : '')"
                    :class="{ 'bg-blue-50': event.image === fenceImageUrl }"
                    class="hover:bg-gray-50 cursor-pointer border-b border-gray-100">
                    <td class="px-4 py-2 text-xs">{{ event.time }}</td>
                    <td class="px-4 py-2">
                      <span class="inline-block px-1 py-0.5 rounded text-xs font-medium"
                        :class="event.eventType === 1 ? 'bg-red-100 text-red-800' : 'bg-blue-100 text-blue-800'">
                        {{ event.eventType === 1 ? '闖入' : '離開' }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Device Info -->
          <div v-if="'observingTime' in sidebarData || 'lastEvent' in sidebarData"
            class="mb-3 pb-3 border-b border-gray-200">
            <h5 class="font-semibold text-gray-800 mb-2">設備資訊</h5>
            <div class="text-sm space-y-1">
              <div v-if="sidebarData.observingTime" class="flex justify-between py-1">
                <span>監控時間</span>
                <span class="font-medium">{{ sidebarData.observingTime }}</span>
              </div>
              <div v-if="sidebarData.stationName" class="flex justify-between py-1">
                <span>所屬站點</span>
                <span class="font-medium">{{ sidebarData.stationName }}</span>
              </div>
              <div v-if="sidebarData.lastEvent" class="flex justify-between py-1">
                <span>最後事件</span>
                <span class="font-medium">{{ sidebarData.lastEvent }}</span>
              </div>
              <div v-if="sidebarData.lastEventTime" class="flex justify-between py-1">
                <span>事件時間</span>
                <span class="font-mono text-xs">{{ sidebarData.lastEventTime }}</span>
              </div>
            </div>
          </div>

          <!-- Chart -->
          <div class="mb-3" v-if="chartOptions && chartOptions.fence">
            <EChartsWrapper :option="chartOptions.fence" :loading="false" height="180px" />
          </div>
        </div>

        <!-- High Resolution Sidebar -->
        <div v-if="sidebarType === 'highResolution'">
          <!-- Device Info -->
          <div class="mb-6 pb-3 border-b border-gray-200">
            <h5 class="font-semibold text-gray-800 mb-2">設備資訊</h5>
            <div class="text-sm space-y-1">
              <div class="flex justify-between py-1">
                <span>設備名稱</span>
                <span class="font-medium">{{ sidebarData.name }}</span>
              </div>
              <div v-if="sidebarData.stationName" class="flex justify-between py-1">
                <span>所屬站點</span>
                <span class="font-medium">{{ sidebarData.stationName }}</span>
              </div>
            </div>
          </div>

          <!-- 4K Video Stream -->
          <div class="mb-6">
            <div class="bg-gray-800 p-2 flex items-center justify-between rounded-t">
              <div class="flex items-center gap-2">
                <div class="w-2 h-2 bg-red-500 rounded-full animate-pulse"></div>
                <span class="text-white text-sm font-medium">4K即時直播</span>
              </div>
              <Button v-if="'videoUrl' in sidebarData && sidebarData.videoUrl" @click="openVideoInNewWindow"
                variant="ghost" size="sm" class="text-white hover:bg-gray-700 h-6 px-2">
                <ExternalLinkIcon class="w-3 h-3 mr-1" />
                新視窗
              </Button>
            </div>

            <div class="aspect-video bg-black rounded-b">
              <div v-if="'videoUrl' in sidebarData && typeof (sidebarData.videoUrl) === 'string'" class="w-full h-full">
                <!-- YouTube Video -->
                <iframe v-if="isYouTubeUrl(sidebarData.videoUrl)" :src="processYouTubeUrl(sidebarData.videoUrl)"
                  class="w-full h-full rounded-b" frameborder="0" allowfullscreen
                  allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                  referrerpolicy="strict-origin-when-cross-origin"
                  sandbox="allow-scripts allow-same-origin allow-presentation" />

                <!-- Regular Video -->
                <video v-else-if="isVideoFile(sidebarData.videoUrl)" :src="sidebarData.videoUrl" class="w-full h-full object-cover rounded-b" controls autoplay
                  muted playsinline>
                  您的瀏覽器不支援影片播放
                </video>

                <!-- Generic iframe for other URLs -->
                <iframe v-else
                  :src="sidebarData.videoUrl" class="w-full h-full rounded-b" frameborder="0"
                  referrerpolicy="strict-origin-when-cross-origin"
                  sandbox="allow-scripts allow-same-origin allow-forms allow-presentation allow-popups allow-top-navigation-by-user-activation">
                </iframe>
              </div>
              <div v-else class="w-full h-full bg-gray-800 flex items-center justify-center rounded-b">
                <div class="text-center text-gray-400">
                  <CameraIcon class="h-8 w-8 mx-auto mb-2" />
                  <p class="text-sm">暫無影像訊號</p>
                  <p class="text-xs mt-1">請檢查設備連線狀態</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Universal Last Update -->
        <div class="text-center text-xs text-gray-500 py-2 border-t"
          v-if="'lastUpdateTime' in sidebarData && sidebarData.lastUpdateTime">
          最後更新: {{ sidebarData.lastUpdateTime }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Button } from '@/components/ui/button'
import {
  X as XIcon,
  Car as CarIcon,
  Users as UsersIcon,
  Shield as ShieldIcon,
  Camera as CameraIcon,
  ExternalLink as ExternalLinkIcon,
  ParkingCircle as ParkingCircleIcon
} from 'lucide-vue-next'
import { getStatusText, isYouTubeUrl, processYouTubeUrl, isVideoFile } from '@/utils/statusUtils'
import { getCrowdStatus } from '@/utils/crowdStatusUtils'
import EChartsWrapper from '@/components/charts/EChartsWrapper.vue'
import CrowdImageStream from '@/components/device/CrowdImageStream.vue'
import type { EChartsOption } from 'echarts'
import type { ICrowdLandmarkResponse, IFenceLandmarkResponse, IHighResolutionLandmarkResponse, IParkingLandmarkResponse, ITrafficLandmarkResponse } from '@/api/client'
import { getDevicePrimaryColor, type DeviceType } from '@/config/deviceColors'

// Props definition
interface Props {
  showSidebar: boolean
  sidebarLoading: boolean
  sidebarType: 'parking' | 'crowd' | 'traffic' | 'fence' | 'highResolution' | ''
  sidebarTitle: string
  sidebarData: any
  selectedDeviceId: string | number
  fenceImageUrl?: string
  chartOptions?: {
    parking?: EChartsOption
    crowd?: EChartsOption
    traffic?: EChartsOption
    fence?: EChartsOption
  }
}

const props = withDefaults(defineProps<Props>(), {
  showSidebar: false,
  sidebarLoading: false,
  sidebarType: '',
  sidebarTitle: '',
  sidebarData: () => ({}),
  selectedDeviceId: '',
  fenceImageUrl: '',
  chartOptions: () => ({})
})

// Emits
const emit = defineEmits<{
  closeSidebar: []
  changeFenceImage: [imageUrl: string]
  openVideo: []
}>()

// Device colors configuration - now using unified constants
const colors = {
  parking: getDevicePrimaryColor('parking'),
  traffic: getDevicePrimaryColor('traffic'),
  crowd: getDevicePrimaryColor('crowd'),
  fence: getDevicePrimaryColor('fence'),
  highResolution: getDevicePrimaryColor('highResolution')
}

// Get sidebar icon based on type
const getSidebarIcon = (type: string) => {
  switch (type) {
    case 'parking':
      return ParkingCircleIcon
    case 'crowd':
      return UsersIcon
    case 'traffic':
      return CarIcon
    case 'fence':
      return ShieldIcon
    case 'highResolution':
      return CameraIcon
    default:
      return ParkingCircleIcon
  }
}

// Get status indicator class
const getStatusIndicatorClass = (status: any) => {
  switch (status) {
    case 'online':
      return 'bg-green-500'
    case 'offline':
      return 'bg-red-500'
    default:
      return 'bg-gray-500'
  }
}

// Get speed bar class for traffic
const getSpeedBarClass = (currentSpeed: number, speedLimit: number) => {
  const ratio = currentSpeed / speedLimit
  if (ratio <= 0.5) return 'bg-green-500'
  if (ratio <= 0.8) return 'bg-yellow-500'
  return 'bg-red-500'
}

// Event handlers
const closeSidebar = () => {
  emit('closeSidebar')
}

const changeFenceImage = (imageUrl: string) => {
  emit('changeFenceImage', imageUrl)
}


const openVideoInNewWindow = () => {
  // Handle API-based image stream for GetAIJpeg devices
  if ('apiUrl' in props.sidebarData && props.sidebarData.apiUrl && props.sidebarData.apiUrl.includes('GetAIJpeg') && !('videoUrl' in props.sidebarData && props.sidebarData.videoUrl)) {
    // Create URL parameters for the stream viewer
    const params = new URLSearchParams({
      deviceId: props.selectedDeviceId.toString(),
      deviceType: props.sidebarData.type || props.sidebarType,
      deviceName: props.sidebarData.name || `設備 ${props.selectedDeviceId}`,
      channelId: 'cam1'
    })

    // Open the dedicated stream viewer page
    const streamUrl = `/crowd-stream?${params.toString()}`
    window.open(streamUrl, '_blank', 'width=1200,height=800,resizable=yes,scrollbars=yes')
  } else {
    // Handle traditional video stream
    emit('openVideo')
  }
}
</script>