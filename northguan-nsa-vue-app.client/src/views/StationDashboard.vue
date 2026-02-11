<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="px-4 py-6">
      <div class="container mx-auto">
        <!-- Mobile Title -->
        <div class="block md:hidden text-center mb-4">
          <h3 class="text-lg font-bold text-blue-600">分站戰情室</h3>
        </div>

        <!-- Control Panel -->
        <div class="mb-6">
          <Card class="shadow-sm">
            <CardContent class="p-4">
              <div class="flex flex-wrap items-center justify-between gap-4">
                <div class="flex items-center space-x-4">
                  <h1 class="hidden md:block text-xl font-bold text-gray-900">分站戰情室</h1>
                  <Select v-model="selectedStationId" @update:modelValue="onStationChange">
                    <SelectTrigger class="w-48">
                      <SelectValue placeholder="選擇分站" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem v-for="station in stations" :key="station.id" :value="station.id.toString()">
                        {{ station.name }}
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div class="flex items-center space-x-2">
                  <Button @click="refreshData" :disabled="loading" variant="outline" size="sm">
                    <RefreshCwIcon :class="{ 'animate-spin': loading }" class="w-4 h-4 mr-1" />
                    刷新
                  </Button>
                  <Button @click="goBack" variant="outline" size="sm">← 返回主頁</Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        <!-- Main Dashboard Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">

          <!-- Station Overview with Map -->
          <div class="col-span-1 md:col-span-2 xl:col-span-1">
            <Card class="h-full">
              <CardHeader>
                <CardTitle>{{ selectedStation?.name || "分站" }}總覽</CardTitle>
                <p class="text-sm text-gray-500">Report Center</p>
              </CardHeader>
              <CardContent class="p-4">
                <div id="station-map" class="h-64 bg-gray-100 rounded-lg"></div>
              </CardContent>
            </Card>
          </div>

          <!-- Live Video Streams -->
          <div v-for="video in highResolutionList" :key="video.id" class="col-span-1">
            <Card class="h-full">
              <CardHeader>
                <CardTitle>景區Live影像</CardTitle>
                <p class="text-sm text-gray-500">Live broadcast</p>
              </CardHeader>
              <CardContent class="p-4">
                <div class="h-64 bg-gray-100 rounded-lg overflow-hidden">
                  <!-- Regular video -->
                  <video v-if="video.videoUrl && !isYouTubeUrl(video.videoUrl)" controls
                    class="w-full h-full object-cover" :src="video.videoUrl" />
                  <!-- YouTube iframe with nocookie -->
                  <iframe v-else-if="video.videoUrl && isYouTubeUrl(video.videoUrl)" class="w-full h-full"
                    :src="getYouTubeNoCookieUrl(video.videoUrl)" title="YouTube video player" frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                    allowfullscreen />
                  <!-- Placeholder -->
                  <div v-else class="w-full h-full flex items-center justify-center text-gray-500">
                    <span>暫無影像</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- Crowd Statistics -->
          <div v-if="crowdRecords.length" class="col-span-1">
            <Card class="h-full">
              <CardHeader>
                <CardTitle>景區即時人數</CardTitle>
              </CardHeader>
              <CardContent class="p-4">
                <div class="space-y-4">
                  <div v-for="record in crowdRecords" :key="record.deviceName"
                    class="flex items-center justify-between py-2 border-b border-dashed border-gray-200 last:border-b-0">
                    <div class="flex items-center space-x-2">
                      <img src="/images/icons/map-marker.svg" class="w-5 h-5" alt="位置" />
                      <span class="text-sm font-medium">{{ record.deviceName }}</span>
                    </div>
                    <div class="flex items-center space-x-2">
                      <!-- Status Icon -->
                      <div class="w-10 h-10 flex items-center justify-center">
                        <img :src="getCrowdStatusIcon(record.peopleNum || 0)" class="w-8 h-8" alt="人流狀態" />
                      </div>
                      <div class="text-right">
                        <div class="text-blue-600 font-semibold">
                          {{ record.peopleNum || 0 }}人次
                        </div>
                        <div v-if="record.totalIn && record.totalIn > 0" class="text-xs text-gray-500">
                          累計入園數: {{ record.totalIn }}人
                        </div>
                      </div>
                    </div>
                  </div>
                  <div
                    class="flex justify-between items-center pt-2 border-t border-gray-200 text-orange-500 font-semibold">
                    <span>總人數</span>
                    <span>{{ totalCrowdNum }}人次</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- Parking Statistics -->
          <div v-if="parkingRecords.length" class="col-span-1">
            <Card class="h-full">
              <CardHeader>
                <CardTitle>停車場使用率</CardTitle>
              </CardHeader>
              <CardContent class="p-4">
                <div class="space-y-4">
                  <!-- Parking Donut Chart (SVG matching original) -->
                  <div class="relative w-40 h-40 mx-auto">
                    <svg class="w-full h-full transform -rotate-90" viewBox="0 0 40 40">
                      <circle cx="20" cy="20" r="15.915" fill="#fff" stroke="transparent" stroke-width="5" />
                      <circle cx="20" cy="20" r="15.915" fill="transparent" stroke="#e5e7eb" stroke-width="5" />
                      <circle cx="20" cy="20" r="15.915" fill="transparent" stroke="#a6d997" stroke-width="5"
                        :stroke-dasharray="`${parkingUsage} ${100 - parkingUsage}`" stroke-dashoffset="25"
                        class="transition-all duration-500" />
                    </svg>
                    <div class="absolute inset-0 flex items-center justify-center">
                      <span class="text-xl font-bold text-green-600">{{ parkingUsage }}%</span>
                    </div>
                  </div>

                  <!-- Parking Details -->
                  <div class="space-y-3">
                    <div v-for="(record, index) in parkingRecords" :key="record.deviceName" class="flex items-start">
                      <div class="flex items-center mr-2 mt-1">
                        <div class="w-3 h-3 rounded-full"
                          :style="{ backgroundColor: index % 2 === 1 ? '#dfeedb' : '#a6d997' }" />
                      </div>
                      <div class="flex-1">
                        <div class="text-sm font-medium">{{ record.deviceName }}</div>
                        <div class="text-sm text-gray-600">
                          {{ record.parkedNum || 0 }} / {{ record.totalNum || 0 }} 車位
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- Fence Events -->
          <div v-if="fenceRecords.length" class="col-span-1">
            <Card class="h-full">
              <CardHeader>
                <CardTitle>電子圍籬</CardTitle>
              </CardHeader>
              <CardContent class="p-4">
                <div class="space-y-3">
                  <div v-for="record in fenceRecords" :key="record.id"
                    class="flex items-center justify-between cursor-pointer hover:bg-gray-50 p-2 rounded"
                    @click="openFenceEventModal(record.id)">
                    <div class="flex items-center space-x-2 flex-1">
                      <img src="/images/icons/fence.svg" class="w-5 h-5" alt="圍籬" />
                      <span class="text-sm font-medium">{{ record.deviceName }}</span>
                    </div>
                    <div class="text-center px-2">
                      <div class="text-sm font-medium cursor-pointer"
                        :class="record.eventType === 1 ? 'text-red-600' : 'text-blue-600'">
                        {{ record.eventType === 1 ? "闖入事件" : "離開事件" }}
                      </div>
                    </div>
                    <div class="text-right text-xs text-red-500">
                      <div>{{ record.date }}</div>
                      <div>{{ record.time }}</div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- Traffic Conditions -->
          <div v-if="trafficRecords.length" class="col-span-1">
            <Card class="h-full">
              <CardHeader>
                <CardTitle>路段車流</CardTitle>
              </CardHeader>
              <CardContent class="p-4">
                <div class="space-y-4">
                  <div v-for="record in trafficRecords" :key="record.deviceName"
                    class="flex items-center justify-between py-3">
                    <div class="text-sm font-medium flex-1">{{ record.deviceName }}</div>
                    <div class="text-center px-4">{{ record.speed || 0 }} km/h</div>
                    <div class="flex flex-col items-center space-y-1">
                      <!-- Traffic Status Icon -->
                      <img :src="getTrafficStatusIcon(record.status)" class="w-8 h-8" alt="交通狀態" />
                      <span class="text-xs font-medium" :class="getTrafficStatusColor(record.status)">
                        {{ record.status }}
                      </span>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- Charts Section -->
          <!-- 景區容流量圖表 -->
          <div class="col-span-full">
            <Card>
              <CardHeader>
                <CardTitle>景區容流量</CardTitle>
              </CardHeader>
              <CardContent class="p-4">
                <div class="mb-4 flex gap-4">
                  <Select v-model="timeRangeDays" @update:modelValue="loadChartData">
                    <SelectTrigger class="w-32">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1">1天</SelectItem>
                      <SelectItem value="3">3天</SelectItem>
                      <SelectItem value="7">7天</SelectItem>
                      <SelectItem value="30">30天</SelectItem>
                    </SelectContent>
                  </Select>

                  <div class="flex items-center gap-2" v-if="availableCrowdDevices.length > 1">
                    <label class="text-sm font-medium">顯示設備:</label>
                    <div class="flex flex-wrap gap-2">
                      <label v-for="device in availableCrowdDevices" :key="device"
                        class="flex items-center gap-1 text-sm">
                        <input type="checkbox" :value="device" v-model="selectedCrowdDevices" @change="updateCrowdChart"
                          class="rounded border-gray-300" />
                        <span>{{ device }}</span>
                      </label>
                    </div>
                  </div>
                </div>

                <EChartsWrapper v-if="crowdCapacityChartOption && Object.keys(crowdCapacityChartOption).length"
                  :option="crowdCapacityChartOption" class="h-96" />
                <div v-else class="h-96 flex items-center justify-center text-gray-500">
                  <span>暫無容流量數據</span>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- 路段車流趨勢圖表 -->
          <div class="col-span-full">
            <Card>
              <CardHeader>
                <CardTitle>路段車流趨勢</CardTitle>
              </CardHeader>
              <CardContent class="p-4">
                <div class="mb-4 flex gap-4">
                  <div class="flex items-center gap-2" v-if="availableTrafficDevices.length > 1">
                    <label class="text-sm font-medium">顯示設備:</label>
                    <div class="flex flex-wrap gap-2">
                      <label v-for="device in availableTrafficDevices" :key="device"
                        class="flex items-center gap-1 text-sm">
                        <input type="checkbox" :value="device" v-model="selectedTrafficDevices"
                          @change="updateTrafficChart" class="rounded border-gray-300" />
                        <span>{{ device }}</span>
                      </label>
                    </div>
                  </div>
                </div>

                <EChartsWrapper v-if="trafficTrendChartOption && Object.keys(trafficTrendChartOption).length"
                  :option="trafficTrendChartOption" class="h-96" />
                <div v-else class="h-96 flex items-center justify-center text-gray-500">
                  <span>暫無車流趨勢數據</span>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- People Count History Chart -->
          <div v-if="showPeopleNumChartHistoryData" class="col-span-full md:col-span-2">
            <Card>
              <CardHeader>
                <CardTitle>即時入場人數</CardTitle>
              </CardHeader>
              <CardContent class="p-4">
                <EChartsWrapper v-if="peopleChartOption && Object.keys(peopleChartOption).length"
                  :option="peopleChartOption" class="h-80" />
                <div v-else class="h-80 flex items-center justify-center text-gray-500">
                  <span>暫無歷史數據</span>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- Parking History Chart -->
          <div v-if="showRoadConditionHistoryData" class="col-span-full md:col-span-2">
            <Card>
              <CardHeader>
                <CardTitle>停車場轉換率</CardTitle>
              </CardHeader>
              <CardContent class="p-4">
                <EChartsWrapper v-if="parkingHistoryChartOption && Object.keys(parkingHistoryChartOption).length"
                  :option="parkingHistoryChartOption" class="h-80" />
                <div v-else class="h-80 flex items-center justify-center text-gray-500">
                  <span>暫無歷史數據</span>
                </div>
              </CardContent>
            </Card>
          </div>

        </div>

        <!-- Fence Event Modal -->
        <Dialog :open="!!fenceEventImageUrl" @update:open="closeFenceEventModal">
          <DialogContent class="max-w-2xl">
            <DialogHeader>
              <DialogTitle>電子圍籬事件</DialogTitle>
            </DialogHeader>
            <div class="text-center">
              <img v-if="fenceEventImageUrl" :src="fenceEventImageUrl" class="max-w-full h-auto rounded" alt="圍籬事件圖片" />
            </div>
          </DialogContent>
        </Dialog>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import Card from '@/components/ui/card/Card.vue'
import CardHeader from '@/components/ui/card/CardHeader.vue'
import CardContent from '@/components/ui/card/CardContent.vue'
import CardTitle from '@/components/ui/card/CardTitle.vue'
import Button from '@/components/ui/button/Button.vue'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import AppHeader from '@/components/layout/AppHeader.vue'
import EChartsWrapper from '@/components/charts/EChartsWrapper.vue'
import { StationService } from '@/services/StationService'
import { CrowdOverviewService } from '@/services/overview/CrowdOverviewService'
import { ParkingOverviewService } from '@/services/overview/ParkingOverviewService'
import { TrafficOverviewService } from '@/services/overview/TrafficOverviewService'
import { FenceOverviewService } from '@/services/overview/FenceOverviewService'
import { HighResolutionService } from '@/services/HighResolutionService'
import { MapService } from '@/services/MapService'
import { useToast } from '@/composables/useToast'
import type { EChartsOption } from 'echarts'
import { RefreshCw as RefreshCwIcon } from 'lucide-vue-next'

// Utils
import { CHART_COLORS } from '@/utils/chartUtils'

// Leaflet imports
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

// Composables
const router = useRouter()
const route = useRoute()
const toast = useToast()

// State
const loading = ref(false)
const selectedStationId = ref("")
const timeRangeDays = ref("1")
const stations = ref<any[]>([])
const station = ref<any>({})
const landmarks = ref<any[]>([])
const crowdRecords = ref<any[]>([])
const parkingRecords = ref<any[]>([])
const trafficRecords = ref<any[]>([])
const fenceRecords = ref<any[]>([])
const highResolutionList = ref<any[]>([])
const peopleNumChartHistoryData = ref<any[]>([])
const roadConditionHistoryData = ref<any[]>([])
const fenceEventImageUrl = ref<string | null>(null)

// Chart data and device selections
const crowdCapacityHistory = ref<any>({})
const trafficTrendHistory = ref<any>({})
const availableCrowdDevices = ref<string[]>([])
const selectedCrowdDevices = ref<string[]>([])
const availableTrafficDevices = ref<string[]>([])
const selectedTrafficDevices = ref<string[]>([])

// Chart options
const peopleChartOption = ref<EChartsOption>({})
const parkingHistoryChartOption = ref<EChartsOption>({})
const crowdCapacityChartOption = ref<EChartsOption>({})
const trafficTrendChartOption = ref<EChartsOption>({})

// Leaflet map
let map: L.Map | null = null
let markersLayer: L.LayerGroup | null = null
let refreshInterval: any = null

// Computed properties
const selectedStation = computed(() => {
  return stations.value.find((s) => s.id?.toString() === selectedStationId.value)
})

const totalCrowdNum = computed(() => {
  return crowdRecords.value.reduce((sum, record) => sum + (record.peopleNum || 0), 0)
})

const parkingUsage = computed(() => {
  if (parkingRecords.value.length === 0) return 0
  const { parkedNumSum, totalNumSum } = parkingRecords.value.reduce((acc, obj) => {
    acc.parkedNumSum += obj.parkedNum || 0
    acc.totalNumSum += obj.totalNum || 0
    return acc
  }, { parkedNumSum: 0, totalNumSum: 0 })

  return totalNumSum > 0 ? Math.floor((100 * parkedNumSum) / totalNumSum) : 0
})

const showPeopleNumChartHistoryData = computed(() => {
  return peopleNumChartHistoryData.value.some(e => e.length > 0)
})

const showRoadConditionHistoryData = computed(() => {
  return roadConditionHistoryData.value.some(e => e.length > 0)
})

// Utility functions
const isYouTubeUrl = (url: string): boolean => {
  return url.includes('.youtube.com/')
}

const getYouTubeNoCookieUrl = (url: string): string => {
  if (!url.includes('.youtube.com/')) return url

  const re = /\?v=([a-zA-Z0-9_-]+)/
  const match = url.match(re)
  if (match && match[1]) {
    const videoId = match[1]
    return `https://www.youtube-nocookie.com/embed/${videoId}`
  }
  return url
}

const getCrowdStatusIcon = (peopleNum: number): string => {
  if (peopleNum >= 100) return '/images/icons/crowd-red.svg'
  if (peopleNum >= 50) return '/images/icons/crowd-yellow.svg'
  return '/images/icons/crowd-green.svg'
}

const getTrafficStatusIcon = (status: string): string => {
  switch (status) {
    case '擁擠': return '/images/icons/traffic-red.svg'
    case '稍擠': return '/images/icons/traffic-yellow.svg'
    case '暢通': return '/images/icons/traffic-green.svg'
    default: return '/images/icons/traffic-green.svg'
  }
}

const getTrafficStatusColor = (status: string): string => {
  switch (status) {
    case '擁擠': return 'text-red-500'
    case '稍擠': return 'text-yellow-500'
    case '暢通': return 'text-green-500'
    default: return 'text-green-500'
  }
}

// Color utility for charts
const getChartColor = (index: number): string => {
  return CHART_COLORS[index % CHART_COLORS.length]
}

// Data loading methods
const loadStations = async () => {
  try {
    const response = await StationService.getStations()
    stations.value = response || []
  } catch (error) {
    console.error('載入分站列表失敗:', error)
    toast.error('載入分站列表失敗')
  }
}

const loadStationDetail = async () => {
  if (!selectedStationId.value) return

  try {
    const response = await StationService.getStationDetail(Number(selectedStationId.value))
    station.value = response || {}

    // Initialize map if not already done
    if (!map && (station.value.latitude || station.value.longitude)) {
      await nextTick()
      await initializeMap()
    } else if (map && markersLayer) {
      // Update map center if station coordinates are available
      if (station.value.latitude && station.value.longitude) {
        map.setView([station.value.latitude, station.value.longitude], 14)
      }
      await loadMapLandmarks()
    }
  } catch (error) {
    console.error('載入分站詳情失敗:', error)
  }
}

const refreshCrowdRecords = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = selectedStationId.value === '0' ? undefined : Number(selectedStationId.value)
    const response = await CrowdOverviewService.getRecentCapacityRate(stationId, 86400)
    crowdRecords.value = response?.data || []
  } catch (error) {
    console.error('載入人流數據失敗:', error)
  }
}

const refreshParkingRecords = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = selectedStationId.value === '0' ? undefined : Number(selectedStationId.value)
    const response = await ParkingOverviewService.getRecentParkingRate(stationId, 86400)
    parkingRecords.value = response?.data || []
  } catch (error) {
    console.error('載入停車數據失敗:', error)
  }
}

const refreshFenceRecords = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = selectedStationId.value === '0' ? undefined : Number(selectedStationId.value)
    const response = await FenceOverviewService.getRecentRecord(stationId, 86400, 6)
    fenceRecords.value = (response?.data || [])
  } catch (error) {
    console.error('載入圍籬數據失敗:', error)
  }
}

const refreshTrafficRecords = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = selectedStationId.value === '0' ? undefined : Number(selectedStationId.value)
    const response = await TrafficOverviewService.getRecentRoadCondition(stationId)
    trafficRecords.value = response?.data || []
  } catch (error) {
    console.error('載入交通數據失敗:', error)
  }
}

const loadHighResolution = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = Number(selectedStationId.value)
    const response = await HighResolutionService.getOverviewInfo(stationId)
    highResolutionList.value = response?.data || []
  } catch (error) {
    console.error('載入高解析度影像失敗:', error)
  }
}

// 景區容流量圖表方法
const loadCrowdCapacityHistory = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = selectedStationId.value === '0' ? undefined : Number(selectedStationId.value)
    const response = await CrowdOverviewService.getRecentCapacityHistory(
      stationId,
      parseInt(timeRangeDays.value) * 24 * 60 * 60
    )
    crowdCapacityHistory.value = response || {}

    // 更新可用設備列表
    const data = response?.data || []
    const deviceGroups = data.reduce((groups: Record<string, any>, record: any) => {
      const deviceKey = `${record.stationName} - ${record.deviceName}`
      if (!groups[deviceKey]) {
        groups[deviceKey] = []
      }
      groups[deviceKey].push(record)
      return groups
    }, {})

    const devices = Object.keys(deviceGroups)
    availableCrowdDevices.value = devices
    selectedCrowdDevices.value = devices.slice(0, 5) // 默認選擇前5個設備

    updateCrowdCapacityChart()
  } catch (error) {
    console.error('載入景區容流量歷史數據失敗:', error)
  }
}

const updateCrowdCapacityChart = () => {
  const data = crowdCapacityHistory.value?.data || []
  if (data.length === 0) {
    crowdCapacityChartOption.value = {}
    return
  }

  // 按設備分組數據
  const deviceGroups = data.reduce((groups: Record<string, any>, record: any) => {
    const deviceKey = `${record.stationName} - ${record.deviceName}`
    if (!groups[deviceKey]) {
      groups[deviceKey] = []
    }
    groups[deviceKey].push(record)
    return groups
  }, {})

  // 獲取所有唯一時間並排序
  const allTimes = Array.from(new Set(data.map((item: any) => item.time))).sort()

  // 顏色調色板
  const colors = CHART_COLORS

  const series: any[] = []
  const legendData: string[] = []
  let colorIndex = 0

  Object.entries(deviceGroups).forEach(([deviceName, records]: [string, any]) => {
    // 跳過未選中的設備
    if (selectedCrowdDevices.value.length > 0 && !selectedCrowdDevices.value.includes(deviceName)) {
      return
    }

    // 創建時間-值映射
    const timeValueMap: Record<string, { density: number; peopleCount: number }> = {}
    records.forEach((record: any) => {
      timeValueMap[record.time] = {
        density: record.density || 0,
        peopleCount: record.peopleCount || 0
      }
    })

    const deviceColor = colors[colorIndex % colors.length]

    // 添加密度線系列
    series.push({
      name: `${deviceName} (密度)`,
      type: 'line',
      yAxisIndex: 0,
      data: allTimes.map(time => timeValueMap[time]?.density ?? 0),
      smooth: true,
      connectNulls: true,
      lineStyle: { color: deviceColor, width: 2 },
      itemStyle: { color: deviceColor },
      symbol: 'circle',
      symbolSize: 4
    })

    // 添加人數線系列
    series.push({
      name: `${deviceName} (人數)`,
      type: 'line',
      yAxisIndex: 1,
      data: allTimes.map(time => timeValueMap[time]?.peopleCount ?? 0),
      smooth: true,
      connectNulls: true,
      lineStyle: { color: deviceColor, width: 2, type: 'dashed' },
      itemStyle: { color: deviceColor },
      symbol: 'diamond',
      symbolSize: 4
    })

    legendData.push(`${deviceName} (密度)`, `${deviceName} (人數)`)
    colorIndex++
  })

  crowdCapacityChartOption.value = {
    title: { text: '景區容流量趨勢 (依設備顯示)', left: 'center' },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'cross' }
    },
    legend: { data: legendData, top: 30, type: 'scroll', orient: 'horizontal' },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'category', data: allTimes, boundaryGap: false },
    yAxis: [
      {
        type: 'value',
        name: '密度 (人/m²)',
        position: 'left',
        axisLabel: { formatter: '{value}' }
      },
      {
        type: 'value',
        name: '人數',
        position: 'right',
        axisLabel: { formatter: '{value}人' }
      }
    ],
    series
  }
}

// 路段車流趨勢圖表方法
const loadTrafficTrendHistory = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = selectedStationId.value === '0' ? undefined : Number(selectedStationId.value)
    const response = await TrafficOverviewService.getRecentRoadConditionHistory(
      stationId,
      parseInt(timeRangeDays.value) * 24 * 60 * 60
    )
    trafficTrendHistory.value = response || {}

    // 更新可用設備列表
    const data = response?.data || []
    const deviceGroups = data.reduce((groups: Record<string, any>, record: any) => {
      const deviceKey = `${record.stationName} - ${record.deviceName}`
      if (!groups[deviceKey]) {
        groups[deviceKey] = []
      }
      groups[deviceKey].push(record)
      return groups
    }, {})

    const devices = Object.keys(deviceGroups)
    availableTrafficDevices.value = devices
    selectedTrafficDevices.value = devices.slice(0, 5) // 默認選擇前5個設備

    updateTrafficTrendChart()
  } catch (error) {
    console.error('載入路段車流趨勢歷史數據失敗:', error)
  }
}

const updateTrafficTrendChart = () => {
  const data = trafficTrendHistory.value?.data || []
  if (data.length === 0) {
    trafficTrendChartOption.value = {}
    return
  }

  // 按設備分組數據
  const deviceGroups = data.reduce((groups: Record<string, any>, record: any) => {
    const deviceKey = `${record.stationName} - ${record.deviceName}`
    if (!groups[deviceKey]) {
      groups[deviceKey] = []
    }
    groups[deviceKey].push(record)
    return groups
  }, {})

  const allTimes = Array.from(new Set(data.map((item: any) => item.time))).sort()
  const colors = CHART_COLORS

  const series: any[] = []
  const legendData: string[] = []
  let colorIndex = 0

  Object.entries(deviceGroups).forEach(([deviceName, records]: [string, any]) => {
    if (selectedTrafficDevices.value.length > 0 && !selectedTrafficDevices.value.includes(deviceName)) {
      return
    }

    const timeValueMap: Record<string, { averageSpeed: number; vehicleCount: number }> = {}
    records.forEach((record: any) => {
      timeValueMap[record.time] = {
        averageSpeed: record.averageSpeed || 0,
        vehicleCount: record.vehicleCount || 0
      }
    })

    const deviceColor = colors[colorIndex % colors.length]

    // 添加平均車速系列
    series.push({
      name: `${deviceName} (平均車速)`,
      type: 'line',
      yAxisIndex: 0,
      data: allTimes.map(time => timeValueMap[time]?.averageSpeed ?? 0),
      smooth: true,
      connectNulls: true,
      lineStyle: { color: deviceColor, width: 2 },
      itemStyle: { color: deviceColor },
      symbol: 'circle',
      symbolSize: 4
    })

    // 添加車輛數系列
    series.push({
      name: `${deviceName} (車輛數)`,
      type: 'line',
      yAxisIndex: 1,
      data: allTimes.map(time => timeValueMap[time]?.vehicleCount ?? 0),
      smooth: true,
      connectNulls: false,
      lineStyle: { color: deviceColor, width: 2, type: 'dashed' },
      itemStyle: { color: deviceColor },
      symbol: 'diamond',
      symbolSize: 4
    })

    legendData.push(`${deviceName} (平均車速)`, `${deviceName} (車輛數)`)
    colorIndex++
  })

  trafficTrendChartOption.value = {
    title: { text: '路段車流趨勢 (依設備顯示)', left: 'center' },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'cross' }
    },
    legend: { data: legendData, top: 30, type: 'scroll', orient: 'horizontal' },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'category', data: allTimes, boundaryGap: false },
    yAxis: [
      {
        type: 'value',
        name: '平均車速 (km/h)',
        position: 'left',
        axisLabel: { formatter: '{value} km/h' }
      },
      {
        type: 'value',
        name: '車輛數',
        position: 'right',
        axisLabel: { formatter: '{value}輛' }
      }
    ],
    series
  }
}

const updateCrowdChart = () => {
  updateCrowdCapacityChart()
}

const updateTrafficChart = () => {
  updateTrafficTrendChart()
}

const loadChartData = async () => {
  await Promise.all([
    loadCrowdCapacityHistory(),
    loadTrafficTrendHistory()
  ])
}

// Chart methods
const renewPeopleNumChart = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = selectedStationId.value === '0' ? undefined : Number(selectedStationId.value)
    const response = await CrowdOverviewService.getRecentCapacityHistory(stationId, 86400)
    const historyDatas = response?.data || []

    peopleNumChartHistoryData.value = historyDatas.map(e => e.records || [])

    if (historyDatas.length === 0) return

    // Extract all times and create labels
    const times = historyDatas
      .map(history => history.records || [])
      .flat()
      .map(record => record.time)
    const labels = Array.from(new Set(times)).sort()

    // Create series for each device
    const series = historyDatas.map((history, idx) => {
      const timeValueMap: Record<string, number> = {}

      return {
        name: history.deviceName,
        type: 'line',
        smooth: true,
        data: labels.map(label => timeValueMap[label] ?? null),
        lineStyle: { color: getChartColor(idx) }
      }
    })

    peopleChartOption.value = {
      title: { text: '即時入場人數' },
      tooltip: { trigger: 'axis' },
      legend: { data: historyDatas.map(h => h.deviceName) },
      xAxis: { type: 'category', data: labels },
      yAxis: { type: 'value' },
      series
    }
  } catch (error) {
    console.error('載入人流歷史數據失敗:', error)
  }
}

const renewRoadConditionHistoryChart = async () => {
  if (!selectedStationId.value) return

  try {
    const stationId = selectedStationId.value === '0' ? undefined : Number(selectedStationId.value)
    const response = await ParkingOverviewService.getRecentConversionHistory(stationId, 86400)
    const historyDatas = response?.data || []

    roadConditionHistoryData.value = historyDatas.map(e => e.records || [])

    if (historyDatas.length === 0) return

    const times = historyDatas
      .map(history => history.records || [])
      .flat()
      .map(record => record.time)
    const labels = Array.from(new Set(times)).sort()

    const series = historyDatas.map((history, idx) => {
      const timeValueMap: Record<string, number> = {}

        ; (history.records || []).forEach(record => {
          timeValueMap[record.time] = (100 * (record.parkedNum || 0)) / (record.totalNum || 1)
        })

      return {
        name: history.deviceName,
        type: 'line',
        smooth: true,
        data: labels.map(label => timeValueMap[label] ?? null),
        lineStyle: { color: getChartColor(idx) }
      }
    })

    parkingHistoryChartOption.value = {
      title: { text: '停車場轉換率' },
      tooltip: { trigger: 'axis' },
      legend: { data: historyDatas.map(h => h.deviceName) },
      xAxis: { type: 'category', data: labels },
      yAxis: { type: 'value', max: 100 },
      series
    }
  } catch (error) {
    console.error('載入停車場歷史數據失敗:', error)
  }
}

// Map initialization
const initializeMap = async () => {
  const mapContainer = document.getElementById('station-map')
  console.log('initializeMap called, container:', !!mapContainer, 'existing map:', !!map)

  if (!mapContainer || map) {
    console.log('Map container not found or map already exists')
    return
  }

  try {
    // 清理可能存在的舊地圖
    if (map) {
      map.remove()
      map = null
      markersLayer = null
    }

    // 設置默認座標（台灣中心）
    const defaultLat = station.value?.latitude || 25.29641766098067
    const defaultLng = station.value?.longitude || 121.57495111227038

    console.log('Initializing map with coordinates:', defaultLat, defaultLng)

    // 初始化地圖
    map = L.map('station-map', {
      center: [defaultLat, defaultLng],
      zoom: 14,
      zoomControl: true,
      attributionControl: true
    })

    console.log('Map created:', !!map)

    // 添加地圖圖層
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
      maxZoom: 18,
    }).addTo(map)

    console.log('Tile layer added')

    // 初始化標記圖層
    markersLayer = L.layerGroup().addTo(map)

    console.log('Markers layer added')

    // 強制地圖重新計算大小
    setTimeout(() => {
      if (map) {
        map.invalidateSize()
        console.log('Map size invalidated')
      }
    }, 100)

    // 載入地圖標點
    await loadMapLandmarks()

  } catch (error) {
    console.error('地圖初始化失敗:', error)
  }
}

// 載入地圖標點
const loadMapLandmarks = async () => {
  if (!selectedStationId.value) return

  try {
    // 先檢查 MapService 的方法
    console.log('MapService methods:', Object.getOwnPropertyNames(MapService))

    const response = await MapService.getLandmarks()
    console.log('Map landmarks response:', response)

    landmarks.value = response?.landmarks.filter(
      (landmark: any) => landmark.stationID === Number(selectedStationId.value)
    ) || []

    console.log('Filtered landmarks:', landmarks.value)
    updateMapMarkers()
  } catch (error) {
    console.error('載入地圖標點失敗:', error)
    // 如果載入標點失敗，仍然更新地圖
    updateMapMarkers()
  }
}

// 更新地圖標記
const updateMapMarkers = () => {
  if (!map || !markersLayer) return

  // 清除現有標記
  markersLayer.clearLayers()

  console.log('Updating map markers with landmarks:', landmarks.value.length)

  // 添加新標記
  landmarks.value.forEach((landmark: any) => {
    const icon = getMapIcon(landmark.type, landmark.status)
    const marker = L.marker([landmark.lat, landmark.lng], { icon })
      .bindPopup(`
        <div class="p-2">
          <h4 class="font-semibold">${landmark.name || '設備'}</h4>
          <p class="text-sm text-gray-600">類型: ${landmark.type}</p>
          <p class="text-sm text-gray-600">狀態: ${landmark.status}</p>
        </div>
      `)

    markersLayer.addLayer(marker)
  })

  // 自動調整地圖視圖以顯示所有標記
  nextTick(() => {
    setTimeout(() => {
      fitMapToMarkers()
    }, 100)
  })
}

// 自動居中和縮放地圖到所有可見標點 (參考 HomeMap.vue)
const fitMapToMarkers = () => {
  if (!map || !markersLayer) return

  const allMarkers: L.LatLng[] = []

  // 收集所有可見的標點座標
  markersLayer.eachLayer((layer) => {
    if (layer instanceof L.Marker) {
      allMarkers.push(layer.getLatLng())
    }
  })

  console.log('Fitting map to markers:', allMarkers.length)

  if (allMarkers.length > 0) {
    if (allMarkers.length === 1) {
      // 只有一個標點時，設置適當的縮放級別
      map.setView(allMarkers[0], 15, { animate: true })
    } else {
      // 多個標點時，使用 bounds
      const group = new L.FeatureGroup(
        allMarkers.map(latlng => L.marker(latlng))
      )

      map.fitBounds(group.getBounds(), {
        paddingTopLeft: [20, 20],
        paddingBottomRight: [20, 20],
        animate: true
      })
    }
  } else {
    // 沒有標點時，使用分站座標或默認座標
    const defaultLat = station.value?.latitude || 25.29641766098067
    const defaultLng = station.value?.longitude || 121.57495111227038
    map.setView([defaultLat, defaultLng], 14, { animate: true })
  }
}

// 獲取地圖圖標
const getMapIcon = (type: string, status: string) => {
  let iconUrl = '/images/icons/map-marker.svg'
  let iconColor = '#3B82F6'

  switch (type) {
    case 'crowd':
      iconUrl = '/images/icons/crowd.svg'
      iconColor = status === 'online' ? '#10B981' : '#EF4444'
      break
    case 'parking':
      iconUrl = '/images/icons/parking.svg'
      iconColor = status === 'online' ? '#10B981' : '#EF4444'
      break
    case 'traffic':
      iconUrl = '/images/icons/traffic.svg'
      iconColor = status === 'online' ? '#10B981' : '#EF4444'
      break
    case 'fence':
      iconUrl = '/images/icons/fence.svg'
      iconColor = status === 'online' ? '#10B981' : '#EF4444'
      break
  }

  return L.divIcon({
    html: `<div style="background-color: ${iconColor}; border-radius: 50%; width: 24px; height: 24px; display: flex; align-items: center; justify-content: center; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.2);">
      <img src="${iconUrl}" style="width: 12px; height: 12px; filter: brightness(0) invert(1);" />
    </div>`,
    className: 'custom-div-icon',
    iconSize: [24, 24],
    iconAnchor: [12, 12]
  })
}

// Event handlers
const openFenceEventModal = async (id: string | number) => {
  const record = fenceRecords.value.find(record => record.id == id)
  if (record?.imageUrl) {
    fenceEventImageUrl.value = record.imageUrl
  }
}

const closeFenceEventModal = () => {
  fenceEventImageUrl.value = null
}

const onStationChange = async () => {
  console.log('Station changed to:', selectedStationId.value)
  if (selectedStationId.value) {
    // 更新路由到新的分站
    const currentStationId = route.params.id as string
    if (currentStationId !== selectedStationId.value) {
      console.log('Updating route to station:', selectedStationId.value)
      await router.push(`/stations/${selectedStationId.value}`)
    }

    await loadStationData()

    // 重新初始化地圖如果需要
    if (!map) {
      await nextTick()
      setTimeout(async () => {
        await initializeMap()
      }, 200)
    }
  }
}

const refreshData = async () => {
  if (loading.value || !selectedStationId.value) return

  loading.value = true
  try {
    await loadStationData()
  } finally {
    loading.value = false
  }
}

const loadStationData = async () => {
  if (!selectedStationId.value) {
    console.log('No station selected, skipping data load')
    return
  }

  console.log('Loading station data for station:', selectedStationId.value)

  try {
    // 依序載入數據，便於調試
    await loadStationDetail()
    console.log('Station detail loaded')

    await refreshCrowdRecords()
    console.log('Crowd records loaded')

    await refreshParkingRecords()
    console.log('Parking records loaded')

    await refreshFenceRecords()
    console.log('Fence records loaded')

    await refreshTrafficRecords()
    console.log('Traffic records loaded')

    await loadHighResolution()
    console.log('High resolution loaded')

    // 載入圖表數據
    await Promise.all([
      renewPeopleNumChart(),
      renewRoadConditionHistoryChart(),
      loadChartData()
    ])
    console.log('Chart data loaded')

  } catch (error) {
    console.error('Error loading station data:', error)
  }
}

const goBack = () => {
  router.push('/')
}

// Auto-refresh setup
const startAutoRefresh = () => {
  if (refreshInterval) return

  refreshInterval = setInterval(() => {
    if (selectedStationId.value) {
      loadStationData()
    }
  }, 15000) // 15 seconds like original
}

const stopAutoRefresh = () => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
    refreshInterval = null
  }
}

// Watchers
watch(selectedStationId, (newStationId) => {
  console.log('selectedStationId changed:', newStationId)
  if (newStationId) {
    // 防止無限循環：只有當路由參數與選中分站不一致時才更新路由
    const currentRouteStationId = route.params.id as string
    if (currentRouteStationId !== newStationId) {
      onStationChange()
    } else {
      // 如果路由參數已經一致，直接載入數據
      loadStationData()
    }
  }
})

// 監聽路由變化，支援 /stations/:id 格式
watch(() => route.params.id, async (newStationId) => {
  console.log('Route params changed:', newStationId)
  if (newStationId && newStationId !== selectedStationId.value && stations.value.length > 0) {
    await setStationFromRoute()
  }
})

// 監聽分站列表變化，確保在分站載入後設置正確的分站
watch(stations, async (newStations) => {
  if (newStations.length > 0 && !selectedStationId.value) {
    await setStationFromRoute()
  }
}, { immediate: true })

// 設置選中的分站 (提取為獨立函數)
const setStationFromRoute = async () => {
  const routeStationId = route.params.id as string
  console.log('Route station ID:', routeStationId)
  console.log('Available stations:', stations.value.map(s => ({ id: s.id, name: s.name })))

  if (routeStationId) {
    const stationExists = stations.value.some(s => s.id?.toString() === routeStationId)
    if (stationExists) {
      selectedStationId.value = routeStationId
      console.log('Station set from route:', routeStationId)
      return true
    } else {
      console.warn('Station not found in route:', routeStationId)
    }
  }

  // 如果沒有路由參數或路由中的分站不存在，選擇第一個分站
  if (stations.value.length > 0) {
    const firstStationId = stations.value[0].id?.toString() || ''
    selectedStationId.value = firstStationId
    console.log('Using first available station:', firstStationId)

    // 如果 URL 沒有分站 ID，更新路由到第一個分站
    if (!routeStationId && firstStationId) {
      console.log('No station in route, redirecting to first station:', firstStationId)
      await router.replace(`/stations/${firstStationId}`)
    }

    return true
  }

  return false
}

// Lifecycle
onMounted(async () => {
  console.log('StationDashboard mounted')

  await loadStations()

  // 設置選中的分站
  await setStationFromRoute()

  // 等待 DOM 完全渲染後初始化地圖
  await nextTick()
  setTimeout(async () => {
    const mapContainer = document.getElementById('station-map')
    console.log('Map container found:', !!mapContainer, mapContainer?.offsetWidth, mapContainer?.offsetHeight)

    if (mapContainer && mapContainer.offsetWidth > 0 && mapContainer.offsetHeight > 0 && !map) {
      console.log('Initializing map...')
      await initializeMap()
    }
  }, 100)

  // Start auto-refresh
  startAutoRefresh()
})

onUnmounted(() => {
  stopAutoRefresh()

  // 清理地圖
  if (map) {
    map.remove()
    map = null
    markersLayer = null
  }
})
</script>

<style scoped>
/* Leaflet 地圖樣式 */
#station-map {
  height: 100%;
  width: 100%;
  min-height: 256px;
  border-radius: 0.5rem;
  z-index: 1;
}

/* 確保地圖容器有正確的尺寸 */
:deep(.leaflet-container) {
  height: 100% !important;
  width: 100% !important;
  border-radius: 0.5rem;
}

/* Leaflet 控制項樣式 */
:deep(.leaflet-control-zoom a) {
  background-color: white;
  border: 1px solid #ccc;
  color: #333;
}

:deep(.leaflet-control-zoom a:hover) {
  background-color: #f5f5f5;
}

/* 自定義圖標樣式 */
:deep(.custom-div-icon) {
  background: transparent;
  border: none;
}

/* Leaflet popup 樣式 */
:deep(.leaflet-popup-content-wrapper) {
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

:deep(.leaflet-popup-content) {
  margin: 0;
  padding: 8px;
  font-family: inherit;
}

:deep(.leaflet-popup-tip) {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}
</style>
