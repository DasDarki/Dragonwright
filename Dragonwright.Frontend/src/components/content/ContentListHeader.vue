<script setup lang="ts">
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiButton from '@/components/ui/UiButton.vue'
import type { SelectOption } from '@/components/ui/UiSelect.vue'

defineProps<{
  showSourceFilter?: boolean
  showNewButton?: boolean
}>()

const search = defineModel<string>('search', { default: '' })
const source = defineModel<string | number | undefined>('source', { default: undefined })

defineEmits<{
  (e: 'new'): void
}>()

const sourceOptions: SelectOption[] = [
  { label: 'Legacy 2014', value: '0' },
  { label: 'One 2024', value: '1' },
  { label: 'Homebrew', value: '2' },
]
</script>

<template>
  <div class="content-list-header">
    <div class="content-list-header__filters">
      <UiInput
        v-model="search"
        placeholder="Search..."
        left-icon="fas fa-search"
        size="sm"
      />
      <UiSelect
        v-if="showSourceFilter"
        v-model="source"
        :options="sourceOptions"
        placeholder="All Sources"
        size="sm"
      />

      <slot name="filters"></slot>
    </div>
    <UiButton
      v-if="showNewButton"
      size="sm"
      left-icon="fas fa-plus"
      @click="$emit('new')"
    >
      New
    </UiButton>
  </div>
</template>
