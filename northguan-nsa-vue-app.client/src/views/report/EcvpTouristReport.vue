<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- 頁面標題 -->
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900">客群輪廓與報表下載</h1>
        <p class="text-gray-600 mt-2">查看和分析遊客客群輪廓數據</p>
      </div>

      <!-- 搜尋和篩選區域 -->
      <Card class="mb-6">
        <CardContent class="p-6">
          <div class="space-y-4">
            <!-- 第一行：日期範圍、位置選擇、搜尋和匯出 -->
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

              <!-- 位置選擇 -->
              <div class="w-48">
                <Label class="text-sm font-medium mb-2 block">位置</Label>
                <Select v-model="selectedLocation">
                  <SelectTrigger>
                    <SelectValue placeholder="選擇位置" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="station in currentStationOptions" :key="station.name" :value="station.name">
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

            <!-- 第二行：面板類型選擇 -->
            <div class="flex gap-4 items-center">
              <Label class="text-sm font-medium">資料類型：</Label>
              <div class="flex gap-2">
                <Button :variant="panelType === 'E2TOURIST' ? 'default' : 'outline'" @click="panelType = 'E2TOURIST'"
                  size="sm">
                  日人潮輪廓數
                </Button>
                <Button :variant="panelType === 'G2TOURIST' ? 'default' : 'outline'" @click="panelType = 'G2TOURIST'"
                  size="sm">
                  分時人潮輪廓數
                </Button>
              </div>
            </div>

            <!-- 第三行：圖表選項 -->
            <div class="flex gap-4 items-center">
              <div class="w-48">
                <Label class="text-sm font-medium mb-2 block">圖表欄位</Label>
                <Select v-model="chartOptions.field">
                  <SelectTrigger>
                    <SelectValue placeholder="選擇欄位" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="(label, value) in currentChartFields" :key="value" :value="value">
                      {{ label }}
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div v-if="panelType === 'E2TOURIST'" class="w-32">
                <Label class="text-sm font-medium mb-2 block">停留時間</Label>
                <Select v-model="chartOptions.stay_mins">
                  <SelectTrigger>
                    <SelectValue placeholder="選擇時間" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="10">10 分鐘</SelectItem>
                    <SelectItem value="30">30 分鐘</SelectItem>
                    <SelectItem value="60">60 分鐘</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- 圖表區域 -->
      <Card v-if="chartData.length" class="mb-6">
        <CardHeader>
          <CardTitle>數據趨勢圖</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="h-96">
            <div v-if="chartLoading" class="flex items-center justify-center h-full">
              <div class="text-gray-500">載入圖表中...</div>
            </div>
            <div v-else ref="chartContainer" class="w-full h-full"></div>
          </div>
        </CardContent>
      </Card>

      <!-- 資料表格 -->
      <Card>
        <CardContent class="p-0">
          <div class="overflow-x-auto">
            <!-- E2TOURIST 表格 -->
            <table v-if="panelType === 'E2TOURIST'" class="w-full">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">序號</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">位置</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">日期</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">停留時間</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">當下人數(男性/女性)</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">年齡</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900" colspan="4">縣市旅客</th>
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
                <tr v-else v-for="(record, index) in paginatedRecords" :key="index" class="hover:bg-gray-50">
                  <td class="px-4 py-3 text-sm text-gray-900">
                    #{{ (currentPage - 1) * pageSize + index + 1 }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.ev_name }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.yyyymmdd }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.stay_mins }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.male + record.female }}({{ record.male }}/{{ record.female }})
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div>0-19歲人數 - {{ record.age19 }}</div>
                      <div>20-29歲人數 - {{ record.age29 }}</div>
                      <div>30-39歲人數 - {{ record.age39 }}</div>
                      <div>40-49歲人數 - {{ record.age49 }}</div>
                      <div>50-59歲人數 - {{ record.age59 }}</div>
                      <div>60歲以上人數 - {{ record.age60 }}</div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="item in record.travelerAreaPart1" :key="item.label">
                        {{ item.label }}: {{ item.val }}
                      </div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="item in record.travelerAreaPart2" :key="item.label">
                        {{ item.label }}: {{ item.val }}
                      </div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="item in record.travelerAreaPart3" :key="item.label">
                        {{ item.label }}: {{ item.val }}
                      </div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="item in record.travelerAreaPart4" :key="item.label">
                        {{ item.label }}: {{ item.val }}
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>

            <!-- G2TOURIST 表格 -->
            <table v-if="panelType === 'G2TOURIST'" class="w-full">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">序號</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">位置</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">日期</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">當下人數(男性/女性)</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">年齡</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900">外籍人數</th>
                  <th class="px-4 py-3 text-left text-sm font-medium text-gray-900" colspan="4">縣市旅客</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200">
                <tr v-if="loading">
                  <td colspan="9" class="px-4 py-8 text-center text-gray-500">
                    載入中...
                  </td>
                </tr>
                <tr v-else-if="!records.length">
                  <td colspan="9" class="px-4 py-8 text-center text-gray-500">
                    暫無資料
                  </td>
                </tr>
                <tr v-else v-for="(record, index) in paginatedRecords" :key="index" class="hover:bg-gray-50">
                  <td class="px-4 py-3 text-sm text-gray-900">
                    #{{ (currentPage - 1) * pageSize + index + 1 }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.name }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record._time }}
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    {{ record.male + record.female }}({{ record.male }}/{{ record.female }})
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div>0-19歲人數 - {{ record.age0019 }}</div>
                      <div>20-29歲人數 - {{ record.age2029 }}</div>
                      <div>30-39歲人數 - {{ record.age3039 }}</div>
                      <div>40-49歲人數 - {{ record.age4049 }}</div>
                      <div>50-59歲人數 - {{ record.age5059 }}</div>
                      <div>60歲以上人數 - {{ record.age6099 }}</div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="(item, ind) in record._national" :key="ind">
                        {{ item[0] }}: {{ item[1] }}
                      </div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="(item, ind) in record.travelerAreaPart1" :key="ind">
                        {{ item.label }}: {{ item.val }}
                      </div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="(item, ind) in record.travelerAreaPart2" :key="ind">
                        {{ item.label }}: {{ item.val }}
                      </div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="(item, ind) in record.travelerAreaPart3" :key="ind">
                        {{ item.label }}: {{ item.val }}
                      </div>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-900">
                    <div class="space-y-1">
                      <div v-for="(item, ind) in record.travelerAreaPart4" :key="ind">
                        {{ item.label }}: {{ item.val }}
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 分頁控制 -->
          <div v-if="records.length" class="flex items-center justify-between px-4 py-3 border-t">
            <div class="flex items-center gap-2">
              <Select v-model="currentPage" @update:model-value="changePage">
                <SelectTrigger class="w-20">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="pageNo in totalPages" :key="pageNo" :value="pageNo.toString()">
                    {{ pageNo }}
                  </SelectItem>
                </SelectContent>
              </Select>
              <span class="text-sm text-gray-600">of {{ totalPages }}</span>
            </div>

            <div class="flex gap-2">
              <Button variant="outline" size="sm" @click="changePage((currentPage - 1).toString())"
                :disabled="currentPage === 1">
                <ChevronLeftIcon class="w-4 h-4" />
              </Button>
              <Button variant="outline" size="sm" @click="changePage((currentPage + 1).toString())"
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
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { SearchIcon, DownloadIcon, ChevronLeftIcon, ChevronRightIcon } from 'lucide-vue-next'
import { CalendarDate } from '@internationalized/date'
import * as echarts from 'echarts'

// Components
import AppHeader from '@/components/layout/AppHeader.vue'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { DatePicker } from '@/components/ui/date-picker'

// Services
// import { ExportService } from '@/services/ExportService'
import { useToast } from '@/composables/useToast'

// Types
import type { E2TouristRecord, G2TouristRecord } from '@/types/report-types'

const toast = useToast()

// State
const loading = ref(true)
const chartLoading = ref(false)
const selectedLocation = ref('')
const today = new Date()
const startDate = ref(new CalendarDate(today.getFullYear(), today.getMonth() + 1, 1))
const endDate = ref(new CalendarDate(today.getFullYear(), today.getMonth() + 1, today.getDate()))

const panelType = ref<'E2TOURIST' | 'G2TOURIST'>('E2TOURIST')
const chartOptions = ref({
  field: '',
  stay_mins: '10'
})

const records = ref<(E2TouristRecord | G2TouristRecord)[]>([])
const chartData = ref<{ date: string; value: number }[]>([])
const chartContainer = ref<HTMLElement>()
let chartInstance: echarts.ECharts | null = null

// 分頁
const currentPage = ref(1)
const pageSize = ref(10)

// 模擬資料
const stationOptions = ref({
  E2TOURIST: [
    { name: '和平島公園入口' },
    { name: '觀景台' },
    { name: '海蝕平台' }
  ],
  G2TOURIST: [
    { name: '和平島公園入口' },
    { name: '觀景台' },
    { name: '海蝕平台' }
  ]
})

const e2ChartTargetFields = {
  'male': '男性人數',
  'female': '女性人數',
  'age19': '0-19歲',
  'age29': '20-29歲',
  'age39': '30-39歲',
  'age49': '40-49歲',
  'age59': '50-59歲',
  'age60': '60歲以上'
}

const g2ChartTargetFields = {
  'male': '男性人數',
  'female': '女性人數',
  'age0019': '0-19歲',
  'age2029': '20-29歲',
  'age3039': '30-39歲',
  'age4049': '40-49歲',
  'age5059': '50-59歲',
  'age6099': '60歲以上'
}

// Computed
const currentStationOptions = computed(() => {
  return stationOptions.value[panelType.value] || []
})

const currentChartFields = computed(() => {
  return panelType.value === 'E2TOURIST' ? e2ChartTargetFields : g2ChartTargetFields
})

const totalPages = computed(() => {
  return Math.ceil(records.value.length / pageSize.value)
})

const paginatedRecords = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return records.value.slice(start, end)
})

// Methods
const search = async () => {
  loading.value = true
  try {

    // const response = await ECvpTouristService.getRecords({
    //   startDate: dateRange.value.start,
    //   endDate: dateRange.value.end,
    //   location: selectedLocation.value,
    //   panelType: panelType.value
    // })
    // records.value = response.data || []

    // 暫時使用模擬資料
    records.value = []
    chartData.value = []
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
    // await ExportService.exportEcvpTouristRecords({
    //   startDate: startDate.value || undefined,
    //   endDate: endDate.value || undefined,
    //   stationId: selectedLocation.value,
    //   panelType: panelType.value
    // })
    toast.success('匯出成功')
  } catch (error) {
    console.error('匯出失敗:', error)
    toast.error('匯出失敗')
  }
}

const changePage = (page: string) => {
  currentPage.value = parseInt(page)
}

const handleDateChange = () => {
  // 日期變更時的處理
}

const initChart = async () => {
  await nextTick()
  if (chartContainer.value && chartData.value.length) {
    if (chartInstance) {
      chartInstance.dispose()
    }
    chartInstance = echarts.init(chartContainer.value)

    // 設置圖表選項
    const option = {
      title: {
        text: '客群輪廓趨勢'
      },
      tooltip: {
        trigger: 'axis'
      },
      legend: {
        data: ['數據']
      },
      xAxis: {
        type: 'category',
        data: chartData.value.map(item => {
          return item.date
        })
      },
      yAxis: {
        type: 'value'
      },
      series: [{
        name: '數據',
        type: 'line',
        data: chartData.value.map(item => item.value)
      }]
    }

    chartInstance.setOption(option)
  }
}

// Watch
watch(panelType, () => {
  selectedLocation.value = ''
  chartOptions.value.field = ''
  records.value = []
  chartData.value = []
})

watch(chartData, async () => {
  if (chartData.value.length) {
    await initChart()
  }
})

// Lifecycle
onMounted(async () => {
  await search()

  // Add resize listener
  window.addEventListener('resize', () => {
    if (chartInstance) {
      chartInstance.resize()
    }
  })
})
</script>
