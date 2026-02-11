<script setup lang="ts">
import { ref, computed, provide, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTitle } from '@/composables/useTitle'
import { useToast } from '@/composables/useToast'
import { getCharactersId } from '@/api'

const route = useRoute()
const router = useRouter()
const { showToast } = useToast()

const character = ref<any>(null)
const loading = ref(true)

const characterId = computed(() => route.params.id as string)
const currentStage = computed(() => {
  const path = route.path.split('/edit/')[1]
  return path ? path.split('/')[0] : 'configuration'
})

const pageTitle = computed(() => character.value ? `Edit ${character.value.name}` : 'Edit Character')
watch(pageTitle, (title) => useTitle(title), { immediate: true })

const stages = [
  { key: 'configuration', label: 'Configuration', icon: 'fas fa-cog' },
  { key: 'race', label: 'Race', icon: 'fas fa-dna' },
  { key: 'background', label: 'Background', icon: 'fas fa-scroll' },
  { key: 'class', label: 'Class', icon: 'fas fa-shield-halved' },
  { key: 'abilities', label: 'Abilities', icon: 'fas fa-fist-raised' },
  { key: 'details', label: 'Details', icon: 'fas fa-user' },
]

async function fetchCharacter() {
  loading.value = true
  try {
    const res = await getCharactersId(characterId.value)
    character.value = (res as any).data
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load character.' })
    await router.push('/characters')
  } finally {
    loading.value = false
  }
}

function goToStage(stage: string) {
  router.push(`/characters/${characterId.value}/edit/${stage}`)
}

function goToSheet() {
  router.push(`/characters/${characterId.value}`)
}

function goBack() {
  router.push('/characters')
}

onMounted(fetchCharacter)

watch(characterId, fetchCharacter)

provide('character', character)
provide('characterId', characterId)
provide('refreshCharacter', fetchCharacter)
</script>

<template>
  <div class="edit-page">
    <div class="edit-page__header">
      <button class="edit-page__back" @click="goBack">
        <i class="fas fa-arrow-left" />
      </button>
      <h1 class="edit-page__title">{{ character?.name ?? 'Loading...' }}</h1>
      <button class="edit-page__view" @click="goToSheet">
        <i class="fas fa-book-open" /> View Sheet
      </button>
    </div>

    <nav class="edit-page__nav">
      <button
        v-for="stage in stages"
        :key="stage.key"
        class="edit-page__nav-item"
        :class="{ 'is-active': currentStage === stage.key }"
        @click="goToStage(stage.key)"
      >
        <i :class="stage.icon" />
        <span class="edit-page__nav-label">{{ stage.label }}</span>
      </button>
    </nav>

    <div v-if="loading" class="edit-page__loading">
      <i class="fas fa-spinner fa-spin" /> Loading character...
    </div>

    <div v-else class="edit-page__content">
      <RouterView :key="currentStage" />
    </div>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.edit-page {
  max-width: 900px;
  margin: 0 auto;
  padding: 0 $space-4 $space-8;
}

.edit-page__header {
  display: flex;
  align-items: center;
  gap: $space-3;
  margin-bottom: $space-4;
}

.edit-page__back {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;

  &:hover {
    border-color: $color-border-strong;
  }
}

.edit-page__title {
  flex: 1;
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
}

.edit-page__view {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text;
  cursor: pointer;
  font-size: 0.875rem;

  &:hover {
    border-color: $color-border-strong;
  }
}

.edit-page__nav {
  display: flex;
  gap: $space-1;
  margin-bottom: $space-4;
  padding: $space-2;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-lg;
  overflow-x: auto;
}

.edit-page__nav-item {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: none;
  background: transparent;
  color: $color-text-muted;
  cursor: pointer;
  white-space: nowrap;
  transition: all 150ms ease;

  &:hover {
    color: $color-text;
    background: $color-surface-alt;
  }

  &.is-active {
    color: $color-accent;
    background: $color-accent-soft;
  }
}

.edit-page__nav-label {
  @media (max-width: 768px) {
    display: none;
  }
}

.edit-page__loading {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;
}

.edit-page__content {
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-lg;
  padding: $space-4;
}

@media (max-width: 640px) {
  .edit-page {
    padding: 0 $space-3 $space-6;
  }

  .edit-page__nav {
    padding: $space-1;
  }

  .edit-page__nav-item {
    padding: $space-2;
  }
}
</style>
