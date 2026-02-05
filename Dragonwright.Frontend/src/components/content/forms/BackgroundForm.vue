<script setup lang="ts">
import { ref } from 'vue'
import { useContentForm } from '@/composables/useContentForm'
import { getBackgroundsId, postBackgrounds, putBackgroundsId, postBackgroundsBackgroundIdCharacteristics, putBackgroundsBackgroundIdCharacteristicsId, deleteBackgroundsBackgroundIdCharacteristicsId } from '@/api'
import type { Background, Characteristics } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiCheckboxGroup from '@/components/ui/UiCheckboxGroup.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import {
  sourceOptions, abilityScoreOptions, skillOptions, toolOptions,
  itemTypeOptions, weaponTypeOptions, characteristicsTypeOptions, characteristicsTypeLabels,
} from '@/content/enums'
import { commonTips, backgroundTips } from '@/content/tips'

const props = defineProps<{ id?: string }>()

const originalCharacteristics = ref<Characteristics[]>([])

const { form, loading, saving, isEdit, save, cancel } = useContentForm<Background>({
  typeLabel: 'Background',
  listRoute: '/content/backgrounds',
  editRoute: '/content/backgrounds',
  id: props.id,
  defaultData: () => ({
    name: '',
    source: 1,
    languageCount: 0,
    abilityScoreIncreases: [],
    skillProficiencies: [],
    toolProficiencies: [],
    armorProficiencies: [],
    weaponProficiencies: [],
    characteristics: [],
  }),
  getFn: async (id) => {
    const res = await getBackgroundsId(id)
    const data = (res as any).data ?? res
    originalCharacteristics.value = (data.characteristics ?? []).map((c: Characteristics) => ({ ...c }))
    return res
  },
  createFn: postBackgrounds,
  updateFn: putBackgroundsId,
  onAfterSave: async (bgId) => {
    const current = form.value.characteristics ?? []
    const original = originalCharacteristics.value

    // Delete removed characteristics
    for (const orig of original) {
      if (orig.id && !current.some(c => c.id === orig.id)) {
        await deleteBackgroundsBackgroundIdCharacteristicsId(bgId, String(orig.id))
      }
    }

    // Create or update characteristics
    for (const char of current) {
      if (char.id && original.some(o => o.id === char.id)) {
        await putBackgroundsBackgroundIdCharacteristicsId(bgId, String(char.id), char)
      } else {
        await postBackgroundsBackgroundIdCharacteristics(bgId, char)
      }
    }

    originalCharacteristics.value = current.map(c => ({ ...c }))
  },
})

const charModalOpen = ref(false)
const charEditIndex = ref<number | null>(null)
const charForm = ref<Characteristics>({ type: 0, text: '' })

function openAddCharacteristic() {
  charForm.value = { type: 0, text: '' }
  charEditIndex.value = null
  charModalOpen.value = true
}

function openEditCharacteristic(index: number) {
  const c = form.value.characteristics![index]
  charForm.value = { ...c }
  charEditIndex.value = index
  charModalOpen.value = true
}

function saveCharacteristic() {
  if (!form.value.characteristics) form.value.characteristics = []
  if (charEditIndex.value !== null) {
    form.value.characteristics[charEditIndex.value] = { ...charForm.value }
  } else {
    form.value.characteristics.push({ ...charForm.value })
  }
  charModalOpen.value = false
}

function removeCharacteristic(index: number) {
  form.value.characteristics?.splice(index, 1)
}
</script>

<template>
  <ContentFormLayout
    :title="isEdit ? 'Edit Background' : 'New Background'"
    back-route="/content/backgrounds"
    :loading="loading"
    :saving="saving"
    @save="save"
    @cancel="cancel"
  >
    <div class="content-form__section">
      <h3 class="content-form__section-title">Basic Info</h3>
      <div class="content-form__row">
        <UiInput v-model="form.name" label="Name" placeholder="Background name" :tip="commonTips.name" />
        <UiSelect v-model="form.source" label="Source" :options="sourceOptions" :tip="commonTips.source" />
      </div>
      <UiInput v-model.number="form.languageCount as any" label="Language Count" type="number" placeholder="0" :tip="backgroundTips.languageCount" />
    </div>

    <div class="content-form__section">
      <h3 class="content-form__section-title">Proficiencies</h3>
      <UiCheckboxGroup v-model="form.abilityScoreIncreases" label="Ability Score Increases" :options="abilityScoreOptions" :tip="backgroundTips.abilityScoreIncreases" />
      <UiCheckboxGroup v-model="form.skillProficiencies" label="Skill Proficiencies" :options="skillOptions" :tip="backgroundTips.skillProficiencies" />
      <UiCheckboxGroup v-model="form.toolProficiencies" label="Tool Proficiencies" :options="toolOptions" />
      <UiCheckboxGroup v-model="form.armorProficiencies" label="Armor Proficiencies" :options="itemTypeOptions" />
      <UiCheckboxGroup v-model="form.weaponProficiencies" label="Weapon Proficiencies" :options="weaponTypeOptions" />
    </div>

    <div class="content-form__section">
      <div class="content-form__section-header">
        <h3 class="content-form__section-title">Characteristics</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddCharacteristic">Add</UiButton>
      </div>

      <table v-if="form.characteristics?.length" class="content-table content-form__nested-table">
        <thead>
          <tr>
            <th>Type</th>
            <th>Text</th>
            <th class="content-table__actions-col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(c, i) in form.characteristics" :key="i">
            <td>{{ characteristicsTypeLabels[c.type ?? 0] ?? '—' }}</td>
            <td class="content-form__text-cell">{{ c.text }}</td>
            <td class="content-table__actions-col">
              <button type="button" class="content-table__action-btn" title="Edit" @click="openEditCharacteristic(i)">
                <i class="fas fa-pen" />
              </button>
              <button type="button" class="content-table__action-btn content-table__action-btn--danger" title="Delete" @click="removeCharacteristic(i)">
                <i class="fas fa-trash" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else class="content-form__empty-hint">No characteristics added yet.</p>
    </div>

    <UiModal v-model="charModalOpen" :title="charEditIndex !== null ? 'Edit Characteristic' : 'Add Characteristic'" close-on-backdrop close-on-esc>
      <div class="content-form__modal-body">
        <UiSelect v-model="charForm.type" label="Type" :options="characteristicsTypeOptions" />
        <UiTextarea v-model="charForm.text" label="Text" placeholder="Characteristic text..." :rows="3" />
      </div>
      <template #footer>
        <UiButton @click="saveCharacteristic">Save</UiButton>
        <UiButton variant="ghost" @click="charModalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>
  </ContentFormLayout>
</template>
