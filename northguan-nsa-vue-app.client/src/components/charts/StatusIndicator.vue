<template>
  <div class="flex flex-col items-center">
    <div class="relative mb-2" style="width: 80px; height: 80px;">
      <EChartsWrapper :option="pieChartOption" width="80px" height="80px" class="!w-full !h-full !min-h-0"
        style="position: absolute; top: 0; left: 0; min-height: 80px !important;" />
      <div class="absolute inset-0 flex items-center justify-center pointer-events-none z-10">
        <span class="text-sm font-bold" :style="{ color: item.color }">
          {{ item.percentage }}%
        </span>
      </div>
    </div>
    <div class="text-center">
      <div class="px-3 py-1 rounded-full text-xs font-semibold"
        :style="{ backgroundColor: item.bgColor, color: item.color }">
        {{ item.status }}
      </div>
      <p class="text-xs text-gray-500 mt-1">{{ item.label }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { EChartsOption } from 'echarts'
import EChartsWrapper from '@/components/charts/EChartsWrapper.vue'

interface DeviceStatusItem {
  label: string
  percentage: number
  color: string
  bgColor: string
  status: string
}

interface Props {
  item: DeviceStatusItem
}

const props = defineProps<Props>()

const pieChartOption = computed<EChartsOption>(() => ({
  series: [
    {
      type: 'pie',
      radius: ['60%', '90%'],
      center: ['50%', '50%'],
      startAngle: 90,
      data: [
        {
          value: props.item.percentage,
          itemStyle: {
            color: props.item.color
          }
        },
        {
          value: 100 - props.item.percentage,
          itemStyle: {
            color: props.item.bgColor
          }
        }
      ],
      label: {
        show: false
      },
      labelLine: {
        show: false
      },
      emphasis: {
        disabled: true
      },
      silent: true
    }
  ],
  animation: false
}))
</script>