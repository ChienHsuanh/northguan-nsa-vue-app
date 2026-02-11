<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- Page Header with Enhanced Design -->
      <div class="mb-8">
        <div class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 mb-2">設備管理中心</h1>
            <p class="text-gray-600">統一管理所有監控設備，支援分站聯動操作</p>
          </div>

          <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-4">
            <Button @click="openCreateModal" class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2.5">
              <PlusIcon class="w-5 h-5 mr-2" />
              新增設備
            </Button>

            <Button @click="refreshData" variant="outline" class="px-6 py-2.5" :disabled="loading">
              <div v-if="loading" class="animate-spin rounded-full h-4 w-4 border-b-2 border-gray-600 mr-2"></div>
              <component v-else :is="RefreshIcon" class="w-4 h-4 mr-2" />
              {{ loading ? '載入中...' : '重新整理' }}
            </Button>

            <div class="flex">
              <Input v-model="searchKeyword" placeholder="搜尋設備名稱或序號..." class="rounded-r-none w-64"
                @keyup.enter="handleSearch" @input="handleSearchInput" />
              <Button @click="handleSearch" variant="outline" class="rounded-l-none border-l-0 px-4">
                <SearchIcon class="w-4 h-4" />
              </Button>
            </div>
          </div>
        </div>
      </div>

      <!-- Enhanced Status Overview Cards -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <Card class="bg-green-600 text-white border-0 shadow-lg hover:shadow-xl transition-all duration-300">
          <CardContent class="p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-green-100 text-sm font-medium">在線設備</p>
                <p class="text-3xl font-bold">{{ deviceStatus.online }}</p>
                <p class="text-green-100 text-xs mt-1">{{ ((deviceStatus.online / deviceStatus.total) * 100).toFixed(1)
                }}%</p>
              </div>
              <div class="p-3 bg-white/20 rounded-full">
                <CheckCircleIcon class="w-8 h-8" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card class="bg-red-600 text-white border-0 shadow-lg hover:shadow-xl transition-all duration-300">
          <CardContent class="p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-red-100 text-sm font-medium">離線設備</p>
                <p class="text-3xl font-bold">{{ deviceStatus.offline }}</p>
                <p class="text-red-100 text-xs mt-1">{{ ((deviceStatus.offline / deviceStatus.total) * 100).toFixed(1)
                }}%</p>
              </div>
              <div class="p-3 bg-white/20 rounded-full">
                <XCircleIcon class="w-8 h-8" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card class="bg-blue-600 text-white border-0 shadow-lg hover:shadow-xl transition-all duration-300">
          <CardContent class="p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-blue-100 text-sm font-medium">總設備數</p>
                <p class="text-3xl font-bold">{{ deviceStatus.total }}</p>
                <p class="text-blue-100 text-xs mt-1">跨 {{ stations.length }} 個分站</p>
              </div>
              <div class="p-3 bg-white/20 rounded-full">
                <ServerIcon class="w-8 h-8" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <!-- Enhanced Filters -->
      <Card class="mb-8 shadow-lg border-0">
        <CardContent class="p-6">
          <div class="flex flex-wrap items-center gap-6">
            <div class="flex items-center space-x-3">
              <Label class="text-sm font-semibold text-gray-700">分站篩選:</Label>
              <Select v-model="stationFilter" @update:model-value="handleFilterChange">
                <SelectTrigger class="w-48 border-gray-300">
                  <SelectValue placeholder="選擇分站" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">全部分站</SelectItem>
                  <SelectItem v-for="station in stations" :key="station.id" :value="station.id?.toString() || ''">
                    {{ station.name }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div class="flex items-center space-x-3">
              <Label class="text-sm font-semibold text-gray-700">狀態篩選:</Label>
              <Select v-model="statusFilter" @update:model-value="handleFilterChange">
                <SelectTrigger class="w-36 border-gray-300">
                  <SelectValue placeholder="選擇狀態" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">全部狀態</SelectItem>
                  <SelectItem value="online">在線</SelectItem>
                  <SelectItem value="offline">離線</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div class="flex items-center space-x-3">
              <Label class="text-sm font-semibold text-gray-700">排序方式:</Label>
              <Select v-model="sortBy" @update:model-value="handleSortChange">
                <SelectTrigger class="w-40 border-gray-300">
                  <SelectValue placeholder="選擇排序" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="name">設備名稱</SelectItem>
                  <SelectItem value="station">分站名稱</SelectItem>
                  <SelectItem value="status">設備狀態</SelectItem>
                  <!-- <SelectItem value="type">設備類型</SelectItem> -->
                </SelectContent>
              </Select>
            </div>

            <div class="ml-auto flex items-center space-x-4">
              <Button v-if="hasActiveFilters" @click="clearAllFilters" variant="outline" size="sm"
                class="text-gray-600 hover:text-gray-800">
                <XIcon class="w-4 h-4 mr-2" />
                清除篩選
              </Button>
              <span class="text-sm text-gray-600 font-medium">
                顯示 {{filteredDevicesByType.reduce((sum, group) => sum + group.devices.length, 0)}} / {{
                  deviceStatus.total }} 個設備
              </span>
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-16">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p class="text-gray-600">載入設備資料中...</p>
        </div>
      </div>

      <!-- Device Type Groups -->
      <div v-else class="space-y-8">
        <!-- 展開/收合全部控制區域 -->
        <div v-if="filteredDevicesByType.length > 0" class="flex justify-between items-center mb-6">
          <div class="flex items-center space-x-2">
            <h3 class="text-lg font-semibold text-gray-900">設備分類</h3>
            <span class="text-sm text-gray-500">{{ filteredDevicesByType.length }} 個類別</span>
          </div>
          <div class="flex items-center space-x-2">
            <Button @click="expandAllGroups" variant="outline" size="sm"
              class="text-gray-600 hover:text-gray-800 border-gray-300">
              <ChevronDownIcon class="w-4 h-4 mr-1" />
              展開全部
            </Button>
            <Button @click="collapseAllGroups" variant="outline" size="sm"
              class="text-gray-600 hover:text-gray-800 border-gray-300">
              <ChevronUpIcon class="w-4 h-4 mr-1" />
              收合全部
            </Button>
          </div>
        </div>

        <div v-for="deviceGroup in filteredDevicesByType" :key="deviceGroup.type" class="transition-all duration-300">
          <!-- Type Header - 可點擊展開/收合 -->
          <div @click="toggleGroupExpanded(deviceGroup.type)"
            class="flex items-center justify-between mb-4 p-4 bg-white rounded-lg shadow-sm border cursor-pointer hover:shadow-md transition-all duration-200">
            <div class="flex items-center space-x-4">
              <div class="flex items-center space-x-3">
                <div :class="`p-2 rounded-lg ${getTypeIconBg(deviceGroup.type)}`">
                  <component :is="getTypeIcon(deviceGroup.type)"
                    :class="`w-5 h-5 ${getTypeIconColor(deviceGroup.type)}`" />
                </div>
                <div>
                  <h2 class="text-lg font-bold text-gray-900">{{ getTypeDisplayName(deviceGroup.type) }}</h2>
                  <p class="text-sm text-gray-600">{{ deviceGroup.devices.length }} 個設備</p>
                </div>
              </div>
            </div>

            <div class="flex items-center space-x-4">
              <!-- 狀態統計 -->
              <div class="flex items-center space-x-2 text-xs">
                <span class="flex items-center space-x-1">
                  <div class="w-2 h-2 bg-green-500 rounded-full"></div>
                  <span class="text-gray-600">{{ getTypeStatusCount(deviceGroup.type, 'online') }}</span>
                </span>
                <span class="flex items-center space-x-1">
                  <div class="w-2 h-2 bg-red-500 rounded-full"></div>
                  <span class="text-gray-600">{{ getTypeStatusCount(deviceGroup.type, 'offline') }}</span>
                </span>
              </div>

              <!-- 快速新增按鈕 -->
              <Button @click.stop="openCreateModalWithType(deviceGroup.type)" variant="outline" size="sm"
                class="bg-blue-50 hover:bg-blue-100 border-blue-200 text-blue-600 hover:text-blue-700 px-3 py-1.5 text-xs">
                <PlusIcon class="w-3 h-3 mr-1" />
                新增{{ getTypeDisplayName(deviceGroup.type) }}
              </Button>

              <component :is="expandedGroups.includes(deviceGroup.type) ? ChevronUpIcon : ChevronDownIcon"
                class="w-5 h-5 text-gray-400" />
            </div>
          </div>

          <!-- Device Cards Grid - 完整資訊顯示 -->
          <div v-show="expandedGroups.includes(deviceGroup.type)"
            class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            <Card v-for="device in deviceGroup.devices" :key="`${device.type}-${device.id}`"
              class="transition-all duration-300 border-0 shadow-sm hover:shadow-md hover:-translate-y-1 h-full flex flex-col">
              <CardContent class="p-4 flex-1 flex flex-col">
                <!-- 完整的設備卡片 -->
                <div class="space-y-3 flex-1 flex flex-col">
                  <!-- 設備名稱和操作選單 -->
                  <div class="flex items-start justify-between">
                    <div class="flex-1 min-w-0">
                      <h3 class="font-semibold text-base text-gray-900 break-words leading-tight">
                        {{ device.name }}
                      </h3>
                      <p class="text-xs text-gray-500 font-mono mt-1">ID: {{ device.id }}</p>
                    </div>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="sm" class="h-8 w-8 p-0 ml-2">
                          <MoreVerticalIcon class="w-4 h-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem @click="openUpdateModal(device)">
                          <EditIcon class="w-4 h-4 mr-2" />
                          編輯
                        </DropdownMenuItem>
                        <DropdownMenuItem @click="viewDeviceLocation(device)">
                          <MapPinIcon class="w-4 h-4 mr-2" />
                          位置
                        </DropdownMenuItem>
                        <DropdownMenuItem @click="viewDeviceDetails(device)">
                          <EyeIcon class="w-4 h-4 mr-2" />
                          詳情
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem @click="confirmDeleteDevice(device)" class="text-red-600">
                          <TrashIcon class="w-4 h-4 mr-2" />
                          刪除
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </div>

                  <!-- 狀態指示器 -->
                  <div class="flex items-center space-x-2">
                    <div :class="`w-3 h-3 rounded-full ${getStatusColor(getDeviceStatus(device))}`"></div>
                    <span :class="`text-sm font-medium ${getStatusTextColor(getDeviceStatus(device))}`">
                      {{ getStatusText(getDeviceStatus(device)) }}
                    </span>
                    <span class="text-xs text-gray-400">•</span>
                    <span class="text-xs text-gray-500">{{ getLastUpdateTime(device) }}</span>
                  </div>

                  <!-- 基本資訊 -->
                  <div class="space-y-2 text-sm">
                    <div class="flex items-center text-gray-600">
                      <BuildingIcon class="w-4 h-4 mr-2 flex-shrink-0" />
                      <span class="truncate">{{ getStationName(device.stationID || 0) }}</span>
                    </div>

                    <div class="flex items-center text-gray-600">
                      <component :is="getTypeIcon(device.type || '')" class="w-4 h-4 mr-2 flex-shrink-0" />
                      <span>{{ getTypeDisplayName(device.type || '') }}</span>
                    </div>

                    <div class="flex items-center text-gray-600">
                      <MapPinIcon class="w-4 h-4 mr-2 flex-shrink-0" />
                      <span class="text-xs font-mono">{{ device.lat?.toFixed(6) }}, {{ device.lng?.toFixed(6) }}</span>
                    </div>

                    <div class="flex items-center text-gray-600">
                      <ServerIcon class="w-4 h-4 mr-2 flex-shrink-0" />
                      <span class="text-xs font-mono truncate">{{ device.serial }}</span>
                    </div>
                  </div>

                  <!-- 類型特定資訊 -->
                  <div v-if="getDetailedTypeInfo(device)" class="bg-gray-50 rounded-lg p-3">
                    <h4 class="text-xs font-semibold text-gray-700 mb-2">設備詳情</h4>
                    <div class="space-y-1 text-xs text-gray-600" v-html="getDetailedTypeInfo(device)"></div>
                  </div>

                  <!-- 操作按鈕 -->
                  <div class="flex space-x-2 pt-2 border-t border-gray-100 mt-auto">
                    <Button @click="openUpdateModal(device)" variant="outline" size="sm" class="flex-1 h-8 text-xs">
                      <EditIcon class="w-3 h-3 mr-1" />
                      編輯
                    </Button>
                    <Button @click="viewDeviceLocation(device)" variant="outline" size="sm" class="flex-1 h-8 text-xs">
                      <MapPinIcon class="w-3 h-3 mr-1" />
                      位置
                    </Button>
                    <Button @click="viewDeviceDetails(device)" variant="outline" size="sm" class="flex-1 h-8 text-xs">
                      <EyeIcon class="w-3 h-3 mr-1" />
                      詳情
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <!-- Empty State for Type -->
          <div v-if="expandedGroups.includes(deviceGroup.type) && deviceGroup.devices.length === 0"
            class="text-center py-12 bg-white rounded-lg border-2 border-dashed border-gray-200">
            <component :is="getTypeIcon(deviceGroup.type)" class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-500 mb-4">此類型暫無設備</p>
            <Button @click="openCreateModalWithType(deviceGroup.type)" variant="outline" size="sm">
              <PlusIcon class="w-4 h-4 mr-2" />
              新增{{ getTypeDisplayName(deviceGroup.type) }}設備
            </Button>
          </div>
        </div>

        <!-- Global Empty State -->
        <div v-if="filteredDevicesByType.length === 0" class="text-center py-16">
          <div class="w-24 h-24 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <ServerIcon class="w-12 h-12 text-gray-400" />
          </div>
          <h3 class="text-lg font-medium text-gray-900 mb-2">
            {{ hasActiveFilters ? '沒有找到符合條件的設備' : '尚未有任何設備' }}
          </h3>
          <p class="text-gray-600 mb-6">
            {{ hasActiveFilters ? '請調整篩選條件或清除所有篩選' : '開始新增您的第一個監控設備' }}
          </p>
          <div class="flex justify-center space-x-4">
            <Button v-if="hasActiveFilters" @click="clearAllFilters" variant="outline">
              清除所有篩選條件
            </Button>
            <Button @click="openCreateModal" class="bg-blue-600 hover:bg-blue-700 text-white">
              <PlusIcon class="w-4 h-4 mr-2" />
              新增設備
            </Button>
          </div>
        </div>
      </div>
    </main>

    <!-- Device Modals -->
    <CreateDeviceModal :is-open="showCreateModal" :stations="stations" :default-type="defaultCreateType"
      @close="closeCreateModal" @success="handleDeviceCreated" />

    <UpdateDeviceModal :is-open="showUpdateModal" :stations="stations" :device="currentDevice" @close="closeUpdateModal"
      @success="handleDeviceUpdated" />

    <DeleteDeviceModal :is-open="showDeleteModal" :device="deviceToDelete" @close="closeDeleteModal"
      @success="handleDeviceDeleted" />

    <!-- Device Details Modal -->
    <DeviceDetailsModal :is-open="showDetailsModal" :device="detailsDevice"
      :station="detailsDevice ? getStationById(detailsDevice.stationID || 0) : null" @close="closeDetailsModal"
      @edit="handleEditFromDetails" @viewLocation="handleViewLocationFromDetails" />

    <!-- Device Location Modal -->
    <DeviceLocationModal :is-open="showLocationModal" :device="locationDevice" @close="closeLocationModal"
      @edit="handleEditFromLocation" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue'
import {
  DeviceService,
  DeviceListResponse,
  DeviceStatusResponse,
  StationService,
  StationResponse
} from '@/services'
import { useToast } from '@/composables/useToast'
import { handleApiError, type ApiErrorResponse, dissmissApiErrorToast } from '@/utils/errorHandler'

// Components
import AppHeader from '@/components/layout/AppHeader.vue'
import Card from '@/components/ui/card/Card.vue'
import CardContent from '@/components/ui/card/CardContent.vue'
import Button from '@/components/ui/button/Button.vue'
import Input from '@/components/ui/input/Input.vue'
import Label from '@/components/ui/label/Label.vue'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'

// Modals
import CreateDeviceModal from '@/components/device/CreateDeviceModal.vue'
import UpdateDeviceModal from '@/components/device/UpdateDeviceModal.vue'
import DeleteDeviceModal from '@/components/device/DeleteDeviceModal.vue'
import DeviceDetailsModal from '@/components/device/DeviceDetailsModal.vue'
import DeviceLocationModal from '@/components/device/DeviceLocationModal.vue'

// Icons
import {
  Search as SearchIcon,
  Edit as EditIcon,
  Trash as TrashIcon,
  Plus as PlusIcon,
  CheckCircle as CheckCircleIcon,
  XCircle as XCircleIcon,
  Server as ServerIcon,
  Settings as SettingsIcon,
  ChevronUp as ChevronUpIcon,
  ChevronDown as ChevronDownIcon,
  MoreVertical as MoreVerticalIcon,
  MapPin as MapPinIcon,
  Eye as EyeIcon,
  Building as BuildingIcon,
  Clock as ClockIcon,
  X as XIcon,
  Users as UsersIcon,
  Car as CarIcon,
  Camera as CameraIcon,
  Shield as ShieldIcon,
  Monitor as MonitorIcon,
  RefreshCw as RefreshIcon
} from 'lucide-vue-next'

// Composables
const toast = useToast()

// State
const devices = ref<DeviceListResponse[]>([])
const deviceStatusList = ref<DeviceStatusResponse[]>([])
const stations = ref<StationResponse[]>([])
const searchKeyword = ref('')
const loading = ref(false)
const isInitialLoad = ref(true)

// Modals
const showCreateModal = ref(false)
const showUpdateModal = ref(false)
const showDeleteModal = ref(false)
const showDetailsModal = ref(false)
const showLocationModal = ref(false)
const deviceToDelete = ref<DeviceListResponse | null>(null)
const currentDevice = ref<DeviceListResponse | null>(null)
const detailsDevice = ref<DeviceListResponse | null>(null)
const locationDevice = ref<DeviceListResponse | null>(null)
const defaultCreateType = ref<string>('')

// Filters and Sorting
const stationFilter = ref('all')
const statusFilter = ref('all')
const sortBy = ref('name')
const expandedGroups = ref<string[]>([])

// Device Types Configuration
const deviceTypes = [
  {
    type: 'crowd',
    name: '人流辨識',
    icon: UsersIcon,
    iconBg: 'bg-blue-100',
    iconColor: 'text-blue-600'
  },
  {
    type: 'parking',
    name: '停車場監控',
    icon: CarIcon,
    iconBg: 'bg-green-100',
    iconColor: 'text-green-600'
  },
  {
    type: 'traffic',
    name: '車流監控',
    icon: CarIcon,
    iconBg: 'bg-yellow-100',
    iconColor: 'text-yellow-600'
  },
  {
    type: 'fence',
    name: '圍籬監控',
    icon: ShieldIcon,
    iconBg: 'bg-red-100',
    iconColor: 'text-red-600'
  },
  {
    type: 'highResolution',
    name: '4K高解析度',
    icon: MonitorIcon,
    iconBg: 'bg-purple-100',
    iconColor: 'text-purple-600'
  }
]

// Computed Properties
const stationMap = computed(() => {
  return stations.value.reduce((map, station) => {
    map[station.id || 0] = station
    return map
  }, {} as Record<number, StationResponse>)
})

const deviceStatusMap = computed(() => {
  return deviceStatusList.value.reduce((map, status) => {
    map[status.id || 0] = status
    return map
  }, {} as Record<number, DeviceStatusResponse>)
})

const deviceStatus = computed(() => {
  const statusCounts = deviceStatusList.value.reduce((counts, device) => {
    const status = device.status || 'offline'
    counts[status] = (counts[status] || 0) + 1
    return counts
  }, {} as Record<string, number>)

  return {
    online: statusCounts.online || 0,
    offline: statusCounts.offline || 0,
    total: deviceStatusList.value.length
  }
})

const filteredDevices = computed(() => {
  let result = devices.value

  // Search filter
  if (searchKeyword.value) {
    const keyword = searchKeyword.value.toLowerCase()
    result = result.filter(device =>
      device.name?.toLowerCase().includes(keyword) ||
      device.serial?.toLowerCase().includes(keyword) ||
      device.type?.toLowerCase().includes(keyword)
    )
  }

  // Station filter
  if (stationFilter.value !== 'all') {
    result = result.filter(device => device.stationID?.toString() === stationFilter.value)
  }

  // Status filter
  if (statusFilter.value !== 'all') {
    result = result.filter(device => {
      const deviceStatus = getDeviceStatus(device)
      return deviceStatus === statusFilter.value
    })
  }

  return result
})

const sortedDevices = computed(() => {
  return [...filteredDevices.value].sort((a, b) => {
    let aValue: any, bValue: any

    switch (sortBy.value) {
      case 'name':
        aValue = a.name || ''
        bValue = b.name || ''
        break
      case 'station':
        aValue = getStationName(a.stationID || 0)
        bValue = getStationName(b.stationID || 0)
        break
      case 'status':
        aValue = getDeviceStatus(a)
        bValue = getDeviceStatus(b)
        break
      case 'type':
        aValue = a.type || ''
        bValue = b.type || ''
        break
      default:
        aValue = a.name || ''
        bValue = b.name || ''
    }

    return String(aValue).localeCompare(String(bValue))
  })
})

const filteredDevicesByType = computed(() => {
  const devicesByType = deviceTypes.map(typeConfig => ({
    type: typeConfig.type,
    name: typeConfig.name,
    devices: sortedDevices.value.filter(device => device.type === typeConfig.type)
  }))

  // Only return types that have devices or show all if no filters
  return devicesByType.filter(group =>
    group.devices.length > 0 || !hasActiveFilters.value
  )
})

const hasActiveFilters = computed(() => {
  return searchKeyword.value ||
    stationFilter.value !== 'all' ||
    statusFilter.value !== 'all'
})

// Methods
const loadDevices = async (showLoading = true) => {
  try {
    if (showLoading) {
      loading.value = true
    }

    // 使用 size = -1 獲取所有設備（不分頁）
    devices.value = await DeviceService.getDevices(undefined, undefined, 1, -1)
  } catch (error) {
    dissmissApiErrorToast()
    console.error('載入設備失敗:', error)
    const apiError: ApiErrorResponse = handleApiError(error)
    toast.error('載入失敗', `無法載入設備列表: ${apiError.message}`)
  } finally {
    if (showLoading) {
      loading.value = false
    }
  }
}

const loadDeviceStatus = async (showLoading = true) => {
  try {
    // 使用 size = -1 獲取所有設備狀態（不分頁）
    deviceStatusList.value = await DeviceService.getDevicesStatus(1, -1)
  } catch (error) {
    console.error('載入設備狀態失敗:', error)
    const apiError: ApiErrorResponse = handleApiError(error)
    toast.error('載入失敗', `無法載入設備狀態: ${apiError.message}`)
  }
}

const loadStations = async () => {
  try {
    stations.value = await StationService.getStations()
  } catch (error) {
    console.error('載入分站失敗:', error)
    const apiError: ApiErrorResponse = handleApiError(error)
    toast.error('載入失敗', `無法載入分站列表: ${apiError.message}`)
  }
}

const handleSearch = () => {
  // Trigger computed property recalculation
  // 搜尋後自動展開全部類別
  nextTick(() => {
    expandAllGroups()
  })
}

const handleSearchInput = () => {
  // Real-time search
  // 搜尋輸入時自動展開全部類別
  nextTick(() => {
    expandAllGroups()
  })
}

const handleFilterChange = () => {
  // Trigger computed property recalculation
  // 篩選後自動展開全部類別
  nextTick(() => {
    expandAllGroups()
  })
}

const handleSortChange = () => {
  // Trigger computed property recalculation
}

const clearAllFilters = () => {
  searchKeyword.value = ''
  stationFilter.value = 'all'
  statusFilter.value = 'all'
}

const toggleGroupExpanded = (type: string) => {
  const index = expandedGroups.value.indexOf(type)
  if (index > -1) {
    expandedGroups.value.splice(index, 1)
  } else {
    expandedGroups.value.push(type)
  }
}

// 展開全部分類
const expandAllGroups = () => {
  const allTypes = filteredDevicesByType.value.map(group => group.type)
  expandedGroups.value = [...allTypes]
}

// 收合全部分類
const collapseAllGroups = () => {
  expandedGroups.value = []
}

// Device Helper Methods
const getDeviceStatus = (device: DeviceListResponse): string => {
  // 優先使用設備本身的狀態，如果沒有則從狀態列表中查找
  if (device.status) {
    return device.status
  }
  const statusInfo = deviceStatusList.value.find(s => s.id === device.id && s.type === device.type)
  return statusInfo?.status || 'offline'
}

const getStationName = (stationId: number): string => {
  return stationMap.value[stationId]?.name || '未知分站'
}

const getStationById = (stationId: number): StationResponse | null => {
  return stationMap.value[stationId] || null
}

const getStatusColor = (status: string): string => {
  switch (status) {
    case 'online':
      return 'bg-green-500'
    case 'offline':
      return 'bg-red-500'
    default:
      return 'bg-gray-500'
  }
}

const getStatusText = (status: string): string => {
  switch (status) {
    case 'online':
      return '在線'
    case 'offline':
      return '離線'
    default:
      return '未知'
  }
}

const getStatusTextColor = (status: string): string => {
  switch (status) {
    case 'online':
      return 'text-green-600'
    case 'offline':
      return 'text-red-600'
    case 'maintenance':
      return 'text-yellow-600'
    default:
      return 'text-gray-600'
  }
}

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

const getTypeSpecificInfo = (device: DeviceListResponse): string => {
  switch (device.type) {
    case 'crowd':
      return device.area ? `監控面積: ${device.area} 平方米` : ''
    case 'parking':
      return device.numberOfParking ? `停車位: ${device.numberOfParking} 個` : ''
    case 'traffic':
      return device.speedLimit ? `速限: ${device.speedLimit} km/h` : ''
    case 'fence':
      return device.observingTimeStart && device.observingTimeEnd
        ? `監控時段: ${device.observingTimeStart} - ${device.observingTimeEnd}`
        : ''
    default:
      return ''
  }
}


const getDetailedTypeInfo = (device: DeviceListResponse): string => {
  const details: string[] = []

  switch (device.type) {
    case 'crowd':
      if (device.area) details.push(`<div>監控面積: <strong>${device.area} 平方米</strong></div>`)
      if (device.videoUrl) details.push(`<div>影片串流: <span class="text-blue-600">${device.videoUrl.length > 30 ? device.videoUrl.substring(0, 30) + '...' : device.videoUrl}</span></div>`)
      if (device.apiUrl) details.push(`<div>API 端點: <span class="text-green-600">${device.apiUrl.length > 30 ? device.apiUrl.substring(0, 30) + '...' : device.apiUrl}</span></div>`)
      break
    case 'parking':
      if (device.numberOfParking) details.push(`<div>總車位數: <strong>${device.numberOfParking} 個</strong></div>`)
      if (device.apiUrl) details.push(`<div>API 端點: <span class="text-green-600">${device.apiUrl.length > 30 ? device.apiUrl.substring(0, 30) + '...' : device.apiUrl}</span></div>`)
      break
    case 'traffic':
      if (device.speedLimit) details.push(`<div>道路速限: <strong>${device.speedLimit} km/h</strong></div>`)
      if (device.city) details.push(`<div>所在縣市: <strong>${device.city}</strong></div>`)
      if (device.eTagNumber) details.push(`<div>eTag 代碼: <span class="font-mono">${device.eTagNumber}</span></div>`)
      break
    case 'fence':
      if (device.observingTimeStart && device.observingTimeEnd) {
        details.push(`<div>監控時段: <strong>${device.observingTimeStart} - ${device.observingTimeEnd}</strong></div>`)
      }
      break
    case 'highResolution':
      if (device.videoUrl) details.push(`<div>4K 串流: <span class="text-blue-600">${device.videoUrl.length > 30 ? device.videoUrl.substring(0, 30) + '...' : device.videoUrl}</span></div>`)
      break
  }

  return details.join('')
}

const getLastUpdateTime = (device: DeviceListResponse): string => {
  // 優先使用設備本身的更新時間
  // if (device.updatedAt) {
  //   return `最後更新: ${device.updatedAt}`
  // }
  if (device.latestOnlineTime) {
    return `最後上線: ${device.latestOnlineTime}`
  }
  // 如果設備本身沒有時間信息，再從狀態列表中查找
  const statusInfo = deviceStatusList.value.find(s => s.id === device.id && s.type === device.type)
  if (statusInfo?.latestOnlineTime) {
    return `最後上線: ${statusInfo.latestOnlineTime}`
  }
  return '無更新記錄'
}

const getTypeStatusCount = (deviceType: string, status: string): number => {
  const typeDevices = sortedDevices.value.filter(device => device.type === deviceType)
  return typeDevices.filter(device => getDeviceStatus(device) === status).length
}

// Modal Methods
const openCreateModal = () => {
  defaultCreateType.value = ''
  showCreateModal.value = true
}

const openCreateModalWithType = (type: string) => {
  defaultCreateType.value = type
  showCreateModal.value = true
}

const closeCreateModal = () => {
  showCreateModal.value = false
  defaultCreateType.value = ''
}

const openUpdateModal = (device: DeviceListResponse) => {
  currentDevice.value = device
  showUpdateModal.value = true
}

const closeUpdateModal = () => {
  showUpdateModal.value = false
  currentDevice.value = null
}

const confirmDeleteDevice = (device: DeviceListResponse) => {
  deviceToDelete.value = device
  showDeleteModal.value = true
}

const closeDeleteModal = () => {
  showDeleteModal.value = false
  deviceToDelete.value = null
}

const viewDeviceDetails = (device: DeviceListResponse) => {
  detailsDevice.value = device
  showDetailsModal.value = true
}

const closeDetailsModal = () => {
  showDetailsModal.value = false
  detailsDevice.value = null
}

const viewDeviceLocation = (device: DeviceListResponse) => {
  locationDevice.value = device
  showLocationModal.value = true
}

const closeLocationModal = () => {
  showLocationModal.value = false
  locationDevice.value = null
}

// Handle events from modals - 優化模態框切換性能
const handleEditFromDetails = (device: DeviceListResponse) => {
  // 先設置新的設備，再切換模態框狀態，避免中間狀態
  currentDevice.value = device
  showDetailsModal.value = false
  detailsDevice.value = null
  nextTick(() => {
    showUpdateModal.value = true
  })
}

const handleViewLocationFromDetails = (device: DeviceListResponse) => {
  // 先設置新的設備，再切換模態框狀態
  locationDevice.value = device
  showDetailsModal.value = false
  detailsDevice.value = null
  nextTick(() => {
    showLocationModal.value = true
  })
}

const handleEditFromLocation = (device: DeviceListResponse) => {
  // 先設置新的設備，再切換模態框狀態
  currentDevice.value = device
  showLocationModal.value = false
  locationDevice.value = null
  nextTick(() => {
    showUpdateModal.value = true
  })
}

// Refresh Data
const refreshData = async () => {
  // 手動重新整理始終顯示加載畫面
  await Promise.all([
    loadDevices(true),
    loadDeviceStatus(true),
    loadStations()
  ])
  isInitialLoad.value = false
  toast.success('重新整理完成', '設備資料已更新')
}

// Event Handlers - 即時更新設備列表
const handleDeviceCreated = async () => {
  // 新增後即時更新設備列表，不顯示加載畫面
  await Promise.all([
    loadDevices(false),
    loadDeviceStatus(false)
  ])
}

const handleDeviceUpdated = async () => {
  // 編輯後即時更新設備列表，不顯示加載畫面
  await Promise.all([
    loadDevices(false),
    loadDeviceStatus(false)
  ])
}

const handleDeviceDeleted = async () => {
  // 刪除後即時更新設備列表，不顯示加載畫面
  await Promise.all([
    loadDevices(false),
    loadDeviceStatus(false)
  ])
}

// Lifecycle
onMounted(async () => {
  await Promise.all([
    loadDevices(true), // 首次加載顯示加載畫面
    loadDeviceStatus(true),
    loadStations()
  ])
  isInitialLoad.value = false
})
</script>
