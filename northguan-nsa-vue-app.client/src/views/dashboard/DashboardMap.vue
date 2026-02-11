<template>
  <div class="h-screen bg-gradient-to-br from-sky-100 via-blue-100 to-slate-200 font-sans text-gray-800 flex flex-col">
    <!-- Header -->
    <DashboardHeader :current-view="currentView" />

    <main class="flex-1 flex flex-col md:flex-row overflow-hidden relative">
      <!-- Mobile Toggle Button -->
      <button @click="showMobileSidebar = !showMobileSidebar"
        class="md:hidden absolute top-4 right-4 z-2 bg-white rounded-lg shadow-lg p-2 border hover:bg-gray-50">
        <Menu v-if="!showMobileSidebar" class="w-5 h-5 text-gray-600" />
        <X v-else class="w-5 h-5 text-gray-600" />
      </button>

      <!-- Left Sidebar -->
      <aside :class="[
        'w-full md:w-96 flex-shrink-0 bg-white flex flex-col shadow-lg z-[1000]',
        'transition-transform duration-300 ease-in-out',
        showMobileSidebar ? 'translate-x-0' : '-translate-x-full md:translate-x-0',
        'fixed md:relative inset-y-0 left-0 md:inset-auto',
        'max-h-screen md:max-h-none'
      ]">
        <div :class="[getCurrentDeviceColors.bg, 'text-white p-4 flex items-center justify-between']">
          <div class="flex items-center space-x-3">
            <component :is="getCurrentViewIcon()" class="w-8 h-8" />
            <h2 class="text-xl font-bold">{{ getCurrentViewTitle() }}</h2>
          </div>
          <!-- Mobile Close Button -->
          <button @click="showMobileSidebar = false" class="md:hidden p-2 hover:bg-black hover:bg-opacity-20 rounded">
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="flex-1 flex items-center justify-center">
          <div class="text-center">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p class="text-gray-600">載入設備資料中...</p>
          </div>
        </div>

        <!-- Landmarks List -->
        <div v-else class="flex-grow overflow-y-auto custom-scrollbar">
          <ul class="divide-y divide-gray-200">
            <LandmarkListItem v-for="landmark in currentLandmarks" :key="landmark.id" :landmark="landmark"
              :view-type="currentView" :is-selected="selectedLandmark?.id === landmark.id"
              :device-colors="getCurrentDeviceColors" @click="handleSelectLandmark(landmark)"
              @hover="handleHoverLandmark(landmark)" />
          </ul>

          <!-- Empty State -->
          <div v-if="currentLandmarks.length === 0" class="p-8 text-center text-gray-500">
            <component :is="getCurrentViewIcon()" class="w-12 h-12 mx-auto mb-4 text-gray-300" />
            <p class="text-lg font-medium mb-2">目前沒有資料</p>
            <p class="text-sm">{{ getCurrentViewTitle() }}設備暫無資料顯示</p>
          </div>
        </div>
      </aside>

      <!-- Mobile Overlay -->
      <div v-if="showMobileSidebar" @click="showMobileSidebar = false"
        class="md:hidden fixed inset-0 bg-black bg-opacity-50 z-10"></div>

      <!-- Map Container -->
      <div class="flex-1 relative">
        <!-- Map -->
        <SmartTourismMap :view="currentView" :landmarks="currentLandmarks" :selected-landmark="selectedLandmark"
          :show-device-sidebar="showDeviceSidebar" @select-landmark="handleSelectLandmark"
          @deviceDetail="handleDeviceDetail" />

        <!-- Device Details Sidebar -->
        <DeviceSidebar :show-sidebar="showDeviceSidebar" :sidebar-loading="sidebarLoading" :sidebar-type="sidebarType"
          :sidebar-title="sidebarTitle" :sidebar-data="sidebarData" :selected-device-id="selectedDeviceId"
          :fence-image-url="fenceImageUrl" :chart-options="chartOptions" @close-sidebar="closeSidebar"
          @change-fence-image="changeFenceImage" @open-video="openVideoInNewWindow" />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Users, Car, ParkingCircle, Shield, Camera, Menu, X } from 'lucide-vue-next'
import type { ILandmarkItem, ICrowdLandmarkResponse, IFenceLandmarkResponse, IHighResolutionLandmarkResponse, IParkingLandmarkResponse, ITrafficLandmarkResponse } from '@/api/client'
import { useRealTimeData } from '@/composables/useRealTimeData'
import { MapService } from '@/services/MapService'
import type { EChartsOption } from 'echarts'
import LandmarkListItem from '@/components/dashboard/LandmarkListItem.vue'
import SmartTourismMap from '@/components/dashboard/DashboardMap.vue'
import DashboardHeader from '@/components/layout/DashboardHeader.vue'
import DeviceSidebar from '@/components/shared/DeviceSidebar.vue'
import { getDeviceColors, getDevicePrimaryColor, type DeviceType } from '@/config/deviceColors'

type ViewType = 'crowd' | 'traffic' | 'parking' | 'fence' | 'highResolution'
type SidebarData = IParkingLandmarkResponse | ICrowdLandmarkResponse | ITrafficLandmarkResponse | IFenceLandmarkResponse | IHighResolutionLandmarkResponse

const { loadMapLandmarks, loadDeviceHistory, clearHistoryData, parkingHistory, crowdHistory, trafficHistory, fenceHistory } = useRealTimeData()
const route = useRoute()
const router = useRouter()

// 獲取當前設備類型的顏色配置
const getCurrentDeviceColors = computed(() => {
  return getDeviceColors(currentView.value as DeviceType)
})

const currentView = ref<ViewType>('parking')
const selectedLandmark = ref<ILandmarkItem | null>(null)
const loading = ref(true)
const showMobileSidebar = ref(false)

// Device sidebar state
const showDeviceSidebar = ref(false)
const sidebarLoading = ref(false)
const sidebarType = ref<'parking' | 'crowd' | 'traffic' | 'fence' | 'highResolution' | ''>('')
const sidebarTitle = ref('')
const sidebarData = ref<SidebarData>({})
const selectedDeviceId = ref<string | number>('')
const fenceImageUrl = ref('')

// 儲存各個類型的 landmarks 資料
const landmarksData = ref({
  parkingLandmarks: [] as ILandmarkItem[],
  crowdLandmarks: [] as ILandmarkItem[],
  trafficLandmarks: [] as ILandmarkItem[],
  fenceLandmarks: [] as ILandmarkItem[],
  highResolutionLandmarks: [] as ILandmarkItem[]
})

// 根據當前檢視類型返回對應的 landmarks
const currentLandmarks = computed(() => {
  switch (currentView.value) {
    case 'parking':
      return landmarksData.value.parkingLandmarks
    case 'crowd':
      return landmarksData.value.crowdLandmarks
    case 'traffic':
      return landmarksData.value.trafficLandmarks
    case 'fence':
      return landmarksData.value.fenceLandmarks
    case 'highResolution':
      return landmarksData.value.highResolutionLandmarks
    default:
      return []
  }
})

// 取得當前檢視的圖示組件
const getCurrentViewIcon = () => {
  switch (currentView.value) {
    case 'parking':
      return ParkingCircle
    case 'crowd':
      return Users
    case 'traffic':
      return Car
    case 'fence':
      return Shield
    case 'highResolution':
      return Camera
    default:
      return ParkingCircle
  }
}

// 取得當前檢視的標題
const getCurrentViewTitle = () => {
  switch (currentView.value) {
    case 'parking':
      return '停車場資訊'
    case 'crowd':
      return '人流監控'
    case 'traffic':
      return '車流監控'
    case 'fence':
      return '電子圍籬'
    case 'highResolution':
      return '4K影像'
    default:
      return '設備資訊'
  }
}

// 載入地圖標點資料
const loadData = async () => {
  try {
    loading.value = true
    const landmarks = await loadMapLandmarks()
    landmarksData.value = landmarks
    console.log('智慧觀光情報站資料載入完成:', landmarks)
  } catch (error) {
    console.error('載入智慧觀光情報站資料失敗:', error)
  } finally {
    loading.value = false
  }
}

// 設定當前檢視並更新路由
const setCurrentView = (view: ViewType) => {
  currentView.value = view
  selectedLandmark.value = null // 清除選中的標點

  // 關閉設備詳情側邊欄
  showDeviceSidebar.value = false

  // 更新路由參數
  router.push({
    name: 'SmartTourism',
    query: { type: view }
  })
}

// 懸停標點 - 只放大到標點
const handleHoverLandmark = (landmark: ILandmarkItem) => {
  selectedLandmark.value = landmark
  // 只設置地圖焦點，不打開側邊欄
}

// 選擇標點 - 放大並顯示詳情
const handleSelectLandmark = (landmark: ILandmarkItem) => {
  selectedLandmark.value = landmark
  // 在移動端關閉側邊欄
  showMobileSidebar.value = false
  // 同時觸發設備詳情顯示
  if (landmark.id) {
    handleDeviceDetail(landmark.id, currentView.value)
  }
}

// Chart options for sidebar
const chartOptions = computed(() => ({
  parking: parkingHistory.value && parkingHistory.value.length > 0 ? (() => {
    // 確保數據按時間排序
    const sortedRecords = parkingHistory.value.sort((a, b) => {
      const dateTimeA = new Date(a.timestamp ?? 0)
      const dateTimeB = new Date(b.timestamp ?? 0)
      return dateTimeA.getTime() - dateTimeB.getTime()
    })

    return {
      title: {
        text: `${sortedRecords[0].deviceName} - 停車場使用率歷史`,
        textStyle: { fontSize: 12 }
      },
      tooltip: {
        trigger: 'axis',
        formatter: '{b}: {c}%'
      },
      xAxis: {
        type: 'category',
        data: sortedRecords.map(record => record.time ?? ""),
        axisLabel: { fontSize: 10, rotate: 45 }
      },
      yAxis: {
        type: 'value',
        name: '使用率(%)',
        axisLabel: { fontSize: 10 }
      },
      series: [{
        type: 'line',
        data: sortedRecords.map(record => record.occupancyRate ?? 0),
        itemStyle: { color: getDevicePrimaryColor('parking') },
        lineStyle: { color: getDevicePrimaryColor('parking') },
        smooth: true
      }]
    }
  })() as EChartsOption : {
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
  } as EChartsOption,

  crowd: crowdHistory.value && crowdHistory.value.length > 0 ? (() => {
    // 確保數據按時間排序
    const sortedRecords = crowdHistory.value.sort((a, b) => {
      const dateTimeA = new Date(a.timestamp ?? 0)
      const dateTimeB = new Date(b.timestamp ?? 0)
      return dateTimeA.getTime() - dateTimeB.getTime()
    })

    return {
      title: {
        text: `${sortedRecords[0].deviceName} - 人流密度歷史`,
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
        data: sortedRecords.map(record => record.time ?? ""),
        axisLabel: { fontSize: 10, rotate: 45 }
      },
      yAxis: {
        type: 'value',
        name: '人數',
        axisLabel: { fontSize: 10, formatter: '{value}人' }
      },
      series: [{
        type: 'line',
        data: sortedRecords.map(record => record.peopleCount ?? 0),
        itemStyle: { color: getDevicePrimaryColor('crowd') },
        lineStyle: { color: getDevicePrimaryColor('crowd') },
        smooth: true
      }]
    }
  })() as EChartsOption : {
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
  } as EChartsOption,

  traffic: trafficHistory.value && trafficHistory.value.length > 0 ? (() => {
    // 確保數據按時間排序
    const sortedRecords = trafficHistory.value.sort((a, b) => {
      const dateTimeA = new Date(a.timestamp ?? 0)
      const dateTimeB = new Date(b.timestamp ?? 0)
      return dateTimeA.getTime() - dateTimeB.getTime()
    })

    return {
      title: {
        text: `${sortedRecords[0].deviceName} - 平均速度歷史`,
        textStyle: { fontSize: 12 }
      },
      tooltip: {
        trigger: 'axis',
        formatter: (params: any) => {
          const value = params[0].value
          return `${params[0].axisValue}: ${value} km/h`
        }
      },
      xAxis: {
        type: 'category',
        data: sortedRecords.map(record => record.time ?? ""),
        axisLabel: { fontSize: 10, rotate: 45 }
      },
      yAxis: {
        type: 'value',
        name: '平均速度(km/h)',
        axisLabel: { fontSize: 10 }
      },
      series: [{
        type: 'line',
        data: sortedRecords.map(record => record.averageSpeed ?? 0),
        itemStyle: { color: getDevicePrimaryColor('traffic') },
        lineStyle: { color: getDevicePrimaryColor('traffic') },
        smooth: true
      }]
    }
  })() as EChartsOption : {
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
  } as EChartsOption,

  fence: fenceHistory.value && fenceHistory.value.length > 0 ? (() => {
    // 確保數據按時間排序
    const sortedRecords = fenceHistory.value.sort((a, b) => {
      const dateTimeA = new Date(a.timestamp ?? 0)
      const dateTimeB = new Date(b.timestamp ?? 0)
      return dateTimeA.getTime() - dateTimeB.getTime()
    })

    return {
      title: {
        text: `${sortedRecords[0]?.deviceName ?? ""} - 圍籬事件歷史`,
        textStyle: { fontSize: 12 }
      },
      tooltip: {
        trigger: 'axis',
        formatter: '{b}: {c} 次事件'
      },
      xAxis: {
        type: 'category',
        data: sortedRecords.map(record => record.time ?? ""),
        axisLabel: { fontSize: 10, rotate: 45 }
      },
      yAxis: {
        type: 'value',
        name: '事件次數',
        axisLabel: { fontSize: 10 }
      },
      series: [{
        type: 'bar',
        data: sortedRecords.map(() => 1),
        itemStyle: { color: getDevicePrimaryColor('fence') },
        emphasis: {
          itemStyle: { color: getDeviceColors('fence').primaryDark }
        }
      }]
    }
  })() as EChartsOption : {
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
  } as EChartsOption
}))

// Handle device detail from map
const handleDeviceDetail = async (deviceId: number, deviceType: ViewType) => {
  sidebarLoading.value = true
  sidebarType.value = deviceType
  showDeviceSidebar.value = true
  selectedDeviceId.value = deviceId

  // Clear previous history data
  clearHistoryData()

  try {
    // Get device details based on type
    switch (deviceType) {
      case 'parking':
        sidebarTitle.value = '停車場詳情'
        const parkingResponse = await MapService.getLandmarkParking(deviceId)
        sidebarData.value = parkingResponse
        break

      case 'crowd':
        sidebarTitle.value = '人流監控詳情'
        const crowdResponse = await MapService.getLandmarkCrowd(deviceId)
        sidebarData.value = crowdResponse
        break

      case 'traffic':
        sidebarTitle.value = '交通監控詳情'
        const trafficResponse = await MapService.getLandmarkTraffic(deviceId)
        sidebarData.value = trafficResponse
        break

      case 'fence':
        sidebarTitle.value = '電子圍籬詳情'
        const fenceResponse = await MapService.getLandmarkFence(deviceId)
        sidebarData.value = fenceResponse
        // Set default fence image if available
        if ('events' in fenceResponse && fenceResponse.events && fenceResponse.events.length > 0) {
          fenceImageUrl.value = fenceResponse.events[0].image || ''
        }
        break

      case 'highResolution':
        sidebarTitle.value = '4K影像詳情'
        const hrResponse = await MapService.getLandmarkHighResolution(deviceId)
        sidebarData.value = hrResponse
        break
    }
  } catch (error) {
    console.error('載入設備詳細資料失敗:', error)
    // Use basic data as fallback
    sidebarData.value = {
      name: `設備 ${deviceId}`,
      status: 'offline',
      lastUpdateTime: '',
    }
  } finally {
    sidebarLoading.value = false
  }

  try {
    // Load history data (24 hours) for non-highResolution devices
    if (deviceType !== 'highResolution') {
      const deviceName = sidebarData.value.name || `設備 ${deviceId}`
      loadDeviceHistory(deviceName, deviceType)
    }
  } catch (error) {
    console.error('載入歷史數據失敗:', error)
  }
}

// Close device sidebar
const closeSidebar = () => {
  showDeviceSidebar.value = false
  sidebarType.value = ''
  sidebarData.value = {}
  selectedDeviceId.value = ''
  fenceImageUrl.value = ''
}

// Change fence image
const changeFenceImage = (imageUrl: string) => {
  fenceImageUrl.value = imageUrl
}

// Open video in new window
const openVideoInNewWindow = () => {
  if ('videoUrl' in sidebarData.value && sidebarData.value.videoUrl) {
    const newWindow = window.open('', '_blank')
    if (newWindow) {
      newWindow.location.href = sidebarData.value.videoUrl
    } else {
      window.location.href = sidebarData.value.videoUrl
    }
  }
}


// 從路由參數初始化檢視類型
const initializeViewFromRoute = () => {
  const typeParam = route.query.type as ViewType
  if (typeParam && ['crowd', 'traffic', 'parking', 'fence', 'highResolution'].includes(typeParam)) {
    currentView.value = typeParam
  } else {
    currentView.value = 'parking' // 預設值
  }
}

// 組件掛載時載入資料
onMounted(() => {
  initializeViewFromRoute()
  loadData()
})

// 監聽路由變化
watch(() => route.query.type, (newType) => {
  if (newType && typeof newType === 'string' && ['crowd', 'traffic', 'parking', 'fence', 'highResolution'].includes(newType)) {
    currentView.value = newType as ViewType
    selectedLandmark.value = null
    // 關閉設備詳情側邊欄
    showDeviceSidebar.value = false
  }
})
</script>

<style scoped>
.custom-scrollbar {
  scrollbar-width: thin;
  scrollbar-color: #cbd5e1 transparent;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 3px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background-color: #94a3b8;
}
</style>