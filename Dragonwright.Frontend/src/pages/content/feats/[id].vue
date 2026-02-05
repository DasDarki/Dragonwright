<script setup lang="ts">
import FeatForm from '@/components/content/forms/FeatForm.vue'
import { useRoute } from 'vue-router'

const route = useRoute('/content/feats/[id]')
const id = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id
</script>

<template>
  <FeatForm :id="id" />
</template>
