<template>
  <DashboardCard :class="`flex flex-col ${props.class}`">
    <h2 class="text-md font-bold text-gray-700 mb-4">{{ title }}</h2>
    <div v-if="chartData.length > 0" class="flex-1 -ml-4">
      <EChartsWrapper :option="barChartOption" width="100%" height="100%" />
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

interface StatusChartData {
  station: string
  value: number
}

interface Props {
  title: string
  data: StatusChartData[]
  class?: string
  type?: 'parking' | 'crowd' | 'traffic' | 'fence'
}

const props = withDefaults(defineProps<Props>(), {
  class: '',
  type: 'parking'
})

const chartData = computed(() => {
  return props.data || []
})

// Get colors based on chart type
const getChartColors = (type: string) => {
  switch (type) {
    case 'parking':
      return {
        primary: '#3b82f6', // blue
        gradient: ['rgba(59, 130, 246, 0.8)', 'rgba(59, 130, 246, 0.3)']
      }
    case 'crowd':
      return {
        primary: '#10b981', // green
        gradient: ['rgba(16, 185, 129, 0.8)', 'rgba(16, 185, 129, 0.3)']
      }
    case 'traffic':
      return {
        primary: '#f59e0b', // amber
        gradient: ['rgba(245, 158, 11, 0.8)', 'rgba(245, 158, 11, 0.3)']
      }
    case 'fence':
      return {
        primary: '#ef4444', // red
        gradient: ['rgba(239, 68, 68, 0.8)', 'rgba(239, 68, 68, 0.3)']
      }
    default:
      return {
        primary: '#6b7280', // gray
        gradient: ['rgba(107, 114, 128, 0.8)', 'rgba(107, 114, 128, 0.3)']
      }
  }
}

const colors = computed(() => getChartColors(props.type))

const barChartOption = computed<EChartsOption>(() => ({
  grid: {
    top: 20,
    right: 0,
    left: 20,
    bottom: 0,
    containLabel: true
  },
  xAxis: {
    type: 'category',
    data: chartData.value.map(item => item.station),
    axisLabel: {
      fontSize: 11,
      interval: 0, // Show all labels
      rotate: 45,
      overflow: 'truncate',
      color: '#374151' // 深灰色確保可讀性
    },
    axisLine: {
      lineStyle: {
        color: '#e5e7eb'
      }
    }
  },
  yAxis: {
    type: 'value',
    min: 0,
    max: 100,
    axisLabel: {
      fontSize: 10,
      formatter: '{value}%'
    },
    axisLine: {
      show: false
    },
    splitLine: {
      lineStyle: {
        color: '#f3f4f6',
        type: 'dashed'
      }
    }
  },
  series: [
    {
      name: '當前狀態',
      type: 'bar',
      data: chartData.value.map(item => item.value),
      itemStyle: {
        color: {
          type: 'linear',
          x: 0,
          y: 0,
          x2: 0,
          y2: 0.8, // 縮小漸層範圍，避免影響底部文字
          colorStops: [
            { offset: 0, color: colors.value.gradient[0] },
            { offset: 1, color: colors.value.gradient[1] }
          ]
        },
        borderRadius: [4, 4, 0, 0] // Rounded top corners
      },
      emphasis: {
        itemStyle: {
          color: colors.value.primary
        }
      },
      barWidth: '60%'
    }
  ],
  tooltip: {
    trigger: 'axis',
    axisPointer: {
      type: 'shadow'
    },
    formatter: (params: any) => {
      const param = params[0]
      const unitMap = {
        parking: '使用率',
        crowd: '容流量',
        traffic: '車流量',
        fence: '警示狀態'
      }
      const unit = unitMap[props.type] || '數值'
      return `${param.axisValue}<br/><span style="color: ${colors.value.primary}">${unit}: ${param.value}%</span>`
    }
  }
}))
</script>