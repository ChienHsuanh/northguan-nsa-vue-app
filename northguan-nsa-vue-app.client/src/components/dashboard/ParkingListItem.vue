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
        <h3 class="font-bold text-gray-800 text-md">{{ parkingLot.name }}</h3>
        <div class="flex items-center text-gray-500 text-sm mt-1 space-x-2">
          <MapPin class="w-4 h-4" />
          <span>{{ parkingLot.address }}</span>
        </div>
        <div class="flex items-center text-gray-500 text-sm mt-2 space-x-2">
          <Car class="w-4 h-4" />
          <span>車位尚餘</span>
          <span :class="['font-bold text-lg', statusConfig.textColor]">{{ parkingLot.availableSpots }}</span>
          <span class="text-gray-400"> (總車位{{ parkingLot.totalSpots }}格)</span>
        </div>
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { MapPin, Car } from 'lucide-vue-next'
import { ParkingStatus, type ParkingLot } from '@/types/dashboard'

interface Props {
  parkingLot: ParkingLot
  isSelected: boolean
}

const props = defineProps<Props>()

defineEmits<{
  click: []
}>()

const statusConfig = computed(() => {
  switch (props.parkingLot.status) {
    case ParkingStatus.Available:
      return {
        color: 'bg-green-500',
        textColor: 'text-green-600'
      }
    case ParkingStatus.Limited:
      return {
        color: 'bg-orange-400',
        textColor: 'text-orange-500'
      }
    case ParkingStatus.Full:
      return {
        color: 'bg-red-500',
        textColor: 'text-red-600'
      }
    default:
      return {
        color: 'bg-gray-400',
        textColor: 'text-gray-500'
      }
  }
})
</script>