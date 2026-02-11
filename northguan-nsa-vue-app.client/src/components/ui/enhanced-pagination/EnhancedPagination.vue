<template>
  <div class="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 bg-white border-t">
    <!-- 分頁大小選擇器 -->
    <div class="flex items-center gap-2">
      <span class="text-sm text-gray-600">每頁顯示</span>
      <Select v-model="currentPageSize">
        <SelectTrigger class="w-20">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem v-for="size in pageSizeOptions" :key="size" :value="size.toString()">
            {{ size }}
          </SelectItem>
        </SelectContent>
      </Select>
      <span class="text-sm text-gray-600">筆</span>
    </div>

    <!-- 分頁信息 -->
    <div class="text-sm text-gray-600">
      顯示第 {{ startItem }} - {{ endItem }} 筆，共 {{ totalCount }} 筆
    </div>

    <!-- 分頁控制 -->
    <div class="flex items-center gap-2">
      <Button
        variant="outline"
        size="sm"
        :disabled="currentPage <= 1"
        @click="goToPage(1)"
      >
        首頁
      </Button>
      
      <Button
        variant="outline"
        size="sm"
        :disabled="currentPage <= 1"
        @click="goToPage(currentPage - 1)"
      >
        上一頁
      </Button>

      <!-- 頁碼按鈕 -->
      <div class="flex items-center gap-1">
        <template v-for="page in visiblePages" :key="page">
          <Button
            v-if="page !== '...'"
            :variant="page === currentPage ? 'default' : 'outline'"
            size="sm"
            class="w-10"
            @click="goToPage(page as number)"
          >
            {{ page }}
          </Button>
          <span v-else class="px-2 text-gray-400">...</span>
        </template>
      </div>

      <Button
        variant="outline"
        size="sm"
        :disabled="currentPage >= totalPages"
        @click="goToPage(currentPage + 1)"
      >
        下一頁
      </Button>
      
      <Button
        variant="outline"
        size="sm"
        :disabled="currentPage >= totalPages"
        @click="goToPage(totalPages)"
      >
        末頁
      </Button>
    </div>

    <!-- 跳轉到指定頁 -->
    <div class="flex items-center gap-2">
      <span class="text-sm text-gray-600">跳至</span>
      <Input
        v-model="jumpToPageInput"
        type="number"
        :min="1"
        :max="totalPages"
        class="w-16 text-center"
        @keyup.enter="jumpToPage"
      />
      <span class="text-sm text-gray-600">頁</span>
      <Button size="sm" variant="outline" @click="jumpToPage">
        跳轉
      </Button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'

interface Props {
  currentPage: number
  pageSize: number
  totalCount: number
  totalPages: number
  pageSizeOptions?: number[]
}

interface Emits {
  (e: 'update:currentPage', page: number): void
  (e: 'update:pageSize', size: number): void
  (e: 'pageChange', page: number): void
  (e: 'pageSizeChange', size: number): void
}

const props = withDefaults(defineProps<Props>(), {
  pageSizeOptions: () => [10, 20, 50, 100]
})

const emit = defineEmits<Emits>()

const jumpToPageInput = ref<string>('')
const currentPageSize = ref(props.pageSize.toString())

// 計算顯示的項目範圍
const startItem = computed(() => {
  return props.totalCount === 0 ? 0 : (props.currentPage - 1) * props.pageSize + 1
})

const endItem = computed(() => {
  return Math.min(props.currentPage * props.pageSize, props.totalCount)
})

// 計算可見的頁碼
const visiblePages = computed(() => {
  const pages: (number | string)[] = []
  const total = props.totalPages
  const current = props.currentPage
  
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

// 跳轉到指定頁
const goToPage = (page: number) => {
  if (page >= 1 && page <= props.totalPages && page !== props.currentPage) {
    emit('update:currentPage', page)
    emit('pageChange', page)
  }
}

// 跳轉到輸入的頁碼
const jumpToPage = () => {
  const page = parseInt(jumpToPageInput.value)
  if (!isNaN(page)) {
    goToPage(page)
    jumpToPageInput.value = ''
  }
}

// 監聽頁面大小變化
watch(currentPageSize, (newSize) => {
  const size = parseInt(newSize)
  if (size !== props.pageSize) {
    emit('update:pageSize', size)
    emit('pageSizeChange', size)
  }
})

// 同步外部的 pageSize 變化
watch(() => props.pageSize, (newSize) => {
  currentPageSize.value = newSize.toString()
})
</script>