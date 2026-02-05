<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useTitle } from '@/composables/useTitle'
import { useAuthStore } from '@/stores/auth'
import { useContentList } from '@/composables/useContentList'
import { getContentType } from '@/content/types'
import type { Background } from '@/api'
import type { ColumnDef } from '@/components/content/ContentListTable.vue'
import ContentListHeader from '@/components/content/ContentListHeader.vue'
import ContentListTable from '@/components/content/ContentListTable.vue'
import ContentPagination from '@/components/content/ContentPagination.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { sourceBadge } from '@/content/enums'

useTitle('Backgrounds')

const router = useRouter()
const typeDef = getContentType('backgrounds')!
const authStore = useAuthStore()
const isTeamMember = computed(() => (authStore.loggedInUser?.userRole ?? 0) >= 1)

const {
  items, loading, page, totalPages, totalCount,
  search, source, deleteItem, goToPage,
} = useContentList<Background>(typeDef)

const columns: ColumnDef[] = [
  { key: 'name', label: 'Name' },
]

function canDelete(item: Background): boolean {
  if (isTeamMember.value) return true
  return item.sourceCreatorId === authStore.loggedInUser?.id
}

function onNew() {
  router.push('/content/backgrounds/new')
}

function onEdit(item: Background) {
  if (item.id) router.push(`/content/backgrounds/${item.id}`)
}
</script>

<template>
  <div>
    <div class="content-page__header">
      <RouterLink to="/content" class="content-page__back">
        <i class="fas fa-arrow-left" />
      </RouterLink>
      <h1 class="content-page__title">Backgrounds</h1>
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
      empty-icon="fas fa-scroll"
      empty-title="No backgrounds found"
      empty-subtitle="Try adjusting your search or filters."
      @delete="deleteItem"
    >
      <template #cell-name="{ item }">
        <a class="content-table__link" @click.prevent="onEdit(item)">{{ item.name }}</a>
        <UiBadge
          v-if="item.source !== undefined && sourceBadge[item.source]"
          :label="sourceBadge[item.source]!.label"
          :variant="sourceBadge[item.source]!.variant"
          style="margin-left: 0.5rem"
        />
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
