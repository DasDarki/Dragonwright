<script setup lang="ts">
import {computed, ref, toRef} from "vue";
import UiBadge from "@/components/ui/UiBadge.vue";
import {
  damageTypeLabels, weaponTypeLabels, weaponPropertyLabels, masteryLabels,
  abilityScoreLabels, resetTypeLabels, shapeLabels, spellLevelLabels, spellSchoolLabels,
  attackTypeLabels as attackLabels,
} from "@/content/enums";
import {useCharacterHelpers, abilityAbbrev} from "@/composables/useCharacterHelpers";

const props = defineProps<{ character: any }>();

const {signed, getAbilityModRaw} = useCharacterHelpers(toRef(props, "character"));

const expandedIds = ref<Set<string>>(new Set());

function toggleExpand(id: string) {
  if (expandedIds.value.has(id)) expandedIds.value.delete(id);
  else expandedIds.value.add(id);
}

const equippedWeapons = computed(() => {
  if (!props.character) return [];
  const prof = props.character.proficiencyBonus ?? 2;
  const strMod = getAbilityModRaw(0);
  const dexMod = getAbilityModRaw(1);
  const weapons: any[] = [];
  const items = (props.character.items ?? []).filter((ci: any) => (ci.equipped || ci.attuned) && ci.item?.type === 1);
  for (const ci of items) {
    const item = ci.item;
    const wType = Number(item.weaponType ?? 0);
    const itemProps: number[] = item.weaponProperties ?? [];
    const isRanged = wType === 1 || wType === 3;
    const isFinesse = itemProps.includes(1);
    let abilityMod: number;
    let abilityLabel: string;
    if (isFinesse) {
      abilityMod = Math.max(strMod, dexMod);
      abilityLabel = dexMod >= strMod ? "DEX" : "STR";
    } else if (isRanged) {
      abilityMod = dexMod;
      abilityLabel = "DEX";
    } else {
      abilityMod = strMod;
      abilityLabel = "STR";
    }
    const itemAttackBonus = Number(item.attackBonus ?? 0);
    const attackBonus = abilityMod + prof + itemAttackBonus;
    let damageMod = abilityMod;
    if (item.damageBonusAbility != null) {
      damageMod = getAbilityModRaw(Number(item.damageBonusAbility));
    }
    const damages: { dice: string; type: string }[] = [];
    for (const d of (item.damages ?? [])) {
      const dc = Number(d.diceCount ?? 0);
      const dv = Number(d.diceValue ?? 0);
      const bonus = Number(d.bonus ?? 0) + damageMod;
      const typeName = damageTypeLabels[Number(d.damageType ?? 0)] ?? "";
      let dice = "";
      if (dc > 0 && dv > 0) dice = `${dc}d${dv}`;
      if (bonus !== 0) dice += dice ? ` ${bonus >= 0 ? "+" : "-"} ${Math.abs(bonus)}` : `${bonus}`;
      damages.push({dice: dice || "0", type: typeName.toLowerCase()});
    }
    if (damages.length === 0) {
      damages.push({dice: `${1 + damageMod}`, type: "bludgeoning"});
    }
    weapons.push({
      id: ci.id, name: item.name, attackBonus, damages, properties: itemProps,
      mastery: item.mastery != null ? Number(item.mastery) : null,
      rangeInFeet: Number(item.rangeInFeet ?? 0),
      maximumRangeInFeet: Number(item.maximumRangeInFeet ?? 0),
      weaponType: wType, abilityUsed: abilityLabel,
    });
  }
  return weapons;
});

const unarmedStrike = computed(() => {
  const prof = props.character?.proficiencyBonus ?? 2;
  const strMod = getAbilityModRaw(0);
  return {
    id: "unarmed", name: "Unarmed Strike",
    attackBonus: strMod + prof,
    damages: [{dice: `1 ${strMod >= 0 ? "+" : "-"} ${Math.abs(strMod)}`, type: "bludgeoning"}],
    properties: [] as number[], mastery: null as number | null,
    rangeInFeet: 0, maximumRangeInFeet: 0,
    weaponType: null as number | null, abilityUsed: "STR",
  };
});

interface FeatureActionEntry {
  id: string;
  name: string;
  sourceName: string;
  sourceCategory: string;
  description: string;
  activationUnit: number | null;
  attackType: number | null;
  attackBonus: number | null;
  saveDC: number | null;
  saveAbility: string | null;
  diceCount: number;
  diceValue: number;
  fixedValue: number;
  damageType: number | null;
  range: number | null;
  maximumRange: number | null;
  areaOfEffect: number | null;
  areaSize: number;
  resetType: number | null;
}

const featureActions = computed<FeatureActionEntry[]>(() => {
  if (!props.character) return [];
  const lvl = props.character.level ?? 0;
  const prof = props.character.proficiencyBonus ?? 2;
  const result: FeatureActionEntry[] = [];

  function calcAttackBonus(action: any): number | null {
    if (action.attackType == null || Number(action.attackType) === 0) return null;
    const mod = action.abilityScore != null ? getAbilityModRaw(Number(action.abilityScore)) : 0;
    return mod + (action.isProficient ? prof : 0);
  }

  function calcSaveDC(action: any): number | null {
    if (action.save == null) return null;
    if (Number(action.fixedSaveDC ?? 0) > 0) return Number(action.fixedSaveDC);
    const mod = action.abilityScore != null ? getAbilityModRaw(Number(action.abilityScore)) : 0;
    return 8 + mod + prof;
  }

  function push(action: any, sourceName: string, sourceCategory: string) {
    if (Number(action.requiredCharacterLevel ?? 0) > lvl) return;
    result.push({
      id: action.id,
      name: action.name || sourceName,
      sourceName,
      sourceCategory,
      description: action.description ?? "",
      activationUnit: action.activationTime?.unit ?? null,
      attackType: action.attackType != null ? Number(action.attackType) : null,
      attackBonus: calcAttackBonus(action),
      saveDC: calcSaveDC(action),
      saveAbility: action.save != null ? (abilityAbbrev[Number(action.save)] ?? "") : null,
      diceCount: Number(action.diceCount ?? 0),
      diceValue: Number(action.diceValue ?? 0),
      fixedValue: Number(action.fixedValue ?? 0),
      damageType: action.damageType != null ? Number(action.damageType) : null,
      range: action.range != null ? Number(action.range) : null,
      maximumRange: action.maximumRange != null ? Number(action.maximumRange) : null,
      areaOfEffect: action.areaOfEffect != null ? Number(action.areaOfEffect) : null,
      areaSize: Number(action.areaSize ?? 0),
      resetType: action.resetType != null ? Number(action.resetType) : null,
    });
  }

  for (const trait of (props.character.race?.race?.traits ?? [])) {
    if (trait.hideInCharacterSheet) continue;
    for (const a of (trait.actions ?? [])) {
      push(a, trait.name, props.character.race?.race?.name ?? "Race");
    }
  }

  for (const cc of (props.character.classes ?? [])) {
    const className = cc.class?.name ?? "Unknown";
    for (const feat of [...(cc.class?.features ?? []), ...(cc.subclass?.classFeatures ?? [])]) {
      if (feat.hideInCharacterSheet) continue;
      if (Number(feat.requiredCharacterLevel || 0) > lvl) continue;
      for (const a of (feat.actions ?? [])) {
        push(a, feat.name, className);
      }
    }
  }

  for (const cf of (props.character.feats ?? [])) {
    for (const a of (cf.feat?.actions ?? [])) {
      push(a, cf.feat?.name ?? "Unknown", "Feat");
    }
  }

  return result;
});

const preparedSpells = computed(() => {
  if (!props.character) return [];
  return (props.character.spells ?? [])
    .filter((cs: any) => cs.isPrepared || cs.alwaysPrepared || (cs.spell?.level ?? 0) === 0);
});

function getSpellUnit(cs: any): number | null {
  const ct = cs.spell?.castingTimes?.[0];
  if (!ct) return null;
  return ct.unit ?? null;
}

type GroupKey = "action" | "bonus" | "reaction" | "other";

interface ActionGroup {
  label: string;
  icon: string;
  features: FeatureActionEntry[];
  spells: any[];
}

const groups = computed<Record<GroupKey, ActionGroup>>(() => {
  const g: Record<GroupKey, ActionGroup> = {
    action: {label: "Actions", icon: "fa-bolt", features: [], spells: []},
    bonus: {label: "Bonus Actions", icon: "fa-forward", features: [], spells: []},
    reaction: {label: "Reactions", icon: "fa-shield-halved", features: [], spells: []},
    other: {label: "Other", icon: "fa-ellipsis", features: [], spells: []},
  };

  for (const fa of featureActions.value) {
    const key: GroupKey = fa.activationUnit === 0 ? "action"
      : fa.activationUnit === 1 ? "bonus"
        : fa.activationUnit === 2 ? "reaction" : "other";
    g[key].features.push(fa);
  }

  for (const cs of preparedSpells.value) {
    const unit = getSpellUnit(cs);
    const key: GroupKey = unit === 0 ? "action"
      : unit === 1 ? "bonus"
        : unit === 2 ? "reaction" : "other";
    g[key].spells.push(cs);
  }

  return g;
});

const visibleGroupKeys = computed<GroupKey[]>(() => {
  const keys: GroupKey[] = ["action", "bonus", "reaction", "other"];
  return keys.filter(k => groups.value[k].features.length > 0 || groups.value[k].spells.length > 0);
});

function formatDamage(fa: FeatureActionEntry): string {
  if (fa.diceCount === 0 && fa.diceValue === 0 && fa.fixedValue === 0) return "";
  let dice = "";
  if (fa.diceCount > 0 && fa.diceValue > 0) dice = `${fa.diceCount}d${fa.diceValue}`;
  if (fa.fixedValue !== 0) dice += dice ? ` ${fa.fixedValue >= 0 ? "+" : "-"} ${Math.abs(fa.fixedValue)}` : `${fa.fixedValue}`;
  const type = fa.damageType != null ? ` ${(damageTypeLabels[fa.damageType] ?? "").toLowerCase()}` : "";
  return (dice || "0") + type;
}

function formatRange(range: number | null): string {
  if (range == null) return "";
  if (range === 0) return "Self";
  if (range === -1) return "Touch";
  if (range === -2) return "Unlimited";
  return `${range} ft.`;
}

function formatSpellCastingTime(spell: any): string {
  const ct = spell?.castingTimes?.[0];
  if (!ct) return "";
  const val = Number(ct.value ?? 1);
  const unit = Number(ct.unit ?? 0);
  const labels: Record<number, string> = {0: "Action", 1: "Bonus Action", 2: "Reaction", 3: "Round", 4: "Minute", 5: "Hour", 6: "Day"};
  if (val === 1 && labels[unit]) return `1 ${labels[unit]}`;
  return `${val} ${labels[unit] ?? ""}`.trim();
}

function formatSpellRange(spell: any): string {
  const range = Number(spell?.range ?? 0);
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
  <div class="actions-tab">
    <div class="section">
      <div class="section__title">Weapon Attacks</div>
      <div class="actions-list">
        <div v-for="w in equippedWeapons" :key="w.id" class="weapon-card">
          <div class="weapon-card__header">
            <span class="weapon-card__name">{{ w.name }}</span>
            <span class="weapon-card__atk">{{ signed(w.attackBonus) }}</span>
          </div>
          <div class="weapon-card__body">
            <div class="weapon-card__damages">
              <span v-for="(d, i) in w.damages" :key="i" class="weapon-card__damage">
                {{ d.dice }} {{ d.type }}
              </span>
            </div>
            <div class="weapon-card__meta">
              <UiBadge :label="w.abilityUsed" variant="muted"/>
              <UiBadge v-if="w.weaponType != null" :label="weaponTypeLabels[w.weaponType] ?? ''" variant="muted"/>
              <UiBadge v-for="p in w.properties" :key="p" :label="weaponPropertyLabels[p] ?? ''" variant="info"/>
              <UiBadge v-if="w.mastery != null" :label="masteryLabels[w.mastery] ?? ''" variant="accent"/>
              <span v-if="w.rangeInFeet > 0" class="weapon-card__range">
                {{ w.rangeInFeet }}<template v-if="w.maximumRangeInFeet > w.rangeInFeet">/{{ w.maximumRangeInFeet }}</template> ft
              </span>
            </div>
          </div>
        </div>

        <div class="weapon-card weapon-card--unarmed">
          <div class="weapon-card__header">
            <span class="weapon-card__name">{{ unarmedStrike.name }}</span>
            <span class="weapon-card__atk">{{ signed(unarmedStrike.attackBonus) }}</span>
          </div>
          <div class="weapon-card__body">
            <div class="weapon-card__damages">
              <span v-for="(d, i) in unarmedStrike.damages" :key="i" class="weapon-card__damage">
                {{ d.dice }} {{ d.type }}
              </span>
            </div>
            <div class="weapon-card__meta">
              <UiBadge label="STR" variant="muted"/>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!equippedWeapons.length" class="actions-tab__hint">
        Equip weapons in the Inventory tab to see them here.
      </div>
    </div>

    <template v-for="key in visibleGroupKeys" :key="key">
      <div class="section">
        <div class="section__title">
          <i class="fas" :class="groups[key].icon"/>
          {{ groups[key].label }}
        </div>

        <div class="actions-list">
          <div
            v-for="fa in groups[key].features" :key="fa.id"
            class="action-card"
            :class="{ 'action-card--expanded': expandedIds.has(fa.id) }"
          >
            <div class="action-card__header" @click="toggleExpand(fa.id)">
              <div class="action-card__title">
                <span class="action-card__name">{{ fa.name }}</span>
                <UiBadge :label="fa.sourceCategory" variant="muted"/>
              </div>
              <div class="action-card__stats">
                <span v-if="fa.attackBonus != null" class="action-card__atk">{{ signed(fa.attackBonus) }}</span>
                <span v-if="fa.saveDC != null" class="action-card__save">DC {{ fa.saveDC }} {{ fa.saveAbility }}</span>
              </div>
            </div>
            <div class="action-card__body">
              <div class="action-card__info">
                <span v-if="formatDamage(fa)" class="action-card__damage">{{ formatDamage(fa) }}</span>
                <span v-if="fa.attackType != null && fa.attackType > 0" class="action-card__tag">{{ attackLabels[fa.attackType] ?? '' }}</span>
                <span v-if="fa.range != null" class="action-card__tag">
                  {{ formatRange(fa.range) }}<template v-if="fa.maximumRange != null && fa.maximumRange > (fa.range ?? 0)">/{{ fa.maximumRange }} ft.</template>
                </span>
                <span v-if="fa.areaOfEffect != null" class="action-card__tag">
                  {{ fa.areaSize }} ft. {{ shapeLabels[fa.areaOfEffect] ?? '' }}
                </span>
                <UiBadge v-if="fa.resetType != null && fa.resetType !== 3" :label="resetTypeLabels[fa.resetType] ?? ''" variant="info"/>
              </div>
              <div v-if="fa.description" class="action-card__desc" v-html="fa.description"/>
            </div>
          </div>

          <div
            v-for="cs in groups[key].spells" :key="cs.id"
            class="spell-action-card"
            :class="{ 'spell-action-card--expanded': expandedIds.has('spell-' + cs.id) }"
          >
            <div class="spell-action-card__header" @click="toggleExpand('spell-' + cs.id)">
              <div class="spell-action-card__title">
                <span class="spell-action-card__name">{{ cs.spell?.name }}</span>
                <div class="spell-action-card__badges">
                  <UiBadge :label="spellLevelLabels[cs.spell?.level ?? 0]" variant="muted"/>
                  <UiBadge v-if="cs.spell?.concentration" label="C" variant="info"/>
                  <UiBadge v-if="cs.spell?.ritual" label="R" variant="info"/>
                </div>
              </div>
              <div class="spell-action-card__meta">
                <span>{{ formatSpellCastingTime(cs.spell) }}</span>
                <span>{{ formatSpellRange(cs.spell) }}</span>
                <span>{{ formatComponents(cs.spell) }}</span>
              </div>
            </div>
            <div class="spell-action-card__body">
              <div class="spell-action-card__info">
                <span>{{ spellSchoolLabels[cs.spell?.school ?? 0] }}</span>
                <UiBadge v-if="cs.alwaysPrepared" label="Always Prepared" variant="accent"/>
              </div>
              <div v-if="cs.spell?.description" class="spell-action-card__desc" v-html="cs.spell.description"/>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.actions-tab {
  display: flex;
  flex-direction: column;
  gap: $space-4;
}

.section {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.section__title {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: $space-2;
}

.actions-list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.actions-tab__hint {
  font-size: 0.82rem;
  color: $color-text-muted;
  padding: $space-2 0;
}

.weapon-card {
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  background: $color-surface-alt;
  overflow: hidden;

  &--unarmed { border-style: dashed; opacity: 0.85; }
  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: $space-2 $space-3;
    border-bottom: 1px solid $color-border-subtle;
  }
  &__name { font-weight: 600; font-size: 0.92rem; }
  &__atk { font-weight: 700; font-size: 1rem; color: $color-accent; }
  &__body { padding: $space-2 $space-3; display: flex; flex-direction: column; gap: $space-2; }
  &__damages { display: flex; gap: $space-3; flex-wrap: wrap; }
  &__damage { font-size: 0.88rem; font-weight: 500; }
  &__meta { display: flex; align-items: center; gap: $space-1; flex-wrap: wrap; }
  &__range { font-size: 0.8rem; color: $color-text-muted; }
}

.action-card {
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  background: $color-surface-alt;
  overflow: hidden;

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: $space-2;
    padding: $space-2 $space-3;
    cursor: pointer;
    transition: background 120ms ease;

    &:hover { background: rgba(249, 115, 22, 0.04); }
  }

  &__title {
    display: flex;
    align-items: center;
    gap: $space-2;
    min-width: 0;
  }

  &__name {
    font-weight: 600;
    font-size: 0.92rem;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  &__stats {
    display: flex;
    align-items: center;
    gap: $space-2;
    flex-shrink: 0;
  }

  &__atk {
    font-weight: 700;
    font-size: 1rem;
    color: $color-accent;
  }

  &__save {
    font-weight: 600;
    font-size: 0.88rem;
    color: $color-text-soft;
  }

  &__body {
    display: none;
    padding: $space-2 $space-3 $space-3;
    border-top: 1px solid $color-border-subtle;
  }

  &--expanded &__body { display: block; }

  &__info {
    display: flex;
    align-items: center;
    gap: $space-2;
    flex-wrap: wrap;
    margin-bottom: $space-2;
  }

  &__damage {
    font-size: 0.88rem;
    font-weight: 500;
  }

  &__tag {
    font-size: 0.8rem;
    color: $color-text-muted;
  }

  &__desc {
    font-size: 0.85rem;
    color: $color-text-muted;
    line-height: 1.6;
  }
}

.spell-action-card {
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  background: $color-surface-alt;
  overflow: hidden;
  border-left: 3px solid rgba(99, 102, 241, 0.5);

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: $space-2;
    padding: $space-2 $space-3;
    cursor: pointer;
    transition: background 120ms ease;

    &:hover { background: rgba(99, 102, 241, 0.04); }
  }

  &__title {
    display: flex;
    align-items: center;
    gap: $space-2;
    min-width: 0;
  }

  &__name {
    font-weight: 500;
    font-size: 0.92rem;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  &__badges {
    display: flex;
    gap: 0.35rem;
    flex-shrink: 0;
  }

  &__meta {
    display: flex;
    gap: $space-2;
    font-size: 0.78rem;
    color: $color-text-muted;
    flex-shrink: 0;
  }

  &__body {
    display: none;
    padding: $space-2 $space-3 $space-3;
    border-top: 1px solid $color-border-subtle;
  }

  &--expanded &__body { display: block; }

  &__info {
    display: flex;
    align-items: center;
    gap: $space-2;
    flex-wrap: wrap;
    margin-bottom: $space-2;
    font-size: 0.82rem;
    color: $color-text-muted;
  }

  &__desc {
    font-size: 0.85rem;
    color: $color-text-muted;
    line-height: 1.6;
  }
}
</style>
