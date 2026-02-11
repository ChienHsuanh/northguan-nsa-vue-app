<template>
  <Card class="h-full">
    <CardHeader class="pb-3">
      <CardTitle class="flex items-center gap-2 text-lg">
        <Shield class="h-5 w-5" />
        電子圍籬事件
      </CardTitle>
    </CardHeader>
    <CardContent class="pt-0">
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 3" :key="i" class="animate-pulse">
          <div class="h-12 bg-gray-200 rounded"></div>
        </div>
      </div>

      <div v-else-if="events.length === 0" class="text-center py-8 text-gray-500">
        暫無事件
      </div>

      <div v-else class="space-y-3">
        <div
          v-for="event in events"
          :key="event.id"
          class="flex items-center gap-3 p-3 border rounded-lg hover:bg-gray-50 cursor-pointer transition-colors"
          @click="$emit('eventClick', event)"
        >
          <div class="flex-shrink-0">
            <div class="w-8 h-8 bg-red-100 rounded-full flex items-center justify-center">
              <AlertTriangle class="h-4 w-4 text-red-600" />
            </div>
          </div>

          <div class="flex-1 min-w-0">
            <div class="font-medium text-sm truncate">{{ event.deviceName }}</div>
            <div class="text-xs text-red-600">
              {{ event.event }}
            </div>
          </div>

          <div class="flex-shrink-0 text-right">
            <div class="text-xs text-gray-500">{{ event.date }}</div>
            <div class="text-xs text-gray-400">{{ event.time }}</div>
          </div>
        </div>
      </div>
    </CardContent>
  </Card>
</template>

<script setup lang="ts">
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Shield, AlertTriangle } from 'lucide-vue-next'
import type { FenceEvent } from '@/composables/useHomeData'

interface Props {
  events: FenceEvent[]
  loading: boolean
}

defineProps<Props>()

defineEmits<{
  eventClick: [event: FenceEvent]
}>()
</script>
