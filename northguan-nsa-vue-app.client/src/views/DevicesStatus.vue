<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- Page Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 space-y-4 sm:space-y-0">
        <h1 class="text-2xl font-bold text-gray-900">設備狀態監控</h1>

        <div class="flex items-center space-x-4">
          <Button variant="outline" size="sm" @click="refreshData" :disabled="refreshing">
            <RefreshIcon :class="{ 'animate-spin': refreshing }" class="w-4 h-4 mr-2" />
            {{ refreshing ? '刷新中...' : '刷新' }}
          </Button>

          <div class="flex items-center space-x-2 text-sm text-gray-500">
            <div class="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
            <span>即時監控</span>
          </div>
        </div>
      </div>

      <!-- Status Overview Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
        <Card>
          <CardContent>
            <div class="flex items-center">
              <div class="p-2 bg-green-100 rounded-lg">
                <CheckCircleIcon class="w-6 h-6 text-green-600" />
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-600">在線設備</p>
                <p class="text-2xl font-bold text-green-600">{{ onlineDevices }}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <div class="flex items-center">
              <div class="p-2 bg-red-100 rounded-lg">
                <XCircleIcon class="w-6 h-6 text-red-600" />
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-600">離線設備</p>
                <p class="text-2xl font-bold text-red-600">{{ offlineDevices }}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <div class="flex items-center">
              <div class="p-2 bg-yellow-100 rounded-lg">
                <AlertTriangleIcon class="w-6 h-6 text-yellow-600" />
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-600">維護中</p>
                <p class="text-2xl font-bold text-yellow-600">{{ maintenanceDevices }}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <div class="flex items-center">
              <div class="p-2 bg-blue-100 rounded-lg">
                <ServerIcon class="w-6 h-6 text-blue-600" />
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-600">總設備數</p>
                <p class="text-2xl font-bold text-gray-900">{{ totalDevices }}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <!-- Device Status Chart -->
      <Card class="mb-6">
        <CardHeader>
          <CardTitle>設備狀態趨勢</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="h-80 relative">
            <canvas id="device-status-chart" ref="statusChartRef" class="w-full h-full"></canvas>
            <div v-if="!statusHistory.length"
              class="absolute inset-0 flex items-center justify-center bg-gray-50 rounded">
              <p class="text-gray-500">暫無歷史數據</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- Filter and Search -->
      <Card class="mb-6">
        <CardContent class="p-4">
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="flex-1">
              <Input v-model="searchKeyword" placeholder="搜尋設備名稱..." class="w-full" @input="filterDevices" />
            </div>
            <Select v-model="statusFilter" @change="filterDevices" class="w-full sm:w-48">
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">全部狀態</SelectItem>
                <SelectItem value="online">在線</SelectItem>
                <SelectItem value="offline">離線</SelectItem>
                <SelectItem value="maintenance">維護中</SelectItem>
              </SelectContent>
            </Select>
            <Select v-model="stationFilter" @change="filterDevices" class="w-full sm:w-48">
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">全部分站</SelectItem>
                <SelectItem v-for="station in stations" :key="station.id" :value="station.id">
                  {{ station.name }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      <!-- Devices Status Table -->
      <Card>
        <CardContent class="p-0">
          <div v-if="loading" class="flex justify-center items-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
          </div>
          <Table v-else>
            <TableHeader>
              <TableRow>
                <TableHead>設備名稱</TableHead>
                <TableHead>類型</TableHead>
                <TableHead>所屬分站</TableHead>
                <TableHead>狀態</TableHead>
                <TableHead>位置</TableHead>
                <TableHead>最後心跳</TableHead>
                <TableHead>運行時間</TableHead>
                <TableHead class="text-center">操作</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="device in filteredDevices" :key="device.id" class="hover:bg-gray-50">
                <TableCell class="font-medium">{{ device.name }}</TableCell>
                <TableCell>
                  <span
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                    {{ getDeviceTypeText(device.type) }}
                  </span>
                </TableCell>
                <TableCell>
                  <span
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                    {{ getStationName(device.stationID || 0) }}
                  </span>
                </TableCell>
                <TableCell>
                  <div class="flex items-center space-x-2">
                    <div :class="`w-3 h-3 rounded-full ${getStatusColor(device.status)}`"></div>
                    <span :class="getStatusTextColor(device.status)">
                      {{ getStatusText(device.status) }}
                    </span>
                  </div>
                </TableCell>
                <TableCell class="text-sm text-gray-600">
                  <span v-if="device.latitude && device.longitude">
                    {{ device.latitude.toFixed(4) }}, {{ device.longitude.toFixed(4) }}
                  </span>
                  <span v-else class="text-gray-400">未設定</span>
                </TableCell>
                <TableCell class="text-sm text-gray-600">
                  {{ device.lastUpdateTime ?? '-' }}
                </TableCell>
                <TableCell class="text-sm text-gray-600">
                  {{ getUptime(device.lastUpdateTime) }}
                </TableCell>
                <TableCell class="text-center">
                  <div class="flex justify-center space-x-1">
                    <Button variant="ghost" size="sm" @click="viewDeviceDetails(device)" title="查看詳情">
                      <EyeIcon class="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="sm" @click="restartDevice(device)" title="重啟設備"
                      :disabled="device.status === 'offline'">
                      <RotateCcwIcon class="w-4 h-4" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
              <TableRow v-if="filteredDevices.length === 0">
                <TableCell colspan="8" class="text-center py-8 text-gray-500">
                  {{ searchKeyword || statusFilter !== 'all' || stationFilter !== 'all' ? '沒有找到符合條件的設備' : '暫無設備數據' }}
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </main>

    <!-- Device Details Modal -->
    <Dialog v-model:open="showDetailsModal">
      <DialogContent class="sm:max-w-[425px]" v-if="selectedDevice">
        <DialogHeader>
          <DialogTitle class="text-center">設備詳情</DialogTitle>
          <DialogDescription class="text-center">{{ selectedDevice.name }}</DialogDescription>
        </DialogHeader>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <Label class="text-sm font-medium text-gray-600">設備名稱</Label>
            <p class="mt-1 text-sm text-gray-900">{{ selectedDevice.name }}</p>
          </div>

          <div>
            <Label class="text-sm font-medium text-gray-600">設備類型</Label>
            <p class="mt-1 text-sm text-gray-900">{{ getDeviceTypeText(selectedDevice.type) }}</p>
          </div>

          <div>
            <Label class="text-sm font-medium text-gray-600">所屬分站</Label>
            <p class="mt-1 text-sm text-gray-900">{{ getStationName(selectedDevice.stationId) }}</p>
          </div>

          <div>
            <Label class="text-sm font-medium text-gray-600">當前狀態</Label>
            <div class="mt-1 flex items-center space-x-2">
              <div :class="`w-3 h-3 rounded-full ${getStatusColor(selectedDevice.status)}`"></div>
              <span :class="getStatusTextColor(selectedDevice.status)">
                {{ getStatusText(selectedDevice.status) }}
              </span>
            </div>
          </div>

          <div>
            <Label class="text-sm font-medium text-gray-600">位置座標</Label>
            <p class="mt-1 text-sm text-gray-900">
              <span v-if="selectedDevice.latitude && selectedDevice.longitude">
                {{ selectedDevice.latitude.toFixed(6) }}, {{ selectedDevice.longitude.toFixed(6) }}
              </span>
              <span v-else class="text-gray-400">未設定</span>
            </p>
          </div>

          <div>
            <Label class="text-sm font-medium text-gray-600">最後更新</Label>
            <p class="mt-1 text-sm text-gray-900">{{ selectedDevice.lastUpdateTime ?
              selectedDevice.lastUpdateTime : '-' }}</p>
          </div>
        </div>

        <div class="flex justify-end space-x-3 pt-6 border-t">
          <Button variant="outline" @click="showDetailsModal = false">
            關閉
          </Button>
          <Button @click="restartDevice(selectedDevice)" :disabled="selectedDevice.status === 'offline'">
            重啟設備
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import {
  DeviceService,
  DeviceStatusResponse,
  DeviceStatusLogResponse,
  StationService,
  StationResponse
} from '@/services'
import { ChartManager } from '@/utils/ChartManager'
import { ColorUtil } from '@/utils/ColorUtil'
import { useToast } from '@/composables/useToast'
import AppHeader from '@/components/layout/AppHeader.vue'
import Card from '@/components/ui/card/Card.vue'
import CardHeader from '@/components/ui/card/CardHeader.vue'
import CardContent from '@/components/ui/card/CardContent.vue'
import CardTitle from '@/components/ui/card/CardTitle.vue'
import Table from '@/components/ui/table/Table.vue'
import TableHeader from '@/components/ui/table/TableHeader.vue'
import TableBody from '@/components/ui/table/TableBody.vue'
import TableRow from '@/components/ui/table/TableRow.vue'
import TableHead from '@/components/ui/table/TableHead.vue'
import TableCell from '@/components/ui/table/TableCell.vue'
import Button from '@/components/ui/button/Button.vue'
import Input from '@/components/ui/input/Input.vue'
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import Label from '@/components/ui/label/Label.vue'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter
} from '@/components/ui/dialog'
import {
  RefreshCw as RefreshIcon,
  CheckCircle as CheckCircleIcon,
  XCircle as XCircleIcon,
  AlertTriangle as AlertTriangleIcon,
  Server as ServerIcon,
  Eye as EyeIcon,
  RotateCcw as RotateCcwIcon
} from 'lucide-vue-next'

// Composables
const toast = useToast()

// State
const devices = ref<DeviceStatusResponse[]>([])
const stations = ref<StationResponse[]>([])
const statusHistory = ref<DeviceStatusLogResponse[]>([])
const statusChartRef = ref<HTMLCanvasElement>()
const loading = ref(false)
const refreshing = ref(false)

const searchKeyword = ref('')
const statusFilter = ref('all')
const stationFilter = ref('all')

const showDetailsModal = ref(false)
const selectedDevice = ref<DeviceStatusResponse | null>(null)

let statusChart: ChartManager | null = null
let refreshInterval: number | null = null

// Computed
const filteredDevices = computed(() => {
  let filtered = devices.value

  if (searchKeyword.value) {
    filtered = filtered.filter(device =>
      device.name.toLowerCase().includes(searchKeyword.value.toLowerCase())
    )
  }

  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(device => device.status === statusFilter.value)
  }

  if (stationFilter.value !== 'all') {
    filtered = filtered.filter(device => device.id === Number(stationFilter.value))
  }

  return filtered
})

const onlineDevices = computed(() => {
  return devices.value.filter(d => d.status === 'online').length
})

const offlineDevices = computed(() => {
  return devices.value.filter(d => d.status === 'offline').length
})

const maintenanceDevices = computed(() => {
  return devices.value.filter(d => d.status === 'maintenance').length
})

const totalDevices = computed(() => {
  return devices.value.length
})

const stationMap = computed(() => {
  return stations.value.reduce((map, station) => {
    map[station.id || 0] = station
    return map
  }, {} as Record<number, StationResponse>)
})

// Methods
const loadDevices = async () => {
  try {
    loading.value = true
    devices.value = await DeviceService.getDevicesStatus()
  } catch (error) {
    console.error('載入設備失敗:', error)
    toast.error('載入失敗', '無法載入設備狀態')
  } finally {
    loading.value = false
  }
}

const loadStations = async () => {
  try {
    stations.value = await StationService.getStations()
  } catch (error) {
    console.error('載入分站失敗:', error)
  }
}

const loadStatusHistory = async () => {
  try {
    statusHistory.value = await DeviceService.getDeviceStatusLogs()
    await updateStatusChart()
  } catch (error) {
    console.error('載入狀態歷史失敗:', error)
    toast.error('載入失敗', '無法載入狀態歷史')
  }
}

const updateStatusChart = async () => {
  if (!statusHistory.value.length) return

  const labels = statusHistory.value.map(item => item.time)
  const datasets = [
    {
      label: '在線',
      data: statusHistory.value.filter(item => item.status === 'online'),
      borderColor: ColorUtil.pickColorByOrder(0),
      backgroundColor: 'rgba(0, 0, 0, 0)'
    },
    {
      label: '離線',
      data: statusHistory.value.filter(item => item.status === 'offline'),
      borderColor: ColorUtil.pickColorByOrder(1),
      backgroundColor: 'rgba(0, 0, 0, 0)'
    },
    {
      label: '維護中',
      data: statusHistory.value.map(item => item.status === 'maintenance'),
      borderColor: ColorUtil.pickColorByOrder(2),
      backgroundColor: 'rgba(0, 0, 0, 0)'
    }
  ]

  if (statusChart) {
    statusChart.update(labels, datasets)
  } else {
    await nextTick()
    statusChart = new ChartManager(
      'device-status-chart',
      'line',
      labels,
      datasets,
      {
        showLegend: true
      }
    )
  }
}

const refreshData = async () => {
  try {
    refreshing.value = true
    await Promise.all([
      loadDevices(),
      loadStatusHistory()
    ])
  } catch (error) {
    console.error('刷新數據失敗:', error)
  } finally {
    refreshing.value = false
  }
}

const filterDevices = () => {
  // 觸發計算屬性重新計算
}

const getStationName = (stationId: number) => {
  return stationMap.value[stationId]?.name || '未知分站'
}

const getDeviceTypeText = (type: string) => {
  const typeMap: Record<string, string> = {
    camera: '攝影機',
    sensor: '感測器',
    counter: '計數器',
    gate: '閘門'
  }
  return typeMap[type] || type
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

const getUptime = (lastUpdate: string | undefined) => {
  if (!lastUpdate) return '-'

  const now = new Date()
  const updateTime = new Date(lastUpdate)
  const diffMs = now.getTime() - updateTime.getTime()
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
  const diffDays = Math.floor(diffHours / 24)

  if (diffDays > 0) {
    return `${diffDays}天 ${diffHours % 24}小時`
  } else {
    return `${diffHours}小時`
  }
}

const viewDeviceDetails = (device: DeviceStatusResponse) => {
  selectedDevice.value = device
  showDetailsModal.value = true
}

const restartDevice = async (device: DeviceStatusResponse) => {
  try {
    // 這裡應該調用重啟設備的 API
    toast.success('重啟成功', `設備 ${device.name} 重啟指令已發送`)
  } catch (error) {
    console.error('重啟設備失敗:', error)
    toast.error('重啟失敗', '無法重啟設備')
  }
}

// 設置自動刷新
const setupAutoRefresh = () => {
  refreshInterval = setInterval(() => {
    refreshData()
  }, 30000) // 每30秒刷新一次
}

const clearAutoRefresh = () => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
    refreshInterval = null
  }
}

onMounted(async () => {
  await Promise.all([
    loadDevices(),
    loadStations(),
    loadStatusHistory()
  ])
  setupAutoRefresh()
})

onUnmounted(() => {
  clearAutoRefresh()
  if (statusChart) {
    statusChart.destroy()
  }
})
</script>
