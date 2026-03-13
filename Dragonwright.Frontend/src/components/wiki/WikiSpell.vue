<script setup lang="ts">
import UiBadge from "@/components/ui/UiBadge.vue";
import {
  spellLevelLabels, spellSchoolLabels, attackTypeLabels,
  abilityScoreLabels, damageTypeLabels, conditionLabels, shapeLabels,
} from "@/content/enums";

defineProps<{entity: any}>();

function formatTime(t: any): string {
  if (!t) return "";
  const units: Record<number, string> = {
    0: "seconds", 1: "minutes", 2: "hours", 3: "days", 4: "rounds",
    5: "actions", 6: "bonus actions", 7: "reactions",
  };
  if (t.amount === 1 && t.unit === 5) return "1 Action";
  if (t.amount === 1 && t.unit === 6) return "1 Bonus Action";
  if (t.amount === 1 && t.unit === 7) return "1 Reaction";
  return `${t.amount} ${units[t.unit] ?? ""}`;
}

function formatRange(r: number): string {
  if (r === 0) return "Self";
  if (r === -1) return "Touch";
  if (r === -2) return "Unlimited";
  return `${r} ft.`;
}

function formatComponents(e: any): string {
  const parts: string[] = [];
  if (e.hasVocalComponent) parts.push("V");
  if (e.hasSomaticComponent) parts.push("S");
  if (e.hasMaterialComponent) parts.push(`M (${e.materialComponents || "..."})`);
  return parts.join(", ") || "None";
}
</script>

<template>
  <div class="wiki-section">
    <div class="wiki-stats">
      <div class="wiki-stat">
        <span class="wiki-stat__label">Level</span>
        <span class="wiki-stat__value">{{ spellLevelLabels[entity.level] ?? entity.level }}</span>
      </div>
      <div class="wiki-stat">
        <span class="wiki-stat__label">School</span>
        <span class="wiki-stat__value">{{ spellSchoolLabels[entity.school] ?? "\u2014" }}</span>
      </div>
      <div v-if="entity.castingTimes?.length" class="wiki-stat">
        <span class="wiki-stat__label">Casting Time</span>
        <span class="wiki-stat__value">{{ entity.castingTimes.map(formatTime).join(", ") }}</span>
      </div>
      <div class="wiki-stat">
        <span class="wiki-stat__label">Range</span>
        <span class="wiki-stat__value">{{ formatRange(entity.range ?? 0) }}</span>
      </div>
      <div class="wiki-stat">
        <span class="wiki-stat__label">Components</span>
        <span class="wiki-stat__value">{{ formatComponents(entity) }}</span>
      </div>
      <div v-if="entity.durations?.length" class="wiki-stat">
        <span class="wiki-stat__label">Duration</span>
        <span class="wiki-stat__value">
          <template v-if="entity.concentration">Concentration, </template>
          {{ entity.durations.map(formatTime).join(", ") }}
        </span>
      </div>
    </div>
  </div>

  <div class="wiki-section wiki-badges-row">
    <UiBadge v-if="entity.ritual" label="Ritual" variant="accent"/>
    <UiBadge v-if="entity.concentration" label="Concentration" variant="info"/>
    <UiBadge v-if="entity.attackType && entity.attackType !== 0" :label="attackTypeLabels[entity.attackType] ?? ''" variant="muted"/>
    <UiBadge v-if="entity.save != null" :label="`${abilityScoreLabels[entity.save] ?? ''} Save`" variant="muted"/>
    <UiBadge v-if="entity.areaOfEffect != null" :label="`${entity.areaSize ?? ''} ft ${shapeLabels[entity.areaOfEffect] ?? ''}`" variant="muted"/>
  </div>

  <div v-if="entity.description" class="wiki-section">
    <div class="wiki-prose" v-html="entity.description"/>
  </div>

  <div v-if="entity.damageTypes?.length" class="wiki-section">
    <h3 class="wiki-section__title">Damage Types</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="dt in entity.damageTypes" :key="dt" :label="damageTypeLabels[dt] ?? dt" variant="danger"/>
    </div>
  </div>

  <div v-if="entity.conditions?.length" class="wiki-section">
    <h3 class="wiki-section__title">Conditions</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="c in entity.conditions" :key="c" :label="conditionLabels[c] ?? c" variant="warning"/>
    </div>
  </div>

  <div v-if="entity.classes?.length" class="wiki-section">
    <h3 class="wiki-section__title">Available To</h3>
    <div class="wiki-chip-list">
      <RouterLink
        v-for="cls in entity.classes"
        :key="cls.id"
        :to="`/wiki/classes/${cls.id}`"
        class="wiki-link-badge"
      >
        {{ cls.name }}
      </RouterLink>
    </div>
  </div>
</template>
