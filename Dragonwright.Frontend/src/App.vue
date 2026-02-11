<script setup lang="ts">
import UiConfirm from "@/components/ui/UiConfirm.vue";
import UiToastContainer from "@/components/ui/UiToastContainer.vue";
import Navbar from "@/components/Navbar.vue";
import {useAuthStore} from "@/stores/auth.ts";

const authStore = useAuthStore();
</script>

<template>
  <div class="app-shell">
    <main class="app-shell__main">
      <div class="app-shell__content">
        <Navbar v-if="authStore.loggedInUser" />

        <RouterView />
      </div>
    </main>

    <UiConfirm />
    <UiToastContainer />
  </div>
</template>