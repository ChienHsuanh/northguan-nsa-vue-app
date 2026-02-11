<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- Navigation -->
      <OverviewNavigation current-page="traffic" />

      <!-- 路段車流圖表 -->
      <Card class="mb-6">
        <CardHeader>
          <CardTitle>路段車流</CardTitle>
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
            <div v-else-if="!roadConditionChartOption" class="flex items-center justify-center h-full text-gray-500">
              暫無數據
            </div>
            <div v-else ref="chartContainer" class="w-full h-full"></div>
          </div>
        </CardContent>
      </Card>

      <!-- 車流現況表格 -->
      <Card>
        <CardHeader>
          <CardTitle>車流現況</CardTitle>
        </CardHeader>
        <CardContent class="p-0">
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">名稱</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">分站</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">平均車速(km/h)</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">平均車輛數</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">即時路況</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">時間</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                <tr v-if="loading">
                  <td colspan="6" class="px-4 py-8 text-center text-gray-500">
                    載入中...
                  </td>
                </tr>
                <tr v-else-if="roadConditions.length === 0">
                  <td colspan="6" class="px-4 py-8 text-center text-gray-500">
                    暫無數據
                  </td>
                </tr>
                <tr v-else v-for="condition in roadConditions"
                  :key="`${condition.stationName}-${condition.deviceName}`">
                  <td class="px-4 py-3 text-sm text-gray-900">{{ condition.deviceName }}</td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ condition.stationName }}</td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ condition.averageSpeed }}</td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ condition.averageVehicleCount }}</td>
                  <td class="px-4 py-3 text-sm">
                    <div class="flex items-center gap-2">
                      <div class="w-3 h-3 rounded-full" :class="getSpeedStatusColor(condition.speedStatus)"></div>
                      <span>{{ condition.speedStatus }}</span>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">{{ condition.latestTime }}</td>
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
import { TrafficOverviewService } from '@/services/overview/TrafficOverviewService'
import { StationService } from '@/services/StationService'

// Utils
import { CHART_COLORS } from "@/utils/chartUtils";
import type { TrafficRoadConditionHistoryResponse, TrafficRoadConditionResponse } from '@/services/overview/TrafficOverviewService'

// State
const loading = ref(true)
const chartLoading = ref(true)
const selectedStationId = ref('0')
const timeRangeDays = ref('1')
const chartContainer = ref<HTMLElement>()
let chartInstance: echarts.ECharts | null = null

const stations = ref<any[]>([])
const roadConditionHistory = ref<TrafficRoadConditionHistoryResponse | null>(null)
const roadConditions = ref<any[]>([])
const availableDevices = ref<string[]>([])
const selectedDevices = ref<string[]>([])

// Chart configuration
const roadConditionChartOption = computed<EChartsOption | null>(() => {
  if (!roadConditionHistory.value?.data || roadConditionHistory.value.data.length === 0) {
    return null
  }

  const data = roadConditionHistory.value.data

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
    const timeValueMap: Record<string, { averageSpeed: number; vehicleCount: number }> = {}
    records.forEach(record => {
      timeValueMap[record.time] = {
        averageSpeed: parseFloat(record.averageSpeed.toString()),
        vehicleCount: record.vehicleCount
      }
    })

    // Create data arrays aligned with allTimes
    const speedData = allTimes.map(time => timeValueMap[time]?.averageSpeed ?? 0)
    const vehicleCountData = allTimes.map(time => timeValueMap[time]?.vehicleCount ?? 0)

    const deviceColor = colors[colorIndex % colors.length]

    // Add average speed line series for this device
    series.push({
      name: `${deviceName} (平均車速)`,
      type: 'line',
      yAxisIndex: 0,
      data: speedData,
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

    // Add vehicle count line series for this device
    series.push({
      name: `${deviceName} (車輛數)`,
      type: 'line',
      yAxisIndex: 1,
      data: vehicleCountData,
      smooth: true,
      connectNulls: false,
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

    legendData.push(`${deviceName} (平均車速)`, `${deviceName} (車輛數)`)
    colorIndex++
  })

  return {
    title: {
      text: '路段車流趨勢 (依設備顯示)',
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
        const deviceData: Record<string, { averageSpeed?: number; vehicleCount?: number }> = {}
        params.forEach((param: any) => {
          if (param.value !== null) {
            const deviceName = param.seriesName.replace(/ \((平均車速|車輛數)\)$/, '')
            if (!deviceData[deviceName]) deviceData[deviceName] = {}

            if (param.seriesName.includes('(平均車速)')) {
              deviceData[deviceName].averageSpeed = param.value
            } else if (param.seriesName.includes('(車輛數)')) {
              deviceData[deviceName].vehicleCount = param.value
            }
          }
        })

        Object.entries(deviceData).forEach(([deviceName, values]) => {
          tooltip += `<div style="margin: 4px 0;">`
          tooltip += `<span style="color: ${params.find((p: any) => p.seriesName.startsWith(deviceName))?.color};">●</span> `
          tooltip += `<strong>${deviceName}</strong><br/>`
          if (values.averageSpeed !== undefined) {
            tooltip += `&nbsp;&nbsp;平均車速: ${values.averageSpeed.toFixed(1)} km/h<br/>`
          }
          if (values.vehicleCount !== undefined) {
            tooltip += `&nbsp;&nbsp;車輛數: ${values.vehicleCount} 輛`
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
        name: '車速 (km/h)',
        position: 'left',
        axisLabel: {
          formatter: '{value} km/h'
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

const loadRoadConditionHistory = async () => {
  try {
    chartLoading.value = true
    const stationId = selectedStationId.value === '0' ? undefined : parseInt(selectedStationId.value)
    const response = await TrafficOverviewService.getRecentRoadConditionHistory(stationId, parseInt(timeRangeDays.value) * 24 * 60 * 60)
    roadConditionHistory.value = response

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
    console.error('載入路況歷史失敗:', error)
    roadConditionHistory.value = null
    availableDevices.value = []
    selectedDevices.value = []
  } finally {
    chartLoading.value = false
  }
}

const loadRoadConditions = async () => {
  try {
    loading.value = true
    const stationId = selectedStationId.value === '0' ? undefined : parseInt(selectedStationId.value)
    const response = await TrafficOverviewService.getRecentRoadCondition(stationId, parseInt(timeRangeDays.value) * 24 * 60 * 60, 100)
    roadConditions.value = response.data || []
  } catch (error) {
    console.error('載入路況失敗:', error)
    roadConditions.value = []
  } finally {
    loading.value = false
  }
}

const loadData = async () => {
  await Promise.all([
    loadRoadConditionHistory(),
    loadRoadConditions()
  ])
}

const getSpeedStatusColor = (status: string): string => {
  switch (status) {
    case '擁擠':
      return 'bg-red-500'
    case '稍擠':
      return 'bg-yellow-500'
    case '正常':
      return 'bg-green-500'
    default:
      return 'bg-gray-500'
  }
}

// Chart methods
const initChart = async () => {
  await nextTick()
  if (chartContainer.value && roadConditionChartOption.value) {
    if (chartInstance) {
      chartInstance.dispose()
    }
    chartInstance = echarts.init(chartContainer.value)
    chartInstance.setOption(roadConditionChartOption.value)
  }
}

const resizeChart = () => {
  if (chartInstance) {
    chartInstance.resize()
  }
}

const updateChart = async () => {
  await nextTick()
  if (roadConditionChartOption.value) {
    await initChart()
  }
}

// Watch for chart option changes
watch(roadConditionChartOption, async (newOption) => {
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
