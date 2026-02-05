<script setup lang="ts">
import { useContentForm } from '@/composables/useContentForm'
import { getLanguagesId, postLanguages, putLanguagesId } from '@/api'
import type { Language } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import UiInput from '@/components/ui/UiInput.vue'
import { languageTips } from '@/content/tips'
import UiTextarea from "@/components/ui/UiTextarea.vue";

const props = defineProps<{ id?: string }>()

const { form, loading, saving, isEdit, save, cancel } = useContentForm<Language>({
  typeLabel: 'Language',
  listRoute: '/content/languages',
  editRoute: '/content/languages',
  id: props.id,
  defaultData: () => ({ name: '', description: '' }),
  getFn: getLanguagesId,
  createFn: postLanguages,
  updateFn: putLanguagesId,
})
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
    <div class="content-form__section" style="width: 20rem">
      <UiInput v-model="form.name" label="Name" placeholder="Language name" :tip="languageTips.name" />
    </div>
    <div class="content-form__section" style="width: 20rem">
      <UiTextarea
        v-model="form.description"
        label="Description"
        placeholder="Language description"
        textarea
        :rows="4"
      />
    </div>
  </ContentFormLayout>
</template>
