<script setup lang="ts">
import { ref, inject, watch, computed } from 'vue'
import { useToast } from '@/composables/useToast'
import { useAutoSave } from '@/composables/useAutoSave'
import { putCharactersId } from '@/api'
import type { Characteristics } from '@/api'
import { alignmentOptions, genderOptions, lifestyleOptions, sizeLabels, characteristicsTypeLabels } from '@/content/enums'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiGrid from '@/components/ui/layout/UiGrid.vue'
import UiModal from '@/components/ui/UiModal.vue'

const { showToast } = useToast()

const character = inject<any>('character')
const characterId = inject<any>('characterId')

const heightUnit = ref<'imperial' | 'metric'>('imperial')
const weightUnit = ref<'imperial' | 'metric'>('imperial')

const displayHeightMeters = ref(0)
const displayWeightKg = ref(0)

const form = ref({
  alignment: 4,
  gender: 0,
  lifestyle: 3,
  age: 0,
  heightFeet: 0,
  heightInches: 0,
  weightInPounds: 0,
  skin: '',
  hair: '',
  eyes: '',
  appearance: '',
  faith: '',
  personalityTraits: [] as string[],
  ideals: [] as string[],
  bonds: [] as string[],
  flaws: [] as string[],
  backstory: '',
  notes: ''
})

const showPresetModal = ref(false)
const presetModalType = ref<'personalityTraits' | 'ideals' | 'bonds' | 'flaws'>('personalityTraits')
const customInput = ref('')

const sizeFromRace = computed(() => {
  const raceSize = character.value?.race?.race?.size
  return raceSize !== undefined ? sizeLabels[raceSize] : 'Medium'
})

const backgroundCharacteristics = computed(() => {
  const bg = character.value?.background?.background
  if (!bg?.characteristics) return {}

  const result: Record<number, Characteristics[]> = {}
  for (const c of bg.characteristics) {
    const type = c.type ?? 0
    if (!result[type]) result[type] = []
    result[type].push(c)
  }
  return result
})

const presetOptions = computed(() => {
  const typeMap: Record<string, number> = {
    personalityTraits: 0,
    ideals: 1,
    bonds: 2,
    flaws: 3
  }
  const typeIndex = typeMap[presetModalType.value] ?? 0
  return backgroundCharacteristics.value[typeIndex] ?? []
})

const presetModalTitle = computed(() => {
  const titles: Record<string, string> = {
    personalityTraits: 'Add Personality Trait',
    ideals: 'Add Ideal',
    bonds: 'Add Bond',
    flaws: 'Add Flaw'
  }
  return titles[presetModalType.value]
})

function syncHeightToDisplay() {
  const totalInches = form.value.heightFeet * 12 + form.value.heightInches
  displayHeightMeters.value = Math.round(totalInches * 2.54) / 100
}

function syncWeightToDisplay() {
  displayWeightKg.value = Math.round(form.value.weightInPounds * 0.453592 * 10) / 10
}

function onHeightUnitChange(unit: 'imperial' | 'metric') {
  if (unit === 'metric') {
    syncHeightToDisplay()
  }
  heightUnit.value = unit
}

function onWeightUnitChange(unit: 'imperial' | 'metric') {
  if (unit === 'metric') {
    syncWeightToDisplay()
  }
  weightUnit.value = unit
}

function updateHeightFromMeters(meters: number) {
  displayHeightMeters.value = meters
  const totalInches = Math.round(meters * 100 / 2.54)
  form.value.heightFeet = Math.floor(totalInches / 12)
  form.value.heightInches = totalInches % 12
}

function updateWeightFromKg(kg: number) {
  displayWeightKg.value = kg
  form.value.weightInPounds = Math.round(kg / 0.453592)
}

function updateHeightFeet(feet: number) {
  form.value.heightFeet = feet
  syncHeightToDisplay()
}

function updateHeightInches(inches: number) {
  form.value.heightInches = inches
  syncHeightToDisplay()
}

function updateWeightPounds(pounds: number) {
  form.value.weightInPounds = pounds
  syncWeightToDisplay()
}

const autoSaveEnabled = ref(false)

const { isSaving } = useAutoSave({
  source: () => ({ ...form.value }),
  save: saveToBackend,
  delay: 1000,
  enabled: autoSaveEnabled
})

function openPresetModal(type: 'personalityTraits' | 'ideals' | 'bonds' | 'flaws') {
  presetModalType.value = type
  customInput.value = ''
  showPresetModal.value = true
}

function selectPreset(text: string) {
  form.value[presetModalType.value].push(text)
  showPresetModal.value = false
}

function addCustom() {
  if (customInput.value.trim()) {
    form.value[presetModalType.value].push(customInput.value.trim())
    showPresetModal.value = false
  }
}

function removeListItem(list: string[], index: number) {
  list.splice(index, 1)
}

function updateListItem(list: string[], index: number, value: string) {
  list[index] = value
}

async function saveToBackend() {
  if (!characterId?.value || !character.value) return
  try {
    const c = character.value
    const totalHeightInches = form.value.heightFeet * 12 + form.value.heightInches

    await putCharactersId(characterId.value, {
      name: c.name,
      avatarId: c.avatarId,
      sources: c.sources,
      advancementType: c.advancementType,
      hitPointType: c.hitPointType,
      abilityScoreGenerationMethod: c.abilityScoreGenerationMethod,
      optionalClassFeatures: c.optionalClassFeatures,
      customizeOrigin: c.customizeOrigin,
      exceedLevelCap: c.exceedLevelCap,
      allowMulticlassing: c.allowMulticlassing,
      checkMulticlassingPrerequisites: c.checkMulticlassingPrerequisites,
      movementSpeed: c.movementSpeed,
      swimmingSpeed: c.swimmingSpeed,
      flyingSpeed: c.flyingSpeed,
      inspirationPoints: c.inspirationPoints,
      maxHitDie: c.maxHitDie,
      currentHitDie: c.currentHitDie,
      temporaryHitPoints: c.temporaryHitPoints,
      currentHitPoints: c.currentHitPoints,
      rawMaximumHitPoints: c.rawMaximumHitPoints,
      hitPointBonus: c.hitPointBonus,
      overriddenMaximumHitPoints: c.overriddenMaximumHitPoints,
      initiativeBonus: c.initiativeBonus,
      baseArmorClass: c.baseArmorClass,
      armorClassBonus: c.armorClassBonus,
      passivePerceptionBonus: c.passivePerceptionBonus,
      passiveInvestigationBonus: c.passiveInvestigationBonus,
      passiveInsightBonus: c.passiveInsightBonus,
      xp: c.xp,
      deathSaveSuccesses: c.deathSaveSuccesses,
      deathSaveFailures: c.deathSaveFailures,
      exhaustionLevel: c.exhaustionLevel,
      conditions: c.conditions,
      damageDefenses: c.damageDefenses,
      conditionDefenses: c.conditionDefenses,
      savingThrowAdvantages: c.savingThrowAdvantages,
      savingThrowDisadvantages: c.savingThrowDisadvantages,
      blindsightRange: c.blindsightRange,
      blindsightNote: c.blindsightNote,
      darkvisionRange: c.darkvisionRange,
      darkvisionNote: c.darkvisionNote,
      tremorsenseRange: c.tremorsenseRange,
      tremorsenseNote: c.tremorsenseNote,
      truesightRange: c.truesightRange,
      truesightNote: c.truesightNote,
      languages: c.languages,
      armorProficiencies: c.armorProficiencies,
      weaponProficiencies: c.weaponProficiencies,
      toolProficiencies: c.toolProficiencies,
      countMoneyWeight: c.countMoneyWeight,
      gold: c.gold,
      electrum: c.electrum,
      silver: c.silver,
      copper: c.copper,
      arrowQuiver: c.arrowQuiver,
      boltQuiver: c.boltQuiver,
      lifestyle: form.value.lifestyle,
      alignment: form.value.alignment,
      gender: form.value.gender,
      size: c.size,
      age: form.value.age,
      heightInInches: totalHeightInches,
      weightInPounds: form.value.weightInPounds,
      skin: form.value.skin,
      hair: form.value.hair,
      eyes: form.value.eyes,
      appearance: form.value.appearance,
      faith: form.value.faith,
      personalityTraits: form.value.personalityTraits.filter(t => t.trim()),
      ideals: form.value.ideals.filter(t => t.trim()),
      bonds: form.value.bonds.filter(t => t.trim()),
      flaws: form.value.flaws.filter(t => t.trim()),
      organizations: c.organizations,
      allies: c.allies,
      enemies: c.enemies,
      backstory: form.value.backstory,
      notes: form.value.notes
    })
    updateLocalCharacter()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to save details.' })
  }
}

function updateLocalCharacter() {
  if (!character.value) return
  const totalHeightInches = form.value.heightFeet * 12 + form.value.heightInches

  character.value.lifestyle = form.value.lifestyle
  character.value.alignment = form.value.alignment
  character.value.gender = form.value.gender
  character.value.age = form.value.age
  character.value.heightInInches = totalHeightInches
  character.value.weightInPounds = form.value.weightInPounds
  character.value.skin = form.value.skin
  character.value.hair = form.value.hair
  character.value.eyes = form.value.eyes
  character.value.appearance = form.value.appearance
  character.value.faith = form.value.faith
  character.value.personalityTraits = [...form.value.personalityTraits.filter(t => t.trim())]
  character.value.ideals = [...form.value.ideals.filter(t => t.trim())]
  character.value.bonds = [...form.value.bonds.filter(t => t.trim())]
  character.value.flaws = [...form.value.flaws.filter(t => t.trim())]
  character.value.backstory = form.value.backstory
  character.value.notes = form.value.notes
}

function loadFromCharacter() {
  if (!character.value) {
    autoSaveEnabled.value = true
    return
  }
  const c = character.value
  form.value = {
    alignment: c.alignment ?? 4,
    gender: c.gender ?? 0,
    lifestyle: c.lifestyle ?? 3,
    age: c.age ?? 0,
    heightFeet: Math.floor((c.heightInInches ?? 0) / 12),
    heightInches: (c.heightInInches ?? 0) % 12,
    weightInPounds: c.weightInPounds ?? 0,
    skin: c.skin ?? '',
    hair: c.hair ?? '',
    eyes: c.eyes ?? '',
    appearance: c.appearance ?? '',
    faith: c.faith ?? '',
    personalityTraits: c.personalityTraits ?? [],
    ideals: c.ideals ?? [],
    bonds: c.bonds ?? [],
    flaws: c.flaws ?? [],
    backstory: c.backstory ?? '',
    notes: c.notes ?? ''
  }
  syncHeightToDisplay()
  syncWeightToDisplay()
  autoSaveEnabled.value = true
}

watch(() => character.value, loadFromCharacter, { immediate: true })
</script>

<template>
  <div class="details-stage">
    <div class="details-stage__header-row">
      <div>
        <h2 class="details-stage__title">Character Details</h2>
        <p class="details-stage__desc">Add personal details, backstory, and more.</p>
      </div>
      <span v-if="isSaving" class="details-stage__saving">
        <i class="fas fa-spinner fa-spin" /> Saving...
      </span>
    </div>

    <div class="details-stage__section">
      <h3 class="details-stage__section-title">Basic Information</h3>
      <UiGrid :cols="1" :cols-md="2" :cols-lg="4" :gap="1">
        <UiSelect
          v-model="form.alignment"
          label="Alignment"
          :options="alignmentOptions"
        />
        <UiSelect
          v-model="form.gender"
          label="Gender"
          :options="genderOptions"
        />
        <UiSelect
          v-model="form.lifestyle"
          label="Lifestyle"
          :options="lifestyleOptions"
        />
        <div class="readonly-field">
          <label class="readonly-field__label">Size</label>
          <div class="readonly-field__value">{{ sizeFromRace }}</div>
          <span class="readonly-field__hint">Determined by race</span>
        </div>
      </UiGrid>
    </div>

    <div class="details-stage__section">
      <h3 class="details-stage__section-title">Physical Characteristics</h3>
      <UiGrid :cols="1" :cols-md="2" :cols-lg="4" :gap="1">
        <UiInput
          v-model.number="form.age"
          label="Age"
          type="number"
          min="0"
        />
        <div class="measurement-input">
          <div class="measurement-input__header">
            <label class="measurement-input__label">Height</label>
            <div class="measurement-input__toggle">
              <button
                class="measurement-input__toggle-btn"
                :class="{ 'is-active': heightUnit === 'imperial' }"
                @click="onHeightUnitChange('imperial')"
              >
                ft/in
              </button>
              <button
                class="measurement-input__toggle-btn"
                :class="{ 'is-active': heightUnit === 'metric' }"
                @click="onHeightUnitChange('metric')"
              >
                m
              </button>
            </div>
          </div>
          <div v-if="heightUnit === 'imperial'" class="measurement-input__fields">
            <div class="measurement-input__field">
              <UiInput
                :model-value="form.heightFeet"
                type="number"
                min="0"
                @update:model-value="updateHeightFeet(Number($event))"
              />
              <span class="measurement-input__unit">ft</span>
            </div>
            <div class="measurement-input__field">
              <UiInput
                :model-value="form.heightInches"
                type="number"
                min="0"
                max="11"
                @update:model-value="updateHeightInches(Number($event))"
              />
              <span class="measurement-input__unit">in</span>
            </div>
          </div>
          <div v-else class="measurement-input__fields measurement-input__fields--single">
            <div class="measurement-input__field">
              <UiInput
                :model-value="displayHeightMeters"
                type="number"
                min="0"
                step="0.01"
                @update:model-value="updateHeightFromMeters(Number($event))"
              />
              <span class="measurement-input__unit">m</span>
            </div>
          </div>
        </div>
        <div class="measurement-input">
          <div class="measurement-input__header">
            <label class="measurement-input__label">Weight</label>
            <div class="measurement-input__toggle">
              <button
                class="measurement-input__toggle-btn"
                :class="{ 'is-active': weightUnit === 'imperial' }"
                @click="onWeightUnitChange('imperial')"
              >
                lbs
              </button>
              <button
                class="measurement-input__toggle-btn"
                :class="{ 'is-active': weightUnit === 'metric' }"
                @click="onWeightUnitChange('metric')"
              >
                kg
              </button>
            </div>
          </div>
          <div class="measurement-input__fields measurement-input__fields--single">
            <div class="measurement-input__field">
              <UiInput
                v-if="weightUnit === 'imperial'"
                :model-value="form.weightInPounds"
                type="number"
                min="0"
                @update:model-value="updateWeightPounds(Number($event))"
              />
              <UiInput
                v-else
                :model-value="displayWeightKg"
                type="number"
                min="0"
                step="0.1"
                @update:model-value="updateWeightFromKg(Number($event))"
              />
              <span class="measurement-input__unit">{{ weightUnit === 'imperial' ? 'lbs' : 'kg' }}</span>
            </div>
          </div>
        </div>
        <UiInput
          v-model="form.faith"
          label="Faith"
          placeholder="Deity or religion..."
        />
      </UiGrid>
      <UiGrid :cols="1" :cols-md="2" :cols-lg="4" :gap="1" class="details-stage__row">
        <UiInput
          v-model="form.skin"
          label="Skin"
          placeholder="Skin color..."
        />
        <UiInput
          v-model="form.hair"
          label="Hair"
          placeholder="Hair color/style..."
        />
        <UiInput
          v-model="form.eyes"
          label="Eyes"
          placeholder="Eye color..."
        />
        <UiInput
          v-model="form.appearance"
          label="Appearance"
          placeholder="Distinguishing features..."
        />
      </UiGrid>
    </div>

    <div class="details-stage__section">
      <h3 class="details-stage__section-title">Personality Traits</h3>
      <div class="list-editor">
        <div v-for="(trait, index) in form.personalityTraits" :key="index" class="list-editor__row">
          <UiInput
            :model-value="trait"
            placeholder="Enter a personality trait..."
            @update:model-value="updateListItem(form.personalityTraits, index, $event)"
          />
          <button class="list-editor__remove" @click="removeListItem(form.personalityTraits, index)">
            <i class="fas fa-times" />
          </button>
        </div>
        <button class="list-editor__add" @click="openPresetModal('personalityTraits')">
          <i class="fas fa-plus" /> Add Personality Trait
        </button>
      </div>
    </div>

    <div class="details-stage__section">
      <h3 class="details-stage__section-title">Ideals</h3>
      <div class="list-editor">
        <div v-for="(ideal, index) in form.ideals" :key="index" class="list-editor__row">
          <UiInput
            :model-value="ideal"
            placeholder="Enter an ideal..."
            @update:model-value="updateListItem(form.ideals, index, $event)"
          />
          <button class="list-editor__remove" @click="removeListItem(form.ideals, index)">
            <i class="fas fa-times" />
          </button>
        </div>
        <button class="list-editor__add" @click="openPresetModal('ideals')">
          <i class="fas fa-plus" /> Add Ideal
        </button>
      </div>
    </div>

    <div class="details-stage__section">
      <h3 class="details-stage__section-title">Bonds</h3>
      <div class="list-editor">
        <div v-for="(bond, index) in form.bonds" :key="index" class="list-editor__row">
          <UiInput
            :model-value="bond"
            placeholder="Enter a bond..."
            @update:model-value="updateListItem(form.bonds, index, $event)"
          />
          <button class="list-editor__remove" @click="removeListItem(form.bonds, index)">
            <i class="fas fa-times" />
          </button>
        </div>
        <button class="list-editor__add" @click="openPresetModal('bonds')">
          <i class="fas fa-plus" /> Add Bond
        </button>
      </div>
    </div>

    <div class="details-stage__section">
      <h3 class="details-stage__section-title">Flaws</h3>
      <div class="list-editor">
        <div v-for="(flaw, index) in form.flaws" :key="index" class="list-editor__row">
          <UiInput
            :model-value="flaw"
            placeholder="Enter a flaw..."
            @update:model-value="updateListItem(form.flaws, index, $event)"
          />
          <button class="list-editor__remove" @click="removeListItem(form.flaws, index)">
            <i class="fas fa-times" />
          </button>
        </div>
        <button class="list-editor__add" @click="openPresetModal('flaws')">
          <i class="fas fa-plus" /> Add Flaw
        </button>
      </div>
    </div>

    <div class="details-stage__section">
      <h3 class="details-stage__section-title">Backstory</h3>
      <textarea
        v-model="form.backstory"
        class="details-stage__textarea"
        placeholder="Write your character's backstory..."
        rows="6"
      />
    </div>

    <div class="details-stage__section">
      <h3 class="details-stage__section-title">Notes</h3>
      <textarea
        v-model="form.notes"
        class="details-stage__textarea"
        placeholder="Additional notes..."
        rows="4"
      />
    </div>

    <UiModal v-model="showPresetModal" :title="presetModalTitle" :close-on-backdrop="true" :close-on-esc="true" class="preset-modal">
      <div class="preset-content">
        <div class="preset-custom">
          <h4 class="preset-custom__title">Write your own</h4>
          <div class="preset-custom__input">
            <UiInput
              v-model="customInput"
              :placeholder="`Enter a custom ${characteristicsTypeLabels[presetModalType === 'personalityTraits' ? 0 : presetModalType === 'ideals' ? 1 : presetModalType === 'bonds' ? 2 : 3]?.toLowerCase() ?? 'entry'}...`"
              @keyup.enter="addCustom"
            />
            <button class="preset-custom__btn" :disabled="!customInput.trim()" @click="addCustom">
              <i class="fas fa-plus" />
            </button>
          </div>
        </div>

        <div v-if="presetOptions.length" class="preset-suggestions">
          <h4 class="preset-suggestions__title">
            Suggestions from {{ character?.background?.background?.name ?? 'Background' }}
          </h4>
          <div class="preset-suggestions__list">
            <button
              v-for="preset in presetOptions"
              :key="preset.id"
              class="preset-option"
              @click="selectPreset(preset.text ?? '')"
            >
              {{ preset.text }}
            </button>
          </div>
        </div>

        <div v-else class="preset-empty">
          <p>No suggestions available from your background. Write your own above.</p>
        </div>
      </div>
    </UiModal>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.details-stage__header-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: $space-4;
}

.details-stage__title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0 0 $space-1 0;
}

.details-stage__desc {
  color: $color-text-muted;
  margin: 0;
}

.details-stage__saving {
  font-size: 0.875rem;
  color: $color-text-muted;
}

.details-stage__section {
  margin-bottom: $space-5;
  padding-bottom: $space-4;
  border-bottom: 1px solid $color-border-subtle;

  &:last-of-type {
    border-bottom: none;
  }
}

.details-stage__section-title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 $space-3 0;
}

.details-stage__row {
  margin-top: $space-3;
}

.readonly-field {
  display: flex;
  flex-direction: column;
}

.readonly-field__label {
  font-size: 0.875rem;
  font-weight: 500;
  margin-bottom: $space-1;
  color: $color-text;
}

.readonly-field__value {
  padding: $space-2 $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  color: $color-text;
  font-size: 0.875rem;
}

.readonly-field__hint {
  font-size: 0.75rem;
  color: $color-text-muted;
  margin-top: $space-1;
}

.measurement-input {
  display: flex;
  flex-direction: column;
}

.measurement-input__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: $space-1;
}

.measurement-input__label {
  font-size: 0.875rem;
  font-weight: 500;
  color: $color-text;
}

.measurement-input__toggle {
  display: flex;
  gap: 1px;
  background: $color-border-subtle;
  border-radius: $radius-sm;
  overflow: hidden;
}

.measurement-input__toggle-btn {
  padding: $space-1 $space-2;
  background: $color-surface-alt;
  border: none;
  color: $color-text-muted;
  font-size: 0.75rem;
  cursor: pointer;
  transition: all 150ms ease;

  &:hover {
    color: $color-text;
  }

  &.is-active {
    background: $color-accent;
    color: white;
  }
}

.measurement-input__fields {
  display: flex;
  gap: $space-2;

  &--single {
    .measurement-input__field {
      flex: 1;
    }
  }
}

.measurement-input__field {
  display: flex;
  align-items: center;
  gap: $space-1;
  flex: 1;
}

.measurement-input__unit {
  font-size: 0.75rem;
  color: $color-text-muted;
  white-space: nowrap;
}

.list-editor {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.list-editor__row {
  display: flex;
  gap: $space-2;
  align-items: center;
}

.list-editor__remove {
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
  flex-shrink: 0;

  &:hover {
    border-color: $color-danger;
    color: $color-danger;
  }
}

.list-editor__add {
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

.details-stage__textarea {
  width: 100%;
  padding: $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  color: $color-text;
  font-family: inherit;
  font-size: 0.875rem;
  resize: vertical;
  min-height: 100px;

  &::placeholder {
    color: $color-text-soft;
  }

  &:focus {
    outline: none;
    border-color: $color-accent;
  }
}

.preset-modal :deep(.modal) {
  max-width: 600px;
}

.preset-content {
  display: flex;
  flex-direction: column;
  gap: $space-4;
}

.preset-custom {
  padding: $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
}

.preset-custom__title {
  font-size: 0.875rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
  color: $color-text;
}

.preset-custom__input {
  display: flex;
  gap: $space-2;
}

.preset-custom__btn {
  width: 2.5rem;
  height: 2.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: $color-accent;
  border: none;
  border-radius: $radius-md;
  cursor: pointer;
  color: white;
  flex-shrink: 0;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  &:hover:not(:disabled) {
    opacity: 0.9;
  }
}

.preset-suggestions__title {
  font-size: 0.875rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
  color: $color-text;
}

.preset-suggestions__list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
  max-height: 300px;
  overflow-y: auto;
}

.preset-option {
  padding: $space-2 $space-3;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  text-align: left;
  color: $color-text;
  font-size: 0.875rem;
  line-height: 1.4;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-accent;
    background: rgba($color-accent, 0.05);
  }
}

.preset-empty {
  text-align: center;
  padding: $space-4;
  color: $color-text-muted;
  font-size: 0.875rem;
}
</style>
