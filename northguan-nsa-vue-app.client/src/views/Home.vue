<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Use AppHeader -->
    <AppHeader />

    <!-- Main Content -->
    <div class="container mx-auto px-4 py-6 space-y-6">
      <!-- Page Header -->
      <div class="flex flex-col gap-4">
        <div>
          <h1 class="text-2xl sm:text-3xl font-bold text-gray-900">設備即時監控</h1>
          <p class="text-gray-600 mt-1">即時監控系統狀態與事件</p>
        </div>

        <div class="flex flex-col sm:flex-row sm:items-center gap-3">
          <Select v-model="selectedStation" @update:modelValue="onStationChange">
            <SelectTrigger class="w-full sm:w-48">
              <SelectValue placeholder="選擇站點" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">全部站點</SelectItem>
              <SelectItem v-for="station in stations" :key="station.id" :value="station.id.toString()">
                {{ station.name }}
              </SelectItem>
            </SelectContent>
          </Select>

          <div class="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
            <!-- 更新間隔選擇 -->
            <Select :model-value="state.updateInterval.toString()"
              @update:modelValue="(value) => setUpdateInterval(parseInt(value))">
              <SelectTrigger class="w-full sm:w-32">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="0">永不刷新</SelectItem>
                <SelectItem value="5000">5秒</SelectItem>
                <SelectItem value="10000">10秒</SelectItem>
                <SelectItem value="30000">30秒</SelectItem>
                <SelectItem value="60000">1分鐘</SelectItem>
                <SelectItem value="300000">5分鐘</SelectItem>
              </SelectContent>
            </Select>

            <Button @click="refreshAllData" :disabled="loading" variant="outline" class="w-full sm:w-auto">
              <RefreshCw :class="['h-4 w-4 mr-2', { 'animate-spin': loading }]" />
              重新整理
            </Button>
          </div>

          <!-- 最後更新時間 -->
          <div class="text-xs text-gray-500 text-center sm:text-left">
            <div>最後更新</div>
            <div>{{ state.lastUpdateTime || '尚未更新' }}</div>
          </div>
        </div>
      </div>

      <!-- Statistics Overview Cards -->
      <!-- <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">總設備數</p>
              <p class="text-2xl font-bold text-gray-900">{{ totalDevices }}</p>
            </div>
            <div class="h-12 w-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <Monitor class="h-6 w-6 text-blue-600" />
            </div>
          </div>
        </Card>

        <Card class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">今日事件</p>
              <p class="text-2xl font-bold text-gray-900">{{ todayEvents }}</p>
            </div>
            <div class="h-12 w-12 bg-red-100 rounded-lg flex items-center justify-center">
              <AlertTriangle class="h-6 w-6 text-red-600" />
            </div>
          </div>
        </Card>

        <Card class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">平均使用率</p>
              <p class="text-2xl font-bold text-gray-900">{{ averageUsage }}%</p>
            </div>
            <div class="h-12 w-12 bg-green-100 rounded-lg flex items-center justify-center">
              <TrendingUp class="h-6 w-6 text-green-600" />
            </div>
          </div>
        </Card>

        <Card class="p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">系統狀態</p>
              <p :class="`text-2xl font-bold text-${systemStatus.color}`">{{ systemStatus.status }}</p>
            </div>
            <div :class="`h-12 w-12 bg-${systemStatus.bgColor}-100 rounded-lg flex items-center justify-center`">
              <CheckCircle v-if="systemStatus.status === '正常'" :class="`h-6 w-6 text-${systemStatus.color}`" />
              <AlertTriangle v-else-if="systemStatus.status === '警告' || systemStatus.status === '注意'" :class="`h-6 w-6 text-${systemStatus.color}`" />
              <AlertCircle v-else :class="`h-6 w-6 text-${systemStatus.color}`" />
            </div>
          </div>
        </Card>
      </div> -->

      <!-- Map Section -->
      <HomeMap ref="homeMapRef" :parking-hotspots="parkingHotspots" :crowd-hotspots="crowdHotspots"
        :traffic-hotspots="trafficHotspots" :fence-events="fenceEvents" :high-resolution-devices="highResolutionDevices"
        :selected-station="selectedStation" />


      <!-- Quick Access Section -->
      <QuickAccessGrid />

      <!-- Dashboard Section -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <!-- Parking Hotspots -->
        <HotspotCard title="停車場熱點" :items="parkingHotspots" :loading="loading" type="parking" :icon="Car"
          @item-click="handleDeviceClick" />

        <!-- Crowd Hotspots -->
        <HotspotCard title="人流熱點" :items="crowdHotspots" :loading="loading" type="crowd" :icon="Users"
          @item-click="handleDeviceClick" />

        <!-- Traffic Hotspots -->
        <HotspotCard title="路段熱點" :items="trafficHotspots" :loading="loading" type="traffic" :icon="Navigation"
          @item-click="handleDeviceClick" />

        <!-- Fence Events -->
        <FenceEventsCard :events="fenceEvents" :loading="loading" @event-click="showFenceEventDetail" />
      </div>

      <!-- Fence Event Modal -->
      <FenceEventModal :show-fence-modal="showFenceModal" :selected-fence-event="selectedFenceEvent"
        @close="closeFenceModal" />

      <!-- Video Modal -->
      <div v-if="showVideoModalState" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-75"
        @click="closeVideoModal">
        <div class="relative max-w-4xl w-full mx-4" @click.stop>
          <button class="absolute -top-10 right-0 text-white text-2xl hover:text-gray-300 transition-colors"
            @click="closeVideoModal">
            ✕
          </button>
          <div class="bg-white rounded-lg overflow-hidden">
            <div class="p-4 border-b">
              <h3 class="text-lg font-semibold">{{ currentVideoDevice?.deviceName }} - 即時影像</h3>
            </div>
            <div class="aspect-video">
              <iframe v-if="processedVideoUrl.includes('youtube') || processedVideoUrl.includes('embed')"
                :src="processedVideoUrl" class="w-full h-full" frameborder="0" allowfullscreen
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                referrerpolicy="strict-origin-when-cross-origin"
                sandbox="allow-scripts allow-same-origin allow-presentation"></iframe>
              <video v-else :src="processedVideoUrl" class="w-full h-full" controls autoplay></video>
            </div>
          </div>
        </div>
      </div>

      <!-- Fence Image Modal -->
      <div v-if="showFenceImageModalState"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-75"
        @click="closeFenceImageModal">
        <div class="relative max-w-2xl w-full mx-4" @click.stop>
          <button class="absolute -top-10 right-0 text-white text-2xl hover:text-gray-300 transition-colors"
            @click="closeFenceImageModal">
            ✕
          </button>
          <div class="bg-white rounded-lg overflow-hidden">
            <div class="p-4 border-b">
              <h3 class="text-lg font-semibold">{{ currentFenceEvent?.deviceName }} - 圍籬事件</h3>
              <p class="text-sm text-gray-600">{{ currentFenceEvent?.date }} {{ currentFenceEvent?.time }} - {{
                currentFenceEvent?.event }}</p>
            </div>
            <div class="p-4">
              <img :src="currentFenceEvent?.imageUrl" alt="圍籬事件圖片" class="w-full h-auto rounded"
                @error="$event.target.src = '/images/image-placeholder.png'" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  RefreshCw, Car, Users, Navigation, AlertTriangle, Info, AlertCircle
} from 'lucide-vue-next'

// Components
import AppHeader from '@/components/layout/AppHeader.vue'
import HomeMap from '@/components/home/HomeMap.vue'
import QuickAccessGrid from '@/components/home/QuickAccessGrid.vue'
import HotspotCard from '@/components/home/HotspotCard.vue'
import FenceEventsCard from '@/components/home/FenceEventsCard.vue'
import FenceEventModal from '@/components/home/FenceEventModal.vue'

// Composables
import { useRealTimeData } from '@/composables/useRealTimeData'
import { useFenceModal } from '@/composables/useFenceModal'

// Utils
import { CHART_COLORS } from '@/utils/chartUtils'
import { useToast } from '@/composables/useToast'
import type { ParkingDevice, CrowdDevice, TrafficDevice, FenceDevice, HighResolutionDevice } from '@/composables/useRealTimeData'
import type { EChartsOption } from 'echarts'

// Utils
import { processYouTubeUrl } from '@/utils/statusUtils'

// Use composables
const route = useRoute()
const router = useRouter()
const toast = useToast()
const {
  state,
  selectedStation,
  stations,
  parkingHotspots,
  crowdHotspots,
  trafficHotspots,
  fenceEvents,
  highResolutionDevices,
  parkingHistory,
  crowdHistory,
  trafficHistory,
  fenceHistory,
  loadStations,
  loadDashboardData,
  loadDeviceHistory,
  clearHistoryData,
  refreshData,
  onStationChange,
  toggleAutoRefresh,
  setUpdateInterval
} = useRealTimeData()

// 為了向後兼容，創建別名
const loading = computed(() => state.value.loading)

// 初始載入狀態（只在首次載入時顯示 skeleton）
const initialLoading = ref(true)
const chartsLoading = ref(true)

const {
  showFenceModal,
  selectedFenceEvent,
  showFenceEventDetail,
  closeFenceModal
} = useFenceModal()

// Refs
const homeMapRef = ref<InstanceType<typeof HomeMap>>()

// Statistics data
const totalDevices = computed(() => {
  return parkingHotspots.value.length + crowdHotspots.value.length + trafficHotspots.value.length + highResolutionDevices.value.length + fenceEvents.value.length
})

const onlineDevices = computed(() => {
  const allDevices = [...parkingHotspots.value, ...crowdHotspots.value, ...trafficHotspots.value, ...highResolutionDevices.value]
  return allDevices.filter(device => device.status === 'online').length
})

const todayEvents = computed(() => {
  return fenceEvents.value.length
})

const criticalEvents = computed(() => {
  return fenceEvents.value.filter(event =>
    event.event.includes('入侵') || event.event.includes('異常')
  ).length
})

const averageUsage = computed(() => {
  const allDevices = [...parkingHotspots.value, ...crowdHotspots.value, ...trafficHotspots.value]
  if (allDevices.length === 0) return 0
  const totalRate = allDevices.reduce((sum, device) => sum + (device.rate || 0), 0)
  return Math.round(totalRate / allDevices.length)
})

// 系統狀態計算
const systemStatus = computed(() => {
  const allDevices = [...parkingHotspots.value, ...crowdHotspots.value, ...trafficHotspots.value, ...highResolutionDevices.value]
  if (allDevices.length === 0) return { status: '未知', color: 'gray', bgColor: 'gray' }

  const offlineCount = allDevices.filter(device => device.status === 'offline').length
  const errorCount = allDevices.filter(device => device.status === 'error').length
  const warningCount = allDevices.filter(device => device.status === 'warning').length
  const criticalFenceEvents = criticalEvents.value

  // 根據問題設備比例和嚴重事件判斷系統狀態
  const totalDeviceCount = allDevices.length
  const problemRatio = (offlineCount + errorCount) / totalDeviceCount

  if (errorCount > 0 || criticalFenceEvents > 3) {
    return { status: '異常', color: 'red-600', bgColor: 'red' }
  } else if (problemRatio > 0.3 || warningCount > totalDeviceCount * 0.5 || criticalFenceEvents > 1) {
    return { status: '警告', color: 'yellow-600', bgColor: 'yellow' }
  } else if (problemRatio > 0.1 || warningCount > 0) {
    return { status: '注意', color: 'blue-600', bgColor: 'blue' }
  } else {
    return { status: '正常', color: 'green-600', bgColor: 'green' }
  }
})

const usageTrend = ref(5.2) // Mock data for trend

// Chart options
const realTimeChartOption = computed<EChartsOption>(() => {
  // 合併所有時間標籤
  const allTimes = new Set<string>()

  // 從停車場歷史數據提取時間
  parkingHistory.value.forEach(history => {
    history.records.forEach(record => {
      allTimes.add(`${record.date} ${record.time}`)
    })
  })

  // 從人流歷史數據提取時間
  crowdHistory.value.forEach(history => {
    history.records.forEach(record => {
      allTimes.add(`${record.date} ${record.time}`)
    })
  })

  // 從交通歷史數據提取時間
  trafficHistory.value.forEach(history => {
    history.records.forEach(record => {
      allTimes.add(record.time)
    })
  })

  const labels = Array.from(allTimes).sort().slice(-24) // 取最近24個時間點

  const series: any[] = []
  let colorCounter = 0
  const colors = CHART_COLORS

  // 停車場數據系列
  parkingHistory.value.forEach(history => {
    const timeValueMap: Record<string, number> = {}
    history.records.forEach(record => {
      timeValueMap[`${record.date} ${record.time}`] = record.conversionRate
    })

    const data = labels.map(label => timeValueMap[label] ?? null)

    series.push({
      name: `停車-${history.deviceName}`,
      type: 'line',
      smooth: true,
      data,
      itemStyle: { color: colors[colorCounter % colors.length] },
      lineStyle: { color: colors[colorCounter % colors.length] }
    })
    colorCounter++
  })

  // 人流數據系列
  const conditionMap = { '空曠': 0, '稍擠': 50, '擁擠': 100 }
  crowdHistory.value.forEach(history => {
    const timeValueMap: Record<string, number> = {}
    history.records.forEach(record => {
      timeValueMap[`${record.date} ${record.time}`] = conditionMap[record.status as keyof typeof conditionMap] ?? 0
    })

    const data = labels.map(label => timeValueMap[label] ?? null)

    series.push({
      name: `人流-${history.deviceName}`,
      type: 'line',
      smooth: true,
      data,
      itemStyle: { color: colors[colorCounter % colors.length] },
      lineStyle: { color: colors[colorCounter % colors.length] }
    })
    colorCounter++
  })

  // 交通數據系列
  const trafficConditionMap = { '暢通': 0, '稍多': 50, '壅塞': 100 }
  trafficHistory.value.forEach(history => {
    const timeValueMap: Record<string, number> = {}
    history.records.forEach(record => {
      timeValueMap[record.time] = trafficConditionMap[record.status as keyof typeof trafficConditionMap] ?? 0
    })

    const data = labels.map(label => timeValueMap[label] ?? null)

    series.push({
      name: `交通-${history.deviceName}`,
      type: 'line',
      smooth: true,
      data,
      itemStyle: { color: colors[colorCounter % colors.length] },
      lineStyle: { color: colors[colorCounter % colors.length] }
    })
    colorCounter++
  })

  return {
    title: {
      text: '歷史數據趨勢',
      textStyle: { fontSize: 14, fontWeight: 'normal' }
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'cross' },
      formatter: (params: any) => {
        let result = `<div><strong>${params[0].axisValue}</strong></div>`
        params.forEach((param: any) => {
          const value = param.value !== null ? `${param.value}%` : '無數據'
          result += `<div style="color: ${param.color}">● ${param.seriesName}: ${value}</div>`
        })
        return result
      }
    },
    legend: {
      data: series.map(s => s.name),
      bottom: 0,
      type: 'scroll'
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '15%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: labels.map(label => {
        const date = new Date(label)
        return date.toLocaleString('zh-TW', { timeZone: 'Asia/Taipei', year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
      })
    },
    yAxis: {
      type: 'value',
      name: '數值',
      axisLabel: {
        formatter: '{value}%'
      }
    },
    series: series.map(s => ({ ...s, hidden: true })) // 預設隱藏，讓用戶選擇要顯示的線
  }
})

const deviceStatusChartOption = computed<EChartsOption>(() => ({
  title: {
    text: '設備狀態分布',
    textStyle: { fontSize: 14, fontWeight: 'normal' }
  },
  tooltip: {
    trigger: 'item',
    formatter: '{a} <br/>{b}: {c} ({d}%)'
  },
  legend: {
    orient: 'vertical',
    left: 'left'
  },
  series: [
    {
      name: '設備狀態',
      type: 'pie',
      radius: ['40%', '70%'],
      center: ['60%', '50%'],
      data: [
        { value: onlineDevices.value, name: '在線', itemStyle: { color: '#10b981' } },
        { value: totalDevices.value - onlineDevices.value, name: '離線', itemStyle: { color: '#ef4444' } }
      ],
      emphasis: {
        itemStyle: {
          shadowBlur: 10,
          shadowOffsetX: 0,
          shadowColor: 'rgba(0, 0, 0, 0.5)'
        }
      }
    }
  ]
}))

const usageComparisonChartOption = computed<EChartsOption>(() => ({
  title: {
    text: '各類型設備使用率對比',
    textStyle: { fontSize: 14, fontWeight: 'normal' }
  },
  tooltip: {
    trigger: 'axis',
    axisPointer: { type: 'shadow' }
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '3%',
    containLabel: true
  },
  xAxis: {
    type: 'category',
    data: ['停車場', '人流監控', '交通監控']
  },
  yAxis: {
    type: 'value',
    name: '平均使用率 (%)'
  },
  series: [
    {
      name: '使用率',
      type: 'bar',
      data: [
        {
          value: parkingHotspots.value.length > 0
            ? Math.round(parkingHotspots.value.reduce((sum, item) => sum + item.rate, 0) / parkingHotspots.value.length)
            : 0,
          itemStyle: { color: '#3b82f6' }
        },
        {
          value: crowdHotspots.value.length > 0
            ? Math.round(crowdHotspots.value.reduce((sum, item) => sum + item.rate, 0) / crowdHotspots.value.length)
            : 0,
          itemStyle: { color: '#10b981' }
        },
        {
          value: trafficHotspots.value.length > 0
            ? Math.round(trafficHotspots.value.reduce((sum, item) => sum + item.rate, 0) / trafficHotspots.value.length)
            : 0,
          itemStyle: { color: '#f59e0b' }
        }
      ],
      barWidth: '60%'
    }
  ]
}))

const eventTimelineChartOption = computed<EChartsOption>(() => {
  // 統計圍籬事件的時間分布
  const hourlyEvents: Record<string, number> = {}

  fenceHistory.value.forEach(history => {
    history.records.forEach(record => {
      const dateTime = `${record.date} ${record.time}`
      const hour = new Date(dateTime).getHours()
      const hourKey = `${hour}:00`
      hourlyEvents[hourKey] = (hourlyEvents[hourKey] || 0) + record.recordNum
    })
  })

  // 生成24小時的標籤
  const hours = Array.from({ length: 24 }, (_, i) => `${i}:00`)
  const eventData = hours.map(hour => hourlyEvents[hour] || 0)

  return {
    title: {
      text: '圍籬事件時間分布',
      textStyle: { fontSize: 14, fontWeight: 'normal' }
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      formatter: (params: any) => {
        const param = params[0]
        return `<div><strong>${param.axisValue}</strong></div><div>事件數量: ${param.value}</div>`
      }
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: hours
    },
    yAxis: {
      type: 'value',
      name: '事件數量',
      minInterval: 1
    },
    series: [
      {
        name: '事件數量',
        type: 'bar',
        data: eventData,
        itemStyle: {
          color: '#ef4444',
          borderRadius: [4, 4, 0, 0]
        },
        barWidth: '60%'
      }
    ]
  }
})

// Methods
const handleDeviceClick = (item: ParkingDevice | CrowdDevice | TrafficDevice) => {
  console.log('Device clicked:', item)

  // 滾動到地圖區域
  const mapElement = homeMapRef.value?.$el || document.querySelector('#main-map')?.parentElement
  if (mapElement) {
    mapElement.scrollIntoView({
      behavior: 'smooth',
      block: 'start',
      inline: 'nearest'
    })
  }

  // 通過地圖組件顯示設備詳情
  if (homeMapRef.value) {
    let deviceType: 'parking' | 'crowd' | 'traffic'

    // 根據設備數據判斷類型
    if ('parkingUsage' in item || parkingHotspots.value.includes(item)) {
      deviceType = 'parking'
    } else if ('peopleNum' in item || crowdHotspots.value.includes(item)) {
      deviceType = 'crowd'
    } else {
      deviceType = 'traffic'
    }

    // 延遲一點時間確保滾動完成後再顯示側邊欄
    setTimeout(() => {
      // 調用地圖組件的 showDeviceDetail 方法
      homeMapRef.value.showDeviceDetail(deviceType, item)
    }, 500)
  }
}

const getActivityIcon = (type: string) => {
  switch (type) {
    case 'warning':
      return AlertTriangle
    case 'error':
      return AlertCircle
    default:
      return Info
  }
}

const refreshAllData = async () => {
  await refreshData()
}


// 查看詳細資訊按鈕實現
const showDeviceDetail = (device: ParkingDevice | CrowdDevice | TrafficDevice, type: 'parking' | 'crowd' | 'traffic') => {
  if (homeMapRef.value) {
    // 調用地圖組件的 showDeviceDetail 方法
    const mapDevice = {
      id: device.id,
      name: device.deviceName,
      type,
      status: device.status,
      latitude: 0, // 會在地圖組件中設置正確的座標
      longitude: 0,
      rate: device.rate,
      peopleNum: 'peopleNum' in device ? device.peopleNum : undefined,
      speed: 'speed' in device ? device.speed : undefined,
      lastUpdateTime: device.lastUpdateTime
    }
    homeMapRef.value.showDeviceDetail(type, mapDevice)
  }
}

// 模態框狀態
const showVideoModalState = ref(false)
const showFenceImageModalState = ref(false)
const currentVideoDevice = ref<CrowdDevice | null>(null)
const currentFenceEvent = ref<FenceDevice | null>(null)
const processedVideoUrl = ref('')

const showVideoModal = (device: CrowdDevice) => {
  if (device.videoUrl) {
    currentVideoDevice.value = device

    // 使用統一的YouTube URL處理函數
    processedVideoUrl.value = processYouTubeUrl(device.videoUrl)
    showVideoModalState.value = true
  }
}

const closeVideoModal = () => {
  showVideoModalState.value = false
  currentVideoDevice.value = null
  processedVideoUrl.value = ''
}

const showFenceImageModal = (event: FenceDevice) => {
  if (event.imageUrl) {
    currentFenceEvent.value = event
    showFenceImageModalState.value = true
  }
}

const closeFenceImageModal = () => {
  showFenceImageModalState.value = false
  currentFenceEvent.value = null
}

// Watch for station changes to update charts
watch(selectedStation, () => {
  // Charts data removed as per requirement
})

// 全局函數供地圖彈出窗口使用
const setupGlobalFunctions = () => {
  (window as any).showDeviceDetailFromMap = (type: string, deviceId: string) => {
    let device: ParkingDevice | CrowdDevice | TrafficDevice | undefined

    switch (type) {
      case 'parking':
        device = parkingHotspots.value.find(d => d.id.toString() === deviceId)
        if (device) {
          showDeviceDetail(device, 'parking')
        }
        break
      case 'crowd':
        device = crowdHotspots.value.find(d => d.id.toString() === deviceId)
        if (device) {
          const crowdDevice = device as CrowdDevice
          if (crowdDevice.videoUrl) {
            showVideoModal(crowdDevice)
          } else {
            showDeviceDetail(device, 'crowd')
          }
        }
        break
      case 'traffic':
        device = trafficDevices.value.find(d => d.id.toString() === deviceId)
        if (device) {
          showDeviceDetail(device, 'traffic')
        }
        break
      case 'highResolution':
        const hrDevice = highResolutionDevices.value.find(d => d.id.toString() === deviceId)
        if (hrDevice) {
          // 顯示4K影像設備詳情在sidebar中
          if (homeMapRef.value) {
            const mapDevice = {
              id: hrDevice.id,
              name: hrDevice.deviceName,
              type: 'highResolution' as const,
              status: hrDevice.status,
              latitude: hrDevice.latitude,
              longitude: hrDevice.longitude,
              lastUpdateTime: hrDevice.lastUpdateTime
            }
            homeMapRef.value.showDeviceDetail('highResolution', mapDevice)
          }
        }
        break
    }
  }

  (window as any).showFenceEventFromMap = (eventId: string) => {
    const event = fenceEvents.value.find(e => e.id.toString() === eventId)
    if (event) {
      showFenceImageModal(event)
    }
  }
}

// 清理全局函數
const cleanupGlobalFunctions = () => {
  delete (window as any).showDeviceDetailFromMap
  delete (window as any).showFenceEventFromMap
}

// Lifecycle
onMounted(async () => {
  // 檢查是否有權限錯誤
  if (route.query.error === 'access_denied') {
    toast.error('權限不足：您沒有權限訪問該頁面')
    // 清除 URL 中的錯誤參數
    const { error, ...query } = route.query
    await router.replace({ query })
  }

  setupGlobalFunctions()
  await loadStations()
  await loadDashboardData()
  initialLoading.value = false
})

onUnmounted(() => {
  cleanupGlobalFunctions()
})
</script>
