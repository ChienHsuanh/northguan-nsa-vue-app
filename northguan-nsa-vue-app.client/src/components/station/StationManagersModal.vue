<template>
  <Dialog :open="isOpen" @update:open="$emit('close')">
    <DialogContent class="max-w-2xl max-h-[90vh]">
      <DialogHeader>
        <DialogTitle class="flex items-center space-x-3">
          <UsersIcon class="w-5 h-5 text-blue-600" />
          <div>
            <span>分站管理人員</span>
            <p class="text-sm font-normal text-gray-600 mt-1">{{ station?.name }}</p>
          </div>
        </DialogTitle>
        <DialogDescription>
          查看和管理分站的管理人員清單
        </DialogDescription>
      </DialogHeader>

      <div v-if="station" class="space-y-4">
        <!-- 管理人員列表 -->
        <div v-if="managers.length > 0" class="space-y-3">
          <h3 class="font-semibold text-gray-900 flex items-center justify-between">
            <span class="flex items-center">
              <UserIcon class="w-4 h-4 mr-2" />
              管理人員清單
            </span>
            <span class="text-sm font-normal text-gray-500">共 {{ managers.length }} 位</span>
          </h3>
          
          <div class="space-y-2">
            <div 
              v-for="manager in managers" 
              :key="manager.id"
              class="flex items-center p-3 bg-gray-50 rounded-lg border hover:bg-gray-100 transition-colors"
            >
              <div class="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center mr-3">
                <UserIcon class="w-5 h-5 text-blue-600" />
              </div>
              <div class="flex-1">
                <h4 class="font-medium text-gray-900">{{ manager.name }}</h4>
                <p class="text-sm text-gray-600">{{ manager.username }}</p>
                <p v-if="manager.email" class="text-xs text-gray-500">{{ manager.email }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- 空狀態 -->
        <div v-else class="text-center py-12">
          <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <UsersIcon class="w-8 h-8 text-gray-400" />
          </div>
          <h3 class="text-lg font-medium text-gray-900 mb-2">尚無管理人員</h3>
          <p class="text-gray-600">此分站目前沒有指派任何管理人員</p>
        </div>
      </div>

      <DialogFooter>
        <Button @click="$emit('close')" variant="outline">
          關閉
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>

<script setup lang="ts">
import { StationResponse, UserResponse } from '@/services'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import Button from '@/components/ui/button/Button.vue'
import {
  Users as UsersIcon,
  User as UserIcon
} from 'lucide-vue-next'

interface Props {
  isOpen: boolean
  station: StationResponse | null
  managers: UserResponse[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
}>()
</script>