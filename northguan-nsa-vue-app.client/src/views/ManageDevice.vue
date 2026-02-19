<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- Page Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 space-y-4 sm:space-y-0">
        <h1 class="text-2xl font-bold text-gray-900">設備管理</h1>

        <div class="flex flex-col sm:flex-row items-stretch sm:items-center space-y-2 sm:space-y-0 sm:space-x-4">
          <Button @click="openCreateModal">
            <PlusIcon class="w-4 h-4 mr-2" />
            新增設備
          </Button>

          <div class="flex">
            <Input v-model="searchKeyword" placeholder="搜尋設備..." class="rounded-r-none" @keyup.enter="handleSearch"
              @input="handleSearchInput" />
            <Button @click="handleSearch" variant="outline" class="rounded-l-none border-l-0">
              <SearchIcon class="w-4 h-4" />
            </Button>
          </div>
          <Button v-if="searchKeyword" @click="clearSearch" variant="ghost" size="sm"
            class="text-gray-500 hover:text-gray-700">
            清除
          </Button>
        </div>
      </div>

      <!-- Filters -->
      <div class="flex flex-wrap items-center gap-4 mb-6 p-4 bg-white rounded-lg shadow-sm">
        <div class="flex items-center space-x-2">
          <Label class="text-sm font-medium text-gray-700">設備類型:</Label>
          <Select v-model="typeFilter" @update:model-value="handleFilterChange">
            <SelectTrigger class="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">全部</SelectItem>
              <SelectItem value="crowd">人流辨識</SelectItem>
              <SelectItem value="parking">停車場</SelectItem>
              <SelectItem value="traffic">車流</SelectItem>
              <SelectItem value="fence">電子圍籬</SelectItem>
              <SelectItem value="highResolution">4K影像</SelectItem>
              <SelectItem value="water">水域監控</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div class="flex items-center space-x-2">
          <Label class="text-sm font-medium text-gray-700">所屬分站:</Label>
          <Select v-model="stationFilter" @update:model-value="handleFilterChange">
            <SelectTrigger class="w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">全部分站</SelectItem>
              <SelectItem v-for="station in stations" :key="station.id" :value="station.id?.toString() || ''">
                {{ station.name }}
              </SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div class="flex items-center space-x-2">
          <Label class="text-sm font-medium text-gray-700">狀態篩選:</Label>
          <Select v-model="statusFilter" @update:model-value="handleFilterChange">
            <SelectTrigger class="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">全部</SelectItem>
              <SelectItem value="online">在線</SelectItem>
              <SelectItem value="offline">離線</SelectItem>
              <SelectItem value="maintenance">維護中</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div class="ml-auto text-sm text-gray-600">
          共 {{ totalCount }} 個設備
        </div>
      </div>

      <!-- Device Status Cards -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
        <Card>
          <CardContent class="p-6">
            <div class="flex items-center">
              <div class="p-2 bg-green-100 rounded-lg">
                <CheckCircleIcon class="w-6 h-6 text-green-600" />
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-600">在線設備</p>
                <p class="text-2xl font-bold text-gray-900">{{ deviceStatus.online }}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent class="p-6">
            <div class="flex items-center">
              <div class="p-2 bg-red-100 rounded-lg">
                <XCircleIcon class="w-6 h-6 text-red-600" />
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-600">離線設備</p>
                <p class="text-2xl font-bold text-gray-900">{{ deviceStatus.offline }}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent class="p-6">
            <div class="flex items-center">
              <div class="p-2 bg-blue-100 rounded-lg">
                <ServerIcon class="w-6 h-6 text-blue-600" />
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-600">總設備數</p>
                <p class="text-2xl font-bold text-gray-900">{{ deviceStatus.total }}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>

      <!-- Devices Table -->
      <Card v-else>
        <CardContent class="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>
                  <Button variant="ghost" size="sm" @click="toggleSort('name')" class="p-0 h-auto font-medium">
                    設備名稱
                    <ChevronUpIcon v-if="sortField === 'name' && sortOrder === 'asc'" class="w-4 h-4 ml-1" />
                    <ChevronDownIcon v-else-if="sortField === 'name' && sortOrder === 'desc'" class="w-4 h-4 ml-1" />
                    <ChevronsUpDownIcon v-else class="w-4 h-4 ml-1 opacity-50" />
                  </Button>
                </TableHead>
                <TableHead>
                  <Button variant="ghost" size="sm" @click="toggleSort('type')" class="p-0 h-auto font-medium">
                    類型
                    <ChevronUpIcon v-if="sortField === 'type' && sortOrder === 'asc'" class="w-4 h-4 ml-1" />
                    <ChevronDownIcon v-else-if="sortField === 'type' && sortOrder === 'desc'" class="w-4 h-4 ml-1" />
                    <ChevronsUpDownIcon v-else class="w-4 h-4 ml-1 opacity-50" />
                  </Button>
                </TableHead>
                <TableHead>所屬分站</TableHead>
                <TableHead>狀態</TableHead>
                <TableHead>位置</TableHead>
                <TableHead>
                  <Button variant="ghost" size="sm" @click="toggleSort('updatedAt')" class="p-0 h-auto font-medium">
                    最後更新
                    <ChevronUpIcon v-if="sortField === 'updatedAt' && sortOrder === 'asc'" class="w-4 h-4 ml-1" />
                    <ChevronDownIcon v-else-if="sortField === 'updatedAt' && sortOrder === 'desc'"
                      class="w-4 h-4 ml-1" />
                    <ChevronsUpDownIcon v-else class="w-4 h-4 ml-1 opacity-50" />
                  </Button>
                </TableHead>
                <TableHead class="text-right">操作</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="(device, index) in sortedDevices"
                :key="`device-${device.id}-${device.type}-${device.stationID}-${index}`" class="hover:bg-gray-50">
                <TableCell class="font-medium">{{ device.name }}</TableCell>
                <TableCell>
                  <span
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                    {{ device.type }}
                  </span>
                </TableCell>
                <TableCell>{{ getStationName(device.stationID || 0) }}</TableCell>
                <TableCell>
                  <div class="flex items-center space-x-2">
                    <div :class="`w-3 h-3 rounded-full ${getStatusColor(device.status || 'offline')}`"></div>
                    <span :class="getStatusTextColor(device.status || 'offline')">
                      {{ getStatusText(device.status || 'offline') }}
                    </span>
                  </div>
                </TableCell>
                <TableCell class="text-sm text-gray-600">
                  <span v-if="device.lat && device.lng">
                    {{ device.lat.toFixed(4) }}, {{ device.lng.toFixed(4) }}
                  </span>
                  <span v-else class="text-gray-400">未設定</span>
                </TableCell>
                <TableCell class="text-sm text-gray-600">
                  {{ device.updatedAt ? device.updatedAt : '-' }}
                </TableCell>
                <TableCell class="text-right">
                  <div class="flex justify-end space-x-1">
                    <Button variant="ghost" size="sm" @click="openUpdateModal(device)" title="編輯">
                      <EditIcon class="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="sm" @click="confirmDeleteDevice(device)" title="刪除">
                      <TrashIcon class="w-4 h-4 text-red-500" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
              <TableRow v-if="sortedDevices.length === 0">
                <TableCell colspan="7" class="text-center py-12">
                  <div class="flex flex-col items-center space-y-3">
                    <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
                      <ServerIcon class="w-8 h-8 text-gray-400" />
                    </div>
                    <div class="text-gray-500">
                      {{ hasActiveFilters ? '沒有找到符合條件的設備' : '尚未有任何設備' }}
                    </div>
                    <Button v-if="hasActiveFilters" @click="clearAllFilters" variant="outline" size="sm">
                      清除所有篩選條件
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </main>

    <!-- Device Modals -->
    <CreateDeviceModal :is-open="showCreateModal" :stations="stations" @close="showCreateModal = false"
      @success="loadDevices" />

    <UpdateDeviceModal :is-open="showUpdateModal" :stations="stations" :device="currentDevice"
      @close="showUpdateModal = false" @success="loadDevices" />

    <DeleteDeviceModal :is-open="showDeleteModal" :device="deviceToDelete" @close="showDeleteModal = false"
      @success="loadDevices" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import {
  DeviceService,
  DeviceListResponse,
  StationService,
  StationResponse
} from '@/services'
import { useToast } from '@/composables/useToast'
import { handleApiError, type ApiErrorResponse, dissmissApiErrorToast } from '@/utils/errorHandler'
import AppHeader from '@/components/layout/AppHeader.vue'
import Card from '@/components/ui/card/Card.vue'
import CardContent from '@/components/ui/card/CardContent.vue'
import Table from '@/components/ui/table/Table.vue'
import TableHeader from '@/components/ui/table/TableHeader.vue'
import TableBody from '@/components/ui/table/TableBody.vue'
import TableRow from '@/components/ui/table/TableRow.vue'
import TableHead from '@/components/ui/table/TableHead.vue'
import TableCell from '@/components/ui/table/TableCell.vue'
import Button from '@/components/ui/button/Button.vue'
import Input from '@/components/ui/input/Input.vue'
import CreateDeviceModal from '@/components/device/CreateDeviceModal.vue'
import UpdateDeviceModal from '@/components/device/UpdateDeviceModal.vue'
import DeleteDeviceModal from '@/components/device/DeleteDeviceModal.vue'
import Label from '@/components/ui/label/Label.vue'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Search as SearchIcon,
  Edit as EditIcon,
  Trash as TrashIcon,
  Plus as PlusIcon,
  CheckCircle as CheckCircleIcon,
  XCircle as XCircleIcon,
  Server as ServerIcon,
  ChevronUp as ChevronUpIcon,
  ChevronDown as ChevronDownIcon,
  ChevronsUpDown as ChevronsUpDownIcon
} from 'lucide-vue-next'

// Composables
const toast = useToast()

// State
const devices = ref<DeviceListResponse[]>([])
const stations = ref<StationResponse[]>([])
const deviceStatus = ref({ online: 0, offline: 0, total: 0 })
const searchKeyword = ref('')
const loading = ref(false)

const showCreateModal = ref(false)
const showUpdateModal = ref(false)
const showDeleteModal = ref(false)
const deviceToDelete = ref<DeviceListResponse | null>(null)
const currentDevice = ref<DeviceListResponse | null>(null)

// Sorting
const sortField = ref<string>('')
const sortOrder = ref<'asc' | 'desc'>('asc')

// Filters
const typeFilter = ref('all')
const stationFilter = ref('all')
const statusFilter = ref('all')

// Computed
const stationMap = computed(() => {
  return stations.value.reduce((map, station) => {
    map[station.id || 0] = station
    return map
  }, {} as Record<number, StationResponse>)
})

const filteredDevices = computed(() => {
  let result = devices.value

  // Search filter
  if (searchKeyword.value) {
    result = result.filter(device =>
      device.name.toLowerCase().includes(searchKeyword.value.toLowerCase()) ||
      device.type.toLowerCase().includes(searchKeyword.value.toLowerCase())
    )
  }

  // Type filter
  if (typeFilter.value !== 'all') {
    result = result.filter(device => device.type === typeFilter.value)
  }

  // Station filter
  if (stationFilter.value !== 'all') {
    result = result.filter(device => device.stationID?.toString() === stationFilter.value)
  }

  // Status filter (假設有狀態欄位，這裡先用固定值)
  if (statusFilter.value !== 'all') {
    // 這裡需要根據實際的狀態欄位來篩選
    // result = result.filter(device => device.status === statusFilter.value)
  }

  return result
})

const sortedDevices = computed(() => {
  if (!sortField.value) return filteredDevices.value

  return [...filteredDevices.value].sort((a, b) => {
    let aValue: any = a[sortField.value as keyof DeviceListResponse]
    let bValue: any = b[sortField.value as keyof DeviceListResponse]

    // Handle null/undefined values
    if (aValue == null && bValue == null) return 0
    if (aValue == null) return 1
    if (bValue == null) return -1

    // Convert to string for comparison
    aValue = String(aValue).toLowerCase()
    bValue = String(bValue).toLowerCase()

    const result = aValue.localeCompare(bValue)
    return sortOrder.value === 'asc' ? result : -result
  })
})

const totalCount = computed(() => filteredDevices.value.length)

const hasActiveFilters = computed(() => {
  return searchKeyword.value ||
    typeFilter.value !== 'all' ||
    stationFilter.value !== 'all' ||
    statusFilter.value !== 'all'
})

// Methods
const loadDevices = async () => {
  try {
    loading.value = true
    // 只載入所有設備，不使用搜尋參數，讓前端處理篩選
    devices.value = await DeviceService.getDevices()

    // 載入設備狀態統計
    const statusList = await DeviceService.getDevicesStatus()
    const online = statusList.filter(s => s.status === 'online').length
    const offline = statusList.filter(s => s.status === 'offline').length
    deviceStatus.value = { online, offline, total: statusList.length }
  } catch (error) {
    dissmissApiErrorToast()
    console.error('載入設備失敗:', error)
    const apiError: ApiErrorResponse = handleApiError(error)
    // toast.error('載入失敗', `無法載入設備列表: ${apiError.message}`)
  } finally {
    loading.value = false
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
  // 觸發計算屬性重新計算
}

const handleSearchInput = () => {
  // 即時搜尋
}

const clearSearch = () => {
  searchKeyword.value = ''
}

const handleFilterChange = () => {
  // 觸發計算屬性重新計算
}

const clearAllFilters = () => {
  searchKeyword.value = ''
  typeFilter.value = 'all'
  stationFilter.value = 'all'
  statusFilter.value = 'all'
}

const getStationName = (stationId: number) => {
  return stationMap.value[stationId]?.name || '未知分站'
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'online':
      return 'bg-green-500'
    case 'offline':
      return 'bg-red-500'
    case 'maintenance':
      return 'bg-yellow-500'
    default:
      return 'bg-gray-500'
  }
}

const getStatusText = (status: string) => {
  switch (status) {
    case 'online':
      return '在線'
    case 'offline':
      return '離線'
    case 'maintenance':
      return '維護中'
    default:
      return '未知'
  }
}

const getStatusTextColor = (status: string) => {
  switch (status) {
    case 'online':
      return 'text-green-600 font-medium'
    case 'offline':
      return 'text-red-600 font-medium'
    case 'maintenance':
      return 'text-yellow-600 font-medium'
    default:
      return 'text-gray-600'
  }
}

const openCreateModal = () => {
  showCreateModal.value = true
}

const toggleSort = (field: string) => {
  if (sortField.value === field) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortField.value = field
    sortOrder.value = 'asc'
  }
}

const openUpdateModal = (device: DeviceListResponse) => {
  currentDevice.value = device
  showUpdateModal.value = true
}

const confirmDeleteDevice = (device: DeviceListResponse) => {
  deviceToDelete.value = device
  showDeleteModal.value = true
}


onMounted(() => {
  loadDevices()
  loadStations()
})
</script>
