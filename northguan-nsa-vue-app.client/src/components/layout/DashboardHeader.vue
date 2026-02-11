<template>
  <header class="flex-shrink-0">
    <div class="bg-gradient-to-r from-sky-400 to-indigo-600 pt-4 pb-8">
      <div class="container mx-auto px-4 lg:px-6">
        <div class="flex justify-between items-center">
          <h1 class="text-2xl lg:text-3xl font-bold text-white tracking-wider">
            北觀智慧管理平台
          </h1>

          <!-- Gear Icon with Dropdown -->
          <DropdownMenu>
            <DropdownMenuTrigger as-child>
              <button
                class="flex items-center justify-center w-10 h-10 bg-white/20 hover:bg-white/30 rounded-full transition-colors duration-200">
                <SettingsIcon class="w-5 h-5 text-white" />
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" class="w-48">
              <DropdownMenuItem @click="navigateToManageDevice">
                <SettingsIcon class="w-4 h-4 mr-2" />
                設備管理
              </DropdownMenuItem>
              <DropdownMenuItem @click="navigateToManageStation">
                <MapPinIcon class="w-4 h-4 mr-2" />
                分站管理
              </DropdownMenuItem>
              <DropdownMenuItem @click="navigateToProfile">
                <UserIcon class="w-4 h-4 mr-2" />
                個人設定
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </div>
    <nav class="container mx-auto px-4 lg:px-6 -mt-6 mb-4">
      <div class="bg-white/60 backdrop-blur-sm rounded-lg shadow-md p-2">
        <ul class="flex flex-wrap items-center justify-center sm:justify-start gap-2">
          <li>
            <button @click="navigateToDashboard" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isIntelligentMonitoringActive
                ? 'bg-blue-500 text-white'
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <BarChart3Icon class="h-5 w-5" />
              <span>戰情室總覽</span>
            </button>
          </li>
          <li>
            <button @click="navigateToDashboardMap('traffic')" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isDashboardMapActive && currentView === 'traffic'
                ? `${getButtonColorClass('traffic')} text-white`
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <CarIcon class="h-5 w-5" />
              <span>車流辨識</span>
            </button>
          </li>
          <li>
            <button @click="navigateToDashboardMap('crowd')" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isDashboardMapActive && currentView === 'crowd'
                ? `${getButtonColorClass('crowd')} text-white`
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <UsersIcon class="h-5 w-5" />
              <span>人流辨識</span>
            </button>
          </li>
          <li>
            <button @click="navigateToDashboardMap('highResolution')" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isDashboardMapActive && currentView === 'highResolution'
                ? `${getButtonColorClass('highResolution')} text-white`
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <VideoIcon class="h-5 w-5" />
              <span>即時影像</span>
            </button>
          </li>
          <li>
            <button @click="navigateToDashboardMap('parking')" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isDashboardMapActive && currentView === 'parking'
                ? `${getButtonColorClass('parking')} text-white`
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <ParkingCircleIcon class="h-5 w-5" />
              <span>停車場車位辨識</span>
            </button>
          </li>
          <li>
            <button @click="navigateToDashboardMap('fence')" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isDashboardMapActive && currentView === 'fence'
                ? `${getButtonColorClass('fence')} text-white`
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <ShieldIcon class="h-5 w-5" />
              <span>電子圍籬</span>
            </button>
          </li>
          <li>
            <button @click="navigateToSignage('water')" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isSignageActive && currentSignageType === 'water'
                ? 'bg-blue-500 text-white'
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <WavesIcon class="h-5 w-5" />
              <span>水域監控</span>
            </button>
          </li>
          <li>
            <button @click="navigateToSignage('bridge')" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isSignageActive && currentSignageType === 'bridge'
                ? 'bg-blue-500 text-white'
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <ConstructionIcon class="h-5 w-5" />
              <span>橋梁監控</span>
            </button>
          </li>
          <li>
            <button @click="navigateToSignage('digital')" :class="[
              'flex items-center space-x-2 px-4 py-2 rounded-md transition-colors duration-200 text-sm font-medium shadow',
              isSignageActive && currentSignageType === 'digital'
                ? 'bg-blue-500 text-white'
                : 'bg-white/80 text-slate-600 hover:bg-gray-200 shadow-sm'
            ]">
              <TvIcon class="h-5 w-5" />
              <span>電子看板</span>
            </button>
          </li>
        </ul>
      </div>
    </nav>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  MapPin as MapPinIcon,
  Settings as SettingsIcon,
  User as UserIcon,
  BarChart3 as BarChart3Icon,
  Car as CarIcon,
  Users as UsersIcon,
  Video as VideoIcon,
  ParkingCircle as ParkingCircleIcon,
  Shield as ShieldIcon,
  Monitor as MonitorIcon,
  Waves as WavesIcon,
  Construction as ConstructionIcon,
  Tv as TvIcon
} from 'lucide-vue-next'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { getDeviceColors, type DeviceType } from '@/config/deviceColors'

interface Props {
  currentView?: string
  currentSignageType?: string
}

const emit = defineEmits<{
  signageTabChanged: [type: string]
}>()

const props = defineProps<Props>()

const router = useRouter()
const route = useRoute()

// 判斷當前頁面
const isIntelligentMonitoringActive = computed(() => {
  return route.name === 'Dashboard'
})

const isDashboardMapActive = computed(() => {
  return route.name === 'DashboardMap'
})

const isSignageActive = computed(() => {
  return route.name === 'Signage' || route.name === 'SignageWater' || route.name === 'SignageBridge' || route.name === 'SignageDigital'
})

const currentSignageType = computed(() => {
  // 優先使用 props 傳入的值，這樣可以避免頁面重新加載
  if (props.currentSignageType) {
    return props.currentSignageType
  }
  // 備用方案：從路由判斷
  if (route.name === 'SignageWater') return 'water'
  if (route.name === 'SignageBridge') return 'bridge'
  if (route.name === 'SignageDigital') return 'digital'
  return 'water' // default
})

const currentView = computed(() => {
  return props.currentView || route.query.type as string
})

// 獲取按鈕的動態顏色
const getButtonColorClass = (deviceType: DeviceType) => {
  const colors = getDeviceColors(deviceType)
  return colors.bg.replace('bg-', 'bg-') // 確保格式正確
}

// 導航函數
const navigateToDashboard = () => {
  router.push({ name: 'Dashboard' })
}

// 電子看板
const navigateToSignage = (type: string) => {
  // 如果已經在 Signage 頁面，發送事件而不是路由跳轉
  if (isSignageActive.value) {
    emit('signageTabChanged', type)
    return
  }

  // 如果不在 Signage 頁面，進行路由跳轉
  switch (type) {
    case 'water':
      router.push({ name: 'SignageWater' })
      break
    case 'bridge':
      router.push({ name: 'SignageBridge' })
      break
    case 'digital':
      router.push({ name: 'SignageDigital' })
      break
    default:
      router.push({ name: 'SignageWater' })
  }
}

const navigateToDashboardMap = (type: string) => {
  router.push({
    name: 'DashboardMap',
    query: { type }
  })
}

const navigateToManageDevice = () => {
  router.push({ name: 'ManageDevice' })
}

const navigateToManageStation = () => {
  router.push({ name: 'ManageStation' })
}

const navigateToProfile = () => {
  router.push({ name: 'Profile' })
}
</script>