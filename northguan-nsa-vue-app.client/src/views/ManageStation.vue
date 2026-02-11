<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- Page Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 space-y-4 sm:space-y-0">
        <h1 class="text-2xl font-bold text-gray-900">分站管理</h1>

        <div class="flex flex-col sm:flex-row items-stretch sm:items-center space-y-2 sm:space-y-0 sm:space-x-4">
          <Button @click="openCreateModal">
            <PlusIcon class="w-4 h-4 mr-2" />
            新增分站
          </Button>

          <Button @click="refreshAllData" variant="outline" :disabled="loading">
            <RefreshIcon class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
            重新整理
          </Button>

          <div class="flex">
            <Input
              v-model="searchKeyword"
              placeholder="搜尋分站..."
              class="rounded-r-none"
              @keyup.enter="handleSearch"
              @input="handleSearchInput"
            />
            <Button @click="handleSearch" variant="outline" class="rounded-l-none border-l-0">
              <SearchIcon class="w-4 h-4" />
            </Button>
          </div>
          <Button
            v-if="searchKeyword"
            @click="clearSearch"
            variant="ghost"
            size="sm"
            class="text-gray-500 hover:text-gray-700"
          >
            清除
          </Button>
        </div>
      </div>

      <!-- Filters -->
      <div class="flex flex-wrap items-center gap-4 mb-6 p-4 bg-white rounded-lg shadow-sm">
        <div class="flex items-center space-x-2">
          <Label class="text-sm font-medium text-gray-700">設備數量:</Label>
          <Select v-model="deviceCountFilter" @update:model-value="handleFilterChange">
            <SelectTrigger class="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">全部</SelectItem>
              <SelectItem value="hasDevices">有設備</SelectItem>
              <SelectItem value="noDevices">無設備</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div class="flex items-center space-x-2">
          <Label class="text-sm font-medium text-gray-700">位置設定:</Label>
          <Select v-model="locationFilter" @update:model-value="handleFilterChange">
            <SelectTrigger class="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">全部</SelectItem>
              <SelectItem value="hasLocation">已設定</SelectItem>
              <SelectItem value="noLocation">未設定</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div class="ml-auto text-sm text-gray-600">
          共 {{ totalCount }} 個分站
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>

      <!-- Stations Table -->
      <Card v-else>
        <CardContent class="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>
                  <Button variant="ghost" size="sm" @click="toggleSort('name')" class="p-0 h-auto font-medium">
                    分站名稱
                    <ChevronUpIcon v-if="sortField === 'name' && sortOrder === 'asc'" class="w-4 h-4 ml-1" />
                    <ChevronDownIcon v-else-if="sortField === 'name' && sortOrder === 'desc'" class="w-4 h-4 ml-1" />
                    <ChevronsUpDownIcon v-else class="w-4 h-4 ml-1 opacity-50" />
                  </Button>
                </TableHead>
                <TableHead>通知設定</TableHead>
                <TableHead>位置座標</TableHead>
                <TableHead>管理人員</TableHead>
                <TableHead>設備數量</TableHead>
                <TableHead class="text-right">操作</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="station in sortedAndFilteredStations" :key="station.id" class="hover:bg-gray-50">
                <TableCell class="font-medium">{{ station.name }}</TableCell>
                <TableCell>
                  <span v-if="station.enableNotify" class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    已啟用
                  </span>
                  <span v-else class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                    已停用
                  </span>
                </TableCell>
                <TableCell class="text-sm text-gray-600">
                  <span v-if="station.lat && station.lng">
                    {{ station.lat.toFixed(4) }}, {{ station.lng.toFixed(4) }}
                  </span>
                  <span v-else class="text-gray-400">未設定</span>
                </TableCell>
                <TableCell>
                  <div class="flex items-center justify-center">
                    <Button
                      @click="viewStationManagers(station)"
                      variant="outline"
                      size="sm"
                      class="text-xs"
                    >
                      <UsersIcon class="w-3 h-3 mr-1" />
                      {{ getStationManagers(station.id || 0).length }} 位管理員
                    </Button>
                  </div>
                </TableCell>
                <TableCell class="text-center">
                  <div class="flex flex-col items-center space-y-1">
                    <span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      {{ getDeviceCount(station.id || 0) }} 個設備
                    </span>
                    <span v-if="getDeviceCount(station.id || 0) > 0" class="text-xs text-gray-500">
                      {{ getStationDeviceTypeSummary(station.id || 0) }}
                    </span>
                  </div>
                </TableCell>
                <TableCell class="text-right">
                  <div class="flex justify-end space-x-1">
                    <Button variant="ghost" size="sm" @click="openUpdateModal(station)" title="編輯">
                      <EditIcon class="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="sm" @click="viewOnMap(station)" title="在地圖上查看">
                      <MapIcon class="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="sm" @click="confirmDeleteStation(station)" title="刪除">
                      <TrashIcon class="w-4 h-4 text-red-500" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
              <TableRow v-if="sortedAndFilteredStations.length === 0">
                <TableCell colspan="6" class="text-center py-12">
                  <div class="flex flex-col items-center space-y-3">
                    <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
                      <MapIcon class="w-8 h-8 text-gray-400" />
                    </div>
                    <div class="text-gray-500">
                      {{ hasActiveFilters ? '沒有找到符合條件的分站' : '尚未有任何分站' }}
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

    <!-- Station Modals -->
    <CreateStationModal
      :is-open="showCreateModal"
      @close="showCreateModal = false"
      @success="handleStationSuccess"
    />

    <UpdateStationModal
      :is-open="showUpdateModal"
      :station="currentStation"
      @close="showUpdateModal = false"
      @success="handleStationSuccess"
    />

    <DeleteStationModal
      :is-open="showDeleteModal"
      :station="stationToDelete"
      @close="showDeleteModal = false"
      @success="handleStationSuccess"
    />


    <StationMapModal
      :is-open="showMapModal"
      :station="mapStation"
      :managers="mapStation ? getStationManagers(mapStation.id || 0) : []"
      :device-count="mapStation ? getDeviceCount(mapStation.id || 0) : 0"
      :devices="mapStation ? getStationDevices(mapStation.id || 0) : []"
      @close="showMapModal = false"
    />

    <StationManagersModal
      :is-open="showManagersModal"
      :station="managersStation"
      :managers="managersStation ? getStationManagers(managersStation.id || 0) : []"
      @close="showManagersModal = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import {
  StationService,
  StationResponse,
  AccountService,
  UserResponse,
  DeviceService,
  DeviceListResponse
} from '@/services'
import { useToast } from '@/composables/useToast'
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
import CreateStationModal from '@/components/station/CreateStationModal.vue'
import UpdateStationModal from '@/components/station/UpdateStationModal.vue'
import DeleteStationModal from '@/components/station/DeleteStationModal.vue'
import StationMapModal from '@/components/station/StationMapModal.vue'
import StationManagersModal from '@/components/station/StationManagersModal.vue'
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
  Map as MapIcon,
  ChevronUp as ChevronUpIcon,
  ChevronDown as ChevronDownIcon,
  ChevronsUpDown as ChevronsUpDownIcon,
  RefreshCw as RefreshIcon,
  Users as UsersIcon
} from 'lucide-vue-next'

// Composables
const toast = useToast()

// State
const stations = ref<StationResponse[]>([])
const accounts = ref<UserResponse[]>([])
const devices = ref<DeviceListResponse[]>([])
const searchKeyword = ref('')
const loading = ref(false)

const showCreateModal = ref(false)
const showUpdateModal = ref(false)
const showDeleteModal = ref(false)
const showMapModal = ref(false)
const showManagersModal = ref(false)
const stationToDelete = ref<StationResponse | null>(null)
const currentStation = ref<StationResponse | null>(null)
const mapStation = ref<StationResponse | null>(null)
const managersStation = ref<StationResponse | null>(null)

// Sorting
const sortField = ref<string>('')
const sortOrder = ref<'asc' | 'desc'>('asc')

// Filters
const deviceCountFilter = ref('all')
const locationFilter = ref('all')

// Computed
const filteredStations = computed(() => {
  let result = stations.value

  // Search filter
  if (searchKeyword.value) {
    result = result.filter(station =>
      station.name.toLowerCase().includes(searchKeyword.value.toLowerCase())
    )
  }

  // Device count filter
  if (deviceCountFilter.value !== 'all') {
    result = result.filter(station => {
      const deviceCount = getDeviceCount(station.id || 0)
      if (deviceCountFilter.value === 'hasDevices') {
        return deviceCount > 0
      } else if (deviceCountFilter.value === 'noDevices') {
        return deviceCount === 0
      }
      return true
    })
  }

  // Location filter
  if (locationFilter.value !== 'all') {
    result = result.filter(station => {
      const hasLocation = station.lat && station.lng
      if (locationFilter.value === 'hasLocation') {
        return hasLocation
      } else if (locationFilter.value === 'noLocation') {
        return !hasLocation
      }
      return true
    })
  }

  return result
})

const sortedAndFilteredStations = computed(() => {
  let result = filteredStations.value

  if (sortField.value) {
    result = [...result].sort((a, b) => {
      let aValue: any = a[sortField.value as keyof StationResponse]
      let bValue: any = b[sortField.value as keyof StationResponse]

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
  }

  return result
})

const totalCount = computed(() => filteredStations.value.length)

const hasActiveFilters = computed(() => {
  return searchKeyword.value ||
         deviceCountFilter.value !== 'all' ||
         locationFilter.value !== 'all'
})

// Methods
const loadStations = async () => {
  try {
    loading.value = true
    stations.value = await StationService.getStations()
  } catch (error) {
    console.error('載入分站失敗:', error)
    toast.error('載入失敗', '無法載入分站列表')
  } finally {
    loading.value = false
  }
}

const loadAccounts = async () => {
  try {
    accounts.value = await AccountService.getAccountList()
  } catch (error) {
    console.error('載入帳戶失敗:', error)
  }
}

const loadDevices = async () => {
  try {
    devices.value = await DeviceService.getDevices()
  } catch (error) {
    console.error('載入設備失敗:', error)
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
  deviceCountFilter.value = 'all'
  locationFilter.value = 'all'
}

const getStationManagers = (stationId: number) => {
  // 根據 stationIds 欄位篩選出管理該分站的用戶
  return accounts.value.filter(account =>
    account.stationIds && account.stationIds.includes(stationId)
  )
}

const getDeviceCount = (stationId: number) => {
  // 優先使用分站數據中的設備統計（後端已預先計算）
  const station = stations.value.find(s => s.id === stationId)
  if (station) {
    const totalDevices = (station.crowdDevices?.length || 0) +
                        (station.fenceDevices?.length || 0) +
                        (station.highResolutionDevices?.length || 0) +
                        (station.parkingDevices?.length || 0) +
                        (station.trafficDevices?.length || 0)
    return totalDevices
  }

  // 備用方案：從全局設備列表中計算
  return devices.value.filter(device => device.stationID === stationId).length
}

const getStationDevices = (stationId: number) => {
  // 優先使用分站數據中的設備列表（後端已預先組織）
  const station = stations.value.find(s => s.id === stationId)
  if (station) {
    const allStationDevices = [
      ...(station.crowdDevices || []),
      ...(station.fenceDevices || []),
      ...(station.highResolutionDevices || []),
      ...(station.parkingDevices || []),
      ...(station.trafficDevices || [])
    ]
    return allStationDevices
  }

  // 備用方案：從全局設備列表中過濾
  return devices.value.filter(device => device.stationID === stationId)
}

const getStationDeviceTypeSummary = (stationId: number) => {
  const station = stations.value.find(s => s.id === stationId)
  if (station) {
    const typeCounts = []
    if (station.crowdDevices?.length) typeCounts.push(`人流: ${station.crowdDevices.length}`)
    if (station.parkingDevices?.length) typeCounts.push(`停車: ${station.parkingDevices.length}`)
    if (station.trafficDevices?.length) typeCounts.push(`車流: ${station.trafficDevices.length}`)
    if (station.fenceDevices?.length) typeCounts.push(`圍籬: ${station.fenceDevices.length}`)
    if (station.highResolutionDevices?.length) typeCounts.push(`4K: ${station.highResolutionDevices.length}`)
    return typeCounts.join(' | ')
  }

  // 備用方案
  const devices = getStationDevices(stationId)
  return devices.map(d => d.type).join(', ')
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

const openUpdateModal = (station: StationResponse) => {
  currentStation.value = station
  showUpdateModal.value = true
}

const confirmDeleteStation = (station: StationResponse) => {
  stationToDelete.value = station
  showDeleteModal.value = true
}

const viewOnMap = (station: StationResponse) => {
  mapStation.value = station
  showMapModal.value = true
}

const viewStationManagers = (station: StationResponse) => {
  managersStation.value = station
  showManagersModal.value = true
}

// 處理分站操作成功後的更新
const handleStationSuccess = async () => {
  // 重新載入所有相關數據以確保數據同步
  await Promise.all([
    loadStations(),
    loadAccounts(),
    loadDevices()
  ])
}

// 手動刷新所有數據
const refreshAllData = async () => {
  loading.value = true
  try {
    await Promise.all([
      loadStations(),
      loadAccounts(),
      loadDevices()
    ])
    toast.success('重新整理完成', '分站和設備數據已更新')
  } catch (error) {
    toast.error('重新整理失敗', '請稍後再試')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  // 確保所有數據完全載入後再顯示
  loading.value = true
  try {
    await Promise.all([
      loadStations(),
      loadAccounts(),
      loadDevices()
    ])
  } finally {
    loading.value = false
  }
})
</script>
