<script setup lang="ts">
import { ref } from 'vue'
import { useContentForm } from '@/composables/useContentForm'
import { useToast } from '@/composables/useToast'
import {
  getFeatsId, postFeats, putFeatsId,
  postFeatsFeatIdOptions, putFeatsFeatIdOptionsId, deleteFeatsFeatIdOptionsId,
} from '@/api'
import type { Feat, FeatOption } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiCheckboxGroup from '@/components/ui/UiCheckboxGroup.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'
import { sourceOptions, abilityScoreOptions } from '@/content/enums'
import { commonTips, featTips } from '@/content/tips'

const props = defineProps<{ id?: string }>()
const { showToast } = useToast()

const pendingOptions = ref<FeatOption[]>([])

const { form, loading, saving, isEdit, entityId, save, cancel } = useContentForm<Feat>({
  typeLabel: 'Feat',
  listRoute: '/content/feats',
  editRoute: '/content/feats',
  id: props.id,
  defaultData: () => ({
    name: '',
    source: 1,
    description: '',
    featLevel: 1,
    isRepeatable: false,
    prerequisiteDescription: '',
    prerequisiteAbilityScore: null,
    prerequisiteAbilityScoreMinimum: 0,
    prerequisiteSpellcasting: false,
    abilityScoreOptions: [],
    abilityScoreIncrease: 0,
    options: [],
    actions: [],
    spells: [],
    modifiers: [],
  }),
  getFn: getFeatsId,
  createFn: postFeats,
  updateFn: putFeatsId,
  onAfterSave: async (featId) => {
    for (const opt of pendingOptions.value) {
      const { id: _id, ...payload } = opt
      await postFeatsFeatIdOptions(featId, { ...payload, featId } as FeatOption)
    }
    pendingOptions.value = []
  },
})

const optionModalOpen = ref(false)
const optionEditIndex = ref<number | null>(null)
const optionForm = ref<FeatOption>(defaultOption())
const optionSaving = ref(false)

function defaultOption(): FeatOption {
  return {
    name: '',
    description: '',
    requiredCharacterLevel: 1,
    isGranted: false,
  }
}

function openAddOption() {
  optionForm.value = defaultOption()
  optionEditIndex.value = null
  optionModalOpen.value = true
}

function openEditOption(index: number) {
  optionForm.value = { ...form.value.options![index]! } as FeatOption
  optionEditIndex.value = index
  optionModalOpen.value = true
}

async function saveOption() {
  if (!form.value.options) form.value.options = []

  if (isEdit.value) {
    optionSaving.value = true
    try {
      if (optionEditIndex.value !== null) {
        const existing = form.value.options[optionEditIndex.value]!
        if (existing.id) {
          await putFeatsFeatIdOptionsId(entityId.value!, String(existing.id), { ...optionForm.value, featId: entityId.value })
        }
        form.value.options[optionEditIndex.value] = { ...optionForm.value }
      } else {
        const { id: _id, ...payload } = optionForm.value
        const res = await postFeatsFeatIdOptions(entityId.value!, { ...payload, featId: entityId.value } as FeatOption)
        const data = (res as any).data ?? res
        form.value.options.push({ ...optionForm.value, id: data.id })
      }
      showToast({ variant: 'success', message: 'Option saved.' })
    } catch {
      showToast({ variant: 'danger', message: 'Failed to save option.' })
    } finally {
      optionSaving.value = false
    }
  } else {
    if (optionEditIndex.value !== null) {
      pendingOptions.value[optionEditIndex.value] = { ...optionForm.value }
      form.value.options[optionEditIndex.value] = { ...optionForm.value }
    } else {
      pendingOptions.value.push({ ...optionForm.value })
      form.value.options.push({ ...optionForm.value })
    }
  }
  optionModalOpen.value = false
}

async function removeOption(index: number) {
  const opt = form.value.options?.[index]
  if (!opt) return

  if (isEdit.value && opt.id) {
    try {
      await deleteFeatsFeatIdOptionsId(entityId.value!, String(opt.id))
      showToast({ variant: 'success', message: 'Option deleted.' })
    } catch {
      showToast({ variant: 'danger', message: 'Failed to delete option.' })
      return
    }
  } else if (!isEdit.value) {
    pendingOptions.value.splice(index, 1)
  }
  form.value.options?.splice(index, 1)
}
</script>

<template>
  <ContentFormLayout
    :title="isEdit ? 'Edit Feat' : 'New Feat'"
    back-route="/content/feats"
    :loading="loading"
    :saving="saving"
    @save="save"
    @cancel="cancel"
  >
    <div class="content-form__section">
      <h3 class="content-form__section-title">Basic Info</h3>
      <div class="content-form__row">
        <UiInput v-model="form.name" label="Name" placeholder="Feat name" :tip="commonTips.name" />
        <UiSelect v-model="form.source" label="Source" :options="sourceOptions" :tip="commonTips.source" />
      </div>
      <UiTextarea v-model="form.description" label="Description" placeholder="Feat description..." :rows="4" :tip="commonTips.description" />
      <div class="content-form__row">
        <UiInput v-model="form.featLevel" label="Feat Level" type="number" placeholder="1" :tip="featTips.featLevel" />
        <UiCheckbox v-model="form.isRepeatable" label="Repeatable" :tip="featTips.isRepeatable" />
      </div>
    </div>

    <div class="content-form__section">
      <h3 class="content-form__section-title">Prerequisites</h3>
      <UiTextarea v-model="form.prerequisiteDescription" label="Prerequisite Description" placeholder="Description of requirements..." :rows="2" :tip="featTips.prerequisiteDescription" />
      <div class="content-form__row">
        <UiSelect v-model="form.prerequisiteAbilityScore" label="Required Ability" :options="abilityScoreOptions" placeholder="None" :tip="featTips.prerequisiteAbilityScore" />
        <UiInput v-model="form.prerequisiteAbilityScoreMinimum" label="Minimum Score" type="number" placeholder="0" :tip="featTips.prerequisiteAbilityScoreMinimum" />
      </div>
      <UiCheckbox v-model="form.prerequisiteSpellcasting" label="Requires Spellcasting" :tip="featTips.prerequisiteSpellcasting" />
    </div>

    <div class="content-form__section">
      <h3 class="content-form__section-title">Ability Score Increase</h3>
      <UiCheckboxGroup v-model="form.abilityScoreOptions" label="Ability Score Options" :options="abilityScoreOptions" :tip="featTips.abilityScoreOptions" />
      <UiInput v-model="form.abilityScoreIncrease" label="Increase Amount" type="number" placeholder="0" :tip="featTips.abilityScoreIncrease" />
    </div>

    <div class="content-form__section">
      <div class="content-form__section-header">
        <h3 class="content-form__section-title">Options</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddOption">Add</UiButton>
      </div>
      <table v-if="form.options?.length" class="content-table content-form__nested-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Level</th>
            <th class="content-table__actions-col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(o, i) in form.options" :key="i">
            <td>{{ o.name }}</td>
            <td>{{ o.requiredCharacterLevel ?? '—' }}</td>
            <td class="content-table__actions-col">
              <button type="button" class="content-table__action-btn" title="Edit" @click="openEditOption(i)"><i class="fas fa-pen" /></button>
              <button type="button" class="content-table__action-btn content-table__action-btn--danger" title="Delete" @click="removeOption(i)"><i class="fas fa-trash" /></button>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else class="content-form__empty-hint">No options added yet.</p>
    </div>

    <UiModal v-model="optionModalOpen" :title="optionEditIndex !== null ? 'Edit Option' : 'Add Option'" close-on-backdrop close-on-esc>
      <div class="content-form__modal-body">
        <UiInput v-model="optionForm.name" label="Name" placeholder="Option name" :tip="commonTips.name" />
        <UiTextarea v-model="optionForm.description" label="Description" placeholder="Option description..." :rows="3" :tip="commonTips.description" />
        <UiInput v-model="optionForm.requiredCharacterLevel" label="Required Level" type="number" placeholder="1" :tip="commonTips.requiredLevel" />
        <UiCheckbox v-model="optionForm.isGranted" label="Is Granted" />
      </div>
      <template #footer>
        <UiButton @click="saveOption">Save</UiButton>
        <UiButton variant="ghost" @click="optionModalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>
  </ContentFormLayout>
</template>
