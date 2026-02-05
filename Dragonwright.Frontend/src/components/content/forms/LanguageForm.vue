<script setup lang="ts">
import { ref, watch } from 'vue'
import { useContentForm } from '@/composables/useContentForm'
import { getLanguagesId, postLanguages, putLanguagesId } from '@/api'
import type { Language } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import { languageTips, commonTips } from '@/content/tips'
import { languageTypeOptions } from '@/content/enums'

const props = defineProps<{ id?: string }>()

const { form, loading, saving, isEdit, save, cancel } = useContentForm<Language>({
  typeLabel: 'Language',
  listRoute: '/content/languages',
  editRoute: '/content/languages',
  id: props.id,
  defaultData: () => ({
    name: '',
    description: '',
    type: 0,
    script: '',
    typicalSpeakers: [],
  }),
  getFn: getLanguagesId,
  createFn: postLanguages,
  updateFn: putLanguagesId,
})

// For managing typical speakers as comma-separated input
const speakersInput = ref('')

watch(() => form.value.typicalSpeakers, (val) => {
  if (Array.isArray(val)) {
    speakersInput.value = val.join(', ')
  }
}, { immediate: true })

function updateSpeakers() {
  form.value.typicalSpeakers = speakersInput.value
    .split(',')
    .map(s => s.trim())
    .filter(s => s.length > 0)
}
</script>

<template>
  <ContentFormLayout
    :title="isEdit ? `Edit Language` : `New Language`"
    back-route="/content/languages"
    :loading="loading"
    :saving="saving"
    @save="save"
    @cancel="cancel"
  >
    <div class="content-form__section">
      <h3 class="content-form__section-title">Basic Info</h3>
      <div class="content-form__row">
        <UiInput v-model="form.name" label="Name" placeholder="Language name" :tip="languageTips.name" />
        <UiSelect v-model="form.type" label="Type" :options="languageTypeOptions" :tip="{ title: 'Language Type', body: 'Standard languages are commonly spoken and easier to learn. Exotic languages are rarer and often associated with specific creatures or planes.' }" />
      </div>
      <div class="content-form__row">
        <UiInput v-model="form.script" label="Script" placeholder="e.g., Common, Dwarvish" :tip="{ title: 'Script', body: 'The writing system used for this language. Many languages share scripts (e.g., Orc uses Dwarvish script).' }" />
        <UiInput
          v-model="speakersInput"
          label="Typical Speakers"
          placeholder="e.g., Dwarves, Gnomes"
          :tip="{ title: 'Typical Speakers', body: 'Comma-separated list of creatures or peoples who typically speak this language.' }"
          @blur="updateSpeakers"
        />
      </div>
    </div>
    <div class="content-form__section">
      <UiTextarea
        v-model="form.description"
        label="Description"
        placeholder="Language description"
        :rows="4"
        :tip="commonTips.description"
      />
    </div>
  </ContentFormLayout>
</template>
