<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- Navigation -->
      <OverviewNavigation current-page="parking" />

      <!-- 停車場轉換率圖表 -->
      <Card class="mb-6">
        <CardHeader>
          <CardTitle>停車場轉換率</CardTitle>
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
            <div v-else-if="!conversionChartOption" class="flex items-center justify-center h-full text-gray-500">
              暫無數據
            </div>
            <div v-else ref="chartContainer" class="w-full h-full"></div>
          </div>
        </CardContent>
      </Card>

      <!-- 停車場停駐率表格 -->
      <Card>
        <CardHeader>
          <CardTitle>停車場停駐率</CardTitle>
        </CardHeader>
        <CardContent class="p-0">
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">名稱</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">分站</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">平均停駐率</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">記錄數</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">時間</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                <tr v-if="loading">
                  <td colspan="5" class="px-4 py-8 text-center text-gray-500">
                    載入中...
                  </td>
                </tr>
                <tr v-else-if="parkingRates.length === 0">
                  <td colspan="5" class="px-4 py-8 text-center text-gray-500">
                    暫無數據
                  </td>
                </tr>
                <tr v-else v-for="rate in parkingRates" :key="`${rate.stationName}-${rate.deviceName}`">
                  <td class="px-4 py-3 text-sm text-gray-900">{{ rate.deviceName }}</td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ rate.stationName }}</td>
                  <td class="px-4 py-3 text-sm">
                    <div class="flex items-center gap-2">
                      <div class="w-3 h-3 rounded-full" :class="getOccupancyStatusColor(rate.averageOccupancyRate)">
                      </div>
                      <span>{{ rate.averageOccupancyRate.toFixed(1) }}%</span>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ rate.totalRecords }}</td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ rate.latestTime }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, nextTick, watch } from 'vue'
import * as echarts from 'echarts'
import type { EChartsOption } from 'echarts'

// Components
import AppHeader from '@/components/layout/AppHeader.vue'
import OverviewNavigation from '@/components/overview/OverviewNavigation.vue'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Checkbox } from '@/components/ui/checkbox'

// Services
import { ParkingOverviewService } from '@/services/overview/ParkingOverviewService'
import { StationService } from '@/services/StationService'

// Utils
import { CHART_COLORS } from "@/utils/chartUtils";
import type { ParkingConversionHistoryResponse, ParkingRateResponse } from '@/services/overview/ParkingOverviewService'

// State
const loading = ref(true)
const chartLoading = ref(true)
const selectedStationId = ref('0')
const timeRangeDays = ref('1')
const chartContainer = ref<HTMLElement>()
let chartInstance: echarts.ECharts | null = null

const stations = ref<any[]>([])
const conversionHistory = ref<ParkingConversionHistoryResponse | null>(null)
const parkingRates = ref<any[]>([])
const availableDevices = ref<string[]>([])
const selectedDevices = ref<string[]>([])

// Chart configuration
const conversionChartOption = computed<EChartsOption | null>(() => {
  if (!conversionHistory.value?.data || conversionHistory.value.data.length === 0) {
    return null
  }

  const data = conversionHistory.value.data

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

    // Create time-value map for this device
    const timeValueMap: Record<string, { occupancyRate: number; parkedNum: number; totalSpaces: number }> = {}
    records.forEach(record => {
      timeValueMap[record.time] = {
        occupancyRate: record.occupancyRate, // Already a percentage from backend
        parkedNum: record.parkedNum,
        totalSpaces: record.totalSpaces
      }
    })

    // Create data arrays aligned with allTimes with last known value fallback
    const occupancyRateData: number[] = []
    const parkedNumData: number[] = []
    let lastKnownOccupancyRate: number | null = null
    let lastKnownParkedNum: number | null = null

    allTimes.forEach(time => {
      if (timeValueMap[time]) {
        // Update last known values when we have data
        lastKnownOccupancyRate = timeValueMap[time].occupancyRate
        lastKnownParkedNum = timeValueMap[time].parkedNum
        occupancyRateData.push(timeValueMap[time].occupancyRate)
        parkedNumData.push(timeValueMap[time].parkedNum)
      } else {
        // Use last known values or 0 if no previous data exists
        occupancyRateData.push(lastKnownOccupancyRate ?? 0)
        parkedNumData.push(lastKnownParkedNum ?? 0)
      }
    })

    const deviceColor = colors[colorIndex % colors.length]

    // Add occupancy rate line series for this device
    series.push({
      name: `${deviceName} (停駐率%)`,
      type: 'line',
      yAxisIndex: 0,
      data: occupancyRateData,
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

    // Add parked vehicles line series for this device
    series.push({
      name: `${deviceName} (停駐車輛)`,
      type: 'line',
      yAxisIndex: 1,
      data: parkedNumData,
      smooth: true,
      connectNulls: true,
      lineStyle: {
        color: deviceColor,
        width: 2,
        type: 'dashed'
      },
      itemStyle: {
        color: deviceColor
      },
      symbol: 'diamond',
      symbolSize: 4
    })

    legendData.push(`${deviceName} (停駐率%)`, `${deviceName} (停駐車輛)`)
    colorIndex++
  })

  return {
    title: {
      text: '停車場轉換率趨勢 (依設備顯示)',
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

        // Group by device
        const deviceData: Record<string, { occupancyRate?: number; parkedNum?: number }> = {}
        params.forEach((param: any) => {
          if (param.value !== null) {
            const deviceName = param.seriesName.replace(/ \((停駐率%|停駐車輛)\)$/, '')
            if (!deviceData[deviceName]) deviceData[deviceName] = {}

            if (param.seriesName.includes('(停駐率%)')) {
              deviceData[deviceName].occupancyRate = param.value
            } else if (param.seriesName.includes('(停駐車輛)')) {
              deviceData[deviceName].parkedNum = param.value
            }
          }
        })

        Object.entries(deviceData).forEach(([deviceName, values]) => {
          tooltip += `<div style="margin: 4px 0;">`
          tooltip += `<span style="color: ${params.find((p: any) => p.seriesName.startsWith(deviceName))?.color};">●</span> `
          tooltip += `<strong>${deviceName}</strong><br/>`
          if (values.occupancyRate !== undefined) {
            tooltip += `&nbsp;&nbsp;停駐率: ${values.occupancyRate.toFixed(1)}%<br/>`
          }
          if (values.parkedNum !== undefined) {
            tooltip += `&nbsp;&nbsp;停駐車輛: ${values.parkedNum} 輛`
          }
          tooltip += `</div>`
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
    yAxis: [
      {
        type: 'value',
        name: '停駐率 (%)',
        position: 'left',
        max: 100,
        axisLabel: {
          formatter: '{value}%'
        }
      },
      {
        type: 'value',
        name: '車輛數',
        position: 'right',
        axisLabel: {
          formatter: '{value}'
        }
      }
    ],
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

const loadConversionHistory = async () => {
  try {
    chartLoading.value = true
    const stationId = selectedStationId.value === '0' ? undefined : parseInt(selectedStationId.value)
    const response = await ParkingOverviewService.getRecentConversionHistory(stationId, parseInt(timeRangeDays.value) * 24 * 60 * 60)
    conversionHistory.value = response

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
    console.error('載入轉換歷史失敗:', error)
    conversionHistory.value = null
    availableDevices.value = []
    selectedDevices.value = []
  } finally {
    chartLoading.value = false
  }
}

const loadParkingRates = async () => {
  try {
    loading.value = true
    const stationId = selectedStationId.value === '0' ? undefined : parseInt(selectedStationId.value)
    const response = await ParkingOverviewService.getRecentParkingRate(stationId, parseInt(timeRangeDays.value) * 24 * 60 * 60, 100)
    parkingRates.value = response.data || []
  } catch (error) {
    console.error('載入停車率失敗:', error)
    parkingRates.value = []
  } finally {
    loading.value = false
  }
}

const loadData = async () => {
  await Promise.all([
    loadConversionHistory(),
    loadParkingRates()
  ])
}

const getOccupancyStatusColor = (rate: number): string => {
  if (rate >= 90) return 'bg-red-500'
  if (rate >= 70) return 'bg-yellow-500'
  return 'bg-green-500'
}

// Chart methods
const initChart = async () => {
  await nextTick()
  if (chartContainer.value && conversionChartOption.value) {
    if (chartInstance) {
      chartInstance.dispose()
    }
    chartInstance = echarts.init(chartContainer.value)
    chartInstance.setOption(conversionChartOption.value)
  }
}

const resizeChart = () => {
  if (chartInstance) {
    chartInstance.resize()
  }
}

const updateChart = async () => {
  await nextTick()
  if (conversionChartOption.value) {
    await initChart()
  }
}

// Watch for chart option changes
watch(conversionChartOption, async (newOption) => {
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
})

// Cleanup
onUnmounted(() => {
  if (chartInstance) {
    chartInstance.dispose()
  }
  window.removeEventListener('resize', resizeChart)
})
</script>
