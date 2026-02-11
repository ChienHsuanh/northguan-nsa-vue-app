<template>
  <div class="flex-grow h-full relative">
    <div ref="mapContainer" class="h-full w-full" />

    <!-- Map Controls - 移到左邊避免與右邊側邊欄重疊 -->
    <div class="absolute top-4 left-4 z-[999] flex flex-col space-y-2 items-start">
      <div class="bg-white rounded-md shadow-md flex">
        <button @click="toggleMapLayer" :class="[
          'px-4 py-2 text-sm font-semibold rounded-l-md',
          !isSatellite ? 'bg-blue-600 text-white' : 'bg-white text-gray-700'
        ]">
          地圖
        </button>
        <button @click="toggleMapLayer" :class="[
          'px-4 py-2 text-sm font-semibold rounded-r-md',
          isSatellite ? 'bg-blue-600 text-white' : 'bg-white text-gray-700'
        ]">
          衛星
        </button>
      </div>
      <button @click="requestFullscreen" class="bg-white p-2.5 rounded-md shadow-md hover:bg-gray-50">
        <Maximize class="h-5 w-5 text-gray-600" />
      </button>
    </div>

    <!-- Status Legend -->
    <div class="absolute bottom-4 left-16 z-[999] bg-white bg-opacity-90 p-3 rounded-lg shadow-lg">
      <div class="flex items-center space-x-4">
        <div class="text-xs text-center text-gray-600 font-semibold">
          <p>設備狀態</p>
        </div>
        <div class="flex items-center space-x-4">
          <div class="flex items-center space-x-2">
            <div class="w-3 h-3 bg-green-500 rounded-full"></div>
            <span class="text-xs text-gray-600">在線</span>
          </div>
          <div class="flex items-center space-x-2">
            <div class="w-3 h-3 bg-red-500 rounded-full"></div>
            <span class="text-xs text-gray-600">離線</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { Maximize } from 'lucide-vue-next'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import type { ILandmarkItem } from '@/api/client'
import { getDevicePrimaryColor, type DeviceType } from '@/config/deviceColors'

interface Props {
  view: 'crowd' | 'traffic' | 'parking' | 'fence' | 'highResolution'
  landmarks: ILandmarkItem[]
  selectedLandmark: ILandmarkItem | null
  showDeviceSidebar?: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  selectLandmark: [landmark: ILandmarkItem]
  deviceDetail: [deviceId: number, deviceType: 'crowd' | 'traffic' | 'parking' | 'fence' | 'highResolution']
}>()

const mapContainer = ref<HTMLDivElement>()
const mapInstance = ref<L.Map>()
const markers = ref<{ [key: string]: L.Marker }>({})
const isSatellite = ref(false)

let mapLayer: L.TileLayer
let satelliteLayer: L.TileLayer

onMounted(async () => {
  await nextTick()
  initMap()
})

onUnmounted(() => {
  if (mapInstance.value) {
    mapInstance.value.remove()
  }
})

const initMap = () => {
  if (!mapContainer.value) return

  // Initialize map
  const map = L.map(mapContainer.value, { zoomControl: false }).setView([25.2, 121.65], 11)

  // Add zoom control to bottom left to avoid overlap with right sidebar
  L.control.zoom({ position: 'bottomleft' }).addTo(map)

  // Initialize tile layers
  mapLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; OpenStreetMap contributors'
  })

  satelliteLayer = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
    attribution: 'Tiles &copy; Esri'
  })

  // Add default layer
  mapLayer.addTo(map)

  mapInstance.value = map
  updateMarkers()
}

const toggleMapLayer = () => {
  if (!mapInstance.value) return

  if (isSatellite.value) {
    mapInstance.value.removeLayer(satelliteLayer)
    mapInstance.value.addLayer(mapLayer)
  } else {
    mapInstance.value.removeLayer(mapLayer)
    mapInstance.value.addLayer(satelliteLayer)
  }

  isSatellite.value = !isSatellite.value
}

const requestFullscreen = () => {
  if (mapContainer.value?.requestFullscreen) {
    mapContainer.value.requestFullscreen()
  }
}

// 計算考慮側邊欄的地圖置中點
const calculateCenterPoint = (targetLatLng: [number, number]): [number, number] => {
  if (!mapInstance.value || !props.showDeviceSidebar) {
    return targetLatLng
  }

  // 計算側邊欄寬度（在中等螢幕以上為45%，小螢幕為100%但通常不會同時顯示地圖）
  const isMediumScreen = window.innerWidth >= 768 // md breakpoint in Tailwind
  if (!isMediumScreen) {
    return targetLatLng // 小螢幕不需要調整
  }

  const sidebarWidthRatio = 0.45 // 側邊欄佔45%

  // 使用固定的縮放級別和計算，不依賴地圖當前狀態
  // 我們使用目標縮放級別17（flyTo的目標縮放）而不是當前縮放級別
  const targetZoom = 17

  // 計算在縮放級別17下的度數/像素比例
  // 使用地球周長和目標緯度計算經度偏移
  const earthCircumference = 40075017 // 地球周長（米）
  const metersPerPixel = earthCircumference * Math.cos(targetLatLng[0] * Math.PI / 180) / Math.pow(2, targetZoom + 8)

  // 計算需要偏移的米數（使用固定的地圖容器寬度）
  // 使用當前容器寬度，但不依賴地圖狀態
  const mapContainer = mapInstance.value.getContainer()
  const mapWidth = mapContainer.clientWidth
  const offsetPixels = mapWidth * sidebarWidthRatio / 2
  const offsetMeters = offsetPixels * metersPerPixel

  // 轉換為經度偏移
  const offsetDegrees = offsetMeters / (earthCircumference * Math.cos(targetLatLng[0] * Math.PI / 180) / 360)

  return [targetLatLng[0], targetLatLng[1] + offsetDegrees]
}

// Helper functions for landmarks
const getDeviceStatusInfo = (status?: string) => {
  switch (status) {
    case 'online':
      return { text: '在線', color: '#10B981', colorClass: 'bg-green-500', textColorClass: 'text-green-600' }
    case 'offline':
      return { text: '離線', color: '#EF4444', colorClass: 'bg-red-500', textColorClass: 'text-red-600' }
    default:
      return { text: '未知', color: '#9CA3AF', colorClass: 'bg-gray-400', textColorClass: 'text-gray-500' }
  }
}

const createLandmarkIcon = (viewType: string, status?: string, isSelected: boolean = false) => {
  // 1. 獲取狀態顏色
  const { color: statusColor } = getDeviceStatusInfo(status)
  // 獲取設備主要顏色
  const mainColor = getDevicePrimaryColor(viewType as DeviceType)
  // 設定不同狀態下的主要顏色
  const baseColor = mainColor

  // 2. 調整大小和動畫參數
  const baseSize = 40 // 圖標基礎尺寸
  const selectedSize = 48 // 選中時的尺寸
  const size = isSelected ? selectedSize : baseSize
  const halfSize = size / 2

  // 3. 調整陰影和動畫
  const shadow = 'drop-shadow(0 4px 8px rgba(0,0,0,0.2))'
  const selectedShadow = 'drop-shadow(0 8px 16px rgba(0,0,0,0.4))'
  const filterStyle = isSelected ? selectedShadow : shadow
  const transition = 'all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1)'

  // 4. 定義圖標路徑 (保持不變或微調)
  let iconColor = '#ffffff'
  let iconPath = ''
  let viewBox = '0 0 24 24'

  // 圖標 SVG 路徑
  switch (viewType) {
    case 'parking':
      iconPath = `
        <rect x="5" y="6" width="14" height="12" rx="1.5" fill="none" stroke="${iconColor}" stroke-width="1.8"/>
        <path d="M9 10h4v6H9z" fill="${iconColor}"/>
        <circle cx="15" cy="14" r="1.2" fill="${iconColor}"/>
      `
      break
    case 'traffic':
      // 交通號誌燈
      iconPath = `
        <rect x="9" y="4" width="6" height="16" rx="3" fill="none" stroke="${iconColor}" stroke-width="1.8"/>
        <circle cx="12" cy="8" r="1.8" fill="${iconColor}"/>
        <circle cx="12" cy="12" r="1.8" fill="${iconColor}" opacity="0.7"/>
        <circle cx="12" cy="16" r="1.8" fill="${iconColor}" opacity="0.4"/>
      `
      break
    case 'crowd':
      // 兩人圖標
      iconPath = `
        <circle cx="8" cy="8" r="2.5" fill="${iconColor}"/>
        <circle cx="16" cy="8" r="2.5" fill="${iconColor}"/>
        <path d="M5 17c0-2.2 1.8-4 4-4s4 1.8 4 4" fill="none" stroke="${iconColor}" stroke-width="1.8"/>
        <path d="M11 17c0-2.2 1.8-4 4-4s4 1.8 4 4" fill="none" stroke="${iconColor}" stroke-width="1.8"/>
      `
      break
    case 'fence':
      // 圍籬/邊界
      iconPath = `
        <path d="M4 12h16M4 8h16M4 16h16" stroke="${iconColor}" stroke-width="1.8" stroke-linecap="round"/>
        <rect x="3" y="6" width="18" height="12" rx="2" fill="none" stroke="${iconColor}" stroke-width="1.8"/>
        <path d="M12 6v12" stroke="${iconColor}" stroke-width="1.8" stroke-linecap="round"/>
      `
      break
    case 'highResolution':
      // 攝影機
      iconPath = `
        <rect x="5" y="8" width="14" height="10" rx="2" fill="none" stroke="${iconColor}" stroke-width="1.8"/>
        <circle cx="12" cy="13" r="3" fill="none" stroke="${iconColor}" stroke-width="1.8"/>
        <path d="M9 6l2-2h2l2 2" fill="none" stroke="${iconColor}" stroke-width="1.8"/>
      `
      break
    default:
      iconPath = `<circle cx="12" cy="12" r="4" fill="${iconColor}"/>`
  }

  // 5. 優化 DivIcon 的 HTML 結構
  return L.divIcon({
    html: `
      <div style="
        width: ${size}px;
        height: ${size + 15}px; /* 增加底部空間給指針 */
        transform: scale(1); /* 實際縮放由 CSS 控制 */
        filter: ${filterStyle};
        transition: ${transition};
        position: relative;
        transform-origin: center bottom;
      ">
        <div style="
          width: ${size}px;
          height: ${size}px;
          background: ${baseColor}; /* 使用純色或更精緻的漸變 */
          border-radius: ${halfSize}px ${halfSize}px ${halfSize}px 0; /* 調整為更流線的形狀 */
          position: absolute;
          top: 0;
          left: 0;
          display: flex;
          align-items: center;
          justify-content: center;
          transform: rotate(-45deg); /* 旋轉創造水滴視覺 */
          box-shadow: inset 0 0 0 2px rgba(255,255,255,0.2); /* 內陰影增加立體感 */
          ${isSelected ? 'animation: pulse-ring 1.5s infinite;' : ''}
        ">
        </div>

        <div style="
          position: absolute;
          top: 0;
          left: 0;
          width: ${size}px;
          height: ${size}px;
          display: flex;
          align-items: center;
          justify-content: center;
          transform: rotate(45deg); /* 反向旋轉修正 */
        ">
          <svg width="${size * 0.5}" height="${size * 0.5}" viewBox="${viewBox}" fill="none" xmlns="http://www.w3.org/2000/svg" style="transform: rotate(-45deg);">
            ${iconPath}
          </svg>
        </div>

        <div style="
          position: absolute;
          top: 10%;
          right: 10%;
          width: 12px;
          height: 12px;
          background-color: ${statusColor};
          border-radius: 50%;
          box-shadow: 0 0 0 2px ${statusColor}40; /* 狀態光暈 */
          z-index: 10;
          transform: translate(50%, -50%); /* 精準定位 */
        "></div>
      </div>
      <style>
        /* 新增選中動畫 - 脈衝光環 */
        @keyframes pulse-ring {
          0% {
            box-shadow: 0 0 0 0 ${baseColor}80;
          }
          70% {
            box-shadow: 0 0 0 10px ${baseColor}00;
          }
          100% {
            box-shadow: 0 0 0 0 ${baseColor}00;
          }
        }
      </style>
    `,
    className: '',
    iconSize: [size, size + 15],
    iconAnchor: [halfSize, size + 15], // 錨點設置在水滴下方尖端
    popupAnchor: [0, -size - 10], // 彈出視窗稍微往上移
  })
}

// const createLandmarkPopupContent = (landmark: ILandmarkItem, viewType: string): string => {
//   const statusInfo = getDeviceStatusInfo(landmark.status)

//   // 基本資訊
//   let basicInfo = `
//     <div style="padding: 16px; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; min-width: 280px; max-width: 320px;">
//       <h3 style="font-size: 18px; font-weight: bold; color: #1d4ed8; margin: 0 0 12px 0; line-height: 1.3;">${landmark.name || '未知設備'}</h3>

//       <div style="display: flex; align-items: flex-start; color: #6b7280; font-size: 13px; margin-bottom: 8px; gap: 8px;">
//         <svg style="width: 16px; height: 16px; flex-shrink: 0; margin-top: 1px;" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
//           <path stroke-linecap="round" stroke-linejoin="round" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
//           <path stroke-linecap="round" stroke-linejoin="round" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
//         </svg>
//         <span style="line-height: 1.4;">位置: ${landmark.lat?.toFixed(4) || 'N/A'}, ${landmark.lng?.toFixed(4) || 'N/A'}</span>
//       </div>

//       <div style="display: flex; align-items: center; color: #6b7280; font-size: 13px; margin-bottom: 8px; gap: 8px;">
//         <svg style="width: 16px; height: 16px; flex-shrink: 0;" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
//           <path stroke-linecap="round" stroke-linejoin="round" d="M9 12l2 2 4-4" />
//           <path stroke-linecap="round" stroke-linejoin="round" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
//         </svg>
//         <span>ID: ${landmark.id || 'N/A'}</span>
//       </div>

//       <div style="display: flex; align-items: center; color: #6b7280; font-size: 13px; margin-bottom: 16px; gap: 8px;">
//         <svg style="width: 16px; height: 16px; flex-shrink: 0;" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
//           <circle cx="12" cy="12" r="3"/>
//           <path d="M12 1v6m0 6v6"/>
//         </svg>
//         <span>狀態:</span>
//         <div style="width: 12px; height: 12px; border-radius: 50%; background-color: ${statusInfo.color}; margin: 0 4px;"></div>
//         <span style="font-weight: 600; color: ${statusInfo.color};">${statusInfo.text}</span>
//       </div>
//   `

//   // 根據設備類型添加特定資訊
//   let specificInfo = ''
//   switch (viewType) {
//     case 'parking':
//       if (landmark.numberOfParking !== null && landmark.numberOfParking !== undefined) {
//         specificInfo += `
//           <div style="border-top: 1px solid #e5e7eb; padding-top: 12px;">
//             <h4 style="font-weight: 600; color: #374151; margin: 0 0 8px 0; font-size: 14px;">停車場資訊</h4>
//             <div style="background-color: #f3f4f6; padding: 12px; border-radius: 6px;">
//               <div style="color: #7C3AED; font-weight: bold; font-size: 24px; line-height: 1;">${landmark.numberOfParking}</div>
//               <div style="color: #8B5CF6; font-size: 12px; margin-top: 2px;">車位數量</div>
//             </div>
//           </div>
//         `
//       }
//       if (landmark.area !== null && landmark.area !== undefined) {
//         specificInfo += `
//           <div class="mt-3">
//             <div class="bg-violet-50 p-3 rounded">
//               <div class="text-violet-600 font-bold text-lg">${landmark.area}</div>
//               <div class="text-violet-500 text-sm">停車場面積 (平方公尺)</div>
//             </div>
//           </div>
//         `
//       }
//       break
//     case 'traffic':
//       if (landmark.speedLimit !== null && landmark.speedLimit !== undefined) {
//         specificInfo = `
//           <div class="border-t pt-3">
//             <h4 class="font-semibold text-gray-700 mb-2">交通資訊</h4>
//             <div class="bg-blue-50 p-3 rounded">
//               <div class="text-blue-700 font-bold text-2xl">${landmark.speedLimit}</div>
//               <div class="text-red-500 text-sm">速限 (km/hr)</div>
//             </div>
//           </div>
//         `
//       }
//       break
//     case 'crowd':
//       if (landmark.currentPeopleCount !== null && landmark.currentPeopleCount !== undefined) {
//         let additionalInfo = ''
//         if (landmark.totalIn !== null && landmark.totalIn !== undefined) {
//           additionalInfo = `
//             <div style="background-color: #f3e8ff; padding: 8px; border-radius: 4px; margin-top: 8px;">
//               <div style="color: #0F766E; font-weight: bold; font-size: 16px; line-height: 1;">${landmark.totalIn}</div>
//               <div style="color: #7c3aed; font-size: 11px; margin-top: 1px;">累計入場人數</div>
//             </div>
//           `
//         }
//         if (landmark.crowdDensity !== null && landmark.crowdDensity !== undefined) {
//           additionalInfo += `
//             <div style="background-color: #ede9fe; padding: 8px; border-radius: 4px; margin-top: 4px;">
//               <div style="color: #0F766E; font-weight: bold; font-size: 14px; line-height: 1;">${landmark.crowdDensity.toFixed(2)}</div>
//               <div style="color: #7c3aed; font-size: 11px; margin-top: 1px;">人群密度</div>
//             </div>
//           `
//         }

//         specificInfo = `
//           <div style="border-top: 1px solid #e5e7eb; padding-top: 12px;">
//             <h4 style="font-weight: 600; color: #374151; margin: 0 0 8px 0; font-size: 14px;">人流資訊</h4>
//             <div style="background-color: #faf5ff; padding: 12px; border-radius: 6px;">
//               <div style="color: #7c3aed; font-weight: bold; font-size: 24px; line-height: 1;">${landmark.currentPeopleCount}</div>
//               <div style="color: #8b5cf6; font-size: 12px; margin-top: 2px;">目前人數</div>
//               ${additionalInfo}
//             </div>
//             ${landmark.congestionStatus ? `
//               <div style="margin-top: 8px; padding: 8px; background-color: #fef3c7; border-radius: 4px;">
//                 <div style="color: #92400e; font-weight: 600; font-size: 12px;">擁擠狀態: ${landmark.congestionStatus}</div>
//               </div>
//             ` : ''}
//           </div>
//         `
//       } else if (landmark.observingTime) {
//         specificInfo = `
//           <div style="border-top: 1px solid #e5e7eb; padding-top: 12px;">
//             <h4 style="font-weight: 600; color: #374151; margin: 0 0 8px 0; font-size: 14px;">監控資訊</h4>
//             <div style="background-color: #faf5ff; padding: 12px; border-radius: 6px;">
//               <div style="color: #7c3aed; font-weight: bold; font-size: 13px; line-height: 1.3;">${new Date(landmark.observingTime).toLocaleString()}</div>
//               <div style="color: #8b5cf6; font-size: 12px; margin-top: 2px;">最後觀測時間</div>
//             </div>
//           </div>
//         `
//       }
//       break
//     case 'fence':
//       if (landmark.stationName) {
//         specificInfo = `
//           <div class="border-t pt-3">
//             <h4 class="font-semibold text-gray-700 mb-2">圍籬資訊</h4>
//             <div class="bg-indigo-50 p-3 rounded">
//               <div class="text-indigo-700 font-bold text-lg">${landmark.stationName}</div>
//               <div class="text-indigo-600 text-sm">監控站名稱</div>
//             </div>
//           </div>
//         `
//       }
//       if (landmark.stationID) {
//         specificInfo += `
//           <div class="mt-3">
//             <div class="bg-yellow-50 p-3 rounded">
//               <div class="text-yellow-600 font-bold text-lg">${landmark.stationID}</div>
//               <div class="text-yellow-500 text-sm">站點編號</div>
//             </div>
//           </div>
//         `
//       }
//       break
//     case 'highResolution':
//       if (landmark.videoUrl) {
//         specificInfo = `
//           <div class="border-t pt-3">
//             <h4 class="font-semibold text-gray-700 mb-2">影像資訊</h4>
//             <div class="bg-cyan-50 p-3 rounded">
//               <div class="text-cyan-600 font-bold text-sm">✓ 影像可用</div>
//               <div class="text-cyan-500 text-sm">4K 高解析度影像</div>
//             </div>
//           </div>
//         `
//       }
//       break
//   }

//   // 顯示設備編號
//   if (landmark.serial) {
//     specificInfo += `
//       <div class="mt-3 bg-gray-50 p-2 rounded">
//         <div class="text-gray-600 text-xs">設備編號: ${landmark.serial}</div>
//       </div>
//     `
//   }

//   return basicInfo + specificInfo + '</div>'
// }


const updateMarkers = () => {
  if (!mapInstance.value) return

  // Clear existing markers
  Object.values(markers.value).forEach(marker => marker.remove())
  markers.value = {}

  props.landmarks.forEach(landmark => {
    if (!landmark.lat || !landmark.lng) return

    const isSelected = props.selectedLandmark?.id === landmark.id
    const icon = createLandmarkIcon(props.view, landmark.status, isSelected)
    const marker = L.marker([landmark.lat, landmark.lng], { icon }).addTo(mapInstance.value!)

    // marker.bindPopup(createLandmarkPopupContent(landmark, props.view), { className: 'custom-popup' })
    marker.on('click', () => {
      emit('selectLandmark', landmark)
      // 發出設備詳情事件來顯示側邊欄
      if (landmark.id) {
        emit('deviceDetail', landmark.id, props.view)
      }
    })

    markers.value[landmark.id?.toString() || Math.random().toString()] = marker
  })
}

// 重新置中當前選中的地標
const recenterSelectedLandmark = () => {
  if (props.selectedLandmark && props.selectedLandmark.lat && props.selectedLandmark.lng && mapInstance.value) {
    // 計算地圖置中位置，考慮設備側邊欄
    const centerPoint = calculateCenterPoint([props.selectedLandmark.lat, props.selectedLandmark.lng])

    // 居中放大到地標位置
    mapInstance.value.flyTo(centerPoint, 17, {
      duration: 0.75,
      easeLinearity: 0.25
    })
  }
}

// Watch for changes
watch(() => [props.view, props.landmarks], updateMarkers, { deep: true })

watch(() => props.selectedLandmark, (newLandmark) => {
  if (newLandmark && newLandmark.lat && newLandmark.lng && mapInstance.value) {
    recenterSelectedLandmark()
    // 不自動開啟 popup，讓點擊事件來處理
  }
}, { deep: true })

// 監聽側邊欄狀態變化，立即重新置中
watch(() => props.showDeviceSidebar, () => {
  // 當側邊欄展開或收起時，如果有選中的地標，立即重新置中
  if (props.selectedLandmark) {
    recenterSelectedLandmark()
  }
})
</script>

<style>
/* Custom popup styles */
:deep(.custom-popup .leaflet-popup-content-wrapper) {
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
}

:deep(.custom-popup .leaflet-popup-content) {
  margin: 0;
  font-family: inherit;
}

:deep(.custom-popup .leaflet-popup-tip) {
  background: white;
}
</style>