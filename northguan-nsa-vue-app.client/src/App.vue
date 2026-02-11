<template>
  <Toaster
      position="bottom-left"
      :duration="4000"
      :close-button="true"
      rich-colors
    />
  <RouterView />
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import { Toaster } from 'vue-sonner'
import 'vue-sonner/style.css'
</script>
