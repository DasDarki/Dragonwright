<script setup lang="ts">
import { ref } from 'vue'
import { useContentForm } from '@/composables/useContentForm'
import { getSpellsId, postSpells, putSpellsId } from '@/api'
import type { Spell, Time, AttackDamage } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiCheckboxGroup from '@/components/ui/UiCheckboxGroup.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'
import {
  sourceOptions, spellLevelOptions, spellSchoolOptions, attackTypeOptions,
  abilityScoreOptions, shapeOptions, damageTypeOptions, conditionOptions,
  timeUnitOptions, timeUnitLabels, damageTypeLabels,
} from '@/content/enums'
import { commonTips, spellTips, timeValueTip, timeUnitTip, diceCountTip, diceValueTip, damageBonusTip } from '@/content/tips'

const props = defineProps<{ id?: string }>()

const { form, loading, saving, isEdit, save, cancel } = useContentForm<Spell>({
  typeLabel: 'Spell',
  listRoute: '/content/spells',
  editRoute: '/content/spells',
  id: props.id,
  defaultData: () => ({
    name: '',
    level: 0,
    source: 1,
    school: 0,
    description: '',
    hasVocalComponent: false,
    hasSomaticComponent: false,
    hasMaterialComponent: false,
    materialComponents: '',
    concentration: false,
    ritual: false,
    attackType: 0,
    save: null,
    range: 0,
    areaOfEffect: null,
    areaSize: 0,
    castingTimes: [],
    durations: [],
    damageTypes: [],
    damages: [],
    conditions: [],
    tags: [],
    classIds: [],
  }),
  getFn: getSpellsId,
  createFn: postSpells,
  updateFn: putSpellsId,
})

const tagsInput = ref('')

function updateTags() {
  form.value.tags = tagsInput.value
    .split(',')
    .map(t => t.trim())
    .filter(Boolean)
}

const timeModalOpen = ref(false)
const timeModalTarget = ref<'castingTimes' | 'durations'>('castingTimes')
const timeEditIndex = ref<number | null>(null)
const timeForm = ref<Time>({ value: 1, unit: 0 })

function openAddTime(target: 'castingTimes' | 'durations') {
  timeModalTarget.value = target
  timeForm.value = { value: 1, unit: 0 }
  timeEditIndex.value = null
  timeModalOpen.value = true
}

function openEditTime(target: 'castingTimes' | 'durations', index: number) {
  timeModalTarget.value = target
  const arr = form.value[target] ?? []
  timeForm.value = { ...arr[index] }
  timeEditIndex.value = index
  timeModalOpen.value = true
}

function saveTime() {
  const arr = form.value[timeModalTarget.value] ?? []
  if (timeEditIndex.value !== null) {
    arr[timeEditIndex.value] = { ...timeForm.value }
  } else {
    arr.push({ ...timeForm.value })
  }
  ;(form.value as any)[timeModalTarget.value] = arr
  timeModalOpen.value = false
}

function removeTime(target: 'castingTimes' | 'durations', index: number) {
  form.value[target]?.splice(index, 1)
}

function formatTime(t: Time): string {
  return `${t.value ?? ''} ${timeUnitLabels[t.unit ?? 0] ?? ''}`
}

const dmgModalOpen = ref(false)
const dmgEditIndex = ref<number | null>(null)
const dmgForm = ref<AttackDamage>({ diceCount: 1, diceValue: 6, bonus: 0, damageType: 0 })

function openAddDamage() {
  dmgForm.value = { diceCount: 1, diceValue: 6, bonus: 0, damageType: 0 }
  dmgEditIndex.value = null
  dmgModalOpen.value = true
}

function openEditDamage(index: number) {
  dmgForm.value = { ...form.value.damages![index] }
  dmgEditIndex.value = index
  dmgModalOpen.value = true
}

function saveDamage() {
  if (!form.value.damages) form.value.damages = []
  if (dmgEditIndex.value !== null) {
    form.value.damages[dmgEditIndex.value] = { ...dmgForm.value }
  } else {
    form.value.damages.push({ ...dmgForm.value })
  }
  dmgModalOpen.value = false
}

function removeDamage(index: number) {
  form.value.damages?.splice(index, 1)
}
</script>

<template>
  <ContentFormLayout
    :title="isEdit ? 'Edit Spell' : 'New Spell'"
    back-route="/content/spells"
    :loading="loading"
    :saving="saving"
    @save="save"
    @cancel="cancel"
  >
    <div class="content-form__section">
      <h3 class="content-form__section-title">Basic Info</h3>
      <div class="content-form__row">
        <UiInput v-model="form.name" label="Name" placeholder="Spell name" :tip="commonTips.name" />
        <UiSelect v-model="form.source" label="Source" :options="sourceOptions" :tip="commonTips.source" />
      </div>
      <div class="content-form__row">
        <UiSelect v-model="form.level" label="Level" :options="spellLevelOptions" :tip="spellTips.level" />
        <UiSelect v-model="form.school" label="School" :options="spellSchoolOptions" :tip="spellTips.school" />
      </div>
      <UiTextarea v-model="form.description" label="Description" placeholder="Spell description..." :rows="5" :tip="commonTips.description" />
    </div>

    <div class="content-form__section">
      <h3 class="content-form__section-title">Components</h3>
      <div class="content-form__row">
        <UiCheckbox v-model="form.hasVocalComponent" label="Vocal (V)" :tip="spellTips.vocalComponent" />
        <UiCheckbox v-model="form.hasSomaticComponent" label="Somatic (S)" :tip="spellTips.somaticComponent" />
        <UiCheckbox v-model="form.hasMaterialComponent" label="Material (M)" :tip="spellTips.materialComponent" />
      </div>
      <UiInput
        v-if="form.hasMaterialComponent"
        v-model="form.materialComponents"
        label="Material Components"
        placeholder="e.g., a pinch of sulfur"
        :tip="spellTips.materialComponents"
      />
    </div>

    <div class="content-form__section">
      <h3 class="content-form__section-title">Casting Properties</h3>
      <div class="content-form__row">
        <UiCheckbox v-model="form.concentration" label="Concentration" :tip="spellTips.concentration" />
        <UiCheckbox v-model="form.ritual" label="Ritual" :tip="spellTips.ritual" />
      </div>
      <div class="content-form__row">
        <UiSelect v-model="form.attackType" label="Attack Type" :options="attackTypeOptions" :tip="spellTips.attackType" />
        <UiSelect v-model="form.save" label="Saving Throw" :options="abilityScoreOptions" placeholder="None" :tip="spellTips.save" />
      </div>
      <div class="content-form__row">
        <UiInput v-model="form.range" label="Range (ft)" type="number" placeholder="0" :tip="spellTips.range" />
        <UiSelect v-model="form.areaOfEffect" label="Area of Effect" :options="shapeOptions" placeholder="None" :tip="spellTips.areaOfEffect" />
        <UiInput v-if="form.areaOfEffect !== null && form.areaOfEffect !== undefined" v-model="form.areaSize" label="Area Size (ft)" type="number" placeholder="0" :tip="spellTips.areaSize" />
      </div>
    </div>

    <div class="content-form__section">
      <div class="content-form__section-header">
        <h3 class="content-form__section-title">Casting Times</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddTime('castingTimes')">Add</UiButton>
      </div>
      <div v-if="form.castingTimes?.length" class="content-form__chip-list">
        <span v-for="(t, i) in form.castingTimes" :key="i" class="content-form__chip">
          {{ formatTime(t) }}
          <button type="button" class="content-form__chip-remove" @click="openEditTime('castingTimes', i)"><i class="fas fa-pen" /></button>
          <button type="button" class="content-form__chip-remove" @click="removeTime('castingTimes', i)"><i class="fas fa-xmark" /></button>
        </span>
      </div>
      <p v-else class="content-form__empty-hint">No casting times added.</p>
    </div>

    <div class="content-form__section">
      <div class="content-form__section-header">
        <h3 class="content-form__section-title">Durations</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddTime('durations')">Add</UiButton>
      </div>
      <div v-if="form.durations?.length" class="content-form__chip-list">
        <span v-for="(t, i) in form.durations" :key="i" class="content-form__chip">
          {{ formatTime(t) }}
          <button type="button" class="content-form__chip-remove" @click="openEditTime('durations', i)"><i class="fas fa-pen" /></button>
          <button type="button" class="content-form__chip-remove" @click="removeTime('durations', i)"><i class="fas fa-xmark" /></button>
        </span>
      </div>
      <p v-else class="content-form__empty-hint">No durations added.</p>
    </div>

    <div class="content-form__section">
      <h3 class="content-form__section-title">Damage & Conditions</h3>
      <UiCheckboxGroup v-model="form.damageTypes" label="Damage Types" :options="damageTypeOptions" :tip="spellTips.damageTypes" />
      <UiCheckboxGroup v-model="form.conditions" label="Conditions" :options="conditionOptions" :tip="spellTips.conditions" />

      <div class="content-form__section-header" style="margin-top: 1rem">
        <h3 class="content-form__section-title">Damage Rolls</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddDamage">Add</UiButton>
      </div>
      <table v-if="form.damages?.length" class="content-table content-form__nested-table">
        <thead>
          <tr>
            <th>Dice</th>
            <th>Bonus</th>
            <th>Type</th>
            <th class="content-table__actions-col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(d, i) in form.damages" :key="i">
            <td>{{ d.diceCount }}d{{ d.diceValue }}</td>
            <td>{{ d.bonus ? `+${d.bonus}` : '—' }}</td>
            <td>{{ damageTypeLabels[d.damageType ?? 0] ?? '—' }}</td>
            <td class="content-table__actions-col">
              <button type="button" class="content-table__action-btn" title="Edit" @click="openEditDamage(i)"><i class="fas fa-pen" /></button>
              <button type="button" class="content-table__action-btn content-table__action-btn--danger" title="Delete" @click="removeDamage(i)"><i class="fas fa-trash" /></button>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else class="content-form__empty-hint">No damage rolls added.</p>
    </div>

    <div class="content-form__section">
      <h3 class="content-form__section-title">Tags</h3>
      <UiInput
        v-model="tagsInput"
        label="Tags"
        placeholder="Comma-separated tags"
        hint="Separate multiple tags with commas"
        :tip="spellTips.tags"
        @blur="updateTags"
      />
    </div>

    <UiModal v-model="timeModalOpen" :title="timeEditIndex !== null ? 'Edit Time' : 'Add Time'" close-on-backdrop close-on-esc>
      <div class="content-form__modal-body">
        <UiInput v-model="timeForm.value" label="Value" type="number" placeholder="1" :tip="timeValueTip" />
        <UiSelect v-model="timeForm.unit" label="Unit" :options="timeUnitOptions" :tip="timeUnitTip" />
      </div>
      <template #footer>
        <UiButton @click="saveTime">Save</UiButton>
        <UiButton variant="ghost" @click="timeModalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>

    <UiModal v-model="dmgModalOpen" :title="dmgEditIndex !== null ? 'Edit Damage' : 'Add Damage'" close-on-backdrop close-on-esc>
      <div class="content-form__modal-body">
        <div class="content-form__row">
          <UiInput v-model="dmgForm.diceCount" label="Dice Count" type="number" placeholder="1" :tip="diceCountTip" />
          <UiInput v-model="dmgForm.diceValue" label="Dice Value" type="number" placeholder="6" :tip="diceValueTip" />
          <UiInput v-model="dmgForm.bonus" label="Bonus" type="number" placeholder="0" :tip="damageBonusTip" />
        </div>
        <UiSelect v-model="dmgForm.damageType" label="Damage Type" :options="damageTypeOptions" :tip="spellTips.damageTypes" />
      </div>
      <template #footer>
        <UiButton @click="saveDamage">Save</UiButton>
        <UiButton variant="ghost" @click="dmgModalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>
  </ContentFormLayout>
</template>
