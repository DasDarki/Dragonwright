<script setup lang="ts">
import UiBadge from "@/components/ui/UiBadge.vue";
import {abilityScoreLabels, resetTypeLabels} from "@/content/enums";

defineProps<{entity: any}>();
</script>

<template>
  <div class="wiki-section">
    <div class="wiki-stats">
      <div v-if="entity.featLevel" class="wiki-stat">
        <span class="wiki-stat__label">Level</span>
        <span class="wiki-stat__value">{{ entity.featLevel }}</span>
      </div>
      <div v-if="entity.prerequisiteDescription" class="wiki-stat">
        <span class="wiki-stat__label">Prerequisites</span>
        <span class="wiki-stat__value">{{ entity.prerequisiteDescription }}</span>
      </div>
      <div v-if="entity.prerequisiteAbilityScore != null" class="wiki-stat">
        <span class="wiki-stat__label">Required Ability</span>
        <span class="wiki-stat__value">{{ abilityScoreLabels[entity.prerequisiteAbilityScore] ?? "" }} {{ entity.prerequisiteAbilityScoreMinimum ?? "" }}+</span>
      </div>
    </div>
  </div>

  <div class="wiki-section wiki-badges-row">
    <UiBadge v-if="entity.isRepeatable" label="Repeatable" variant="info"/>
    <UiBadge v-if="entity.prerequisiteSpellcasting" label="Requires Spellcasting" variant="muted"/>
    <UiBadge v-if="entity.abilityScoreIncrease" :label="`+${entity.abilityScoreIncrease} ASI`" variant="accent"/>
  </div>

  <div v-if="entity.description" class="wiki-section">
    <div class="wiki-prose" v-html="entity.description"/>
  </div>

  <div v-if="entity.abilityScoreOptions?.length" class="wiki-section">
    <h3 class="wiki-section__title">Ability Score Options</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="a in entity.abilityScoreOptions" :key="a" :label="abilityScoreLabels[a] ?? a" variant="muted"/>
    </div>
  </div>

  <div v-if="entity.actions?.length" class="wiki-section">
    <h3 class="wiki-section__title">Actions</h3>
    <div class="wiki-features">
      <div v-for="action in entity.actions" :key="action.id" class="wiki-feature">
        <div class="wiki-feature__header">
          <span class="wiki-feature__name">{{ action.name }}</span>
          <UiBadge v-if="action.resetType != null && action.resetType !== 3" :label="resetTypeLabels[action.resetType] ?? ''" variant="info"/>
        </div>
        <div v-if="action.description" class="wiki-prose wiki-feature__desc" v-html="action.description"/>
      </div>
    </div>
  </div>

  <div v-if="entity.spells?.length" class="wiki-section">
    <h3 class="wiki-section__title">Granted Spells</h3>
    <div class="wiki-chip-list">
      <RouterLink
        v-for="s in entity.spells"
        :key="s.spellId ?? s.spell?.id"
        :to="`/wiki/spells/${s.spellId ?? s.spell?.id}`"
        class="wiki-link-badge"
      >
        {{ s.spell?.name ?? s.spellId }}
      </RouterLink>
    </div>
  </div>
</template>
