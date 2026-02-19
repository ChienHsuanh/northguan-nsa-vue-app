<template>
  <Dialog :open="isOpen" @update:open="$emit('close')">
    <DialogContent class="max-w-2xl max-h-[90vh] overflow-y-auto">
      <DialogHeader>
        <DialogTitle class="flex items-center space-x-3">
          <div :class="`p-2 rounded-lg ${getTypeIconBg(device?.type || '')}`">
            <component :is="getTypeIcon(device?.type || '')" :class="`w-5 h-5 ${getTypeIconColor(device?.type || '')}`" />
          </div>
          <div>
            <span>設備詳細資訊</span>
            <p class="text-sm font-normal text-gray-600 mt-1">{{ device?.name }}</p>
          </div>
        </DialogTitle>
      </DialogHeader>

      <div v-if="device" class="space-y-6">
        <!-- Basic Information -->
        <div class="bg-gray-50 rounded-lg p-4">
          <h3 class="font-semibold text-gray-900 mb-3 flex items-center">
            <InfoIcon class="w-4 h-4 mr-2" />
            基本資訊
          </h3>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="text-sm font-medium text-gray-600">設備名稱</label>
              <p class="text-gray-900">{{ device.name }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-600">設備類型</label>
              <p class="text-gray-900">{{ getTypeDisplayName(device.type || '') }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-600">設備序號</label>
              <p class="text-gray-900 font-mono">{{ device.serial }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-600">所屬分站</label>
              <p class="text-gray-900">{{ station?.name || '未知分站' }}</p>
            </div>
          </div>
        </div>

        <!-- Location Information -->
        <div class="bg-gray-50 rounded-lg p-4">
          <h3 class="font-semibold text-gray-900 mb-3 flex items-center">
            <MapPinIcon class="w-4 h-4 mr-2" />
            位置資訊
          </h3>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="text-sm font-medium text-gray-600">經度</label>
              <p class="text-gray-900">{{ device.lng?.toFixed(6) || '未設定' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-600">緯度</label>
              <p class="text-gray-900">{{ device.lat?.toFixed(6) || '未設定' }}</p>
            </div>
          </div>
          <div v-if="device.lat && device.lng" class="mt-3">
            <Button @click="openInMap" variant="outline" size="sm">
              <ExternalLinkIcon class="w-4 h-4 mr-2" />
              在地圖中查看
            </Button>
          </div>
        </div>

        <!-- Type-specific Information -->
        <div v-if="hasTypeSpecificInfo" class="bg-gray-50 rounded-lg p-4">
          <h3 class="font-semibold text-gray-900 mb-3 flex items-center">
            <SettingsIcon class="w-4 h-4 mr-2" />
            專屬設定
          </h3>

          <!-- Crowd Device -->
          <div v-if="device.type === 'crowd'" class="space-y-3">
            <div v-if="device.area">
              <label class="text-sm font-medium text-gray-600">監控面積</label>
              <p class="text-gray-900">{{ device.area }} 平方米</p>
            </div>
            <div v-if="device.videoUrl">
              <label class="text-sm font-medium text-gray-600">影像串流網址</label>
              <p class="text-gray-900 break-all">{{ device.videoUrl }}</p>
            </div>
            <div v-if="device.apiUrl">
              <label class="text-sm font-medium text-gray-600">API 網址</label>
              <p class="text-gray-900 break-all">{{ device.apiUrl }}</p>
            </div>
          </div>

          <!-- Parking Device -->
          <div v-if="device.type === 'parking'" class="space-y-3">
            <div v-if="device.numberOfParking">
              <label class="text-sm font-medium text-gray-600">停車位數量</label>
              <p class="text-gray-900">{{ device.numberOfParking }} 個</p>
            </div>
          </div>

          <!-- Traffic Device -->
          <div v-if="device.type === 'traffic'" class="space-y-3">
            <div v-if="device.city">
              <label class="text-sm font-medium text-gray-600">所屬城市</label>
              <p class="text-gray-900">{{ device.city }}</p>
            </div>
            <div v-if="device.eTagNumber">
              <label class="text-sm font-medium text-gray-600">ETag 編號</label>
              <p class="text-gray-900">{{ device.eTagNumber }}</p>
            </div>
            <div v-if="device.speedLimit">
              <label class="text-sm font-medium text-gray-600">速度限制</label>
              <p class="text-gray-900">{{ device.speedLimit }} km/h</p>
            </div>
          </div>

          <!-- Fence Device -->
          <div v-if="device.type === 'fence'" class="space-y-3">
            <div v-if="device.observingTimeStart">
              <label class="text-sm font-medium text-gray-600">監控開始時間</label>
              <p class="text-gray-900">{{ device.observingTimeStart }}</p>
            </div>
            <div v-if="device.observingTimeEnd">
              <label class="text-sm font-medium text-gray-600">監控結束時間</label>
              <p class="text-gray-900">{{ device.observingTimeEnd }}</p>
            </div>
          </div>
        </div>

        <!-- Station Information -->
        <div v-if="station" class="bg-gray-50 rounded-lg p-4">
          <h3 class="font-semibold text-gray-900 mb-3 flex items-center">
            <BuildingIcon class="w-4 h-4 mr-2" />
            分站資訊
          </h3>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="text-sm font-medium text-gray-600">分站名稱</label>
              <p class="text-gray-900">{{ station.name }}</p>
            </div>
            <div v-if="station.lat && station.lng">
              <label class="text-sm font-medium text-gray-600">分站位置</label>
              <p class="text-gray-900">{{ station.lat.toFixed(4) }}, {{ station.lng.toFixed(4) }}</p>
            </div>
          </div>
        </div>
      </div>

      <DialogFooter class="flex justify-between">
        <Button @click="$emit('close')" variant="outline">
          關閉
        </Button>
        <div class="space-x-2">
          <Button @click="editDevice" variant="outline">
            <EditIcon class="w-4 h-4 mr-2" />
            編輯設備
          </Button>
          <Button @click="viewLocation" v-if="device?.lat && device?.lng">
            <MapPinIcon class="w-4 h-4 mr-2" />
            查看位置
          </Button>
        </div>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { DeviceListResponse, StationResponse } from '@/services'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import Button from '@/components/ui/button/Button.vue'
import {
  Info as InfoIcon,
  MapPin as MapPinIcon,
  Settings as SettingsIcon,
  Building as BuildingIcon,
  Edit as EditIcon,
  ExternalLink as ExternalLinkIcon,
  Users as UsersIcon,
  Car as CarIcon,
  Shield as ShieldIcon,
  Monitor as MonitorIcon,
  Server as ServerIcon
} from 'lucide-vue-next'

interface Props {
  isOpen: boolean
  device: DeviceListResponse | null
  station: StationResponse | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  edit: [device: DeviceListResponse]
  viewLocation: [device: DeviceListResponse]
}>()

const deviceTypes = [
  { type: 'crowd', name: '人流辨識', icon: UsersIcon, iconBg: 'bg-blue-100', iconColor: 'text-blue-600' },
  { type: 'parking', name: '停車場監控', icon: CarIcon, iconBg: 'bg-green-100', iconColor: 'text-green-600' },
  { type: 'traffic', name: '車流監控', icon: CarIcon, iconBg: 'bg-yellow-100', iconColor: 'text-yellow-600' },
  { type: 'fence', name: '圍籬監控', icon: ShieldIcon, iconBg: 'bg-red-100', iconColor: 'text-red-600' },
  { type: 'highResolution', name: '4K高解析度', icon: MonitorIcon, iconBg: 'bg-purple-100', iconColor: 'text-purple-600' },
  { type: 'water', name: '水域監控', icon: MonitorIcon, iconBg: 'bg-cyan-100', iconColor: 'text-cyan-600' }
]

const hasTypeSpecificInfo = computed(() => {
  if (!props.device) return false

  switch (props.device.type) {
    case 'crowd':
      return !!(props.device.area || props.device.videoUrl || props.device.apiUrl)
    case 'parking':
      return !!props.device.numberOfParking
    case 'traffic':
      return !!(props.device.city || props.device.eTagNumber || props.device.speedLimit)
    case 'fence':
      return !!(props.device.observingTimeStart || props.device.observingTimeEnd)
    default:
      return false
  }
})

const getTypeDisplayName = (type: string): string => {
  const typeConfig = deviceTypes.find(t => t.type === type)
  return typeConfig?.name || type
}

const getTypeIcon = (type: string) => {
  const typeConfig = deviceTypes.find(t => t.type === type)
  return typeConfig?.icon || ServerIcon
}

const getTypeIconBg = (type: string): string => {
  const typeConfig = deviceTypes.find(t => t.type === type)
  return typeConfig?.iconBg || 'bg-gray-100'
}

const getTypeIconColor = (type: string): string => {
  const typeConfig = deviceTypes.find(t => t.type === type)
  return typeConfig?.iconColor || 'text-gray-600'
}

const editDevice = () => {
  if (props.device) {
    emit('edit', props.device)
  }
}

const viewLocation = () => {
  if (props.device) {
    emit('viewLocation', props.device)
  }
}

const openInMap = () => {
  if (props.device?.lat && props.device?.lng) {
    const url = `https://www.google.com/maps?q=${props.device.lat},${props.device.lng}`
    window.open(url, '_blank')
  }
}
</script>
