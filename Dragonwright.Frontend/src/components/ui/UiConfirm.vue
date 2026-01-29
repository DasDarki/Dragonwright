<script setup lang="ts">
import { computed } from 'vue'
import { useConfirm } from '@/composables/useConfirm'
import UiModal from './UiModal.vue'
import UiButton from './UiButton.vue'

const { state, close } = useConfirm()

const isOpen = computed({
  get: () => state.value.open,
  set: (val: boolean) => {
    if (!val) close(false)
  }
})
</script>

<template>
  <UiModal
    v-model="isOpen"
    :title="state.title ?? 'Confirmation'"
    topmost
  >
    <p v-if="state.message">
      {{ state.message }}
    </p>

    <template #footer>
      <UiButton variant="ghost" @click="close(false)">
        {{ state.cancelText ?? 'Cancel' }}
      </UiButton>
      <UiButton
        :variant="state.danger ? 'danger' : 'primary'"
        @click="close(true)"
      >
        {{ state.confirmText ?? 'OK' }}
      </UiButton>
    </template>
  </UiModal>
</template>
