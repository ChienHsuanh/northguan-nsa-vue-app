<template>
  <Card class="h-[80vh] overflow-hidden p-0">
    <div class="relative h-full">
      <!-- Map Container -->
      <div id="main-map" class="h-full w-full"></div>

      <!-- Map Controls -->
      <div class="absolute bottom-4 left-4 z-10 flex flex-col gap-2">
        <Button variant="outline" size="sm" @click="selectLayer('all')"
          :class="selectedLayer === 'all' ? [
            'bg-emerald-100', 
            'border-emerald-400', 
            'text-emerald-700', 
            'font-medium', 
            'shadow-sm'
          ] : [
            'bg-gray-50', 
            'text-gray-500', 
            'border-gray-300', 
            'hover:bg-gray-100'
          ]"
          class="min-w-[100px] transition-all duration-200 hover:shadow-md flex items-center gap-2">
          <ActivityIcon :class="selectedLayer === 'all' ? 'text-emerald-700' : 'text-gray-500'" class="w-4 h-4" />
          全部
        </Button>

        <Button v-for="layer in mapLayers" :key="layer.type" variant="outline" size="sm"
          @click="selectLayer(layer.type)" 
          :class="selectedLayer === layer.type ? [
            getDeviceColors(layer.type as DeviceType).bgLight, 
            getDeviceColors(layer.type as DeviceType).border, 
            getDeviceColors(layer.type as DeviceType).textClass, 
            'font-medium', 
            'shadow-sm'
          ] : [
            'bg-gray-50', 
            'text-gray-500', 
            'border-gray-300', 
            'hover:bg-gray-100'
          ]"
          class="min-w-[100px] transition-all duration-200 hover:shadow-md flex items-center gap-2">
          <component :is="getLayerIcon(layer.type)" 
            :class="selectedLayer === layer.type ? getDeviceColors(layer.type as DeviceType).textClass : 'text-gray-500'" 
            class="w-4 h-4" />
          {{ layer.name }}
        </Button>
      </div>

      <!-- Device Details Sidebar -->
      <DeviceSidebar 
        :show-sidebar="showSidebar"
        :sidebar-loading="sidebarLoading"
        :sidebar-type="sidebarType"
        :sidebar-title="sidebarTitle"
        :sidebar-data="sidebarData"
        :selected-device-id="selectedDeviceId"
        :fence-image-url="fenceImageUrl"
        :chart-options="chartOptions"
        @close-sidebar="closeSidebar"
        @change-fence-image="changeFenceImage"
        @open-video="openVideoInNewWindow"
      />
    </div>
  </Card>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick, computed } from 'vue'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import {
  X as XIcon,
  Car as CarIcon,
  Users as UsersIcon,
  Navigation as NavigationIcon,
  Shield as ShieldIcon,
  Camera as CameraIcon,
  ExternalLink as ExternalLinkIcon,
  Activity as ActivityIcon
} from 'lucide-vue-next'
import { getStatusText, isYouTubeUrl, processYouTubeUrl } from '@/utils/statusUtils'
import { useRealTimeData, type CrowdHotspotItem, type FenceEventItem, type HighResolutionDeviceItem, type ParkingHotspotItem, type TrafficHotspotItem } from '@/composables/useRealTimeData'
import EChartsWrapper from '@/components/charts/EChartsWrapper.vue'
import DeviceSidebar from '@/components/shared/DeviceSidebar.vue'
import type { EChartsOption } from 'echarts'
import { MapService } from '@/services/MapService'
import type { FenceEventInfo, ICrowdLandmarkResponse, IFenceLandmarkResponse, IHighResolutionLandmarkResponse, ILandmarkItem, IParkingLandmarkResponse, ITrafficLandmarkResponse, LandmarkItem } from '@/api/client'
import { getDeviceColors, getDevicePrimaryColor, type DeviceType } from '@/config/deviceColors'

const {
  loadDeviceHistory,
  clearHistoryData,
  parkingHistory,
  crowdHistory,
  trafficHistory,
  fenceHistory,
  clearLandmarksCache
} = useRealTimeData()

// 地圖設備類型定義
interface MapDevice {
  id: string | number
  name: string
  type: string
  status: 'online' | 'offline'
  latitude: number
  longitude: number
}

// 側邊欄數據類型
type SidebarData = IParkingLandmarkResponse | ICrowdLandmarkResponse | ITrafficLandmarkResponse | IFenceLandmarkResponse | IHighResolutionLandmarkResponse

// Props to receive data from parent
interface Props {
  parkingHotspots?: ParkingHotspotItem[]
  crowdHotspots?: CrowdHotspotItem[]
  trafficHotspots?: TrafficHotspotItem[]
  fenceEvents?: FenceEventItem[]
  highResolutionDevices?: HighResolutionDeviceItem[]
  selectedStation?: string
}

const props = withDefaults(defineProps<Props>(), {
  parkingHotspots: () => [],
  crowdHotspots: () => [],
  trafficHotspots: () => [],
  fenceEvents: () => [],
  highResolutionDevices: () => [],
  selectedStation: 'all'
})

// Map instance
let map: L.Map | null = null

// State
const showSidebar = ref(false)
const sidebarLoading = ref(false)
const sidebarType = ref<'parking' | 'crowd' | 'traffic' | 'fence' | 'highResolution' | ''>('')
const sidebarTitle = ref('')
const sidebarData = ref<SidebarData>({})
const fenceImageUrl = ref('')
const allLayersVisible = ref(true)
const selectedDeviceId = ref<string | number>('')

// 跟蹤用戶是否手動操作了地圖
const userHasInteracted = ref(false)

// 動態計算側邊欄寬度 (px)
const dynamicSidebarWidthPx = ref(0)
const calculateSidebarWidth = () => {
  const mdBreakpoint = 768 // Tailwind's default md breakpoint
  if (window.innerWidth < mdBreakpoint) {
    dynamicSidebarWidthPx.value = window.innerWidth // Full width on mobile
  } else {
    dynamicSidebarWidthPx.value = window.innerWidth * 0.45 // 45% of window width on larger screens
  }
}

// Map layers configuration - using unified device colors
const mapLayers = ref([
  { type: 'traffic', name: '車流', visible: true, color: getDevicePrimaryColor('traffic') },
  { type: 'parking', name: '停車場', visible: true, color: getDevicePrimaryColor('parking') },
  { type: 'crowd', name: '人潮', visible: true, color: getDevicePrimaryColor('crowd') },
  { type: 'fence', name: '電子圍欄', visible: true, color: getDevicePrimaryColor('fence') },
  { type: 'highResolution', name: '4K影像', visible: true, color: getDevicePrimaryColor('highResolution') }
])

// Selected layer for single selection mode
const selectedLayer = ref<string>('all')

// ECharts 圖表配置
const parkingChartOption = computed<EChartsOption>(() => {
  // 獲取的停車場歷史數據
  if (!parkingHistory.value || parkingHistory.value.length === 0) {
    return {
      title: {
        text: '停車場使用率歷史',
        textStyle: { fontSize: 12 }
      },
      graphic: {
        type: 'text',
        left: 'center',
        top: 'middle',
        style: {
          text: '暫無歷史數據',
          fontSize: 14,
          fill: '#999'
        }
      }
    }
  }

  // 轉換歷史數據為圖表格式
  const labels: string[] = []
  const rates: number[] = []

  // 確保數據按時間排序
  const sortedRecords = parkingHistory.value.sort((a, b) => {
    const dateTimeA = new Date(a.timestamp ?? 0)
    const dateTimeB = new Date(b.timestamp ?? 0)
    return dateTimeA.getTime() - dateTimeB.getTime()
  })

  sortedRecords.forEach(record => {
    labels.push(record.time ?? "")
    rates.push(record.occupancyRate ?? 0)
  })

  return {
    title: {
      text: `${parkingHistory.value[0]?.deviceName || '未知設備'} - 停車場使用率歷史`,
      textStyle: { fontSize: 12 }
    },
    tooltip: {
      trigger: 'axis',
      formatter: '{b}: {c}%'
    },
    xAxis: {
      type: 'category',
      data: labels,
      axisLabel: { fontSize: 10, rotate: 45 }
    },
    yAxis: {
      type: 'value',
      name: '使用率(%)',
      axisLabel: { fontSize: 10 }
    },
    series: [{
      type: 'line',
      data: rates,
      itemStyle: { color: getDevicePrimaryColor('parking') },
      lineStyle: { color: getDevicePrimaryColor('parking') },
      smooth: true
    }]
  }
})

const crowdChartOption = computed<EChartsOption>(() => {
  // 獲取人流歷史數據
  if (!crowdHistory.value || crowdHistory.value.length === 0) {
    return {
      title: {
        text: '人流密度歷史',
        textStyle: { fontSize: 12 }
      },
      graphic: {
        type: 'text',
        left: 'center',
        top: 'middle',
        style: {
          text: '暫無歷史數據',
          fontSize: 14,
          fill: '#999'
        }
      }
    }
  }

  const labels: string[] = []
  const peopleCount: number[] = []

  // 確保數據按時間排序
  const sortedRecords = crowdHistory.value.sort((a, b) => {
    const dateTimeA = new Date(a.timestamp ?? 0)
    const dateTimeB = new Date(b.timestamp ?? 0)
    return dateTimeA.getTime() - dateTimeB.getTime()
  })

  sortedRecords.forEach(record => {
    labels.push(record.time ?? "")
    // 使用實際的人數數據
    peopleCount.push(record.peopleCount ?? 0)
  })

  return {
    title: {
      text: `${crowdHistory.value[0]?.deviceName || '未知設備'} - 人流密度歷史`,
      textStyle: { fontSize: 12 }
    },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const value = params[0].value
        return `${params[0].axisValue}: ${value}人`
      }
    },
    xAxis: {
      type: 'category',
      data: labels,
      axisLabel: { fontSize: 10, rotate: 45 }
    },
    yAxis: {
      type: 'value',
      name: '人數',
      axisLabel: { fontSize: 10, formatter: '{value}人' }
    },
    series: [{
      type: 'line',
      data: peopleCount,
      itemStyle: { color: getDevicePrimaryColor('crowd') },
      lineStyle: { color: getDevicePrimaryColor('crowd') },
      smooth: true
    }]
  }
})

const trafficChartOption = computed<EChartsOption>(() => {
  // 獲取交通歷史數據
  if (!trafficHistory.value || trafficHistory.value.length === 0) {
    return {
      title: {
        text: '交通狀況歷史',
        textStyle: { fontSize: 12 }
      },
      graphic: {
        type: 'text',
        left: 'center',
        top: 'middle',
        style: {
          text: '暫無歷史數據',
          fontSize: 14,
          fill: '#999'
        }
      }
    }
  }

  // 使用停駐百分比數據
  const labels: string[] = []
  const avarageSpeeds: number[] = []

  // 確保數據按時間排序
  const sortedRecords = trafficHistory.value.sort((a, b) => {
    const dateTimeA = new Date(a.timestamp ?? 0)
    const dateTimeB = new Date(b.timestamp ?? 0)
    return dateTimeA.getTime() - dateTimeB.getTime()
  })

  sortedRecords.forEach(record => {
    labels.push(record.time ?? "")
    avarageSpeeds.push(record.averageSpeed ?? 0)
  })

  return {
    title: {
      text: `${trafficHistory.value[0]?.deviceName || '未知設備'} - 平均速度歷史`,
      textStyle: { fontSize: 12 }
    },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const value = params[0].value
        return `${params[0].axisValue}: ${value}%`
      }
    },
    xAxis: {
      type: 'category',
      data: labels,
      axisLabel: { fontSize: 10, rotate: 45 }
    },
    yAxis: {
      type: 'value',
      max: 100,
      min: 0,
      name: '平均速度(%)',
      axisLabel: { fontSize: 10, formatter: '{value}%' }
    },
    series: [{
      type: 'line',
      data: avarageSpeeds,
      itemStyle: { color: getDevicePrimaryColor('traffic') },
      lineStyle: { color: getDevicePrimaryColor('traffic') },
      smooth: true
    }]
  }
})

const fenceChartOption = computed<EChartsOption>(() => {
  // 獲取圍籬歷史數據
  if (!fenceHistory.value || fenceHistory.value.length === 0) {
    return {
      title: {
        text: '圍籬事件歷史',
        textStyle: { fontSize: 12 }
      },
      graphic: {
        type: 'text',
        left: 'center',
        top: 'middle',
        style: {
          text: '暫無歷史數據',
          fontSize: 14,
          fill: '#999'
        }
      }
    }
  }

  const labels: string[] = []
  const eventCounts: number[] = []

  // 確保數據按時間排序
  const sortedRecords = fenceHistory.value.sort((a, b) => {
    const dateTimeA = new Date(a.timestamp ?? 0)
    const dateTimeB = new Date(b.timestamp ?? 0)
    return dateTimeA.getTime() - dateTimeB.getTime()
  })

  sortedRecords.forEach(record => {
    labels.push(record.time ?? "")
    eventCounts.push(1)
  })

  return {
    title: {
      text: `${fenceHistory.value[0]?.deviceName ?? ""} - 圍籬事件歷史`,
      textStyle: { fontSize: 12 }
    },
    tooltip: {
      trigger: 'axis',
      formatter: '{b}: {c} 次事件'
    },
    xAxis: {
      type: 'category',
      data: labels,
      axisLabel: { fontSize: 10, rotate: 45 }
    },
    yAxis: {
      type: 'value',
      name: '事件次數',
      axisLabel: { fontSize: 10 }
    },
    series: [{
      type: 'bar',
      data: eventCounts,
      itemStyle: { color: getDevicePrimaryColor('fence') },
      emphasis: {
        itemStyle: { color: getDeviceColors('fence').primaryDark }
      }
    }]
  }
})

// Chart options for DeviceSidebar component
const chartOptions = computed(() => ({
  parking: parkingChartOption.value,
  crowd: crowdChartOption.value,
  traffic: trafficChartOption.value,
  fence: fenceChartOption.value
}))

// Initialize map
const initializeMap = async () => {
  try {
    console.log('開始初始化地圖...')

    // 更強力的清理現有地圖實例
    cleanup()

    // 等待清理完成
    await nextTick()

    // 確保地圖容器存在且乾淨
    const mapContainer = document.getElementById('main-map')
    if (!mapContainer) {
      console.warn('主地圖容器不存在')
      return
    }

    // 強制清理容器內容和 Leaflet 相關屬性
    mapContainer.innerHTML = ''
    if ((mapContainer as any)._leaflet_id) {
      delete (mapContainer as any)._leaflet_id
    }
    // 移除所有 leaflet 相關的類名和屬性
    mapContainer.className = mapContainer.className.replace(/leaflet-\S+/g, '').trim()
    mapContainer.removeAttribute('style')

    // 等待 DOM 更新
    await nextTick()

    // 確保容器恢復基本樣式
    mapContainer.className = 'h-full w-full'

    // 動態載入 Leaflet 以保持一致的 chunking
    const L = await import('leaflet')

    // 初始化地圖實例
    map = L.map('main-map', {
      zoomControl: true,
      attributionControl: true,
      preferCanvas: false,
      // 添加這些選項以提高穩定性
      trackResize: true,
      boxZoom: true,
      doubleClickZoom: true,
      dragging: true,
      zoomSnap: 1,
      zoomDelta: 1,
      wheelDebounceTime: 40,
      wheelPxPerZoomLevel: 60
    }).setView([25.0330, 121.5654], 13)

    // 添加圖層
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
      maxZoom: 19,
      detectRetina: true
    }).addTo(map)

    // 添加用戶交互事件監聽器
    map.on('dragstart', () => {
      userHasInteracted.value = true
    })

    map.on('zoomstart', () => {
      userHasInteracted.value = true
    })

    // 確保地圖正確渲染
    setTimeout(() => {
      if (map) {
        map.invalidateSize()
      }
    }, 100)

    console.log('地圖初始化成功')
  } catch (error) {
    console.error('地圖初始化失敗:', error)
    // 如果初始化失敗，重置地圖變數
    map = null
  }
}

// Clear all markers from map
const clearMapMarkers = async () => {
  if (map) {
    const L = await import('leaflet')
    map.eachLayer((layer) => {
      if (layer instanceof L.Marker) {
        map?.removeLayer(layer)
      }
    })
  }
}

// Device colors and icons configuration - using unified device colors
const colors = {
  parking: getDevicePrimaryColor('parking'),
  traffic: getDevicePrimaryColor('traffic'),
  crowd: getDevicePrimaryColor('crowd'),
  fence: getDevicePrimaryColor('fence'),
  highResolution: getDevicePrimaryColor('highResolution')
}

// Get lucide icon SVG string for device types
const getDeviceIconSvg = (type: string): string => {
  switch (type) {
    case 'parking':
      return `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="4" rx="2" ry="2"/><line x1="16" x2="16" y1="2" y2="6"/><line x1="8" x2="16" y1="9" y2="9"/><line x1="8" x2="10" y1="12" y2="12"/><line x1="8" x2="10" y1="15" y2="15"/><line x1="8" x2="10" y1="18" y2="18"/></svg>`
    case 'traffic':
      return `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="3 11 22 2 13 21 11 13 3 11"/></svg>`
    case 'crowd':
      return `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`
    case 'fence':
      return `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10"/></svg>`
    case 'highResolution':
      return `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14.5 4h-5L7 7H4a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-3l-2.5-3z"/><circle cx="12" cy="13" r="3"/></svg>`
    default:
      return `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="4" rx="2" ry="2"/><line x1="16" x2="16" y1="2" y2="6"/><line x1="8" x2="16" y1="9" y2="9"/><line x1="8" x2="10" y1="12" y2="12"/><line x1="8" x2="10" y1="15" y2="15"/><line x1="8" x2="10" y1="18" y2="18"/></svg>`
  }
}

// Get status badge class for popup
const getStatusBadgeClass = (status: string): string => {
  switch (status) {
    case 'online':
    case 'active':
    case 'normal':
      return 'bg-green-100 text-green-800'
    case 'warning':
    case 'medium':
      return 'bg-yellow-100 text-yellow-800'
    case 'error':
    case 'high':
    case 'critical':
      return 'bg-red-100 text-red-800'
    case 'offline':
    case 'inactive':
      return 'bg-gray-100 text-gray-800'
    default:
      return 'bg-blue-100 text-blue-800'
  }
}

// Get status indicator class for sidebar header
const getStatusIndicatorClass = (status?: string): string => {
  switch (status) {
    case 'online':
    case 'active':
    case 'normal':
      return 'bg-green-400 animate-pulse'
    case 'warning':
    case 'medium':
      return 'bg-yellow-400 animate-pulse'
    case 'error':
    case 'high':
    case 'critical':
      return 'bg-red-400 animate-pulse'
    case 'offline':
    case 'inactive':
      return 'bg-gray-400'
    default:
      return 'bg-blue-400'
  }
}

// Get sidebar icon component
const getSidebarIcon = (type: string) => {
  switch (type) {
    case 'parking':
      return CarIcon
    case 'traffic':
      return NavigationIcon
    case 'crowd':
      return UsersIcon
    case 'fence':
      return ShieldIcon
    case 'highResolution':
      return CameraIcon
    default:
      return ActivityIcon
  }
}

// Get sidebar header class based on device type - using unified device colors
const getSidebarHeaderClass = (type: string): string => {
  const deviceType = type as DeviceType
  const colors = getDeviceColors(deviceType)
  return `${colors.bg}`
}

// Get layer icon for each device type
const getLayerIcon = (type: string) => {
  switch (type) {
    case 'traffic':
      return CarIcon
    case 'parking':
      return NavigationIcon
    case 'crowd':
      return UsersIcon
    case 'fence':
      return ShieldIcon
    case 'highResolution':
      return CameraIcon
    default:
      return ActivityIcon
  }
}

// Get speed bar class based on speed vs limit
const getSpeedBarClass = (speed: number, speedLimit: number): string => {
  if (speedLimit <= 0) return 'bg-blue-500'

  const ratio = speed / speedLimit
  if (ratio >= 0.8) return 'bg-green-500'  // 正常速度
  if (ratio >= 0.5) return 'bg-yellow-500' // 稍慢
  return 'bg-red-500' // 很慢/壅塞
}

// Get device icon HTML
const getDeviceIconHtml = (type: string, status: string = 'online') => {

  const color = colors[type as keyof typeof colors] || '#6c757d'
  const iconSvg = getDeviceIconSvg(type)
  const opacity = status === 'online' ? '1' : '0.6'

  return `
    <div style="
      width: 30px;
      height: 30px;
      background-color: ${color};
      border: 2px solid white;
      border-radius: 45%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      opacity: ${opacity};
      box-shadow: 0 2px 4px rgba(0,0,0,0.2);
    ">
      ${iconSvg}
    </div>
  `
}

// Add device marker to map
const addDeviceMarker = async (device: MapDevice): Promise<void> => {
  if (!map || !device.latitude || !device.longitude) return

  const L = await import('leaflet')
  const iconHtml = getDeviceIconHtml(device.type, device.status)
  const icon = L.divIcon({
    html: iconHtml,
    className: 'custom-device-marker',
    iconSize: [30, 30],
    iconAnchor: [15, 15]
  })

  // 直接點擊打開 sidebar，移除 popup
  const marker = L.marker([device.latitude, device.longitude], { icon })
    .on('click', (e) => {
      e.originalEvent?.stopPropagation()
      resetSidebar()
      showDeviceDetail(device.type, device)
    })

  marker.addTo(map)
}

// 儲存已添加的標記座標以避免重疊
const addedMarkerCoordinates = new Set<string>()

// Update map with hotspot data
const updateMapWithHotspots = async (): Promise<void> => {
  if (!map) return

  await clearMapMarkers()
  addedMarkerCoordinates.clear()

  const layers = [
    { type: 'parking', data: mapLandmarks.value.parkingLandmarks },
    { type: 'crowd', data: mapLandmarks.value.crowdLandmarks },
    { type: 'traffic', data: mapLandmarks.value.trafficLandmarks },
    { type: 'fence', data: mapLandmarks.value.fenceLandmarks },
    { type: 'highResolution', data: mapLandmarks.value.highResolutionLandmarks }
  ]

  // 逐個添加標記
  for (const { type, data } of layers) {
    const layer = mapLayers.value.find(l => l.type === type)
    if (!layer?.visible || !data?.length) continue

    for (const device of data) {
      if (device.lat === undefined || device.lng === undefined) continue

      try {
        const mapDevice: MapDevice = {
          id: device.id ?? -1,
          name: device.name ?? "",
          type,
          status: device.status === "online" || device.status === "offline" ? device.status : "offline",
          latitude: device.lat,
          longitude: device.lng
        }
        await addDeviceMarker(mapDevice)
      } catch (err) {
        console.error(`添加 ${type} 標記失敗:`, err)
      }
    }
  }

  console.log('地圖更新完成:', {
    parking: mapLandmarks.value.parkingLandmarks?.length ?? 0,
    crowd: mapLandmarks.value.crowdLandmarks?.length ?? 0,
    traffic: mapLandmarks.value.trafficLandmarks?.length ?? 0,
    fence: mapLandmarks.value.fenceLandmarks?.length ?? 0,
    highResolution: mapLandmarks.value.highResolutionLandmarks?.length ?? 0,
  })
}

// Select layer (single selection mode)
const selectLayer = async (layerType: string) => {
  selectedLayer.value = layerType
  
  // Update visibility based on selection
  if (layerType === 'all') {
    // Show all layers
    mapLayers.value.forEach(layer => {
      layer.visible = true
    })
    allLayersVisible.value = true
  } else {
    // Show only selected layer
    mapLayers.value.forEach(layer => {
      layer.visible = layer.type === layerType
    })
    allLayersVisible.value = false
  }
  
  // Update map display
  await updateMapWithHotspots()
}

const resetSidebar = () => {
  sidebarLoading.value = false
  sidebarType.value = ''
  sidebarTitle.value = ''
  sidebarData.value = {}
  selectedDeviceId.value = ''
  showSidebar.value = false
  clearHistoryData()
}

const showDeviceDetail = async (type: 'parking' | 'crowd' | 'traffic' | 'fence' | 'highResolution', device: MapDevice): Promise<void> => {
  // 設置 loading 狀態
  sidebarLoading.value = true
  sidebarType.value = type
  showSidebar.value = true
  selectedDeviceId.value = device.id

  // 清空之前的歷史數據
  clearHistoryData()

  try {
    // 根據設備類型調用對應的 API 獲取詳細數據
    switch (type) {
      case 'parking':
        sidebarTitle.value = '停車場詳情'
        const parkingResponse = await MapService.getLandmarkParking(Number(device.id))
        sidebarData.value = parkingResponse
        break

      case 'crowd':
        sidebarTitle.value = '人流監控詳情'
        const crowdResponse = await MapService.getLandmarkCrowd(Number(device.id))
        sidebarData.value = crowdResponse
        break

      case 'traffic':
        sidebarTitle.value = '交通監控詳情'
        const trafficResponse = await MapService.getLandmarkTraffic(Number(device.id))
        sidebarData.value = trafficResponse
        break

      case 'fence':
        sidebarTitle.value = '電子圍籬詳情'
        const fenceResponse = await MapService.getLandmarkFence(Number(device.id))
        sidebarData.value = fenceResponse
        break

      case 'highResolution':
        sidebarTitle.value = '4K影像詳情'
        const hrResponse = await MapService.getLandmarkHighResolution(Number(device.id))
        sidebarData.value = hrResponse
        break
    }
  } catch (error) {
    console.error('載入設備詳細資料失敗:', error)
    // 如果 API 調用失敗，使用基本數據作為備用
    const basicDevice = device as MapDevice
    sidebarData.value = {
      name: basicDevice.name,
      status: basicDevice.status,
      lastUpdateTime: '',
    }
  } finally {
    sidebarLoading.value = false
  }

  try {
    // 載入歷史數據（24小時內）
    if (type !== 'highResolution') {
      // 戰情室總覽篩選的分站
      const currentStationId = props.selectedStation && props.selectedStation !== 'all' ? parseInt(props.selectedStation) : undefined

      // 不等待歷史數據載入完成
      loadDeviceHistory(device.name, type, currentStationId)
    }
  } catch (error) {
    console.error('載入歷史數據失敗:', error)
  }

  // Ensure map stays visible when sidebar opens
  nextTick(() => {
    setTimeout(() => {
      if (map) {
        map.invalidateSize()
      }
    }, 50)
  })
}

// Close sidebar
const closeSidebar = () => {
  showSidebar.value = false
  sidebarType.value = ''
  sidebarData.value = {}
  selectedDeviceId.value = ''

  // Ensure map redraws properly when sidebar closes
  nextTick(() => {
    setTimeout(() => {
      if (map) {
        map.invalidateSize()
      }
    }, 50)
  })
}

// Change fence image
const changeFenceImage = (imageUrl: string) => {
  fenceImageUrl.value = imageUrl
}

// 在新視窗開啟影片
const openVideoInNewWindow = () => {
  if (sidebarData.value.videoUrl) {
    const newWindow = window.open('', '_blank');
    if (newWindow) {
      newWindow.location.href = sidebarData.value.videoUrl;
    } else {
      // 如果無法打開新窗口（例如被彈出窗口攔截器阻止），則在當前窗口打開
      window.location.href = sidebarData.value.videoUrl;
    }
  }
}

// Cleanup
const cleanup = () => {
  try {
    console.log('開始清理地圖實例...')

    // 先清理標記和圖層
    if (map) {
      try {
        // 清除所有事件監聽器
        map.off()
        // 移除所有圖層
        map.eachLayer((layer) => {
          map?.removeLayer(layer)
        })
        // 完全移除地圖實例
        map.remove()
      } catch (error) {
        console.warn('移除地圖時發生錯誤:', error)
      }
      map = null
    }

    // 徹底清理地圖容器
    const mapContainer = document.getElementById('main-map') as HTMLElement
    if (mapContainer) {
      // 清除所有 Leaflet 相關屬性
      if ((mapContainer as any)._leaflet_id) {
        delete (mapContainer as any)._leaflet_id
      }
      if ((mapContainer as any)._leaflet) {
        delete (mapContainer as any)._leaflet
      }

      // 清除所有子元素
      mapContainer.innerHTML = ''

      // 移除所有 Leaflet 相關的類名
      mapContainer.className = mapContainer.className.replace(/leaflet-\S*/g, '').trim()

      // 重置樣式
      mapContainer.removeAttribute('style')
      mapContainer.className = 'h-full w-full'
    }

    // 清理其他狀態
    addedMarkerCoordinates.clear()
    userHasInteracted.value = false
    isLoadingLandmarks.value = false

    // 重置地圖標點數據
    mapLandmarks.value = {
      parkingLandmarks: [],
      crowdLandmarks: [],
      trafficLandmarks: [],
      fenceLandmarks: [],
      highResolutionLandmarks: []
    }

    console.log('地圖清理完成')
  } catch (error) {
    console.warn('清理主地圖時發生錯誤:', error)
    map = null
  }
}

// 新增：地圖標點數據
const mapLandmarks = ref({
  parkingLandmarks: [] as ILandmarkItem[],
  crowdLandmarks: [] as ILandmarkItem[],
  trafficLandmarks: [] as ILandmarkItem[],
  fenceLandmarks: [] as ILandmarkItem[],
  highResolutionLandmarks: [] as ILandmarkItem[]
})

// 載入地圖標點數據
const loadMapLandmarks = async () => {
  try {
    const { loadMapLandmarks: loadLandmarks } = useRealTimeData()
    const stationId = props.selectedStation && props.selectedStation !== 'all'
      ? parseInt(props.selectedStation)
      : undefined

    const landmarks = await loadLandmarks(stationId)
    mapLandmarks.value = landmarks

    console.log('地圖標點數據載入完成:', landmarks)
  } catch (error) {
    console.error('載入地圖標點數據失敗:', error)
  }
}

// 自動居中和縮放地圖到所有可見標點
const fitMapToMarkers = async () => {
  if (!map) return

  const L = await import('leaflet')
  const allMarkers: L.LatLng[] = []

  // 收集所有可見的標點座標
  map.eachLayer((layer) => {
    if (layer instanceof L.Marker) {
      allMarkers.push(layer.getLatLng())
    }
  })

  if (allMarkers.length > 0) {
    if (allMarkers.length === 1) {
      const sidebarWidth = dynamicSidebarWidthPx.value // 始終預留 sidebar 寬度 w-[45%] = 動態計算px
      const bounds = L.latLngBounds([allMarkers[0], allMarkers[0]])

      map.fitBounds(bounds, {
        paddingTopLeft: [0, 20],
        paddingBottomRight: [sidebarWidth + 20, 20]
      })
    } else {
      // 多個標點時，始終考慮 sidebar 寬度
      const group = new L.FeatureGroup(
        allMarkers.map(latlng => L.marker(latlng))
      )
      const sidebarWidth = dynamicSidebarWidthPx.value // 始終預留 sidebar 寬度 w-[45%] = 動態計算px

      map.fitBounds(group.getBounds(), {
        paddingTopLeft: [20, 20],
        paddingBottomRight: [sidebarWidth + 20, 20]
      })
    }
  }
}

// 防止重複載入的標記
const isLoadingLandmarks = ref(true)

// Watch for data changes and update map
watch(
  () => [props.parkingHotspots, props.crowdHotspots, props.trafficHotspots, props.fenceEvents, props.highResolutionDevices, props.selectedStation],
  async (newValues, oldValues) => {
    // 檢查是否為分站切換
    const isStationChange = newValues[5] !== oldValues?.[5]

    // 只在分站切換或初次載入時重新載入標點數據
    if (isStationChange || !oldValues) {
      if (!isLoadingLandmarks.value) {
        isLoadingLandmarks.value = true
        try {
          // 載入地圖標點數據
          await loadMapLandmarks()
          // 更新地圖顯示
          await updateMapWithHotspots()
        } finally {
          isLoadingLandmarks.value = false
        }
      }
    } else {
      // 如果不是分站切換，只更新地圖顯示（使用已載入的數據）
      await updateMapWithHotspots()
    }

    // 只在初次載入或分站切換且用戶未手動操作時自動居中
    // 只在初次載入或分站切換且用戶未手動操作時自動居中
    // if ((!oldValues || isStationChange) && !userHasInteracted.value) {
    //   await nextTick()
    //   await fitMapToMarkers()
    // }

    // 如果是分站切換，重置用戶交互標記
    if (isStationChange) {
      userHasInteracted.value = false
      await nextTick()
      await fitMapToMarkers()
    }
  },
  { deep: true, immediate: true }
)

// 在 updateMapWithHotspots 中執行 fitMapToMarkers
watch(
  () => isLoadingLandmarks.value,
  async (newVal) => {
    if (!newVal && map && !userHasInteracted.value) {
      await nextTick()
      await fitMapToMarkers()
    }
  }
)


onMounted(async () => {
  // 初始化側邊欄寬度
  calculateSidebarWidth()
  window.addEventListener('resize', calculateSidebarWidth)

  // 確保在熱重載時先清理舊的地圖實例
  cleanup()
  // 等待一個 tick 確保 DOM 清理完成
  await nextTick()

  // 確保容器已經完全渲染
  setTimeout(async () => {
    const mapContainer = document.getElementById('main-map')
    if (mapContainer && mapContainer.offsetWidth > 0 && mapContainer.offsetHeight > 0) {
      await initializeMap()

      // 初始化完成後，立即載入標點數據
      setTimeout(async () => {
        console.log('熱重載後重新載入標點數據')
        await loadMapLandmarks()
        await updateMapWithHotspots()
        await fitMapToMarkers()
      }, 300)
    } else {
      console.warn('地圖容器尚未完全渲染，延遲初始化')
      // 如果容器還沒有尺寸，再等一下
      setTimeout(async () => {
        await initializeMap()
        // 延遲初始化也要載入標點
        setTimeout(async () => {
          await loadMapLandmarks()
          await updateMapWithHotspots()
          await fitMapToMarkers()
        }, 300)
      }, 200)
    }
  }, 50)
})

onUnmounted(() => {
  window.removeEventListener('resize', calculateSidebarWidth)
  cleanup()
})

// 處理熱重載時的清理
if (import.meta.hot) {
  import.meta.hot.dispose(() => {
    console.log('熱重載清理：清理地圖和快取')
    cleanup()
    // 清除 landmarks 快取，確保重新載入新數據
    if (clearLandmarksCache) {
      clearLandmarksCache()
    }
  })
}

defineExpose({
  showDeviceDetail: (type: 'parking' | 'crowd' | 'traffic' | 'fence' | 'highResolution', device: any) => {
    showDeviceDetail(type, device);
  }
})
</script>

<style>
/* Import Leaflet CSS */
@import 'leaflet/dist/leaflet.css';

/* Custom device marker styles */
:deep(.custom-device-marker) {
  background: transparent !important;
  border: none !important;
}

:deep(.custom-device-marker div) {
  transition: all 0.3s ease;
}

:deep(.custom-device-marker:hover div) {
  transform: scale(1.1);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3) !important;
}

/* Map container */
#main-map {
  z-index: 1;
  position: relative;
  width: 100%;
  height: 100%;
}

/* Ensure map tiles load properly */
:deep(.leaflet-container) {
  background: #ddd;
}

:deep(.leaflet-tile) {
  max-width: none !important;
}

/* Fix for map rendering issues */
:deep(.leaflet-map-pane) {
  z-index: 1;
}

:deep(.leaflet-tile-pane) {
  z-index: 1;
}

/* Leaflet popup styling */
:deep(.leaflet-popup-content-wrapper) {
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
  border: 1px solid rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

:deep(.leaflet-popup-content) {
  margin: 0;
  padding: 0;
  font-family: inherit;
}

:deep(.leaflet-popup-tip) {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

/* Custom popup styling */
:deep(.custom-popup .leaflet-popup-content-wrapper) {
  background: white;
  color: #374151;
  font-size: 14px;
  line-height: 1.5;
}

:deep(.custom-popup .leaflet-popup-close-button) {
  color: #6b7280;
  font-size: 18px;
  font-weight: bold;
  padding: 8px;
  top: 8px;
  right: 8px;
  width: auto;
  height: auto;
  border-radius: 6px;
  transition: all 0.2s ease;
}

:deep(.custom-popup .leaflet-popup-close-button:hover) {
  background-color: #f3f4f6;
  color: #374151;
}

/* Popup content styling */
:deep(.custom-popup .bg-gray-50) {
  background-color: #f9fafb !important;
}

:deep(.custom-popup .text-gray-600) {
  color: #4b5563 !important;
}

:deep(.custom-popup .text-gray-800) {
  color: #1f2937 !important;
}

:deep(.custom-popup .text-gray-500) {
  color: #6b7280 !important;
}

:deep(.custom-popup .border-gray-200) {
  border-color: #e5e7eb !important;
}

:deep(.custom-popup .bg-green-100) {
  background-color: #dcfce7 !important;
}

:deep(.custom-popup .text-green-800) {
  color: #166534 !important;
}

:deep(.custom-popup .bg-yellow-100) {
  background-color: #fef3c7 !important;
}

:deep(.custom-popup .text-yellow-800) {
  color: #92400e !important;
}

:deep(.custom-popup .bg-red-100) {
  background-color: #fee2e2 !important;
}

:deep(.custom-popup .text-red-800) {
  color: #991b1b !important;
}

:deep(.custom-popup .bg-blue-100) {
  background-color: #dbeafe !important;
}

:deep(.custom-popup .text-blue-800) {
  color: #1e40af !important;
}

:deep(.custom-popup .bg-gray-100) {
  background-color: #f3f4f6 !important;
}

:deep(.custom-popup .bg-gray-200) {
  background-color: #e5e7eb !important;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  :deep(.custom-device-marker div) {
    width: 24px !important;
    height: 24px !important;
    font-size: 12px !important;
  }
}
</style>
