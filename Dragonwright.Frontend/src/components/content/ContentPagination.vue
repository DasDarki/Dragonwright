<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  page: number
  totalPages: number
  totalCount: number
}>()

const emit = defineEmits<{
  (e: 'update:page', page: number): void
}>()

const hasPrev = computed(() => props.page > 1)
const hasNext = computed(() => props.page < props.totalPages)

const visiblePages = computed(() => {
  const pages: number[] = []
  const start = Math.max(1, props.page - 2)
  const end = Math.min(props.totalPages, props.page + 2)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})
</script>

<template>
  <div v-if="totalPages > 1" class="content-pagination">
    <span class="content-pagination__info">
      Page {{ page }} of {{ totalPages }} ({{ totalCount }} total)
    </span>
    <div class="content-pagination__controls">
      <button
        class="content-pagination__btn"
        :disabled="!hasPrev"
        @click="emit('update:page', page - 1)"
      >
        <i class="fas fa-chevron-left" />
      </button>
      <button
        v-for="p in visiblePages"
        :key="p"
        class="content-pagination__btn"
        :class="{ 'content-pagination__btn--active': p === page }"
        @click="emit('update:page', p)"
      >
        {{ p }}
      </button>
      <button
        class="content-pagination__btn"
        :disabled="!hasNext"
        @click="emit('update:page', page + 1)"
      >
        <i class="fas fa-chevron-right" />
      </button>
    </div>
  </div>
</template>
