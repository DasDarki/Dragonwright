<script setup lang="ts">
import {toRef} from "vue";
import UiCard from "@/components/ui/UiCard.vue";
import {abilityScoreLabels} from "@/content/enums";
import {useCharacterHelpers} from "@/composables/useCharacterHelpers";

const props = defineProps<{ character: any; openOverride: (type: string, targetId?: number) => void }>();

const {getAbilityScore, getAbilityMod, getSavingThrow, getSaveProficiencyLevel} =
  useCharacterHelpers(toRef(props, "character"));
</script>

<template>
  <UiCard class="abilities-top" title="Ability Scores">
    <div class="abilities-row">
      <div v-for="i in 6" :key="i" class="ability-pill" @contextmenu.prevent="openOverride('ability', i - 1)">
        <div class="ability-pill__top">
          <span class="ability-pill__name">{{ abilityScoreLabels[i - 1] }}</span>
          <span class="ability-pill__score">{{ getAbilityScore(i - 1) }}</span>
        </div>

        <div class="ability-pill__mid">
          <span class="ability-pill__mod" v-roll>{{ getAbilityMod(getAbilityScore(i - 1)) }}</span>
        </div>

        <div class="ability-pill__bottom">
          <span class="save-pill" :class="`save-pill--${getSaveProficiencyLevel(i - 1)}`">
            <span class="save-pill__label">Save</span>
            <span class="save-pill__value" v-roll>{{ getSavingThrow(i - 1) }}</span>
            <span class="save-pill__mark"/>
          </span>
        </div>
      </div>
    </div>
  </UiCard>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.abilities-top :deep(.card__title) {
  letter-spacing: 0.06em;
}

.abilities-row {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: $space-3;

  @media (max-width: 1024px) {
    grid-template-columns: repeat(3, 1fr);
  }
  @media (max-width: 520px) {
    grid-template-columns: repeat(2, 1fr);
  }
}

.ability-pill {
  position: relative;
  padding: $space-3;
  border-radius: $radius-lg;
  border: 1px solid $color-border-subtle;
  background: radial-gradient(900px 140px at 50% -60%, rgba(249, 115, 22, 0.10), transparent 60%),
  $color-surface-alt;
  overflow: hidden;
}

.ability-pill::after {
  content: "";
  position: absolute;
  inset: -2px;
  border-radius: $radius-lg;
  pointer-events: none;
  background: radial-gradient(220px 120px at 20% 10%, rgba(249, 115, 22, 0.18), transparent 60%),
  radial-gradient(260px 140px at 90% 0%, rgba(56, 189, 248, 0.12), transparent 55%);
  opacity: 0.55;
}

.ability-pill__top {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: $space-2;
}

.ability-pill__name {
  font-size: 0.72rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.ability-pill__score {
  font-size: 0.85rem;
  color: $color-text-soft;
  border: 1px solid $color-border-subtle;
  background: rgba(17, 24, 39, 0.6);
  padding: 0.12rem 0.5rem;
  border-radius: $radius-pill;
}

.ability-pill__mid {
  position: relative;
  z-index: 1;
  display: flex;
  justify-content: center;
  margin-top: $space-2;
}

.ability-pill__mod {
  font-size: 1.75rem;
  font-weight: 900;
  color: $color-accent;
  text-shadow: 0 0 18px rgba(249, 115, 22, 0.25);
}

.ability-pill__bottom {
  position: relative;
  z-index: 1;
  margin-top: $space-2;
  display: flex;
  justify-content: center;
}

.save-pill {
  position: relative;
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
  padding: 0.45rem 0.7rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: rgba(5, 8, 20, 0.30);
}

.save-pill__label {
  font-size: 0.82rem;
  color: $color-text-muted;
}

.save-pill__value {
  font-weight: 900;
  color: $color-text;
}

.save-pill__mark {
  width: 0.65rem;
  height: 0.65rem;
  border-radius: 999px;
  border: 1px solid $color-border-strong;
  background: rgba(17, 24, 39, 0.65);
}

.save-pill--half .save-pill__mark {
  background: linear-gradient(90deg, rgba(249, 115, 22, 0.25) 50%, rgba(17, 24, 39, 0.65) 50%);
  border-color: rgba(249, 115, 22, 0.35);
}

.save-pill--prof .save-pill__mark {
  background: rgba(249, 115, 22, 0.25);
  border-color: rgba(249, 115, 22, 0.55);
  box-shadow: 0 0 0 2px rgba(249, 115, 22, 0.08);
}

.save-pill--expert .save-pill__mark {
  background: rgba(249, 115, 22, 0.25);
  border-color: rgba(249, 115, 22, 0.75);
  box-shadow: 0 0 0 2px rgba(249, 115, 22, 0.10);
  outline: 2px solid rgba(249, 115, 22, 0.20);
  outline-offset: 1px;
}
</style>
