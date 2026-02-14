<script setup lang="ts">
import {computed} from "vue";
import UiCard from "@/components/ui/UiCard.vue";

const props = defineProps<{ character: any }>();

const hasAnySenses = computed(() => {
  const c = props.character;
  if (!c) return false;
  return (c.darkvisionRange ?? 0) > 0 || (c.blindsightRange ?? 0) > 0 || (c.tremorsenseRange ?? 0) > 0 || (c.truesightRange ?? 0) > 0;
});
</script>

<template>
  <UiCard title="Senses & Passives">
    <div class="kvlist">
      <div class="kvrow">
        <span class="kvrow__k">Passive Perception</span>
        <span class="kvrow__v">{{ character.passivePerception }}</span>
      </div>
      <div class="kvrow">
        <span class="kvrow__k">Passive Investigation</span>
        <span class="kvrow__v">{{ character.passiveInvestigation }}</span>
      </div>
      <div class="kvrow">
        <span class="kvrow__k">Passive Insight</span>
        <span class="kvrow__v">{{ character.passiveInsight }}</span>
      </div>

      <div class="divider"/>

      <div v-if="(character.darkvisionRange ?? 0) > 0" class="kvrow">
        <span class="kvrow__k">Darkvision</span>
        <span class="kvrow__v">{{ character.darkvisionRange }} ft</span>
      </div>
      <div v-if="(character.blindsightRange ?? 0) > 0" class="kvrow">
        <span class="kvrow__k">Blindsight</span>
        <span class="kvrow__v">{{ character.blindsightRange }} ft</span>
      </div>
      <div v-if="(character.tremorsenseRange ?? 0) > 0" class="kvrow">
        <span class="kvrow__k">Tremorsense</span>
        <span class="kvrow__v">{{ character.tremorsenseRange }} ft</span>
      </div>
      <div v-if="(character.truesightRange ?? 0) > 0" class="kvrow">
        <span class="kvrow__k">Truesight</span>
        <span class="kvrow__v">{{ character.truesightRange }} ft</span>
      </div>
      <div v-if="!hasAnySenses" class="kvrow">
        <span class="kvrow__k">Senses</span>
        <span class="kvrow__v kvrow__v--muted">None</span>
      </div>
    </div>
  </UiCard>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.kvlist {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.kvrow {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-3;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: rgba(5, 8, 20, 0.25);
}

.kvrow__k {
  color: $color-text-muted;
  font-size: 0.9rem;
}

.kvrow__v {
  color: $color-text;
  font-weight: 900;
}

.kvrow__v--muted {
  color: $color-text-muted;
  font-weight: 800;
}

.divider {
  height: 1px;
  background: rgba(55, 65, 81, 0.6);
  margin: $space-2 0;
}
</style>
