<script setup lang="ts">
import UiEmptyState from '@/components/ui/UiEmptyState.vue'

export interface ColumnDef {
  key: string
  label: string
  hideOnMobile?: boolean
}

defineProps<{
  items: any[]
  columns: ColumnDef[]
  loading?: boolean
  emptyIcon?: string
  emptyTitle?: string
  emptySubtitle?: string
}>()

defineEmits<{
  (e: 'delete', item: any): void
}>()
</script>

<template>
  <div v-if="loading" class="content-list-loading">
    <span class="btn__spinner content-list-loading__spinner" />
    <span class="content-list-loading__text">Loading...</span>
  </div>

  <div v-else-if="items.length === 0" class="content-list-empty">
    <UiEmptyState
      :icon="emptyIcon ?? 'fas fa-inbox'"
      :title="emptyTitle ?? 'Nothing here yet'"
      :subtitle="emptySubtitle"
    />
  </div>

  <template v-else>
    <table class="content-table">
      <thead>
        <tr>
          <th
            v-for="col in columns"
            :key="col.key"
            :class="{ 'hide-mobile': col.hideOnMobile }"
          >
            {{ col.label }}
          </th>
          <th class="content-table__actions-col">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in items" :key="item.id ?? item.name">
          <td
            v-for="col in columns"
            :key="col.key"
            :class="{ 'hide-mobile': col.hideOnMobile }"
          >
            <slot :name="`cell-${col.key}`" :item="item" :value="item[col.key]">
              {{ item[col.key] ?? '—' }}
            </slot>
          </td>
          <td class="content-table__actions-col">
            <slot name="actions" :item="item">
              <button
                class="content-table__action-btn content-table__action-btn--danger"
                title="Delete"
                @click="$emit('delete', item)"
              >
                <i class="fas fa-trash" />
              </button>
            </slot>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Mobile card list -->
    <div class="content-cards-mobile">
      <div
        v-for="item in items"
        :key="item.id ?? item.name"
        class="content-card-mobile"
      >
        <div class="content-card-mobile__body">
          <div
            v-for="col in columns"
            :key="col.key"
            class="content-card-mobile__field"
          >
            <span class="content-card-mobile__label">{{ col.label }}</span>
            <span class="content-card-mobile__value">
              <slot :name="`cell-${col.key}`" :item="item" :value="item[col.key]">
                {{ item[col.key] ?? '—' }}
              </slot>
            </span>
          </div>
        </div>
        <div class="content-card-mobile__actions">
          <slot name="actions" :item="item">
            <button
              class="content-table__action-btn content-table__action-btn--danger"
              title="Delete"
              @click="$emit('delete', item)"
            >
              <i class="fas fa-trash" />
            </button>
          </slot>
        </div>
      </div>
    </div>
  </template>
</template>
