<script setup lang="ts">
import { computed } from 'vue'
import { useTitle } from '@/composables/useTitle'
import { useAuthStore } from '@/stores/auth'
import { contentTypes } from '@/content/types'
import UiGrid from '@/components/ui/layout/UiGrid.vue'
import ContentOverviewCard from '@/components/content/ContentOverviewCard.vue'

useTitle('Content')

const authStore = useAuthStore()
const isTeamMember = computed(() => (authStore.loggedInUser?.userRole ?? 0) >= 1)
</script>

<template>
  <div>
    <h1 class="content-page__title" style="margin-bottom: 1.5rem">Content Dashboard</h1>

    <UiGrid :cols="1" :cols-sm="2" :cols-lg="3" :gap="1">
      <ContentOverviewCard
        v-for="ct in contentTypes"
        :key="ct.key"
        :icon="ct.icon"
        :label="ct.label"
        :description="ct.description"
        :to="`/content/${ct.key}`"
        :team-only="ct.teamOnly && !isTeamMember"
      />
    </UiGrid>
  </div>
</template>
