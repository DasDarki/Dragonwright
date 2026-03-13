<script setup lang="ts">
import {useTitle} from "@/composables/useTitle";
import {contentTypes} from "@/content/types";
import UiGrid from "@/components/ui/layout/UiGrid.vue";
import ContentOverviewCard from "@/components/content/ContentOverviewCard.vue";

useTitle("Wiki");
</script>

<template>
  <div>
    <div class="content-page__header">
      <h1 class="content-page__title">Wiki</h1>
    </div>
    <p class="wiki-subtitle">Browse all spells, classes, races, items, feats, backgrounds and languages.</p>

    <UiGrid :cols="1" :cols-sm="2" :cols-lg="3" :gap="1">
      <ContentOverviewCard
        v-for="ct in contentTypes"
        :key="ct.key"
        :icon="ct.icon"
        :label="ct.label"
        :description="ct.description"
        :to="`/wiki/${ct.key}`"
      />
    </UiGrid>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.wiki-subtitle {
  color: $color-text-muted;
  font-size: 0.9rem;
  margin: 0 0 $space-4;
}
</style>
