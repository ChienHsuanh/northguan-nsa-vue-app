<template>
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
    <Card v-for="i in 4" :key="i">
      <CardContent class="p-6">
        <div class="flex items-center justify-between">
          <div class="space-y-2">
            <Skeleton class="h-4 w-20" />
            <Skeleton class="h-8 w-16" />
          </div>
          <Skeleton class="w-8 h-8 rounded" />
        </div>
      </CardContent>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
</script>