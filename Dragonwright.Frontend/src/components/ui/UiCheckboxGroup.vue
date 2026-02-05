<script setup lang="ts">
import type { SelectOption } from '@/components/ui/UiSelect.vue'
import UiFieldTip from '@/components/ui/UiFieldTip.vue'
import type { FieldTip } from '@/components/ui/UiFieldTip.vue'

defineProps<{
  label?: string
  options: SelectOption[]
  tip?: FieldTip
}>()

const model = defineModel<number[]>({ default: () => [] })

function toggle(value: number) {
  const arr = [...model.value]
  const idx = arr.indexOf(value)
  if (idx >= 0) {
    arr.splice(idx, 1)
  } else {
    arr.push(value)
  }
  model.value = arr
}
</script>

<template>
  <div class="field">
    <label v-if="label" class="field__label">{{ label }}<UiFieldTip v-if="tip" v-bind="tip" /></label>
    <div class="checkbox-group">
      <label
        v-for="opt in options"
        :key="opt.value"
        class="checkbox checkbox-group__item"
        @click.prevent="toggle(Number(opt.value))"
      >
        <span
          class="checkbox__box"
          :class="{ 'checkbox__box--checked': model.includes(Number(opt.value)) }"
        >
          <i v-if="model.includes(Number(opt.value))" class="fas fa-check" />
        </span>
        <span class="checkbox__label">{{ opt.label }}</span>
      </label>
    </div>
  </div>
</template>

<style scoped>
.checkbox-group {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1rem;
}
</style>
