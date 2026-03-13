<script setup lang="ts">
import UiBadge from "@/components/ui/UiBadge.vue";
import {languageTypeLabels} from "@/content/enums";

defineProps<{entity: any}>();
</script>

<template>
  <div class="wiki-section">
    <div class="wiki-stats">
      <div v-if="entity.type != null" class="wiki-stat">
        <span class="wiki-stat__label">Type</span>
        <span class="wiki-stat__value">{{ languageTypeLabels[entity.type] ?? "\u2014" }}</span>
      </div>
      <div v-if="entity.script" class="wiki-stat">
        <span class="wiki-stat__label">Script</span>
        <span class="wiki-stat__value">{{ entity.script }}</span>
      </div>
    </div>
  </div>

  <div v-if="entity.typicalSpeakers?.length" class="wiki-section">
    <h3 class="wiki-section__title">Typical Speakers</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="s in entity.typicalSpeakers" :key="s" :label="s" variant="muted"/>
    </div>
  </div>

  <div v-if="entity.description" class="wiki-section">
    <div class="wiki-prose" v-html="entity.description"/>
  </div>
</template>
