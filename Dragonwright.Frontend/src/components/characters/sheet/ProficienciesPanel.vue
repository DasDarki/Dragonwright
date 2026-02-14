<script setup lang="ts">
import UiCard from "@/components/ui/UiCard.vue";

defineProps<{ character: any }>();
</script>

<template>
  <UiCard title="Proficiencies & Training">
    <div class="profs">
      <div class="profs__top">
        <span class="profs__k">Proficiency Bonus</span>
        <span class="profs__v">+{{ character.proficiencyBonus }}</span>
      </div>

      <div class="profs__section">
        <div class="profs__title">Armor</div>
        <div class="profs__items">
          <span
            v-if="!(character.armorProficiencies?.length ?? 0) && !(character.specificArmorProficiencies?.length ?? 0)"
            class="chips__empty">None</span>
          <span v-for="a in character.armorProficiencies ?? []" :key="a" class="chip chip--muted">{{ a }}</span>
          <span v-for="a in character.specificArmorProficiencies ?? []" :key="a" class="chip chip--muted">{{ a }}</span>
        </div>
      </div>

      <div class="profs__section">
        <div class="profs__title">Weapons</div>
        <div class="profs__items">
          <span
            v-if="!(character.weaponProficiencies?.length ?? 0) && !(character.specificWeaponProficiencies?.length ?? 0)"
            class="chips__empty">None</span>
          <span v-for="w in character.weaponProficiencies ?? []" :key="w" class="chip chip--muted">{{ w }}</span>
          <span v-for="w in character.specificWeaponProficiencies ?? []" :key="w" class="chip chip--muted">{{ w }}</span>
        </div>
      </div>

      <div class="profs__section">
        <div class="profs__title">Tools</div>
        <div class="profs__items">
          <span
            v-if="!(character.toolProficiencies?.length ?? 0) && !(character.specificToolProficiencies?.length ?? 0)"
            class="chips__empty">None</span>
          <span v-for="t in character.toolProficiencies ?? []" :key="t" class="chip chip--muted">{{ t }}</span>
          <span v-for="t in character.specificToolProficiencies ?? []" :key="t" class="chip chip--muted">{{ t }}</span>
        </div>
      </div>

      <div class="profs__section">
        <div class="profs__title">Languages</div>
        <div class="profs__items">
          <span v-if="!(character.languages?.length ?? 0)" class="chips__empty">None</span>
          <span v-for="l in character.languages ?? []" :key="l" class="chip chip--info">{{ l }}</span>
        </div>
      </div>
    </div>
  </UiCard>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.profs {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.profs__top {
  display: flex;
  justify-content: space-between;
  gap: $space-3;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: rgba(5, 8, 20, 0.25);
}

.profs__k {
  color: $color-text-muted;
  font-size: 0.85rem;
}

.profs__v {
  font-weight: 900;
}

.profs__title {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  margin-bottom: $space-2;
}

.profs__items {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
}

.chips__empty {
  color: $color-text-muted;
  font-size: 0.9rem;
}

.chip {
  padding: 0.25rem 0.6rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  font-size: 0.85rem;
  color: $color-text;
}

.chip--muted {
  color: $color-text-muted;
}

.chip--info {
  border-color: rgba(56, 189, 248, 0.35);
  background: rgba(56, 189, 248, 0.10);
  color: $color-accent-alt;
}
</style>
