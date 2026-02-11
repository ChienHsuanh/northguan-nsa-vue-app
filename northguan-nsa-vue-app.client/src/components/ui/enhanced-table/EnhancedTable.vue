<template>
  <div class="w-full">
    <!-- 表格工具欄 -->
    <div class="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 bg-gray-50 border-b">
      <!-- 搜索和過濾 -->
      <div class="flex items-center gap-2">
        <Input
          v-model="searchKeyword"
          placeholder="搜索..."
          class="w-64"
          @input="handleSearch"
        />
        <slot name="filters" />
      </div>

      <!-- 操作按鈕 -->
      <div class="flex items-center gap-2">
        <slot name="actions" />
        <Button variant="outline" @click="refreshData">
          <RefreshCwIcon class="w-4 h-4 mr-2" />
          刷新
        </Button>
      </div>
    </div>

    <!-- 表格 -->
    <div class="relative">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead
              v-for="column in columns"
              :key="column.key"
              :class="[
                column.sortable ? 'cursor-pointer hover:bg-gray-50' : '',
                column.align === 'center' ? 'text-center' : column.align === 'right' ? 'text-right' : 'text-left'
              ]"
              @click="column.sortable ? handleSort(column.key) : null"
            >
              <div class="flex items-center gap-2">
                {{ column.title }}
                <template v-if="column.sortable">
                  <ChevronUpIcon
                    v-if="sortBy === column.key && sortOrder === 'asc'"
                    class="w-4 h-4"
                  />
                  <ChevronDownIcon
                    v-else-if="sortBy === column.key && sortOrder === 'desc'"
                    class="w-4 h-4"
                  />
                  <ChevronsUpDownIcon
                    v-else
                    class="w-4 h-4 text-gray-400"
                  />
                </template>
              </div>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          <!-- Skeleton loading rows -->
          <template v-if="loading">
            <TableRow v-for="i in Math.min(pageSize, 10)" :key="`skeleton-${i}`">
              <TableCell
                v-for="column in columns"
                :key="column.key"
                :class="[
                  column.align === 'center' ? 'text-center' : column.align === 'right' ? 'text-right' : 'text-left'
                ]"
              >
                <Skeleton 
                  :class="[
                    'h-4',
                    column.key === 'actions' ? 'w-16' : 
                    column.align === 'right' ? 'w-16' : 
                    column.key.includes('time') || column.key.includes('Time') ? 'w-32' :
                    'w-24'
                  ]" 
                />
              </TableCell>
            </TableRow>
          </template>
          <TableRow v-else-if="data.length === 0">
            <TableCell :colspan="columns.length" class="text-center py-8 text-gray-500">
              暫無數據
            </TableCell>
          </TableRow>
          <TableRow v-else v-for="(item, index) in data" :key="getRowKey(item, index)">
            <TableCell
              v-for="column in columns"
              :key="column.key"
              :class="[
                column.align === 'center' ? 'text-center' : column.align === 'right' ? 'text-right' : 'text-left'
              ]"
            >
              <slot
                :name="`cell-${column.key}`"
                :item="item"
                :value="getColumnValue(item, column.key)"
                :index="index"
              >
                {{ formatColumnValue(item, column) }}
              </slot>
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </div>

    <!-- 分頁 -->
    <EnhancedPagination
      v-if="pagination"
      v-model:current-page="currentPage"
      v-model:page-size="pageSize"
      :total-count="totalCount"
      :total-pages="totalPages"
      @page-change="handlePageChange"
      @page-size-change="handlePageSizeChange"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { debounce } from 'lodash'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Skeleton } from '@/components/ui/skeleton'
import { RefreshCwIcon, ChevronUpIcon, ChevronDownIcon, ChevronsUpDownIcon } from 'lucide-vue-next'
import EnhancedPagination from '../enhanced-pagination/EnhancedPagination.vue'

interface Column {
  key: string
  title: string
  sortable?: boolean
  align?: 'left' | 'center' | 'right'
  formatter?: (value: any, item: any) => string
  width?: string
}

interface Props {
  columns: Column[]
  data: any[]
  loading?: boolean
  pagination?: boolean
  currentPage?: number
  pageSize?: number
  totalCount?: number
  totalPages?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  searchable?: boolean
  rowKey?: string | ((item: any, index: number) => string | number)
}

interface Emits {
  (e: 'update:currentPage', page: number): void
  (e: 'update:pageSize', size: number): void
  (e: 'update:sortBy', sortBy: string): void
  (e: 'update:sortOrder', sortOrder: 'asc' | 'desc'): void
  (e: 'search', keyword: string): void
  (e: 'sort', sortBy: string, sortOrder: 'asc' | 'desc'): void
  (e: 'pageChange', page: number): void
  (e: 'pageSizeChange', size: number): void
  (e: 'refresh'): void
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  pagination: true,
  currentPage: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 1,
  sortBy: '',
  sortOrder: 'desc',
  searchable: true,
  rowKey: 'id'
})

const emit = defineEmits<Emits>()

const searchKeyword = ref('')
const currentPage = ref(props.currentPage)
const pageSize = ref(props.pageSize)
const sortBy = ref(props.sortBy)
const sortOrder = ref(props.sortOrder)

// 處理搜索
const handleSearch = debounce(() => {
  emit('search', searchKeyword.value)
}, 500)

// 處理排序
const handleSort = (columnKey: string) => {
  if (sortBy.value === columnKey) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = columnKey
    sortOrder.value = 'desc'
  }
  
  emit('update:sortBy', sortBy.value)
  emit('update:sortOrder', sortOrder.value)
  emit('sort', sortBy.value, sortOrder.value)
}

// 處理分頁變化
const handlePageChange = (page: number) => {
  currentPage.value = page
  emit('update:currentPage', page)
  emit('pageChange', page)
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  currentPage.value = 1 // 重置到第一頁
  emit('update:pageSize', size)
  emit('update:currentPage', 1)
  emit('pageSizeChange', size)
}

// 刷新數據
const refreshData = () => {
  emit('refresh')
}

// 獲取行鍵值
const getRowKey = (item: any, index: number): string | number => {
  if (typeof props.rowKey === 'function') {
    return props.rowKey(item, index)
  }
  return item[props.rowKey] || index
}

// 獲取列值
const getColumnValue = (item: any, key: string) => {
  return key.split('.').reduce((obj, k) => obj?.[k], item)
}

// 格式化列值
const formatColumnValue = (item: any, column: Column) => {
  const value = getColumnValue(item, column.key)
  if (column.formatter) {
    return column.formatter(value, item)
  }
  return value ?? '--'
}

// 同步外部 props 變化
watch(() => props.currentPage, (newPage) => {
  currentPage.value = newPage
})

watch(() => props.pageSize, (newSize) => {
  pageSize.value = newSize
})

watch(() => props.sortBy, (newSortBy) => {
  sortBy.value = newSortBy
})

watch(() => props.sortOrder, (newSortOrder) => {
  sortOrder.value = newSortOrder
})
</script>