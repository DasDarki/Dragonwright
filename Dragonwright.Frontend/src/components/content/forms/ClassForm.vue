<script setup lang="ts">
import { ref } from 'vue'
import { useContentForm } from '@/composables/useContentForm'
import { getClassesId, postClasses, putClassesId, postClassesClassIdFeatures, putClassesClassIdFeaturesId, deleteClassesClassIdFeaturesId, postClassesClassIdSubclasses, putClassesClassIdSubclassesId, deleteClassesClassIdSubclassesId } from '@/api'
import type { Class, ClassFeature, Subclass } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiCheckboxGroup from '@/components/ui/UiCheckboxGroup.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'
import {
  sourceOptions, abilityScoreOptions, skillOptions, toolOptions,
  itemTypeOptions, weaponTypeOptions, featureTypeOptions, featureTypeLabels,
  spellPrepareTypeOptions, spellLearnTypeOptions,
} from '@/content/enums'
import { commonTips, classTips } from '@/content/tips'

const props = defineProps<{ id?: string }>()

const originalFeatures = ref<ClassFeature[]>([])
const originalSubclasses = ref<Subclass[]>([])

const { form, loading, saving, isEdit, save, cancel } = useContentForm<Class>({
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
  }),
  getFn: async (id) => {
    const res = await getClassesId(id)
    const data = (res as any).data ?? res
    originalFeatures.value = (data.features ?? []).map((f: ClassFeature) => ({ ...f }))
    originalSubclasses.value = (data.subclasses ?? []).map((s: Subclass) => ({ ...s }))
    return res
  },
  createFn: postClasses,
  updateFn: putClassesId,
  onAfterSave: async (classId) => {
    const currentFeatures = form.value.features ?? []
    const currentSubclasses = form.value.subclasses ?? []

    for (const orig of originalFeatures.value) {
      if (orig.id && !currentFeatures.some(f => f.id === orig.id)) {
        await deleteClassesClassIdFeaturesId(classId, String(orig.id))
      }
    }
    for (const feature of currentFeatures) {
      if (feature.id && originalFeatures.value.some(o => o.id === feature.id)) {
        await putClassesClassIdFeaturesId(classId, String(feature.id), feature)
      } else {
        await postClassesClassIdFeatures(classId, feature)
      }
    }

    for (const orig of originalSubclasses.value) {
      if (orig.id && !currentSubclasses.some(s => s.id === orig.id)) {
        await deleteClassesClassIdSubclassesId(classId, String(orig.id))
      }
    }
    for (const sub of currentSubclasses) {
      if (sub.id && originalSubclasses.value.some(o => o.id === sub.id)) {
        await putClassesClassIdSubclassesId(classId, String(sub.id), sub)
      } else {
        await postClassesClassIdSubclasses(classId, sub)
      }
    }

    originalFeatures.value = currentFeatures.map(f => ({ ...f }))
    originalSubclasses.value = currentSubclasses.map(s => ({ ...s }))
  },
})

const featureModalOpen = ref(false)
const featureEditIndex = ref<number | null>(null)
const featureForm = ref<ClassFeature>(defaultFeature())

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

function saveFeature() {
  if (!form.value.features) form.value.features = []
  if (featureEditIndex.value !== null) {
    form.value.features[featureEditIndex.value] = { ...featureForm.value }
  } else {
    form.value.features.push({ ...featureForm.value })
  }
  featureModalOpen.value = false
}

function removeFeature(index: number) {
  form.value.features?.splice(index, 1)
}

const subclassModalOpen = ref(false)
const subclassEditIndex = ref<number | null>(null)
const subclassForm = ref<Subclass>(defaultSubclass())

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

function saveSubclass() {
  if (!form.value.subclasses) form.value.subclasses = []
  if (subclassEditIndex.value !== null) {
    form.value.subclasses[subclassEditIndex.value] = { ...subclassForm.value }
  } else {
    form.value.subclasses.push({ ...subclassForm.value })
  }
  subclassModalOpen.value = false
}

function removeSubclass(index: number) {
  form.value.subclasses?.splice(index, 1)
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
