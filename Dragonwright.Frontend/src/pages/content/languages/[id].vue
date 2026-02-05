<script setup lang="ts">
import { useRoute } from 'vue-router'
import LanguageForm from '@/components/content/forms/LanguageForm.vue'

const route = useRoute()
const id = route.params.id as string
</script>

<template>
  <LanguageForm :id="id" />
</template>
