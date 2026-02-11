<template>
  <div :class="cardClasses">
    <slot />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  class: ''
})

const cardClasses = computed(() => {
  return `bg-gradient-to-br from-white to-slate-50 p-4 rounded-xl shadow-lg shadow-blue-100/50 ${props.class}`
})
</script>