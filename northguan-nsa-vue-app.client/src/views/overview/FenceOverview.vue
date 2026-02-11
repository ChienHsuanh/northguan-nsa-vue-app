<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- Navigation -->
      <OverviewNavigation current-page="fence" />

      <!-- 電子圍籬事件圖表 -->
      <Card class="mb-6">
        <CardHeader>
          <CardTitle>電子圍籬事件</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="mb-4 flex flex-col sm:flex-row gap-4">
            <Select v-model="selectedStationId" @update:model-value="loadData">
              <SelectTrigger class="w-full sm:w-48">
                <SelectValue placeholder="選擇分站" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="0">所有分站</SelectItem>
                <SelectItem v-for="station in stations" :key="station.id" :value="station.id.toString()">
                  {{ station.name }}
                </SelectItem>
              </SelectContent>
            </Select>

            <Select v-model="timeRangeDays" @update:model-value="loadData">
              <SelectTrigger class="w-full sm:w-32">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="1">1天</SelectItem>
                <SelectItem value="3">3天</SelectItem>
                <SelectItem value="7">7天</SelectItem>
                <SelectItem value="30">30天</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div class="mb-4" v-if="availableDevices.length > 1">
            <div class="flex flex-col items-start">
              <label class="text-sm font-medium mb-2">顯示設備:</label>
              <div class="flex flex-wrap gap-2">
                <label v-for="device in availableDevices" :key="device" class="flex items-center gap-1 text-sm">
                  <Checkbox :value="device" :model-value="selectedDevices.includes(device)" @update:model-value="(checked) => {
                    if (checked) {
                      selectedDevices = [...selectedDevices, device]
                    } else {
                      selectedDevices = selectedDevices.filter(d => d !== device)
                    }
                    updateChart()
                  }" />
                  <span>{{ device }}</span>
                </label>
              </div>
            </div>
          </div>

          <div class="h-96">
            <div v-if="chartLoading" class="flex items-center justify-center h-full">
              <div class="text-gray-500">載入圖表中...</div>
            </div>
            <div v-else-if="!recordHistoryChartOption" class="flex items-center justify-center h-full text-gray-500">
              暫無數據
            </div>
            <div v-else ref="chartContainer" class="w-full h-full"></div>
          </div>
        </CardContent>
      </Card>

      <!-- 電子圍籬事件紀錄表格 -->
      <Card>
        <CardHeader>
          <CardTitle>電子圍籬事件紀錄</CardTitle>
        </CardHeader>
        <CardContent class="p-0">
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">時間</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">名稱</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">分站</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">事件</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">照片</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                <tr v-if="loading">
                  <td colspan="5" class="px-4 py-8 text-center text-gray-500">
                    載入中...
                  </td>
                </tr>
                <tr v-else-if="recentRecords.length === 0">
                  <td colspan="5" class="px-4 py-8 text-center text-gray-500">
                    暫無數據
                  </td>
                </tr>
                <tr v-else v-for="record in recentRecords"
                  :key="`${record.station}-${record.deviceName}-${record.time}`">
                  <td class="px-4 py-3 text-sm text-gray-900">{{ `${record.date} ${record.time}` }}</td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ record.deviceName }}</td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ record.station }}</td>
                  <td class="px-4 py-3 text-sm">
                    <span class="px-2 py-1 text-xs rounded-full" :class="getEventTypeClass(record.eventType ?? 0)">
                      {{ record.event }}
                    </span>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div v-if="record.imageUrl" class="flex justify-center">
                      <img :src="record.imageUrl" @click="showPhotoModal(record.imageUrl)"
                        @error="handleTableImageError"
                        class="w-42 h-24 object-cover rounded cursor-pointer hover:opacity-80 transition-opacity border border-gray-200" />
                    </div>
                    <span v-else class="text-gray-400 text-xs">無照片</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </main>
  </div>

  <!-- Photo Modal -->
  <Dialog v-model:open="photoModalOpen">
    <DialogContent class="max-w-5xl max-h-[90vh] p-6">
      <DialogHeader>
        <DialogTitle class="text-center">
          圍籬事件照片
          <span v-if="availablePhotos.length > 1" class="text-sm text-gray-500 font-normal">
            ({{ currentPhotoIndex + 1 }} / {{ availablePhotos.length }})
          </span>
        </DialogTitle>
      </DialogHeader>
      <div class="relative flex justify-center items-center min-h-[60vh]">
        <!-- Previous Button -->
        <Button v-if="availablePhotos.length > 1" @click="showPreviousPhoto" variant="outline" size="sm"
          class="absolute left-4 top-1/2 transform -translate-y-1/2 z-10 bg-white/80 hover:bg-white"
          :disabled="currentPhotoIndex === 0">
          <ArrowLeftIcon class="w-4 h-4" />
        </Button>

        <!-- Next Button -->
        <Button v-if="availablePhotos.length > 1" @click="showNextPhoto" variant="outline" size="sm"
          class="absolute right-4 top-1/2 transform -translate-y-1/2 z-10 bg-white/80 hover:bg-white"
          :disabled="currentPhotoIndex === availablePhotos.length - 1">
          <ArrowRightIcon class="w-4 h-4" />
        </Button>

        <!-- Loading Skeleton -->
        <div v-if="imageLoading" class="w-full max-w-2xl h-96 rounded animate-pulse flex items-center justify-center">
          <div class="w-8 h-8 border-4 border-gray-300 border-t-blue-500 rounded-full animate-spin"></div>
        </div>

        <!-- Error State -->
        <div v-else-if="imageError"
          class="w-full max-w-2xl h-96 bg-gray-100 rounded border-2 border-dashed border-gray-300 flex flex-col items-center justify-center text-gray-500">
          <AlertCircleIcon class="w-12 h-12 mb-4 text-gray-400" />
          <p class="text-sm font-medium">圖片加載失敗</p>
          <p class="text-xs text-gray-400 mt-1">請稍後再試</p>
        </div>

        <!-- Image -->
        <img v-else-if="!imageLoading && !imageError && selectedPhoto" :src="selectedPhoto"
          class="w-full max-w-6xl max-h-[75vh] object-contain rounded shadow-lg" />

        <!-- Hidden preloader image -->
        <img v-if="selectedPhoto" :src="selectedPhoto" @load="handleImageLoad" @error="handleImageError"
          style="display: none" ref="modalImage" />
      </div>

      <!-- Navigation hints -->
      <div v-if="availablePhotos.length > 1" class="text-center text-sm text-gray-500 mt-4">
        使用左右箭頭鍵或點擊按鈕切換圖片
      </div>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, nextTick, watch } from 'vue'
import * as echarts from 'echarts'
import type { EChartsOption } from 'echarts'
import { AlertCircleIcon, ArrowLeftIcon, ArrowRightIcon } from 'lucide-vue-next'

// Components
import AppHeader from '@/components/layout/AppHeader.vue'
import OverviewNavigation from '@/components/overview/OverviewNavigation.vue'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'

// Services
import { FenceOverviewService } from '@/services/overview/FenceOverviewService'
import { StationResponse, StationService } from '@/services/StationService'

// Utils
import { CHART_COLORS } from '@/utils/chartUtils'
import type { FenceRecordHistoryResponse } from '@/services/overview/FenceOverviewService'
import type { FenceRecentRecordDetail } from '@/api/client'

// State
const loading = ref(true)
const chartLoading = ref(true)
const selectedStationId = ref('0')
const timeRangeDays = ref('1')
const chartContainer = ref<HTMLElement>()

// Photo modal state
const photoModalOpen = ref(false)
const selectedPhoto = ref<string | null>(null)
const imageLoading = ref(true)
const imageError = ref(false)
const modalImage = ref<HTMLImageElement>()
const currentPhotoIndex = ref(0)
const availablePhotos = ref<string[]>([])
let chartInstance: echarts.ECharts | null = null

const stations = ref<StationResponse[]>([])
const recordHistory = ref<FenceRecordHistoryResponse | null>(null)
const recentRecords = ref<FenceRecentRecordDetail[]>([])
const availableDevices = ref<string[]>([])
const selectedDevices = ref<string[]>([])

// Chart configuration
const recordHistoryChartOption = computed<EChartsOption | null>(() => {
  if (!recordHistory.value?.data || recordHistory.value.data.length === 0) {
    return null
  }

  const data = recordHistory.value.data

  // Group data by device (similar to old implementation)
  const deviceGroups = data.reduce((groups, record) => {
    const deviceKey = `${record.stationName} - ${record.deviceName}`
    if (!groups[deviceKey]) {
      groups[deviceKey] = []
    }
    groups[deviceKey].push(record)
    return groups
  }, {} as Record<string, typeof data>)

  // Get all unique times and sort them
  const allTimes = Array.from(new Set(data.map(item => item.time))).sort()

  // Color palette for different devices
  const colors = CHART_COLORS

  // Create series for each device
  const series: any[] = []
  const legendData: string[] = []
  let colorIndex = 0

  Object.entries(deviceGroups).forEach(([deviceName, records]) => {
    // Skip devices that are not selected
    if (selectedDevices.value.length > 0 && !selectedDevices.value.includes(deviceName)) {
      return
    }

    // Create time-value map for this device (count events per time)
    const timeValueMap: Record<string, number> = {}
    records.forEach(record => {
      timeValueMap[record.time] = (timeValueMap[record.time] || 0) + 1
    })

    // Create event count data array aligned with allTimes
    const eventCountData = allTimes.map(time => timeValueMap[time] || 0)

    const deviceColor = colors[colorIndex % colors.length]

    // Add event count line series for this device
    series.push({
      name: deviceName,
      type: 'line',
      data: eventCountData,
      smooth: true,
      connectNulls: true,
      lineStyle: {
        color: deviceColor,
        width: 2
      },
      itemStyle: {
        color: deviceColor
      },
      symbol: 'circle',
      symbolSize: 4
    })

    legendData.push(deviceName)
    colorIndex++
  })

  return {
    title: {
      text: '電子圍籬事件趨勢 (依設備顯示)',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross'
      },
      formatter: function (params: any) {
        if (!params || params.length === 0) return ''

        const time = params[0].axisValue
        let tooltip = `<div><strong>${time}</strong></div>`

        params.forEach((param: any) => {
          if (param.value !== null && param.value !== undefined) {
            tooltip += `<div style="margin: 4px 0;">`
            tooltip += `<span style="color: ${param.color};">●</span> `
            tooltip += `<strong>${param.seriesName}</strong>: ${param.value} 事件`
            tooltip += `</div>`
          }
        })

        return tooltip
      }
    },
    legend: {
      data: legendData,
      top: 30,
      type: 'scroll',
      orient: 'horizontal'
    },
    xAxis: {
      type: 'category',
      data: allTimes,
      axisLabel: {
        rotate: 45,
        interval: Math.max(1, Math.floor(allTimes.length / 10))
      }
    },
    yAxis: {
      type: 'value',
      name: '事件數量',
      axisLabel: {
        formatter: '{value}'
      }
    },
    series: series,
    grid: {
      top: 100,
      bottom: 80,
      left: 80,
      right: 80
    },
    dataZoom: [
      {
        type: 'slider',
        show: allTimes.length > 20,
        xAxisIndex: 0,
        bottom: 10
      }
    ]
  }
})

// Methods
const loadStations = async () => {
  try {
    const response = await StationService.getStations()
    stations.value = response
  } catch (error) {
    console.error('載入分站失敗:', error)
  }
}

const loadRecordHistory = async () => {
  try {
    chartLoading.value = true
    const stationId = selectedStationId.value === '0' ? undefined : parseInt(selectedStationId.value)
    const response = await FenceOverviewService.getRecentRecordHistory(stationId, parseInt(timeRangeDays.value) * 24 * 60 * 60)
    recordHistory.value = response

    // Update available devices
    if (response?.data) {
      const devices = Array.from(new Set(response.data.map(item => `${item.stationName} - ${item.deviceName}`)))
      availableDevices.value = devices

      // If no devices are selected, select all by default
      if (selectedDevices.value.length === 0) {
        selectedDevices.value = [...devices]
      } else {
        // Remove devices that are no longer available
        selectedDevices.value = selectedDevices.value.filter(device => devices.includes(device))
      }
    }
  } catch (error) {
    console.error('載入記錄歷史失敗:', error)
    recordHistory.value = null
    availableDevices.value = []
    selectedDevices.value = []
  } finally {
    chartLoading.value = false
  }
}

const loadRecentRecords = async () => {
  try {
    loading.value = true
    const stationId = selectedStationId.value === '0' ? undefined : parseInt(selectedStationId.value)
    const response = await FenceOverviewService.getRecentRecord(stationId, parseInt(timeRangeDays.value) * 24 * 60 * 60)
    recentRecords.value = response.data || []
  } catch (error) {
    console.error('載入最近記錄失敗:', error)
    recentRecords.value = []
  } finally {
    loading.value = false
  }
}

const loadData = async () => {
  await Promise.all([
    loadRecordHistory(),
    loadRecentRecords()
  ])
}

const getEventTypeClass = (eventType: number): string => {
  switch (eventType) {
    case 1:
      return 'bg-red-100 text-red-800'
    case 2:
      return 'bg-green-100 text-green-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
}

// Photo modal methods
const showPhotoModal = (photoUrl: string) => {
  // 收集所有有照片的記錄
  availablePhotos.value = recentRecords.value
    .filter(record => record.imageUrl)
    .map(record => record.imageUrl!)

  // 找到當前照片的索引
  currentPhotoIndex.value = availablePhotos.value.indexOf(photoUrl)
  if (currentPhotoIndex.value === -1) {
    currentPhotoIndex.value = 0
  }

  selectedPhoto.value = photoUrl
  imageLoading.value = true
  imageError.value = false
  photoModalOpen.value = true
}

const handleImageLoad = () => {
  imageLoading.value = false
  imageError.value = false
}

const handleImageError = () => {
  imageLoading.value = false
  imageError.value = true
}

const handleTableImageError = (event: Event) => {
  const img = event.target as HTMLImageElement
  img.src = '/images/image-placeholder.png'
  img.onerror = null // Prevent infinite loop
}

// 圖片導航功能
const showPreviousPhoto = () => {
  if (currentPhotoIndex.value > 0) {
    const newIndex = currentPhotoIndex.value - 1
    const newPhoto = availablePhotos.value[newIndex]

    // 如果是同一張圖片，不需要重新載入
    if (newPhoto === selectedPhoto.value) {
      currentPhotoIndex.value = newIndex
      return
    }

    currentPhotoIndex.value = newIndex
    selectedPhoto.value = newPhoto
    imageLoading.value = true
    imageError.value = false
  }
}

const showNextPhoto = () => {
  if (currentPhotoIndex.value < availablePhotos.value.length - 1) {
    const newIndex = currentPhotoIndex.value + 1
    const newPhoto = availablePhotos.value[newIndex]

    // 如果是同一張圖片，不需要重新載入
    if (newPhoto === selectedPhoto.value) {
      currentPhotoIndex.value = newIndex
      return
    }

    currentPhotoIndex.value = newIndex
    selectedPhoto.value = newPhoto
    imageLoading.value = true
    imageError.value = false
  }
}

// 鍵盤事件處理
const handleKeydown = (event: KeyboardEvent) => {
  if (!photoModalOpen.value) return

  if (event.key === 'ArrowLeft') {
    event.preventDefault()
    showPreviousPhoto()
  } else if (event.key === 'ArrowRight') {
    event.preventDefault()
    showNextPhoto()
  } else if (event.key === 'Escape') {
    event.preventDefault()
    photoModalOpen.value = false
  }
}


// Chart methods
const initChart = async () => {
  await nextTick()
  if (chartContainer.value && recordHistoryChartOption.value) {
    if (chartInstance) {
      chartInstance.dispose()
    }
    chartInstance = echarts.init(chartContainer.value)
    chartInstance.setOption(recordHistoryChartOption.value)
  }
}

const resizeChart = () => {
  if (chartInstance) {
    chartInstance.resize()
  }
}

const updateChart = async () => {
  await nextTick()
  if (recordHistoryChartOption.value) {
    await initChart()
  }
}

// Watch for chart option changes
watch(recordHistoryChartOption, async (newOption) => {
  if (newOption) {
    await initChart()
  }
}, { immediate: true })

// Watch for device selection changes
watch(selectedDevices, async () => {
  await updateChart()
}, { deep: true })

// Lifecycle
onMounted(async () => {
  await loadStations()
  await loadData()

  // Add resize listener
  window.addEventListener('resize', resizeChart)

  // 添加鍵盤事件監聽器
  document.addEventListener('keydown', handleKeydown)
})

// Cleanup
onUnmounted(() => {
  if (chartInstance) {
    chartInstance.dispose()
  }
  window.removeEventListener('resize', resizeChart)

  // 移除鍵盤事件監聽器
  document.removeEventListener('keydown', handleKeydown)
})
</script>
