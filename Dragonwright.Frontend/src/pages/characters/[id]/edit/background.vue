<script setup lang="ts">
import { ref, inject, watch, computed, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'
import { useAutoSave } from '@/composables/useAutoSave'
import { getBackgrounds, getBackgroundsId, getLanguages, putCharactersIdBackground } from '@/api'
import type { Background, Language } from '@/api'
import { sourceLabels, sourceSortOrder, abilityScoreLabels, skillLabels, toolLabels } from '@/content/enums'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'

const { showToast } = useToast()

const character = inject<any>('character')
const characterId = inject<any>('characterId')

const backgrounds = ref<Background[]>([])
const languages = ref<Language[]>([])
const selectedBackground = ref<Background | null>(null)
const loadingBackgrounds = ref(true)
const loadingBackground = ref(false)
const search = ref('')

const chosenAbilityScoreIncreases = ref<Record<string, number>>({})
const chosenLanguages = ref<string[]>([])
const distributionMode = ref<'two' | 'all'>('two')
const plus2Ability = ref<string>('')
const plus1Ability = ref<string>('')

const showPreviewModal = ref(false)
const previewBackground = ref<Background | null>(null)
const loadingPreview = ref(false)

const lockedEdition = computed(() => {
  const raceSource = character.value?.race?.race?.source
  if (raceSource === 0 || raceSource === 1) return raceSource
  return null
})

const lockedEditionLabel = computed(() => lockedEdition.value !== null ? sourceLabels[lockedEdition.value] : '')

const filteredBackgrounds = computed(() => {
  let list = backgrounds.value
  if (lockedEdition.value !== null) {
    list = list.filter(b => b.source === lockedEdition.value || b.source === 2)
  }
  if (search.value) {
    const s = search.value.toLowerCase()
    list = list.filter(b => b.name.toLowerCase().includes(s))
  }
  return [...list].sort((a, b) => {
    const so = (sourceSortOrder[a.source ?? 0] ?? 2) - (sourceSortOrder[b.source ?? 0] ?? 2)
    if (so !== 0) return so
    return a.name.localeCompare(b.name)
  })
})

const languageOptions = computed(() => {
  const restrictions = selectedBackground.value?.languageRestrictions ?? []
  let filtered = languages.value

  if (restrictions.length > 0) {
    filtered = languages.value.filter(l =>
      restrictions.some(r => l.name.toLowerCase().includes(r.toLowerCase()))
    )
  }

  return filtered.map(l => ({ label: l.name, value: l.name }))
})

const autoSaveEnabled = ref(false)

const { isSaving } = useAutoSave({
  source: () => ({
    backgroundId: selectedBackground.value?.id,
    chosenAbilityScoreIncreases: chosenAbilityScoreIncreases.value,
    chosenLanguages: chosenLanguages.value
  }),
  save: saveToBackend,
  delay: 1000,
  enabled: autoSaveEnabled
})

async function loadBackgrounds() {
  loadingBackgrounds.value = true
  try {
    const sources = character.value?.sources ?? [0, 1]
    const [bgRes, langRes] = await Promise.all([
      getBackgrounds({ pageSize: 1000 }),
      getLanguages({ pageSize: 1000 })
    ])
    backgrounds.value = ((bgRes as any).data?.items ?? []).filter((b: Background) =>
      sources.includes(b.source)
    )
    languages.value = (langRes as any).data?.items ?? []
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load backgrounds.' })
  } finally {
    loadingBackgrounds.value = false
  }
}

async function openPreview(bg: Background) {
  loadingPreview.value = true
  showPreviewModal.value = true
  try {
    const res = await getBackgroundsId(bg.id!)
    previewBackground.value = (res as any).data
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load background details.' })
    showPreviewModal.value = false
  } finally {
    loadingPreview.value = false
  }
}

function cancelPreview() {
  showPreviewModal.value = false
  previewBackground.value = null
}

async function confirmBackgroundSelection() {
  if (!previewBackground.value) return

  autoSaveEnabled.value = false
  selectedBackground.value = previewBackground.value

  chosenAbilityScoreIncreases.value = {}
  chosenLanguages.value = []
  distributionMode.value = 'two'
  plus2Ability.value = ''
  plus1Ability.value = ''

  showPreviewModal.value = false
  previewBackground.value = null

  await saveToBackend()
  updateLocalCharacter()
  autoSaveEnabled.value = true
}

function clearBackground() {
  selectedBackground.value = null
  chosenAbilityScoreIncreases.value = {}
  chosenLanguages.value = []
}

const abilityOptions = computed(() => {
  const abilities = selectedBackground.value?.abilityScoreIncreases ?? []
  return abilities.map(a => ({ label: abilityScoreLabels[a] ?? String(a), value: String(a) }))
})

const plus1Options = computed(() => {
  return abilityOptions.value.filter(o => o.value !== plus2Ability.value)
})

function getAbilityIncrease(abilityIndex: number): number {
  return chosenAbilityScoreIncreases.value[String(abilityIndex)] ?? 0
}

function setDistributionMode(mode: 'two' | 'all') {
  distributionMode.value = mode
  const abilities = selectedBackground.value?.abilityScoreIncreases ?? []
  if (mode === 'all') {
    plus2Ability.value = ''
    plus1Ability.value = ''
    const result: Record<string, number> = {}
    for (const a of abilities) {
      result[String(a)] = 1
    }
    chosenAbilityScoreIncreases.value = result
  } else {
    plus2Ability.value = ''
    plus1Ability.value = ''
    chosenAbilityScoreIncreases.value = {}
  }
}

function setPlus2(val: string) {
  plus2Ability.value = val
  if (plus1Ability.value === val) {
    plus1Ability.value = ''
  }
  rebuildTwoMode()
}

function setPlus1(val: string) {
  plus1Ability.value = val
  rebuildTwoMode()
}

function rebuildTwoMode() {
  const result: Record<string, number> = {}
  if (plus2Ability.value) result[plus2Ability.value] = 2
  if (plus1Ability.value) result[plus1Ability.value] = 1
  chosenAbilityScoreIncreases.value = result
}

function addLanguage() {
  const langCount = selectedBackground.value?.languageCount ?? 0
  if (chosenLanguages.value.length < Number(langCount)) {
    chosenLanguages.value = [...chosenLanguages.value, '']
  }
}

function removeLanguage(index: number) {
  chosenLanguages.value = chosenLanguages.value.filter((_, i) => i !== index)
}

function setLanguage(index: number, value: string) {
  const updated = [...chosenLanguages.value]
  updated[index] = value
  chosenLanguages.value = updated
}

async function saveToBackend() {
  if (!characterId?.value) return
  try {
    await putCharactersIdBackground(characterId.value, {
      backgroundId: selectedBackground.value?.id ?? null,
      chosenAbilityScoreIncreases: chosenAbilityScoreIncreases.value,
      chosenLanguages: chosenLanguages.value.filter(l => l),
      chosenCharacteristics: {}
    })
    updateLocalCharacter()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to save background.' })
  }
}

function updateLocalCharacter() {
  if (!character.value) return
  character.value.background = {
    ...character.value.background,
    backgroundId: selectedBackground.value?.id ?? null,
    background: selectedBackground.value,
    chosenAbilityScoreIncreases: { ...chosenAbilityScoreIncreases.value },
    chosenLanguages: [...chosenLanguages.value.filter(l => l)]
  }
}

function loadFromCharacter() {
  if (!character.value?.background) {
    selectedBackground.value = null
    chosenAbilityScoreIncreases.value = {}
    chosenLanguages.value = []
    autoSaveEnabled.value = true
    return
  }

  const cb = character.value.background
  if (cb.background) {
    selectedBackground.value = cb.background
  }
  chosenAbilityScoreIncreases.value = cb.chosenAbilityScoreIncreases ?? {}
  chosenLanguages.value = cb.chosenLanguages ?? []

  const vals = Object.values(chosenAbilityScoreIncreases.value)
  if (vals.length === 3 && vals.every(v => v === 1)) {
    distributionMode.value = 'all'
    plus2Ability.value = ''
    plus1Ability.value = ''
  } else {
    distributionMode.value = 'two'
    plus2Ability.value = ''
    plus1Ability.value = ''
    for (const [k, v] of Object.entries(chosenAbilityScoreIncreases.value)) {
      if (v === 2) plus2Ability.value = k
      else if (v === 1) plus1Ability.value = k
    }
  }

  autoSaveEnabled.value = true
}

watch(() => character.value, loadFromCharacter, { immediate: true })

onMounted(loadBackgrounds)
</script>

<template>
  <div class="bg-stage">
    <div class="bg-stage__header-row">
      <div>
        <h2 class="bg-stage__title">Background Selection</h2>
        <p class="bg-stage__desc">Choose your character's background and configure options.</p>
      </div>
      <span v-if="isSaving" class="bg-stage__saving">
        <i class="fas fa-spinner fa-spin" /> Saving...
      </span>
    </div>

    <div v-if="!selectedBackground" class="bg-stage__selection">
      <div v-if="lockedEdition !== null" class="edition-banner">
        <i class="fas fa-info-circle" />
        Backgrounds are filtered to match your race's edition ({{ lockedEditionLabel }}). Change your race to see other editions.
      </div>

      <UiInput
        v-model="search"
        placeholder="Search backgrounds..."
        class="bg-stage__search"
      />

      <div v-if="loadingBackgrounds" class="bg-stage__loading">
        <i class="fas fa-spinner fa-spin" /> Loading backgrounds...
      </div>

      <div v-else class="bg-stage__grid">
        <button
          v-for="bg in filteredBackgrounds"
          :key="bg.id"
          class="bg-card"
          @click="openPreview(bg)"
        >
          <div class="bg-card__name">{{ bg.name }}</div>
          <div class="bg-card__source">{{ sourceLabels[bg.source ?? 0] }}</div>
        </button>
      </div>

      <div v-if="!loadingBackgrounds && filteredBackgrounds.length === 0" class="bg-stage__empty">
        No backgrounds found matching your search.
      </div>
    </div>

    <div v-else class="bg-stage__details">
      <div class="bg-stage__header">
        <div class="bg-stage__header-info">
          <h3 class="bg-stage__bg-name">{{ selectedBackground.name }}</h3>
          <span class="bg-stage__bg-source">{{ sourceLabels[selectedBackground.source ?? 0] }}</span>
        </div>
        <button class="bg-stage__change" @click="clearBackground">
          <i class="fas fa-exchange-alt" /> Change Background
        </button>
      </div>

      <div v-if="loadingBackground" class="bg-stage__loading">
        <i class="fas fa-spinner fa-spin" /> Loading background details...
      </div>

      <div v-else class="bg-stage__content">
        <div v-if="selectedBackground.skillProficiencies?.length" class="bg-section">
          <h4 class="bg-section__title">Skill Proficiencies</h4>
          <div class="bg-section__tags">
            <span v-for="skill in selectedBackground.skillProficiencies" :key="skill" class="bg-tag">
              {{ skillLabels[skill] }}
            </span>
          </div>
        </div>

        <div v-if="selectedBackground.toolProficiencies?.length" class="bg-section">
          <h4 class="bg-section__title">Tool Proficiencies</h4>
          <div class="bg-section__tags">
            <span v-for="tool in selectedBackground.toolProficiencies" :key="tool" class="bg-tag">
              {{ toolLabels[tool] }}
            </span>
          </div>
        </div>

        <div v-if="selectedBackground.abilityScoreIncreases?.length" class="bg-section">
          <h4 class="bg-section__title">Ability Score Increases</h4>
          <div class="distribution-toggle">
            <button
              class="distribution-toggle__btn"
              :class="{ 'is-active': distributionMode === 'two' }"
              @click="setDistributionMode('two')"
            >
              Two Abilities (+2 / +1)
            </button>
            <button
              class="distribution-toggle__btn"
              :class="{ 'is-active': distributionMode === 'all' }"
              @click="setDistributionMode('all')"
            >
              All Three (+1 each)
            </button>
          </div>

          <div v-if="distributionMode === 'two'" class="ability-selects">
            <div class="ability-select-row">
              <label class="ability-select-row__label">+2 to:</label>
              <UiSelect
                :model-value="plus2Ability"
                :options="abilityOptions"
                placeholder="Choose ability..."
                @update:model-value="setPlus2(String($event))"
              />
            </div>
            <div class="ability-select-row">
              <label class="ability-select-row__label">+1 to:</label>
              <UiSelect
                :model-value="plus1Ability"
                :options="plus1Options"
                placeholder="Choose ability..."
                @update:model-value="setPlus1(String($event))"
              />
            </div>
          </div>

          <div v-else class="ability-all-display">
            <div
              v-for="ability in selectedBackground.abilityScoreIncreases"
              :key="ability"
              class="ability-all-item"
            >
              <span class="ability-all-item__label">{{ abilityScoreLabels[ability] }}</span>
              <span class="ability-all-item__value">+1</span>
            </div>
          </div>
        </div>

        <div v-if="selectedBackground.languageCount && Number(selectedBackground.languageCount) > 0" class="bg-section">
          <h4 class="bg-section__title">Languages</h4>
          <p class="bg-section__hint">
            Choose {{ selectedBackground.languageCount }} language(s).
          </p>
          <div class="language-list">
            <div v-for="(lang, index) in chosenLanguages" :key="index" class="language-row">
              <UiSelect
                :model-value="lang"
                :options="languageOptions"
                placeholder="Select language..."
                @update:model-value="setLanguage(index, String($event))"
              />
              <button class="language-row__remove" @click="removeLanguage(index)">
                <i class="fas fa-times" />
              </button>
            </div>
            <button
              v-if="chosenLanguages.length < Number(selectedBackground.languageCount ?? 0)"
              class="language-add"
              @click="addLanguage"
            >
              <i class="fas fa-plus" /> Add Language
            </button>
          </div>
        </div>
      </div>
    </div>

    <UiModal v-model="showPreviewModal" :title="previewBackground?.name ?? 'Background Preview'" :close-on-backdrop="true" :close-on-esc="true" class="preview-modal">
      <div v-if="loadingPreview" class="preview-loading">
        <i class="fas fa-spinner fa-spin" /> Loading background details...
      </div>

      <div v-else-if="previewBackground" class="preview-content">
        <div class="preview-header">
          <span class="preview-source">{{ sourceLabels[previewBackground.source ?? 0] }}</span>
        </div>

        <div v-if="(previewBackground as any).description" class="preview-section">
          <p class="preview-description">{{ (previewBackground as any).description }}</p>
        </div>

        <div class="preview-section">
          <h4 class="preview-section__title">You will gain:</h4>
          <ul class="preview-gains">
            <li v-if="previewBackground.skillProficiencies?.length">
              <strong>Skill Proficiencies:</strong>
              {{ previewBackground.skillProficiencies.map(s => skillLabels[s]).join(', ') }}
            </li>
            <li v-if="previewBackground.toolProficiencies?.length">
              <strong>Tool Proficiencies:</strong>
              {{ previewBackground.toolProficiencies.map(t => toolLabels[t]).join(', ') }}
            </li>
            <li v-if="previewBackground.abilityScoreIncreases?.length">
              <strong>Ability Score Increases:</strong>
              {{ previewBackground.abilityScoreIncreases.map(a => abilityScoreLabels[a]).join(', ') }}
              (choose how to distribute)
            </li>
            <li v-if="previewBackground.languageCount && Number(previewBackground.languageCount) > 0">
              <strong>Languages:</strong>
              {{ previewBackground.languageCount }} of your choice
            </li>
          </ul>
        </div>

        <div v-if="previewBackground.characteristics?.length" class="preview-section preview-section--note">
          <p class="preview-note">
            <i class="fas fa-info-circle" />
            This background includes suggested personality traits, ideals, bonds, and flaws.
            You can choose from these in the Details page.
          </p>
        </div>
      </div>

      <template #footer>
        <div class="modal-actions">
          <button class="modal-btn modal-btn--secondary" @click="cancelPreview">Cancel</button>
          <UiButton @click="confirmBackgroundSelection">
            Select {{ previewBackground?.name }}
          </UiButton>
        </div>
      </template>
    </UiModal>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.bg-stage__header-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: $space-4;
}

.bg-stage__title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0 0 $space-1 0;
}

.bg-stage__desc {
  color: $color-text-muted;
  margin: 0;
}

.bg-stage__saving {
  font-size: 0.875rem;
  color: $color-text-muted;
}

.edition-banner {
  display: flex;
  align-items: flex-start;
  gap: $space-2;
  padding: $space-3;
  margin-bottom: $space-4;
  background: rgba($color-accent, 0.05);
  border: 1px solid rgba($color-accent, 0.2);
  border-radius: $radius-md;
  font-size: 0.875rem;
  color: $color-text-muted;
  line-height: 1.4;

  i { color: $color-accent; margin-top: 2px; }
}

.bg-stage__search {
  margin-bottom: $space-4;
}

.bg-stage__loading {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;
}

.bg-stage__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: $space-3;
}

.bg-card {
  padding: $space-3;
  color: $color-text;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  cursor: pointer;
  text-align: left;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-accent;
    background: rgba($color-accent, 0.05);
  }
}

.bg-card__name {
  font-weight: 600;
  margin-bottom: $space-1;
}

.bg-card__source {
  font-size: 0.75rem;
  color: $color-text-muted;
}

.bg-stage__empty {
  text-align: center;
  padding: $space-6;
  color: $color-text-muted;
}

.bg-stage__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-3;
  margin-bottom: $space-4;
  padding-bottom: $space-3;
  border-bottom: 1px solid $color-border-subtle;
}

.bg-stage__bg-name {
  font-size: 1.125rem;
  font-weight: 600;
  margin: 0;
}

.bg-stage__bg-source {
  font-size: 0.75rem;
  color: $color-text-muted;
}

.bg-stage__change {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2 $space-3;
  background: transparent;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  color: $color-text-muted;
  cursor: pointer;
  font-size: 0.875rem;

  &:hover {
    border-color: $color-border-strong;
    color: $color-text;
  }
}

.bg-stage__content {
  display: flex;
  flex-direction: column;
  gap: $space-4;
}

.bg-section {
  padding: $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
}

.bg-section__title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
  color: $color-text;
}

.bg-section__hint {
  font-size: 0.875rem;
  color: $color-text-muted;
  margin: 0 0 $space-3 0;
}

.bg-section__tags {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
}

.bg-tag {
  padding: $space-1 $space-2;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  font-size: 0.875rem;
  color: $color-text;
}

.distribution-toggle {
  display: flex;
  gap: $space-2;
  margin-bottom: $space-3;
}

.distribution-toggle__btn {
  flex: 1;
  padding: $space-2 $space-3;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  color: $color-text-muted;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-border-strong;
    color: $color-text;
  }

  &.is-active {
    border-color: $color-accent;
    background: rgba($color-accent, 0.05);
    color: $color-accent;
    font-weight: 600;
  }
}

.ability-selects {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.ability-select-row {
  display: flex;
  align-items: center;
  gap: $space-3;
}

.ability-select-row__label {
  font-size: 0.875rem;
  font-weight: 600;
  min-width: 3.5rem;
  color: $color-text;
}

.ability-all-display {
  display: flex;
  flex-wrap: wrap;
  gap: $space-3;
}

.ability-all-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: $space-1;
  padding: $space-2 $space-3;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  min-width: 100px;
}

.ability-all-item__label {
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  color: $color-text-muted;
}

.ability-all-item__value {
  font-size: 1.125rem;
  font-weight: 600;
  color: $color-accent;
}

.language-list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.language-row {
  display: flex;
  gap: $space-2;
  align-items: center;
}

.language-row__remove {
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  color: $color-text-muted;

  &:hover {
    border-color: $color-danger;
    color: $color-danger;
  }
}

.language-add {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2 $space-3;
  background: transparent;
  border: 1px dashed $color-border-subtle;
  border-radius: $radius-md;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.875rem;
  width: fit-content;

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.preview-modal :deep(.modal) {
  max-width: 600px;
}

.preview-loading {
  text-align: center;
  padding: $space-6;
  color: $color-text-muted;
}

.preview-content {
  display: flex;
  flex-direction: column;
  gap: $space-4;
}

.preview-header {
  display: flex;
  justify-content: flex-end;
}

.preview-source {
  font-size: 0.75rem;
  color: $color-text-muted;
  padding: $space-1 $space-2;
  background: $color-surface-alt;
  border-radius: $radius-sm;
}

.preview-description {
  margin: 0;
  color: $color-text;
  line-height: 1.5;
  font-size: 0.875rem;
}

.preview-section {
  padding-top: $space-3;
  border-top: 1px solid $color-border-subtle;

  &--note {
    background: rgba($color-accent, 0.05);
    padding: $space-3;
    border-radius: $radius-md;
    border: 1px solid rgba($color-accent, 0.2);
  }
}

.preview-section__title {
  font-size: 0.875rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
  color: $color-text;
}

.preview-gains {
  margin: 0;
  padding-left: $space-4;
  color: $color-text;
  font-size: 0.875rem;
  line-height: 1.6;

  li {
    margin-bottom: $space-1;
  }

  strong {
    color: $color-text;
  }
}

.preview-note {
  margin: 0;
  font-size: 0.875rem;
  color: $color-text-muted;
  display: flex;
  align-items: flex-start;
  gap: $space-2;

  i {
    color: $color-accent;
    margin-top: 2px;
  }
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: $space-2;
}

.modal-btn {
  padding: $space-2 $space-4;
  border-radius: $radius-md;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 150ms ease;

  &--secondary {
    background: transparent;
    border: 1px solid $color-border-subtle;
    color: $color-text-muted;

    &:hover {
      border-color: $color-border-strong;
      color: $color-text;
    }
  }
}
</style>
