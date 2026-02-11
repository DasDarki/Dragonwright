<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useTitle } from '@/composables/useTitle'
import { useToast } from '@/composables/useToast'
import { useConfirm } from '@/composables/useConfirm'
import { getCharacters, postCharacters, deleteCharactersId } from '@/api'
import UiGrid from '@/components/ui/layout/UiGrid.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import Avatar from "@/components/characters/Avatar.vue";

useTitle('My Characters')

const router = useRouter()
const { showToast } = useToast()
const { confirm } = useConfirm()

const characters = ref<any[]>([])
const loading = ref(false)
const search = ref('')
const page = ref(1)
const totalCount = ref(0)
const pageSize = 20

const creating = ref(false)

const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize)))

async function fetchCharacters() {
  loading.value = true
  try {
    const params: any = { page: page.value, pageSize }
    if (search.value) params.search = search.value
    const res = await getCharacters(params)
    const body = (res as any).data
    if (body && 'items' in body) {
      characters.value = body.items
      totalCount.value = body.totalCount
    } else {
      characters.value = []
      totalCount.value = 0
    }
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load characters.' })
  } finally {
    loading.value = false
  }
}

let searchTimeout: ReturnType<typeof setTimeout> | null = null
watch(search, () => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    page.value = 1
    fetchCharacters()
  }, 300)
})

watch(page, fetchCharacters)
fetchCharacters()

function openSheet(id: string) {
  router.push(`/characters/${id}`)
}

function openEdit(id: string) {
  router.push(`/characters/${id}/edit/configuration`)
}

async function deleteCharacter(char: any) {
  const ok = await confirm({
    title: 'Delete Character',
    message: `Are you sure you want to delete "${char.name}"? This action cannot be undone.`,
    confirmText: 'Delete',
    cancelText: 'Cancel',
    danger: true,
  })
  if (!ok) return
  try {
    await deleteCharactersId(char.id)
    showToast({ variant: 'success', message: 'Character deleted.' })
    await fetchCharacters()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to delete character.' })
  }
}

async function createCharacter() {
  creating.value = true
  try {
    const res = await postCharacters({ name: "Unnamed Hero" })
    const data = (res as any).data
    showToast({ variant: 'success', message: 'Character created!' })
    if (data?.id) {
      await router.push(`/characters/${data.id}/edit/configuration`)
    } else {
      await fetchCharacters()
    }
  } catch {
    showToast({ variant: 'danger', message: 'Failed to create character.' })
  } finally {
    creating.value = false
  }
}

function getClassSummary(char: any): string {
  if (!char.classes || char.classes.length === 0) return 'No class'
  return char.classes
    .map((c: any) => `${c.class?.name ?? 'Unknown'} ${c.level}`)
    .join(' / ')
}

function getRaceName(char: any): string {
  return char.race?.race?.name ?? 'No race'
}
</script>

<template>
  <div class="characters-page">
    <div class="characters-page__header">
      <h1 class="characters-page__title">My Characters</h1>
      <UiButton @click="createCharacter" :loading="creating">
        <i class="fas fa-plus" /> New Character
      </UiButton>
    </div>

    <div class="characters-page__search">
      <UiInput
        v-model="search"
        placeholder="Search characters..."
        left-icon="fas fa-search"
      />
    </div>

    <div v-if="loading && characters.length === 0" class="characters-page__loading">
      <i class="fas fa-spinner fa-spin" /> Loading...
    </div>

    <UiEmptyState
      v-else-if="characters.length === 0"
      icon="fas fa-user-slash"
      title="No characters yet"
      subtitle="Create your first character to get started!"
    />

    <UiGrid v-else :cols="1" :cols-md="2" :cols-lg="3" :gap="1">
      <div
        v-for="char in characters"
        :key="char.id"
        class="character-card"
      >
        <Avatar :character="char" @click="openSheet(char.id)" />

        <div class="character-card__content" @click="openSheet(char.id)">
          <h3 class="character-card__name">{{ char.name }}</h3>
          <p class="character-card__meta">{{ getRaceName(char) }}</p>
          <p class="character-card__meta">{{ getClassSummary(char) }}</p>
          <p v-if="char.level > 0" class="character-card__level">Level {{ char.level }}</p>
        </div>

        <div class="character-card__actions">
          <button class="character-card__action" title="View Sheet" @click="openSheet(char.id)">
            <i class="fas fa-book-open" />
          </button>
          <button class="character-card__action" title="Edit" @click="openEdit(char.id)">
            <i class="fas fa-pen" />
          </button>
          <button class="character-card__action character-card__action--danger" title="Delete" @click="deleteCharacter(char)">
            <i class="fas fa-trash" />
          </button>
        </div>
      </div>
    </UiGrid>

    <div v-if="totalPages > 1" class="characters-page__pagination">
      <button :disabled="page <= 1" @click="page--">Previous</button>
      <span>Page {{ page }} of {{ totalPages }}</span>
      <button :disabled="page >= totalPages" @click="page++">Next</button>
    </div>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.characters-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 $space-4;
}

.characters-page__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-4;
  margin-bottom: $space-4;
  flex-wrap: wrap;
}

.characters-page__title {
  font-size: 1.75rem;
  font-weight: 700;
  color: $color-text;
  margin: 0;
}

.characters-page__search {
  margin-bottom: $space-4;
  max-width: 400px;
}

.characters-page__loading {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;
}

.characters-page__pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: $space-4;
  margin-top: $space-6;
  color: $color-text-muted;

  button {
    padding: $space-2 $space-4;
    border-radius: $radius-md;
    border: 1px solid $color-border-subtle;
    background: $color-surface;
    color: $color-text;
    cursor: pointer;

    &:hover:not(:disabled) {
      border-color: $color-border-strong;
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }
}

.character-card {
  display: flex;
  align-items: center;
  gap: $space-3;
  padding: $space-4;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-lg;
  transition: border-color 150ms ease, box-shadow 150ms ease;

  &:hover {
    border-color: $color-border-strong;
    box-shadow: $shadow-card;
  }
}

.character-card__content {
  flex: 1;
  min-width: 0;
  cursor: pointer;
}

.character-card__name {
  font-size: 1.1rem;
  font-weight: 600;
  color: $color-text;
  margin: 0 0 $space-1 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.character-card__meta {
  font-size: 0.875rem;
  color: $color-text-muted;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.character-card__level {
  font-size: 0.8rem;
  color: $color-accent;
  margin: $space-1 0 0 0;
  font-weight: 600;
}

.character-card__actions {
  display: flex;
  flex-direction: column;
  gap: $space-1;
}

.character-card__action {
  width: 2rem;
  height: 2rem;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text-muted;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-border-strong;
    color: $color-text;
  }

  &--danger:hover {
    border-color: $color-danger;
    color: $color-danger;
    background: rgba(239, 68, 68, 0.08);
  }
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: $space-2;
  margin-top: $space-4;
}

@media (max-width: 640px) {
  .characters-page {
    padding: 0 $space-3;
  }

  .character-card {
    flex-wrap: wrap;
  }

  .character-card__actions {
    flex-direction: row;
    width: 100%;
    justify-content: flex-end;
    padding-top: $space-2;
    border-top: 1px solid $color-border-subtle;
    margin-top: $space-2;
  }
}
</style>
