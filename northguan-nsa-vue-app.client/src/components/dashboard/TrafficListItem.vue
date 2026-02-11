<template>
  <li @click="$emit('click')" :class="[
    'p-4 cursor-pointer transition-all duration-200',
    isSelected ? 'bg-blue-50 border-l-4 border-blue-500' : 'hover:bg-gray-50'
  ]">
    <div class="flex items-start space-x-4">
      <div class="flex-shrink-0 pt-1">
        <span :class="['h-4 w-4 rounded-full block', statusConfig.color]" />
      </div>
      <div class="flex-grow">
        <h3 class="font-semibold text-gray-800 text-sm leading-tight">{{ trafficSegment.name }}</h3>
        <div class="flex items-center text-gray-500 text-sm mt-2 space-x-2">
          <Gauge class="w-4 h-4" />
          <span>目前車速：</span>
          <div class="flex items-baseline">
            <span v-if="trafficSegment.speed === null" :class="['font-bold text-md', statusConfig.textColor]">
              {{ statusConfig.text }}
            </span>
            <template v-else>
              <span :class="['font-bold text-lg', statusConfig.textColor]">{{ trafficSegment.speed }}</span>
              <span class="text-sm text-gray-500 ml-1">km/hr</span>
            </template>
          </div>
        </div>
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Gauge } from 'lucide-vue-next'
import { TrafficStatus, type TrafficSegment } from '@/types/dashboard'

interface Props {
  trafficSegment: TrafficSegment
  isSelected: boolean
}

const props = defineProps<Props>()

defineEmits<{
  click: []
}>()

const statusConfig = computed(() => {
  switch (props.trafficSegment.status) {
    case TrafficStatus.Smooth:
      return {
        color: 'bg-green-500',
        textColor: 'text-green-600',
        text: '順暢'
      }
    case TrafficStatus.Slow:
      return {
        color: 'bg-orange-400',
        textColor: 'text-orange-500',
        text: '稍擠'
      }
    case TrafficStatus.Congested:
      return {
        color: 'bg-red-500',
        textColor: 'text-red-600',
        text: '壅塞'
      }
    default:
      return {
        color: 'bg-gray-400',
        textColor: 'text-gray-500',
        text: '未知'
      }
  }
})
</script>