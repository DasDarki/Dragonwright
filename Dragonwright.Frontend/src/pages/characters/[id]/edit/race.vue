<script setup lang="ts">
import { ref, inject, watch, computed, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'
import { useAutoSave } from '@/composables/useAutoSave'
import { getRaces, getRacesId, putCharactersIdRace, getLanguages } from '@/api'
import type { Race, RaceTrait, RaceTraitOption, Language } from '@/api'
import { sourceLabels, sourceSortOrder, abilityScoreLabels, abilityScoreOptions } from '@/content/enums'
import { useImageUrl } from '@/composables/useFileUpload'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'

const { showToast } = useToast()

const character = inject<any>('character')
const characterId = inject<any>('characterId')

const races = ref<Race[]>([])
const selectedRace = ref<Race | null>(null)
const loadingRaces = ref(true)
const loadingRace = ref(false)
const search = ref('')

const chosenTraitOptions = ref<Record<string, string[]>>({})
const chosenSpells = ref<Record<string, string[]>>({})
const raceTraitUsages = ref<Record<string, number | string>>({})
const languages = ref<Language[]>([])

const isCustomizeOriginActive = computed(() =>
  character.value?.customizeOrigin && selectedRace.value?.source === 0
)

const showPreviewModal = ref(false)
const previewRace = ref<Race | null>(null)
const loadingPreview = ref(false)

const lockedEdition = computed(() => {
  const bgSource = character.value?.background?.background?.source
  if (bgSource === 0 || bgSource === 1) return bgSource
  return null
})

const lockedEditionLabel = computed(() => lockedEdition.value !== null ? sourceLabels[lockedEdition.value] : '')

const filteredRaces = computed(() => {
  let list = races.value
  if (lockedEdition.value !== null) {
    list = list.filter(r => r.source === lockedEdition.value || r.source === 2)
  }
  if (search.value) {
    const s = search.value.toLowerCase()
    list = list.filter(r => r.name.toLowerCase().includes(s))
  }
  return [...list].sort((a, b) => {
    const so = (sourceSortOrder[a.source ?? 0] ?? 2) - (sourceSortOrder[b.source ?? 0] ?? 2)
    if (so !== 0) return so
    return a.name.localeCompare(b.name)
  })
})

const visibleTraits = computed(() => {
  if (!selectedRace.value?.traits) return []
  return selectedRace.value.traits
    .filter(t => !t.hideInBuilder)
    .sort((a, b) => Number(a.displayOrder ?? 0) - Number(b.displayOrder ?? 0))
})

const previewTraits = computed(() => {
  if (!previewRace.value?.traits) return []
  return previewRace.value.traits
    .filter(t => !t.hideInBuilder)
    .sort((a, b) => Number(a.displayOrder ?? 0) - Number(b.displayOrder ?? 0))
})

const autoSaveEnabled = ref(false)

const { isSaving } = useAutoSave({
  source: () => ({
    raceId: selectedRace.value?.id,
    chosenTraitOptions: chosenTraitOptions.value,
    chosenSpells: chosenSpells.value,
    raceTraitUsages: raceTraitUsages.value
  }),
  save: saveToBackend,
  delay: 1000,
  enabled: autoSaveEnabled
})

async function loadRaces() {
  loadingRaces.value = true
  try {
    const sources = character.value?.sources ?? [0, 1]
    const res = await getRaces({ pageSize: 1000 })
    races.value = ((res as any).data?.items ?? []).filter((r: Race) =>
      sources.includes(r.source)
    )
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load races.' })
  } finally {
    loadingRaces.value = false
  }
}

async function openPreview(race: Race) {
  loadingPreview.value = true
  showPreviewModal.value = true
  try {
    const res = await getRacesId(race.id!)
    previewRace.value = (res as any).data
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load race details.' })
    showPreviewModal.value = false
  } finally {
    loadingPreview.value = false
  }
}

function cancelPreview() {
  showPreviewModal.value = false
  previewRace.value = null
}

async function confirmRaceSelection() {
  if (!previewRace.value) return

  autoSaveEnabled.value = false
  selectedRace.value = previewRace.value

  chosenTraitOptions.value = {}
  chosenSpells.value = {}
  raceTraitUsages.value = {}

  if (selectedRace.value?.traits) {
    for (const trait of selectedRace.value.traits) {
      if (trait.options?.length) {
        const granted = trait.options.filter(o => o.isGranted).map(o => o.id!)
        if (granted.length) {
          chosenTraitOptions.value[trait.id!] = granted
        }
      }
    }
  }

  showPreviewModal.value = false
  previewRace.value = null

  await saveToBackend()
  updateLocalCharacter()
  autoSaveEnabled.value = true
}

function clearRace() {
  selectedRace.value = null
  chosenTraitOptions.value = {}
  chosenSpells.value = {}
  raceTraitUsages.value = {}
}

function toggleTraitOption(traitId: string, optionId: string, trait: RaceTrait) {
  const current = chosenTraitOptions.value[traitId] ?? []
  const hasOption = trait.options?.some(o => o.id === optionId && o.isGranted)

  if (hasOption) return

  if (current.includes(optionId)) {
    chosenTraitOptions.value[traitId] = current.filter(id => id !== optionId)
  } else {
    chosenTraitOptions.value[traitId] = [...current, optionId]
  }
  chosenTraitOptions.value = { ...chosenTraitOptions.value }
}

function isOptionSelected(traitId: string, optionId: string): boolean {
  return (chosenTraitOptions.value[traitId] ?? []).includes(optionId)
}

function isOptionDisabled(option: RaceTraitOption, trait: RaceTrait): boolean {
  if (option.isGranted) return true
  if (option.requiredOptionId) {
    const parentSelected = (chosenTraitOptions.value[trait.id!] ?? []).includes(option.requiredOptionId)
    if (!parentSelected) return true
  }
  return false
}

interface AbilityScoreModifierInfo {
  traitName: string
  modifierId: string
  originalAbility: number
  fixedValue: number
}

interface LanguageModifierInfo {
  traitName: string
  modifierId: string
  originalLanguage: string
}

function getAbilityScoreModifiers(): AbilityScoreModifierInfo[] {
  if (!selectedRace.value?.traits) return []
  const result: AbilityScoreModifierInfo[] = []
  for (const trait of selectedRace.value.traits) {
    for (const mod of trait.modifiers ?? []) {
      if (mod.type === 0 && mod.subtype?.target === 0) {
        result.push({
          traitName: trait.name ?? '',
          modifierId: mod.id!,
          originalAbility: mod.abilityScore ?? 0,
          fixedValue: mod.fixedValue ?? 0
        })
      }
    }
  }
  return result
}

function getLanguageModifiers(): LanguageModifierInfo[] {
  if (!selectedRace.value?.traits) return []
  const result: LanguageModifierInfo[] = []
  for (const trait of selectedRace.value.traits) {
    for (const mod of trait.modifiers ?? []) {
      if (mod.type === 12) {
        result.push({
          traitName: trait.name ?? '',
          modifierId: mod.id!,
          originalLanguage: mod.language ?? ''
        })
      }
    }
  }
  return result
}

const languageOptions = computed(() =>
  languages.value.map(l => ({ label: l.name!, value: l.name! }))
)

function getChosenAbility(modifierId: string, originalAbility: number): number {
  const val = raceTraitUsages.value[modifierId]
  return val !== undefined ? Number(val) : originalAbility
}

function setChosenAbility(modifierId: string, value: number | string) {
  raceTraitUsages.value = { ...raceTraitUsages.value, [modifierId]: Number(value) }
}

function getChosenLanguage(modifierId: string, originalLanguage: string): string {
  const val = raceTraitUsages.value[modifierId]
  return val !== undefined ? String(val) : originalLanguage
}

function setChosenLanguage(modifierId: string, value: string) {
  raceTraitUsages.value = { ...raceTraitUsages.value, [modifierId]: value }
}

function getUsedAbilities(excludeModifierId: string): number[] {
  const mods = getAbilityScoreModifiers()
  const used: number[] = []
  for (const m of mods) {
    if (m.modifierId === excludeModifierId) continue
    used.push(getChosenAbility(m.modifierId, m.originalAbility))
  }
  return used
}

function getAvailableAbilityOptions(modifierId: string) {
  const used = getUsedAbilities(modifierId)
  return abilityScoreOptions.map(o => ({
    ...o,
    label: used.includes(Number(o.value)) ? `${o.label} (taken)` : o.label
  }))
}

const hasDuplicateAbilities = computed(() => {
  const mods = getAbilityScoreModifiers()
  const chosen = mods.map(m => getChosenAbility(m.modifierId, m.originalAbility))
  return new Set(chosen).size !== chosen.length
})

async function loadLanguages() {
  try {
    const res = await getLanguages({ pageSize: 1000 })
    languages.value = (res as any).data?.items ?? []
  } catch {
    /* ignore */
  }
}

async function saveToBackend() {
  if (!characterId?.value) return
  try {
    await putCharactersIdRace(characterId.value, {
      raceId: selectedRace.value?.id ?? null,
      chosenTraitOptions: chosenTraitOptions.value,
      chosenSpells: chosenSpells.value,
      raceTraitUsages: raceTraitUsages.value
    })
    updateLocalCharacter()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to save race.' })
  }
}

function updateLocalCharacter() {
  if (!character.value) return
  character.value.race = {
    ...character.value.race,
    raceId: selectedRace.value?.id ?? null,
    race: selectedRace.value,
    chosenTraitOptions: { ...chosenTraitOptions.value },
    chosenSpells: { ...chosenSpells.value },
    raceTraitUsages: { ...raceTraitUsages.value }
  }
}

function loadFromCharacter() {
  if (!character.value?.race) {
    selectedRace.value = null
    chosenTraitOptions.value = {}
    chosenSpells.value = {}
    raceTraitUsages.value = {}
    autoSaveEnabled.value = true
    return
  }

  const cr = character.value.race
  if (cr.race) {
    selectedRace.value = cr.race
  }
  chosenTraitOptions.value = cr.chosenTraitOptions ?? {}
  chosenSpells.value = cr.chosenSpells ?? {}
  raceTraitUsages.value = cr.raceTraitUsages ?? {}
  autoSaveEnabled.value = true
}

watch(() => character.value, loadFromCharacter, { immediate: true })

onMounted(() => {
  loadRaces()
  loadLanguages()
})
</script>

<template>
  <div class="race-stage">
    <div class="race-stage__header-row">
      <div>
        <h2 class="race-stage__title">Race Selection</h2>
        <p class="race-stage__desc">Choose your character's race and configure trait options.</p>
      </div>
      <span v-if="isSaving" class="race-stage__saving">
        <i class="fas fa-spinner fa-spin" /> Saving...
      </span>
    </div>

    <div v-if="!selectedRace" class="race-stage__selection">
      <div v-if="lockedEdition !== null" class="edition-banner">
        <i class="fas fa-info-circle" />
        Races are filtered to match your background's edition ({{ lockedEditionLabel }}). Change your background to see other editions.
      </div>

      <UiInput
        v-model="search"
        placeholder="Search races..."
        class="race-stage__search"
      />

      <div v-if="loadingRaces" class="race-stage__loading">
        <i class="fas fa-spinner fa-spin" /> Loading races...
      </div>

      <div v-else class="race-stage__grid">
        <button
          v-for="race in filteredRaces"
          :key="race.id"
          class="race-card"
          @click="openPreview(race)"
        >
          <img v-if="useImageUrl(race.image)" :src="useImageUrl(race.image)!" class="race-card__img" alt="" />
          <div class="race-card__name">{{ race.name }}</div>
          <div class="race-card__source">{{ sourceLabels[race.source ?? 0] }}</div>
        </button>
      </div>

      <div v-if="!loadingRaces && filteredRaces.length === 0" class="race-stage__empty">
        No races found matching your search.
      </div>
    </div>

    <div v-else class="race-stage__details">
      <div class="race-stage__header">
        <div class="race-stage__header-info">
          <img v-if="useImageUrl(selectedRace.image)" :src="useImageUrl(selectedRace.image)!" class="race-stage__header-img" alt="" />
          <div>
            <h3 class="race-stage__race-name">{{ selectedRace.name }}</h3>
            <span class="race-stage__race-source">{{ sourceLabels[selectedRace.source ?? 0] }}</span>
          </div>
        </div>
        <button class="race-stage__change" @click="clearRace">
          <i class="fas fa-exchange-alt" /> Change Race
        </button>
      </div>

      <div v-if="loadingRace" class="race-stage__loading">
        <i class="fas fa-spinner fa-spin" /> Loading race details...
      </div>

      <div v-else class="race-stage__traits">
        <div v-for="trait in visibleTraits" :key="trait.id" class="trait-card">
          <div class="trait-card__header">
            <h4 class="trait-card__name">{{ trait.name }}</h4>
          </div>
          <p v-if="trait.description" class="trait-card__desc">{{ trait.description }}</p>

          <div v-if="trait.options?.length" class="trait-card__options">
            <div class="trait-card__options-label">Options:</div>
            <div class="trait-card__options-list">
              <button
                v-for="option in trait.options"
                :key="option.id"
                class="trait-option"
                :class="{
                  'is-selected': isOptionSelected(trait.id!, option.id!),
                  'is-disabled': isOptionDisabled(option, trait),
                  'is-granted': option.isGranted
                }"
                :disabled="isOptionDisabled(option, trait)"
                @click="toggleTraitOption(trait.id!, option.id!, trait)"
              >
                <span class="trait-option__check">
                  <i v-if="isOptionSelected(trait.id!, option.id!)" class="fas fa-check" />
                </span>
                <span class="trait-option__content">
                  <span class="trait-option__name">{{ option.name }}</span>
                  <span v-if="option.isGranted" class="trait-option__granted">(Granted)</span>
                  <span v-if="option.description" class="trait-option__desc">{{ option.description }}</span>
                </span>
              </button>
            </div>
          </div>

          <div v-if="trait.spellList?.length" class="trait-card__spells">
            <div class="trait-card__options-label">Spells:</div>
            <div class="trait-card__spell-list">
              <span v-for="spell in trait.spellList" :key="spell.id" class="trait-spell">
                {{ spell.name }}
              </span>
            </div>
          </div>
        </div>

        <div v-if="isCustomizeOriginActive && getAbilityScoreModifiers().length > 0" class="customize-origin-section">
          <h4 class="customize-origin-section__title">
            <i class="fas fa-exchange-alt" /> Customize Origin — Ability Scores
          </h4>
          <div class="customize-origin-section__banner">
            <i class="fas fa-info-circle" />
            Customize Your Origin is enabled. You may reassign your racial ability score bonuses.
          </div>
          <div v-if="hasDuplicateAbilities" class="customize-origin-section__warning">
            <i class="fas fa-exclamation-triangle" />
            Two bonuses target the same ability. Choose different abilities.
          </div>
          <div class="customize-origin-section__list">
            <div
              v-for="mod in getAbilityScoreModifiers()"
              :key="mod.modifierId"
              class="customize-origin-row"
            >
              <span class="customize-origin-row__bonus">+{{ mod.fixedValue }}</span>
              <UiSelect
                :model-value="getChosenAbility(mod.modifierId, mod.originalAbility)"
                :options="getAvailableAbilityOptions(mod.modifierId)"
                @update:model-value="setChosenAbility(mod.modifierId, $event as number)"
              />
              <span class="customize-origin-row__original">
                (originally {{ abilityScoreLabels[mod.originalAbility] }})
              </span>
            </div>
          </div>
        </div>

        <div v-if="isCustomizeOriginActive && getLanguageModifiers().length > 0" class="customize-origin-section">
          <h4 class="customize-origin-section__title">
            <i class="fas fa-language" /> Customize Origin — Languages
          </h4>
          <div class="customize-origin-section__list">
            <div
              v-for="mod in getLanguageModifiers()"
              :key="mod.modifierId"
              class="customize-origin-row"
            >
              <UiSelect
                :model-value="getChosenLanguage(mod.modifierId, mod.originalLanguage)"
                :options="languageOptions"
                placeholder="Choose language..."
                @update:model-value="setChosenLanguage(mod.modifierId, String($event))"
              />
              <span class="customize-origin-row__original">
                (originally {{ mod.originalLanguage }})
              </span>
            </div>
          </div>
        </div>

        <div v-if="visibleTraits.length === 0 && !(isCustomizeOriginActive && (getAbilityScoreModifiers().length > 0 || getLanguageModifiers().length > 0))" class="race-stage__no-traits">
          This race has no configurable traits.
        </div>
      </div>
    </div>

    <UiModal v-model="showPreviewModal" :title="previewRace?.name ?? 'Race Preview'" :close-on-backdrop="true" :close-on-esc="true" class="preview-modal">
      <div v-if="loadingPreview" class="preview-loading">
        <i class="fas fa-spinner fa-spin" /> Loading race details...
      </div>

      <div v-else-if="previewRace" class="preview-content">
        <img v-if="useImageUrl(previewRace.image)" :src="useImageUrl(previewRace.image)!" class="preview-img" alt="" />
        <div class="preview-header">
          <span class="preview-source">{{ sourceLabels[previewRace.source ?? 0] }}</span>
        </div>

        <div v-if="(previewRace as any).description" class="preview-section">
          <p class="preview-description">{{ (previewRace as any).description }}</p>
        </div>

        <div v-if="previewTraits.length" class="preview-section">
          <h4 class="preview-section__title">Traits</h4>
          <div class="preview-traits">
            <div v-for="trait in previewTraits" :key="trait.id" class="preview-trait">
              <strong>{{ trait.name }}:</strong>
              <span v-if="trait.description">{{ trait.description }}</span>
              <span v-if="trait.options?.length" class="preview-trait__options">
                ({{ trait.options.length }} options to choose from)
              </span>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="modal-actions">
          <button class="modal-btn modal-btn--secondary" @click="cancelPreview">Cancel</button>
          <UiButton @click="confirmRaceSelection">
            Select {{ previewRace?.name }}
          </UiButton>
        </div>
      </template>
    </UiModal>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.race-stage__header-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: $space-4;
}

.race-stage__title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0 0 $space-1 0;
}

.race-stage__desc {
  color: $color-text-muted;
  margin: 0;
}

.race-stage__saving {
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

.race-stage__search {
  margin-bottom: $space-4;
}

.race-stage__loading {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;
}

.race-stage__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: $space-3;
}

.race-card {
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

.race-card__img {
  width: 48px;
  height: 48px;
  border-radius: $radius-md;
  object-fit: cover;
  margin-bottom: $space-2;
}

.race-card__name {
  font-weight: 600;
  margin-bottom: $space-1;
}

.race-card__source {
  font-size: 0.75rem;
  color: $color-text-muted;
}

.race-stage__empty {
  text-align: center;
  padding: $space-6;
  color: $color-text-muted;
}

.race-stage__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-3;
  margin-bottom: $space-4;
  padding-bottom: $space-3;
  border-bottom: 1px solid $color-border-subtle;
}

.race-stage__header-info {
  display: flex;
  align-items: center;
  gap: $space-3;
}

.race-stage__header-img {
  width: 40px;
  height: 40px;
  border-radius: $radius-md;
  object-fit: cover;
}

.race-stage__race-name {
  font-size: 1.125rem;
  font-weight: 600;
  margin: 0;
}

.race-stage__race-source {
  font-size: 0.75rem;
  color: $color-text-muted;
}

.race-stage__change {
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

.race-stage__traits {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.trait-card {
  padding: $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
}

.trait-card__name {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
}

.trait-card__desc {
  font-size: 0.875rem;
  color: $color-text-muted;
  margin: 0 0 $space-3 0;
  line-height: 1.5;
}

.trait-card__options-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: $color-text-muted;
  text-transform: uppercase;
  margin-bottom: $space-2;
}

.trait-card__options-list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.trait-option {
  display: flex;
  align-items: flex-start;
  gap: $space-2;
  padding: $space-2;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  text-align: left;
  color: $color-text;
  transition: all 150ms ease;

  &:hover:not(.is-disabled) {
    border-color: $color-border-strong;
  }

  &.is-selected {
    border-color: $color-accent;
    background: rgba($color-accent, 0.05);
  }

  &.is-disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  &.is-granted {
    border-color: $color-success;
    background: rgba($color-success, 0.05);
  }
}

.trait-option__check {
  width: 1.25rem;
  height: 1.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid $color-border-strong;
  border-radius: $radius-xs;
  flex-shrink: 0;
  color: $color-accent;
  font-size: 0.75rem;

  .is-selected & {
    background: $color-accent;
    border-color: $color-accent;
    color: white;
  }

  .is-granted & {
    background: $color-success;
    border-color: $color-success;
    color: white;
  }
}

.trait-option__content {
  display: flex;
  flex-direction: column;
  gap: $space-1;
}

.trait-option__name {
  font-weight: 500;
  font-size: 0.875rem;
}

.trait-option__granted {
  font-size: 0.75rem;
  color: $color-success;
}

.trait-option__desc {
  font-size: 0.75rem;
  color: $color-text-muted;
  line-height: 1.4;
}

.trait-card__spells {
  margin-top: $space-3;
}

.trait-card__spell-list {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
}

.trait-spell {
  padding: $space-1 $space-2;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  font-size: 0.75rem;
}

.race-stage__no-traits {
  text-align: center;
  padding: $space-6;
  color: $color-text-muted;
  font-style: italic;
}

.customize-origin-section {
  padding: $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
}

.customize-origin-section__title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
  display: flex;
  align-items: center;
  gap: $space-2;
  color: $color-accent;

  i { font-size: 0.875rem; }
}

.customize-origin-section__banner {
  display: flex;
  align-items: flex-start;
  gap: $space-2;
  padding: $space-2;
  margin-bottom: $space-3;
  background: rgba($color-accent, 0.05);
  border: 1px solid rgba($color-accent, 0.2);
  border-radius: $radius-sm;
  font-size: 0.8rem;
  color: $color-text-muted;
  line-height: 1.4;

  i { color: $color-accent; margin-top: 2px; }
}

.customize-origin-section__warning {
  display: flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2;
  margin-bottom: $space-3;
  background: rgba($color-warning, 0.1);
  border: 1px solid rgba($color-warning, 0.3);
  border-radius: $radius-sm;
  font-size: 0.8rem;
  color: $color-warning;
}

.customize-origin-section__list {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.customize-origin-row {
  display: flex;
  align-items: center;
  gap: $space-3;
}

.customize-origin-row__bonus {
  font-size: 1rem;
  font-weight: 700;
  color: $color-accent;
  min-width: 2rem;
  text-align: center;
}

.customize-origin-row__original {
  font-size: 0.75rem;
  color: $color-text-muted;
  white-space: nowrap;
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

.preview-img {
  width: 100%;
  max-height: 200px;
  object-fit: cover;
  border-radius: $radius-md;
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
}

.preview-section__title {
  font-size: 0.875rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
  color: $color-text;
}

.preview-traits {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.preview-trait {
  font-size: 0.875rem;
  color: $color-text;
  line-height: 1.4;

  strong {
    color: $color-text;
  }
}

.preview-trait__options {
  color: $color-text-muted;
  font-style: italic;
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
