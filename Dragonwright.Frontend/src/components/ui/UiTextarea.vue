<script setup lang="ts">
import { computed } from 'vue'
import UiFieldTip from '@/components/ui/UiFieldTip.vue'
import type { FieldTip } from '@/components/ui/UiFieldTip.vue'

type Size = 'sm' | 'md' | 'lg'

const props = defineProps<{
  label?: string
  placeholder?: string
  size?: Size
  disabled?: boolean
  rows?: number
  error?: string
  hint?: string
  tip?: FieldTip
}>()

const model = defineModel<string>({ default: '' })

const sizeClass = computed(() => `input--${props.size ?? 'md'}`)

const classList = computed(() => [
  'input',
  'input--textarea',
  sizeClass.value,
  props.error && 'input--error',
])
</script>

<template>
  <div class="field">
    <label v-if="label" class="field__label">{{ label }}<UiFieldTip v-if="tip" v-bind="tip" /></label>
    <textarea
      :class="classList"
      :placeholder="placeholder"
      :disabled="disabled"
      :rows="rows ?? 4"
      v-model="model"
    />
    <p v-if="error" class="field__error">{{ error }}</p>
    <p v-else-if="hint" class="field__hint">{{ hint }}</p>
  </div>
</template>

<style scoped>
.input--textarea {
  resize: vertical;
  min-height: 3rem;
}
</style>
