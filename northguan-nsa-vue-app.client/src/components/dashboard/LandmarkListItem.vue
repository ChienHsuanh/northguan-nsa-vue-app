<template>
  <li @click="$emit('click')" @mouseenter="$emit('hover')" :class="[
    'p-4 cursor-pointer transition-all duration-200',
    isSelected ? [deviceColors?.bgLight || 'bg-blue-50', 'border-l-4', deviceColors?.border || 'border-blue-500'] : 'hover:bg-gray-50'
  ]">
    <div class="flex items-start space-x-4">
      <div class="flex-shrink-0 pt-1">
        <span :class="['h-4 w-4 rounded-full block', getStatusColor()]" />
      </div>
      <div class="flex-grow">
        <h3 class="font-bold text-gray-800 text-md">{{ landmark.name || '未知設備' }}</h3>
        <div class="flex items-center text-gray-500 text-sm mt-1 space-x-2">
          <MapPin class="w-4 h-4" />
          <span>{{ getLocationText() }}</span>
        </div>
        <div class="flex items-center text-gray-500 text-sm mt-2 space-x-2">
          <component :is="getViewIcon()" class="w-4 h-4" />
          <span>{{ getDetailText() }}</span>
        </div>
        <div v-if="getExtraInfo()" class="text-gray-600 text-sm mt-2 font-medium">
          {{ getExtraInfo() }}
        </div>
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { MapPin, Car, Users, ParkingCircle, Shield, Camera } from 'lucide-vue-next'
import type { ILandmarkItem } from '@/api/client'

import type { DeviceColors } from '@/config/deviceColors'

interface Props {
  landmark: ILandmarkItem
  viewType: 'crowd' | 'traffic' | 'parking' | 'fence' | 'highResolution'
  isSelected: boolean
  deviceColors?: DeviceColors
}

const props = defineProps<Props>()

defineEmits<{
  click: []
  hover: []
}>()

// 根據設備狀態取得顏色
const getStatusColor = () => {
  switch (props.landmark.status) {
    case 'online':
      return 'bg-green-500'
    case 'offline':
      return 'bg-red-500'
    default:
      return 'bg-gray-400'
  }
}

// 根據檢視類型取得圖示
const getViewIcon = () => {
  switch (props.viewType) {
    case 'parking':
      return ParkingCircle
    case 'crowd':
      return Users
    case 'traffic':
      return Car
    case 'fence':
      return Shield
    case 'highResolution':
      return Camera
    default:
      return ParkingCircle
  }
}

// 取得位置文字
const getLocationText = () => {
  if (props.landmark.lat && props.landmark.lng) {
    return `${props.landmark.lat.toFixed(4)}, ${props.landmark.lng.toFixed(4)}`
  }
  return '位置未知'
}

// 取得詳細資訊文字
const getDetailText = () => {
  const status = props.landmark.status === 'online' ? '在線' : '離線'

  switch (props.viewType) {
    case 'parking':
      return `設備狀態: ${status}`
    case 'traffic':
      return `設備狀態: ${status}`
    case 'crowd':
      return `設備狀態: ${status}`
    case 'fence':
      return `設備狀態: ${status}`
    case 'highResolution':
      return `設備狀態: ${status}`
    default:
      return `設備狀態: ${status}`
  }
}

// 取得額外資訊（如車位數、車速等）
const getExtraInfo = () => {
  const landmark = props.landmark

  switch (props.viewType) {
    case 'parking':
      if (landmark.numberOfParking !== null && landmark.numberOfParking !== undefined) {
        return `車位數: ${landmark.numberOfParking} 格`
      }
      if (landmark.area !== null && landmark.area !== undefined) {
        return `停車場面積: ${landmark.area} 平方公尺`
      }
      break
    case 'traffic':
      if (landmark.speedLimit !== null && landmark.speedLimit !== undefined) {
        return `速限: ${landmark.speedLimit} km/hr`
      }
      break
    case 'crowd':
      return ''
    case 'fence':
      if (landmark.stationName) {
        return `監控站: ${landmark.stationName}`
      }
      break
    case 'highResolution':
      if (landmark.videoUrl) {
        return '影像可用'
      }
      break
  }

  // 顯示設備編號或站點ID
  if (landmark.serial) {
    return `設備編號: ${landmark.serial}`
  }
  if (landmark.stationID) {
    return `站點ID: ${landmark.stationID}`
  }

  return ''
}
</script>