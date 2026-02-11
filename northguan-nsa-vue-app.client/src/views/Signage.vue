<template>
  <div
    class="min-h-screen bg-gradient-to-br from-sky-100 via-blue-100 to-slate-200 font-sans text-gray-800 flex flex-col">
    <!-- Header -->
    <DashboardHeader :title="currentTitle" :current-signage-type="currentSignageType" @signage-tab-changed="handleTabChange" />

    <!-- Main Content -->
    <main class="flex-1 relative">
      <iframe :src="currentIframeUrl"
        class="absolute top-0 left-0 w-full border-none iframe-with-hidden-nav" 
        title="Signage Content"></iframe>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import DashboardHeader from '@/components/layout/DashboardHeader.vue'

const route = useRoute()
const router = useRouter()

// 當前活躍的 tab 類型
const currentSignageType = ref('water')
const currentIframeUrl = ref('https://signage.beiguan-platform.com/Index#/water')

// 根據路由初始化當前類型
const initializeFromRoute = () => {
  switch (route.name) {
    case 'SignageWater':
      currentSignageType.value = 'water'
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Index#/water'
      break
    case 'SignageBridge':
      currentSignageType.value = 'bridge'
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Index#/bridge'
      break
    case 'SignageDigital':
      currentSignageType.value = 'digital'
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Signage'
      break
    case 'SignageDevice':
      currentSignageType.value = 'device'
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Index#/device'
      break
    default:
      currentSignageType.value = 'water'
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Index#/water'
  }
}

// 處理 tab 切換（不重新加載頁面）
const handleTabChange = (type: string) => {
  currentSignageType.value = type
  
  switch (type) {
    case 'water':
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Index#/water'
      router.replace({ name: 'SignageWater' })
      break
    case 'bridge':
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Index#/bridge'
      router.replace({ name: 'SignageBridge' })
      break
    case 'digital':
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Signage'
      router.replace({ name: 'SignageDigital' })
      break
    case 'device':
      currentIframeUrl.value = 'https://signage.beiguan-platform.com/Index#/device'
      router.replace({ name: 'SignageDevice' })
      break
  }
}

const currentTitle = computed(() => {
  switch (currentSignageType.value) {
    case 'water':
      return '水域監控'
    case 'bridge':
      return '橋梁監控'
    case 'digital':
      return '電子看板'
    case 'device':
      return '設備狀態'
    default:
      return '水域監控'
  }
})

// 監聽路由變化（處理 F5 重整或直接 URL 訪問）
watch(() => route.name, () => {
  initializeFromRoute()
})

onMounted(() => {
  initializeFromRoute()
})
</script>

<style scoped>
/* 隱藏 iframe 內的頂部導航 */
.iframe-with-hidden-nav {
  /* 向上移動 50px 來隱藏頂部導航 */
  margin-top: -50px;
  /* 增加高度來補償被隱藏的部分 */
  height: calc(100% + 50px) !important;
  /* 確保沒有滾動條 */
  overflow: hidden;
}

/* 確保主容器正確處理 iframe 的溢出 */
main {
  overflow: hidden;
}
</style>
