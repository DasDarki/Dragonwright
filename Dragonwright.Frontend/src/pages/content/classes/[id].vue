<script setup lang="ts">
import { useRoute } from 'vue-router'
import ClassForm from '@/components/content/forms/ClassForm.vue'

const route = useRoute()
const id = route.params.id as string
</script>

<template>
  <ClassForm :id="id" />
</template>
