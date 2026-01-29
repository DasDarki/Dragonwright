<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  cols?: number
  colsSm?: number
  colsMd?: number
  colsLg?: number
  gap?: number
}>()

const style = computed(() => {
  const s: Record<string, string> = {}
  if (props.cols) s['--cols'] = String(props.cols)
  if (props.colsSm) s['--cols-sm'] = String(props.colsSm)
  if (props.colsMd) s['--cols-md'] = String(props.colsMd)
  if (props.colsLg) s['--cols-lg'] = String(props.colsLg)
  if (props.gap) s['gap'] = `${props.gap}rem`
  return s
})
</script>

<template>
  <div class="grid" :style="style">
    <slot />
  </div>
</template>
