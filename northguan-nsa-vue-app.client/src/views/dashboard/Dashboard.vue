<template>
  <div
    class="min-h-screen bg-gradient-to-br from-sky-100 via-blue-100 to-slate-200 font-sans text-gray-800 flex flex-col">
    <!-- Header -->
    <DashboardHeader />

    <!-- Main Content -->
    <main class="flex-1 container mx-auto px-4 lg:px-6 pb-6 flex flex-col lg:min-h-0">
      <div class="flex-1 grid grid-cols-1 xl:grid-cols-12 gap-4 xl:min-h-0">

        <!-- Left Column - Monitoring Cards -->
        <aside class="xl:col-span-2 flex flex-col gap-4 xl:h-full justify-center">
          <MonitoringChart title="停車場車位辨識" :data="parkingMonitoringData.data" :legend="parkingMonitoringData.legend"
            class="flex-1" />
          <MonitoringChart title="人流辨識" :data="crowdMonitoringData.data" :legend="crowdMonitoringData.legend"
            class="flex-1" />
          <MonitoringChart title="車流辨識" :data="trafficMonitoringData.data" :legend="trafficMonitoringData.legend"
            class="flex-1" />
          <MonitoringChart title="電子圍籬" :data="fenceMonitoringData.data" :legend="fenceMonitoringData.legend"
            class="flex-1" />
        </aside>

        <!-- Right Content Area -->
        <div class="xl:col-span-10 flex flex-col gap-4 xl:h-full">

          <!-- Top Row: Image Grid & Device Status -->
          <div class="grid grid-cols-1 xl:grid-cols-4 gap-4 flex-[3]">
            <!-- Image Grid -->
            <section class="xl:col-span-3">
              <DashboardCard class="flex flex-col h-full">
                <div class="flex-1 grid grid-cols-2 lg:grid-cols-4 gap-3 min-h-0">
                  <div v-for="location in locations" :key="location.name"
                    class="relative overflow-hidden rounded-xl group min-h-0 flex-shrink-0 h-full"
                    :class="{ 'cursor-pointer': hasStreamCapability(location), 'cursor-default': !hasStreamCapability(location) }"
                    @click="hasStreamCapability(location) ? openVideoModal(location) : null">
                    <img :src="location.url" :alt="location.name"
                      class="w-full h-full object-cover transition-transform duration-300"
                      :class="{ 'group-hover:scale-110': hasStreamCapability(location) }" loading="lazy" />
                    <div class="absolute inset-0 transition-background duration-300"
                      :class="hasStreamCapability(location) ? 'bg-black/0 group-hover:bg-black/20' : 'bg-black/10'">
                    </div>
                    <!-- Play Button Overlay - show if has streaming capability -->
                    <div v-if="hasStreamCapability(location)"
                      class="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                      <div class="bg-white/90 rounded-full p-3 shadow-lg">
                        <PlayIcon class="w-8 h-8 text-blue-600" />
                      </div>
                    </div>
                    <!-- No Video Indicator -->
                    <div v-else class="absolute inset-0 flex items-center justify-center">
                      <div class="bg-black/60 rounded-full p-2">
                        <span class="text-white text-xs">暫無影像</span>
                      </div>
                    </div>
                    <div class="absolute bottom-0 left-0 p-3">
                      <span class="text-white text-xs font-semibold px-3 py-1 rounded-full"
                        :class="hasStreamCapability(location) ? 'bg-blue-600' : 'bg-gray-600'">
                        {{ location.name }}
                      </span>
                    </div>
                  </div>
                </div>
              </DashboardCard>
            </section>

            <!-- Device Status -->
            <aside class="xl:col-span-1">
              <DashboardCard class="flex flex-col h-full">
                <h2 class="text-sm font-bold text-gray-700 mb-3">設備在線率</h2>
                <div class="flex-1 grid grid-cols-2 gap-y-3 gap-x-2 content-around">
                  <StatusIndicator v-for="(item, index) in deviceStatusData" :key="index" :item="item" />
                </div>
              </DashboardCard>
            </aside>
          </div>

          <!-- Bottom Row: Status Charts -->
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 flex-[2] min-h-0">
            <StatusChart title="停車場車位辨識 各分站使用率現況" :data="parkingStationData" type="parking" class="h-full" />
            <StatusChart title="人流辨識 各分站容流量現況" :data="crowdStationData" type="crowd" class="h-full" />
            <StatusChart title="車流辨識 各分站車流量現況" :data="trafficStationData" type="traffic" class="h-full" />
            <StatusChart title="電子圍籬 各分站狀態現況" :data="fenceStationData" type="fence" class="h-full" />
          </div>

        </div>
      </div>
    </main>

    <!-- Video Modal -->
    <VideoModal :location="selectedLocation" :device="selectedDevice" v-model:is-open="showVideoModal"
      @open-in-new-window="handleOpenVideoInNewWindow" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { DashboardCard } from '@/components/ui/dashboard-card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import MonitoringChart from '@/components/charts/MonitoringChart.vue'
import StatusIndicator from '@/components/charts/StatusIndicator.vue'
import StatusChart from '@/components/charts/StatusChart.vue'
import VideoModal from '@/components/video/VideoModal.vue'
import DashboardHeader from '@/components/layout/DashboardHeader.vue'

// Lucide Vue Next Icons
import {
  BarChart3,
  Car,
  Users,
  Video,
  ParkingCircle,
  Shield,
  Play as PlayIcon
} from 'lucide-vue-next'

// Utils
import { isYouTubeUrl, processYouTubeUrl } from '@/utils/statusUtils'

// Composables
import { useRealTimeData } from '@/composables/useRealTimeData'

// Services
import {
  DeviceService,
  DeviceStatusResponse,
  DeviceListResponse,
  ParkingOverviewService,
  CrowdOverviewService,
  TrafficOverviewService,
  FenceOverviewService
} from '@/services'

// Use real-time data composable
const {
  state,
  selectedStation,
  stations,
  parkingHotspots,
  crowdHotspots,
  trafficHotspots,
  fenceEvents,
  highResolutionDevices,
  loadStations,
  loadDashboardData,
  onStationChange
} = useRealTimeData()

// Device status state
const allDevicesStatus = ref<DeviceStatusResponse[]>([])
const allDevices = ref<DeviceListResponse[]>([])

// Station-based current status data for flow charts
const parkingStationData = ref<any[]>([])
const crowdStationData = ref<any[]>([])
const trafficStationData = ref<any[]>([])
const fenceStationData = ref<any[]>([])

// Types
interface PieChartData {
  name: string
  value: number
}

interface LegendItem {
  color: string
  label: string
}

interface MonitoringData {
  data: PieChartData[]
  legend: LegendItem[]
}

interface DeviceStatusItem {
  label: string
  percentage: number
  color: string
  bgColor: string
  status: string
}

interface Location {
  name: string
  url: string
  videoUrl?: string
}

// 停車場監控數據 - 直接從 overview service 獲取
const parkingMonitoringData = ref<MonitoringData>({
  data: [],
  legend: []
})

// 人流監控數據 - 直接從 overview service 獲取
const crowdMonitoringData = ref<MonitoringData>({
  data: [],
  legend: []
})

// 車流監控數據 - 直接從 overview service 獲取
const trafficMonitoringData = ref<MonitoringData>({
  data: [],
  legend: []
})

// 圍籬監控數據 - 直接從 overview service 獲取
const fenceMonitoringData = ref<MonitoringData>({
  data: [],
  legend: []
})

// 設備狀態數據
const deviceStatusData = computed<DeviceStatusItem[]>(() => {
  if (allDevicesStatus.value.length === 0) {
    return [
      {
        label: '停車場車位辨識',
        percentage: 0,
        status: '無數據',
        color: '#6b7280',
        bgColor: '#f3f4f6'
      },
      {
        label: '人流辨識',
        percentage: 0,
        status: '無數據',
        color: '#6b7280',
        bgColor: '#f3f4f6'
      },
      {
        label: '車流辨識',
        percentage: 0,
        status: '無數據',
        color: '#6b7280',
        bgColor: '#f3f4f6'
      },
      {
        label: '電子圍籬',
        percentage: 0,
        status: '無數據',
        color: '#6b7280',
        bgColor: '#f3f4f6'
      }
    ]
  }

  // 根據設備類型分組計算在線率
  const deviceTypeMap = {
    'parking': '停車場車位辨識',
    'crowd': '人流辨識',
    'traffic': '車流辨識',
    'fence': '電子圍籬'
  }

  const getColorByRate = (rate: number) => {
    if (rate >= 90) return { color: '#10b981', bgColor: '#dcfce7' }
    if (rate >= 70) return { color: '#f59e0b', bgColor: '#fef3c7' }
    return { color: '#ef4444', bgColor: '#fee2e2' }
  }

  const getStatusByRate = (rate: number) => {
    if (rate >= 90) return '優良'
    if (rate >= 70) return '正常'
    if (rate === 0) return '無數據'
    return '異常'
  }

  const calculateDeviceTypeRate = (deviceType: string) => {
    const devicesOfType = allDevicesStatus.value.filter(device => device.type === deviceType)
    if (devicesOfType.length === 0) return 0

    const onlineDevices = devicesOfType.filter(device => device.status === 'online').length
    return Math.round((onlineDevices / devicesOfType.length) * 100)
  }

  return Object.entries(deviceTypeMap).map(([type, label]) => {
    const percentage = calculateDeviceTypeRate(type)
    return {
      label,
      percentage,
      status: getStatusByRate(percentage),
      ...getColorByRate(percentage)
    }
  })
})


// 載入監控數據 - 直接從 overview service 獲取
const loadMonitoringData = async () => {
  try {
    const stationId = selectedStation.value && selectedStation.value !== 'all'
      ? parseInt(selectedStation.value)
      : undefined

    // 載入停車場監控數據
    const parkingResponse = await ParkingOverviewService.getRecentParkingRate(stationId, 3600, 50)
    if (parkingResponse.data && parkingResponse?.data?.length > 0) {
      const total = parkingResponse.data.length
      const highUsage = parkingResponse.data.filter(item => (item.rate || item.averageOccupancyRate || 0) >= 80).length
      const mediumUsage = parkingResponse.data.filter(item => {
        const rate = item.rate || item.averageOccupancyRate || 0
        return rate >= 50 && rate < 80
      }).length
      const lowUsage = total - highUsage - mediumUsage

      parkingMonitoringData.value = {
        data: [
          { name: '高使用率', value: highUsage },
          { name: '中使用率', value: mediumUsage },
          { name: '低使用率', value: lowUsage }
        ],
        legend: [
          { color: '#ff4a6b', label: `高使用率 ${Math.round((highUsage / total) * 100)}%` },
          { color: '#ffa500', label: `中使用率 ${Math.round((mediumUsage / total) * 100)}%` },
          { color: '#00e872', label: `低使用率 ${Math.round((lowUsage / total) * 100)}%` }
        ]
      }
    }

    // 載入人流監控數據
    const crowdResponse = await CrowdOverviewService.getRecentCapacityRate(stationId, 3600, 50)
    if (crowdResponse.data && crowdResponse.data.length > 0) {
      const total = crowdResponse.data.length
      const crowded = crowdResponse.data.filter(item => {
        const rate = item.rate || (item.averageDensity ? item.averageDensity * 100 : 0) || 0
        return rate >= 70
      }).length
      const moderate = crowdResponse.data.filter(item => {
        const rate = item.rate || (item.averageDensity ? item.averageDensity * 100 : 0) || 0
        return rate >= 30 && rate < 70
      }).length
      const sparse = total - crowded - moderate

      crowdMonitoringData.value = {
        data: [
          { name: '擁擠', value: crowded },
          { name: '適中', value: moderate },
          { name: '空曠', value: sparse }
        ],
        legend: [
          { color: '#ff4a6b', label: `擁擠 ${Math.round((crowded / total) * 100)}%` },
          { color: '#ffa500', label: `適中 ${Math.round((moderate / total) * 100)}%` },
          { color: '#00e872', label: `空曠 ${Math.round((sparse / total) * 100)}%` }
        ]
      }
    }

    // 載入車流監控數據
    const trafficResponse = await TrafficOverviewService.getRecentRoadCondition(stationId, 3600, 50)
    if (trafficResponse.data && trafficResponse.data.length > 0) {
      const total = trafficResponse.data.length
      const smooth = trafficResponse.data.filter(item => (item.rate || item.averageSpeed || 0) >= 60).length
      const moderate = trafficResponse.data.filter(item => {
        const rate = item.rate || item.averageSpeed || 0
        return rate >= 30 && rate < 60
      }).length
      const congested = total - smooth - moderate

      trafficMonitoringData.value = {
        data: [
          { name: '通暢', value: smooth },
          { name: '緩慢', value: moderate },
          { name: '擁堵', value: congested }
        ],
        legend: [
          { color: '#00e872', label: `通暢 ${Math.round((smooth / total) * 100)}%` },
          { color: '#ffa500', label: `緩慢 ${Math.round((moderate / total) * 100)}%` },
          { color: '#ff4a6b', label: `擁堵 ${Math.round((congested / total) * 100)}%` }
        ]
      }
    }

    // 載入圍籬監控數據
    const fenceResponse = await FenceOverviewService.getRecentRecord(stationId, 3600, 50)
    if (fenceResponse.data && fenceResponse.data.length > 0) {
      const total = fenceResponse.data.length
      const critical = fenceResponse.data.filter(event =>
        event.event?.includes('入侵') || event.event?.includes('異常')
      ).length
      const normal = total - critical

      fenceMonitoringData.value = {
        data: [
          { name: '正常事件', value: normal },
          { name: '重要事件', value: critical }
        ],
        legend: [
          { color: '#00e872', label: `正常事件 ${Math.round((normal / total) * 100)}%` },
          { color: '#ff4a6b', label: `重要事件 ${Math.round((critical / total) * 100)}%` }
        ]
      }
    } else {
      // 沒有事件時顯示安全狀態
      fenceMonitoringData.value = {
        data: [
          { name: '安全', value: 1 }
        ],
        legend: [
          { color: '#00e872', label: '安全 100%' }
        ]
      }
    }

    console.log('監控數據載入完成')
  } catch (error) {
    console.error('載入監控數據失敗:', error)
  }
}

// Load station-based current status data for flow charts
const loadStationChartData = async () => {
  try {
    // Get all stations for chart x-axis
    const allStations = stations.value

    // Load parking station data - current usage rate by station
    const parkingStationPromises = allStations.map(async (station) => {
      try {
        const response = await ParkingOverviewService.getRecentParkingRate(station.id, 3600, 1) // Last hour, latest only
        const avgRate = response.data && response.data.length > 0
          ? response.data.reduce((sum, item) => sum + (item.rate || item.averageOccupancyRate || 0), 0) / response.data.length
          : 0
        return {
          station: station.name, // Using station name as x-axis
          value: Math.round(avgRate)
        }
      } catch (error) {
        console.warn(`載入分站 ${station.name} 停車場數據失敗:`, error)
        return {
          station: station.name,
          value: 0
        }
      }
    })

    // Load crowd station data - current density by station
    const crowdStationPromises = allStations.map(async (station) => {
      try {
        const response = await CrowdOverviewService.getRecentCapacityRate(station.id, 3600, 1)
        const avgRate = response.data && response.data.length > 0
          ? response.data.reduce((sum, item) => sum + (item.rate || (item.averageDensity ? item.averageDensity * 100 : 0) || 0), 0) / response.data.length
          : 0
        return {
          station: station.name,
          value: Math.round(avgRate)
        }
      } catch (error) {
        console.warn(`載入分站 ${station.name} 人流數據失敗:`, error)
        return {
          station: station.name,
          value: 0
        }
      }
    })

    // Load traffic station data - current road condition by station
    const trafficStationPromises = allStations.map(async (station) => {
      try {
        const response = await TrafficOverviewService.getRecentRoadCondition(station.id, 3600, 1)
        const avgRate = response.data && response.data.length > 0
          ? response.data.reduce((sum, item) => sum + (item.rate || item.averageSpeed || 0), 0) / response.data.length
          : 0
        return {
          station: station.name,
          value: Math.round(avgRate)
        }
      } catch (error) {
        console.warn(`載入分站 ${station.name} 車流數據失敗:`, error)
        return {
          station: station.name,
          value: 0
        }
      }
    })

    // Load fence station data - current fence status by station
    const fenceStationPromises = allStations.map(async (station) => {
      try {
        const response = await FenceOverviewService.getRecentRecord(station.id, 3600, 10) // Last hour, up to 10 events
        // Calculate status based on recent events
        let statusValue = 0 // Default: no events (safe)
        if (response.data && response.data.length > 0) {
          const criticalEvents = response.data.filter(event =>
            event.event?.includes('入侵') || event.event?.includes('異常')
          ).length
          const totalEvents = response.data.length

          if (criticalEvents > 0) {
            statusValue = Math.min(80, (criticalEvents / totalEvents) * 100) // Critical events: up to 80%
          } else if (totalEvents > 0) {
            statusValue = 20 // Normal events: 20%
          }
        }
        return {
          station: station.name,
          value: Math.round(statusValue)
        }
      } catch (error) {
        console.warn(`載入分站 ${station.name} 圍籬數據失敗:`, error)
        return {
          station: station.name,
          value: 0
        }
      }
    })

    // Execute all promises and update data
    const [parkingResults, crowdResults, trafficResults, fenceResults] = await Promise.all([
      Promise.all(parkingStationPromises),
      Promise.all(crowdStationPromises),
      Promise.all(trafficStationPromises),
      Promise.all(fenceStationPromises)
    ])

    parkingStationData.value = parkingResults
    crowdStationData.value = crowdResults
    trafficStationData.value = trafficResults
    fenceStationData.value = fenceResults

    console.log('分站圖表數據載入完成:', {
      parking: parkingStationData.value.length,
      crowd: crowdStationData.value.length,
      traffic: trafficStationData.value.length,
      fence: fenceStationData.value.length
    })

  } catch (error) {
    console.error('載入分站圖表數據失敗:', error)
    // Set empty arrays on error
    parkingStationData.value = []
    crowdStationData.value = []
    trafficStationData.value = []
    fenceStationData.value = []
  }
}

// 即時影像位置 - 使用設備名稱獲取數據
const locations = computed<Location[]>(() => {
  // 指定的設備名稱配置
  const deviceConfig = [
    {
      deviceName: '中角灣',
      fallbackImage: '/images/livestream/center-beach.jpg'
    },
    {
      deviceName: '硬漢嶺',
      fallbackImage: '/images/livestream/concert-mountain.jpg'
    },
    {
      deviceName: '綠石槽',
      fallbackImage: '/images/livestream/old-mei-green-rock.jpg'
    },
    {
      deviceName: '和平島',
      fallbackImage: '/images/livestream/peace-island-park.jpg'
    },
    {
      deviceName: '白沙灣',
      fallbackImage: '/images/livestream/white-sand-beach.jpg'
    },
    {
      deviceName: '野柳',
      fallbackImage: '/images/livestream/wild-grass-park.jpg'
    },
    {
      deviceName: '仙履橋',
      fallbackImage: '/images/livestream/yilan-bridge.jpg'
    },
    {
      deviceName: '仙履橋-2',
      fallbackImage: '/images/livestream/yilan-bridge.jpg'
    }
  ]

  // 從所有設備數據中查找相同名稱的設備
  return deviceConfig.map(config => {
    const device = allDevices.value.find(d =>
      d.name === config.deviceName
    )

    return {
      name: config.deviceName,
      url: config.fallbackImage,
      videoUrl: device?.videoUrl || undefined
    }
  })
})

// Video modal state
const showVideoModal = ref(false)
const selectedLocation = ref<Location | null>(null)
const selectedDevice = ref<any>(null)

// Check if location has streaming capability (video URL or GetAIJpeg API)
const hasStreamCapability = (location: Location): boolean => {
  if (location.videoUrl) return true

  // Check if this device has GetAIJpeg API URL
  const device = allDevices.value.find(d => d.name === location.name)
  return !!device?.apiUrl && device.apiUrl.includes('GetAIJpeg')
}

// Open video modal
const openVideoModal = (location: Location) => {
  // Find the corresponding device
  const device = allDevices.value.find(d => d.name === location.name)

  // Always open modal, let VideoModal handle different stream types
  selectedLocation.value = location
  selectedDevice.value = device || null
  showVideoModal.value = true
}

// Handle opening video in new window (from VideoModal component)
const handleOpenVideoInNewWindow = (location: Location) => {
  // Find the corresponding device to check if it has GetAIJpeg API
  const device = allDevices.value.find(d => d.name === location.name)

  // Check if this device has GetAIJpeg API URL
  if (device?.apiUrl && device.apiUrl.includes('GetAIJpeg') && !device?.videoUrl) {
    // Open crowd stream viewer for API-based crowd devices
    const params = new URLSearchParams({
      deviceId: device.id?.toString() || '',
      deviceType: device.type || 'crowd',
      deviceName: device.name || location.name,
      channelId: 'cam1'
    })

    const streamUrl = `/crowd-stream?${params.toString()}`
    window.open(streamUrl, '_blank', 'width=1200,height=800,resizable=yes,scrollbars=yes')
  } else if (location?.videoUrl) {
    // Handle traditional video streams
    const videoUrl = isYouTubeUrl(location.videoUrl)
      ? processYouTubeUrl(location.videoUrl)
      : location.videoUrl
    window.open(videoUrl, '_blank', 'width=1280,height=720,scrollbars=yes,resizable=yes')
  }
}

// Load device data
const loadDevices = async () => {
  try {
    allDevices.value = await DeviceService.getDevices()
  } catch (error) {
    console.error('載入設備數據失敗:', error)
    allDevices.value = []
  }
}

// Load device status data
const loadDeviceStatus = async () => {
  try {
    allDevicesStatus.value = await DeviceService.getDevicesStatus()
  } catch (error) {
    console.error('載入設備狀態失敗:', error)
    allDevicesStatus.value = []
  }
}

// 生命週期
onMounted(async () => {
  await loadStations()
  await loadDashboardData()
  await loadDevices()
  await loadDeviceStatus()
  await loadStationChartData()
  await loadMonitoringData()
})
</script>