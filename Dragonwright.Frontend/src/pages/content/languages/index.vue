<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useTitle } from '@/composables/useTitle'
import { useAuthStore } from '@/stores/auth'
import { useContentList } from '@/composables/useContentList'
import { getContentType } from '@/content/types'
import type { Language } from '@/api'
import type { ColumnDef } from '@/components/content/ContentListTable.vue'
import ContentListHeader from '@/components/content/ContentListHeader.vue'
import ContentListTable from '@/components/content/ContentListTable.vue'
import ContentPagination from '@/components/content/ContentPagination.vue'

useTitle('Languages')

const router = useRouter()
const typeDef = getContentType('languages')!
const authStore = useAuthStore()
const isTeamMember = computed(() => (authStore.loggedInUser?.userRole ?? 0) >= 1)

const {
  items, loading, page, totalPages, totalCount,
  search, deleteItem, goToPage,
} = useContentList<Language>(typeDef)

const columns: ColumnDef[] = [
  { key: 'name', label: 'Name' },
]

function canDelete(): boolean {
  return isTeamMember.value
}

function onNew() {
  router.push('/content/languages/new')
}

function onEdit(item: Language) {
  if (item.id) router.push(`/content/languages/${item.id}`)
}
</script>

<template>
  <div>
    <div class="content-page__header">
      <RouterLink to="/content" class="content-page__back">
        <i class="fas fa-arrow-left" />
      </RouterLink>
      <h1 class="content-page__title">Languages</h1>
    </div>

    <ContentListHeader
      v-model:search="search"
      :show-source-filter="false"
      :show-new-button="isTeamMember"
      @new="onNew"
    />

    <ContentListTable
      :items="items"
      :columns="columns"
      :loading="loading"
      empty-icon="fas fa-language"
      empty-title="No languages found"
      empty-subtitle="Try adjusting your search."
      @delete="deleteItem"
    >
      <template #cell-name="{ item }">
        <a class="content-table__link" @click.prevent="onEdit(item)">{{ item.name }}</a>
      </template>
      <template #actions="{ item }">
        <button class="content-table__action-btn" title="Edit" @click="onEdit(item)">
          <i class="fas fa-pen" />
        </button>
        <button
          v-if="canDelete()"
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
