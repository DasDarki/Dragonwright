<script setup lang="ts">
import { computed } from 'vue'
import UiFieldTip from '@/components/ui/UiFieldTip.vue'
import type { FieldTip } from '@/components/ui/UiFieldTip.vue'

type Size = 'sm' | 'md' | 'lg'

const props = defineProps<{
  label?: string
  placeholder?: string
  size?: Size
  type?: string
  disabled?: boolean
  loading?: boolean
  error?: string
  hint?: string
  leftIcon?: string | null
  rightIcon?: string | null
  tip?: FieldTip
}>()

const model = defineModel<any>({default: ""});

const sizeClass = computed(() => `input--${props.size ?? 'md'}`)

const classList = computed(() => [
  'input',
  sizeClass.value,
  props.error && 'input--error',
  props.leftIcon && 'input--with-left-icon',
  (props.rightIcon || props.loading) && 'input--with-right-icon'
])
</script>

<template>
  <div class="field">
    <label v-if="label" class="field__label">
      {{ label }}<UiFieldTip v-if="tip" v-bind="tip" />
    </label>

    <div class="input-wrapper">
      <span v-if="leftIcon" class="input__icon input__icon--left">
        <i :class="leftIcon" />
      </span>

      <input
        :type="type ?? 'text'"
        :class="classList"
        :placeholder="placeholder"
        :disabled="disabled"
        v-model="model"
      />

      <span
        v-if="loading"
        class="input__spinner"
      />

      <span v-else-if="rightIcon" class="input__icon input__icon--right">
        <i :class="rightIcon" />
      </span>
    </div>

    <p v-if="error" class="field__error">{{ error }}</p>
    <p v-else-if="hint" class="field__hint">{{ hint }}</p>
  </div>
</template>
