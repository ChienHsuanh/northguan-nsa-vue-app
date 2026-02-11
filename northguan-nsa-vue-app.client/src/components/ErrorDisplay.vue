<template>
  <div v-if="error" class="rounded-md bg-red-50 p-4 mb-4">
    <div class="flex">
      <div class="flex-shrink-0">
        <AlertCircleIcon class="h-5 w-5 text-red-400" />
      </div>
      <div class="ml-3">
        <h3 class="text-sm font-medium text-red-800">{{ title || '錯誤' }}</h3>
        <div class="mt-2 text-sm text-red-700">
          <div v-if="error.errorCode === 'VALIDATION_FAILED' && error.validationErrors">
            <p class="mb-2">{{ error.message }}</p>
            <ul class="list-disc list-inside space-y-1">
              <li v-for="(errors, field) in error.validationErrors" :key="field">
                <strong>{{ field }}:</strong> {{ errors.join(', ') }}
              </li>
            </ul>
          </div>
          <div v-else>
            {{ error.message }}
          </div>
        </div>
        <div v-if="showDetails && error.details" class="mt-2 text-xs text-red-600">
          <details>
            <summary class="cursor-pointer">詳細資訊</summary>
            <pre class="mt-1 whitespace-pre-wrap">{{ error.details }}</pre>
          </details>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { AlertCircle as AlertCircleIcon } from 'lucide-vue-next'
import type { ApiErrorResponse } from '@/utils/errorHandler'

interface Props {
  error?: ApiErrorResponse | null
  title?: string
  showDetails?: boolean
}

withDefaults(defineProps<Props>(), {
  error: null,
  title: '',
  showDetails: false
})
</script>