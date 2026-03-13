<script setup lang="ts">
import UiBadge from "@/components/ui/UiBadge.vue";
import {creatureTypeLabels, featureTypeLabels, resetTypeLabels} from "@/content/enums";
import {apiUrl} from "@/api/http";

const props = defineProps<{entity: any}>();

function imageUrl(id: string): string {
  return `${apiUrl}/files/${id}`;
}
</script>

<template>
  <div v-if="entity.image?.id" class="wiki-section">
    <img :src="imageUrl(entity.image.id)" :alt="entity.name" class="wiki-race-image"/>
  </div>

  <div class="wiki-section">
    <div class="wiki-stats">
      <div v-if="entity.type != null" class="wiki-stat">
        <span class="wiki-stat__label">Creature Type</span>
        <span class="wiki-stat__value">{{ creatureTypeLabels[entity.type] ?? "\u2014" }}</span>
      </div>
    </div>
  </div>

  <div v-if="entity.traits?.length" class="wiki-section">
    <h3 class="wiki-section__title">Racial Traits</h3>
    <div class="wiki-features">
      <div v-for="trait in entity.traits" :key="trait.id" class="wiki-feature">
        <div class="wiki-feature__header">
          <span class="wiki-feature__name">{{ trait.name }}</span>
          <div class="wiki-feature__badges">
            <template v-for="action in (trait.actions ?? []).filter((a: any) => a.resetType != null && a.resetType !== 3)" :key="action.id">
              <UiBadge :label="resetTypeLabels[action.resetType] ?? ''" variant="info"/>
            </template>
            <UiBadge :label="featureTypeLabels[trait.featureType ?? 0] ?? 'Passive'" variant="muted"/>
          </div>
        </div>
        <div v-if="trait.description" class="wiki-prose wiki-feature__desc" v-html="trait.description"/>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.wiki-race-image {
  max-width: 300px;
  max-height: 300px;
  border-radius: $radius-lg;
  border: 1px solid $color-border-subtle;
  object-fit: cover;
}
</style>
