<template>
  <div class="flex flex-wrap gap-4 mb-8">
    <router-link to="/">
      <Button variant="outline" class="flex flex-col items-center p-4 h-auto">
        <HomeIcon class="w-6 h-6 mb-2" />
        <span class="text-sm">返回地圖總覽</span>
      </Button>
    </router-link>

    <router-link to="/overview/parking">
      <Button :variant="currentPage === 'parking' ? 'default' : 'outline'" class="flex flex-col items-center p-4 h-auto">
        <CarIcon class="w-6 h-6 mb-2" :class="currentPage === 'parking' ? 'text-white' : ''" />
        <span class="text-sm" :class="currentPage === 'parking' ? 'text-white' : ''">停車紀錄列表</span>
      </Button>
    </router-link>

    <router-link to="/overview/crowd">
      <Button :variant="currentPage === 'crowd' ? 'default' : 'outline'" class="flex flex-col items-center p-4 h-auto">
        <UsersIcon class="w-6 h-6 mb-2" :class="currentPage === 'crowd' ? 'text-white' : ''" />
        <span class="text-sm" :class="currentPage === 'crowd' ? 'text-white' : ''">人流紀錄列表</span>
      </Button>
    </router-link>

    <router-link to="/overview/fence">
      <Button :variant="currentPage === 'fence' ? 'default' : 'outline'" class="flex flex-col items-center p-4 h-auto">
        <ShieldIcon class="w-6 h-6 mb-2" :class="currentPage === 'fence' ? 'text-white' : ''" />
        <span class="text-sm" :class="currentPage === 'fence' ? 'text-white' : ''">電子圍籬事件</span>
      </Button>
    </router-link>

    <router-link to="/overview/traffic">
      <Button :variant="currentPage === 'traffic' ? 'default' : 'outline'" class="flex flex-col items-center p-4 h-auto">
        <CarIcon class="w-6 h-6 mb-2" :class="currentPage === 'traffic' ? 'text-white' : ''" />
        <span class="text-sm" :class="currentPage === 'traffic' ? 'text-white' : ''">車流紀錄列表</span>
      </Button>
    </router-link>
  </div>
</template>

<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { HomeIcon, CarIcon, UsersIcon, ShieldIcon } from 'lucide-vue-next'

interface Props {
  currentPage: 'parking' | 'crowd' | 'fence' | 'traffic'
}

defineProps<Props>()
</script>