<template>
  <Card class="h-full">
    <CardHeader class="pb-3">
      <CardTitle class="flex items-center gap-2 text-lg">
        <component :is="icon" class="h-5 w-5" />
        {{ title }}
      </CardTitle>
    </CardHeader>
    <CardContent class="pt-0">
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 10" :key="i" class="animate-pulse">
          <div class="h-12 bg-gray-200 rounded"></div>
        </div>
      </div>

      <div v-else-if="items.length === 0" class="text-center py-8 text-gray-500">
        暫無資料
      </div>

      <div v-else class="space-y-3">
        <div
          v-for="item in items"
          :key="item.id"
          class="flex items-center gap-3 p-3 border rounded-lg hover:bg-gray-50 cursor-pointer transition-colors"
          @click="$emit('itemClick', item)"
        >
          <div class="flex-shrink-0">
            <img :src="getItemIcon(item.status)" :alt="item.status" class="w-6 h-6">
          </div>

          <div class="flex-1 min-w-0">
            <div class="font-medium text-sm truncate">{{ item.deviceName }}</div>
            <div :class="['text-xs', getTrafficStatusClass(item.rate)]">
              {{ getTrafficStatusText(item.rate) }}
            </div>
          </div>

          <div class="flex-shrink-0 w-16">
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div
                class="h-2 rounded-full transition-all duration-300"
                :style="{
                  width: Math.min(100, Math.max(0, item.rate)) + '%',
                  backgroundColor: getTrafficStatusColor(item.rate)
                }"
              ></div>
            </div>
            <div :class="['text-xs text-center mt-1', getTrafficStatusClass(item.rate)]">{{ item.rate }}%</div>
          </div>
        </div>
      </div>
    </CardContent>
  </Card>
</template>

<script setup lang="ts">
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { getStatusText, getStatusClass, getStatusColor, getParkingIcon, getCrowdIcon, getTrafficIcon } from '@/utils/statusUtils'
import type { HotspotItem } from '@/composables/useHomeData'

interface Props {
  title: string
  items: HotspotItem[]
  loading: boolean
  type: 'parking' | 'crowd' | 'traffic'
  icon: any
}

const props = defineProps<Props>()

defineEmits<{
  itemClick: [item: HotspotItem]
}>()

const getItemIcon = (status: string): string => {
  switch (props.type) {
    case 'parking':
      return getParkingIcon(status)
    case 'crowd':
      return getCrowdIcon(status)
    case 'traffic':
      return getTrafficIcon(status)
    default:
      return ''
  }
}

// 根據使用率顯示交通狀況
const getTrafficStatusText = (rate: number): string => {
  if (rate >= 70) return '擁擠'
  if (rate >= 30) return '稍擠'
  return '暢通'
}

const getTrafficStatusClass = (rate: number): string => {
  if (rate >= 70) return 'text-red-600'
  if (rate >= 30) return 'text-yellow-600'
  return 'text-green-600'
}

const getTrafficStatusColor = (rate: number): string => {
  if (rate >= 70) return '#dc2626' // red-600
  if (rate >= 30) return '#d97706' // yellow-600
  return '#16a34a' // green-600
}
</script>
