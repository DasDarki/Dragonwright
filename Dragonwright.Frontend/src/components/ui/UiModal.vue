<script setup lang="ts">
import { onMounted, onBeforeUnmount, watch } from 'vue'

const props = defineProps<{
  modelValue: boolean
  title?: string
  closeOnBackdrop?: boolean
  closeOnEsc?: boolean
  topmost?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'close'): void
}>()

const close = () => {
  emit('update:modelValue', false)
  emit('close')
}

const onBackdropClick = () => {
  if (props.closeOnBackdrop) close()
}

const onKeyDown = (e: KeyboardEvent) => {
  if (!props.modelValue) return
  if (!props.closeOnEsc) return
  if (e.key === 'Escape') close()
}

onMounted(() => {
  window.addEventListener('keydown', onKeyDown)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeyDown)
})

watch(
  () => props.modelValue,
  (open) => {
    if (open) {
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
  },
  { immediate: true }
)
</script>

<template>
  <Teleport to="body">
    <div v-if="modelValue" class="modal-backdrop" :class="{topmost}" @click.self="onBackdropClick">
      <section class="modal" role="dialog" aria-modal="true" :aria-label="title">
        <header class="modal__header">
          <h2 v-if="title" class="modal__title">{{ title }}</h2>
          <button type="button" class="toast__close" aria-label="Close" @click="close">
            <i class="fas fa-xmark"/>
          </button>
        </header>

        <div class="modal__body">
          <slot />
        </div>

        <footer v-if="$slots.footer" class="modal__footer">
          <slot name="footer" />
        </footer>
      </section>
    </div>
  </Teleport>
</template>
