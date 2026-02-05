<script setup lang="ts">
import { computed } from 'vue'
import UiFieldTip from '@/components/ui/UiFieldTip.vue'
import type { FieldTip } from '@/components/ui/UiFieldTip.vue'

type Size = 'sm' | 'md' | 'lg'

export interface SelectOption {
  label: string
  value: string | number
}

const props = defineProps<{
  label?: string
  options: SelectOption[]
  placeholder?: string
  size?: Size
  disabled?: boolean
  tip?: FieldTip
}>()

const model = defineModel<any>({ default: undefined })

const sizeClass = computed(() => `input--${props.size ?? 'md'}`)
</script>

<template>
  <div class="field">
    <label v-if="label" class="field__label">{{ label }}<UiFieldTip v-if="tip" v-bind="tip" /></label>
    <select
      class="input"
      :class="[sizeClass]"
      :disabled="disabled"
      v-model="model"
    >
      <option v-if="placeholder" value="">{{ placeholder }}</option>
      <option
        v-for="opt in options"
        :key="opt.value"
        :value="opt.value"
      >
        {{ opt.label }}
      </option>
    </select>
  </div>
</template>
