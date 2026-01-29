<script setup lang="ts">
import { computed } from 'vue'

type Direction = 'row' | 'column'
type Align = 'start' | 'center' | 'end' | 'stretch'
type Justify = 'start' | 'center' | 'end' | 'between'

const props = defineProps<{
  direction?: Direction
  gap?: number
  align?: Align
  justify?: Justify
  wrap?: boolean
}>()

const classes = computed(() => [
  'stack',
  props.direction === 'row' && 'stack--row',
  props.align === 'center' && 'stack--center',
  props.justify === 'between' && 'stack--between'
])

const style = computed(() => ({
  gap: props.gap ? `${props.gap}rem` : undefined,
  flexWrap: props.wrap ? 'wrap' : undefined,
  alignItems: props.align && props.align !== 'center' ? props.align : undefined,
  justifyContent: props.justify && props.justify !== 'between'
    ? (props.justify === 'start'
      ? 'flex-start'
      : props.justify === 'end'
        ? 'flex-end'
        : 'center')
    : undefined
}))
</script>

<template>
  <div :class="classes" :style="style as any">
    <slot />
  </div>
</template>
