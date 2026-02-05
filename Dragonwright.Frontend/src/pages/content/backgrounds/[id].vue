<script setup lang="ts">
import { useRoute } from 'vue-router'
import BackgroundForm from '@/components/content/forms/BackgroundForm.vue'

const route = useRoute()
const id = route.params.id as string
</script>

<template>
  <BackgroundForm :id="id" />
</template>
