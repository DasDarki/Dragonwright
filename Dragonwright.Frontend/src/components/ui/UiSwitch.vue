<script setup lang="ts">
defineProps<{
  label?: string
  disabled?: boolean
}>()

const model = defineModel<boolean>({ default: false })

function toggle() {
  model.value = !model.value;
}
</script>

<template>
  <button
    type="button"
    class="switch"
    :class="[{ 'switch--on': model, 'switch--disabled': disabled }]"
    role="switch"
    :aria-checked="model"
    :disabled="disabled"
    @click="toggle"
  >
    <div class="switch__track">
      <div class="switch__thumb" />
    </div>
    <span v-if="label">{{ label }}</span>
  </button>
</template>
