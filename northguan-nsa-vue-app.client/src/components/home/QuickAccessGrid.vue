<template>
  <Card>
    <CardContent class="p-6">
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <router-link 
          v-for="item in quickAccessItems" 
          :key="item.route"
          :to="item.route"
          class="flex flex-col items-center gap-3 p-4 rounded-lg border-2 border-transparent hover:border-blue-200 hover:bg-blue-50 transition-all duration-200 group"
        >
          <div class="w-12 h-12 flex items-center justify-center rounded-lg bg-gray-100 group-hover:bg-blue-100 transition-colors">
            <component :is="item.icon" class="h-6 w-6 text-gray-600 group-hover:text-blue-600" />
          </div>
          <span class="text-sm font-medium text-gray-700 group-hover:text-blue-700 text-center">
            {{ item.name }}
          </span>
        </router-link>
      </div>
    </CardContent>
  </Card>
</template>

<script setup lang="ts">
import { Card, CardContent } from '@/components/ui/card'
import { Car, Users, Shield, Navigation } from 'lucide-vue-next'

const quickAccessItems = [
  {
    route: '/overview/parking',
    name: '停車紀錄列表',
    icon: Car
  },
  {
    route: '/overview/crowd',
    name: '人流紀錄列表',
    icon: Users
  },
  {
    route: '/overview/fence',
    name: '電子圍籬事件',
    icon: Shield
  },
  {
    route: '/overview/traffic',
    name: '車流紀錄列表',
    icon: Navigation
  }
]
</script>