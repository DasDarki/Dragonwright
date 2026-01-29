<script setup lang="ts">
import UiConfirm from "@/components/ui/UiConfirm.vue";
import UiToastContainer from "@/components/ui/UiToastContainer.vue";
</script>

<template>
  <div class="app-shell">
    <main class="app-shell__main">
      <div class="app-shell__content">
        <RouterView />
      </div>
    </main>

    <UiConfirm />
    <UiToastContainer />
  </div>
</template>