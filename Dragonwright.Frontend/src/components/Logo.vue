<script setup lang="ts">
defineProps<{
  withText?: boolean;
  logoSize?: string;
}>();
</script>

<template>
  <div class="logo-wrapper">
    <img src="/logo.png" alt="logo" class="logo" :style="{width: logoSize}"/>

    <h1 v-if="withText">
      <b>Dragon</b>wright
    </h1>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.logo-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  .logo {
    width: 7.5rem;
    height: auto;
    user-select: none;
    -webkit-user-drag: none;
  }
  h1 {
    font-size: 2.25rem;
    margin: 0;
    user-select: none;
    b {
      color: $color-accent;
      font-weight: 700;
      animation: pulse 2s infinite;
    }
  }
}

@keyframes pulse {
  0% {
    text-shadow: 0 0 5px $color-accent;
  }
  50% {
    text-shadow: 0 0 20px $color-accent;
  }
  100% {
    text-shadow: 0 0 5px $color-accent;
  }
}

@media (max-width: 600px) {
  .logo-wrapper {
    flex-direction: column;
    .logo {
      width: 5rem;
    }
    h1 {
      font-size: 1.5rem;
      margin-top: 0.5rem;
    }
  }
}
</style>