<script setup lang="ts">
import UiFieldTip from '@/components/ui/UiFieldTip.vue'
import type { FieldTip } from '@/components/ui/UiFieldTip.vue'

const props = defineProps<{
  label?: string
  disabled?: boolean
  tip?: FieldTip
}>()

const model = defineModel<boolean>({default: false})

function toggle() {
  if (props.disabled) return
  model.value = !model.value
}
</script>

<template>
  <label
    class="checkbox"
    :class="{ 'checkbox--disabled': disabled }"
    @click.prevent="toggle"
  >
    <span
      class="checkbox__box"
      :class="{ 'checkbox__box--checked': modelValue }"
    >
      <i v-if="modelValue" class="fas fa-check" />
    </span>
    <span v-if="label" class="checkbox__label">{{ label }}<UiFieldTip v-if="tip" v-bind="tip" /></span>
    <input
      type="checkbox"
      :checked="modelValue"
      :disabled="disabled"
      class="sr-only"
      tabindex="-1"
    />
  </label>
</template>

<style scoped>
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>
