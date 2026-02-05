<script setup lang="ts">
import { ref } from 'vue'
import { useContentForm } from '@/composables/useContentForm'
import { useToast } from '@/composables/useToast'
import { getRacesId, postRaces, putRacesId, postRacesRaceIdTraits, putRacesRaceIdTraitsId, deleteRacesRaceIdTraitsId } from '@/api'
import type { Race, RaceTrait } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'
import { sourceOptions, creatureTypeOptions, featureTypeOptions, featureTypeLabels } from '@/content/enums'
import { commonTips, raceTips } from '@/content/tips'

const props = defineProps<{ id?: string }>()
const { showToast } = useToast()

// Pending traits for create mode
const pendingTraits = ref<RaceTrait[]>([])

const { form, loading, saving, isEdit, entityId, save, cancel } = useContentForm<Race>({
  typeLabel: 'Race',
  listRoute: '/content/races',
  editRoute: '/content/races',
  id: props.id,
  defaultData: () => ({
    name: '',
    source: 1,
    type: 9,
    traits: [],
  }),
  getFn: getRacesId,
  createFn: postRaces,
  updateFn: putRacesId,
  onAfterSave: async (raceId) => {
    // Save pending traits after parent creation
    for (const trait of pendingTraits.value) {
      const { id: _id, ...payload } = trait
      await postRacesRaceIdTraits(raceId, payload as RaceTrait)
    }
    pendingTraits.value = []
  },
})

// ─── Traits ─────────────────────────────────────────────────

const traitModalOpen = ref(false)
const traitEditIndex = ref<number | null>(null)
const traitForm = ref<RaceTrait>(defaultTrait())
const traitSaving = ref(false)

function defaultTrait(): RaceTrait {
  return { name: '', description: '', displayOrder: 0, requiredCharacterLevel: 1, featureType: 0 }
}

function openAddTrait() {
  traitForm.value = defaultTrait()
  traitEditIndex.value = null
  traitModalOpen.value = true
}

function openEditTrait(index: number) {
  const t = form.value.traits![index]
  traitForm.value = { ...t } as RaceTrait
  traitEditIndex.value = index
  traitModalOpen.value = true
}

async function saveTrait() {
  if (!form.value.traits) form.value.traits = []

  if (isEdit.value) {
    // Edit mode: save to backend immediately
    traitSaving.value = true
    try {
      if (traitEditIndex.value !== null) {
        const existing = form.value.traits[traitEditIndex.value]!
        if (existing.id) {
          await putRacesRaceIdTraitsId(entityId.value!, String(existing.id), traitForm.value)
        }
        form.value.traits[traitEditIndex.value] = { ...traitForm.value }
      } else {
        // Strip id when creating new - backend assigns it
        const { id: _id, ...payload } = traitForm.value
        const res = await postRacesRaceIdTraits(entityId.value!, payload as RaceTrait)
        const data = (res as any).data ?? res
        form.value.traits.push({ ...traitForm.value, id: data.id })
      }
      showToast({ variant: 'success', message: 'Trait saved.' })
    } catch {
      showToast({ variant: 'danger', message: 'Failed to save trait.' })
    } finally {
      traitSaving.value = false
    }
  } else {
    // Create mode: store locally until parent is saved
    if (traitEditIndex.value !== null) {
      pendingTraits.value[traitEditIndex.value] = { ...traitForm.value }
      form.value.traits[traitEditIndex.value] = { ...traitForm.value }
    } else {
      pendingTraits.value.push({ ...traitForm.value })
      form.value.traits.push({ ...traitForm.value })
    }
  }
  traitModalOpen.value = false
}

async function removeTrait(index: number) {
  const trait = form.value.traits?.[index]
  if (!trait) return

  if (isEdit.value && trait.id) {
    try {
      await deleteRacesRaceIdTraitsId(entityId.value!, String(trait.id))
      showToast({ variant: 'success', message: 'Trait deleted.' })
    } catch {
      showToast({ variant: 'danger', message: 'Failed to delete trait.' })
      return
    }
  } else if (!isEdit.value) {
    pendingTraits.value.splice(index, 1)
  }
  form.value.traits?.splice(index, 1)
}
</script>

<template>
  <ContentFormLayout
    :title="isEdit ? 'Edit Race' : 'New Race'"
    back-route="/content/races"
    :loading="loading"
    :saving="saving"
    @save="save"
    @cancel="cancel"
  >
    <div class="content-form__section">
      <h3 class="content-form__section-title">Basic Info</h3>
      <div class="content-form__row">
        <UiInput v-model="form.name" label="Name" placeholder="Race name" :tip="commonTips.name" />
        <UiSelect v-model="form.source" label="Source" :options="sourceOptions" :tip="commonTips.source" />
      </div>
      <UiSelect v-model="form.type" label="Creature Type" :options="creatureTypeOptions" :tip="raceTips.creatureType" />
    </div>

    <div class="content-form__section">
      <div class="content-form__section-header">
        <h3 class="content-form__section-title">Traits</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddTrait">Add</UiButton>
      </div>

      <table v-if="form.traits?.length" class="content-table content-form__nested-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Type</th>
            <th>Level</th>
            <th class="content-table__actions-col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(t, i) in form.traits" :key="i">
            <td>{{ t.name }}</td>
            <td>{{ featureTypeLabels[t.featureType ?? 0] ?? '—' }}</td>
            <td>{{ t.requiredCharacterLevel ?? '—' }}</td>
            <td class="content-table__actions-col">
              <button type="button" class="content-table__action-btn" title="Edit" @click="openEditTrait(i)">
                <i class="fas fa-pen" />
              </button>
              <button type="button" class="content-table__action-btn content-table__action-btn--danger" title="Delete" @click="removeTrait(i)">
                <i class="fas fa-trash" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else class="content-form__empty-hint">No traits added yet.</p>
    </div>

    <UiModal v-model="traitModalOpen" :title="traitEditIndex !== null ? 'Edit Trait' : 'Add Trait'" close-on-backdrop close-on-esc>
      <div class="content-form__modal-body">
        <UiInput v-model="traitForm.name" label="Name" placeholder="Trait name" :tip="commonTips.name" />
        <UiTextarea v-model="traitForm.description" label="Description" placeholder="Trait description..." :rows="4" :tip="commonTips.description" />
        <div class="content-form__row">
          <UiSelect v-model="traitForm.featureType" label="Feature Type" :options="featureTypeOptions" :tip="commonTips.featureType" />
          <UiInput v-model="traitForm.requiredCharacterLevel" label="Required Level" type="number" placeholder="1" :tip="commonTips.requiredLevel" />
        </div>
        <UiInput v-model="traitForm.displayOrder" label="Display Order" type="number" placeholder="0" :tip="commonTips.displayOrder" />
      </div>
      <template #footer>
        <UiButton @click="saveTrait">Save</UiButton>
        <UiButton variant="ghost" @click="traitModalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>
  </ContentFormLayout>
</template>
