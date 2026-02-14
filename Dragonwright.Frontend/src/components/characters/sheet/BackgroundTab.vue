<script setup lang="ts">
import {computed} from "vue";
import {abilityScoreLabels, skillLabels, toolLabels, sourceLabels} from "@/content/enums";

const props = defineProps<{ character: any }>();

const bgSkills = computed(() => {
  const bg = props.character?.background?.background;
  return (bg?.skillProficiencies ?? []).map((s: number) => skillLabels[s]).filter(Boolean);
});

const bgTools = computed(() => {
  const bg = props.character?.background?.background;
  return (bg?.toolProficiencies ?? []).map((t: number) => toolLabels[t]).filter(Boolean);
});

const bgAbilityIncreases = computed(() => {
  const chosen = props.character?.background?.chosenAbilityScoreIncreases ?? {};
  return Object.entries(chosen).map(([k, v]) => ({
    label: abilityScoreLabels[Number(k)] ?? k,
    value: v as number
  })).filter(e => e.value > 0);
});

const bgLanguages = computed(() => {
  return (props.character?.background?.chosenLanguages ?? []).filter(Boolean);
});
</script>

<template>
  <div v-if="character.background?.background" class="bg-tab">
    <div class="bg-tab__header">
      <h3 class="bg-tab__name">{{ character.background.background.name }}</h3>
      <span class="bg-tab__source">{{ sourceLabels[character.background.background.source ?? 0] }}</span>
    </div>

    <div v-if="bgSkills.length" class="bg-tab__section">
      <div class="bg-tab__label">Skill Proficiencies</div>
      <div class="chips__wrap">
        <span v-for="s in bgSkills" :key="s" class="chip chip--muted">{{ s }}</span>
      </div>
    </div>

    <div v-if="bgTools.length" class="bg-tab__section">
      <div class="bg-tab__label">Tool Proficiencies</div>
      <div class="chips__wrap">
        <span v-for="t in bgTools" :key="t" class="chip chip--muted">{{ t }}</span>
      </div>
    </div>

    <div v-if="bgAbilityIncreases.length" class="bg-tab__section">
      <div class="bg-tab__label">Ability Score Increases</div>
      <div class="chips__wrap">
        <span v-for="a in bgAbilityIncreases" :key="a.label" class="chip chip--info">
          {{ a.label }} +{{ a.value }}
        </span>
      </div>
    </div>

    <div v-if="bgLanguages.length" class="bg-tab__section">
      <div class="bg-tab__label">Languages</div>
      <div class="chips__wrap">
        <span v-for="l in bgLanguages" :key="l" class="chip chip--info">{{ l }}</span>
      </div>
    </div>
  </div>
  <div v-else class="bg-tab__none">No background selected.</div>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.bg-tab {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.bg-tab__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-3;
}

.bg-tab__name {
  font-size: 1.1rem;
  font-weight: 700;
  margin: 0;
}

.bg-tab__source {
  font-size: 0.75rem;
  color: $color-text-muted;
  padding: $space-1 $space-2;
  background: $color-surface-alt;
  border-radius: $radius-sm;
}

.bg-tab__section {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.bg-tab__label {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 600;
  margin-top: $space-3;
}

.bg-tab__none {
  color: $color-text-muted;
  padding: $space-4;
  text-align: center;
}

.chips__wrap {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
}

.chip {
  padding: 0.25rem 0.6rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  font-size: 0.85rem;
  color: $color-text;
}

.chip--muted { color: $color-text-muted; }

.chip--info {
  border-color: rgba(56, 189, 248, 0.35);
  background: rgba(56, 189, 248, 0.10);
  color: $color-accent-alt;
}
</style>
