<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useTitle } from '@/composables/useTitle'
import { useAuthStore } from '@/stores/auth'
import { useContentList } from '@/composables/useContentList'
import { getContentType } from '@/content/types'
import type { Spell } from '@/api'
import type { ColumnDef } from '@/components/content/ContentListTable.vue'
import ContentListHeader from '@/components/content/ContentListHeader.vue'
import ContentListTable from '@/components/content/ContentListTable.vue'
import ContentPagination from '@/components/content/ContentPagination.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { spellSchoolLabels, sourceBadge } from '@/content/enums'

useTitle('Spells')

const router = useRouter()
const typeDef = getContentType('spells')!
const authStore = useAuthStore()
const isTeamMember = computed(() => (authStore.loggedInUser?.userRole ?? 0) >= 1)

const {
  items, loading, page, totalPages, totalCount,
  search, source, deleteItem, goToPage,
} = useContentList<Spell>(typeDef)

const columns: ColumnDef[] = [
  { key: 'name', label: 'Name' },
  { key: 'level', label: 'Level', hideOnMobile: true },
  { key: 'school', label: 'School', hideOnMobile: true },
]

function canDelete(item: Spell): boolean {
  if (isTeamMember.value) return true
  return item.sourceCreatorId === authStore.loggedInUser?.id
}

function onNew() {
  router.push('/content/spells/new')
}

function onEdit(item: Spell) {
  if (item.id) router.push(`/content/spells/${item.id}`)
}
</script>

<template>
  <div>
    <div class="content-page__header">
      <RouterLink to="/content" class="content-page__back">
        <i class="fas fa-arrow-left" />
      </RouterLink>
      <h1 class="content-page__title">Spells</h1>
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
      empty-icon="fas fa-hat-wizard"
      empty-title="No spells found"
      empty-subtitle="Try adjusting your search or filters."
      @delete="deleteItem"
    >
      <template #cell-level="{ value }">
        {{ value === 0 ? 'Cantrip' : `Level ${value}` }}
      </template>
      <template #cell-school="{ value }">
        {{ spellSchoolLabels[value as number] ?? '—' }}
      </template>
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
        <button
          class="content-table__action-btn" title="Edit"
          @click="onEdit(item)"
        >
          <i class="fas fa-pen" />
        </button>
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
