<script setup lang="ts">
defineProps<{
  title?: string
  subtitle?: string
}>()
</script>

<template>
  <section class="card">
    <header v-if="title || $slots.header" class="card__header">
      <div v-if="title">
        <h2 class="card__title">{{ title }}</h2>
        <p v-if="subtitle" class="card__subtitle">{{ subtitle }}</p>
      </div>
      <slot name="header" />
    </header>

    <div class="card__body">
      <slot name="body"/>
      <slot />
    </div>

    <footer v-if="$slots.footer" class="card__footer">
      <slot name="footer" />
    </footer>
  </section>
</template>
