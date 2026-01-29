<script setup lang="ts">
import { useToast } from '@/composables/useToast'

const { toasts, dismissToast } = useToast()
</script>

<template>
  <Teleport to="body">
    <div v-if="toasts.length" class="toast-container" aria-live="polite">
      <article
        v-for="toast in toasts"
        :key="toast.id"
        class="toast"
        :class="`toast--${toast.variant}`"
      >
        <div class="toast__icon">
          <i
            v-if="toast.variant === 'success'"
            class="fas fa-circle-check"
          />
          <i
            v-else-if="toast.variant === 'warning'"
            class="fas fa-triangle-exclamation"
          />
          <i
            v-else-if="toast.variant === 'danger'"
            class="fas fa-circle-xmark"
          />
          <i
            v-else
            class="fas fa-circle-info"
          />
        </div>

        <div class="toast__content">
          <p v-if="toast.title" class="toast__title">{{ toast.title }}</p>
          <p class="toast__message">{{ toast.message }}</p>
        </div>

        <button class="toast__close" type="button" @click="dismissToast(toast.id)">
          <i class="fas fa-xmark" />
        </button>
      </article>
    </div>
  </Teleport>
</template>
