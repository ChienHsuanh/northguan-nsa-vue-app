<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <!-- Page Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 space-y-4 sm:space-y-0">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">人員管理</h1>
          <p class="text-sm text-gray-600 mt-1">管理系統用戶帳號與權限設定</p>
        </div>

        <div class="flex flex-col sm:flex-row items-stretch sm:items-center space-y-2 sm:space-y-0 sm:space-x-4">
          <Button @click="openCreateModal" class="bg-blue-600 hover:bg-blue-700">
            <PlusIcon class="w-4 h-4 mr-2" />
            新增用戶
          </Button>

          <div class="flex items-center space-x-2">
            <div class="flex">
              <Input v-model="searchKeyword" placeholder="搜尋用戶名稱、帳號或員工編號..." class="rounded-r-none w-64"
                @keyup.enter="handleSearch" @input="handleSearchInput" />
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
      </div>

      <!-- Filters -->
      <div class="flex flex-wrap items-center gap-4 mb-6 p-4 bg-white rounded-lg shadow-sm">
        <div class="flex items-center space-x-2">
          <Label class="text-sm font-medium text-gray-700">角色篩選:</Label>
          <Select v-model="roleFilter" @update:model-value="handleFilterChange">
            <SelectTrigger class="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">全部</SelectItem>
              <SelectItem value="Admin">北觀管理員</SelectItem>
              <SelectItem value="User">分站管理員</SelectItem>
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
              <SelectItem value="normal">一般</SelectItem>
              <SelectItem value="readonly">唯讀</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div class="flex items-center space-x-2">
          <Label class="text-sm font-medium text-gray-700">分站篩選:</Label>
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

        <div class="ml-auto text-sm text-gray-600">
          共 {{ totalCount }} 位用戶
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>

      <!-- Accounts Table -->
      <Card v-else class="shadow-sm">
        <CardContent class="p-0">
          <div class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow class="bg-gray-50">
                  <TableHead class="w-20">序號</TableHead>
                  <TableHead class="min-w-32">
                    <Button variant="ghost" size="sm" @click="toggleSort('name')" class="p-0 h-auto font-medium">
                      名稱
                      <ChevronUpIcon v-if="sortField === 'name' && sortOrder === 'asc'" class="w-4 h-4 ml-1" />
                      <ChevronDownIcon v-else-if="sortField === 'name' && sortOrder === 'desc'" class="w-4 h-4 ml-1" />
                      <ChevronsUpDownIcon v-else class="w-4 h-4 ml-1 opacity-50" />
                    </Button>
                  </TableHead>
                  <TableHead class="min-w-48">帳號</TableHead>
                  <TableHead class="min-w-32">員工編號</TableHead>
                  <TableHead class="min-w-32">角色</TableHead>
                  <TableHead class="min-w-40">分站</TableHead>
                  <TableHead class="min-w-32">
                    <Button variant="ghost" size="sm" @click="toggleSort('createdAt')" class="p-0 h-auto font-medium">
                      建立時間
                      <ChevronUpIcon v-if="sortField === 'createdAt' && sortOrder === 'asc'" class="w-4 h-4 ml-1" />
                      <ChevronDownIcon v-else-if="sortField === 'createdAt' && sortOrder === 'desc'"
                        class="w-4 h-4 ml-1" />
                      <ChevronsUpDownIcon v-else class="w-4 h-4 ml-1 opacity-50" />
                    </Button>
                  </TableHead>
                  <TableHead class="text-right w-24">操作</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="(account, index) in paginatedAccounts" :key="account.id"
                  class="hover:bg-gray-50 transition-colors">
                  <TableCell class="font-medium text-gray-900">
                    #{{ (currentPage - 1) * pageSize + index + 1 }}
                  </TableCell>
                  <TableCell>
                    <div class="flex items-center space-x-3">
                      <div class="flex-shrink-0">
                        <div class="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                          <span class="text-sm font-medium text-blue-600">
                            {{ account.name?.charAt(0)?.toUpperCase() || 'U' }}
                          </span>
                        </div>
                      </div>
                      <div>
                        <div class="font-medium text-gray-900">{{ account.name }}</div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div class="text-sm text-gray-900">{{ account.username }}</div>
                    <div class="text-xs text-gray-500">{{ account.username?.includes('@') ? '電子郵件' : '用戶名' }}</div>
                  </TableCell>
                  <TableCell>
                    <span class="text-sm text-gray-900">{{ account.employeeId || '-' }}</span>
                  </TableCell>
                  <TableCell>
                    <div class="flex flex-col space-y-1">
                      <span v-if="account.role === 'Admin'"
                        class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 w-fit">
                        <ShieldCheckIcon class="w-3 h-3 mr-1" />
                        北觀管理員
                      </span>
                      <span v-else-if="account.role === 'User'"
                        class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800 w-fit">
                        <UsersIcon class="w-3 h-3 mr-1" />
                        分站管理員
                      </span>
                      <span v-if="account.isReadOnly"
                        class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-orange-100 text-orange-800 w-fit">
                        <EyeIcon class="w-3 h-3 mr-1" />
                        唯讀權限
                      </span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div v-if="account.stationIds && account.stationIds.length > 0" class="text-sm">
                      <div v-if="account.stationIds.length <= 2">
                        {{ getStationNames(account.stationIds) }}
                      </div>
                      <div v-else class="space-y-1">
                        <div>{{ getStationNames(account.stationIds.slice(0, 2)) }}</div>
                        <span class="text-xs text-gray-500">等 {{ account.stationIds.length }} 個分站</span>
                      </div>
                    </div>
                    <span v-else class="text-gray-400 text-sm">全部分站</span>
                  </TableCell>
                  <TableCell>
                    <div class="text-xs text-gray-500">
                      {{ account.createdAt }}
                    </div>
                  </TableCell>
                  <TableCell class="text-right">
                    <div class="flex justify-end space-x-1">
                      <Button variant="ghost" size="sm" @click="openUpdateModal(account.id)" title="編輯用戶"
                        class="hover:bg-blue-50 hover:text-blue-600">
                        <EditIcon class="w-4 h-4" />
                      </Button>
                      <Button variant="ghost" size="sm" @click="confirmDeleteAccount(account)" title="刪除用戶"
                        class="hover:bg-red-50 hover:text-red-600">
                        <TrashIcon class="w-4 h-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
                <TableRow v-if="filteredAccounts.length === 0">
                  <TableCell colspan="8" class="text-center py-12">
                    <div class="flex flex-col items-center space-y-3">
                      <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
                        <UsersIcon class="w-8 h-8 text-gray-400" />
                      </div>
                      <div class="text-gray-500">
                        {{ hasActiveFilters ? '沒有找到符合條件的用戶' : '尚未有任何用戶' }}
                      </div>
                      <Button v-if="hasActiveFilters" @click="clearAllFilters" variant="outline" size="sm">
                        清除所有篩選條件
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      <!-- Pagination -->
      <div v-if="totalPages > 1"
        class="flex flex-col sm:flex-row justify-between items-center mt-6 space-y-4 sm:space-y-0 bg-white p-4 rounded-lg shadow-sm">
        <div class="text-sm text-gray-600">
          顯示第 {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, totalCount) }} 項，共 {{
            totalCount }} 項
        </div>
        <div class="flex flex-col sm:flex-row items-center space-y-2 sm:space-y-0 sm:space-x-4">
          <div class="flex items-center space-x-2">
            <span class="text-sm text-gray-600">第</span>
            <Input v-model.number="currentPage" type="number" class="w-16 text-center" :min="1" :max="totalPages"
              @change="changePage(currentPage)" />
            <span class="text-sm text-gray-600">頁，共 {{ totalPages }} 頁</span>
          </div>
          <div class="flex items-center space-x-1">
            <Button variant="outline" size="sm" @click="changePage(1)" :disabled="currentPage <= 1" title="第一頁">
              <ChevronsLeftIcon class="w-4 h-4" />
            </Button>
            <Button variant="outline" size="sm" @click="changePage(currentPage - 1)" :disabled="currentPage <= 1"
              title="上一頁">
              <ChevronLeftIcon class="w-4 h-4" />
            </Button>

            <!-- 頁碼按鈕 -->
            <div class="flex space-x-1">
              <template v-for="page in visiblePages" :key="page">
                <Button v-if="page !== '...'" variant="outline" size="sm" @click="changePage(page as number)" :class="{
                  'bg-blue-600 text-white border-blue-600': page === currentPage,
                  'hover:bg-blue-50': page !== currentPage
                }">
                  {{ page }}
                </Button>
                <span v-else class="px-2 py-1 text-gray-400">...</span>
              </template>
            </div>

            <Button variant="outline" size="sm" @click="changePage(currentPage + 1)"
              :disabled="currentPage >= totalPages" title="下一頁">
              <ChevronRightIcon class="w-4 h-4" />
            </Button>
            <Button variant="outline" size="sm" @click="changePage(totalPages)" :disabled="currentPage >= totalPages"
              title="最後一頁">
              <ChevronsRightIcon class="w-4 h-4" />
            </Button>
          </div>
        </div>
      </div>
    </main>

    <!-- Account Modals -->
    <CreateAccountModal :is-open="showCreateModal" :stations="stations" @close="showCreateModal = false"
      @success="loadAccounts" />

    <UpdateAccountModal :is-open="showUpdateModal" :stations="stations" :account="currentAccount"
      @close="showUpdateModal = false" @success="loadAccounts" />

    <DeleteAccountModal :is-open="showDeleteModal" :account="accountToDelete" @close="showDeleteModal = false"
      @success="loadAccounts" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import Label from '@/components/ui/label/Label.vue'
import {
  AccountService,
  StationService,
  UserResponse,
  StationResponse
} from '@/services'
import {
  Search as SearchIcon,
  Edit as EditIcon,
  Trash as TrashIcon,
  ChevronLeft as ChevronLeftIcon,
  ChevronRight as ChevronRightIcon,
  ChevronsLeft as ChevronsLeftIcon,
  ChevronsRight as ChevronsRightIcon,
  ChevronUp as ChevronUpIcon,
  ChevronDown as ChevronDownIcon,
  ChevronsUpDown as ChevronsUpDownIcon,
  Plus as PlusIcon,
  Users as UsersIcon,
  ShieldCheck as ShieldCheckIcon,
  Eye as EyeIcon
} from 'lucide-vue-next'
// Modal components
import CreateAccountModal from '@/components/account/CreateAccountModal.vue'
import UpdateAccountModal from '@/components/account/UpdateAccountModal.vue'
import DeleteAccountModal from '@/components/account/DeleteAccountModal.vue'

// State
const accounts = ref<UserResponse[]>([])
const stations = ref<StationResponse[]>([])
const searchKeyword = ref('')
const roleFilter = ref('all')
const statusFilter = ref('all')
const stationFilter = ref('all')
const currentPage = ref(1)
const totalPages = ref(1)
const totalCount = ref(0)
const pageSize = 10
const loading = ref(false)
const sortField = ref('')
const sortOrder = ref<'asc' | 'desc'>('asc')
const searchTimeout = ref<number | null>(null)

const showCreateModal = ref(false)
const showUpdateModal = ref(false)
const showDeleteModal = ref(false)
const currentAccount = ref<UserResponse | null>(null)
const accountToDelete = ref<UserResponse | null>(null)

// Computed
const stationMap = computed(() => {
  return stations.value.reduce((map, station) => {
    map[station.id || 0] = station
    return map
  }, {} as Record<number, StationResponse>)
})

const filteredAccounts = computed(() => {
  let filtered = [...accounts.value]

  // 搜索篩選
  if (searchKeyword.value) {
    const keyword = searchKeyword.value.toLowerCase()
    filtered = filtered.filter(account =>
      account.name?.toLowerCase().includes(keyword) ||
      account.username?.toLowerCase().includes(keyword) ||
      account.employeeId?.toLowerCase().includes(keyword)
    )
  }

  // 角色篩選
  if (roleFilter.value !== 'all') {
    filtered = filtered.filter(account => account.role === roleFilter.value)
  }

  // 狀態篩選
  if (statusFilter.value !== 'all') {
    if (statusFilter.value === 'readonly') {
      filtered = filtered.filter(account => account.isReadOnly)
    } else if (statusFilter.value === 'normal') {
      filtered = filtered.filter(account => !account.isReadOnly)
    }
  }

  // 分站篩選
  if (stationFilter.value !== 'all') {
    const stationId = parseInt(stationFilter.value)
    filtered = filtered.filter(account =>
      account.stationIds?.includes(stationId) ||
      (account.role === 'Admin' && !account.stationIds?.length)
    )
  }

  // 排序
  if (sortField.value) {
    filtered.sort((a, b) => {
      let aValue: any = ''
      let bValue: any = ''

      switch (sortField.value) {
        case 'name':
          aValue = a.name || ''
          bValue = b.name || ''
          break
        case 'createdAt':
          aValue = new Date(a.createdAt || 0)
          bValue = new Date(b.createdAt || 0)
          break
        default:
          return 0
      }

      if (aValue < bValue) return sortOrder.value === 'asc' ? -1 : 1
      if (aValue > bValue) return sortOrder.value === 'asc' ? 1 : -1
      return 0
    })
  }

  return filtered
})

const paginatedAccounts = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  const end = start + pageSize
  return filteredAccounts.value.slice(start, end)
})

const hasActiveFilters = computed(() => {
  return !!(searchKeyword.value || roleFilter.value !== 'all' || statusFilter.value !== 'all' || stationFilter.value !== 'all')
})

const visiblePages = computed(() => {
  const pages: (number | string)[] = []
  const total = totalPages.value
  const current = currentPage.value

  if (total <= 7) {
    // 如果總頁數小於等於7，顯示所有頁碼
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    // 總是顯示第一頁
    pages.push(1)

    if (current <= 4) {
      // 當前頁在前面
      for (let i = 2; i <= 5; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(total)
    } else if (current >= total - 3) {
      // 當前頁在後面
      pages.push('...')
      for (let i = total - 4; i <= total; i++) {
        pages.push(i)
      }
    } else {
      // 當前頁在中間
      pages.push('...')
      for (let i = current - 1; i <= current + 1; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(total)
    }
  }

  return pages
})

// 重新計算總頁數
watch(filteredAccounts, (newFiltered) => {
  totalCount.value = newFiltered.length
  totalPages.value = Math.ceil(newFiltered.length / pageSize)

  // 如果當前頁超出範圍，回到第一頁
  if (currentPage.value > totalPages.value && totalPages.value > 0) {
    currentPage.value = 1
  }
})

// Watchers are now handled in individual modal components

// Methods
const loadAccounts = async () => {
  try {
    loading.value = true
    // 載入所有帳戶，使用客戶端篩選和分頁
    const response = await AccountService.getAccountList(1, 1000) // 載入大量數據
    accounts.value = response
  } catch (error) {
    console.error('載入帳戶失敗:', error)
    const apiError: ApiErrorResponse = handleApiError(error)
    showErrorMessage(`載入帳戶失敗: ${apiError.message}`)
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
    showErrorMessage(`載入分站失敗: ${apiError.message}`)
  }
}

const handleSearch = () => {
  currentPage.value = 1
  // 由於現在使用客戶端篩選，不需要重新載入
}

const handleSearchInput = () => {
  // 防抖搜索
  if (searchTimeout.value) {
    clearTimeout(searchTimeout.value)
  }
  searchTimeout.value = setTimeout(() => {
    currentPage.value = 1
  }, 300)
}

const clearSearch = () => {
  searchKeyword.value = ''
  currentPage.value = 1
}

const handleFilterChange = () => {
  currentPage.value = 1
}

const clearAllFilters = () => {
  searchKeyword.value = ''
  roleFilter.value = 'all'
  statusFilter.value = 'all'
  stationFilter.value = 'all'
  currentPage.value = 1
}

const toggleSort = (field: string) => {
  if (sortField.value === field) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortField.value = field
    sortOrder.value = 'asc'
  }
}

const changePage = (page: number) => {
  if (page >= 1 && page <= totalPages.value && page !== currentPage.value) {
    currentPage.value = page
    loadAccounts()
  }
}

const getStationNames = (stationIds: number[]) => {
  if (!stationIds || stationIds.length === 0) return '-'
  return stationIds.map(id => stationMap.value[id]?.name).join(', ')
}

const openCreateModal = () => {
  showCreateModal.value = true
}

const openUpdateModal = (accountId: string | undefined) => {
  if (!accountId) return
  const account = accounts.value.find(a => a.id === accountId)
  if (account) {
    currentAccount.value = account
    showUpdateModal.value = true
  }
}

const confirmDeleteAccount = (account: UserResponse) => {
  accountToDelete.value = account
  showDeleteModal.value = true
}


onMounted(() => {
  loadAccounts()
  loadStations()
})
</script>
