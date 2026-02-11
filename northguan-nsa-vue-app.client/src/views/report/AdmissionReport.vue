<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- 頁面標題 -->
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900">無接觸入場人次紀錄列表與報表下載</h1>
        <p class="text-gray-600 mt-2">查看和下載無接觸入場人次的詳細記錄</p>
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

            <!-- 搜尋按鈕 -->
            <Button @click="search" :disabled="loading">
              <SearchIcon class="w-4 h-4 mr-2" />
              搜尋
            </Button>

            <!-- 匯出按鈕 -->
            <Button @click="exportExcel" variant="outline" :disabled="loading">
              <DownloadIcon class="w-4 h-4 mr-2" />
              匯出
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
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">分站</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">時間</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">行程</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">票種</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">數量</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                <tr v-if="loading">
                  <td colspan="6" class="px-4 py-8 text-center text-gray-500">
                    載入中...
                  </td>
                </tr>
                <tr v-else-if="!records.length">
                  <td colspan="6" class="px-4 py-8 text-center text-gray-500">
                    暫無資料
                  </td>
                </tr>
                <tr v-else v-for="(record, index) in records" :key="index" class="hover:bg-gray-50">
                  <td class="px-4 py-3 text-sm text-gray-900">
                    #{{ index + 1 }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    和平島
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.date }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.evtName }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.ticketName }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.value }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { SearchIcon, DownloadIcon } from 'lucide-vue-next'
import { CalendarDate } from '@internationalized/date'

// Components
import AppHeader from '@/components/layout/AppHeader.vue'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { DatePicker } from '@/components/ui/date-picker'

// Services
import { StationService } from '@/services'
// import { ExportService } from '@/services/ExportService'
import { useToast } from '@/composables/useToast'

// Types
import type { Station, AdmissionRecord } from '@/types/report-types'

const toast = useToast()

// State
const loading = ref(true)
const selectedStationId = ref('all')
const today = new Date()
const startDate = ref(new CalendarDate(today.getFullYear(), today.getMonth() + 1, 1))
const endDate = ref(new CalendarDate(today.getFullYear(), today.getMonth() + 1, today.getDate()))

const stations = ref<Station[]>([])
const records = ref<AdmissionRecord[]>([])

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
    // TODO: 實作 API 調用，並確保將 API 返回的 UTC 時間轉換為台灣時區
    // const response = await AdmissionService.getRecords({
    //   startDate: dateRange.value.start,
    //   endDate: dateRange.value.end,
    //   stationId: selectedStationId.value
    // })
    // records.value = response.data.map(record => ({ ...record, date: record.date })) || []

    records.value = []
    toast.success('搜尋完成')
  } catch (error) {
    console.error('搜尋失敗:', error)
    toast.error('搜尋失敗')
  } finally {
    loading.value = false
  }
}

const exportExcel = async () => {
  try {
    // await ExportService.exportAdmissionRecords({
    //   startDate: startDate.value || undefined,
    //   endDate: endDate.value || undefined,
    //   stationId: selectedStationId.value
    // })
    toast.success('匯出成功')
  } catch (error) {
    console.error('匯出失敗:', error)
    toast.error('匯出失敗')
  }
}

const handleDateChange = () => {
  // 日期變更時的處理
}

// Lifecycle
onMounted(async () => {
  await loadStations()
  await search()
})
</script>
