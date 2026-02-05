<script setup lang="ts">
defineProps<{
  icon: string
  label: string
  description: string
  to: string
  teamOnly?: boolean
}>()
</script>

<template>
  <RouterLink :to="to" class="content-card">
    <div class="content-card__icon">
      <i :class="icon" />
    </div>
    <div class="content-card__body">
      <h3 class="content-card__title">{{ label }}</h3>
      <p class="content-card__desc">{{ description }}</p>
    </div>
    <span v-if="teamOnly" class="content-card__badge">Team only</span>
    <i class="fas fa-chevron-right content-card__arrow" />
  </RouterLink>
</template>
