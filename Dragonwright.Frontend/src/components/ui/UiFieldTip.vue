<script setup lang="ts">
import { ref } from 'vue'
import UiModal from '@/components/ui/UiModal.vue'

export interface FieldTip {
  title: string
  body: string
  examples?: string[]
}

defineProps<FieldTip>()

const open = ref(false)
</script>

<template>
  <span class="field-tip" @click.stop.prevent="open = true" role="button" tabindex="0" :title="`Info: ${title}`">
    <i class="fas fa-circle-info" />
  </span>

  <UiModal v-model="open" :title="title" close-on-backdrop close-on-esc topmost>
    <div class="field-tip__content">
      <p class="field-tip__body">{{ body }}</p>
      <div v-if="examples?.length" class="field-tip__examples">
        <span class="field-tip__examples-label">Examples</span>
        <ul class="field-tip__examples-list">
          <li v-for="(ex, i) in examples" :key="i">{{ ex }}</li>
        </ul>
      </div>
    </div>
  </UiModal>
</template>

<style scoped lang="scss">
.field-tip {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: var(--field-tip-color, #6b7280);
  font-size: 0.78rem;
  margin-left: 0.3rem;
  transition: color 150ms ease;
  vertical-align: middle;

  &:hover {
    color: #38bdf8;
  }
}

.field-tip__content {
  min-width: 16rem;
}

.field-tip__body {
  font-size: 0.9rem;
  line-height: 1.6;
  color: #e5e7eb;
  margin: 0;
  white-space: pre-line;
}

.field-tip__examples {
  margin-top: 0.75rem;
  padding-top: 0.75rem;
  border-top: 1px solid #1f2937;
}

.field-tip__examples-label {
  font-size: 0.78rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #9ca3af;
}

.field-tip__examples-list {
  margin: 0.25rem 0 0;
  padding-left: 1.25rem;
  font-size: 0.85rem;
  color: #e5e7eb;
  line-height: 1.6;
}
</style>
