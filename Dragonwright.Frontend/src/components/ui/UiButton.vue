<script setup lang="ts">
import { computed } from 'vue'

type Variant = 'primary' | 'secondary' | 'ghost' | 'outline' | 'danger'
type Size = 'sm' | 'md' | 'lg'

const props = defineProps<{
  variant?: Variant
  size?: Size
  type?: 'button' | 'submit' | 'reset'
  loading?: boolean
  disabled?: boolean
  fullWidth?: boolean
  leftIcon?: string | null
  rightIcon?: string | null
}>()

const emit = defineEmits<{
  (e: 'click', evt: MouseEvent): void
}>()

const variantClass = computed(() => `btn--${props.variant ?? 'primary'}`)
const sizeClass = computed(() => `btn--${props.size ?? 'md'}`)

const isDisabled = computed(() => props.disabled || props.loading)

function onClick(evt: MouseEvent) {
  if (isDisabled.value) return
  emit('click', evt)
}
</script>

<template>
  <button
    :type="type ?? 'button'"
    class="btn"
    :class="[
      variantClass,
      sizeClass,
      { 'btn--full': fullWidth, 'btn--loading': loading }
    ]"
    :disabled="isDisabled"
    @click="onClick"
  >
    <span v-if="loading" class="btn__spinner" />

    <span class="btn__content">
      <span v-if="leftIcon && !loading" class="btn__icon">
        <i :class="leftIcon" />
      </span>

      <span><slot /></span>

      <span v-if="rightIcon && !loading" class="btn__icon">
        <i :class="rightIcon" />
      </span>
    </span>
  </button>
</template>

<style scoped lang="scss">
</style>
