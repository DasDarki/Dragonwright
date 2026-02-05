<script setup lang="ts">
import { computed } from 'vue'
import { useTitle } from '@/composables/useTitle'
import { useAuthStore } from '@/stores/auth'
import { useContentList } from '@/composables/useContentList'
import { getContentType } from '@/content/types'
import type { Feat } from '@/api'
import type { ColumnDef } from '@/components/content/ContentListTable.vue'
import ContentListHeader from '@/components/content/ContentListHeader.vue'
import ContentListTable from '@/components/content/ContentListTable.vue'
import ContentPagination from '@/components/content/ContentPagination.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { sourceBadge } from '@/content/enums'

useTitle('Feats')

const typeDef = getContentType('feats')!
const authStore = useAuthStore()
const isTeamMember = computed(() => (authStore.loggedInUser?.userRole ?? 0) >= 1)

const {
  items, loading, page, totalPages, totalCount,
  search, source, deleteItem, goToPage,
} = useContentList<Feat>(typeDef)

const columns: ColumnDef[] = [
  { key: 'name', label: 'Name' },
  { key: 'featLevel', label: 'Level', hideOnMobile: true },
  { key: 'isRepeatable', label: 'Repeatable', hideOnMobile: true },
]

function canDelete(item: Feat): boolean {
  if (isTeamMember.value) return true
  return item.sourceCreatorId === authStore.loggedInUser?.id
}
</script>

<template>
  <div>
    <div class="content-page__header">
      <RouterLink to="/content" class="content-page__back">
        <i class="fas fa-arrow-left" />
      </RouterLink>
      <h1 class="content-page__title">Feats</h1>
    </div>

    <ContentListHeader
      v-model:search="search"
      v-model:source="source"
      :show-source-filter="true"
      :show-new-button="true"
    />

    <ContentListTable
      :items="items"
      :columns="columns"
      :loading="loading"
      empty-icon="fas fa-star"
      empty-title="No feats found"
      empty-subtitle="Try adjusting your search or filters."
      @delete="deleteItem"
    >
      <template #cell-featLevel="{ value }">
        {{ value ?? '—' }}
      </template>
      <template #cell-isRepeatable="{ value }">
        <UiBadge v-if="value" label="Yes" variant="success" />
        <span v-else>No</span>
      </template>
      <template #cell-name="{ item }">
        <span>{{ item.name }}</span>
        <UiBadge
          v-if="item.source !== undefined && sourceBadge[item.source]"
          :label="sourceBadge[item.source]!.label"
          :variant="sourceBadge[item.source]!.variant"
          style="margin-left: 0.5rem"
        />
      </template>
      <template #actions="{ item }">
        <button
          v-if="canDelete(item)"
          class="content-table__action-btn content-table__action-btn--danger"
          title="Delete"
          @click="deleteItem(item)"
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
