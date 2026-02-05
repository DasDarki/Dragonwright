<script setup lang="ts">
import { computed } from 'vue'

type Variant = 'accent' | 'success' | 'warning' | 'muted' | 'info' | 'danger'

const props = defineProps<{
  variant?: Variant
  label: string
}>()

const variantClass = computed(() => `badge--${props.variant ?? 'muted'}`)
</script>

<template>
  <span class="badge" :class="variantClass">{{ label }}</span>
</template>
