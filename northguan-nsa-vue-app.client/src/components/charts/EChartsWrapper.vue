<template>
  <div v-if="isMounted" ref="chartRef" :style="{ width: width, height: height }" class="echart-container">
    <!-- Skeleton loading state -->
    <ChartSkeleton 
      v-if="loading" 
      :height="height"
      class="absolute inset-0 z-10 bg-white"
    />
  </div>
  <div v-else :style="{ width: width, height: height }" class="echart-container">
    <ChartSkeleton 
      :height="height"
      class="absolute inset-0 z-10 bg-white"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick, onBeforeUnmount } from 'vue'
import * as echarts from 'echarts'
import type { EChartsOption } from 'echarts'
import ChartSkeleton from '@/components/ui/chart-skeleton/ChartSkeleton.vue'

interface Props {
  option: EChartsOption
  width?: string
  height?: string
  theme?: string
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  width: '100%',
  height: '400px',
  theme: 'default',
  loading: false
})

const chartRef = ref<HTMLDivElement>()
let chartInstance: echarts.ECharts | null = null
let resizeHandler: (() => void) | null = null
let isMounted = ref(false)

const safeDisposeChart = () => {
  try {
    if (chartInstance) {
      if (!chartInstance.isDisposed()) {
        chartInstance.dispose()
      }
      chartInstance = null
    }
  } catch (error) {
    console.warn('Chart dispose error (safe to ignore):', error)
    chartInstance = null
  }
}

const isElementValid = () => {
  return chartRef.value && 
         document.body.contains(chartRef.value) && 
         chartRef.value.parentNode &&
         isMounted.value
}

const initChart = async () => {
  if (!isMounted.value || !chartRef.value) return
  
  await nextTick()
  
  try {
    // 清理現有實例
    safeDisposeChart()
    
    // 檢查DOM元素是否仍然有效
    if (!isElementValid()) {
      return
    }
    
    // 檢查元素尺寸
    const rect = chartRef.value.getBoundingClientRect()
    if (rect.width === 0 || rect.height === 0) {
      // 如果元素還沒有尺寸，延遲初始化
      setTimeout(() => {
        if (isElementValid()) {
          initChart()
        }
      }, 100)
      return
    }
    
    chartInstance = echarts.init(chartRef.value, props.theme)
    
    if (props.option && Object.keys(props.option).length > 0) {
      chartInstance.setOption(props.option, true)
    } else {
      // 設置默認配置
      chartInstance.setOption({
        title: { 
          text: '暫無數據',
          left: 'center',
          top: 'middle',
          textStyle: { fontSize: 14, color: '#999' }
        },
        xAxis: { type: 'category', data: [] },
        yAxis: { type: 'value' },
        series: []
      })
    }
    
    // 設置resize處理器
    resizeHandler = () => {
      if (chartInstance && !chartInstance.isDisposed() && isElementValid()) {
        try {
          chartInstance.resize()
        } catch (error) {
          console.warn('Chart resize error (safe to ignore):', error)
        }
      }
    }
    window.addEventListener('resize', resizeHandler)
    
  } catch (error) {
    console.warn('ECharts initialization failed:', error)
    // 如果初始化失敗，確保清理
    safeDisposeChart()
  }
}

const updateChart = () => {
  if (!isElementValid() || !chartInstance || chartInstance.isDisposed()) return
  
  try {
    if (props.option && Object.keys(props.option).length > 0) {
      chartInstance.setOption(props.option, true)
    }
  } catch (error) {
    console.warn('ECharts update failed:', error)
    // 如果更新失敗，嘗試重新初始化
    if (isElementValid()) {
      setTimeout(() => initChart(), 100)
    }
  }
}

const showLoading = () => {
  if (!isElementValid() || !chartInstance || chartInstance.isDisposed()) return
  
  try {
    chartInstance.showLoading('default', {
      text: '載入中...',
      color: '#3b82f6',
      textColor: '#374151',
      maskColor: 'rgba(255, 255, 255, 0.8)',
      zlevel: 0
    })
  } catch (error) {
    console.warn('ECharts showLoading failed:', error)
  }
}

const hideLoading = () => {
  if (!isElementValid() || !chartInstance || chartInstance.isDisposed()) return
  
  try {
    chartInstance.hideLoading()
  } catch (error) {
    console.warn('ECharts hideLoading failed:', error)
  }
}

// 監聽 option 變化
watch(() => props.option, (newOption) => {
  if (isElementValid() && newOption && Object.keys(newOption).length > 0) {
    // 延遲更新，確保DOM穩定
    setTimeout(() => {
      if (isElementValid()) {
        updateChart()
      }
    }, 50)
  }
}, { deep: true, flush: 'post' })

// 監聽 loading 狀態
watch(() => props.loading, (loading) => {
  if (!isElementValid()) return
  
  setTimeout(() => {
    if (isElementValid()) {
      if (loading) {
        showLoading()
      } else {
        hideLoading()
      }
    }
  }, 50)
})

onMounted(async () => {
  isMounted.value = true
  await nextTick()
  
  // 延遲初始化，確保DOM完全準備好
  setTimeout(async () => {
    if (isMounted.value) {
      await initChart()
      if (props.loading) {
        showLoading()
      }
    }
  }, 200)
})

onBeforeUnmount(() => {
  isMounted.value = false
  
  // 清理resize監聽器
  if (resizeHandler) {
    window.removeEventListener('resize', resizeHandler)
    resizeHandler = null
  }
})

onUnmounted(() => {
  isMounted.value = false
  safeDisposeChart()
})

// 暴露方法給父組件
defineExpose({
  getChartInstance: () => chartInstance,
  resize: () => {
    if (chartInstance && !chartInstance.isDisposed() && isElementValid()) {
      try {
        chartInstance.resize()
      } catch (error) {
        console.warn('Chart resize error (safe to ignore):', error)
      }
    }
  },
  showLoading,
  hideLoading
})
</script>

<style scoped>
.echart-container {
  min-height: 200px;
  position: relative;
}
</style>