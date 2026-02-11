<script setup lang="ts">
import { ref } from 'vue'
import { useContentForm } from '@/composables/useContentForm'
import { useToast } from '@/composables/useToast'
import {
  getClassesId, postClasses, putClassesId,
  postClassesClassIdFeatures, putClassesClassIdFeaturesId, deleteClassesClassIdFeaturesId,
  postClassesClassIdSubclasses, putClassesClassIdSubclassesId, deleteClassesClassIdSubclassesId,
  postClassesClassIdFeaturesFeatureIdModifiers, putClassesClassIdFeaturesFeatureIdModifiersId, deleteClassesClassIdFeaturesFeatureIdModifiersId,
} from '@/api'
import type { Class, ClassFeature, Subclass } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import ModifierEditor from '@/components/content/ModifierEditor.vue'
import type { Modifier } from '@/content/modifiers'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiCheckboxGroup from '@/components/ui/UiCheckboxGroup.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiEntitySelect from '@/components/ui/UiEntitySelect.vue'
import {
  sourceOptions, abilityScoreOptions, abilityScoreLabels, skillOptions, toolOptions,
  itemTypeOptions, weaponTypeOptions, featureTypeOptions, featureTypeLabels,
  spellPrepareTypeOptions, spellLearnTypeOptions,
} from '@/content/enums'
import { commonTips, classTips } from '@/content/tips'

const props = defineProps<{ id?: string }>()
const { showToast } = useToast()

// Pending sub-entities for create mode (saved when parent is created)
const pendingFeatures = ref<ClassFeature[]>([])
const pendingSubclasses = ref<Subclass[]>([])

const { form, loading, saving, isEdit, entityId, save, cancel } = useContentForm<Class>({
  typeLabel: 'Class',
  listRoute: '/content/classes',
  editRoute: '/content/classes',
  id: props.id,
  defaultData: () => ({
    name: '',
    source: 1,
    hitDie: 8,
    fixHitPointsPerLevelAfterFirst: 5,
    baseHitPointsAtFirstLevel: 8,
    hitPointsModifierAbilityScore: 2,
    primaryAbilityScores: [],
    savingThrowProficiencies: [],
    skillProficienciesCount: 2,
    skillProficienciesOptions: [],
    toolProficiencies: [],
    armorProficiencies: [],
    weaponProficiencies: [],
    features: [],
    subclasses: [],
    subclassSelectionLevel: 3,
    standardArray: [15, 14, 13, 12, 10, 8],
    multiclassingRequirements: {},
    multiclassingRequirementsAlt: {},
  }),
  getFn: getClassesId,
  createFn: postClasses,
  updateFn: putClassesId,
  onAfterSave: async (classId) => {
    for (const feature of pendingFeatures.value) {
      const { id: _id, ...payload } = feature
      await postClassesClassIdFeatures(classId, payload as ClassFeature)
    }
    for (const sub of pendingSubclasses.value) {
      const { id: _id, ...rest } = sub
      await postClassesClassIdSubclasses(classId, { ...rest, classId } as Subclass)
    }
    pendingFeatures.value = []
    pendingSubclasses.value = []
  },
})

const featureModalOpen = ref(false)
const featureEditIndex = ref<number | null>(null)
const featureForm = ref<ClassFeature>(defaultFeature())
const featureSaving = ref(false)

function defaultFeature(): ClassFeature {
  return {
    name: '',
    description: '',
    displayOrder: 0,
    requiredCharacterLevel: 1,
    featureType: 0,
    isOptional: false,
    hideInBuilder: false,
    hideInCharacterSheet: false,
    hasOptions: false,
    displayRequiredLevel: true,
    modifiers: [],
    featureToReplaceId: null,
  }
}

function fetchOtherFeatures() {
  const currentId = featureForm.value.id
  const allFeatures = form.value.features ?? []
  const filtered = currentId
    ? allFeatures.filter(f => f.id !== currentId)
    : allFeatures
  return Promise.resolve({ items: filtered, page: 1, totalCount: filtered.length })
}

function getFeatureLabel(feature: ClassFeature) {
  return feature.name || '(unnamed)'
}

function getFeatureValue(feature: ClassFeature) {
  return feature.id as string
}

async function onFeatureModifierSave(modifier: Modifier, index: number | null) {
  const featureId = featureForm.value.id
  if (!isEdit.value || !featureId) return

  try {
    if (index !== null && modifier.id) {
      await putClassesClassIdFeaturesFeatureIdModifiersId(entityId.value!, String(featureId), String(modifier.id), modifier as any)
    } else {
      const { id: _id, ...payload } = modifier
      const res = await postClassesClassIdFeaturesFeatureIdModifiers(entityId.value!, String(featureId), payload as any)
      const data = (res as any).data ?? res
      if (featureForm.value.modifiers && index === null) {
        featureForm.value.modifiers[featureForm.value.modifiers.length - 1] = { ...modifier, id: data.id }
      }
    }
    showToast({ variant: 'success', message: 'Modifier saved.' })
  } catch {
    showToast({ variant: 'danger', message: 'Failed to save modifier.' })
  }
}

async function onFeatureModifierDelete(modifier: Modifier, _index: number) {
  const featureId = featureForm.value.id
  if (!isEdit.value || !featureId || !modifier.id) return

  try {
    await deleteClassesClassIdFeaturesFeatureIdModifiersId(entityId.value!, String(featureId), String(modifier.id))
    showToast({ variant: 'success', message: 'Modifier deleted.' })
  } catch {
    showToast({ variant: 'danger', message: 'Failed to delete modifier.' })
  }
}

function openAddFeature() {
  featureForm.value = defaultFeature()
  featureEditIndex.value = null
  featureModalOpen.value = true
}

function openEditFeature(index: number) {
  featureForm.value = { ...form.value.features![index] } as ClassFeature
  featureEditIndex.value = index
  featureModalOpen.value = true
}

async function saveFeature() {
  if (!form.value.features) form.value.features = []

  if (isEdit.value) {
    // Edit mode: save to backend immediately
    featureSaving.value = true
    try {
      if (featureEditIndex.value !== null) {
        const existing = form.value.features[featureEditIndex.value]!
        if (existing.id) {
          await putClassesClassIdFeaturesId(entityId.value!, String(existing.id), featureForm.value)
        }
        form.value.features[featureEditIndex.value] = { ...featureForm.value }
      } else {
        // Strip id when creating new - backend assigns it
        const { id: _id, ...payload } = featureForm.value
        const res = await postClassesClassIdFeatures(entityId.value!, payload as ClassFeature)
        const data = (res as any).data ?? res
        form.value.features.push({ ...featureForm.value, id: data.id })
      }
      showToast({ variant: 'success', message: 'Feature saved.' })
    } catch {
      showToast({ variant: 'danger', message: 'Failed to save feature.' })
    } finally {
      featureSaving.value = false
    }
  } else {
    // Create mode: store locally until parent is saved
    if (featureEditIndex.value !== null) {
      pendingFeatures.value[featureEditIndex.value] = { ...featureForm.value }
      form.value.features[featureEditIndex.value] = { ...featureForm.value }
    } else {
      pendingFeatures.value.push({ ...featureForm.value })
      form.value.features.push({ ...featureForm.value })
    }
  }
  featureModalOpen.value = false
}

async function removeFeature(index: number) {
  const feature = form.value.features?.[index]
  if (!feature) return

  if (isEdit.value && feature.id) {
    try {
      await deleteClassesClassIdFeaturesId(entityId.value!, String(feature.id))
      showToast({ variant: 'success', message: 'Feature deleted.' })
    } catch {
      showToast({ variant: 'danger', message: 'Failed to delete feature.' })
      return
    }
  } else if (!isEdit.value) {
    pendingFeatures.value.splice(index, 1)
  }
  form.value.features?.splice(index, 1)
}

const subclassModalOpen = ref(false)
const subclassEditIndex = ref<number | null>(null)
const subclassForm = ref<Subclass>(defaultSubclass())
const subclassSaving = ref(false)

function defaultSubclass(): Subclass {
  return {
    name: '',
    shortDescription: '',
    description: '',
    canCastSpells: false,
    spellcastingAbility: null,
    knowsAllSpells: false,
    spellPrepareType: null,
    spellLearnType: null,
  }
}

function openAddSubclass() {
  subclassForm.value = defaultSubclass()
  subclassEditIndex.value = null
  subclassModalOpen.value = true
}

function openEditSubclass(index: number) {
  subclassForm.value = { ...form.value.subclasses![index] } as Subclass
  subclassEditIndex.value = index
  subclassModalOpen.value = true
}

async function saveSubclass() {
  if (!form.value.subclasses) form.value.subclasses = []

  if (isEdit.value) {
    // Edit mode: save to backend immediately
    subclassSaving.value = true
    try {
      if (subclassEditIndex.value !== null) {
        const existing = form.value.subclasses[subclassEditIndex.value]!
        if (existing.id) {
          const payload = { ...subclassForm.value, classId: entityId.value }
          await putClassesClassIdSubclassesId(entityId.value!, String(existing.id), payload)
        }
        form.value.subclasses[subclassEditIndex.value] = { ...subclassForm.value }
      } else {
        // Strip id when creating new - backend assigns it
        const { id: _id, ...rest } = subclassForm.value
        const payload = { ...rest, classId: entityId.value }
        const res = await postClassesClassIdSubclasses(entityId.value!, payload as Subclass)
        const data = (res as any).data ?? res
        form.value.subclasses.push({ ...subclassForm.value, id: data.id })
      }
      showToast({ variant: 'success', message: 'Subclass saved.' })
    } catch {
      showToast({ variant: 'danger', message: 'Failed to save subclass.' })
    } finally {
      subclassSaving.value = false
    }
  } else {
    // Create mode: store locally until parent is saved
    if (subclassEditIndex.value !== null) {
      pendingSubclasses.value[subclassEditIndex.value] = { ...subclassForm.value }
      form.value.subclasses[subclassEditIndex.value] = { ...subclassForm.value }
    } else {
      pendingSubclasses.value.push({ ...subclassForm.value })
      form.value.subclasses.push({ ...subclassForm.value })
    }
  }
  subclassModalOpen.value = false
}

async function removeSubclass(index: number) {
  const subclass = form.value.subclasses?.[index]
  if (!subclass) return

  if (isEdit.value && subclass.id) {
    try {
      await deleteClassesClassIdSubclassesId(entityId.value!, String(subclass.id))
      showToast({ variant: 'success', message: 'Subclass deleted.' })
    } catch {
      showToast({ variant: 'danger', message: 'Failed to delete subclass.' })
      return
    }
  } else if (!isEdit.value) {
    pendingSubclasses.value.splice(index, 1)
  }
  form.value.subclasses?.splice(index, 1)
}

function getMulticlassingRequirement(ability: string): number {
  const reqs = form.value.multiclassingRequirements as Record<string, number> | undefined
  return reqs?.[ability] ?? 0
}

function setMulticlassingRequirement(ability: string, value: number) {
  if (!form.value.multiclassingRequirements) {
    form.value.multiclassingRequirements = {}
  }
  const reqs = form.value.multiclassingRequirements as Record<string, number>
  if (value > 0) {
    reqs[ability] = value
  } else {
    delete reqs[ability]
  }
  form.value.multiclassingRequirements = { ...reqs }
}

function getMulticlassingRequirementAlt(ability: string): number {
  const reqs = form.value.multiclassingRequirementsAlt as Record<string, number> | undefined
  return reqs?.[ability] ?? 0
}

function setMulticlassingRequirementAlt(ability: string, value: number) {
  if (!form.value.multiclassingRequirementsAlt) {
    form.value.multiclassingRequirementsAlt = {}
  }
  const reqs = form.value.multiclassingRequirementsAlt as Record<string, number>
  if (value > 0) {
    reqs[ability] = value
  } else {
    delete reqs[ability]
  }
  form.value.multiclassingRequirementsAlt = { ...reqs }
}

function getStandardArrayValue(index: number): number {
  const arr = form.value.standardArray as number[] | undefined
  return arr?.[index] ?? 0
}

function setStandardArrayValue(index: number, value: number) {
  if (!form.value.standardArray) {
    form.value.standardArray = [15, 14, 13, 12, 10, 8]
  }
  const arr = [...(form.value.standardArray as number[])]
  arr[index] = value
  form.value.standardArray = arr
}
</script>

<template>
  <ContentFormLayout
    :title="isEdit ? 'Edit Class' : 'New Class'"
    back-route="/content/classes"
    :loading="loading"
    :saving="saving"
    @save="save"
    @cancel="cancel"
  >
    <!-- Basic Info -->
    <div class="content-form__section">
      <h3 class="content-form__section-title">Basic Info</h3>
      <div class="content-form__row">
        <UiInput v-model="form.name" label="Name" placeholder="Class name" :tip="commonTips.name" />
        <UiSelect v-model="form.source" label="Source" :options="sourceOptions" :tip="commonTips.source" />
      </div>
    </div>

    <!-- Hit Points -->
    <div class="content-form__section">
      <h3 class="content-form__section-title">Hit Points</h3>
      <div class="content-form__row">
        <UiInput v-model="form.hitDie" label="Hit Die" type="number" placeholder="8" :tip="classTips.hitDie" />
        <UiInput v-model="form.baseHitPointsAtFirstLevel" label="Base HP at 1st Level" type="number" placeholder="8" :tip="classTips.baseHp" />
      </div>
      <div class="content-form__row">
        <UiInput v-model="form.fixHitPointsPerLevelAfterFirst" label="Fix HP Per Level After 1st" type="number" placeholder="5" :tip="classTips.fixHpPerLevel" />
        <UiSelect v-model="form.hitPointsModifierAbilityScore" label="HP Modifier Ability" :options="abilityScoreOptions" placeholder="None" :tip="classTips.hpModifierAbility" />
      </div>
    </div>

    <!-- Proficiencies -->
    <div class="content-form__section">
      <h3 class="content-form__section-title">Ability Scores & Proficiencies</h3>
      <UiCheckboxGroup v-model="form.primaryAbilityScores" label="Primary Ability Scores" :options="abilityScoreOptions" :tip="classTips.primaryAbilityScores" />
      <UiCheckboxGroup v-model="form.savingThrowProficiencies" label="Saving Throw Proficiencies" :options="abilityScoreOptions" :tip="classTips.savingThrows" />
      <div class="content-form__row">
        <UiInput v-model="form.skillProficienciesCount" label="Skill Proficiency Count" type="number" placeholder="2" :tip="classTips.skillCount" />
      </div>
      <UiCheckboxGroup v-model="form.skillProficienciesOptions" label="Skill Proficiency Options" :options="skillOptions" :tip="classTips.skillOptions" />
      <UiCheckboxGroup v-model="form.toolProficiencies" label="Tool Proficiencies" :options="toolOptions" :tip="classTips.toolProficiencies" />
      <UiCheckboxGroup v-model="form.armorProficiencies" label="Armor Proficiencies" :options="itemTypeOptions" :tip="classTips.armorProficiencies" />
      <UiCheckboxGroup v-model="form.weaponProficiencies" label="Weapon Proficiencies" :options="weaponTypeOptions" :tip="classTips.weaponProficiencies" />
    </div>

    <!-- Multiclassing & Advancement -->
    <div class="content-form__section">
      <h3 class="content-form__section-title">Multiclassing & Advancement</h3>
      <div class="content-form__row">
        <UiInput v-model="form.subclassSelectionLevel" label="Subclass Selection Level" type="number" placeholder="3" :tip="classTips.subclassSelectionLevel" />
      </div>
      <div class="content-form__subsection">
        <label class="content-form__label">Standard Array <span class="content-form__label-tip" :title="classTips.standardArray.body">(i)</span></label>
        <div class="content-form__array-grid">
          <UiInput
            v-for="i in 6"
            :key="i"
            :model-value="getStandardArrayValue(i - 1)"
            type="number"
            :placeholder="String([15, 14, 13, 12, 10, 8][i - 1])"
            @update:model-value="setStandardArrayValue(i - 1, Number($event))"
          />
        </div>
      </div>
      <div class="content-form__subsection">
        <label class="content-form__label">Multiclassing Requirements (AND) <span class="content-form__label-tip" :title="classTips.multiclassingRequirements.body">(i)</span></label>
        <div class="content-form__ability-grid">
          <div v-for="(label, ability) in abilityScoreLabels" :key="ability" class="content-form__ability-req">
            <span class="content-form__ability-label">{{ label.slice(0, 3).toUpperCase() }}</span>
            <UiInput
              :model-value="getMulticlassingRequirement(label)"
              type="number"
              placeholder="0"
              @update:model-value="setMulticlassingRequirement(label, Number($event))"
            />
          </div>
        </div>
      </div>
      <div class="content-form__subsection">
        <label class="content-form__label">Alternative Requirements (OR) <span class="content-form__label-tip" :title="classTips.multiclassingRequirements.body">(i)</span></label>
        <div class="content-form__ability-grid">
          <div v-for="(label, ability) in abilityScoreLabels" :key="ability" class="content-form__ability-req">
            <span class="content-form__ability-label">{{ label.slice(0, 3).toUpperCase() }}</span>
            <UiInput
              :model-value="getMulticlassingRequirementAlt(label)"
              type="number"
              placeholder="0"
              @update:model-value="setMulticlassingRequirementAlt(label, Number($event))"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Features -->
    <div class="content-form__section">
      <div class="content-form__section-header">
        <h3 class="content-form__section-title">Features</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddFeature">Add</UiButton>
      </div>
      <table v-if="form.features?.length" class="content-table content-form__nested-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Type</th>
            <th>Level</th>
            <th class="content-table__actions-col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(f, i) in form.features" :key="i">
            <td>{{ f.name }}</td>
            <td>{{ featureTypeLabels[f.featureType ?? 0] ?? '—' }}</td>
            <td>{{ f.requiredCharacterLevel ?? '—' }}</td>
            <td class="content-table__actions-col">
              <button type="button" class="content-table__action-btn" title="Edit" @click="openEditFeature(i)"><i class="fas fa-pen" /></button>
              <button type="button" class="content-table__action-btn content-table__action-btn--danger" title="Delete" @click="removeFeature(i)"><i class="fas fa-trash" /></button>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else class="content-form__empty-hint">No features added yet.</p>
    </div>

    <!-- Subclasses -->
    <div class="content-form__section">
      <div class="content-form__section-header">
        <h3 class="content-form__section-title">Subclasses</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddSubclass">Add</UiButton>
      </div>
      <table v-if="form.subclasses?.length" class="content-table content-form__nested-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th class="content-table__actions-col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(s, i) in form.subclasses" :key="i">
            <td>{{ s.name }}</td>
            <td class="content-form__text-cell">{{ s.shortDescription }}</td>
            <td class="content-table__actions-col">
              <button type="button" class="content-table__action-btn" title="Edit" @click="openEditSubclass(i)"><i class="fas fa-pen" /></button>
              <button type="button" class="content-table__action-btn content-table__action-btn--danger" title="Delete" @click="removeSubclass(i)"><i class="fas fa-trash" /></button>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else class="content-form__empty-hint">No subclasses added yet.</p>
    </div>

    <!-- Feature Modal -->
    <UiModal v-model="featureModalOpen" :title="featureEditIndex !== null ? 'Edit Feature' : 'Add Feature'" close-on-backdrop close-on-esc>
      <div class="content-form__modal-body">
        <UiInput v-model="featureForm.name" label="Name" placeholder="Feature name" :tip="commonTips.name" />
        <UiTextarea v-model="featureForm.description" label="Description" placeholder="Feature description..." :rows="4" :tip="commonTips.description" />
        <div class="content-form__row">
          <UiSelect v-model="featureForm.featureType" label="Feature Type" :options="featureTypeOptions" :tip="commonTips.featureType" />
          <UiInput v-model="featureForm.requiredCharacterLevel" label="Required Level" type="number" placeholder="1" :tip="commonTips.requiredLevel" />
        </div>
        <UiInput v-model="featureForm.displayOrder" label="Display Order" type="number" placeholder="0" :tip="commonTips.displayOrder" />
        <div class="content-form__row">
          <UiCheckbox v-model="featureForm.isOptional" label="Optional" :tip="classTips.featureOptional" />
          <UiCheckbox v-model="featureForm.hasOptions" label="Has Options" :tip="classTips.featureHasOptions" />
          <UiCheckbox v-model="featureForm.displayRequiredLevel" label="Show Level" :tip="classTips.featureShowLevel" />
        </div>
        <div class="content-form__row">
          <UiCheckbox v-model="featureForm.hideInBuilder" label="Hide in Builder" :tip="classTips.featureHideBuilder" />
          <UiCheckbox v-model="featureForm.hideInCharacterSheet" label="Hide in Sheet" :tip="classTips.featureHideSheet" />
        </div>
        <UiEntitySelect
          v-model="featureForm.featureToReplaceId"
          label="Replaces Feature"
          placeholder="None (new feature)"
          :fetch-fn="fetchOtherFeatures"
          :get-option-label="getFeatureLabel"
          :get-option-value="getFeatureValue"
          clearable
          :tip="{ title: 'Replaces Feature', body: 'If this feature replaces another feature (e.g., an optional feature replacing a base class feature), select the feature it replaces.' }"
        />
        <div class="content-form__section-divider" />
        <ModifierEditor
          :model-value="(featureForm.modifiers ?? []) as any"
          @update:model-value="featureForm.modifiers = $event as any"
          @save="onFeatureModifierSave"
          @delete="onFeatureModifierDelete"
        />
      </div>
      <template #footer>
        <UiButton @click="saveFeature">Save</UiButton>
        <UiButton variant="ghost" @click="featureModalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>

    <!-- Subclass Modal -->
    <UiModal v-model="subclassModalOpen" :title="subclassEditIndex !== null ? 'Edit Subclass' : 'Add Subclass'" close-on-backdrop close-on-esc>
      <div class="content-form__modal-body">
        <UiInput v-model="subclassForm.name" label="Name" placeholder="Subclass name" :tip="commonTips.name" />
        <UiInput v-model="subclassForm.shortDescription" label="Short Description" placeholder="Brief description" />
        <UiTextarea v-model="subclassForm.description" label="Description" placeholder="Full subclass description..." :rows="4" :tip="commonTips.description" />
        <UiCheckbox v-model="subclassForm.canCastSpells" label="Can Cast Spells" :tip="classTips.subclassCanCastSpells" />
        <template v-if="subclassForm.canCastSpells">
          <UiSelect v-model="subclassForm.spellcastingAbility" label="Spellcasting Ability" :options="abilityScoreOptions" placeholder="None" :tip="classTips.subclassSpellcastingAbility" />
          <UiCheckbox v-model="subclassForm.knowsAllSpells" label="Knows All Spells" :tip="classTips.subclassKnowsAllSpells" />
          <div class="content-form__row">
            <UiSelect v-model="subclassForm.spellPrepareType" label="Spell Prepare Type" :options="spellPrepareTypeOptions" placeholder="None" :tip="classTips.subclassPrepareType" />
            <UiSelect v-model="subclassForm.spellLearnType" label="Spell Learn Type" :options="spellLearnTypeOptions" placeholder="None" :tip="classTips.subclassLearnType" />
          </div>
        </template>
      </div>
      <template #footer>
        <UiButton @click="saveSubclass">Save</UiButton>
        <UiButton variant="ghost" @click="subclassModalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>
  </ContentFormLayout>
</template>
