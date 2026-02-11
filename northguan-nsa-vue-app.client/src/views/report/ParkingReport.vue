<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- 頁面標題 -->
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900">停車場紀錄列表與報表下載</h1>
        <p class="text-gray-600 mt-2">查看和下載停車場監控的詳細記錄</p>
      </div>

      <!-- 搜尋和篩選區域 -->
      <Card class="mb-6">
        <CardContent class="p-6">
          <div class="flex flex-wrap gap-4 items-end">
            <!-- 開始日期 -->
            <div class="w-48">
              <Label class="text-sm font-medium mb-2 block">開始日期</Label>
              <DatePicker v-model="startDate" @update:model-value="handleDateChange" class="w-full" />
            </div>

            <!-- 結束日期 -->
            <div class="w-48">
              <Label class="text-sm font-medium mb-2 block">結束日期</Label>
              <DatePicker v-model="endDate" @update:model-value="handleDateChange" class="w-full" />
            </div>

            <!-- 分站選擇 -->
            <div class="w-48">
              <Label class="text-sm font-medium mb-2 block">分站</Label>
              <Select v-model="selectedStationId">
                <SelectTrigger>
                  <SelectValue placeholder="選擇分站" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">全部</SelectItem>
                  <SelectItem v-for="station in stations" :key="station.id" :value="station.id.toString()">
                    {{ station.name }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            <!-- 占用率範圍篩選 -->
            <div class="w-48">
              <Label class="text-sm font-medium mb-2 block">占用率範圍(%)</Label>
              <div class="flex gap-2">
                <Input v-model.number="minOccupancyRate" placeholder="最小" type="number" min="0" max="100"
                  class="w-20" />
                <span class="self-center">-</span>
                <Input v-model.number="maxOccupancyRate" placeholder="最大" type="number" min="0" max="100"
                  class="w-20" />
              </div>
            </div>

            <!-- 設備名稱搜尋 -->
            <div class="w-48">
              <Label class="text-sm font-medium mb-2 block">設備名稱</Label>
              <Input v-model="deviceNameKeyword" placeholder="輸入設備名稱" class="w-full" />
            </div>

            <!-- 搜尋按鈕 -->
            <Button @click="search" :disabled="loading">
              <SearchIcon class="w-4 h-4 mr-2" />
              搜尋
            </Button>

            <!-- 匯出按鈕 -->
            <Button @click="exportExcel" variant="outline" :disabled="loading || exporting">
              <DownloadIcon class="w-4 h-4 mr-2" />
              {{ exporting ? '匯出中...' : '匯出' }}
            </Button>
          </div>
        </CardContent>
      </Card>

      <!-- 資料表格 -->
      <Card>
        <CardContent class="p-0">
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">序號</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900 cursor-pointer hover:bg-gray-100"
                    @click="toggleSort('stationName')">
                    <div class="flex items-center">
                      分站
                      <span class="ml-1">
                        <ChevronUpIcon v-if="sortBy === 'stationName' && sortOrder === 'asc'" class="w-4 h-4" />
                        <ChevronDownIcon v-else-if="sortBy === 'stationName' && sortOrder === 'desc'" class="w-4 h-4" />
                        <ChevronsUpDownIcon v-else class="w-4 h-4 text-gray-400" />
                      </span>
                    </div>
                  </th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900 cursor-pointer hover:bg-gray-100"
                    @click="toggleSort('deviceName')">
                    <div class="flex items-center">
                      設備
                      <span class="ml-1">
                        <ChevronUpIcon v-if="sortBy === 'deviceName' && sortOrder === 'asc'" class="w-4 h-4" />
                        <ChevronDownIcon v-else-if="sortBy === 'deviceName' && sortOrder === 'desc'" class="w-4 h-4" />
                        <ChevronsUpDownIcon v-else class="w-4 h-4 text-gray-400" />
                      </span>
                    </div>
                  </th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900 cursor-pointer hover:bg-gray-100"
                    @click="toggleSort('parkedNum')">
                    <div class="flex items-center">
                      停駐車輛
                      <span class="ml-1">
                        <ChevronUpIcon v-if="sortBy === 'parkedNum' && sortOrder === 'asc'" class="w-4 h-4" />
                        <ChevronDownIcon v-else-if="sortBy === 'parkedNum' && sortOrder === 'desc'" class="w-4 h-4" />
                        <ChevronsUpDownIcon v-else class="w-4 h-4 text-gray-400" />
                      </span>
                    </div>
                  </th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900 cursor-pointer hover:bg-gray-100"
                    @click="toggleSort('occupancyRate')">
                    <div class="flex items-center">
                      停駐率
                      <span class="ml-1">
                        <ChevronUpIcon v-if="sortBy === 'occupancyRate' && sortOrder === 'asc'" class="w-4 h-4" />
                        <ChevronDownIcon v-else-if="sortBy === 'occupancyRate' && sortOrder === 'desc'"
                          class="w-4 h-4" />
                        <ChevronsUpDownIcon v-else class="w-4 h-4 text-gray-400" />
                      </span>
                    </div>
                  </th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">總車位</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">可用車位</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900 cursor-pointer hover:bg-gray-100"
                    @click="toggleSort('timestamp')">
                    <div class="flex items-center">
                      時間
                      <span class="ml-1">
                        <ChevronUpIcon v-if="sortBy === 'timestamp' && sortOrder === 'asc'" class="w-4 h-4" />
                        <ChevronDownIcon v-else-if="sortBy === 'timestamp' && sortOrder === 'desc'" class="w-4 h-4" />
                        <ChevronsUpDownIcon v-else class="w-4 h-4 text-gray-400" />
                      </span>
                    </div>
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                <tr v-if="loading">
                  <td colspan="8" class="px-4 py-8 text-center text-gray-500">
                    載入中...
                  </td>
                </tr>
                <tr v-else-if="!records.length">
                  <td colspan="8" class="px-4 py-8 text-center text-gray-500">
                    暫無資料
                  </td>
                </tr>
                <tr v-else v-for="(record, index) in records" :key="record.id" class="hover:bg-gray-50">
                  <td class="px-4 py-3 text-sm text-gray-900">
                    #{{ (currentPage - 1) * pageSize + index + 1 }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.stationName }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.deviceName }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.parkedNum }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="flex items-center">
                      <img :src="getParkingStatusIcon(record.status || getParkingStatus(record.occupancyRate))"
                        :alt="record.status || getParkingStatus(record.occupancyRate)" class="w-5 h-5 mr-2" />
                      {{ record.parkingRate || record.occupancyRate.toFixed(1) }}%
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.totalSpaces || '-' }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.availableSpaces || '-' }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.time }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 分頁控制 -->
          <div v-if="totalCount > 0" class="flex items-center justify-between px-4 py-3 border-t">
            <div class="text-sm text-gray-600">
              第 {{ currentPage }} 頁，共 {{ totalPages }} 頁
            </div>

            <div class="flex gap-2">
              <Button variant="outline" size="sm" @click="changePage(currentPage - 1)" :disabled="currentPage === 1">
                <ChevronLeftIcon class="w-4 h-4" />
              </Button>
              <Button variant="outline" size="sm" @click="changePage(currentPage + 1)"
                :disabled="currentPage === totalPages">
                <ChevronRightIcon class="w-4 h-4" />
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { SearchIcon, DownloadIcon, ChevronLeftIcon, ChevronRightIcon, ChevronUpIcon, ChevronDownIcon, ChevronsUpDownIcon } from 'lucide-vue-next'
import { CalendarDate } from '@internationalized/date'

// Components
import AppHeader from '@/components/layout/AppHeader.vue'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { DatePicker } from '@/components/ui/date-picker'

// Services
import { StationService, RecordService } from '@/services'
import { useToast } from '@/composables/useToast'

// Types

interface Station {
  id: number
  name: string
}

interface ParkingRecord {
  id: number
  deviceSerial: string
  deviceName: string
  stationName: string
  totalSpaces: number
  parkedNum: number
  availableSpaces: number
  occupancyRate: number
  parkingRate?: number
  convertRate?: number
  status?: string
  time: string
  timestamp: number
}

const toast = useToast()

// State
const loading = ref(true)
const exporting = ref(false)
const selectedStationId = ref('all')
const minOccupancyRate = ref<number>()
const maxOccupancyRate = ref<number>()
const deviceNameKeyword = ref('')
const sortBy = ref('timestamp')
const sortOrder = ref<'asc' | 'desc'>('desc')
const today = new Date()
const startDate = ref(new CalendarDate(today.getFullYear(), today.getMonth() + 1, 1))
const endDate = ref(new CalendarDate(today.getFullYear(), today.getMonth() + 1, today.getDate()))

const stations = ref<Station[]>([])
const records = ref<ParkingRecord[]>([])

// 分頁
const currentPage = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

// Computed
const totalPages = computed(() => {
  return Math.ceil(totalCount.value / pageSize.value)
})

// Methods
const loadStations = async () => {
  try {
    const response = await StationService.getStations()
    stations.value = response.map((station) => ({
      id: station.id,
      name: station.name
    })) || []
  } catch (error) {
    console.error('載入分站失敗:', error)
    toast.error('載入分站失敗')
  }
}

const search = async () => {
  loading.value = true
  try {
    const stationIds = selectedStationId.value === 'all' ? undefined : [parseInt(selectedStationId.value)]

    // 如果有選擇日期則格式化，否則為 undefined
    const startDateStr = startDate.value ? new Date(startDate.value.year, startDate.value.month - 1, startDate.value.day).toISOString() : undefined;
    const endDateStr = endDate.value ? new Date(endDate.value.year, endDate.value.month - 1, endDate.value.day, 23, 59, 59, 999).toISOString() : undefined;

    const response = await RecordService.getParkingRecords({
      startDate: startDateStr,
      endDate: endDateStr,
      stationIds,
      keyword: deviceNameKeyword.value,
      minOccupancyRate: minOccupancyRate.value,
      maxOccupancyRate: maxOccupancyRate.value,
      page: currentPage.value,
      size: pageSize.value,
      sortBy: sortBy.value,
      sortOrder: sortOrder.value
    })

    // 處理API響應結構
    if (response.data && response.data.data) {
      records.value = response.data.data || []
      totalCount.value = response.data.totalCount || 0
    } else {
      records.value = response.data || []
      totalCount.value = response.totalCount || 0
    }
    toast.success(`搜尋完成，共找到 ${totalCount.value} 筆記錄`)
  } catch (error) {
    console.error('搜尋失敗:', error)
    toast.error('搜尋失敗')
    records.value = []
    totalCount.value = 0
  } finally {
    loading.value = false
  }
}

const changePage = async (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    await search()
  }
}

const exportExcel = async () => {
  exporting.value = true
  try {
    const stationIds = selectedStationId.value === 'all' ? undefined : [parseInt(selectedStationId.value)]

    await RecordService.exportParkingRecords({
      startDate: startDate.value || undefined,
      endDate: endDate.value || undefined,
      stationIds
    })

    toast.success('匯出成功')
  } catch (error) {
    console.error('匯出失敗:', error)
    toast.error('匯出失敗')
  } finally {
    exporting.value = false
  }
}

const handleDateChange = () => {
  // 重置到第一頁
  currentPage.value = 1
}

const getParkingStatusIcon = (status: string) => {
  switch (status) {
    case '擁擠':
      return '/images/icons/parking-red.svg'
    case '稍擠':
      return '/images/icons/parking-yellow.svg'
    default:
      return '/images/icons/parking-green.svg'
  }
}

// 根據占用率計算狀態
const getParkingStatus = (occupancyRate: number) => {
  if (occupancyRate >= 80) return '擁擠'
  if (occupancyRate >= 60) return '稍擠'
  return '正常'
}

// 排序功能
const toggleSort = (field: string) => {
  if (sortBy.value === field) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = field
    sortOrder.value = 'desc'
  }
  currentPage.value = 1
  search()
}

// Lifecycle
onMounted(async () => {
  await loadStations()
  await search()
})
</script>
