<script setup lang="ts">
import {ref, computed} from "vue";
import {useToast} from "@/composables/useToast";
import {
  getSpells, postCharactersIdSpells, putCharactersIdSpellsSpellId,
  deleteCharactersIdSpellsSpellId, putCharactersIdClasses
} from "@/api";
import type {CharacterClassData} from "@/api";
import {abilityScoreLabels, spellLevelLabels, spellSchoolLabels} from "@/content/enums";
import UiBadge from "@/components/ui/UiBadge.vue";
import UiModal from "@/components/ui/UiModal.vue";
import UiInput from "@/components/ui/UiInput.vue";
import {abilityAbbrev} from "@/composables/useCharacterHelpers";

const character = defineModel<any>("character", {required: true});
const props = defineProps<{ characterId: string }>();
const emit = defineEmits<{ refresh: [] }>();

const {showToast} = useToast();

const FULL_CASTER_SLOTS: Record<number, number[]> = {
  1: [2], 2: [3], 3: [4,2], 4: [4,3], 5: [4,3,2],
  6: [4,3,3], 7: [4,3,3,1], 8: [4,3,3,2], 9: [4,3,3,3,1],
  10: [4,3,3,3,2], 11: [4,3,3,3,2,1], 12: [4,3,3,3,2,1],
  13: [4,3,3,3,2,1,1], 14: [4,3,3,3,2,1,1], 15: [4,3,3,3,2,1,1,1],
  16: [4,3,3,3,2,1,1,1], 17: [4,3,3,3,2,1,1,1,1], 18: [4,3,3,3,3,1,1,1,1],
  19: [4,3,3,3,3,2,1,1,1], 20: [4,3,3,3,3,2,2,1,1],
};

const HALF_CASTER_SLOTS: Record<number, number[]> = {
  2: [2], 3: [3], 4: [3], 5: [4,2], 6: [4,2], 7: [4,3],
  8: [4,3], 9: [4,3,2], 10: [4,3,2], 11: [4,3,3],
  12: [4,3,3], 13: [4,3,3,1], 14: [4,3,3,1], 15: [4,3,3,2],
  16: [4,3,3,2], 17: [4,3,3,3,1], 18: [4,3,3,3,1], 19: [4,3,3,3,2],
  20: [4,3,3,3,2],
};

const spellcastingClasses = computed(() => {
  return (character.value?.classes ?? [])
    .filter((cc: any) => cc.subclass?.canCastSpells)
    .map((cc: any) => ({
      classId: cc.classId,
      className: cc.class?.name ?? "Unknown",
      subclassName: cc.subclass?.name,
      level: cc.level,
      spellcastingAbility: cc.subclass?.spellcastingAbility,
      spellPrepareType: cc.subclass?.spellPrepareType,
      spellLearnType: cc.subclass?.spellLearnType,
      knowsAllSpells: cc.subclass?.knowsAllSpells ?? false,
      additionalSpellListIds: (cc.subclass?.additionalSpellLists ?? []).map((c: any) => c.id),
      additionalSpells: cc.subclass?.additionalSpells ?? [],
      spellSlotsUsed: cc.spellSlotsUsed ?? {},
      pactSlotsUsed: cc.pactSlotsUsed ?? 0,
      isStartingClass: cc.isStartingClass,
      characterClassId: cc.id,
    }));
});

const spellStats = computed(() => {
  const stats: Record<string, { abilityName: string; modifier: number; attackBonus: number; saveDC: number }> = {};
  const prof = character.value?.proficiencyBonus ?? 2;
  for (const sc of spellcastingClasses.value) {
    if (sc.spellcastingAbility == null) continue;
    const ab = character.value?.abilities?.find((a: any) => a.ability === sc.spellcastingAbility);
    const mod = ab ? Math.floor((ab.score - 10) / 2) : 0;
    stats[sc.classId] = {
      abilityName: abilityAbbrev[sc.spellcastingAbility] ?? "???",
      modifier: mod,
      attackBonus: mod + prof,
      saveDC: 8 + mod + prof,
    };
  }
  return stats;
});

const preparedCounts = computed(() => {
  const counts: Record<string, { current: number; max: number }> = {};
  for (const sc of spellcastingClasses.value) {
    if (sc.spellLearnType !== 1) continue;
    const classSpells = (character.value?.spells ?? [])
      .filter((s: any) => s.sourceClassId === sc.classId && !s.alwaysPrepared && (s.spell?.level ?? 0) > 0);
    const current = classSpells.filter((s: any) => s.isPrepared).length;
    const mod = spellStats.value[sc.classId]?.modifier ?? 0;
    const max = Math.max(1, mod + (sc.spellPrepareType === 0 ? sc.level : Math.floor(sc.level / 2)));
    counts[sc.classId] = {current, max};
  }
  return counts;
});

function signed(n: number): string {
  return n >= 0 ? `+${n}` : `${n}`;
}

function getMaxSpellSlots(classId: string): Record<number, number> {
  const sc = spellcastingClasses.value.find((c: any) => c.classId === classId);
  if (!sc) return {};
  const table = sc.spellPrepareType === 0 ? FULL_CASTER_SLOTS : HALF_CASTER_SLOTS;
  const slots = table[sc.level] ?? [];
  const result: Record<number, number> = {};
  slots.forEach((count, idx) => { result[idx + 1] = count; });
  return result;
}

const spellsByClass = computed(() => {
  const groups: Record<string, Record<number, any[]>> = {};
  for (const sc of spellcastingClasses.value) {
    groups[sc.classId] = {};
  }
  groups["other"] = {};
  for (const cs of (character.value?.spells ?? [])) {
    const key = cs.sourceClassId && groups[cs.sourceClassId] ? cs.sourceClassId : "other";
    const level = cs.spell?.level ?? 0;
    if (!groups[key]![level]) groups[key]![level] = [];
    groups[key]![level].push(cs);
  }
  for (const classGroup of Object.values(groups)) {
    for (const levelSpells of Object.values(classGroup)) {
      levelSpells.sort((a: any, b: any) => (a.spell?.name ?? "").localeCompare(b.spell?.name ?? ""));
    }
  }
  return groups;
});

const showAddSpellModal = ref(false);
const addSpellSearch = ref("");
const addSpellResults = ref<any[]>([]);
const addSpellLoading = ref(false);
const addSpellClassId = ref("");
let addSpellTimer: ReturnType<typeof setTimeout> | null = null;
const expandedSpells = ref<Set<string>>(new Set());

function toggleSpellExpand(id: string) {
  if (expandedSpells.value.has(id)) expandedSpells.value.delete(id);
  else expandedSpells.value.add(id);
}

function openAddSpellModal(classId: string) {
  addSpellClassId.value = classId;
  addSpellSearch.value = "";
  addSpellResults.value = [];
  showAddSpellModal.value = true;
  searchSpells("");
}

function onAddSpellSearchInput() {
  if (addSpellTimer) clearTimeout(addSpellTimer);
  addSpellTimer = setTimeout(() => searchSpells(addSpellSearch.value), 300);
}

async function searchSpells(q: string) {
  addSpellLoading.value = true;
  try {
    const existingIds = new Set((character.value?.spells ?? []).map((s: any) => s.spellId));
    const res = await getSpells({pageSize: 100, search: q || undefined, classId: addSpellClassId.value || undefined});
    let spells = ((res as any).data?.items ?? []) as any[];

    const sc = spellcastingClasses.value.find((c: any) => c.classId === addSpellClassId.value);
    if (sc && sc.additionalSpellListIds.length > 0) {
      for (const listClassId of sc.additionalSpellListIds) {
        const extra = await getSpells({pageSize: 100, search: q || undefined, classId: listClassId});
        const extraSpells = ((extra as any).data?.items ?? []) as any[];
        const existing = new Set(spells.map((s: any) => s.id));
        for (const s of extraSpells) {
          if (!existing.has(s.id)) spells.push(s);
        }
      }
    }

    addSpellResults.value = spells.filter((s: any) => !existingIds.has(s.id));
  } catch {
    showToast({variant: "danger", message: "Failed to search spells."});
  } finally {
    addSpellLoading.value = false;
  }
}

async function addSpellToCharacter(spell: any) {
  try {
    await postCharactersIdSpells(props.characterId, {
      spellId: spell.id,
      spellSource: 0,
      sourceClassId: addSpellClassId.value || undefined,
      isPrepared: false,
    });
    showToast({message: `Added ${spell.name}.`});
    emit("refresh");
  } catch {
    showToast({variant: "danger", message: "Failed to add spell."});
  }
}

async function removeSpell(charSpell: any) {
  try {
    await deleteCharactersIdSpellsSpellId(props.characterId, charSpell.id);
    character.value.spells = character.value.spells.filter((s: any) => s.id !== charSpell.id);
    showToast({message: "Spell removed."});
  } catch {
    showToast({variant: "danger", message: "Failed to remove spell."});
  }
}

async function togglePrepared(charSpell: any) {
  try {
    await putCharactersIdSpellsSpellId(props.characterId, charSpell.id, {
      isPrepared: !charSpell.isPrepared,
    });
    charSpell.isPrepared = !charSpell.isPrepared;
  } catch {
    showToast({variant: "danger", message: "Failed to update spell."});
  }
}

async function toggleSlot(classId: string, level: number, delta: number) {
  const cc = (character.value?.classes ?? []).find((c: any) => c.classId === classId);
  if (!cc) return;
  const max = getMaxSpellSlots(classId)[level] ?? 0;
  const current = Number(cc.spellSlotsUsed?.[level] ?? 0);
  const newVal = Math.max(0, Math.min(max, current + delta));
  if (!cc.spellSlotsUsed) cc.spellSlotsUsed = {};
  cc.spellSlotsUsed[level] = newVal;

  try {
    const classes: CharacterClassData[] = (character.value?.classes ?? []).map((c: any) => ({
      id: c.id, classId: c.classId, level: c.level, subclassId: c.subclassId,
      isStartingClass: c.isStartingClass, classFeatureUsages: c.classFeatureUsages,
      chosenSkillProficiencies: c.chosenSkillProficiencies,
      chosenFeatureOptions: c.chosenFeatureOptions, chosenSpells: c.chosenSpells,
      spellSlotsUsed: c.spellSlotsUsed, pactSlotsUsed: c.pactSlotsUsed,
    }));
    await putCharactersIdClasses(props.characterId, {classes});
  } catch {
    showToast({variant: "danger", message: "Failed to update spell slots."});
  }
}

function getSlotUsed(classId: string, level: number): number {
  const cc = (character.value?.classes ?? []).find((c: any) => c.classId === classId);
  return Number(cc?.spellSlotsUsed?.[level] ?? 0);
}

function formatCastingTime(time: any): string {
  if (!time) return "";
  if (time.amount === 1 && time.unit === 5) return "1 Action";
  if (time.amount === 1 && time.unit === 6) return "1 Bonus Action";
  if (time.amount === 1 && time.unit === 7) return "1 Reaction";
  return `${time.amount} ${timeUnitLabel(time.unit)}`;
}

function timeUnitLabel(unit: number): string {
  const labels: Record<number, string> = {
    0: "seconds", 1: "minutes", 2: "hours", 3: "days", 4: "rounds",
    5: "actions", 6: "bonus actions", 7: "reactions",
  };
  return labels[unit] ?? "";
}

function formatRange(range: number): string {
  if (range === 0) return "Self";
  if (range === -1) return "Touch";
  if (range === -2) return "Unlimited";
  return `${range} ft.`;
}

function formatComponents(spell: any): string {
  const parts: string[] = [];
  if (spell?.hasVocalComponent) parts.push("V");
  if (spell?.hasSomaticComponent) parts.push("S");
  if (spell?.hasMaterialComponent) parts.push("M");
  return parts.join(", ");
}
</script>

<template>
  <div v-if="!spellcastingClasses.length && !(character.spells?.length)" class="tabpanel__empty">
    No spellcasting ability.
  </div>
  <div v-else class="spells-tab">
    <div v-for="sc in spellcastingClasses" :key="sc.classId" class="spell-class-section">
      <div class="spell-class-header">
        <div class="spell-class-header__title">{{ sc.className }}</div>
        <div v-if="sc.subclassName" class="spell-class-header__sub">{{ sc.subclassName }}</div>
        <div class="spell-stats">
          <div class="spell-stat">
            <span class="spell-stat__label">Ability</span>
            <span class="spell-stat__value">{{ spellStats[sc.classId]?.abilityName }}</span>
          </div>
          <div class="spell-stat">
            <span class="spell-stat__label">Attack</span>
            <span class="spell-stat__value">{{ signed(spellStats[sc.classId]?.attackBonus ?? 0) }}</span>
          </div>
          <div class="spell-stat">
            <span class="spell-stat__label">Save DC</span>
            <span class="spell-stat__value">{{ spellStats[sc.classId]?.saveDC ?? 0 }}</span>
          </div>
          <div class="spell-stat">
            <span class="spell-stat__label">Modifier</span>
            <span class="spell-stat__value">{{ signed(spellStats[sc.classId]?.modifier ?? 0) }}</span>
          </div>
          <div v-if="preparedCounts[sc.classId]" class="spell-stat">
            <span class="spell-stat__label">Prepared</span>
            <span class="spell-stat__value" :class="{ 'spell-stat__value--over': preparedCounts[sc.classId].current > preparedCounts[sc.classId].max }">
              {{ preparedCounts[sc.classId].current }}/{{ preparedCounts[sc.classId].max }}
            </span>
          </div>
        </div>
      </div>

      <div v-for="level in [0,1,2,3,4,5,6,7,8,9]" :key="level">
        <div v-if="(spellsByClass[sc.classId]?.[level]?.length ?? 0) > 0 || (level > 0 && (getMaxSpellSlots(sc.classId)[level] ?? 0) > 0)" class="spell-level-section">
          <div class="spell-level-header">
            <span class="spell-level-header__name">{{ spellLevelLabels[level] ?? `Level ${level}` }}</span>
            <div v-if="level > 0 && (getMaxSpellSlots(sc.classId)[level] ?? 0) > 0" class="slot-tracker">
              <button
                v-for="s in (getMaxSpellSlots(sc.classId)[level] ?? 0)"
                :key="s"
                class="slot-pip"
                :class="{ 'slot-pip--used': s <= getSlotUsed(sc.classId, level) }"
                @click="toggleSlot(sc.classId, level, s <= getSlotUsed(sc.classId, level) ? -1 : 1)"
              />
            </div>
          </div>

          <div v-for="cs in (spellsByClass[sc.classId]?.[level] ?? [])" :key="cs.id"
               class="spell-card" :class="{ 'spell-card--expanded': expandedSpells.has(cs.id) }">
            <div class="spell-card__header" @click="toggleSpellExpand(cs.id)">
              <span class="spell-card__name">{{ cs.spell?.name }}</span>
              <div class="spell-card__badges">
                <UiBadge v-if="cs.alwaysPrepared" label="Always" variant="accent"/>
                <UiBadge v-else-if="cs.isPrepared" label="Prepared" variant="info"/>
                <UiBadge v-if="cs.spell?.concentration" label="C" variant="muted"/>
                <UiBadge v-if="cs.spell?.ritual" label="R" variant="muted"/>
              </div>
            </div>
            <div class="spell-card__body">
              <div class="spell-card__meta">
                <span>{{ spellSchoolLabels[cs.spell?.school ?? 0] }}</span>
                <span v-if="cs.spell?.castingTimes?.length">{{ formatCastingTime(cs.spell.castingTimes[0]) }}</span>
                <span v-if="cs.spell?.range != null">{{ formatRange(cs.spell.range) }}</span>
                <span>{{ formatComponents(cs.spell) }}</span>
              </div>
              <div v-if="cs.spell?.description" class="spell-card__desc" v-html="cs.spell.description"/>
              <div class="spell-card__actions">
                <button v-if="!cs.alwaysPrepared && level > 0" class="spell-card__action" @click.stop="togglePrepared(cs)">
                  {{ cs.isPrepared ? 'Unprepare' : 'Prepare' }}
                </button>
                <button class="spell-card__action spell-card__action--danger" @click.stop="removeSpell(cs)">Remove</button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <button class="spell-add-btn" @click="openAddSpellModal(sc.classId)">+ Add Spell</button>
    </div>

    <div v-if="Object.keys(spellsByClass['other'] ?? {}).length" class="spell-class-section">
      <div class="spell-class-header">
        <div class="spell-class-header__title">Other Spells</div>
      </div>
      <div v-for="level in [0,1,2,3,4,5,6,7,8,9]" :key="level">
        <div v-if="(spellsByClass['other']?.[level]?.length ?? 0) > 0" class="spell-level-section">
          <div class="spell-level-header">
            <span class="spell-level-header__name">{{ spellLevelLabels[level] }}</span>
          </div>
          <div v-for="cs in (spellsByClass['other']?.[level] ?? [])" :key="cs.id"
               class="spell-card" :class="{ 'spell-card--expanded': expandedSpells.has(cs.id) }">
            <div class="spell-card__header" @click="toggleSpellExpand(cs.id)">
              <span class="spell-card__name">{{ cs.spell?.name }}</span>
              <div class="spell-card__badges">
                <UiBadge v-if="cs.alwaysPrepared" label="Always" variant="accent"/>
                <UiBadge v-else-if="cs.isPrepared" label="Prepared" variant="info"/>
                <UiBadge v-if="cs.spell?.concentration" label="C" variant="muted"/>
                <UiBadge v-if="cs.spell?.ritual" label="R" variant="muted"/>
              </div>
            </div>
            <div class="spell-card__body">
              <div class="spell-card__meta">
                <span>{{ spellSchoolLabels[cs.spell?.school ?? 0] }}</span>
                <span v-if="cs.spell?.castingTimes?.length">{{ formatCastingTime(cs.spell.castingTimes[0]) }}</span>
                <span v-if="cs.spell?.range != null">{{ formatRange(cs.spell.range) }}</span>
                <span>{{ formatComponents(cs.spell) }}</span>
              </div>
              <div v-if="cs.spell?.description" class="spell-card__desc" v-html="cs.spell.description"/>
              <div class="spell-card__actions">
                <button v-if="!cs.alwaysPrepared && (cs.spell?.level ?? 0) > 0" class="spell-card__action" @click.stop="togglePrepared(cs)">
                  {{ cs.isPrepared ? 'Unprepare' : 'Prepare' }}
                </button>
                <button class="spell-card__action spell-card__action--danger" @click.stop="removeSpell(cs)">Remove</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <UiModal v-model="showAddSpellModal" title="Add Spell" :close-on-backdrop="true" :close-on-esc="true" class="inv-modal">
    <div class="add-spell-modal">
      <UiInput v-model="addSpellSearch" placeholder="Search spells..." @input="onAddSpellSearchInput"/>
      <div class="add-spell-modal__list">
        <div v-if="addSpellLoading" class="add-spell-modal__loading">Searching...</div>
        <div v-for="spell in addSpellResults" :key="spell.id" class="add-spell-modal__item" @click="addSpellToCharacter(spell)">
          <span class="add-spell-modal__name">{{ spell.name }}</span>
          <div class="add-spell-modal__info">
            <UiBadge :label="spellLevelLabels[spell.level ?? 0]!" variant="muted"/>
            <UiBadge :label="spellSchoolLabels[spell.school ?? 0]!" variant="muted"/>
          </div>
        </div>
        <div v-if="!addSpellLoading && !addSpellResults.length" class="add-spell-modal__empty">No spells found.</div>
      </div>
    </div>
  </UiModal>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.tabpanel__empty { color: $color-text-muted; padding: $space-4; text-align: center; }

.spells-tab { display: flex; flex-direction: column; gap: 2rem; }

.spell-class-section { display: flex; flex-direction: column; gap: 0.75rem; }

.spell-class-header { display: flex; flex-direction: column; gap: 0.5rem; }

.spell-class-header__title {
  font-size: 1rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.05em; color: $color-text-muted;
}

.spell-class-header__sub { font-size: 0.85rem; color: $color-text-muted; }

.spell-stats { display: flex; gap: 1rem; flex-wrap: wrap; }

.spell-stat {
  display: flex; flex-direction: column; align-items: center;
  background: $color-bg-elevated; border: 1px solid $color-border-subtle;
  border-radius: $radius-md; padding: 0.5rem 1rem; min-width: 70px;
}

.spell-stat__label { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.05em; color: $color-text-muted; }
.spell-stat__value { font-size: 1.1rem; font-weight: 600; &--over { color: $color-danger; } }

.spell-level-section { display: flex; flex-direction: column; gap: 0.25rem; }

.spell-level-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 0.25rem 0; border-bottom: 1px solid $color-border-subtle; margin-bottom: 0.25rem;
}

.spell-level-header__name {
  font-size: 0.85rem; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.05em; color: $color-text-muted;
}

.slot-tracker { display: flex; gap: 0.35rem; }

.slot-pip {
  width: 18px; height: 18px; border-radius: 50%;
  border: 2px solid $color-accent; background: transparent;
  cursor: pointer; padding: 0; transition: background 0.15s;
  &--used { background: $color-accent; }
  &:hover { opacity: 0.8; }
}

.spell-card {
  border: 1px solid $color-border-subtle; border-radius: $radius-md;
  background: $color-bg-elevated; overflow: hidden;

  &__header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 0.5rem 0.75rem; cursor: pointer;
  }
  &__name { font-weight: 500; }
  &__badges { display: flex; gap: 0.35rem; }
  &__body { display: none; padding: 0.5rem 0.75rem 0.75rem; border-top: 1px solid $color-border-subtle; }
  &--expanded &__body { display: block; }
  &__meta { display: flex; gap: 0.75rem; flex-wrap: wrap; font-size: 0.8rem; color: $color-text-muted; margin-bottom: 0.5rem; }
  &__desc { font-size: 0.85rem; color: $color-text-muted; line-height: 1.6; margin-bottom: 0.5rem; }

  &__actions { display: flex; gap: 0.5rem; justify-content: flex-end; }

  &__action {
    background: $color-surface; border: 1px solid $color-border-subtle;
    border-radius: $radius-sm; padding: 0.25rem 0.75rem; font-size: 0.8rem;
    cursor: pointer; color: $color-text;
    &:hover { background: $color-accent; color: #fff; }
    &--danger:hover { background: $color-danger; }
  }
}

.spell-add-btn {
  align-self: flex-start; background: transparent; border: 1px dashed $color-border-subtle;
  border-radius: $radius-md; padding: 0.5rem 1rem; font-size: 0.85rem;
  color: $color-text-muted; cursor: pointer;
  &:hover { border-color: $color-accent; color: $color-accent; }
}

.inv-modal :deep(.modal) { max-width: 560px; }

.add-spell-modal {
  display: flex; flex-direction: column; gap: 0.75rem;

  &__list { max-height: 350px; overflow-y: auto; display: flex; flex-direction: column; gap: 0.25rem; }

  &__item {
    display: flex; align-items: center; justify-content: space-between;
    padding: 0.5rem 0.75rem; border: 1px solid $color-border-subtle;
    border-radius: $radius-sm; cursor: pointer;
    &:hover { background: $color-bg-elevated; }
  }

  &__name { font-weight: 500; }
  &__info { display: flex; gap: 0.35rem; }
  &__loading, &__empty { text-align: center; color: $color-text-muted; padding: 1rem; font-size: 0.85rem; }
}
</style>
