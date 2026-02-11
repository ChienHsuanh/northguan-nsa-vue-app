<template>
  <DashboardCard :class="`flex flex-col ${props.class}`">
    <h2 class="text-md font-bold text-gray-700 mb-4">{{ title }}</h2>
    <div v-if="chartData.length > 0" class="flex-1 -ml-4">
      <EChartsWrapper :option="areaChartOption" width="100%" height="100%" />
    </div>
    <div v-else class="flex-1 flex items-center justify-center">
      <span class="text-sm text-gray-400">暫無數據</span>
    </div>
  </DashboardCard>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { EChartsOption } from 'echarts'
import { DashboardCard } from '@/components/ui/dashboard-card'
import EChartsWrapper from '@/components/charts/EChartsWrapper.vue'

interface FlowChartData {
  time: string
  value: number
}

interface Props {
  title: string
  data: FlowChartData[]
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  class: ''
})

const chartData = computed(() => {
  // 只使用真實傳入的數據，不生成假數據
  return props.data || []
})

const areaChartOption = computed<EChartsOption>(() => ({
  grid: {
    top: 5,
    right: 20,
    left: 0,
    bottom: 20,
    containLabel: true
  },
  xAxis: {
    type: 'category',
    data: chartData.value.map(item => item.time),
    axisLabel: {
      fontSize: 10,
      interval: 0, // Show all labels for station names
      rotate: 45,
      overflow: 'truncate',
      width: 60
    }
  },
  yAxis: {
    type: 'value',
    min: 0,
    max: 100,
    axisLabel: {
      fontSize: 10,
      formatter: '{value}%'
    }
  },
  series: [
    {
      name: '當前數據',
      type: 'line',
      data: chartData.value.map(item => item.value),
      smooth: true,
      // ** 區域顏色更改為藍色系 **
      areaStyle: {
        color: {
          type: 'linear',
          x: 0,
          y: 0,
          x2: 0,
          y2: 1,
          colorStops: [
            // offset 0 (頂部): 使用較亮的藍色 (例如 #3b82f6 是 blue-500) 加上 80% 透明度
            { offset: 0, color: 'rgba(59, 130, 246, 0.8)' },
            // offset 1 (底部): 顏色完全透明
            { offset: 1, color: 'rgba(59, 130, 246, 0)' }
          ]
        }
      },
      // ** 線條顏色更改為藍色系 **
      lineStyle: {
        color: '#3b82f6' // 藍色主色調
      },
      // ** 點的顏色更改為藍色系 **
      itemStyle: {
        color: '#3b82f6' // 藍色主色調
      }
    }
  ],
  tooltip: {
    trigger: 'axis',
    axisPointer: {
      type: 'cross'
    },
    formatter: (params: any) => {
      const param = params[0]
      // 確保 tooltip 的文字顏色也使用藍色 (如果您的 ECharts Wrapper 支援)
      return `時間: ${param.axisValue}<br/><span style="color: ${param.color}">${param.seriesName}: ${param.value}%</span>`
    }
  }
}))
</script>