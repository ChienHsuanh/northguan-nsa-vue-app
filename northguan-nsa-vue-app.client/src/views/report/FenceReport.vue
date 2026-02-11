<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- 頁面標題 -->
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900">電子圍籬事件報表</h1>
        <p class="text-gray-600 mt-2">查看和下載電子圍籬事件的詳細記錄</p>
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

            <!-- 事件類型篩選 -->
            <div class="w-48">
              <Label class="text-sm font-medium mb-2 block">事件類型</Label>
              <Select v-model="selectedEventType">
                <SelectTrigger>
                  <SelectValue placeholder="選擇事件類型" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">全部</SelectItem>
                  <SelectItem value="Enter">闖入</SelectItem>
                  <SelectItem value="Exit">離開</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <!-- 設備序號搜尋 -->
            <div class="w-48">
              <Label class="text-sm font-medium mb-2 block">設備序號</Label>
              <Input v-model="deviceSerialKeyword" placeholder="輸入設備序號" class="w-full" />
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
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900 cursor-pointer hover:bg-gray-100"
                    @click="toggleSort('deviceName')">
                    <div class="flex items-center">
                      設備名稱
                      <span class="ml-1">
                        <ChevronUpIcon v-if="sortBy === 'deviceName' && sortOrder === 'asc'" class="w-4 h-4" />
                        <ChevronDownIcon v-else-if="sortBy === 'deviceName' && sortOrder === 'desc'" class="w-4 h-4" />
                        <ChevronsUpDownIcon v-else class="w-4 h-4 text-gray-400" />
                      </span>
                    </div>
                  </th>
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
                    @click="toggleSort('event')">
                    <div class="flex items-center">
                      事件類型
                      <span class="ml-1">
                        <ChevronUpIcon v-if="sortBy === 'event' && sortOrder === 'asc'" class="w-4 h-4" />
                        <ChevronDownIcon v-else-if="sortBy === 'event' && sortOrder === 'desc'" class="w-4 h-4" />
                        <ChevronsUpDownIcon v-else class="w-4 h-4 text-gray-400" />
                      </span>
                    </div>
                  </th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">設備序號</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">照片</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                <tr v-if="loading">
                  <td colspan="7" class="px-4 py-8 text-center text-gray-500">
                    載入中...
                  </td>
                </tr>
                <tr v-else-if="!records.length">
                  <td colspan="7" class="px-4 py-8 text-center text-gray-500">
                    暫無資料
                  </td>
                </tr>
                <tr v-else v-for="(record, index) in records" :key="record.id" class="hover:bg-gray-50">
                  <td class="px-4 py-3 text-sm text-gray-900">
                    #{{ (currentPage - 1) * pageSize + index + 1 }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.time }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.deviceName }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.stationName }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.event }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.deviceSerial }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div v-if="record.imageUrl" class="flex justify-center">
                      <img :src="record.imageUrl" @click="showPhotoModal(record.imageUrl)"
                        @error="handleTableImageError"
                        class="w-42 h-24 object-cover rounded cursor-pointer hover:opacity-80 transition-opacity border border-gray-200" />
                    </div>
                    <span v-else class="text-gray-400 text-xs">無照片</span>
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

    <!-- Photo Modal -->
    <Dialog v-model:open="showingPhoto">
      <DialogContent class="max-w-5xl max-h-[90vh] p-6">
        <DialogHeader>
          <DialogTitle class="text-center">
            圍籬事件照片
            <span v-if="availablePhotos.length > 1" class="text-sm text-gray-500 font-normal">
              ({{ currentPhotoIndex + 1 }} / {{ availablePhotos.length }})
            </span>
          </DialogTitle>
        </DialogHeader>
        <div class="relative flex justify-center items-center min-h-[60vh]">
          <!-- Previous Button -->
          <Button v-if="availablePhotos.length > 1" @click="showPreviousPhoto" variant="outline" size="sm"
            class="absolute left-4 top-1/2 transform -translate-y-1/2 z-10 bg-white/80 hover:bg-white"
            :disabled="currentPhotoIndex === 0">
            <ArrowLeftIcon class="w-4 h-4" />
          </Button>

          <!-- Next Button -->
          <Button v-if="availablePhotos.length > 1" @click="showNextPhoto" variant="outline" size="sm"
            class="absolute right-4 top-1/2 transform -translate-y-1/2 z-10 bg-white/80 hover:bg-white"
            :disabled="currentPhotoIndex === availablePhotos.length - 1">
            <ArrowRightIcon class="w-4 h-4" />
          </Button>

          <!-- Loading Skeleton -->
          <div v-if="imageLoading" class="w-full max-w-2xl h-96 rounded animate-pulse flex items-center justify-center">
            <div class="w-8 h-8 border-4 border-gray-300 border-t-blue-500 rounded-full animate-spin"></div>
          </div>

          <!-- Error State -->
          <div v-else-if="imageError"
            class="w-full max-w-2xl h-96 bg-gray-100 rounded border-2 border-dashed border-gray-300 flex flex-col items-center justify-center text-gray-500">
            <AlertCircleIcon class="w-12 h-12 mb-4 text-gray-400" />
            <p class="text-sm font-medium">圖片加載失敗</p>
            <p class="text-xs text-gray-400 mt-1">請稍後再試</p>
          </div>

          <!-- Image -->
          <img v-else-if="!imageLoading && !imageError && currentPhoto" :src="currentPhoto"
            class="w-full max-w-6xl max-h-[75vh] object-contain rounded shadow-lg" />

          <!-- Hidden preloader image -->
          <img v-if="currentPhoto" :src="currentPhoto" @load="handleImageLoad" @error="handleImageError"
            style="display: none" ref="modalImage" />
        </div>

        <!-- Navigation hints -->
        <div v-if="availablePhotos.length > 1" class="text-center text-sm text-gray-500 mt-4">
          使用左右箭頭鍵或點擊按鈕切換圖片
        </div>
      </DialogContent>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { SearchIcon, DownloadIcon, ChevronLeftIcon, ChevronRightIcon, ChevronUpIcon, ChevronDownIcon, ChevronsUpDownIcon, AlertCircleIcon, ArrowLeftIcon, ArrowRightIcon } from 'lucide-vue-next'
import { CalendarDate } from '@internationalized/date'

// Components
import AppHeader from '@/components/layout/AppHeader.vue'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { DatePicker } from '@/components/ui/date-picker'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'

// Services
import { StationService, RecordService } from '@/services'
import { useToast } from '@/composables/useToast'

// Types

interface Station {
  id: number
  name: string
}

interface FenceRecord {
  id: number
  deviceSerial: string
  deviceName: string
  stationName: string
  event: string
  imageUrl?: string
  time: string
  timestamp: number
}

const toast = useToast()

// State
const loading = ref(true)
const exporting = ref(false)
const selectedStationId = ref('all')
const selectedEventType = ref('all')
const deviceSerialKeyword = ref('')
const sortBy = ref('timestamp')
const sortOrder = ref<'asc' | 'desc'>('desc')
const today = new Date()
const startDate = ref(new CalendarDate(today.getFullYear(), today.getMonth() + 1, 1))
const endDate = ref(new CalendarDate(today.getFullYear(), today.getMonth() + 1, today.getDate()))

const stations = ref<Station[]>([])
const records = ref<FenceRecord[]>([])

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
    const eventTypes = selectedEventType.value === 'all' ? undefined : [selectedEventType.value]
    const deviceSerials = deviceSerialKeyword.value ? [deviceSerialKeyword.value] : undefined

    // 如果有選擇日期則格式化，否則為 undefined
    const startDateStr = startDate.value ? `${startDate.value.year}-${startDate.value.month.toString().padStart(2, '0')}-${startDate.value.day.toString().padStart(2, '0')}` : undefined;
    const endDateStr = endDate.value ? `${endDate.value.year}-${endDate.value.month.toString().padStart(2, '0')}-${endDate.value.day.toString().padStart(2, '0')} 23:59:59` : undefined;

    const response = await RecordService.getFenceRecords({
      startDate: startDateStr,
      endDate: endDateStr,
      stationIds,
      eventTypes,
      deviceSerials,
      page: currentPage.value,
      size: pageSize.value,
      sortBy: sortBy.value,
      sortOrder: sortOrder.value
    })

    // 處理API響應結構
    if (response.data) {
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

    await RecordService.exportFenceRecords({
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

// 照片模態框相關
const showingPhoto = ref(false)
const currentPhoto = ref('')
const imageLoading = ref(true)
const imageError = ref(false)
const modalImage = ref<HTMLImageElement>()
const currentPhotoIndex = ref(0)
const availablePhotos = ref<string[]>([])

const showPhotoModal = (photoUrl: string) => {
  // 收集所有有照片的記錄
  availablePhotos.value = records.value
    .filter(record => record.imageUrl)
    .map(record => record.imageUrl!)

  // 找到當前照片的索引
  currentPhotoIndex.value = availablePhotos.value.indexOf(photoUrl)
  if (currentPhotoIndex.value === -1) {
    currentPhotoIndex.value = 0
  }

  currentPhoto.value = photoUrl
  imageLoading.value = true
  imageError.value = false
  showingPhoto.value = true
}


const handleImageLoad = () => {
  imageLoading.value = false
  imageError.value = false
}

const handleImageError = () => {
  imageLoading.value = false
  imageError.value = true
}

const handleTableImageError = (event: Event) => {
  const img = event.target as HTMLImageElement
  img.src = '/images/image-placeholder.png'
  img.onerror = null // Prevent infinite loop
}

// 圖片導航功能
const showPreviousPhoto = () => {
  if (currentPhotoIndex.value > 0) {
    const newIndex = currentPhotoIndex.value - 1
    const newPhoto = availablePhotos.value[newIndex]

    // 如果是同一張圖片，不需要重新載入
    if (newPhoto === currentPhoto.value) {
      currentPhotoIndex.value = newIndex
      return
    }

    currentPhotoIndex.value = newIndex
    currentPhoto.value = newPhoto
    imageLoading.value = true
    imageError.value = false
  }
}

const showNextPhoto = () => {
  if (currentPhotoIndex.value < availablePhotos.value.length - 1) {
    const newIndex = currentPhotoIndex.value + 1
    const newPhoto = availablePhotos.value[newIndex]

    // 如果是同一張圖片，不需要重新載入
    if (newPhoto === currentPhoto.value) {
      currentPhotoIndex.value = newIndex
      return
    }

    currentPhotoIndex.value = newIndex
    currentPhoto.value = newPhoto
    imageLoading.value = true
    imageError.value = false
  }
}

// 鍵盤事件處理
const handleKeydown = (event: KeyboardEvent) => {
  if (!showingPhoto.value) return

  if (event.key === 'ArrowLeft') {
    event.preventDefault()
    showPreviousPhoto()
  } else if (event.key === 'ArrowRight') {
    event.preventDefault()
    showNextPhoto()
  } else if (event.key === 'Escape') {
    event.preventDefault()
    showingPhoto.value = false
  }
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

  // 添加鍵盤事件監聽器
  document.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  // 移除鍵盤事件監聽器
  document.removeEventListener('keydown', handleKeydown)
})
</script>
