<script setup lang="ts">
import UiButton from '@/components/ui/UiButton.vue'

defineProps<{
  title: string
  backRoute: string
  loading?: boolean
  saving?: boolean
}>()

defineEmits<{
  (e: 'save'): void
  (e: 'cancel'): void
}>()
</script>

<template>
  <div style="display: flex; flex-direction: column; gap: 2rem; justify-content: center; align-items: center">
    <div>
      <div class="content-page__header" style="width: 100%">
        <RouterLink :to="backRoute" class="content-page__back">
          <i class="fas fa-arrow-left" />
        </RouterLink>
        <h1 class="content-page__title">{{ title }}</h1>
      </div>

      <div v-if="loading" class="content-list-loading">
        <span class="btn__spinner content-list-loading__spinner" />
        <span class="content-list-loading__text">Loading...</span>
      </div>

      <template v-else>
        <form class="content-form" @submit.prevent="$emit('save')">
          <slot />

          <div class="content-form__actions">
            <UiButton type="submit" :loading="saving">
              Save
            </UiButton>
            <UiButton variant="ghost" @click="$emit('cancel')">
              Cancel
            </UiButton>
          </div>
        </form>
      </template>
    </div>
  </div>
</template>
