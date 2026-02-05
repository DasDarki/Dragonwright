<script setup lang="ts">
import FeatForm from '@/components/content/forms/FeatForm.vue'
import { useRoute } from 'vue-router'

const route = useRoute('/content/feats/[id]')
</script>

<template>
  <FeatForm :id="route.params.id" />
</template>
