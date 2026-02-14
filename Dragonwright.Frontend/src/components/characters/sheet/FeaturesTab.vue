<script setup lang="ts">
import {ref, computed} from "vue";
import {useToast} from "@/composables/useToast";
import {
  postCharactersIdFeats, deleteCharactersIdFeatsFeatId, putCharactersIdFeatsFeatId,
  getFeats, putCharactersIdClasses, putCharactersIdRace
} from "@/api";
import type {Feat, CharacterClassData} from "@/api";
import {featureTypeLabels, resetTypeLabels} from "@/content/enums";
import UiBadge from "@/components/ui/UiBadge.vue";
import UiModal from "@/components/ui/UiModal.vue";
import UiInput from "@/components/ui/UiInput.vue";

const character = defineModel<any>("character", {required: true});
const props = defineProps<{ characterId: string }>();
const emit = defineEmits<{ refresh: [] }>();

const {showToast} = useToast();

const raceTraits = computed(() => {
  const traits = character.value?.race?.race?.traits ?? [];
  return traits
    .filter((t: any) => !t.hideInCharacterSheet)
    .sort((a: any, b: any) => (Number(a.displayOrder) || 0) - (Number(b.displayOrder) || 0));
});

const classFeatures = computed(() => {
  const classes = character.value?.classes ?? [];
  const lvl = character.value?.level ?? 0;
  return classes.map((cc: any) => {
    const baseFeatures = (cc.class?.features ?? [])
      .filter((f: any) => !f.hideInCharacterSheet && Number(f.requiredCharacterLevel || 0) <= lvl);
    const subFeatures = (cc.subclass?.classFeatures ?? [])
      .filter((f: any) => !f.hideInCharacterSheet && Number(f.requiredCharacterLevel || 0) <= lvl);
    return {
      className: cc.class?.name ?? "Unknown",
      subclassName: cc.subclass?.name,
      classId: cc.classId,
      features: [...baseFeatures, ...subFeatures],
    };
  }).filter((e: any) => e.features.length > 0);
});

const backgroundFeats = computed(() => character.value?.background?.background?.grantedFeats ?? []);
const allCharacterFeats = computed(() => character.value?.feats ?? []);

const showAddFeatModal = ref(false);
const addFeatSearch = ref("");
const addFeatResults = ref<Feat[]>([]);
const addFeatLoading = ref(false);
let addFeatSearchTimer: ReturnType<typeof setTimeout> | null = null;

const expandedFeatures = ref<Set<string>>(new Set());

function toggleFeatureExpand(id: string) {
  if (expandedFeatures.value.has(id)) expandedFeatures.value.delete(id);
  else expandedFeatures.value.add(id);
}

const resetTypeShort: Record<number, string> = {
  0: "SR", 1: "LR", 2: "Dawn", 3: "None",
};

function getActionsWithReset(actions: any[]): any[] {
  return (actions ?? []).filter((a: any) => a.resetType != null && a.resetType !== 3);
}

function getClassFeatureUsage(classId: string, actionId: string): number {
  const cc = (character.value?.classes ?? []).find((c: any) => c.classId === classId);
  return Number(cc?.classFeatureUsages?.[actionId] ?? 0);
}

let classUsageTimer: ReturnType<typeof setTimeout> | null = null;

function adjustClassFeatureUsage(classId: string, actionId: string, delta: number) {
  const cc = (character.value?.classes ?? []).find((c: any) => c.classId === classId);
  if (!cc) return;
  if (!cc.classFeatureUsages) cc.classFeatureUsages = {};
  cc.classFeatureUsages[actionId] = Math.max(0, Number(cc.classFeatureUsages[actionId] ?? 0) + delta);
  if (classUsageTimer) clearTimeout(classUsageTimer);
  classUsageTimer = setTimeout(async () => {
    try {
      const classes: CharacterClassData[] = (character.value.classes ?? []).map((c: any) => ({
        id: c.id, classId: c.classId, level: c.level, subclassId: c.subclassId,
        isStartingClass: c.isStartingClass, classFeatureUsages: c.classFeatureUsages,
        chosenSkillProficiencies: c.chosenSkillProficiencies,
        chosenFeatureOptions: c.chosenFeatureOptions, chosenSpells: c.chosenSpells,
        spellSlotsUsed: c.spellSlotsUsed, pactSlotsUsed: c.pactSlotsUsed,
      }));
      await putCharactersIdClasses(props.characterId, {classes});
    } catch {
      showToast({variant: "danger", message: "Failed to save feature usage."});
    }
  }, 1000);
}

function getRaceTraitUsage(actionId: string): number {
  return Number(character.value?.race?.raceTraitUsages?.[actionId] ?? 0);
}

let raceUsageTimer: ReturnType<typeof setTimeout> | null = null;

function adjustRaceTraitUsage(actionId: string, delta: number) {
  const race = character.value?.race;
  if (!race) return;
  if (!race.raceTraitUsages) race.raceTraitUsages = {};
  race.raceTraitUsages[actionId] = Math.max(0, Number(race.raceTraitUsages[actionId] ?? 0) + delta);
  if (raceUsageTimer) clearTimeout(raceUsageTimer);
  raceUsageTimer = setTimeout(async () => {
    try {
      await putCharactersIdRace(props.characterId, {
        raceId: character.value.race.raceId,
        raceTraitUsages: character.value.race.raceTraitUsages ?? {},
        chosenTraitOptions: character.value.race.chosenTraitOptions ?? {},
        chosenSpells: character.value.race.chosenSpells ?? {},
      });
    } catch {
      showToast({variant: "danger", message: "Failed to save trait usage."});
    }
  }, 1000);
}

function getFeatUsage(charFeatId: string, actionId: string): number {
  const cf = (character.value?.feats ?? []).find((f: any) => f.id === charFeatId);
  return Number(cf?.featActionUsages?.[actionId] ?? 0);
}

let featUsageTimer: ReturnType<typeof setTimeout> | null = null;

function adjustFeatUsage(charFeatId: string, actionId: string, delta: number) {
  const cf = (character.value?.feats ?? []).find((f: any) => f.id === charFeatId);
  if (!cf) return;
  if (!cf.featActionUsages) cf.featActionUsages = {};
  cf.featActionUsages[actionId] = Math.max(0, Number(cf.featActionUsages[actionId] ?? 0) + delta);
  if (featUsageTimer) clearTimeout(featUsageTimer);
  featUsageTimer = setTimeout(async () => {
    try {
      await putCharactersIdFeatsFeatId(props.characterId, cf.id, {
        featId: cf.featId, source: cf.source, sourceId: cf.sourceId,
        chosenAbilityScoreIncrease: cf.chosenAbilityScoreIncrease,
        chosenOptions: cf.chosenOptions ?? {},
        chosenSpells: cf.chosenSpells ?? {},
        featActionUsages: cf.featActionUsages ?? {},
      });
    } catch {
      showToast({variant: "danger", message: "Failed to save feat usage."});
    }
  }, 1000);
}

function openAddFeatModal() {
  addFeatSearch.value = "";
  addFeatResults.value = [];
  showAddFeatModal.value = true;
  searchFeats("");
}

function onAddFeatSearchInput() {
  if (addFeatSearchTimer) clearTimeout(addFeatSearchTimer);
  addFeatSearchTimer = setTimeout(() => searchFeats(addFeatSearch.value), 300);
}

async function searchFeats(q: string) {
  addFeatLoading.value = true;
  try {
    const res = await getFeats({pageSize: 100, search: q || undefined});
    addFeatResults.value = ((res as any).data?.items ?? []) as Feat[];
  } catch {
    showToast({variant: "danger", message: "Failed to search feats."});
  } finally {
    addFeatLoading.value = false;
  }
}

async function addFeatToCharacter(feat: Feat) {
  try {
    await postCharactersIdFeats(props.characterId, {featId: feat.id, source: 0});
    showToast({message: `Added ${feat.name}.`});
    emit("refresh");
  } catch {
    showToast({variant: "danger", message: "Failed to add feat."});
  }
}

async function removeFeat(charFeat: any) {
  try {
    await deleteCharactersIdFeatsFeatId(props.characterId, charFeat.id);
    character.value.feats = character.value.feats.filter((f: any) => f.id !== charFeat.id);
    showToast({message: "Feat removed."});
  } catch {
    showToast({variant: "danger", message: "Failed to remove feat."});
  }
}
</script>

<template>
  <div v-if="raceTraits.length" class="feature-section">
    <div class="feature-section__title">Racial Traits</div>
    <div v-if="character.race?.race?.name" class="feature-section__subtitle">{{ character.race.race.name }}</div>
    <div v-for="trait in raceTraits" :key="trait.id" class="feature-item" :class="{ 'feature-item--expanded': expandedFeatures.has(trait.id) }">
      <div class="feature-item__header" @click="toggleFeatureExpand(trait.id)">
        <span class="feature-item__name">{{ trait.name }}</span>
        <div class="feature-item__badges">
          <template v-for="action in getActionsWithReset(trait.actions)" :key="action.id">
            <span class="usage-counter" @click.stop>
              <button class="usage-counter__btn" @click.stop="adjustRaceTraitUsage(action.id, -1)"><i class="fas fa-minus"/></button>
              <span class="usage-counter__value">{{ getRaceTraitUsage(action.id) }}</span>
              <button class="usage-counter__btn" @click.stop="adjustRaceTraitUsage(action.id, 1)"><i class="fas fa-plus"/></button>
              <UiBadge :label="resetTypeShort[action.resetType] ?? ''" variant="info"/>
            </span>
          </template>
          <UiBadge :label="featureTypeLabels[trait.featureType ?? 0] ?? 'Passive'" variant="muted"/>
        </div>
      </div>
      <div class="feature-item__desc" v-html="trait.description"/>
    </div>
  </div>

  <div v-for="entry in classFeatures" :key="entry.className" class="feature-section">
    <div class="feature-section__title">{{ entry.className }} Features</div>
    <div v-if="entry.subclassName" class="feature-section__subtitle">{{ entry.subclassName }}</div>
    <div v-for="feat in entry.features" :key="feat.id" class="feature-item" :class="{ 'feature-item--expanded': expandedFeatures.has(feat.id) }">
      <div class="feature-item__header" @click="toggleFeatureExpand(feat.id)">
        <span class="feature-item__name">{{ feat.name }}</span>
        <div class="feature-item__badges">
          <template v-for="action in getActionsWithReset(feat.actions)" :key="action.id">
            <span class="usage-counter" @click.stop>
              <button class="usage-counter__btn" @click.stop="adjustClassFeatureUsage(entry.classId, action.id, -1)"><i class="fas fa-minus"/></button>
              <span class="usage-counter__value">{{ getClassFeatureUsage(entry.classId, action.id) }}</span>
              <button class="usage-counter__btn" @click.stop="adjustClassFeatureUsage(entry.classId, action.id, 1)"><i class="fas fa-plus"/></button>
              <UiBadge :label="resetTypeShort[action.resetType] ?? ''" variant="info"/>
            </span>
          </template>
          <UiBadge v-if="feat.requiredCharacterLevel" :label="`Lv ${feat.requiredCharacterLevel}`" variant="info"/>
          <UiBadge :label="featureTypeLabels[feat.featureType ?? 0] ?? 'Passive'" variant="muted"/>
        </div>
      </div>
      <div class="feature-item__desc" v-html="feat.description"/>
    </div>
  </div>

  <div v-if="backgroundFeats.length" class="feature-section">
    <div class="feature-section__title">Background</div>
    <div v-if="character.background?.background?.name" class="feature-section__subtitle">{{ character.background.background.name }}</div>
    <div v-for="feat in backgroundFeats" :key="feat.id" class="feature-item" :class="{ 'feature-item--expanded': expandedFeatures.has(feat.id) }">
      <div class="feature-item__header" @click="toggleFeatureExpand(feat.id)">
        <span class="feature-item__name">{{ feat.name }}</span>
      </div>
      <div class="feature-item__desc" v-html="feat.description"/>
    </div>
  </div>

  <div class="feature-section">
    <div class="feature-section__title">
      Feats
      <button class="feat-add-btn--inline" @click="openAddFeatModal"><i class="fas fa-plus"/></button>
    </div>
    <template v-if="allCharacterFeats.length">
      <div v-for="cf in allCharacterFeats" :key="cf.id" class="feature-item" :class="{ 'feature-item--expanded': expandedFeatures.has(cf.id) }">
        <div class="feature-item__header" @click="toggleFeatureExpand(cf.id)">
          <span class="feature-item__name">{{ cf.feat?.name ?? 'Unknown' }}</span>
          <div class="feature-item__badges">
            <template v-for="action in getActionsWithReset(cf.feat?.actions)" :key="action.id">
              <span class="usage-counter" @click.stop>
                <button class="usage-counter__btn" @click.stop="adjustFeatUsage(cf.id, action.id, -1)"><i class="fas fa-minus"/></button>
                <span class="usage-counter__value">{{ getFeatUsage(cf.id, action.id) }}</span>
                <button class="usage-counter__btn" @click.stop="adjustFeatUsage(cf.id, action.id, 1)"><i class="fas fa-plus"/></button>
                <UiBadge :label="resetTypeShort[action.resetType] ?? ''" variant="info"/>
              </span>
            </template>
            <UiBadge v-if="cf.source === 0" label="Direct" variant="muted"/>
            <button v-if="cf.source === 0" class="feature-item__remove" @click.stop="removeFeat(cf)" title="Remove">
              <i class="fas fa-trash"/>
            </button>
          </div>
        </div>
        <div class="feature-item__desc" v-html="cf.feat?.description"/>
      </div>
    </template>
    <div v-else class="tabpanel__empty">No feats yet.</div>
    <button class="feat-add-btn" @click="openAddFeatModal"><i class="fas fa-plus"/> Add Feat</button>
  </div>

  <div v-if="!raceTraits.length && !classFeatures.length && !backgroundFeats.length && !allCharacterFeats.length" class="tabpanel__empty">
    No features or traits available.
  </div>

  <UiModal v-model="showAddFeatModal" title="Add Feat" :close-on-backdrop="true" :close-on-esc="true" class="inv-modal">
    <div class="inv-modal__search">
      <UiInput v-model="addFeatSearch" placeholder="Search feats..." @input="onAddFeatSearchInput"/>
    </div>
    <div v-if="addFeatLoading" class="inv-modal__loading"><i class="fas fa-spinner fa-spin"/> Searching...</div>
    <div v-else class="inv-modal__results">
      <button v-for="feat in addFeatResults" :key="feat.id" class="inv-modal__item" @click="addFeatToCharacter(feat)">
        <span class="inv-modal__item-name">{{ feat.name }}</span>
        <div class="inv-modal__item-tags">
          <UiBadge v-if="feat.featLevel" :label="`Lv ${feat.featLevel}`" variant="info"/>
        </div>
      </button>
      <div v-if="!addFeatResults.length" class="inv-modal__empty">No feats found.</div>
    </div>
  </UiModal>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.feature-section {
  margin-bottom: $space-4;
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.feature-section__title {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: $space-2;
}

.feature-section__subtitle {
  font-size: 0.82rem;
  color: $color-text-soft;
  margin-top: -$space-1;
}

.feature-item {
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  background: $color-surface-alt;
  overflow: hidden;
}

.feature-item__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
  padding: $space-2 $space-3;
  cursor: pointer;
  transition: background 120ms ease;

  &:hover { background: rgba(249, 115, 22, 0.04); }
}

.feature-item__name {
  font-weight: 500;
  font-size: 0.92rem;
  color: $color-text;
}

.feature-item__desc {
  display: none;
  padding: 0 $space-3 $space-3;
  color: $color-text-muted;
  font-size: 0.88rem;
  line-height: 1.6;
}

.feature-item--expanded .feature-item__desc { display: block; }

.feature-item__badges {
  display: flex;
  align-items: center;
  gap: $space-2;
  flex-shrink: 0;
}

.feature-item__remove {
  width: 1.5rem;
  height: 1.5rem;
  min-width: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.65rem;
  transition: all 150ms ease;

  &:hover { border-color: $color-danger; color: $color-danger; }
}

.usage-counter {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  background: $color-bg-elevated;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  padding: 0.1rem 0.35rem;
  font-size: 0.78rem;

  &__btn {
    width: 1.2rem;
    height: 1.2rem;
    display: flex;
    align-items: center;
    justify-content: center;
    background: transparent;
    border: none;
    color: $color-text-muted;
    cursor: pointer;
    padding: 0;
    font-size: 0.65rem;
    &:hover { color: $color-accent; }
  }

  &__value {
    min-width: 1.2rem;
    text-align: center;
    font-weight: 600;
    color: $color-accent;
  }
}

.feat-add-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: $space-2;
  width: 100%;
  padding: $space-3;
  margin-top: $space-2;
  background: transparent;
  border: 1px dashed $color-border-subtle;
  border-radius: $radius-md;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.875rem;
  transition: all 150ms ease;

  &:hover { border-color: $color-accent; color: $color-accent; }
}

.feat-add-btn--inline {
  width: 1.4rem;
  height: 1.4rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: 1px dashed $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.6rem;
  transition: all 150ms ease;

  &:hover { border-color: $color-accent; color: $color-accent; }
}

.tabpanel__empty {
  color: $color-text-muted;
  padding: $space-4;
  text-align: center;
}

.inv-modal :deep(.modal) { max-width: 560px; }
.inv-modal__search { margin-bottom: $space-3; }
.inv-modal__loading { text-align: center; padding: $space-4; color: $color-text-muted; }

.inv-modal__results {
  max-height: 320px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: $space-1;
}

.inv-modal__item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  cursor: pointer;
  color: $color-text;
  text-align: left;
  transition: all 150ms ease;

  &:hover { border-color: $color-accent; background: rgba($color-accent, 0.05); }
}

.inv-modal__item-name { font-weight: 500; font-size: 0.9rem; }
.inv-modal__item-tags { display: flex; gap: $space-1; flex-shrink: 0; }
.inv-modal__empty { text-align: center; padding: $space-4; color: $color-text-muted; }
</style>
