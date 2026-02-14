<script setup lang="ts">
import {toRef} from "vue";
import UiCard from "@/components/ui/UiCard.vue";
import {useCharacterHelpers, skillList} from "@/composables/useCharacterHelpers";

const props = defineProps<{ character: any; openOverride: (type: string, targetId?: number) => void }>();

const {getSkillBonus, getSkillProficiencyLevel, getSkillAbility} =
  useCharacterHelpers(toRef(props, "character"));
</script>

<template>
  <UiCard title="Skills">
    <div class="skills-list">
      <div
        v-for="skill in skillList"
        :key="skill.id"
        class="skill-row"
        :class="`skill-row--${getSkillProficiencyLevel(skill.id)}`"
        @contextmenu.prevent="openOverride('skill', skill.id)"
      >
        <span class="skill-row__left">
          <span class="prof-dot" :class="`prof-dot--${getSkillProficiencyLevel(skill.id)}`"/>
          <span class="skill-row__name">
            {{ skill.label }}
            <span class="skill-row__ability">({{ getSkillAbility(skill.id) }})</span>
          </span>
        </span>

        <span class="skill-row__bonus" v-roll>{{ getSkillBonus(skill.id) }}</span>
      </div>
    </div>
  </UiCard>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.skills-list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
  max-height: 760px;
  overflow-y: auto;
  padding-right: 2px;
}

.skill-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-3;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: radial-gradient(600px 140px at 0% 50%, rgba(249, 115, 22, 0.08), transparent 65%),
  $color-surface-alt;
}

.skill-row__left {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  min-width: 0;
}

.skill-row__name {
  font-size: 0.92rem;
  color: $color-text;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.skill-row__ability {
  font-size: 0.78rem;
  color: $color-text-muted;
  margin-left: $space-1;
}

.skill-row__bonus {
  font-weight: 900;
  color: $color-accent;
  font-size: 1.05rem;
  flex: 0 0 auto;
}

.prof-dot {
  width: 0.7rem;
  height: 0.7rem;
  border-radius: 999px;
  border: 1px solid $color-border-strong;
  background: rgba(17, 24, 39, 0.65);
}

.prof-dot--half {
  border-color: rgba(249, 115, 22, 0.35);
  background: linear-gradient(90deg, rgba(249, 115, 22, 0.25) 50%, rgba(17, 24, 39, 0.65) 50%);
}

.prof-dot--prof {
  border-color: rgba(249, 115, 22, 0.55);
  background: rgba(249, 115, 22, 0.22);
  box-shadow: 0 0 0 2px rgba(249, 115, 22, 0.08);
}

.prof-dot--expert {
  border-color: rgba(249, 115, 22, 0.75);
  background: rgba(249, 115, 22, 0.22);
  box-shadow: 0 0 0 2px rgba(249, 115, 22, 0.10);
  outline: 2px solid rgba(249, 115, 22, 0.20);
  outline-offset: 1px;
}

.skill-row--prof {
  border-color: rgba(249, 115, 22, 0.35);
}

.skill-row--expert {
  border-color: rgba(249, 115, 22, 0.55);
}
</style>
