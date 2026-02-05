<script setup lang="ts">
import { useRoute } from 'vue-router'
import SpellForm from '@/components/content/forms/SpellForm.vue'

const route = useRoute()
const id = route.params.id as string
</script>

<template>
  <SpellForm :id="id" />
</template>
