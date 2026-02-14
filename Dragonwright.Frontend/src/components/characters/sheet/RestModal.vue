<script setup lang="ts">
import {ref, computed, watch} from "vue";
import {useToast} from "@/composables/useToast";
import {putCharactersId, putCharactersIdClasses, putCharactersIdRace, putCharactersIdFeatsFeatId} from "@/api";
import type {CharacterClassData} from "@/api";
import UiModal from "@/components/ui/UiModal.vue";

const character = defineModel<any>("character", {required: true});

const props = defineProps<{
  characterId: string;
  visible: boolean;
}>();

const emit = defineEmits<{
  "update:visible": [value: boolean];
  refresh: [];
}>();

const {showToast} = useToast();

const restType = ref<"short" | "long">("short");
const hitDiceToSpend = ref<Record<string, number>>({});
const saving = ref(false);

watch(() => props.visible, (v) => {
  if (v) {
    restType.value = "short";
    hitDiceToSpend.value = {};
  }
});

const conMod = computed(() => {
  const ab = character.value?.abilities?.find((a: any) => a.ability === 2);
  return ab ? Math.floor((ab.score - 10) / 2) : 0;
});

const classHitDice = computed(() => {
  return (character.value?.classes ?? [])
    .filter((cc: any) => cc.level > 0)
    .map((cc: any) => ({
      classId: cc.classId,
      className: cc.class?.name ?? "Unknown",
      hitDie: cc.class?.hitDie ?? 8,
      level: cc.level,
    }));
});

const currentHitDieTotal = computed(() => character.value?.currentHitDie ?? 0);

function getClassHitDiceAvailable(classId: string): number {
  const total = currentHitDieTotal.value;
  const maxTotal = character.value?.maxHitDie ?? 0;
  const cc = classHitDice.value.find((c: any) => c.classId === classId);
  if (!cc) return 0;
  const allocated = Object.entries(hitDiceToSpend.value)
    .filter(([k]) => k !== classId)
    .reduce((sum, [, v]) => sum + v, 0);
  return Math.min(cc.level, Math.max(0, total - allocated));
}

function adjustHitDice(classId: string, delta: number) {
  const current = hitDiceToSpend.value[classId] ?? 0;
  const available = getClassHitDiceAvailable(classId);
  const newVal = Math.max(0, Math.min(available, current + delta));
  hitDiceToSpend.value[classId] = newVal;
}

const totalHitDiceToSpend = computed(() => {
  return Object.values(hitDiceToSpend.value).reduce((s, v) => s + v, 0);
});

const shortRestHealing = computed(() => {
  let total = 0;
  for (const cc of classHitDice.value) {
    const count = hitDiceToSpend.value[cc.classId] ?? 0;
    if (count <= 0) continue;
    const avg = Math.floor(cc.hitDie / 2) + 1 + conMod.value;
    total += Math.max(count, avg * count); // at least 1 per die
  }
  return total;
});

const shortRestHealingBreakdown = computed(() => {
  const parts: string[] = [];
  for (const cc of classHitDice.value) {
    const count = hitDiceToSpend.value[cc.classId] ?? 0;
    if (count <= 0) continue;
    const avg = Math.floor(cc.hitDie / 2) + 1;
    parts.push(`${count}d${cc.hitDie} avg ${(avg + conMod.value) * count}`);
  }
  return parts.join(" + ");
});

const longRestHitDiceRecovery = computed(() => {
  const max = character.value?.maxHitDie ?? 0;
  return Math.max(1, Math.floor(max / 2));
});

function getActionsToReset(resetTypeMax: number): {classFeatures: Record<string, Record<string, number>>; raceTraits: Record<string, number>; feats: Record<string, Record<string, number>>} {
  const classFeatures: Record<string, Record<string, number>> = {};
  const raceTraits: Record<string, number> = {};
  const feats: Record<string, Record<string, number>> = {};

  for (const cc of (character.value?.classes ?? [])) {
    const allFeatures = [...(cc.class?.features ?? []), ...(cc.subclass?.classFeatures ?? [])];
    for (const feat of allFeatures) {
      for (const action of (feat.actions ?? [])) {
        if (action.resetType != null && action.resetType <= resetTypeMax && action.resetType !== 3) {
          if (!classFeatures[cc.classId]) classFeatures[cc.classId] = {};
          classFeatures[cc.classId]![action.id] = 0;
        }
      }
    }
  }

  const traits = character.value?.race?.race?.traits ?? [];
  for (const trait of traits) {
    for (const action of (trait.actions ?? [])) {
      if (action.resetType != null && action.resetType <= resetTypeMax && action.resetType !== 3) {
        raceTraits[action.id] = 0;
      }
    }
  }

  for (const cf of (character.value?.feats ?? [])) {
    for (const action of (cf.feat?.actions ?? [])) {
      if (action.resetType != null && action.resetType <= resetTypeMax && action.resetType !== 3) {
        if (!feats[cf.id]) feats[cf.id] = {};
        feats[cf.id]![action.id] = 0;
      }
    }
  }

  return {classFeatures, raceTraits, feats};
}

function buildClassesPayload(): CharacterClassData[] {
  return (character.value?.classes ?? []).map((c: any) => ({
    id: c.id, classId: c.classId, level: c.level, subclassId: c.subclassId,
    isStartingClass: c.isStartingClass, classFeatureUsages: c.classFeatureUsages,
    chosenSkillProficiencies: c.chosenSkillProficiencies,
    chosenFeatureOptions: c.chosenFeatureOptions, chosenSpells: c.chosenSpells,
    spellSlotsUsed: c.spellSlotsUsed, pactSlotsUsed: c.pactSlotsUsed,
  }));
}

async function takeShortRest() {
  if (!character.value || saving.value) return;
  saving.value = true;

  try {
    let healing = 0;
    for (const cc of classHitDice.value) {
      const count = hitDiceToSpend.value[cc.classId] ?? 0;
      if (count <= 0) continue;
      const avg = Math.floor(cc.hitDie / 2) + 1 + conMod.value;
      healing += Math.max(count, avg * count);
    }

    const newHp = Math.min(
      (character.value.currentHitPoints ?? 0) + healing,
      character.value.maximumHitPoints ?? 0
    );

    const newHitDie = Math.max(0, (character.value.currentHitDie ?? 0) - totalHitDiceToSpend.value);

    const resets = getActionsToReset(0);

    for (const cc of (character.value.classes ?? [])) {
      if (resets.classFeatures[cc.classId]) {
        if (!cc.classFeatureUsages) cc.classFeatureUsages = {};
        for (const actionId of Object.keys(resets.classFeatures[cc.classId]!)) {
          cc.classFeatureUsages[actionId] = 0;
        }
      }

      if (cc.subclass?.canCastSpells && cc.class?.name?.toLowerCase() === "warlock") {
        cc.pactSlotsUsed = 0;
      }
    }

    if (character.value.race && Object.keys(resets.raceTraits).length > 0) {
      if (!character.value.race.raceTraitUsages) character.value.race.raceTraitUsages = {};
      for (const actionId of Object.keys(resets.raceTraits)) {
        character.value.race.raceTraitUsages[actionId] = 0;
      }
    }

    for (const cf of (character.value.feats ?? [])) {
      if (resets.feats[cf.id]) {
        if (!cf.featActionUsages) cf.featActionUsages = {};
        for (const actionId of Object.keys(resets.feats[cf.id]!)) {
          cf.featActionUsages[actionId] = 0;
        }
      }
    }

    character.value.currentHitPoints = newHp;
    character.value.currentHitDie = newHitDie;

    await putCharactersId(props.characterId, {
      name: character.value.name,
      currentHitPoints: newHp,
      currentHitDie: newHitDie,
    });

    await putCharactersIdClasses(props.characterId, {classes: buildClassesPayload()});

    if (character.value.race && Object.keys(resets.raceTraits).length > 0) {
      await putCharactersIdRace(props.characterId, {
        raceId: character.value.race.raceId,
        raceTraitUsages: character.value.race.raceTraitUsages ?? {},
        chosenTraitOptions: character.value.race.chosenTraitOptions ?? {},
        chosenSpells: character.value.race.chosenSpells ?? {},
      });
    }

    for (const cf of (character.value.feats ?? [])) {
      if (resets.feats[cf.id]) {
        await putCharactersIdFeatsFeatId(props.characterId, cf.id, {
          featId: cf.featId, source: cf.source, sourceId: cf.sourceId,
          chosenAbilityScoreIncrease: cf.chosenAbilityScoreIncrease,
          chosenOptions: cf.chosenOptions ?? {},
          chosenSpells: cf.chosenSpells ?? {},
          featActionUsages: cf.featActionUsages ?? {},
        });
      }
    }

    showToast({message: `Short rest complete. Healed ${healing} HP.`});
    emit("update:visible", false);
    emit("refresh");
  } catch {
    showToast({variant: "danger", message: "Failed to complete short rest."});
  } finally {
    saving.value = false;
  }
}

async function takeLongRest() {
  if (!character.value || saving.value) return;
  saving.value = true;

  try {
    const maxHp = character.value.maximumHitPoints ?? 0;
    const maxDie = character.value.maxHitDie ?? 0;
    const currentDie = character.value.currentHitDie ?? 0;
    const recoveredDie = Math.min(maxDie, currentDie + Math.max(1, Math.floor(maxDie / 2)));

    const resets = getActionsToReset(1);

    for (const cc of (character.value.classes ?? [])) {
      if (resets.classFeatures[cc.classId]) {
        if (!cc.classFeatureUsages) cc.classFeatureUsages = {};
        for (const actionId of Object.keys(resets.classFeatures[cc.classId]!)) {
          cc.classFeatureUsages[actionId] = 0;
        }
      }

      cc.spellSlotsUsed = {};
      cc.pactSlotsUsed = 0;
    }

    if (character.value.race && Object.keys(resets.raceTraits).length > 0) {
      if (!character.value.race.raceTraitUsages) character.value.race.raceTraitUsages = {};
      for (const actionId of Object.keys(resets.raceTraits)) {
        character.value.race.raceTraitUsages[actionId] = 0;
      }
    }

    for (const cf of (character.value.feats ?? [])) {
      if (resets.feats[cf.id]) {
        if (!cf.featActionUsages) cf.featActionUsages = {};
        for (const actionId of Object.keys(resets.feats[cf.id]!)) {
          cf.featActionUsages[actionId] = 0;
        }
      }
    }

    character.value.currentHitPoints = maxHp;
    character.value.temporaryHitPoints = 0;
    character.value.deathSaveSuccesses = 0;
    character.value.deathSaveFailures = 0;
    character.value.currentHitDie = recoveredDie;

    await putCharactersId(props.characterId, {
      name: character.value.name,
      currentHitPoints: maxHp,
      temporaryHitPoints: 0,
      deathSaveSuccesses: 0,
      deathSaveFailures: 0,
      currentHitDie: recoveredDie,
    });

    await putCharactersIdClasses(props.characterId, {classes: buildClassesPayload()});

    if (character.value.race) {
      await putCharactersIdRace(props.characterId, {
        raceId: character.value.race.raceId,
        raceTraitUsages: character.value.race.raceTraitUsages ?? {},
        chosenTraitOptions: character.value.race.chosenTraitOptions ?? {},
        chosenSpells: character.value.race.chosenSpells ?? {},
      });
    }

    for (const cf of (character.value.feats ?? [])) {
      if (resets.feats[cf.id]) {
        await putCharactersIdFeatsFeatId(props.characterId, cf.id, {
          featId: cf.featId, source: cf.source, sourceId: cf.sourceId,
          chosenAbilityScoreIncrease: cf.chosenAbilityScoreIncrease,
          chosenOptions: cf.chosenOptions ?? {},
          chosenSpells: cf.chosenSpells ?? {},
          featActionUsages: cf.featActionUsages ?? {},
        });
      }
    }

    showToast({message: "Long rest complete. Fully restored!"});
    emit("update:visible", false);
    emit("refresh");
  } catch {
    showToast({variant: "danger", message: "Failed to complete long rest."});
  } finally {
    saving.value = false;
  }
}

function close() {
  emit("update:visible", false);
}
</script>

<template>
  <UiModal :model-value="visible" @update:model-value="emit('update:visible', $event)" title="Rest" :close-on-backdrop="true" :close-on-esc="true" class="rest-modal">
    <div class="rest-tabs">
      <button class="rest-tab" :class="{ 'rest-tab--active': restType === 'short' }" @click="restType = 'short'">
        <i class="fas fa-mug-hot"/> Short Rest
      </button>
      <button class="rest-tab" :class="{ 'rest-tab--active': restType === 'long' }" @click="restType = 'long'">
        <i class="fas fa-bed"/> Long Rest
      </button>
    </div>

    <div v-if="restType === 'short'" class="rest-body">
      <p class="rest-desc">Spend hit dice to recover HP. Features that recharge on a short rest will be restored.</p>

      <div class="rest-section">
        <div class="rest-section__title">Spend Hit Dice</div>
        <div v-if="!classHitDice.length" class="rest-empty">No classes.</div>
        <div v-for="cc in classHitDice" :key="cc.classId" class="hit-die-row">
          <div class="hit-die-row__info">
            <span class="hit-die-row__name">{{ cc.className }}</span>
            <span class="hit-die-row__die">d{{ cc.hitDie }}</span>
            <span class="hit-die-row__avail">{{ getClassHitDiceAvailable(cc.classId) }} available</span>
          </div>
          <div class="hit-die-row__controls">
            <button class="hd-btn" @click="adjustHitDice(cc.classId, -1)" :disabled="(hitDiceToSpend[cc.classId] ?? 0) <= 0">
              <i class="fas fa-minus"/>
            </button>
            <span class="hd-count">{{ hitDiceToSpend[cc.classId] ?? 0 }}</span>
            <button class="hd-btn" @click="adjustHitDice(cc.classId, 1)" :disabled="(hitDiceToSpend[cc.classId] ?? 0) >= getClassHitDiceAvailable(cc.classId)">
              <i class="fas fa-plus"/>
            </button>
          </div>
        </div>
      </div>

      <div v-if="totalHitDiceToSpend > 0" class="rest-preview">
        <span class="rest-preview__label">Estimated Healing</span>
        <span class="rest-preview__value">{{ shortRestHealing }} HP</span>
        <span class="rest-preview__detail">{{ shortRestHealingBreakdown }}</span>
      </div>

      <button class="rest-confirm rest-confirm--short" @click="takeShortRest" :disabled="saving">
        <i v-if="saving" class="fas fa-spinner fa-spin"/>
        <i v-else class="fas fa-mug-hot"/>
        Take Short Rest
      </button>
    </div>

    <div v-else class="rest-body">
      <p class="rest-desc">Take a long rest to fully recover.</p>

      <div class="rest-summary">
        <div class="rest-summary__item">
          <i class="fas fa-heart rest-summary__icon rest-summary__icon--heal"/>
          HP restored to {{ character.maximumHitPoints ?? 0 }}
        </div>
        <div class="rest-summary__item">
          <i class="fas fa-shield-alt rest-summary__icon rest-summary__icon--temp"/>
          Temporary HP cleared
        </div>
        <div class="rest-summary__item">
          <i class="fas fa-dice rest-summary__icon"/>
          Recover {{ longRestHitDiceRecovery }} hit dice
        </div>
        <div class="rest-summary__item">
          <i class="fas fa-magic rest-summary__icon rest-summary__icon--spell"/>
          All spell slots recovered
        </div>
        <div class="rest-summary__item">
          <i class="fas fa-redo rest-summary__icon"/>
          All short/long rest features reset
        </div>
        <div class="rest-summary__item">
          <i class="fas fa-skull-crossbones rest-summary__icon rest-summary__icon--death"/>
          Death saves cleared
        </div>
      </div>

      <button class="rest-confirm rest-confirm--long" @click="takeLongRest" :disabled="saving">
        <i v-if="saving" class="fas fa-spinner fa-spin"/>
        <i v-else class="fas fa-bed"/>
        Take Long Rest
      </button>
    </div>
  </UiModal>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.rest-modal :deep(.modal) { max-width: 520px; }

.rest-tabs {
  display: flex;
  gap: $space-2;
  margin-bottom: $space-4;
}

.rest-tab {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: $space-2;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text-muted;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 600;
  transition: all 150ms ease;

  &:hover { border-color: $color-border-strong; color: $color-text; }

  &--active {
    border-color: rgba(249, 115, 22, 0.55);
    background: rgba(249, 115, 22, 0.10);
    color: $color-accent;
  }
}

.rest-body {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.rest-desc {
  color: $color-text-muted;
  font-size: 0.85rem;
  margin: 0;
  line-height: 1.5;
}

.rest-section {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.rest-section__title {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 700;
}

.rest-empty {
  color: $color-text-muted;
  font-size: 0.85rem;
}

.hit-die-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
  padding: $space-2 $space-3;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  background: $color-surface-alt;
}

.hit-die-row__info {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.hit-die-row__name {
  font-weight: 600;
  font-size: 0.9rem;
}

.hit-die-row__die {
  color: $color-accent;
  font-weight: 700;
  font-size: 0.85rem;
}

.hit-die-row__avail {
  color: $color-text-muted;
  font-size: 0.78rem;
}

.hit-die-row__controls {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.hd-btn {
  width: 1.6rem;
  height: 1.6rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: $radius-sm;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.65rem;
  transition: all 150ms ease;

  &:hover:not(:disabled) { border-color: $color-accent; color: $color-accent; }
  &:disabled { opacity: 0.35; cursor: not-allowed; }
}

.hd-count {
  min-width: 1.5rem;
  text-align: center;
  font-weight: 700;
  font-size: 1rem;
  color: $color-accent;
}

.rest-preview {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  padding: $space-2 $space-3;
  border: 1px solid rgba(34, 197, 94, 0.35);
  border-radius: $radius-md;
  background: rgba(34, 197, 94, 0.06);
}

.rest-preview__label {
  font-size: 0.72rem;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: $color-text-muted;
}

.rest-preview__value {
  font-size: 1.25rem;
  font-weight: 900;
  color: #22c55e;
}

.rest-preview__detail {
  font-size: 0.78rem;
  color: $color-text-muted;
}

.rest-summary {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.rest-summary__item {
  display: flex;
  align-items: center;
  gap: $space-2;
  font-size: 0.88rem;
  color: $color-text;
}

.rest-summary__icon {
  width: 1.2rem;
  text-align: center;
  color: $color-text-muted;
  font-size: 0.85rem;

  &--heal { color: #22c55e; }
  &--temp { color: rgba(56, 189, 248, 0.8); }
  &--spell { color: $color-accent; }
  &--death { color: #ef4444; }
}

.rest-confirm {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: $space-2;
  width: 100%;
  padding: $space-3;
  border-radius: $radius-md;
  border: 1px solid;
  cursor: pointer;
  font-size: 0.95rem;
  font-weight: 700;
  font-family: inherit;
  transition: all 150ms ease;

  &:disabled { opacity: 0.5; cursor: not-allowed; }

  &--short {
    border-color: rgba(249, 115, 22, 0.55);
    background: rgba(249, 115, 22, 0.12);
    color: $color-accent;

    &:hover:not(:disabled) { background: rgba(249, 115, 22, 0.20); }
  }

  &--long {
    border-color: rgba(56, 189, 248, 0.55);
    background: rgba(56, 189, 248, 0.10);
    color: rgba(56, 189, 248, 0.9);

    &:hover:not(:disabled) { background: rgba(56, 189, 248, 0.18); }
  }
}
</style>
