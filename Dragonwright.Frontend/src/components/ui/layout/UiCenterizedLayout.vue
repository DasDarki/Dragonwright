<script setup lang="ts">

</script>

<template>
  <div class="centerized-layout">
    <slot />
  </div>
</template>

<style scoped lang="scss">
.centerized-layout {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
}
</style>