<script setup lang="ts">
import { useRoute } from 'vue-router'
import ItemForm from '@/components/content/forms/ItemForm.vue'

const route = useRoute()
const id = route.params.id as string
</script>

<template>
  <ItemForm :id="id" />
</template>
