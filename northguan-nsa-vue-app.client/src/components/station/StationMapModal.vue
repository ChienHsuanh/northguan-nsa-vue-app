<template>
  <Dialog :open="isOpen" @update:open="$emit('close')">
    <DialogContent class="sm:max-w-5xl max-h-[90vh] overflow-hidden flex flex-col">
      <DialogHeader class="flex-shrink-0">
        <DialogTitle class="text-center">分站位置 - {{ station?.name }}</DialogTitle>
        <DialogDescription class="text-center">
          <span v-if="station?.lat && station?.lng">
            座標: {{ station.lat.toFixed(6) }}, {{ station.lng.toFixed(6) }}
          </span>
          <span v-else class="text-orange-600">
            此分站尚未設定位置座標
          </span>
        </DialogDescription>
      </DialogHeader>

      <div class="flex-1 overflow-y-auto space-y-4 pr-2">
        <!-- 地圖容器 -->
        <div v-if="station?.lat && station?.lng" class="relative">
          <div
            ref="mapContainer"
            id="station-map"
            class="w-full h-96 rounded-lg border border-gray-200"
          ></div>

          <!-- 地圖載入中 -->
          <div
            v-if="mapLoading"
            class="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center rounded-lg"
          >
            <div class="flex flex-col items-center space-y-2">
              <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
              <span class="text-sm text-gray-600">載入地圖中...</span>
            </div>
          </div>
        </div>

        <!-- 無位置資訊時的提示 -->
        <div v-else class="flex flex-col items-center justify-center h-96 bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
          <MapIcon class="w-16 h-16 text-gray-400 mb-4" />
          <h3 class="text-lg font-medium text-gray-900 mb-2">無位置資訊</h3>
          <p class="text-gray-600 text-center max-w-sm">
            此分站尚未設定位置座標，請先在編輯分站時設定緯度和經度。
          </p>
        </div>

        <!-- 分站資訊卡片 -->
        <div class="bg-gray-50 rounded-lg p-4 flex-shrink-0">
          <h4 class="font-medium text-gray-900 mb-3">分站資訊</h4>
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 text-sm">
            <div class="flex flex-col">
              <span class="text-gray-600 text-xs">分站名稱</span>
              <span class="font-medium truncate">{{ station?.name || '未知' }}</span>
            </div>
            <div class="flex flex-col">
              <span class="text-gray-600 text-xs">管理人員</span>
              <span class="truncate" :title="managers.map(m => m.name).join(', ')">
                <span v-if="managers.length > 0" class="font-medium">
                  {{ managers.map(m => m.name).join(', ') }}
                </span>
                <span v-else class="text-gray-400">無管理人員</span>
              </span>
            </div>
            <div class="flex flex-col">
              <span class="text-gray-600 text-xs">設備數量</span>
              <span class="font-medium">{{ deviceCount }} 個</span>
            </div>
          </div>
        </div>

        <!-- 設備列表 -->
        <div v-if="devices.length > 0" class="bg-white rounded-lg border p-4">
          <h4 class="font-medium text-gray-900 mb-3 flex items-center">
            <ServerIcon class="w-4 h-4 mr-2" />
            分站設備 ({{ devices.length }})
          </h4>
          <div class="max-h-64 overflow-y-auto">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
              <div
                v-for="device in devices"
                :key="device.id"
                class="flex items-start space-x-3 p-3 bg-gray-50 rounded-lg min-h-[80px]"
              >
                <div class="flex-shrink-0 mt-1">
                  <div :class="getDeviceIconClass(device.type)" class="w-8 h-8 rounded-full flex items-center justify-center">
                    <component :is="getDeviceIcon(device.type)" class="w-4 h-4" />
                  </div>
                </div>
                <div class="flex-1 min-w-0 space-y-1">
                  <p class="text-sm font-medium text-gray-900 truncate" :title="device.name">
                    {{ device.name }}
                  </p>
                  <p class="text-xs text-gray-500">
                    {{ getDeviceTypeLabel(device.type) }}
                  </p>
                  <p v-if="device.lat && device.lng" class="text-xs text-gray-400 break-all">
                    {{ device.lat.toFixed(4) }}, {{ device.lng.toFixed(4) }}
                  </p>
                  <p v-else class="text-xs text-gray-400">
                    無位置資訊
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <DialogFooter class="flex-shrink-0 flex flex-col sm:flex-row justify-between gap-3 pt-4 border-t">
        <div class="flex flex-wrap gap-2">
          <Button
            v-if="station?.lat && station?.lng"
            variant="outline"
            size="sm"
            @click="openInGoogleMaps"
          >
            <ExternalLinkIcon class="w-4 h-4 mr-2" />
            Google 地圖
          </Button>
          <Button
            v-if="station?.lat && station?.lng"
            variant="outline"
            size="sm"
            @click="copyCoordinates"
          >
            <CopyIcon class="w-4 h-4 mr-2" />
            複製座標
          </Button>
        </div>
        <Button @click="$emit('close')" class="sm:ml-auto">
          關閉
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch, nextTick, onUnmounted } from 'vue'
import { StationResponse, UserResponse, DeviceListResponse } from '@/services'
import { useToast } from '@/composables/useToast'
import Button from '@/components/ui/button/Button.vue'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter
} from '@/components/ui/dialog'
import {
  Map as MapIcon,
  ExternalLink as ExternalLinkIcon,
  Copy as CopyIcon,
  Server as ServerIcon,
  Camera as CameraIcon,
  Radar as RadarIcon,
  Calculator as CalculatorIcon,
  DoorOpen as GateIcon
} from 'lucide-vue-next'

interface Props {
  isOpen: boolean
  station: StationResponse | null
  managers: UserResponse[]
  deviceCount: number
  devices: DeviceListResponse[]
}

interface Emits {
  (e: 'close'): void
}

const props = defineProps<Props>()
defineEmits<Emits>()

// Composables
const toast = useToast()

// State
const mapContainer = ref<HTMLElement>()
const mapLoading = ref(false)
let map: any = null

// 動態載入 Leaflet
const loadLeaflet = async () => {
  if (typeof window !== 'undefined' && !(window as any).L) {
    // 載入 Leaflet CSS
    const link = document.createElement('link')
    link.rel = 'stylesheet'
    link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'
    document.head.appendChild(link)

    // 載入 Leaflet JS
    const script = document.createElement('script')
    script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'

    return new Promise((resolve, reject) => {
      script.onload = resolve
      script.onerror = reject
      document.head.appendChild(script)
    })
  }
}

// 初始化地圖
const initMap = async () => {
  if (!props.station?.lat || !props.station?.lng || !mapContainer.value) return

  try {
    mapLoading.value = true

    // 載入 Leaflet
    await loadLeaflet()

    await nextTick()

    // 清理現有地圖
    if (map) {
      map.remove()
      map = null
    }

    // 清理地圖容器
    if (mapContainer.value) {
      mapContainer.value.innerHTML = ''
      mapContainer.value._leaflet_id = null
    }

    const L = (window as any).L
    if (!L) {
      throw new Error('Leaflet 載入失敗')
    }

    // 創建地圖
    map = L.map(mapContainer.value).setView([props.station.lat, props.station.lng], 15)

    // 添加圖層
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(map)

    // 創建自定義圖標函數
    const createCustomIcon = (color: string, iconHtml: string) => {
      return L.divIcon({
        className: 'custom-marker',
        html: `
          <div style="
            background-color: ${color};
            width: 30px;
            height: 30px;
            border-radius: 50%;
            border: 3px solid white;
            box-shadow: 0 2px 4px rgba(0,0,0,0.3);
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 14px;
          ">
            ${iconHtml}
          </div>
        `,
        iconSize: [30, 30],
        iconAnchor: [15, 15]
      })
    }

    // 添加分站標記
    const stationIcon = createCustomIcon('#ef4444', '🏢') // red for station
    const stationMarker = L.marker([props.station.lat, props.station.lng], { icon: stationIcon })
      .addTo(map)
      .bindPopup(`
        <div class="p-3 min-w-[200px]">
          <h3 class="font-medium text-gray-900 mb-2">${props.station.name}</h3>
          <div class="space-y-1 text-sm text-gray-600">
            <p>📍 座標: ${props.station.lat.toFixed(6)}, ${props.station.lng.toFixed(6)}</p>
            ${props.station.code ? `<p>🏷️ 代碼: ${props.station.code}</p>` : ''}
            <p>📱 設備數量: ${props.devices.length} 個</p>
            ${props.managers.length > 0 ?
              `<p>👥 管理人員: ${props.managers.map(m => m.name).join(', ')}</p>` :
              '<p>👥 管理人員: 無</p>'
            }
          </div>
        </div>
      `)
      .openPopup()

    // 添加設備標記
    const deviceMarkers: any[] = []
    props.devices.forEach(device => {
      if (device.lat && device.lng) {
        const deviceColor = getDeviceMarkerColor(device.type)
        const deviceIcon = createCustomIcon(deviceColor, getDeviceIconSymbol(device.type))

        const deviceMarker = L.marker([device.lat, device.lng], { icon: deviceIcon })
          .addTo(map)
          .bindPopup(`
            <div class="p-3 min-w-[180px]">
              <h4 class="font-medium text-gray-900 mb-2">${device.name}</h4>
              <div class="space-y-1 text-sm text-gray-600">
                <p>類型: ${device.type}</p>
                <p>座標: ${device.lat.toFixed(6)}, ${device.lng.toFixed(6)}</p>
                ${device.observingTime ?
                  `<p>⏰ 最後更新: ${new Date(device.observingTime).toLocaleString('zh-TW', {
                    timeZone: 'Asia/Taipei',
                    year: 'numeric',
                    month: '2-digit',
                    day: '2-digit',
                    hour: '2-digit',
                    minute: '2-digit',
                    second: '2-digit',
                    hour12: false
                  })}</p>` : ''
                }
              </div>
            </div>
          `)

        deviceMarkers.push(deviceMarker)
      }
    })

    // 調整地圖視野以包含所有標記
    if (deviceMarkers.length > 0) {
      const group = new L.featureGroup([stationMarker, ...deviceMarkers])
      map.fitBounds(group.getBounds().pad(0.1))
    }

  } catch (error) {
    console.error('地圖初始化失敗:', error)
    toast.error('地圖載入失敗', '無法載入地圖組件')
  } finally {
    mapLoading.value = false
  }
}

// 在 Google 地圖中開啟
const openInGoogleMaps = () => {
  if (!props.station?.lat || !props.station?.lng) return

  const url = `https://www.google.com/maps?q=${props.station.lat},${props.station.lng}`
  window.open(url, '_blank')
}

// 複製座標
const copyCoordinates = async () => {
  if (!props.station?.lat || !props.station?.lng) return

  const coordinates = `${props.station.lat}, ${props.station.lng}`

  try {
    await navigator.clipboard.writeText(coordinates)
    toast.success('複製成功', '座標已複製到剪貼簿')
  } catch (error) {
    console.error('複製失敗:', error)
    toast.error('複製失敗', '無法複製座標到剪貼簿')
  }
}

// 設備類型相關函數
const getDeviceTypeLabel = (type: string) => {
  const typeMap: Record<string, string> = {
    camera: '攝影機',
    sensor: '感測器',
    counter: '計數器',
    gate: '閘門'
  }
  return typeMap[type] || type
}

const getDeviceIcon = (type: string) => {
  const iconMap: Record<string, any> = {
    camera: CameraIcon,
    sensor: RadarIcon,
    counter: CalculatorIcon,
    gate: GateIcon
  }
  return iconMap[type] || ServerIcon
}

const getDeviceIconClass = (type: string) => {
  const classMap: Record<string, string> = {
    camera: 'bg-blue-100 text-blue-600',
    sensor: 'bg-green-100 text-green-600',
    counter: 'bg-purple-100 text-purple-600',
    gate: 'bg-orange-100 text-orange-600'
  }
  return classMap[type] || 'bg-gray-100 text-gray-600'
}

const getDeviceMarkerColor = (type: string) => {
  const colorMap: Record<string, string> = {
    camera: '#3b82f6',    // blue
    sensor: '#10b981',    // green
    counter: '#8b5cf6',   // purple
    gate: '#f59e0b'       // orange
  }
  return colorMap[type] || '#6b7280' // gray
}

const getDeviceIconSymbol = (type: string) => {
  const symbolMap: Record<string, string> = {
    camera: '📹',
    sensor: '📡',
    counter: '🔢',
    gate: '🚪'
  }
  return symbolMap[type] || '📱'
}

// 清理地圖
const destroyMap = () => {
  try {
    if (map) {
      map.remove()
      map = null
    }

    // 清理地圖容器
    if (mapContainer.value) {
      mapContainer.value.innerHTML = ''
      mapContainer.value._leaflet_id = null
    }
  } catch (error) {
    console.warn('清理分站地圖時發生錯誤:', error)
    map = null
  }
}

// 監聽模態框開啟
watch(() => [props.isOpen, props.station], ([isOpen, station]) => {
  if (isOpen && station?.lat && station?.lng) {
    nextTick(() => {
      initMap()
    })
  } else {
    destroyMap()
  }
}, { immediate: true })

onUnmounted(() => {
  destroyMap()
})
</script>

<style>
/* 確保地圖容器有正確的高度 */
#station-map {
  min-height: 384px;
}

/* 引入 leaflet 修復樣式 */
/* Leaflet styles are already imported globally in index.css */
</style>
