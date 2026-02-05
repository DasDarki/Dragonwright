<script setup lang="ts">
import { useRoute } from 'vue-router'
import RaceForm from '@/components/content/forms/RaceForm.vue'

const route = useRoute()
const id = route.params.id as string
</script>

<template>
  <RaceForm :id="id" />
</template>
