<script setup lang="ts">
const props = defineProps<{
  modelValue: boolean
  label?: string
  disabled?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
}>()

function toggle() {
  if (props.disabled) return
  emit('update:modelValue', !props.modelValue)
}
</script>

<template>
  <button
    type="button"
    class="switch"
    :class="[{ 'switch--on': modelValue, 'switch--disabled': disabled }]"
    role="switch"
    :aria-checked="modelValue"
    @click="toggle"
  >
    <div class="switch__track">
      <div class="switch__thumb" />
    </div>
    <span v-if="label">{{ label }}</span>
  </button>
</template>
