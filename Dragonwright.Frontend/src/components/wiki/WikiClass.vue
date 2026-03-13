<script setup lang="ts">
import {computed} from "vue";
import UiBadge from "@/components/ui/UiBadge.vue";
import {
  abilityScoreLabels, skillLabels, toolLabels, itemTypeLabels,
  weaponTypeLabels, featureTypeLabels, resetTypeLabels,
  spellPrepareTypeLabels, spellLearnTypeLabels,
} from "@/content/enums";

const props = defineProps<{entity: any}>();

const features = computed(() => {
  return (props.entity.features ?? [])
    .slice()
    .sort((a: any, b: any) => (a.requiredCharacterLevel ?? 0) - (b.requiredCharacterLevel ?? 0));
});

const subclasses = computed(() => props.entity.subclasses ?? []);
</script>

<template>
  <div class="wiki-section">
    <div class="wiki-stats">
      <div class="wiki-stat">
        <span class="wiki-stat__label">Hit Die</span>
        <span class="wiki-stat__value">d{{ entity.hitDie ?? "?" }}</span>
      </div>
      <div v-if="entity.primaryAbilityScores?.length" class="wiki-stat">
        <span class="wiki-stat__label">Primary Abilities</span>
        <span class="wiki-stat__value">{{ entity.primaryAbilityScores.map((a: number) => abilityScoreLabels[a]).join(", ") }}</span>
      </div>
      <div v-if="entity.savingThrowProficiencies?.length" class="wiki-stat">
        <span class="wiki-stat__label">Saving Throws</span>
        <span class="wiki-stat__value">{{ entity.savingThrowProficiencies.map((a: number) => abilityScoreLabels[a]).join(", ") }}</span>
      </div>
      <div v-if="entity.skillProficienciesCount" class="wiki-stat">
        <span class="wiki-stat__label">Skill Choices</span>
        <span class="wiki-stat__value">Choose {{ entity.skillProficienciesCount }}</span>
      </div>
    </div>
  </div>

  <div v-if="entity.armorProficiencies?.length || entity.weaponProficiencies?.length || entity.toolProficiencies?.length" class="wiki-section">
    <h3 class="wiki-section__title">Proficiencies</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="p in (entity.armorProficiencies ?? [])" :key="'a'+p" :label="itemTypeLabels[p] ?? p" variant="muted"/>
      <UiBadge v-for="p in (entity.weaponProficiencies ?? [])" :key="'w'+p" :label="weaponTypeLabels[p] ?? p" variant="muted"/>
      <UiBadge v-for="p in (entity.toolProficiencies ?? [])" :key="'t'+p" :label="toolLabels[p] ?? p" variant="muted"/>
    </div>
  </div>

  <div v-if="entity.skillProficienciesOptions?.length" class="wiki-section">
    <h3 class="wiki-section__title">Skill Options</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="s in entity.skillProficienciesOptions" :key="s" :label="skillLabels[s] ?? s" variant="info"/>
    </div>
  </div>

  <div v-if="features.length" class="wiki-section">
    <h3 class="wiki-section__title">Class Features</h3>
    <div class="wiki-features">
      <div v-for="f in features" :key="f.id" class="wiki-feature">
        <div class="wiki-feature__header">
          <span class="wiki-feature__name">{{ f.name }}</span>
          <div class="wiki-feature__badges">
            <UiBadge v-if="f.requiredCharacterLevel" :label="`Lv ${f.requiredCharacterLevel}`" variant="info"/>
            <UiBadge :label="featureTypeLabels[f.featureType ?? 0] ?? 'Passive'" variant="muted"/>
          </div>
        </div>
        <div v-if="f.description" class="wiki-prose wiki-feature__desc" v-html="f.description"/>
      </div>
    </div>
  </div>

  <div v-if="subclasses.length" class="wiki-section">
    <h3 class="wiki-section__title">Subclasses</h3>
    <div class="wiki-features">
      <div v-for="sc in subclasses" :key="sc.id" class="wiki-feature">
        <div class="wiki-feature__header">
          <span class="wiki-feature__name">{{ sc.name }}</span>
          <div class="wiki-feature__badges">
            <UiBadge v-if="sc.canCastSpells" label="Spellcasting" variant="accent"/>
          </div>
        </div>
        <div v-if="sc.description" class="wiki-prose wiki-feature__desc" v-html="sc.description"/>

        <div v-if="sc.classFeatures?.length" class="wiki-subfeatures">
          <div v-for="sf in sc.classFeatures" :key="sf.id" class="wiki-feature wiki-feature--nested">
            <div class="wiki-feature__header">
              <span class="wiki-feature__name">{{ sf.name }}</span>
              <div class="wiki-feature__badges">
                <UiBadge v-if="sf.requiredCharacterLevel" :label="`Lv ${sf.requiredCharacterLevel}`" variant="info"/>
              </div>
            </div>
            <div v-if="sf.description" class="wiki-prose wiki-feature__desc" v-html="sf.description"/>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
