<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />

    <main class="container mx-auto px-4 py-8">
      <div class="max-w-4xl mx-auto">
        <h1 class="text-2xl font-bold text-gray-900 mb-6">個人資料</h1>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <!-- Profile Card -->
          <Card class="lg:col-span-1">
            <CardContent class="p-6">
              <div class="text-center">
                <div class="w-24 h-24 bg-gray-300 rounded-full mx-auto mb-4 flex items-center justify-center">
                  <UserIcon class="w-12 h-12 text-gray-600" />
                </div>
                <h3 class="text-lg font-semibold text-gray-900">{{ profile.name || '使用者' }}</h3>
                <p class="text-sm text-gray-600">{{ profile.username || 'user@example.com' }}</p>
                <div class="mt-4">
                  <span :class="getRoleBadgeClass(profile.role)">
                    {{ getRoleText(profile.role) }}
                  </span>
                </div>
                <div v-if="profile.isReadOnly" class="mt-2">
                  <span
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                    唯讀帳戶
                  </span>
                </div>
              </div>
            </CardContent>
          </Card>

          <!-- Profile Form -->
          <Card class="lg:col-span-2">
            <CardHeader>
              <CardTitle>基本資料</CardTitle>
            </CardHeader>
            <CardContent>
              <form class="space-y-6" @submit="onSubmit">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <FormField v-slot="{ componentField }" name="name">
                    <FormItem>
                      <FormLabel>姓名 <span class="text-red-500">*</span></FormLabel>
                      <FormControl>
                        <Input type="text" placeholder="請輸入姓名" v-bind="componentField" />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  </FormField>

                  <div class="space-y-2">
                    <Label class="text-sm font-medium">電子郵件</Label>
                    <Input :model-value="profile.username" type="email" disabled class="bg-gray-50" />
                    <p class="text-xs text-gray-500">電子郵件無法修改</p>
                  </div>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <FormField v-slot="{ componentField }" name="employeeID">
                    <FormItem>
                      <FormLabel>員工編號 <span class="text-red-500">*</span></FormLabel>
                      <FormControl>
                        <Input type="text" placeholder="請輸入員工編號" v-bind="componentField" />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  </FormField>

                  <div class="space-y-2">
                    <Label class="text-sm font-medium">角色</Label>
                    <Input :model-value="getRoleText(profile.role)" disabled class="bg-gray-50" />
                    <p class="text-xs text-gray-500">角色由管理員設定</p>
                  </div>
                </div>

                <FormField v-slot="{ componentField }" name="phone">
                  <FormItem>
                    <FormLabel>聯絡電話</FormLabel>
                    <FormControl>
                      <Input type="tel" placeholder="請輸入聯絡電話" v-bind="componentField" />
                    </FormControl>
                    <FormDescription>選填，用於緊急聯絡</FormDescription>
                    <FormMessage />
                  </FormItem>
                </FormField>

                <div v-if="profile.stationIds && profile.stationIds.length > 0" class="space-y-2">
                  <Label class="text-sm font-medium">管理分站</Label>
                  <Input :model-value="getStationNames(profile.stationIds)" disabled class="bg-gray-50" />
                  <p class="text-xs text-gray-500">管理分站由管理員設定</p>
                </div>

                <div class="flex justify-end space-x-3 pt-6 border-t">
                  <Button type="button" variant="outline" @click="resetForm">
                    重置
                  </Button>
                  <Button type="submit" :disabled="isSubmitting">
                    <span v-if="isSubmitting"
                      class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></span>
                    {{ isSubmitting ? '更新中...' : '更新資料' }}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>

        <!-- Account Info -->
        <Card class="mt-6">
          <CardHeader>
            <CardTitle class="flex items-center">
              <ShieldIcon class="w-5 h-5 mr-2" />
              帳戶資訊
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-6">
              <div class="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
                <div class="flex-shrink-0">
                  <div class="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
                    <CheckCircleIcon class="w-5 h-5 text-green-600" />
                  </div>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-gray-600">帳戶狀態</p>
                  <p class="text-sm text-green-600 font-medium">啟用中</p>
                </div>
              </div>

              <div class="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
                <div class="flex-shrink-0">
                  <div :class="profile.role?.toLowerCase() === 'admin' ? 'bg-blue-100' : 'bg-green-100'"
                    class="w-10 h-10 rounded-full flex items-center justify-center">
                    <ShieldIcon :class="profile.role?.toLowerCase() === 'admin' ? 'text-blue-600' : 'text-green-600'"
                      class="w-5 h-5" />
                  </div>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-gray-600">權限等級</p>
                  <p class="text-sm font-medium"
                    :class="profile.role?.toLowerCase() === 'admin' ? 'text-blue-600' : 'text-green-600'">
                    {{ getRoleText(profile.role) }}
                  </p>
                </div>
              </div>

              <!-- <div class="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
                <div class="flex-shrink-0">
                  <div class="w-10 h-10 bg-gray-100 rounded-full flex items-center justify-center">
                    <CalendarIcon class="w-5 h-5 text-gray-600" />
                  </div>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-gray-600">最後登入</p>
                  <p class="text-sm text-gray-900 truncate">{{ profile.lastLoginTime || '未知' }}</p>
                </div>
              </div>

              <div class="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
                <div class="flex-shrink-0">
                  <div class="w-10 h-10 bg-gray-100 rounded-full flex items-center justify-center">
                    <CalendarIcon class="w-5 h-5 text-gray-600" />
                  </div>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-gray-600">建立時間</p>
                  <p class="text-sm text-gray-900 truncate">{{ profile.createdAt || '未知' }}</p>
                </div>
              </div> -->
            </div>

            <!-- 權限說明 -->
            <div v-if="profile.isReadOnly" class="mt-4 p-3 bg-orange-50 border border-orange-200 rounded-lg">
              <div class="flex items-center">
                <XCircleIcon class="w-5 h-5 text-orange-500 mr-2" />
                <span class="text-sm font-medium text-orange-800">唯讀權限</span>
              </div>
              <p class="text-sm text-orange-700 mt-1">您的帳戶設定為唯讀模式，僅能查看資料，無法進行修改操作。</p>
            </div>
          </CardContent>
        </Card>

      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue'
import { toTypedSchema } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import * as z from 'zod'
import { useAuthStore } from '@/stores/auth'
import { AuthService, ProfileResponse } from '@/services'
import { StationService, StationResponse } from '@/services'
import { useToast } from '@/composables/useToast'
import AppHeader from '@/components/layout/AppHeader.vue'
import Card from '@/components/ui/card/Card.vue'
import CardHeader from '@/components/ui/card/CardHeader.vue'
import CardContent from '@/components/ui/card/CardContent.vue'
import CardTitle from '@/components/ui/card/CardTitle.vue'
import Button from '@/components/ui/button/Button.vue'
import Input from '@/components/ui/input/Input.vue'
import Label from '@/components/ui/label/Label.vue'
import {
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from '@/components/ui/form'
import {
  User as UserIcon,
  CheckCircle as CheckCircleIcon,
  XCircle as XCircleIcon,
  Phone as PhoneIcon,
  Mail as MailIcon,
  Calendar as CalendarIcon,
  Shield as ShieldIcon
} from 'lucide-vue-next'

// Composables
const authStore = useAuthStore()
const toast = useToast()

// Validation schema
const profileSchema = toTypedSchema(z.object({
  name: z.string().min(1, '請輸入姓名'),
  employeeID: z.string().min(1, '請輸入員工編號'),
  phone: z.string().optional()
}))

// Use VeeValidate form
const { handleSubmit, setValues, isSubmitting } = useForm({
  validationSchema: profileSchema
})

// State
const profile = ref<ProfileResponse>({
  name: '',
  username: '',
  role: '',
  phone: '',
  employeeID: '',
  avatarUrl: '',
  isReadOnly: false,
  stationIds: [],
  init: function (_data?: any, _mappings?: any): void {
    throw new Error('Function not implemented.')
  },
  toJSON: function (data?: any) {
    throw new Error('Function not implemented.')
  }
})

const stations = ref<StationResponse[]>([])

// Methods
const loadProfile = async () => {
  try {
    const userProfile: ProfileResponse = await authStore.getProfile()
    profile.value = userProfile

    // 設置表單初始值
    nextTick(() => {
      setValues({
        name: userProfile.name || '',
        employeeID: userProfile.employeeID || '',
        phone: userProfile.phone || ''
      })
    })
  } catch (error) {
    console.error('載入個人資料失敗:', error)
    toast.error('載入失敗', '無法載入個人資料')
  }
}

const loadStations = async () => {
  try {
    stations.value = await StationService.getStations()
  } catch (error) {
    console.error('載入分站失敗:', error)
  }
}

const onSubmit = handleSubmit(async (values) => {
  try {
    await authStore.updateProfile(
      values.name,
      values.phone || '',
      values.employeeID
    )

    // 更新本地資料
    profile.value.name = values.name
    profile.value.employeeID = values.employeeID
    profile.value.phone = values.phone || ''

    toast.success('更新成功', '個人資料已更新')
  } catch (error) {
    console.error('更新個人資料失敗:', error)
    toast.error('更新失敗', '無法更新個人資料')
  }
})

const resetForm = () => {
  // 重置表單到原始值
  setValues({
    name: profile.value.name || '',
    employeeID: profile.value.employeeID || '',
    phone: profile.value.phone || ''
  })
}

const getRoleText = (role: string) => {
  switch (role?.toLowerCase()) {
    case 'admin':
      return '北觀管理員'
    case 'user':
    case 'manager':
      return '分站管理員'
    default:
      return '未知角色'
  }
}

const getRoleBadgeClass = (role: string) => {
  const baseClass = 'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium'
  switch (role?.toLowerCase()) {
    case 'admin':
      return `${baseClass} bg-blue-100 text-blue-800`
    case 'user':
    case 'manager':
      return `${baseClass} bg-green-100 text-green-800`
    default:
      return `${baseClass} bg-gray-100 text-gray-800`
  }
}

const getStationNames = (stationIds: number[] | string) => {
  if (!stationIds) return '無'

  // 處理舊格式的字串 ID
  let ids: number[] = []
  if (typeof stationIds === 'string') {
    if (stationIds === '-1' || !stationIds) return '無'
    ids = stationIds.split(',').map(id => parseInt(id)).filter(id => !isNaN(id))
  } else {
    ids = stationIds
  }

  if (ids.length === 0) return '無'

  const stationMap = stations.value.reduce((map, station) => {
    map[station.id || 0] = station
    return map
  }, {} as Record<number, StationResponse>)

  return ids
    .map(id => stationMap[id]?.name)
    .filter(Boolean)
    .join(', ') || '無'
}

onMounted(() => {
  loadProfile()
  loadStations()
})
</script>
