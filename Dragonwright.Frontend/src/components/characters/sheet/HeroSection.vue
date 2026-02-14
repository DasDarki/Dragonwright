<script setup lang="ts">
import {ref, computed, nextTick} from "vue";
import {useToast} from "@/composables/useToast";
import {putCharactersId} from "@/api";
import UiBadge from "@/components/ui/UiBadge.vue";
import Avatar from "@/components/characters/Avatar.vue";
import {alignmentLabels, sizeLabels} from "@/content/enums";

const character = defineModel<any>("character", {required: true});
const props = defineProps<{ characterId: string }>();

const {showToast} = useToast();

const XP_THRESHOLDS = [
  0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000,
  85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000,
];

const classSummary = computed(() => {
  if (!character.value?.classes?.length) return "No class";
  return character.value.classes.map((c: any) => `${c.class?.name ?? "Unknown"} ${c.level}`).join(" / ");
});

const raceName = computed(() => character.value?.race?.race?.name ?? "No race");
const backgroundName = computed(() => character.value?.background?.background?.name ?? "No background");

const currentLevel = computed(() => character.value?.level ?? 1);
const xpForCurrentLevel = computed(() => XP_THRESHOLDS[Math.min(currentLevel.value - 1, 19)] ?? 0);
const xpForNextLevel = computed(() => currentLevel.value >= 20 ? XP_THRESHOLDS[19]! : XP_THRESHOLDS[currentLevel.value]!);
const xpPercent = computed(() => {
  const xp = character.value?.xp ?? 0;
  if (currentLevel.value >= 20) return 100;
  const range = xpForNextLevel.value - xpForCurrentLevel.value;
  if (range <= 0) return 100;
  return Math.min(100, Math.max(0, Math.round(((xp - xpForCurrentLevel.value) / range) * 100)));
});

const editingXp = ref(false);
const xpInput = ref(0);
const xpInputRef = ref<HTMLInputElement | null>(null);
const xpSaving = ref(false);

function startEditXp() {
  xpInput.value = character.value?.xp ?? 0;
  editingXp.value = true;
  nextTick(() => xpInputRef.value?.select());
}

async function saveXp() {
  editingXp.value = false;
  if (!character.value || xpInput.value === (character.value.xp ?? 0)) return;
  xpSaving.value = true;
  try {
    await putCharactersId(props.characterId, {name: character.value.name, xp: xpInput.value});
    character.value.xp = xpInput.value;
  } catch {
    showToast({variant: "danger", message: "Failed to save XP."});
  } finally {
    xpSaving.value = false;
  }
}
</script>

<template>
  <section class="hero">
    <div class="hero__left">
      <Avatar :character="character" allow-edit class="hero__avatar"/>
      <div class="hero__info">
        <div class="hero__badges">
          <UiBadge v-if="character.level" :label="`Level ${character.level}`" variant="accent"/>
          <UiBadge v-if="character.alignment !== undefined" :label="alignmentLabels[character.alignment] ?? ''"
                   variant="muted"/>
          <UiBadge v-if="character.size !== undefined" :label="sizeLabels[character.size] ?? ''" variant="muted"/>
        </div>
        <div v-if="character.advancementType === 0" class="hero__xp">
          <div class="hero__xp-bar">
            <div class="hero__xp-bar-fill" :style="{ width: xpPercent + '%' }"/>
          </div>
          <div class="hero__xp-text">
            <template v-if="editingXp">
              <input
                ref="xpInputRef"
                v-model.number="xpInput"
                type="number"
                class="hero__xp-input"
                min="0"
                @blur="saveXp"
                @keyup.enter="($event.target as HTMLInputElement).blur()"
              />
            </template>
            <template v-else>
              <span class="hero__xp-value" @click="startEditXp" title="Click to edit XP">
                {{ character.xp ?? 0 }} XP
                <i v-if="xpSaving" class="fas fa-spinner fa-spin"/>
              </span>
              <span v-if="currentLevel < 20" class="hero__xp-next">/ {{ xpForNextLevel }} XP</span>
            </template>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.hero {
  position: relative;
  display: grid;
  grid-template-columns: 1fr;
  gap: $space-3;
  padding: $space-4;
  border-radius: $radius-xl;
  border: 1px solid $color-border-subtle;
  background: radial-gradient(1200px 220px at 20% -40%, rgba(249, 115, 22, 0.16), transparent 60%),
  radial-gradient(900px 240px at 80% -30%, rgba(56, 189, 248, 0.12), transparent 55%),
  $color-surface;
  overflow: hidden;
}

.hero::before {
  content: "";
  position: absolute;
  inset: 0;
  border-radius: $radius-xl;
  pointer-events: none;
  background: linear-gradient(90deg, rgba(249, 115, 22, 0.14), rgba(56, 189, 248, 0.10), rgba(249, 115, 22, 0.14));
  mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
  -webkit-mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
  padding: 1px;
  mask-composite: exclude;
  -webkit-mask-composite: xor;
  opacity: 0.9;
}

.hero__left {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-4;
  z-index: 1;
}

.hero__avatar {
  width: 5.2rem;
  height: 5.2rem;
  border-radius: $radius-xl;
  flex-shrink: 0;
}

.hero__info {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: $space-2;
}

.hero__badges {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: $space-2;
}

.hero__xp {
  width: 100%;
  max-width: 280px;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.hero__xp-bar {
  height: 5px;
  border-radius: 3px;
  background: rgba(17, 24, 39, 0.5);
  overflow: hidden;
}

.hero__xp-bar-fill {
  height: 100%;
  border-radius: 3px;
  background: linear-gradient(90deg, $color-accent, rgba(56, 189, 248, 0.9));
  transition: width 300ms ease;
}

.hero__xp-text {
  display: flex;
  align-items: center;
  gap: $space-1;
  font-size: 0.78rem;
  justify-content: flex-end;
}

.hero__xp-value {
  color: $color-text;
  font-weight: 600;
  cursor: pointer;
  transition: color 120ms ease;

  &:hover { color: $color-accent; }
}

.hero__xp-next {
  color: $color-text-muted;
}

.hero__xp-input {
  width: 80px;
  padding: 0.15rem 0.4rem;
  border-radius: $radius-sm;
  border: 1px solid $color-accent;
  background: $color-surface-alt;
  color: $color-text;
  font-size: 0.78rem;
  font-weight: 600;
  font-family: inherit;
  text-align: right;

  &:focus { outline: none; box-shadow: 0 0 0 2px rgba($color-accent, 0.25); }
  &::-webkit-inner-spin-button, &::-webkit-outer-spin-button { -webkit-appearance: none; margin: 0; }
  -moz-appearance: textfield;
}

@media (max-width: 640px) {
  .hero {
    padding: $space-3;
  }
  .hero__left {
    flex-direction: column;
    align-items: center;
  }
  .hero__info {
    align-items: center;
  }
  .hero__badges {
    justify-content: center;
  }
  .hero__xp {
    max-width: 100%;
  }
}
</style>
