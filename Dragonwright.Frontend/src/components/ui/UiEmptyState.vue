<script setup lang="ts">
defineProps<{
  icon?: string
  title: string
  subtitle?: string
}>()
</script>

<template>
  <div class="empty-state">
    <i v-if="icon" :class="icon" class="empty-state__icon" />
    <h3 class="empty-state__title">{{ title }}</h3>
    <p v-if="subtitle" class="empty-state__subtitle">{{ subtitle }}</p>
  </div>
</template>
