<script setup lang="ts">
import UiBadge from "@/components/ui/UiBadge.vue";
import {skillLabels, toolLabels, itemTypeLabels, weaponTypeLabels, characteristicsTypeLabels} from "@/content/enums";

defineProps<{entity: any}>();
</script>

<template>
  <div v-if="entity.skillProficiencies?.length || entity.toolProficiencies?.length" class="wiki-section">
    <h3 class="wiki-section__title">Proficiencies</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="s in (entity.skillProficiencies ?? [])" :key="'s'+s" :label="skillLabels[s] ?? s" variant="info"/>
      <UiBadge v-for="t in (entity.toolProficiencies ?? [])" :key="'t'+t" :label="toolLabels[t] ?? t" variant="muted"/>
      <UiBadge v-for="a in (entity.armorProficiencies ?? [])" :key="'a'+a" :label="itemTypeLabels[a] ?? a" variant="muted"/>
      <UiBadge v-for="w in (entity.weaponProficiencies ?? [])" :key="'w'+w" :label="weaponTypeLabels[w] ?? w" variant="muted"/>
    </div>
  </div>

  <div v-if="entity.abilityScoreIncreases?.length" class="wiki-section">
    <h3 class="wiki-section__title">Ability Score Increases</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="(a, i) in entity.abilityScoreIncreases" :key="i" :label="String(a)" variant="accent"/>
    </div>
  </div>

  <div v-if="entity.languageCount" class="wiki-section">
    <h3 class="wiki-section__title">Languages</h3>
    <p class="wiki-text">Choose {{ entity.languageCount }} language(s).</p>
  </div>

  <div v-if="entity.grantedFeats?.length" class="wiki-section">
    <h3 class="wiki-section__title">Granted Feats</h3>
    <div class="wiki-chip-list">
      <RouterLink
        v-for="f in entity.grantedFeats"
        :key="f.id"
        :to="`/wiki/feats/${f.id}`"
        class="wiki-link-badge"
      >
        {{ f.name }}
      </RouterLink>
    </div>
  </div>

  <div v-if="entity.characteristics?.length" class="wiki-section">
    <h3 class="wiki-section__title">Characteristics</h3>
    <div class="wiki-features">
      <div v-for="ch in entity.characteristics" :key="ch.id ?? ch.type" class="wiki-feature">
        <div class="wiki-feature__header">
          <span class="wiki-feature__name">{{ characteristicsTypeLabels[ch.type] ?? "Characteristic" }}</span>
        </div>
        <p v-if="ch.description" class="wiki-feature__desc wiki-text">{{ ch.description }}</p>
      </div>
    </div>
  </div>
</template>
