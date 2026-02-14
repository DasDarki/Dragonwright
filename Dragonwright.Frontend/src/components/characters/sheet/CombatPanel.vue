<script setup lang="ts">
import {ref, computed, watch} from "vue";
import {useToast} from "@/composables/useToast";
import {putCharactersId} from "@/api";
import {conditionLabels, defenseStateLabels} from "@/content/enums";
import UiGrid from "@/components/ui/layout/UiGrid.vue";
import UiModal from "@/components/ui/UiModal.vue";

const character = defineModel<any>("character", {required: true});

const props = defineProps<{
  characterId: string;
  openOverride: (type: string, targetId?: number) => void;
}>();

const {showToast} = useToast();

const hpInput = ref(0);
const showHpModal = ref(false);
const hpSaving = ref(false);
const showConditionsModal = ref(false);
const activeConditions = ref<number[]>([]);

const isAtZeroHp = computed(() => (character.value?.currentHitPoints ?? 1) <= 0);

const hitDiceBreakdown = computed(() => {
  return (character.value?.classes ?? [])
    .filter((cc: any) => cc.level > 0)
    .map((cc: any) => `${cc.level}d${cc.class?.hitDie ?? '?'}`)
    .join(' + ');
});

const hpPercent = computed(() => {
  const c = character.value;
  if (!c || !c.maximumHitPoints) return 100;
  return Math.min(100, Math.max(0, Math.round((c.currentHitPoints / c.maximumHitPoints) * 100)));
});

const hpBarClass = computed(() => {
  const p = hpPercent.value;
  if (p > 50) return "hp-bar__fill--green";
  if (p > 25) return "hp-bar__fill--yellow";
  return "hp-bar__fill--red";
});

const defenseList = computed(() => {
  const c = character.value;
  if (!c) return [];
  const dmg = c.damageDefenses ? Object.keys(c.damageDefenses) : [];
  const cond = c.conditionDefenses ? Object.keys(c.conditionDefenses) : [];
  return [...dmg, ...cond].filter(Boolean);
});

function signed(n: number): string {
  return n >= 0 ? `+${n}` : `${n}`;
}

async function applyHeal(amount: number) {
  if (!character.value || amount <= 0) return;
  hpSaving.value = true;
  const newHp = Math.min(character.value.currentHitPoints + amount, character.value.maximumHitPoints);
  try {
    await putCharactersId(props.characterId, {name: character.value.name, currentHitPoints: newHp});
    character.value.currentHitPoints = newHp;
  } catch {
    showToast({variant: "danger", message: "Failed to save HP."});
  } finally {
    hpSaving.value = false;
  }
}

async function applyDamage(amount: number) {
  if (!character.value || amount <= 0) return;
  hpSaving.value = true;
  let remaining = amount;
  let tempHp = character.value.temporaryHitPoints ?? 0;
  if (tempHp > 0) {
    const absorbed = Math.min(tempHp, remaining);
    tempHp -= absorbed;
    remaining -= absorbed;
  }
  const newHp = Math.max(0, character.value.currentHitPoints - remaining);
  try {
    await putCharactersId(props.characterId, {
      name: character.value.name,
      currentHitPoints: newHp,
      temporaryHitPoints: tempHp
    });
    character.value.currentHitPoints = newHp;
    character.value.temporaryHitPoints = tempHp;
  } catch {
    showToast({variant: "danger", message: "Failed to save HP."});
  } finally {
    hpSaving.value = false;
  }
}

async function setTempHp(amount: number) {
  if (!character.value || amount < 0) return;
  hpSaving.value = true;
  const newTemp = Math.max(character.value.temporaryHitPoints ?? 0, amount);
  try {
    await putCharactersId(props.characterId, {name: character.value.name, temporaryHitPoints: newTemp});
    character.value.temporaryHitPoints = newTemp;
  } catch {
    showToast({variant: "danger", message: "Failed to save temp HP."});
  } finally {
    hpSaving.value = false;
  }
}

function openHpModal() {
  hpInput.value = 0;
  showHpModal.value = true;
}

async function toggleDeathSave(type: "success" | "failure") {
  if (!character.value) return;
  const field = type === "success" ? "deathSaveSuccesses" : "deathSaveFailures";
  const current = character.value[field] ?? 0;
  const next = current >= 3 ? 0 : current + 1;
  try {
    await putCharactersId(props.characterId, {name: character.value.name, [field]: next});
    character.value[field] = next;
  } catch {
    showToast({variant: "danger", message: "Failed to save death saves."});
  }
}

async function setExhaustion(level: number) {
  if (!character.value) return;
  const next = (character.value.exhaustionLevel ?? 0) === level ? 0 : level;
  try {
    await putCharactersId(props.characterId, {name: character.value.name, exhaustionLevel: next});
    character.value.exhaustionLevel = next;
  } catch {
    showToast({variant: "danger", message: "Failed to save exhaustion."});
  }
}

async function toggleInspiration() {
  if (!character.value) return;
  const next = (character.value.inspirationPoints ?? 0) > 0 ? 0 : 1;
  try {
    await putCharactersId(props.characterId, {name: character.value.name, inspirationPoints: next});
    character.value.inspirationPoints = next;
  } catch {
    showToast({variant: "danger", message: "Failed to save inspiration."});
  }
}

async function toggleCondition(conditionId: number) {
  if (!character.value) return;
  const idx = activeConditions.value.indexOf(conditionId);
  if (idx >= 0) {
    activeConditions.value.splice(idx, 1);
  } else {
    activeConditions.value.push(conditionId);
  }
  try {
    await putCharactersId(props.characterId, {name: character.value.name, conditions: [...activeConditions.value]});
    character.value.conditions = [...activeConditions.value];
  } catch {
    showToast({variant: "danger", message: "Failed to save conditions."});
  }
}

function openConditionsModal() {
  syncCombatFromCharacter();
  showConditionsModal.value = true;
}

function syncCombatFromCharacter() {
  if (!character.value) return;
  activeConditions.value = [...(character.value.conditions ?? [])];
}

watch(() => character.value, () => {
  syncCombatFromCharacter();
}, {immediate: true});
</script>

<template>
  <UiGrid :cols="2" :cols-md="4" :gap="1" class="combat-grid">
    <div class="combat-card combat-card--hp">
      <div class="combat-card__k" @contextmenu.prevent="openOverride('hp')">Hit Points</div>
      <div class="combat-card__v">
        <span class="combat-card__main">{{ character.currentHitPoints }}</span>
        <span class="combat-card__sep">/</span>
        <span class="combat-card__main">{{ character.maximumHitPoints }}</span>
        <span v-if="(character.temporaryHitPoints ?? 0) > 0"
              class="combat-card__temp">+{{ character.temporaryHitPoints }} temp</span>
        <span v-if="hpSaving" class="hp-saving"><i class="fas fa-spinner fa-spin"/></span>
      </div>

      <div class="hp-bar">
        <div class="hp-bar__fill" :class="hpBarClass" :style="{ width: hpPercent + '%' }"/>
      </div>

      <div class="combat-card__sub hp-subline">
        <div>
          <span>Hit Dice {{ character.currentHitDie }}/{{ character.maxHitDie }}</span>
          <span v-if="hitDiceBreakdown" class="hit-dice-breakdown">({{ hitDiceBreakdown }})</span>
        </div>

        <button class="hp-adjust-btn" @click="openHpModal">
          <i class="fas fa-sliders-h"/> Adjust
        </button>
      </div>

      <div v-if="isAtZeroHp" class="death-saves death-saves--compact">
        <div class="death-saves__row">
          <span class="death-saves__label">S</span>
          <span class="pips">
            <span
              v-for="i in 3"
              :key="'ds' + i"
              class="pip pip--clickable"
              :class="{ on: (character.deathSaveSuccesses ?? 0) >= i }"
              @click="toggleDeathSave('success')"
            />
          </span>
        </div>

        <div class="death-saves__row">
          <span class="death-saves__label">F</span>
          <span class="pips pips--danger">
            <span
              v-for="i in 3"
              :key="'df' + i"
              class="pip pip--clickable"
              :class="{ on: (character.deathSaveFailures ?? 0) >= i }"
              @click="toggleDeathSave('failure')"
            />
          </span>
        </div>
      </div>
    </div>

    <div class="combat-card combat-card--accent">
      <h3 style="font-size: 1.1rem">Conditions & Defenses</h3>
      <div class="chips">
        <div class="chips__title">Conditions</div>
        <div class="chips__wrap">
          <span v-if="!(activeConditions.length)" class="chips__empty">None</span>
          <span
            v-for="c in activeConditions"
            :key="c"
            class="chip condition-chip--active"
          >
            {{ conditionLabels[c] ?? c }}
            <button class="condition-chip__remove" @click="toggleCondition(c)"><i class="fas fa-times"/></button>
          </span>
          <button class="condition-add-btn" @click="openConditionsModal"><i class="fas fa-plus"/></button>
        </div>
      </div>

      <div class="chips">
        <div class="chips__title" style="margin-top: 0.75rem" @contextmenu.prevent="openOverride('defense')">Defenses</div>
        <div class="chips__wrap">
          <span v-if="!defenseList.length" class="chips__empty">None</span>
          <span v-else v-for="d in defenseList" :key="d" class="chip chip--info">{{ d }}</span>
        </div>
      </div>

      <div class="status-row">
        <div class="status-block">
          <div class="status-block__k">Exhaustion</div>
          <div class="status-block__pips">
            <span
              v-for="i in 6"
              :key="'ex2' + i"
              class="exhaustion-pip"
              :class="{ on: (character.exhaustionLevel ?? 0) >= i }"
              @click="setExhaustion(i)"
            />
          </div>
        </div>

        <button class="inspiration-chip" @click="toggleInspiration">
          <i class="fas fa-star" :class="{ 'inspiration-star--active': (character.inspirationPoints ?? 0) > 0 }" />
          <span>Inspiration</span>
        </button>
      </div>
    </div>

    <div class="combat-card combat-card--accent-mix"
         style="display: flex; flex-direction: column; justify-content: space-between">
      <div @contextmenu.prevent="openOverride('ac')">
        <div class="combat-card__k">Armor Class</div>
        <div class="combat-card__v">
          <span class="combat-card__big">{{ character.armorClass }}</span>
        </div>
      </div>
      <div style="display: flex; flex-direction: column; align-items: flex-end" @contextmenu.prevent="openOverride('initiative')">
        <div class="combat-card__v">
          <span class="combat-card__big" v-roll>{{ signed(character.initiative) }}</span>
        </div>
        <div class="combat-card__k">Initiative</div>
      </div>
    </div>

    <div class="combat-card combat-card--accent-alt" @contextmenu.prevent="openOverride('speed')">
      <div class="combat-card__k">Speed</div>
      <div class="combat-card__v">
        <span class="combat-card__big">{{ character.movementSpeed }}</span>
        <span class="combat-card__unit">ft</span>
      </div>
      <div class="combat-card__sub">
        <span v-if="(character.swimmingSpeed ?? 0) > 0">Swim {{ character.swimmingSpeed }} ft</span>
        <span v-if="(character.flyingSpeed ?? 0) > 0">Fly {{ character.flyingSpeed }} ft</span>
      </div>
    </div>
  </UiGrid>

  <UiModal v-model="showHpModal" title="Adjust Hit Points" :close-on-backdrop="true" :close-on-esc="true"
           class="hp-modal" style="max-width: min(500px, 90vw)">
    <div class="hp-modal__input">
      <label class="hp-modal__label">Amount</label>
      <input
        type="number"
        class="hp-modal__number"
        v-model.number="hpInput"
        min="0"
        @keyup.enter="applyHeal(hpInput); showHpModal = false"
      />
    </div>
    <div class="hp-modal__actions">
      <button class="hp-btn hp-btn--heal hp-btn--lg" @click="applyHeal(hpInput); showHpModal = false">
        <i class="fas fa-heart"/> Heal
      </button>
      <button class="hp-btn hp-btn--damage hp-btn--lg" @click="applyDamage(hpInput); showHpModal = false">
        <i class="fas fa-bolt"/> Damage
      </button>
      <button class="hp-btn hp-btn--temp hp-btn--lg" @click="setTempHp(hpInput); showHpModal = false">
        <i class="fas fa-shield-alt"/> Set Temp HP
      </button>
    </div>
  </UiModal>

  <UiModal v-model="showConditionsModal" title="Conditions" :close-on-backdrop="true" :close-on-esc="true"
           class="conditions-modal" style="max-width: min(500px, 90vw)">
    <div class="condition-grid">
      <button
        v-for="(label, id) in conditionLabels"
        :key="id"
        class="condition-toggle"
        :class="{ 'condition-toggle--active': activeConditions.includes(Number(id)) }"
        @click="toggleCondition(Number(id))"
      >
        {{ label }}
      </button>
    </div>
  </UiModal>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.combat-grid {
  align-items: stretch;
}

.combat-card {
  position: relative;
  padding: $space-3;
  border-radius: $radius-lg;
  border: 1px solid $color-border-subtle;
  background: radial-gradient(900px 140px at 50% -60%, rgba(249, 115, 22, 0.10), transparent 60%),
  $color-surface;
  overflow: hidden;
}

.combat-card__k {
  font-size: 0.72rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.combat-card__v {
  margin-top: $space-2;
  display: flex;
  align-items: baseline;
  gap: $space-2;
}

.combat-card__main {
  font-size: 1.55rem;
  font-weight: 900;
}

.combat-card__big {
  font-size: 2rem;
  font-weight: 900;
}

.combat-card__sep {
  color: $color-text-soft;
  font-weight: 800;
}

.combat-card__unit {
  color: $color-text-soft;
  font-weight: 800;
}

.combat-card__sub {
  margin-top: $space-2;
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
  color: $color-text-muted;
  font-size: 0.85rem;
}

.combat-card__temp {
  color: $color-accent-alt;
  font-weight: 900;
}

.combat-card--hp {
  border-color: rgba(239, 68, 68, 0.40);
}

.combat-card--accent {
  border-color: rgba(249, 115, 22, 0.35);
}

.combat-card--accent-mix {
  border-top-color: rgba(249, 115, 22, 0.35);
  border-left-color: rgba(249, 115, 22, 0.35);
  border-right-color: rgba(56, 189, 248, 0.35);
  border-bottom-color: rgba(56, 189, 248, 0.35);
}

.combat-card--accent-alt {
  border-color: rgba(56, 189, 248, 0.35);
}

.pips {
  display: inline-flex;
  gap: 0.35rem;
}

.pip {
  width: 0.55rem;
  height: 0.55rem;
  border-radius: 999px;
  border: 1px solid $color-border-strong;
  background: rgba(17, 24, 39, 0.6);
}

.pip.on {
  border-color: rgba(249, 115, 22, 0.55);
  background: rgba(249, 115, 22, 0.22);
  box-shadow: 0 0 0 2px rgba(249, 115, 22, 0.08);
}

.pips--danger .pip.on {
  border-color: rgba(239, 68, 68, 0.55);
  background: rgba(239, 68, 68, 0.20);
  box-shadow: 0 0 0 2px rgba(239, 68, 68, 0.08);
}

.pip--clickable {
  cursor: pointer;
  transition: transform 120ms ease, box-shadow 120ms ease;

  &:hover {
    transform: scale(1.3);
  }
}

.hp-saving {
  color: $color-text-muted;
  font-size: 0.8rem;
  margin-left: $space-1;
}

.hp-bar {
  height: 4px;
  border-radius: 2px;
  background: rgba(17, 24, 39, 0.6);
  margin-top: $space-2;
  overflow: hidden;
}

.hp-bar__fill {
  height: 100%;
  border-radius: 2px;
  transition: width 300ms ease;

  &--green { background: #22c55e; }
  &--yellow { background: #eab308; }
  &--red { background: #ef4444; }
}

.hp-btn {
  display: inline-flex;
  align-items: center;
  gap: $space-1;
  padding: $space-1 $space-2;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  cursor: pointer;
  font-size: 0.78rem;
  font-weight: 600;
  transition: all 150ms ease;

  &--heal { color: #22c55e; border-color: rgba(34, 197, 94, 0.35); &:hover { background: rgba(34, 197, 94, 0.10); } }
  &--damage { color: #ef4444; border-color: rgba(239, 68, 68, 0.35); &:hover { background: rgba(239, 68, 68, 0.10); } }
  &--temp { color: $color-accent-alt; border-color: rgba(56, 189, 248, 0.35); &:hover { background: rgba(56, 189, 248, 0.10); } }
  &--lg { padding: $space-2 $space-3; font-size: 0.85rem; flex: 1; justify-content: center; }
}

.hp-modal :deep(.modal) { max-width: 420px; }

.hp-modal__input {
  display: flex;
  flex-direction: column;
  gap: $space-1;
  margin-bottom: $space-3;
}

.hp-modal__label {
  font-size: 0.8rem;
  color: $color-text-muted;
  font-weight: 600;
  text-transform: uppercase;
}

.hp-modal__number {
  width: 100%;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text;
  font-size: 1.2rem;
  font-weight: 900;
  font-family: inherit;
  text-align: center;

  &:focus { outline: none; border-color: $color-accent; }
  &::-webkit-inner-spin-button, &::-webkit-outer-spin-button { -webkit-appearance: none; margin: 0; }
  -moz-appearance: textfield;
}

.hp-modal__actions {
  display: flex;
  gap: $space-2;
}

.death-saves {
  margin-top: $space-3;
  padding-top: $space-3;
  border-top: 1px solid rgba(239, 68, 68, 0.20);
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.death-saves__row {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.death-saves__label {
  font-size: 0.78rem;
  color: $color-text-muted;
  min-width: 5.5rem;
}

.hp-subline {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
}

.hit-dice-breakdown {
  color: $color-text-soft;
  font-size: 0.78rem;
  margin-left: $space-1;
}

.hp-adjust-btn {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  padding: 0.35rem 0.6rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text-muted;
  cursor: pointer;
  font-size: 0.8rem;
  font-weight: 600;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
    background: rgba($color-accent, 0.06);
  }
}

.death-saves--compact {
  margin-top: $space-3;
  padding-top: $space-3;
  border-top: 1px solid rgba(239, 68, 68, 0.20);
  gap: $space-2;

  .death-saves__label {
    min-width: auto;
    width: 1.25rem;
    text-align: center;
    font-weight: 800;
    letter-spacing: 0.04em;
  }
}

.status-row {
  margin-top: $space-3;
  padding-top: $space-3;
  border-top: 1px solid $color-border-subtle;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
}

.status-block {
  display: flex;
  flex-direction: column;
  gap: $space-1;
}

.status-block__k {
  font-size: 0.72rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 700;
}

.status-block__pips {
  display: inline-flex;
  gap: 0.35rem;
  flex-wrap: wrap;
}

.inspiration-chip {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  padding: 0.45rem 0.7rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: rgba(5, 8, 20, 0.22);
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.85rem;
  font-weight: 700;
  transition: all 150ms ease;

  &:hover { border-color: rgba(251, 191, 36, 0.35); color: $color-text; }
  i { font-size: 1rem; color: $color-border-strong; transition: color 150ms ease, transform 150ms ease; }
  &:hover i { transform: scale(1.1); }
}

.inspiration-star--active {
  color: #fbbf24 !important;
  filter: drop-shadow(0 0 4px rgba(251, 191, 36, 0.40));
}

.exhaustion-pip {
  width: 0.65rem;
  height: 0.65rem;
  border-radius: 999px;
  border: 1px solid $color-border-strong;
  background: rgba(17, 24, 39, 0.6);
  cursor: pointer;
  transition: transform 120ms ease, background 120ms ease;

  &:hover { transform: scale(1.3); }
  &.on {
    border-color: rgba(249, 115, 22, 0.65);
    background: rgba(249, 115, 22, 0.30);
    box-shadow: 0 0 0 2px rgba(249, 115, 22, 0.10);
  }
}

.condition-chip--active {
  display: inline-flex;
  align-items: center;
  gap: $space-1;
  padding: 0.25rem 0.6rem;
  border-radius: $radius-pill;
  border: 1px solid rgba(249, 115, 22, 0.45);
  background: rgba(249, 115, 22, 0.12);
  font-size: 0.85rem;
  color: $color-accent;
}

.condition-chip__remove {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1rem;
  height: 1rem;
  background: transparent;
  border: none;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.6rem;
  padding: 0;
  border-radius: 999px;
  transition: color 120ms ease;

  &:hover { color: $color-danger; }
}

.condition-add-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.6rem;
  height: 1.6rem;
  border-radius: 999px;
  border: 1px dashed $color-border-subtle;
  background: transparent;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.65rem;
  transition: all 150ms ease;

  &:hover { border-color: $color-accent; color: $color-accent; }
}

.conditions-modal :deep(.modal) { max-width: 480px; }

.condition-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: $space-2;

  @media (max-width: 520px) { grid-template-columns: repeat(2, 1fr); }
}

.condition-toggle {
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text-muted;
  cursor: pointer;
  font-size: 0.85rem;
  font-weight: 500;
  text-align: center;
  transition: all 150ms ease;

  &:hover { border-color: $color-border-strong; color: $color-text; }
  &--active {
    border-color: rgba(249, 115, 22, 0.55);
    background: rgba(249, 115, 22, 0.12);
    color: $color-accent;
    font-weight: 600;
  }
}

.chips__title {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  margin-bottom: $space-2;
}

.chips__wrap {
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

.chip--info {
  border-color: rgba(56, 189, 248, 0.35);
  background: rgba(56, 189, 248, 0.10);
  color: $color-accent-alt;
}

@media (max-width: 640px) {
  .combat-card--hp .combat-card__main { font-size: 1.35rem; }
  .hp-bar { margin-top: $space-2; }
}
</style>
