<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <div class="max-w-6xl mx-auto">
        <h1 class="text-2xl font-bold text-gray-900 mb-6">分站資訊總覽</h1>

        <!-- Station Selection -->
        <Card class="mb-6">
          <CardContent class="p-4">
            <div class="flex items-center space-x-4">
              <Label class="text-sm font-medium">選擇分站:</Label>
              <Select v-model="selectedStationId" @change="loadStationData" class="w-64">
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                <SelectItem v-for="station in stations" :key="station.id" :value="station.id">
                  {{ station.name }}
                </SelectItem>
                </SelectContent>
              </Select>
              <Button @click="refreshData" :disabled="loading" variant="outline">
                <RefreshIcon :class="{ 'animate-spin': loading }" class="w-4 h-4 mr-2" />
                刷新
              </Button>
            </div>
          </CardContent>
        </Card>

        <div v-if="selectedStation">
          <!-- Station Overview -->
          <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
            <!-- Station Info Card -->
            <Card>
              <CardHeader>
                <CardTitle>基本資訊</CardTitle>
              </CardHeader>
              <CardContent class="space-y-4">
                <div>
                  <Label class="text-sm font-medium text-gray-600">分站名稱</Label>
                  <p class="text-lg font-semibold">{{ selectedStation.name }}</p>
                </div>
                <div>
                  <Label class="text-sm font-medium text-gray-600">分站代碼</Label>
                  <p class="text-sm">{{ selectedStation.code }}</p>
                </div>
                <div v-if="selectedStation.latitude && selectedStation.longitude">
                  <Label class="text-sm font-medium text-gray-600">位置座標</Label>
                  <p class="text-sm">{{ selectedStation.latitude }}, {{ selectedStation.longitude }}</p>
                </div>
              </CardContent>
            </Card>

            <!-- Device Stats -->
            <Card>
              <CardHeader>
                <CardTitle>設備統計</CardTitle>
              </CardHeader>
              <CardContent class="space-y-4">
                <div class="flex justify-between items-center">
                  <span class="text-sm text-gray-600">總設備數</span>
                  <span class="text-2xl font-bold">{{ deviceStats.total }}</span>
                </div>
                <div class="flex justify-between items-center">
                  <span class="text-sm text-gray-600">在線設備</span>
                  <span class="text-lg font-semibold text-green-600">{{ deviceStats.online }}</span>
                </div>
                <div class="flex justify-between items-center">
                  <span class="text-sm text-gray-600">離線設備</span>
                  <span class="text-lg font-semibold text-red-600">{{ deviceStats.offline }}</span>
                </div>
              </CardContent>
            </Card>

            <!-- Manager Info -->
            <Card>
              <CardHeader>
                <CardTitle>管理人員</CardTitle>
              </CardHeader>
              <CardContent>
                <div v-if="stationManagers.length > 0" class="space-y-2">
                  <div v-for="manager in stationManagers" :key="manager.id" class="flex items-center space-x-2">
                    <UserIcon class="w-4 h-4 text-gray-400" />
                    <span class="text-sm">{{ manager.name }}</span>
                    <span v-if="manager.isReadOnly" class="text-xs text-red-500">(唯讀)</span>
                  </div>
                </div>
                <p v-else class="text-sm text-gray-500">暫無管理人員</p>
              </CardContent>
            </Card>
          </div>

          <!-- Device List -->
          <Card class="mb-6">
            <CardHeader>
              <CardTitle>設備列表</CardTitle>
            </CardHeader>
            <CardContent class="p-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>設備名稱</TableHead>
                    <TableHead>類型</TableHead>
                    <TableHead>狀態</TableHead>
                    <TableHead>位置</TableHead>
                    <TableHead>最後更新</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableRow v-for="device in stationDevices" :key="device.id" class="hover:bg-gray-50">
                    <TableCell class="font-medium">{{ device.name }}</TableCell>
                    <TableCell>{{ getDeviceTypeText(device.type) }}</TableCell>
                    <TableCell>
                      <div class="flex items-center space-x-2">
                        <div :class="`w-3 h-3 rounded-full ${getStatusColor(device.status)}`"></div>
                        <span>{{ getStatusText(device.status) }}</span>
                      </div>
                    </TableCell>
                    <TableCell class="text-sm">
                      <span v-if="device.latitude && device.longitude">
                        {{ device.latitude.toFixed(4) }}, {{ device.longitude.toFixed(4) }}
                      </span>
                      <span v-else class="text-gray-400">未設定</span>
                    </TableCell>
                    <TableCell class="text-sm text-gray-600">
                      {{ device.lastUpdateTime || '-' }}
                    </TableCell>
                  </TableRow>
                  <TableRow v-if="stationDevices.length === 0">
                    <TableCell colspan="5" class="text-center py-8 text-gray-500">
                      此分站暫無設備
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>

        <div v-else class="text-center py-12">
          <BuildingIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <p class="text-gray-500">請選擇一個分站查看詳細資訊</p>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
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
  RefreshCw as RefreshIcon,
  User as UserIcon,
  Building as BuildingIcon
} from 'lucide-vue-next'

// Composables
const toast = useToast()

// State
const stations = ref<StationResponse[]>([])
const accounts = ref<UserResponse[]>([])
const devices = ref<DeviceListResponse[]>([])
const selectedStationId = ref('')
const loading = ref(false)

const deviceStats = reactive({
  total: 0,
  online: 0,
  offline: 0
})

// Computed
const selectedStation = computed(() => {
  return stations.value.find(s => s.id === Number(selectedStationId.value))
})

const stationManagers = computed(() => {
  // UserResponse 沒有 manageStationID 欄位，這個功能需要重新設計
  // 暫時返回空陣列
  return []
})

const stationDevices = computed(() => {
  if (!selectedStationId.value) return []
  return devices.value.filter(device => device.stationID === Number(selectedStationId.value))
})

// Methods
const loadStations = async () => {
  try {
    stations.value = await StationService.getStations()
  } catch (error) {
    console.error('載入分站失敗:', error)
    toast.error('載入失敗', '無法載入分站列表')
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

const loadStationData = () => {
  if (!selectedStationId.value) return

  const stationDeviceList = stationDevices.value
  deviceStats.total = stationDeviceList.length
  deviceStats.online = stationDeviceList.filter(d => d.status === 'online').length
  deviceStats.offline = stationDeviceList.filter(d => d.status === 'offline').length
}

const refreshData = async () => {
  try {
    loading.value = true
    await Promise.all([
      loadStations(),
      loadAccounts(),
      loadDevices()
    ])
    loadStationData()
  } catch (error) {
    console.error('刷新數據失敗:', error)
  } finally {
    loading.value = false
  }
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

onMounted(() => {
  refreshData()
})
</script>
