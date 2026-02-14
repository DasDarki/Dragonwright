<script setup lang="ts">
import { ref, inject, watch, computed, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'
import { useAutoSave } from '@/composables/useAutoSave'
import { getClasses, getClassesId, putCharactersIdClasses } from '@/api'
import type { Class, Subclass, CharacterClassData, Skill } from '@/api'
import { sourceLabels, sourceSortOrder, abilityScoreLabels, skillLabels } from '@/content/enums'
import { useImageUrl } from '@/composables/useFileUpload'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'

const { showToast } = useToast()

const character = inject<any>('character')
const characterId = inject<any>('characterId')

interface CharacterClassState {
  id?: string
  classId: string
  class: Class
  level: number
  subclassId?: string
  subclass?: Subclass
  isStartingClass: boolean
  chosenSkillProficiencies: Skill[]
  chosenFeatureOptions: Record<string, string[]>
  chosenSpells: Record<string, string[]>
}

const classes = ref<Class[]>([])
const selectedClasses = ref<CharacterClassState[]>([])
const loadingClasses = ref(true)
const search = ref('')

const showPreviewModal = ref(false)
const previewClass = ref<Class | null>(null)
const loadingPreview = ref(false)

const filteredClasses = computed(() => {
  let list = classes.value
  if (search.value) {
    const s = search.value.toLowerCase()
    list = list.filter(c => c.name.toLowerCase().includes(s))
  }
  return [...list].sort((a, b) => {
    const so = (sourceSortOrder[a.source ?? 0] ?? 2) - (sourceSortOrder[b.source ?? 0] ?? 2)
    if (so !== 0) return so
    return a.name.localeCompare(b.name)
  })
})

const allowMulticlassing = computed(() => character.value?.allowMulticlassing ?? true)
const checkPrerequisites = computed(() => character.value?.checkMulticlassingPrerequisites ?? true)
const exceedLevelCap = computed(() => character.value?.exceedLevelCap ?? false)

const totalLevel = computed(() => selectedClasses.value.reduce((sum, c) => sum + c.level, 0))
const maxLevel = computed(() => exceedLevelCap.value ? 40 : 20)
const canAddClass = computed(() => {
  if (!allowMulticlassing.value && selectedClasses.value.length >= 1) return false
  return totalLevel.value < maxLevel.value
})

const autoSaveEnabled = ref(false)

const { isSaving } = useAutoSave({
  source: () => selectedClasses.value.map(c => ({
    id: c.id,
    classId: c.classId,
    level: c.level,
    subclassId: c.subclassId,
    isStartingClass: c.isStartingClass,
    chosenSkillProficiencies: c.chosenSkillProficiencies,
    chosenFeatureOptions: c.chosenFeatureOptions,
    chosenSpells: c.chosenSpells
  })),
  save: saveToBackend,
  delay: 1000,
  enabled: autoSaveEnabled
})

function getAbilityScore(abilityStr: string): number {
  const abilities = character.value?.abilities
  if (!abilities) return 10
  switch (abilityStr) {
    case "Strength": return abilities.strength ?? 10
    case "Dexterity": return abilities.dexterity ?? 10
    case "Constitution": return abilities.constitution ?? 10
    case "Intelligence": return abilities.intelligence ?? 10
    case "Wisdom": return abilities.wisdom ?? 10
    case "Charisma": return abilities.charisma ?? 10
    default: return 10
  }
}

function meetsMulticlassingPrerequisites(cls: Class): { met: boolean; reasons: string[] } {
  if (!checkPrerequisites.value) return { met: true, reasons: [] }

  const primaryReqs = cls.multiclassingRequirements as Record<number, number> | undefined ?? {}
  const altReqs = cls.multiclassingRequirementsAlt as Record<number, number> | undefined ?? {}

  const primaryFailures: string[] = []
  const altFailures: string[] = []

  for (const [ability, min] of Object.entries(primaryReqs)) {
    const score = getAbilityScore(ability)
    if (score < Number(min)) {
      primaryFailures.push(`${ability} ${score} < ${min}`)
    }
  }

  const meetsPrimary = Object.keys(primaryReqs).length === 0 || primaryFailures.length === 0

  const hasAltReqs = Object.keys(altReqs).length > 0
  if (hasAltReqs) {
    for (const [ability, min] of Object.entries(altReqs)) {
      const score = getAbilityScore(ability)
      if (score < Number(min)) {
        altFailures.push(`${ability} ${score} < ${min}`)
      }
    }
  }

  const meetsAlt = hasAltReqs && altFailures.length === 0

  if (meetsPrimary || meetsAlt) {
    return { met: true, reasons: [] }
  }

  const reasons = primaryFailures
  if (hasAltReqs && altFailures.length > 0) {
    reasons.push(`Alternative: ${altFailures.join(', ')}`)
  }

  return { met: false, reasons }
}

function needsSubclass(classState: CharacterClassState): boolean {
  const selectionLevel = Number(classState.class.subclassSelectionLevel ?? 3)
  return selectionLevel > 0 && classState.level >= selectionLevel && !classState.subclassId
}

function canSelectSubclass(classState: CharacterClassState): boolean {
  const selectionLevel = Number(classState.class.subclassSelectionLevel ?? 3)
  return selectionLevel > 0 && classState.level >= selectionLevel
}

function getAvailableSubclasses(classState: CharacterClassState): Subclass[] {
  return classState.class.subclasses ?? []
}

function increaseLevel(classIndex: number) {
  const classState = selectedClasses.value[classIndex]
  if (!classState) return
  if (classState.level >= 20) return
  if (totalLevel.value >= maxLevel.value) return

  classState.level++
  selectedClasses.value = [...selectedClasses.value]
}

function decreaseLevel(classIndex: number) {
  const classState = selectedClasses.value[classIndex]
  if (!classState) return

  if (classState.level <= 1) {
    selectedClasses.value = selectedClasses.value.filter((_, i) => i !== classIndex)
    if (classState.isStartingClass && selectedClasses.value.length > 0) {
      const firstClass = selectedClasses.value[0]
      if (firstClass) {
        firstClass.isStartingClass = true
      }
    }
  } else {
    classState.level--
    const selectionLevel = Number(classState.class.subclassSelectionLevel ?? 3)
    if (classState.level < selectionLevel) {
      classState.subclassId = undefined
      classState.subclass = undefined
    }
    selectedClasses.value = [...selectedClasses.value]
  }
}

function selectSubclass(classIndex: number, subclassId: string | null) {
  const classState = selectedClasses.value[classIndex]
  if (!classState) return

  if (subclassId) {
    const subclass = classState.class.subclasses?.find(s => s.id === subclassId)
    classState.subclassId = subclassId
    classState.subclass = subclass
  } else {
    classState.subclassId = undefined
    classState.subclass = undefined
  }
  selectedClasses.value = [...selectedClasses.value]
}

function toggleSkillProficiency(classIndex: number, skill: Skill) {
  const classState = selectedClasses.value[classIndex]
  if (!classState || !classState.isStartingClass) return

  const maxSkills = Number(classState.class.skillProficienciesCount ?? 2)
  const skills = classState.chosenSkillProficiencies ?? []

  if (skills.includes(skill)) {
    classState.chosenSkillProficiencies = skills.filter(s => s !== skill)
  } else if (skills.length < maxSkills) {
    classState.chosenSkillProficiencies = [...skills, skill]
  }
  selectedClasses.value = [...selectedClasses.value]
}

function isSkillSelected(classIndex: number, skill: Skill): boolean {
  const classState = selectedClasses.value[classIndex]
  return classState?.chosenSkillProficiencies?.includes(skill) ?? false
}

async function loadClasses() {
  loadingClasses.value = true
  try {
    const sources = character.value?.sources ?? [0, 1]
    const res = await getClasses({ pageSize: 1000 })
    classes.value = ((res as any).data?.items ?? []).filter((c: Class) =>
      sources.includes(c.source)
    )
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load classes.' })
  } finally {
    loadingClasses.value = false
  }
}

async function openPreview(cls: Class) {
  loadingPreview.value = true
  showPreviewModal.value = true
  try {
    const res = await getClassesId(cls.id!)
    previewClass.value = (res as any).data
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load class details.' })
    showPreviewModal.value = false
  } finally {
    loadingPreview.value = false
  }
}

function cancelPreview() {
  showPreviewModal.value = false
  previewClass.value = null
}

async function addClassToCharacter() {
  if (!previewClass.value) return

  const isFirstClass = selectedClasses.value.length === 0

  const newClassState: CharacterClassState = {
    classId: previewClass.value.id!,
    class: previewClass.value,
    level: 1,
    isStartingClass: isFirstClass,
    chosenSkillProficiencies: [],
    chosenFeatureOptions: {},
    chosenSpells: {}
  }

  autoSaveEnabled.value = false
  selectedClasses.value = [...selectedClasses.value, newClassState]

  showPreviewModal.value = false
  previewClass.value = null

  await saveToBackend()
  updateLocalCharacter()
  autoSaveEnabled.value = true
}

async function saveToBackend() {
  if (!characterId?.value) return
  try {
    const classData: CharacterClassData[] = selectedClasses.value.map(c => ({
      id: c.id || undefined,
      classId: c.classId,
      level: c.level,
      subclassId: c.subclassId || undefined,
      isStartingClass: c.isStartingClass,
      chosenSkillProficiencies: c.chosenSkillProficiencies,
      chosenFeatureOptions: c.chosenFeatureOptions,
      chosenSpells: c.chosenSpells,
      classFeatureUsages: {},
      spellSlotsUsed: {},
      pactSlotsUsed: 0
    }))
    await putCharactersIdClasses(characterId.value, { classes: classData })
    updateLocalCharacter()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to save classes.' })
  }
}

function updateLocalCharacter() {
  if (!character.value) return
  character.value.classes = selectedClasses.value.map(c => ({
    id: c.id,
    classId: c.classId,
    class: c.class,
    level: c.level,
    subclassId: c.subclassId,
    subclass: c.subclass,
    isStartingClass: c.isStartingClass,
    chosenSkillProficiencies: c.chosenSkillProficiencies,
    chosenFeatureOptions: c.chosenFeatureOptions,
    chosenSpells: c.chosenSpells
  }))
}

function loadFromCharacter() {
  if (!character.value?.classes || character.value.classes.length === 0) {
    selectedClasses.value = []
    autoSaveEnabled.value = true
    return
  }

  selectedClasses.value = character.value.classes.map((cc: any) => ({
    id: cc.id,
    classId: cc.classId ?? cc.class?.id,
    class: cc.class,
    level: cc.level ?? 1,
    subclassId: cc.subclassId,
    subclass: cc.subclass,
    isStartingClass: cc.isStartingClass ?? false,
    chosenSkillProficiencies: cc.chosenSkillProficiencies ?? [],
    chosenFeatureOptions: cc.chosenFeatureOptions ?? {},
    chosenSpells: cc.chosenSpells ?? {}
  }))
  autoSaveEnabled.value = true
}

function getVisibleFeatures(classState: CharacterClassState) {
  const features = classState.class.features ?? []
  return features
    .filter(f => !f.hideInBuilder && Number(f.requiredCharacterLevel ?? 1) <= classState.level)
    .filter(f => character.value?.optionalClassFeatures || !f.isOptional)
    .sort((a, b) => Number(a.requiredCharacterLevel ?? 1) - Number(b.requiredCharacterLevel ?? 1))
}

function getPreviewPrerequisiteStatus() {
  if (!previewClass.value) return { met: true, reasons: [] }
  if (selectedClasses.value.length === 0) return { met: true, reasons: [] }
  return meetsMulticlassingPrerequisites(previewClass.value)
}

function isClassAlreadySelected(classId: string): boolean {
  return selectedClasses.value.some(c => c.classId === classId)
}

watch(() => character.value, loadFromCharacter, { immediate: true })

onMounted(loadClasses)
</script>

<template>
  <div class="class-stage">
    <div class="class-stage__header-row">
      <div>
        <h2 class="class-stage__title">Class Selection</h2>
        <p class="class-stage__desc">
          Choose your character's class{{ allowMulticlassing ? 'es' : '' }} and configure options.
        </p>
      </div>
      <div class="class-stage__header-right">
        <span class="class-stage__level-badge">
          Total Level: {{ totalLevel }} / {{ maxLevel }}
        </span>
        <span v-if="isSaving" class="class-stage__saving">
          <i class="fas fa-spinner fa-spin" /> Saving...
        </span>
      </div>
    </div>

    <div v-if="selectedClasses.length > 0" class="class-stage__selected">
      <div
        v-for="(classState, index) in selectedClasses"
        :key="classState.classId"
        class="class-selected-card"
        :class="{ 'has-warning': needsSubclass(classState) }"
      >
        <div class="class-selected-card__header">
          <div class="class-selected-card__title-row">
            <img v-if="useImageUrl(classState.class.image!)" :src="useImageUrl(classState.class.image!)!" class="class-selected-card__img" alt="" />
            <h3 class="class-selected-card__name">{{ classState.class.name }}</h3>
            <span v-if="classState.isStartingClass" class="class-selected-card__starting-badge">
              Starting Class
            </span>
          </div>
          <span class="class-selected-card__source">{{ sourceLabels[classState.class.source ?? 0] }}</span>
        </div>

        <div class="class-selected-card__level-section">
          <span class="class-selected-card__level-label">Level</span>
          <div class="class-selected-card__level-controls">
            <button
              class="class-selected-card__level-btn"
              @click="decreaseLevel(index)"
            >
              <i class="fas fa-minus" />
            </button>
            <span class="class-selected-card__level-value">{{ classState.level }}</span>
            <button
              class="class-selected-card__level-btn"
              :disabled="classState.level >= 20 || totalLevel >= maxLevel"
              @click="increaseLevel(index)"
            >
              <i class="fas fa-plus" />
            </button>
          </div>
        </div>

        <div v-if="canSelectSubclass(classState)" class="class-selected-card__subclass-section">
          <label class="class-selected-card__section-label">
            Subclass
            <span v-if="needsSubclass(classState)" class="class-selected-card__required">(Required)</span>
          </label>
          <UiSelect
            :model-value="classState.subclassId ?? ''"
            :options="getAvailableSubclasses(classState).map(s => ({ label: s.name, value: s.id! }))"
            placeholder="Choose a subclass..."
            @update:model-value="selectSubclass(index, $event as string)"
          />
          <div v-if="classState.subclass && useImageUrl(classState.subclass.image!)" class="class-selected-card__subclass-img-row">
            <img :src="useImageUrl(classState.subclass.image!)!" class="class-selected-card__subclass-img" alt="" />
            <span class="class-selected-card__subclass-name">{{ classState.subclass.name }}</span>
          </div>
        </div>

        <div v-if="classState.isStartingClass && classState.class.skillProficienciesOptions?.length" class="class-selected-card__skills-section">
          <label class="class-selected-card__section-label">
            Skill Proficiencies
            <span class="class-selected-card__skill-count">
              ({{ classState.chosenSkillProficiencies?.length ?? 0 }} / {{ classState.class.skillProficienciesCount }})
            </span>
          </label>
          <div class="class-selected-card__skill-grid">
            <button
              v-for="skill in classState.class.skillProficienciesOptions"
              :key="skill"
              class="skill-option"
              :class="{ 'is-selected': isSkillSelected(index, skill) }"
              :disabled="!isSkillSelected(index, skill) && (classState.chosenSkillProficiencies?.length ?? 0) >= Number(classState.class.skillProficienciesCount ?? 2)"
              @click="toggleSkillProficiency(index, skill)"
            >
              <span class="skill-option__check">
                <i v-if="isSkillSelected(index, skill)" class="fas fa-check" />
              </span>
              {{ skillLabels[skill] }}
            </button>
          </div>
        </div>

        <div v-if="getVisibleFeatures(classState).length > 0" class="class-selected-card__features-section">
          <label class="class-selected-card__section-label">Unlocked Features</label>
          <div class="class-selected-card__features-list">
            <span
              v-for="feature in getVisibleFeatures(classState)"
              :key="feature.id"
              class="feature-tag"
              :title="feature.description"
            >
              {{ feature.name }}
              <span v-if="feature.isOptional" class="feature-tag__optional">Optional</span>
              <span v-if="feature.requiredCharacterLevel && Number(feature.requiredCharacterLevel) > 1" class="feature-tag__level">
                Lv{{ feature.requiredCharacterLevel }}
              </span>
            </span>
          </div>
        </div>
      </div>
    </div>

    <div v-if="canAddClass" class="class-stage__add-section">
      <h3 class="class-stage__add-title">
        {{ selectedClasses.length === 0 ? 'Choose Your Class' : 'Add Another Class' }}
      </h3>

      <UiInput
        v-model="search"
        placeholder="Search classes..."
        class="class-stage__search"
      />

      <div v-if="loadingClasses" class="class-stage__loading">
        <i class="fas fa-spinner fa-spin" /> Loading classes...
      </div>

      <div v-else class="class-stage__grid">
        <button
          v-for="cls in filteredClasses"
          :key="cls.id"
          class="class-card"
          :class="{
            'is-disabled': isClassAlreadySelected(cls.id!),
            'has-prereq-warning': selectedClasses.length > 0 && !meetsMulticlassingPrerequisites(cls).met && checkPrerequisites
          }"
          :disabled="isClassAlreadySelected(cls.id!)"
          @click="openPreview(cls)"
        >
          <img v-if="useImageUrl(cls.image!)" :src="useImageUrl(cls.image!)!" class="class-card__img" alt="" />
          <div class="class-card__name">{{ cls.name }}</div>
          <div class="class-card__info">
            <span class="class-card__source">{{ sourceLabels[cls.source ?? 0] }}</span>
            <span class="class-card__hit-die">d{{ cls.hitDie }}</span>
          </div>
          <span v-if="isClassAlreadySelected(cls.id!)" class="class-card__selected-badge">
            Selected
          </span>
          <span
            v-else-if="selectedClasses.length > 0 && !meetsMulticlassingPrerequisites(cls).met && checkPrerequisites"
            class="class-card__prereq-warning"
            :title="meetsMulticlassingPrerequisites(cls).reasons.join(', ')"
          >
            <i class="fas fa-exclamation-triangle" />
          </span>
        </button>
      </div>

      <div v-if="!loadingClasses && filteredClasses.length === 0" class="class-stage__empty">
        No classes found matching your search.
      </div>
    </div>

    <div v-else-if="selectedClasses.length > 0" class="class-stage__no-add">
      <p v-if="!allowMulticlassing">
        <i class="fas fa-info-circle" />
        Multiclassing is disabled for this character.
      </p>
      <p v-else>
        <i class="fas fa-info-circle" />
        Maximum level reached ({{ maxLevel }}).
      </p>
    </div>

    <UiModal
      v-model="showPreviewModal"
      :title="previewClass?.name ?? 'Class Preview'"
      :close-on-backdrop="true"
      :close-on-esc="true"
      class="preview-modal"
    >
      <div v-if="loadingPreview" class="preview-loading">
        <i class="fas fa-spinner fa-spin" /> Loading class details...
      </div>

      <div v-else-if="previewClass" class="preview-content">
        <img v-if="useImageUrl(previewClass.image!)" :src="useImageUrl(previewClass.image!)!" class="preview-img" alt="" />
        <div class="preview-header">
          <span class="preview-source">{{ sourceLabels[previewClass.source ?? 0] }}</span>
        </div>

        <div class="preview-section">
          <div class="preview-stats">
            <div class="preview-stat">
              <span class="preview-stat__label">Hit Die</span>
              <span class="preview-stat__value">d{{ previewClass.hitDie }}</span>
            </div>
            <div class="preview-stat">
              <span class="preview-stat__label">Primary</span>
              <span class="preview-stat__value">
                {{ previewClass.primaryAbilityScores?.map(a => abilityScoreLabels[a]?.slice(0, 3).toUpperCase()).join(', ') || '—' }}
              </span>
            </div>
            <div class="preview-stat">
              <span class="preview-stat__label">Saves</span>
              <span class="preview-stat__value">
                {{ previewClass.savingThrowProficiencies?.map(a => abilityScoreLabels[a]?.slice(0, 3).toUpperCase()).join(', ') || '—' }}
              </span>
            </div>
          </div>
        </div>

        <div v-if="selectedClasses.length > 0" class="preview-section">
          <h4 class="preview-section__title">Multiclassing Prerequisites</h4>
          <div
            class="preview-prereq"
            :class="{ 'is-met': getPreviewPrerequisiteStatus().met, 'is-not-met': !getPreviewPrerequisiteStatus().met }"
          >
            <i :class="getPreviewPrerequisiteStatus().met ? 'fas fa-check-circle' : 'fas fa-times-circle'" />
            <span v-if="getPreviewPrerequisiteStatus().met">
              Requirements met
            </span>
            <span v-else>
              Requirements not met: {{ getPreviewPrerequisiteStatus().reasons.join(', ') }}
            </span>
          </div>
          <div v-if="!checkPrerequisites" class="preview-prereq-note">
            <i class="fas fa-info-circle" />
            Prerequisite checking is disabled for this character.
          </div>
        </div>

        <div class="preview-section">
          <h4 class="preview-section__title">Proficiencies</h4>
          <ul class="preview-gains">
            <li v-if="previewClass.skillProficienciesOptions?.length">
              <strong>Skills:</strong>
              Choose {{ previewClass.skillProficienciesCount }} from
              {{ previewClass.skillProficienciesOptions.map(s => skillLabels[s]).join(', ') }}
            </li>
            <li v-if="previewClass.savingThrowProficiencies?.length">
              <strong>Saving Throws:</strong>
              {{ previewClass.savingThrowProficiencies.map(a => abilityScoreLabels[a]).join(', ') }}
            </li>
          </ul>
        </div>

        <div v-if="previewClass.subclasses?.length" class="preview-section">
          <h4 class="preview-section__title">
            Subclasses
            <span class="preview-section__subtitle">
              (choose at level {{ previewClass.subclassSelectionLevel ?? 3 }})
            </span>
          </h4>
          <div class="preview-subclasses">
            <div v-for="sub in previewClass.subclasses" :key="sub.id" class="preview-subclass">
              <span class="preview-subclass__name">{{ sub.name }}</span>
              <span v-if="sub.shortDescription" class="preview-subclass__desc">{{ sub.shortDescription }}</span>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="modal-actions">
          <button class="modal-btn modal-btn--secondary" @click="cancelPreview">Cancel</button>
          <UiButton
            :disabled="isClassAlreadySelected(previewClass?.id ?? '') || (selectedClasses.length > 0 && !getPreviewPrerequisiteStatus().met && checkPrerequisites)"
            @click="addClassToCharacter"
          >
            <template v-if="isClassAlreadySelected(previewClass?.id ?? '')">
              Already Selected
            </template>
            <template v-else-if="selectedClasses.length > 0 && !getPreviewPrerequisiteStatus().met && checkPrerequisites">
              Prerequisites Not Met
            </template>
            <template v-else>
              {{ selectedClasses.length === 0 ? 'Select' : 'Add' }} {{ previewClass?.name }}
            </template>
          </UiButton>
        </div>
      </template>
    </UiModal>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.class-stage__header-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: $space-4;
}

.class-stage__title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0 0 $space-1 0;
}

.class-stage__desc {
  color: $color-text-muted;
  margin: 0;
}

.class-stage__header-right {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: $space-1;
}

.class-stage__level-badge {
  font-size: 0.875rem;
  font-weight: 600;
  color: $color-accent;
  padding: $space-1 $space-2;
  background: $color-accent-soft;
  border-radius: $radius-sm;
}

.class-stage__saving {
  font-size: 0.875rem;
  color: $color-text-muted;
}

.class-stage__selected {
  display: flex;
  flex-direction: column;
  gap: $space-3;
  margin-bottom: $space-4;
}

.class-selected-card {
  padding: $space-4;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-lg;

  &.has-warning {
    border-color: $color-warning;
  }
}

.class-selected-card__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: $space-3;
}

.class-selected-card__title-row {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.class-selected-card__img {
  width: 36px;
  height: 36px;
  border-radius: $radius-md;
  object-fit: cover;
}

.class-selected-card__name {
  font-size: 1.125rem;
  font-weight: 600;
  margin: 0;
}

.class-selected-card__starting-badge {
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  color: $color-accent;
  padding: $space-1 $space-2;
  background: $color-accent-soft;
  border-radius: $radius-sm;
}

.class-selected-card__source {
  font-size: 0.75rem;
  color: $color-text-muted;
}

.class-selected-card__level-section {
  display: flex;
  align-items: center;
  gap: $space-3;
  margin-bottom: $space-3;
  padding-bottom: $space-3;
  border-bottom: 1px solid $color-border-subtle;
}

.class-selected-card__level-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: $color-text-muted;
}

.class-selected-card__level-controls {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.class-selected-card__level-btn {
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  font-size: 0.875rem;
  color: $color-text;

  &:hover:not(:disabled) {
    border-color: $color-accent;
    color: $color-accent;
  }

  &:disabled {
    opacity: 0.3;
    cursor: not-allowed;
  }
}

.class-selected-card__level-value {
  font-size: 1.25rem;
  font-weight: 600;
  min-width: 2.5rem;
  text-align: center;
}

.class-selected-card__section-label {
  display: flex;
  align-items: center;
  gap: $space-2;
  font-size: 0.875rem;
  font-weight: 500;
  color: $color-text-muted;
  margin-bottom: $space-2;
}

.class-selected-card__required {
  color: $color-warning;
  font-weight: 600;
}

.class-selected-card__skill-count {
  color: $color-text-soft;
  font-weight: 400;
}

.class-selected-card__subclass-img-row {
  display: flex;
  align-items: center;
  gap: $space-2;
  margin-top: $space-2;
}

.class-selected-card__subclass-img {
  width: 32px;
  height: 32px;
  border-radius: $radius-md;
  object-fit: cover;
}

.class-selected-card__subclass-name {
  font-size: 0.875rem;
  font-weight: 500;
  color: $color-text;
}

.class-selected-card__subclass-section,
.class-selected-card__skills-section,
.class-selected-card__features-section {
  margin-bottom: $space-3;
}

.class-selected-card__skill-grid {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
}

.skill-option {
  display: flex;
  align-items: center;
  gap: $space-2;
  padding: $space-1 $space-2;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  font-size: 0.85rem;
  color: $color-text;
  transition: all 150ms ease;

  &:hover:not(:disabled) {
    border-color: $color-border-strong;
  }

  &.is-selected {
    border-color: $color-accent;
    background: rgba($color-accent, 0.05);
  }

  &:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }
}

.skill-option__check {
  width: 1rem;
  height: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid $color-border-strong;
  border-radius: $radius-xs;
  flex-shrink: 0;
  color: $color-accent;
  font-size: 0.6rem;

  .is-selected & {
    background: $color-accent;
    border-color: $color-accent;
    color: white;
  }
}

.class-selected-card__features-list {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
}

.feature-tag {
  display: inline-flex;
  align-items: center;
  gap: $space-1;
  padding: $space-1 $space-2;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  font-size: 0.8rem;
  color: $color-text;
  cursor: help;
}

.feature-tag__level {
  font-size: 0.7rem;
  color: $color-text-muted;
}

.feature-tag__optional {
  font-size: 0.65rem;
  font-weight: 600;
  text-transform: uppercase;
  color: $color-accent;
  padding: 0 $space-1;
  background: $color-accent-soft;
  border-radius: $radius-xs;
}

.class-stage__add-section {
  padding: $space-4;
  background: $color-surface-alt;
  border: 1px dashed $color-border-subtle;
  border-radius: $radius-lg;
}

.class-stage__add-title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 $space-3 0;
}

.class-stage__search {
  margin-bottom: $space-4;
}

.class-stage__loading {
  text-align: center;
  padding: $space-6;
  color: $color-text-muted;
}

.class-stage__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: $space-3;
}

.class-card {
  position: relative;
  padding: $space-3;
  color: $color-text;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  cursor: pointer;
  text-align: left;
  transition: all 150ms ease;

  &:hover:not(.is-disabled) {
    border-color: $color-accent;
    background: rgba($color-accent, 0.05);
  }

  &.is-disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  &.has-prereq-warning {
    border-color: $color-warning;
  }
}

.class-card__img {
  width: 48px;
  height: 48px;
  border-radius: $radius-md;
  object-fit: cover;
  margin-bottom: $space-2;
}

.class-card__name {
  font-weight: 600;
  margin-bottom: $space-1;
}

.class-card__info {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.class-card__source {
  font-size: 0.75rem;
  color: $color-text-muted;
}

.class-card__hit-die {
  font-size: 0.75rem;
  font-weight: 600;
  color: $color-accent;
}

.class-card__selected-badge {
  position: absolute;
  top: $space-2;
  right: $space-2;
  font-size: 0.65rem;
  font-weight: 600;
  text-transform: uppercase;
  color: $color-success;
  padding: 2px $space-1;
  background: rgba($color-success, 0.1);
  border-radius: $radius-xs;
}

.class-card__prereq-warning {
  position: absolute;
  top: $space-2;
  right: $space-2;
  color: $color-warning;
  font-size: 0.875rem;
}

.class-stage__empty {
  text-align: center;
  padding: $space-6;
  color: $color-text-muted;
}

.class-stage__no-add {
  padding: $space-4;
  background: rgba($color-accent, 0.05);
  border: 1px solid rgba($color-accent, 0.2);
  border-radius: $radius-lg;
  text-align: center;

  p {
    margin: 0;
    color: $color-text-muted;
    font-size: 0.875rem;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: $space-2;

    i {
      color: $color-accent;
    }
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

.preview-section {
  padding-top: $space-3;
  border-top: 1px solid $color-border-subtle;
}

.preview-section__title {
  font-size: 0.875rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
  color: $color-text;
  display: flex;
  align-items: center;
  gap: $space-2;
}

.preview-section__subtitle {
  font-weight: 400;
  color: $color-text-muted;
}

.preview-stats {
  display: flex;
  gap: $space-4;
  flex-wrap: wrap;
}

.preview-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 80px;
}

.preview-stat__label {
  font-size: 0.7rem;
  text-transform: uppercase;
  color: $color-text-muted;
  margin-bottom: $space-1;
}

.preview-stat__value {
  font-size: 1rem;
  font-weight: 600;
  color: $color-text;
}

.preview-prereq {
  display: flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2;
  border-radius: $radius-sm;
  font-size: 0.875rem;

  &.is-met {
    background: rgba($color-success, 0.1);
    color: $color-success;
  }

  &.is-not-met {
    background: rgba($color-danger, 0.1);
    color: $color-danger;
  }
}

.preview-prereq-note {
  display: flex;
  align-items: center;
  gap: $space-2;
  margin-top: $space-2;
  font-size: 0.8rem;
  color: $color-text-muted;

  i {
    color: $color-accent;
  }
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

.preview-subclasses {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.preview-subclass {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: $space-2;
  background: $color-surface;
  border-radius: $radius-sm;
}

.preview-subclass__name {
  font-weight: 600;
  font-size: 0.875rem;
}

.preview-subclass__desc {
  font-size: 0.8rem;
  color: $color-text-muted;
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
