<script setup lang="ts">
import UiBadge from "@/components/ui/UiBadge.vue";
import {
  itemTypeLabels, rarityLabels, weaponTypeLabels, weaponPropertyLabels,
  damageTypeLabels, abilityScoreLabels, masteryLabels,
} from "@/content/enums";

defineProps<{entity: any}>();

function formatWeight(oz: number): string {
  if (!oz) return "\u2014";
  const lb = oz / 16;
  return lb >= 1 ? `${lb} lb.` : `${oz} oz.`;
}

function formatValue(copper: number): string {
  if (!copper) return "\u2014";
  if (copper >= 100) return `${copper / 100} gp`;
  if (copper >= 10) return `${copper / 10} sp`;
  return `${copper} cp`;
}
</script>

<template>
  <div class="wiki-section">
    <div class="wiki-stats">
      <div class="wiki-stat">
        <span class="wiki-stat__label">Type</span>
        <span class="wiki-stat__value">{{ itemTypeLabels[entity.type] ?? "\u2014" }}</span>
      </div>
      <div class="wiki-stat">
        <span class="wiki-stat__label">Rarity</span>
        <span class="wiki-stat__value">{{ rarityLabels[entity.rarity] ?? "\u2014" }}</span>
      </div>
      <div v-if="entity.weightInOunces" class="wiki-stat">
        <span class="wiki-stat__label">Weight</span>
        <span class="wiki-stat__value">{{ formatWeight(Number(entity.weightInOunces)) }}</span>
      </div>
      <div v-if="entity.valueInCopper" class="wiki-stat">
        <span class="wiki-stat__label">Value</span>
        <span class="wiki-stat__value">{{ formatValue(Number(entity.valueInCopper)) }}</span>
      </div>
    </div>
  </div>

  <div class="wiki-section wiki-badges-row">
    <UiBadge v-if="entity.isMagical" label="Magical" variant="accent"/>
    <UiBadge v-if="entity.requiresAttunement" label="Requires Attunement" variant="info"/>
    <UiBadge v-if="entity.isConsumable" label="Consumable" variant="warning"/>
    <UiBadge v-if="entity.weaponType != null" :label="weaponTypeLabels[entity.weaponType] ?? ''" variant="muted"/>
    <UiBadge v-if="entity.mastery != null" :label="`Mastery: ${masteryLabels[entity.mastery] ?? ''}`" variant="muted"/>
  </div>

  <div v-if="entity.baseArmorClass != null || entity.armorClassBonus" class="wiki-section">
    <h3 class="wiki-section__title">Armor</h3>
    <div class="wiki-stats">
      <div v-if="entity.baseArmorClass != null" class="wiki-stat">
        <span class="wiki-stat__label">Base AC</span>
        <span class="wiki-stat__value">{{ entity.baseArmorClass }}</span>
      </div>
      <div v-if="entity.armorClassBonus" class="wiki-stat">
        <span class="wiki-stat__label">AC Bonus</span>
        <span class="wiki-stat__value">+{{ entity.armorClassBonus }}</span>
      </div>
      <div v-if="entity.givesDisadvantageOnStealth" class="wiki-stat">
        <span class="wiki-stat__label">Stealth</span>
        <span class="wiki-stat__value">Disadvantage</span>
      </div>
    </div>
  </div>

  <div v-if="entity.weaponProperties?.length" class="wiki-section">
    <h3 class="wiki-section__title">Weapon Properties</h3>
    <div class="wiki-chip-list">
      <UiBadge v-for="wp in entity.weaponProperties" :key="wp" :label="weaponPropertyLabels[wp] ?? wp" variant="muted"/>
    </div>
  </div>

  <div v-if="entity.damages?.length" class="wiki-section">
    <h3 class="wiki-section__title">Damage</h3>
    <div class="wiki-stats">
      <div v-for="(d, i) in entity.damages" :key="i" class="wiki-stat">
        <span class="wiki-stat__label">{{ damageTypeLabels[d.damageType] ?? "Damage" }}</span>
        <span class="wiki-stat__value">{{ d.diceCount }}d{{ d.diceValue }}{{ d.bonus ? ` + ${d.bonus}` : "" }}</span>
      </div>
    </div>
  </div>

  <div v-if="entity.rangeInFeet" class="wiki-section">
    <h3 class="wiki-section__title">Range</h3>
    <p class="wiki-text">
      {{ entity.rangeInFeet }} ft.{{ entity.maximumRangeInFeet ? ` / ${entity.maximumRangeInFeet} ft.` : "" }}
    </p>
  </div>

  <div v-if="entity.description" class="wiki-section">
    <h3 class="wiki-section__title">Description</h3>
    <div class="wiki-prose" v-html="entity.description"/>
  </div>
</template>
