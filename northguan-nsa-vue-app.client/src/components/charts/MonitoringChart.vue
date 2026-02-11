<template>
  <DashboardCard>
    <h2 class="text-md font-bold text-gray-700 mb-4">{{ title }}</h2>
    <div v-if="data.length > 0" class="flex items-center justify-between gap-4">
      <div class="relative flex-shrink-0" style="width: 96px; height: 96px;">
        <EChartsWrapper
          :option="pieChartOption"
          width="96px"
          height="96px"
          class="!w-full !h-full !min-h-0"
          style="position: absolute; top: 0; left: 0; min-height: 96px !important;"
        />
        <div class="absolute inset-0 flex items-center justify-center pointer-events-none z-10">
          <span class="text-xs font-bold text-gray-600">{{ activeDataName }}</span>
        </div>
      </div>
      <div class="space-y-2">
        <div 
          v-for="(item, index) in legend" 
          :key="index" 
          class="flex items-center text-xs"
        >
          <div 
            class="w-3 h-3 rounded-sm mr-2" 
            :style="{ backgroundColor: item.color }"
          ></div>
          <span>{{ item.label }}</span>
        </div>
      </div>
    </div>
    <div v-else class="flex items-center justify-center h-24">
      <span class="text-sm text-gray-400">暫無數據</span>
    </div>
  </DashboardCard>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { EChartsOption } from 'echarts'
import { DashboardCard } from '@/components/ui/dashboard-card'
import EChartsWrapper from '@/components/charts/EChartsWrapper.vue'

interface PieChartData {
  name: string
  value: number
}

interface LegendItem {
  color: string
  label: string
}

interface Props {
  title: string
  data: PieChartData[]
  legend: LegendItem[]
}

const props = defineProps<Props>()

const COLORS = ['#007BFF', '#60A5FA', '#0B4F9C']

const activeDataName = computed(() => {
  if (props.data.length === 0) {
    return ''
  }
  return props.data[0]?.name || ''
})

const pieChartOption = computed<EChartsOption>(() => ({
  series: [
    {
      type: 'pie',
      radius: ['55%', '90%'],
      center: ['50%', '50%'],
      data: props.data.map((item, index) => ({
        name: item.name,
        value: item.value,
        itemStyle: {
          color: COLORS[index % COLORS.length]
        }
      })),
      label: {
        show: false
      },
      labelLine: {
        show: false
      },
      emphasis: {
        disabled: true
      },
      silent: false
    }
  ],
  tooltip: {
    trigger: 'item',
    formatter: '{b}: {c} ({d}%)'
  },
  animation: false
}))
</script>