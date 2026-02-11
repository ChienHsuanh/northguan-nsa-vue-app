<template>
  <div class="w-full" :style="{ height: height }">
    <div class="animate-pulse">
      <!-- Chart title skeleton -->
      <div class="mb-4">
        <Skeleton class="h-6 w-32" />
      </div>
      
      <!-- Chart area skeleton -->
      <div class="relative bg-gray-50 rounded-lg" :style="{ height: `calc(${height} - 3rem)` }">
        <!-- Y-axis labels -->
        <div class="absolute left-2 top-4 space-y-8">
          <Skeleton v-for="i in 5" :key="`y-${i}`" class="h-3 w-8" />
        </div>
        
        <!-- Chart bars/lines simulation -->
        <div class="absolute bottom-8 left-12 right-4 flex items-end justify-between space-x-2">
          <div 
            v-for="i in 12" 
            :key="`bar-${i}`"
            class="bg-gray-200 rounded-t"
            :style="{ 
              height: `${Math.random() * 60 + 20}%`, 
              width: 'calc(100% / 12 - 0.5rem)' 
            }"
          />
        </div>
        
        <!-- X-axis labels -->
        <div class="absolute bottom-2 left-12 right-4 flex justify-between">
          <Skeleton v-for="i in 6" :key="`x-${i}`" class="h-3 w-8" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Skeleton } from '@/components/ui/skeleton'

interface Props {
  height?: string
}

withDefaults(defineProps<Props>(), {
  height: '300px'
})
</script>