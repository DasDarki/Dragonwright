<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useTitle } from '@/composables/useTitle'
import { useAuthStore } from '@/stores/auth'
import { useContentList } from '@/composables/useContentList'
import { getContentType } from '@/content/types'
import type { Item } from '@/api'
import type { ColumnDef } from '@/components/content/ContentListTable.vue'
import ContentListHeader from '@/components/content/ContentListHeader.vue'
import ContentListTable from '@/components/content/ContentListTable.vue'
import ContentPagination from '@/components/content/ContentPagination.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { itemTypeLabels, rarityLabels } from '@/content/enums'

useTitle('Items')

const router = useRouter()
const typeDef = getContentType('items')!
const authStore = useAuthStore()
const isTeamMember = computed(() => (authStore.loggedInUser?.userRole ?? 0) >= 1)

const {
  items, loading, page, totalPages, totalCount,
  search, source, deleteItem, goToPage,
} = useContentList<Item>(typeDef)

const columns: ColumnDef[] = [
  { key: 'name', label: 'Name' },
  { key: 'type', label: 'Type', hideOnMobile: true },
  { key: 'rarity', label: 'Rarity', hideOnMobile: true },
]

const rarityBadge: Record<number, { label: string; variant: 'muted' | 'success' | 'info' | 'accent' | 'warning' | 'danger' }> = {
  0: { label: 'Common', variant: 'muted' },
  1: { label: 'Uncommon', variant: 'success' },
  2: { label: 'Rare', variant: 'info' },
  3: { label: 'Very Rare', variant: 'accent' },
  4: { label: 'Legendary', variant: 'warning' },
  5: { label: 'Artifact', variant: 'danger' },
}

function canDelete(item: Item): boolean {
  if (isTeamMember.value) return true
  return item.sourceCreatorId === authStore.loggedInUser?.id
}

function onNew() {
  router.push('/content/items/new')
}

function onEdit(item: Item) {
  if (item.id) router.push(`/content/items/${item.id}`)
}
</script>

<template>
  <div>
    <div class="content-page__header">
      <RouterLink to="/content" class="content-page__back">
        <i class="fas fa-arrow-left" />
      </RouterLink>
      <h1 class="content-page__title">Items</h1>
    </div>

    <ContentListHeader
      v-model:search="search"
      v-model:source="source"
      :show-source-filter="true"
      :show-new-button="true"
      @new="onNew"
    />

    <ContentListTable
      :items="items"
      :columns="columns"
      :loading="loading"
      empty-icon="fas fa-ring"
      empty-title="No items found"
      empty-subtitle="Try adjusting your search or filters."
      @delete="deleteItem"
    >
      <template #cell-type="{ value }">
        {{ itemTypeLabels[value as number] ?? '—' }}
      </template>
      <template #cell-rarity="{ value }">
        <UiBadge
          v-if="value !== undefined && rarityBadge[value as number]"
          :label="rarityBadge[value as number]!.label"
          :variant="rarityBadge[value as number]!.variant"
        />
        <span v-else>—</span>
      </template>
      <template #cell-name="{ item }">
        <a class="content-table__link" @click.prevent="onEdit(item)">{{ item.name }}</a>
      </template>
      <template #actions="{ item }">
        <button class="content-table__action-btn" title="Edit" @click="onEdit(item)">
          <i class="fas fa-pen" />
        </button>
        <button
          v-if="canDelete(item)"
          class="content-table__action-btn content-table__action-btn--danger"
          title="Delete" @click="deleteItem(item)"
        >
          <i class="fas fa-trash" />
        </button>
      </template>
    </ContentListTable>

    <ContentPagination
      :page="page"
      :total-pages="totalPages"
      :total-count="totalCount"
      @update:page="goToPage"
    />
  </div>
</template>
