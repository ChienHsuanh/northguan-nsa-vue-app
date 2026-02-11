<template>
  <Dialog :open="isOpen" @update:open="$emit('close')">
    <DialogContent class="max-w-4xl max-h-[90vh]">
      <DialogHeader>
        <DialogTitle class="flex items-center space-x-3">
          <MapPinIcon class="w-5 h-5 text-blue-600" />
          <div>
            <span>設備位置</span>
            <p class="text-sm font-normal text-gray-600 mt-1">{{ device?.name }}</p>
          </div>
        </DialogTitle>
        <DialogDescription id="device-location-description">
          查看設備在地圖上的位置座標和相關資訊
        </DialogDescription>
      </DialogHeader>

      <div v-if="device" class="space-y-4">
        <!-- Location Info -->
        <div class="bg-gray-50 rounded-lg p-4">
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div>
              <label class="font-medium text-gray-600">經度</label>
              <p class="text-gray-900">{{ device.lng?.toFixed(6) || '未設定' }}</p>
            </div>
            <div>
              <label class="font-medium text-gray-600">緯度</label>
              <p class="text-gray-900">{{ device.lat?.toFixed(6) || '未設定' }}</p>
            </div>
            <div>
              <label class="font-medium text-gray-600">設備類型</label>
              <p class="text-gray-900">{{ getTypeDisplayName(device.type || '') }}</p>
            </div>
            <div>
              <label class="font-medium text-gray-600">所屬分站</label>
              <p class="text-gray-900">{{ device.stationName || '未知分站' }}</p>
            </div>
          </div>
        </div>

        <!-- Map Container -->
        <div class="relative">
          <div v-if="!device.lat || !device.lng" class="h-96 bg-gray-100 rounded-lg flex items-center justify-center">
            <div class="text-center">
              <MapPinIcon class="w-12 h-12 text-gray-400 mx-auto mb-4" />
              <p class="text-gray-600">此設備尚未設定位置座標</p>
              <Button @click="$emit('edit', device)" variant="outline" size="sm" class="mt-4">
                <EditIcon class="w-4 h-4 mr-2" />
                編輯設備位置
              </Button>
            </div>
          </div>

          <div v-else :id="mapContainerId" class="h-96 rounded-lg border bg-gray-200"
            style="height: 384px; width: 100%; position: relative; z-index: 1; min-height: 384px;"></div>
        </div>

        <!-- Quick Actions -->
        <div class="flex flex-wrap gap-2">
          <Button v-if="device.lat && device.lng" @click="openInGoogleMaps" variant="outline" size="sm">
            <ExternalLinkIcon class="w-4 h-4 mr-2" />
            在 Google Maps 中開啟
          </Button>

          <Button v-if="device.lat && device.lng" @click="copyCoordinates" variant="outline" size="sm">
            <CopyIcon class="w-4 h-4 mr-2" />
            複製座標
          </Button>

          <Button @click="$emit('edit', device)" variant="outline" size="sm">
            <EditIcon class="w-4 h-4 mr-2" />
            編輯位置
          </Button>
        </div>
      </div>

      <DialogFooter>
        <Button @click="$emit('close')" variant="outline">
          關閉
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import { DeviceListResponse } from '@/services'
import { useToast } from '@/composables/useToast'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import Button from '@/components/ui/button/Button.vue'
import {
  MapPin as MapPinIcon,
  Edit as EditIcon,
  ExternalLink as ExternalLinkIcon,
  Copy as CopyIcon,
  Users as UsersIcon,
  Car as CarIcon,
  Shield as ShieldIcon,
  Monitor as MonitorIcon,
  Server as ServerIcon
} from 'lucide-vue-next'

interface Props {
  isOpen: boolean
  device: DeviceListResponse | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  edit: [device: DeviceListResponse]
}>()

const toast = useToast()

let map: any = null
let marker: any = null

// 生成唯一的地圖容器 ID - 使用穩定的 ID
const mapContainerId = computed(() => `device-map-${props.device?.id || 'default'}`)

const deviceTypes = [
  { type: 'crowd', name: '人流辨識', icon: UsersIcon },
  { type: 'parking', name: '停車場監控', icon: CarIcon },
  { type: 'traffic', name: '車流監控', icon: CarIcon },
  { type: 'fence', name: '圍籬監控', icon: ShieldIcon },
  { type: 'highResolution', name: '4K高解析度', icon: MonitorIcon }
]

const getTypeDisplayName = (type: string): string => {
  const typeConfig = deviceTypes.find(t => t.type === type)
  return typeConfig?.name || type
}

const initMap = async () => {
  if (!props.device?.lat || !props.device?.lng) return

  try {
    // 確保之前的地圖實例已完全清理
    destroyMap()

    // 確保 DOM 元素存在
    const containerId = mapContainerId.value
    const mapContainer = document.getElementById(containerId)

    if (!mapContainer) {
      toast.error('錯誤', '找不到地圖容器')
      return
    }

    // 清理容器內容
    mapContainer.innerHTML = '';
    (mapContainer as any)._leaflet_id = null

    // 動態載入 Leaflet
    const L = await import('leaflet')

    // 初始化地圖
    map = L.map(containerId).setView([props.device.lat, props.device.lng], 15)

    // 添加地圖圖層
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(map)

    // 添加設備標記
    marker = L.marker([props.device.lat, props.device.lng])
      .addTo(map)
      .bindPopup(`
        <div class="text-center">
          <strong>${props.device.name}</strong><br>
          <small>${getTypeDisplayName(props.device.type || '')}</small><br>
          <small>座標: ${props.device.lat.toFixed(6)}, ${props.device.lng.toFixed(6)}</small>
        </div>
      `)
      .openPopup()

    // 優化：僅執行一次尺寸調整
    requestAnimationFrame(() => {
      if (map) {
        map.invalidateSize()
      }
    })

  } catch (error) {
    console.error('載入地圖失敗:', error)
    toast.error('錯誤', '無法載入地圖')
  }
}

const destroyMap = () => {
  try {
    if (marker) {
      marker.remove()
      marker = null
    }
    if (map) {
      map.remove()
      map = null
    }
  } catch (error) {
    // 靜默處理錯誤，避免影響性能
    map = null
    marker = null
  }
}

const openInGoogleMaps = () => {
  if (props.device?.lat && props.device?.lng) {
    const url = `https://www.google.com/maps?q=${props.device.lat},${props.device.lng}`
    window.open(url, '_blank')
  }
}

const copyCoordinates = async () => {
  if (props.device?.lat && props.device?.lng) {
    const coordinates = `${props.device.lat}, ${props.device.lng}`
    try {
      await navigator.clipboard.writeText(coordinates)
      toast.success('成功', '座標已複製到剪貼簿')
    } catch (error) {
      console.error('複製失敗:', error)
      toast.error('錯誤', '無法複製座標')
    }
  }
}

// 監聽模態框開啟狀態和設備變化
watch([() => props.isOpen, () => props.device], async ([isOpen, device], [prevIsOpen, prevDevice]) => {
  // 優化：只在必要時重新初始化地圖
  if (isOpen && device?.lat && device?.lng) {
    // 檢查是否需要重新初始化（設備變化或首次開啟）
    const needsReinit = !prevIsOpen || !prevDevice ||
      prevDevice.id !== device.id ||
      prevDevice.lat !== device.lat ||
      prevDevice.lng !== device.lng

    if (needsReinit) {
      await nextTick()
      initMap()
    }
  } else {
    destroyMap()
  }
})

onUnmounted(() => {
  destroyMap()
})
</script>

<style>
/* Leaflet CSS 修復 */
@import 'leaflet/dist/leaflet.css';

.leaflet-container {
  height: 100% !important;
  width: 100% !important;
  position: relative !important;
  z-index: 1 !important;
}

/* 確保地圖容器有正確的尺寸 */
[id^="device-map-"] {
  height: 384px !important;
  width: 100% !important;
  position: relative !important;
  background-color: #ddd;
}

/* 修復對話框內的地圖顯示 */
:deep(.leaflet-map-pane) {
  left: 0 !important;
  top: 0 !important;
}

:deep(.leaflet-tile-pane) {
  left: 0 !important;
  top: 0 !important;
}

:deep(.leaflet-control-container) {
  position: relative !important;
}
</style>
