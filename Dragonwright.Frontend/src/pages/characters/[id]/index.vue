<script setup lang="ts">
import {ref, computed, onMounted, watch} from "vue";
import {useRoute, useRouter} from "vue-router";
import {useTitle} from "@/composables/useTitle";
import {useToast} from "@/composables/useToast";
import {
  getCharactersId, getItems, putCharactersId,
  postCharactersIdItems, putCharactersIdItemsItemId, deleteCharactersIdItemsItemId,
  putCharactersIdAbilities, putCharactersIdSkills,
  postCharactersIdFeats, deleteCharactersIdFeatsFeatId, putCharactersIdFeatsFeatId,
  getFeats,
  getSpells, postCharactersIdSpells, putCharactersIdSpellsSpellId, deleteCharactersIdSpellsSpellId,
  putCharactersIdClasses, putCharactersIdRace
} from "@/api";
import type {Item, StartItemChoice, Feat, CharacterAbilityData, CharacterSkillData, CharacterClassData} from "@/api";
import {
  abilityScoreLabels, skillLabels, alignmentLabels, sizeLabels, sourceLabels,
  itemTypeLabels, rarityLabels, weaponTypeLabels, weaponPropertyLabels, currencyLabels,
  toolLabels, conditionLabels, damageTypeLabels, defenseStateLabels, advantageStateLabels,
  featureTypeLabels, spellLevelLabels, spellSchoolLabels, spellSourceLabels,
  masteryLabels, resetTypeLabels
} from "@/content/enums";
import UiButton from "@/components/ui/UiButton.vue";
import UiCard from "@/components/ui/UiCard.vue";
import UiGrid from "@/components/ui/layout/UiGrid.vue";
import UiBadge from "@/components/ui/UiBadge.vue";
import UiModal from "@/components/ui/UiModal.vue";
import UiInput from "@/components/ui/UiInput.vue";
import Avatar from "@/components/characters/Avatar.vue";

const route = useRoute();
const router = useRouter();
const {showToast} = useToast();

const character = ref<any>(null);
const loading = ref(true);

const characterId = computed(() => route.params.id as string);

const pageTitle = computed(() => character.value?.name ?? "Character");
watch(pageTitle, (title) => useTitle(title), {immediate: true});

type Tab = "actions" | "spells" | "inventory" | "features" | "background" | "notes";
const activeTab = ref<Tab>("actions");

async function fetchCharacter() {
  loading.value = true;
  try {
    const res = await getCharactersId(characterId.value);
    character.value = (res as any).data;
  } catch {
    showToast({variant: "danger", message: "Failed to load character."});
  } finally {
    loading.value = false;
  }
}

onMounted(fetchCharacter);

function goToEdit() {
  router.push(`/characters/${characterId.value}/edit/configuration`);
}

function goBack() {
  router.push("/characters");
}

function signed(n: number) {
  return n >= 0 ? `+${n}` : `${n}`;
}

function getAbilityMod(score: number): string {
  const mod = Math.floor((score - 10) / 2);
  return mod >= 0 ? `+${mod}` : `${mod}`;
}

function getAbilityModRaw(ability: number): number {
  const score = getAbilityScore(ability)
  return Math.floor((score - 10) / 2)
}

function getClassSummary(): string {
  if (!character.value?.classes?.length) return "No class";
  return character.value.classes.map((c: any) => `${c.class?.name ?? "Unknown"} ${c.level}`).join(" / ");
}

function getRaceName(): string {
  return character.value?.race?.race?.name ?? "No race";
}

function getBackgroundName(): string {
  return character.value?.background?.background?.name ?? "No background";
}

function getAbilityScore(ability: number): number {
  const ab = character.value?.abilities?.find((a: any) => a.ability === ability);
  return ab?.score ?? 10;
}

function getSavingThrow(ability: number): string {
  const ab = character.value?.abilities?.find((a: any) => a.ability === ability);
  if (!ab) return "+0";
  const mod = ab.savingThrow;
  return mod >= 0 ? `+${mod}` : `${mod}`;
}

type ProfLevel = "none" | "half" | "prof" | "expert";

function getSaveProficiencyLevel(ability: number): ProfLevel {
  const p = character.value?.abilities?.find((a: any) => a.ability === ability)?.savingThrowProficiency ?? 0;
  if (p >= 3) return "expert";
  if (p === 2) return "prof";
  if (p === 1) return "half";
  return "none";
}

function getSkillBonus(skill: number): string {
  const sk = character.value?.skills?.find((s: any) => s.skill === skill);
  if (!sk) return "+0";
  const mod = sk.total;
  return mod >= 0 ? `+${mod}` : `${mod}`;
}

function getSkillProficiencyLevel(skill: number): ProfLevel {
  const p = character.value?.skills?.find((s: any) => s.skill === skill)?.proficiency ?? 0;
  if (p >= 3) return "expert";
  if (p === 2) return "prof";
  if (p === 1) return "half";
  return "none";
}

const skillToAbility: Record<number, number> = {
  0: 1, 1: 4, 2: 3, 3: 0, 4: 5, 5: 3, 6: 4, 7: 5,
  8: 3, 9: 4, 10: 3, 11: 4, 12: 5, 13: 5, 14: 3, 15: 1, 16: 1, 17: 4,
};

const abilityAbbrev: Record<number, string> = {
  0: "STR", 1: "DEX", 2: "CON", 3: "INT", 4: "WIS", 5: "CHA",
};

const skillList = Object.entries(skillLabels).map(([k, v]) => ({id: Number(k), label: v}));

function getSkillAbility(skillId: number): string {
  const abilityId = skillToAbility[skillId] ?? 0;
  return abilityAbbrev[abilityId] ?? "";
}

const defenseList = computed(() => {
  const c = character.value;
  if (!c) return [];
  const dmg = c.damageDefenses ? Object.keys(c.damageDefenses) : [];
  const cond = c.conditionDefenses ? Object.keys(c.conditionDefenses) : [];
  return [...dmg, ...cond].filter(Boolean);
});

const hasAnySenses = computed(() => {
  const c = character.value;
  if (!c) return false;
  return (c.darkvisionRange ?? 0) > 0 || (c.blindsightRange ?? 0) > 0 || (c.tremorsenseRange ?? 0) > 0 || (c.truesightRange ?? 0) > 0;
});

const editGp = ref(0);
const editSp = ref(0);
const editCp = ref(0);
const editEp = ref(0);
let currencyTimer: ReturnType<typeof setTimeout> | null = null;

const totalGpValue = computed(() => {
  return editGp.value + editSp.value / 10 + editCp.value / 100 + editEp.value / 2;
});

function syncCurrencyFromCharacter() {
  const c = character.value;
  if (!c) return;
  editGp.value = c.gold ?? 0;
  editSp.value = c.silver ?? 0;
  editCp.value = c.copper ?? 0;
  editEp.value = c.electrum ?? 0;
}

function adjustCurrency(field: "gp" | "sp" | "cp" | "ep", delta: number) {
  const refs = {gp: editGp, sp: editSp, cp: editCp, ep: editEp};
  refs[field].value = Math.max(0, refs[field].value + delta);
  debouncedSaveCurrency();
}

function debouncedSaveCurrency() {
  if (currencyTimer) clearTimeout(currencyTimer);
  currencyTimer = setTimeout(saveCurrency, 1000);
}

async function saveCurrency() {
  if (!character.value) return;
  try {
    await putCharactersId(characterId.value, {
      name: character.value.name,
      gold: editGp.value,
      silver: editSp.value,
      copper: editCp.value,
      electrum: editEp.value,
    });
    character.value.gold = editGp.value;
    character.value.silver = editSp.value;
    character.value.copper = editCp.value;
    character.value.electrum = editEp.value;
  } catch {
    showToast({variant: "danger", message: "Failed to save currency."});
  }
}

const equippedItems = computed(() => (character.value?.items ?? []).filter((i: any) => i.equipped && !i.attuned));
const attunedItems = computed(() => (character.value?.items ?? []).filter((i: any) => i.attuned));
const otherItems = computed(() => (character.value?.items ?? []).filter((i: any) => !i.equipped && !i.attuned));
const attunedCount = computed(() => (character.value?.items ?? []).filter((i: any) => i.attuned).length);
const hasItems = computed(() => (character.value?.items?.length ?? 0) > 0);

const equippedWeapons = computed(() => {
  if (!character.value) return []
  const prof = character.value.proficiencyBonus ?? 2
  const strMod = getAbilityModRaw(0)
  const dexMod = getAbilityModRaw(1)
  const weapons: any[] = []
  const items = (character.value.items ?? []).filter((ci: any) => (ci.equipped || ci.attuned) && ci.item?.type === 1)
  for (const ci of items) {
    const item = ci.item
    const wType = Number(item.weaponType ?? 0)
    const props: number[] = item.weaponProperties ?? []
    const isRanged = wType === 1 || wType === 3
    const isFinesse = props.includes(1)
    let abilityMod: number
    let abilityLabel: string
    if (isFinesse) {
      abilityMod = Math.max(strMod, dexMod)
      abilityLabel = dexMod >= strMod ? 'DEX' : 'STR'
    } else if (isRanged) {
      abilityMod = dexMod
      abilityLabel = 'DEX'
    } else {
      abilityMod = strMod
      abilityLabel = 'STR'
    }
    const itemAttackBonus = Number(item.attackBonus ?? 0)
    const attackBonus = abilityMod + prof + itemAttackBonus
    let damageMod = abilityMod
    if (item.damageBonusAbility != null) {
      damageMod = getAbilityModRaw(Number(item.damageBonusAbility))
    }
    const damages: { dice: string; type: string }[] = []
    for (const d of (item.damages ?? [])) {
      const dc = Number(d.diceCount ?? 0)
      const dv = Number(d.diceValue ?? 0)
      const bonus = Number(d.bonus ?? 0) + damageMod
      const typeName = damageTypeLabels[Number(d.damageType ?? 0)] ?? ''
      let dice = ''
      if (dc > 0 && dv > 0) dice = `${dc}d${dv}`
      if (bonus !== 0) dice += dice ? ` ${bonus >= 0 ? '+' : '-'} ${Math.abs(bonus)}` : `${bonus}`
      damages.push({ dice: dice || '0', type: typeName.toLowerCase() })
    }
    if (damages.length === 0) {
      damages.push({ dice: `${1 + damageMod}`, type: 'bludgeoning' })
    }
    weapons.push({
      id: ci.id, name: item.name, attackBonus, damages, properties: props,
      mastery: item.mastery != null ? Number(item.mastery) : null,
      rangeInFeet: Number(item.rangeInFeet ?? 0),
      maximumRangeInFeet: Number(item.maximumRangeInFeet ?? 0),
      weaponType: wType, abilityUsed: abilityLabel,
    })
  }
  return weapons
})

const unarmedStrike = computed(() => {
  const prof = character.value?.proficiencyBonus ?? 2
  const strMod = getAbilityModRaw(0)
  return {
    id: 'unarmed', name: 'Unarmed Strike',
    attackBonus: strMod + prof,
    damages: [{ dice: `1 ${strMod >= 0 ? '+' : '-'} ${Math.abs(strMod)}`, type: 'bludgeoning' }],
    properties: [] as number[], mastery: null as number | null,
    rangeInFeet: 0, maximumRangeInFeet: 0,
    weaponType: null as number | null, abilityUsed: 'STR',
  }
})

const startingClass = computed(() => {
  return character.value?.classes?.find((c: any) => c.isStartingClass);
});

const hasStartingEquipment = computed(() => {
  const classItems = startingClass.value?.class?.startingItems;
  const bgItems = character.value?.background?.background?.startingItems;
  return (classItems?.length ?? 0) > 0 || (bgItems?.length ?? 0) > 0;
});

const showAddItemModal = ref(false);
const addItemSearch = ref("");
const addItemResults = ref<Item[]>([]);
const addItemLoading = ref(false);
const addItemQuantity = ref(1);
let addSearchTimer: ReturnType<typeof setTimeout> | null = null;

const showEditItemModal = ref(false);
const editingItem = ref<any>(null);
const editQuantity = ref(1);
const editNotes = ref("");
const editEquipped = ref(false);
const editAttuned = ref(false);
const editMaxCharges = ref(0);

const showStartingEquipModal = ref(false);
const startingEquipSelections = ref<Record<string, string>>({});
const startingEquipWeaponSelections = ref<Record<string, string>>({});
const allItemsCache = ref<Item[]>([]);
const startingEquipLoading = ref(false);

function getItemGroup(group: "equipped" | "attuned" | "other") {
  if (group === "equipped") return equippedItems.value;
  if (group === "attuned") return attunedItems.value;
  return otherItems.value;
}

async function toggleEquip(charItem: any) {
  try {
    await putCharactersIdItemsItemId(characterId.value, charItem.id, {
      quantity: charItem.quantity,
      notes: charItem.notes,
      attuned: charItem.attuned,
      equipped: !charItem.equipped,
      maxCharges: charItem.maxCharges ?? 0,
      chargesUsed: charItem.chargesUsed ?? 0,
    });
    charItem.equipped = !charItem.equipped;
  } catch {
    showToast({variant: "danger", message: "Failed to update item."});
  }
}

async function toggleAttune(charItem: any) {
  try {
    await putCharactersIdItemsItemId(characterId.value, charItem.id, {
      quantity: charItem.quantity,
      notes: charItem.notes,
      attuned: !charItem.attuned,
      equipped: charItem.equipped,
      maxCharges: charItem.maxCharges ?? 0,
      chargesUsed: charItem.chargesUsed ?? 0,
    });
    charItem.attuned = !charItem.attuned;
  } catch {
    showToast({variant: "danger", message: "Failed to update item."});
  }
}

function openEditItem(charItem: any) {
  editingItem.value = charItem;
  editQuantity.value = charItem.quantity ?? 1;
  editNotes.value = charItem.notes ?? "";
  editEquipped.value = charItem.equipped ?? false;
  editAttuned.value = charItem.attuned ?? false;
  editMaxCharges.value = charItem.maxCharges ?? 0;
  showEditItemModal.value = true;
}

async function saveEditItem() {
  if (!editingItem.value) return;
  try {
    const clampedCharges = Math.min(editingItem.value.chargesUsed ?? 0, editMaxCharges.value)
    await putCharactersIdItemsItemId(characterId.value, editingItem.value.id, {
      quantity: editQuantity.value,
      notes: editNotes.value,
      attuned: editAttuned.value,
      equipped: editEquipped.value,
      maxCharges: editMaxCharges.value,
      chargesUsed: clampedCharges,
    });
    editingItem.value.quantity = editQuantity.value;
    editingItem.value.notes = editNotes.value;
    editingItem.value.equipped = editEquipped.value;
    editingItem.value.attuned = editAttuned.value;
    editingItem.value.maxCharges = editMaxCharges.value;
    editingItem.value.chargesUsed = clampedCharges;
    showEditItemModal.value = false;
    showToast({message: "Item updated."});
  } catch {
    showToast({variant: "danger", message: "Failed to update item."});
  }
}

async function removeItem(charItem: any) {
  try {
    await deleteCharactersIdItemsItemId(characterId.value, charItem.id);
    character.value.items = character.value.items.filter((i: any) => i.id !== charItem.id);
    if (showEditItemModal.value) showEditItemModal.value = false;
    showToast({message: "Item removed."});
  } catch {
    showToast({variant: "danger", message: "Failed to remove item."});
  }
}

function openAddItemModal() {
  addItemSearch.value = "";
  addItemResults.value = [];
  addItemQuantity.value = 1;
  showAddItemModal.value = true;
  searchAllItems("");
}

function onAddItemSearchInput() {
  if (addSearchTimer) clearTimeout(addSearchTimer);
  addSearchTimer = setTimeout(() => searchAllItems(addItemSearch.value), 300);
}

async function searchAllItems(q: string) {
  addItemLoading.value = true;
  try {
    const res = await getItems({pageSize: 200, search: q || undefined});
    addItemResults.value = ((res as any).data?.items ?? []) as Item[];
  } catch {
    showToast({variant: "danger", message: "Failed to search items."});
  } finally {
    addItemLoading.value = false;
  }
}

async function addItemToCharacter(item: Item) {
  try {
    await postCharactersIdItems(characterId.value, {
      itemId: item.id,
      quantity: addItemQuantity.value,
      attuned: false,
      equipped: false,
    });
    showToast({message: `Added ${item.name}.`});
    addItemQuantity.value = 1;
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to add item."});
  }
}

async function openStartingEquipModal() {
  startingEquipLoading.value = true;
  showStartingEquipModal.value = true;
  startingEquipSelections.value = {};
  startingEquipWeaponSelections.value = {};
  try {
    const res = await getItems({pageSize: 1000});
    allItemsCache.value = ((res as any).data?.items ?? []) as Item[];
  } catch {
    showToast({variant: "danger", message: "Failed to load items."});
  } finally {
    startingEquipLoading.value = false;
  }
}

function getStartingItemChoices(): { label: string; choices: StartItemChoice[] }[] {
  const groups: { label: string; choices: StartItemChoice[] }[] = [];
  const classItems = startingClass.value?.class?.startingItems;
  if (classItems?.length) groups.push({
    label: `${startingClass.value.class.name} Starting Equipment`,
    choices: classItems
  });
  const bgItems = character.value?.background?.background?.startingItems;
  if (bgItems?.length) groups.push({
    label: `${character.value.background.background.name} Starting Equipment`,
    choices: bgItems
  });
  return groups;
}

function getItemsForWeaponTypes(types: number[]): Item[] {
  return allItemsCache.value.filter(i => types.includes(i.weaponType as number));
}

function getItemsForWeaponProperties(props: number[]): Item[] {
  return allItemsCache.value.filter(i => (i.weaponProperties ?? []).some((p: number) => props.includes(p)));
}

function getStartItemLabel(si: any): string {
  if (si.currency != null && si.currencyAmount) {
    return `${si.currencyAmount} ${currencyLabels[si.currency] ?? ""}`;
  }
  if (si.type === 1) return `Choose a ${si.weaponTypes?.map((t: number) => weaponTypeLabels[t]).join("/") ?? ""} weapon`;
  if (si.type === 2) return `Choose a weapon with ${si.weaponProperties?.map((p: number) => weaponPropertyLabels[p]).join("/") ?? ""}`;
  return "Item";
}

function needsSelection(si: any): boolean {
  return si.type === 1 || si.type === 2;
}

async function confirmStartingEquipment() {
  startingEquipLoading.value = true;
  try {
    const groups = getStartingItemChoices();
    let addGp = 0, addSp = 0, addCp = 0, addEp = 0;

    for (const group of groups) {
      for (const choice of group.choices) {
        const isOr = choice.operator === 1;
        const selectedId = startingEquipSelections.value[choice.id!];
        const items = choice.items ?? [];

        for (const si of items) {
          if (isOr && si.id !== selectedId) continue;

          if (si.currency != null && si.currencyAmount) {
            const amount = Number(si.currencyAmount) || 0;
            if (si.currency === 3) addGp += amount;
            else if (si.currency === 1) addSp += amount;
            else if (si.currency === 0) addCp += amount;
            else if (si.currency === 2) addEp += amount;
            continue;
          }

          if (si.type === 1 || si.type === 2) {
            const weaponId = startingEquipWeaponSelections.value[si.id!];
            if (weaponId) {
              await postCharactersIdItems(characterId.value, {
                itemId: weaponId,
                quantity: 1,
                attuned: false,
                equipped: false
              });
            }
          }
        }
      }
    }

    if (addGp || addSp || addCp || addEp) {
      editGp.value += addGp;
      editSp.value += addSp;
      editCp.value += addCp;
      editEp.value += addEp;
      await saveCurrency();
    }

    showStartingEquipModal.value = false;
    showToast({message: "Starting equipment added."});
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to add starting equipment."});
  } finally {
    startingEquipLoading.value = false;
  }
}

const notesForm = ref({
  personalityTraits: [] as string[],
  ideals: [] as string[],
  bonds: [] as string[],
  flaws: [] as string[],
  backstory: "",
  notes: "",
  appearance: "",
  skin: "",
  hair: "",
  eyes: "",
  faith: "",
  heightInInches: 0,
  weightInPounds: 0,
});

const heightUnit = ref<"ft" | "m">("ft");
const weightUnit = ref<"lb" | "kg">("lb");
let notesTimer: ReturnType<typeof setTimeout> | null = null;
const notesSaving = ref(false);

const displayHeightCm = computed({
  get: () => Math.round(notesForm.value.heightInInches * 2.54),
  set: (cm: number) => {
    notesForm.value.heightInInches = Math.round(cm / 2.54);
  },
});

const displayWeightKg = computed({
  get: () => +(notesForm.value.weightInPounds * 0.453592).toFixed(1),
  set: (kg: number) => {
    notesForm.value.weightInPounds = Math.round(kg / 0.453592);
  },
});

const heightFeet = computed({
  get: () => Math.floor(notesForm.value.heightInInches / 12),
  set: (v: number) => {
    notesForm.value.heightInInches = v * 12 + (notesForm.value.heightInInches % 12);
  },
});

const heightInches = computed({
  get: () => notesForm.value.heightInInches % 12,
  set: (v: number) => {
    notesForm.value.heightInInches = heightFeet.value * 12 + v;
  },
});

function syncNotesFromCharacter() {
  if (!character.value) return;
  const c = character.value;
  notesForm.value = {
    personalityTraits: [...(c.personalityTraits ?? [])],
    ideals: [...(c.ideals ?? [])],
    bonds: [...(c.bonds ?? [])],
    flaws: [...(c.flaws ?? [])],
    backstory: c.backstory ?? "",
    notes: c.notes ?? "",
    appearance: c.appearance ?? "",
    skin: c.skin ?? "",
    hair: c.hair ?? "",
    eyes: c.eyes ?? "",
    faith: c.faith ?? "",
    heightInInches: c.heightInInches ?? 0,
    weightInPounds: c.weightInPounds ?? 0,
  };
}

function debouncedSaveNotes() {
  if (notesTimer) clearTimeout(notesTimer);
  notesTimer = setTimeout(saveNotesForm, 1000);
}

async function saveNotesForm() {
  if (!character.value) return;
  notesSaving.value = true;
  try {
    const f = notesForm.value;
    await putCharactersId(characterId.value, {
      name: character.value.name,
      personalityTraits: f.personalityTraits.filter(t => t.trim()),
      ideals: f.ideals.filter(t => t.trim()),
      bonds: f.bonds.filter(t => t.trim()),
      flaws: f.flaws.filter(t => t.trim()),
      backstory: f.backstory,
      notes: f.notes,
      appearance: f.appearance,
      skin: f.skin,
      hair: f.hair,
      eyes: f.eyes,
      faith: f.faith,
      heightInInches: f.heightInInches,
      weightInPounds: f.weightInPounds,
    });
    const c = character.value;
    c.personalityTraits = [...f.personalityTraits.filter(t => t.trim())];
    c.ideals = [...f.ideals.filter(t => t.trim())];
    c.bonds = [...f.bonds.filter(t => t.trim())];
    c.flaws = [...f.flaws.filter(t => t.trim())];
    c.backstory = f.backstory;
    c.notes = f.notes;
    c.appearance = f.appearance;
    c.skin = f.skin;
    c.hair = f.hair;
    c.eyes = f.eyes;
    c.faith = f.faith;
    c.heightInInches = f.heightInInches;
    c.weightInPounds = f.weightInPounds;
  } catch {
    showToast({variant: "danger", message: "Failed to save."});
  } finally {
    notesSaving.value = false;
  }
}

function addListItem(list: string[]) {
  list.push("");
  debouncedSaveNotes();
}

function removeListItem(list: string[], index: number) {
  list.splice(index, 1);
  debouncedSaveNotes();
}

const bgSkills = computed(() => {
  const bg = character.value?.background?.background;
  return (bg?.skillProficiencies ?? []).map((s: number) => skillLabels[s]).filter(Boolean);
});

const bgTools = computed(() => {
  const bg = character.value?.background?.background;
  return (bg?.toolProficiencies ?? []).map((t: number) => toolLabels[t]).filter(Boolean);
});

const bgAbilityIncreases = computed(() => {
  const chosen = character.value?.background?.chosenAbilityScoreIncreases ?? {};
  return Object.entries(chosen).map(([k, v]) => ({
    label: abilityScoreLabels[Number(k)] ?? k,
    value: v as number
  })).filter(e => e.value > 0);
});

const bgLanguages = computed(() => {
  return (character.value?.background?.chosenLanguages ?? []).filter(Boolean);
});

const hpInput = ref(0);
const showHpModal = ref(false);
const hpSaving = ref(false);
const showConditionsModal = ref(false);
const activeConditions = ref<number[]>([]);

const isAtZeroHp = computed(() => (character.value?.currentHitPoints ?? 1) <= 0);

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

async function applyHeal(amount: number) {
  if (!character.value || amount <= 0) return;
  hpSaving.value = true;
  const newHp = Math.min(character.value.currentHitPoints + amount, character.value.maximumHitPoints);
  try {
    await putCharactersId(characterId.value, {name: character.value.name, currentHitPoints: newHp});
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
    await putCharactersId(characterId.value, {
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
    await putCharactersId(characterId.value, {name: character.value.name, temporaryHitPoints: newTemp});
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
    await putCharactersId(characterId.value, {name: character.value.name, [field]: next});
    character.value[field] = next;
  } catch {
    showToast({variant: "danger", message: "Failed to save death saves."});
  }
}

async function setExhaustion(level: number) {
  if (!character.value) return;
  const next = (character.value.exhaustionLevel ?? 0) === level ? 0 : level;
  try {
    await putCharactersId(characterId.value, {name: character.value.name, exhaustionLevel: next});
    character.value.exhaustionLevel = next;
  } catch {
    showToast({variant: "danger", message: "Failed to save exhaustion."});
  }
}

async function toggleInspiration() {
  if (!character.value) return;
  const next = (character.value.inspirationPoints ?? 0) > 0 ? 0 : 1;
  try {
    await putCharactersId(characterId.value, {name: character.value.name, inspirationPoints: next});
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
    await putCharactersId(characterId.value, {name: character.value.name, conditions: [...activeConditions.value]});
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
  syncCurrencyFromCharacter();
  syncNotesFromCharacter();
  syncCombatFromCharacter();
}, {immediate: true});

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
  0: 'SR', 1: 'LR', 2: 'Dawn', 3: 'None',
}

function getActionsWithReset(actions: any[]): any[] {
  return (actions ?? []).filter((a: any) => a.resetType != null && a.resetType !== 3)
}

function getClassFeatureUsage(classId: string, actionId: string): number {
  const cc = (character.value?.classes ?? []).find((c: any) => c.classId === classId)
  return Number(cc?.classFeatureUsages?.[actionId] ?? 0)
}

let classUsageTimer: ReturnType<typeof setTimeout> | null = null

function adjustClassFeatureUsage(classId: string, actionId: string, delta: number) {
  const cc = (character.value?.classes ?? []).find((c: any) => c.classId === classId)
  if (!cc) return
  if (!cc.classFeatureUsages) cc.classFeatureUsages = {}
  cc.classFeatureUsages[actionId] = Math.max(0, Number(cc.classFeatureUsages[actionId] ?? 0) + delta)
  if (classUsageTimer) clearTimeout(classUsageTimer)
  classUsageTimer = setTimeout(async () => {
    try {
      const classes: CharacterClassData[] = (character.value.classes ?? []).map((c: any) => ({
        id: c.id, classId: c.classId, level: c.level, subclassId: c.subclassId,
        isStartingClass: c.isStartingClass, classFeatureUsages: c.classFeatureUsages,
        chosenSkillProficiencies: c.chosenSkillProficiencies,
        chosenFeatureOptions: c.chosenFeatureOptions, chosenSpells: c.chosenSpells,
        spellSlotsUsed: c.spellSlotsUsed, pactSlotsUsed: c.pactSlotsUsed,
      }))
      await putCharactersIdClasses(characterId.value, { classes })
    } catch { showToast({ variant: "danger", message: "Failed to save feature usage." }) }
  }, 1000)
}

function getRaceTraitUsage(actionId: string): number {
  return Number(character.value?.race?.raceTraitUsages?.[actionId] ?? 0)
}

let raceUsageTimer: ReturnType<typeof setTimeout> | null = null

function adjustRaceTraitUsage(actionId: string, delta: number) {
  const race = character.value?.race
  if (!race) return
  if (!race.raceTraitUsages) race.raceTraitUsages = {}
  race.raceTraitUsages[actionId] = Math.max(0, Number(race.raceTraitUsages[actionId] ?? 0) + delta)
  if (raceUsageTimer) clearTimeout(raceUsageTimer)
  raceUsageTimer = setTimeout(async () => {
    try {
      await putCharactersIdRace(characterId.value, {
        raceId: character.value.race.raceId,
        raceTraitUsages: character.value.race.raceTraitUsages ?? {},
        chosenTraitOptions: character.value.race.chosenTraitOptions ?? {},
        chosenSpells: character.value.race.chosenSpells ?? {},
      })
    } catch { showToast({ variant: "danger", message: "Failed to save trait usage." }) }
  }, 1000)
}

function getFeatUsage(charFeatId: string, actionId: string): number {
  const cf = (character.value?.feats ?? []).find((f: any) => f.id === charFeatId)
  return Number(cf?.featActionUsages?.[actionId] ?? 0)
}

let featUsageTimer: ReturnType<typeof setTimeout> | null = null

function adjustFeatUsage(charFeatId: string, actionId: string, delta: number) {
  const cf = (character.value?.feats ?? []).find((f: any) => f.id === charFeatId)
  if (!cf) return
  if (!cf.featActionUsages) cf.featActionUsages = {}
  cf.featActionUsages[actionId] = Math.max(0, Number(cf.featActionUsages[actionId] ?? 0) + delta)
  if (featUsageTimer) clearTimeout(featUsageTimer)
  featUsageTimer = setTimeout(async () => {
    try {
      await putCharactersIdFeatsFeatId(characterId.value, cf.id, {
        featId: cf.featId, source: cf.source, sourceId: cf.sourceId,
        chosenAbilityScoreIncrease: cf.chosenAbilityScoreIncrease,
        chosenOptions: cf.chosenOptions ?? {},
        chosenSpells: cf.chosenSpells ?? {},
        featActionUsages: cf.featActionUsages ?? {},
      })
    } catch { showToast({ variant: "danger", message: "Failed to save feat usage." }) }
  }, 1000)
}

async function toggleCharge(ci: any, delta: number) {
  const max = ci.maxCharges ?? 0
  const newVal = Math.max(0, Math.min(max, (ci.chargesUsed ?? 0) + delta))
  ci.chargesUsed = newVal
  try {
    await putCharactersIdItemsItemId(characterId.value, ci.id, {
      quantity: ci.quantity, notes: ci.notes,
      attuned: ci.attuned, equipped: ci.equipped,
      maxCharges: ci.maxCharges, chargesUsed: newVal,
    })
  } catch { showToast({ variant: "danger", message: "Failed to update charges." }) }
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
    await postCharactersIdFeats(characterId.value, {featId: feat.id, source: 0});
    showToast({message: `Added ${feat.name}.`});
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to add feat."});
  }
}

async function removeFeat(charFeat: any) {
  try {
    await deleteCharactersIdFeatsFeatId(characterId.value, charFeat.id);
    character.value.feats = character.value.feats.filter((f: any) => f.id !== charFeat.id);
    showToast({message: "Feat removed."});
  } catch {
    showToast({variant: "danger", message: "Failed to remove feat."});
  }
}

const FULL_CASTER_SLOTS: Record<number, number[]> = {
  1: [2], 2: [3], 3: [4,2], 4: [4,3], 5: [4,3,2],
  6: [4,3,3], 7: [4,3,3,1], 8: [4,3,3,2], 9: [4,3,3,3,1],
  10: [4,3,3,3,2], 11: [4,3,3,3,2,1], 12: [4,3,3,3,2,1],
  13: [4,3,3,3,2,1,1], 14: [4,3,3,3,2,1,1], 15: [4,3,3,3,2,1,1,1],
  16: [4,3,3,3,2,1,1,1], 17: [4,3,3,3,2,1,1,1,1], 18: [4,3,3,3,3,1,1,1,1],
  19: [4,3,3,3,3,2,1,1,1], 20: [4,3,3,3,3,2,2,1,1],
}

const HALF_CASTER_SLOTS: Record<number, number[]> = {
  2: [2], 3: [3], 4: [3], 5: [4,2], 6: [4,2], 7: [4,3],
  8: [4,3], 9: [4,3,2], 10: [4,3,2], 11: [4,3,3],
  12: [4,3,3], 13: [4,3,3,1], 14: [4,3,3,1], 15: [4,3,3,2],
  16: [4,3,3,2], 17: [4,3,3,3,1], 18: [4,3,3,3,1], 19: [4,3,3,3,2],
  20: [4,3,3,3,2],
}

const PACT_SLOTS: Record<number, { slots: number; level: number }> = {
  1: { slots: 1, level: 1 }, 2: { slots: 2, level: 1 },
  3: { slots: 2, level: 2 }, 4: { slots: 2, level: 2 },
  5: { slots: 2, level: 3 }, 6: { slots: 2, level: 3 },
  7: { slots: 2, level: 4 }, 8: { slots: 2, level: 4 },
  9: { slots: 2, level: 5 }, 10: { slots: 2, level: 5 },
  11: { slots: 3, level: 5 }, 12: { slots: 3, level: 5 },
  13: { slots: 3, level: 5 }, 14: { slots: 3, level: 5 },
  15: { slots: 3, level: 5 }, 16: { slots: 3, level: 5 },
  17: { slots: 4, level: 5 }, 18: { slots: 4, level: 5 },
  19: { slots: 4, level: 5 }, 20: { slots: 4, level: 5 },
}

const spellcastingClasses = computed(() => {
  return (character.value?.classes ?? [])
    .filter((cc: any) => cc.subclass?.canCastSpells)
    .map((cc: any) => ({
      classId: cc.classId,
      className: cc.class?.name ?? 'Unknown',
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
    }))
})

const spellStats = computed(() => {
  const stats: Record<string, { abilityName: string; modifier: number; attackBonus: number; saveDC: number }> = {}
  const prof = character.value?.proficiencyBonus ?? 2
  for (const sc of spellcastingClasses.value) {
    if (sc.spellcastingAbility == null) continue
    const ab = character.value?.abilities?.find((a: any) => a.ability === sc.spellcastingAbility)
    const mod = ab ? Math.floor((ab.score - 10) / 2) : 0
    stats[sc.classId] = {
      abilityName: abilityAbbrev[sc.spellcastingAbility] ?? '???',
      modifier: mod,
      attackBonus: mod + prof,
      saveDC: 8 + mod + prof,
    }
  }
  return stats
})

function getMaxSpellSlots(classId: string): Record<number, number> {
  const sc = spellcastingClasses.value.find((c: any) => c.classId === classId)
  if (!sc) return {}
  const table = sc.spellPrepareType === 0 ? FULL_CASTER_SLOTS : HALF_CASTER_SLOTS
  const slots = table[sc.level] ?? []
  const result: Record<number, number> = {}
  slots.forEach((count, idx) => { result[idx + 1] = count })
  return result
}

const spellsByClass = computed(() => {
  const groups: Record<string, Record<number, any[]>> = {}
  for (const sc of spellcastingClasses.value) {
    groups[sc.classId] = {}
  }
  groups['other'] = {}
  for (const cs of (character.value?.spells ?? [])) {
    const key = cs.sourceClassId && groups[cs.sourceClassId] ? cs.sourceClassId : 'other'
    const level = cs.spell?.level ?? 0
    if (!groups[key]![level]) groups[key]![level] = []
    groups[key]![level].push(cs)
  }
  for (const classGroup of Object.values(groups)) {
    for (const levelSpells of Object.values(classGroup)) {
      levelSpells.sort((a: any, b: any) => (a.spell?.name ?? '').localeCompare(b.spell?.name ?? ''))
    }
  }
  return groups
})

const showAddSpellModal = ref(false)
const addSpellSearch = ref("")
const addSpellResults = ref<any[]>([])
const addSpellLoading = ref(false)
const addSpellClassId = ref("")
let addSpellTimer: ReturnType<typeof setTimeout> | null = null
const expandedSpells = ref<Set<string>>(new Set())

function toggleSpellExpand(id: string) {
  if (expandedSpells.value.has(id)) expandedSpells.value.delete(id)
  else expandedSpells.value.add(id)
}

function openAddSpellModal(classId: string) {
  addSpellClassId.value = classId
  addSpellSearch.value = ""
  addSpellResults.value = []
  showAddSpellModal.value = true
  searchSpells("")
}

function onAddSpellSearchInput() {
  if (addSpellTimer) clearTimeout(addSpellTimer)
  addSpellTimer = setTimeout(() => searchSpells(addSpellSearch.value), 300)
}

async function searchSpells(q: string) {
  addSpellLoading.value = true
  try {
    const existingIds = new Set((character.value?.spells ?? []).map((s: any) => s.spellId))
    const res = await getSpells({ pageSize: 100, search: q || undefined, classId: addSpellClassId.value || undefined })
    let spells = ((res as any).data?.items ?? []) as any[]

    const sc = spellcastingClasses.value.find((c: any) => c.classId === addSpellClassId.value)
    if (sc && sc.additionalSpellListIds.length > 0) {
      for (const listClassId of sc.additionalSpellListIds) {
        const extra = await getSpells({ pageSize: 100, search: q || undefined, classId: listClassId })
        const extraSpells = ((extra as any).data?.items ?? []) as any[]
        const existing = new Set(spells.map((s: any) => s.id))
        for (const s of extraSpells) {
          if (!existing.has(s.id)) spells.push(s)
        }
      }
    }

    addSpellResults.value = spells.filter((s: any) => !existingIds.has(s.id))
  } catch {
    showToast({ variant: "danger", message: "Failed to search spells." })
  } finally {
    addSpellLoading.value = false
  }
}

async function addSpellToCharacter(spell: any) {
  try {
    await postCharactersIdSpells(characterId.value, {
      spellId: spell.id,
      spellSource: 0,
      sourceClassId: addSpellClassId.value || undefined,
      isPrepared: false,
    })
    showToast({ message: `Added ${spell.name}.` })
    await fetchCharacter()
  } catch {
    showToast({ variant: "danger", message: "Failed to add spell." })
  }
}

async function removeSpell(charSpell: any) {
  try {
    await deleteCharactersIdSpellsSpellId(characterId.value, charSpell.id)
    character.value.spells = character.value.spells.filter((s: any) => s.id !== charSpell.id)
    showToast({ message: "Spell removed." })
  } catch {
    showToast({ variant: "danger", message: "Failed to remove spell." })
  }
}

async function togglePrepared(charSpell: any) {
  try {
    await putCharactersIdSpellsSpellId(characterId.value, charSpell.id, {
      isPrepared: !charSpell.isPrepared,
    })
    charSpell.isPrepared = !charSpell.isPrepared
  } catch {
    showToast({ variant: "danger", message: "Failed to update spell." })
  }
}

async function toggleSlot(classId: string, level: number, delta: number) {
  const cc = (character.value?.classes ?? []).find((c: any) => c.classId === classId)
  if (!cc) return
  const max = getMaxSpellSlots(classId)[level] ?? 0
  const current = Number(cc.spellSlotsUsed?.[level] ?? 0)
  const newVal = Math.max(0, Math.min(max, current + delta))
  if (!cc.spellSlotsUsed) cc.spellSlotsUsed = {}
  cc.spellSlotsUsed[level] = newVal

  try {
    const classes: CharacterClassData[] = (character.value?.classes ?? []).map((c: any) => ({
      id: c.id,
      classId: c.classId,
      level: c.level,
      subclassId: c.subclassId,
      isStartingClass: c.isStartingClass,
      classFeatureUsages: c.classFeatureUsages,
      chosenSkillProficiencies: c.chosenSkillProficiencies,
      chosenFeatureOptions: c.chosenFeatureOptions,
      chosenSpells: c.chosenSpells,
      spellSlotsUsed: c.spellSlotsUsed,
      pactSlotsUsed: c.pactSlotsUsed,
    }))
    await putCharactersIdClasses(characterId.value, { classes })
  } catch {
    showToast({ variant: "danger", message: "Failed to update spell slots." })
  }
}

function getSlotUsed(classId: string, level: number): number {
  const cc = (character.value?.classes ?? []).find((c: any) => c.classId === classId)
  return Number(cc?.spellSlotsUsed?.[level] ?? 0)
}

function formatCastingTime(time: any): string {
  if (!time) return ''
  if (time.amount === 1 && time.unit === 5) return '1 Action'
  if (time.amount === 1 && time.unit === 6) return '1 Bonus Action'
  if (time.amount === 1 && time.unit === 7) return '1 Reaction'
  return `${time.amount} ${timeUnitLabel(time.unit)}`
}

function timeUnitLabel(unit: number): string {
  const labels: Record<number, string> = {
    0: 'seconds', 1: 'minutes', 2: 'hours', 3: 'days', 4: 'rounds',
    5: 'actions', 6: 'bonus actions', 7: 'reactions',
  }
  return labels[unit] ?? ''
}

function formatRange(range: number): string {
  if (range === 0) return 'Self'
  if (range === -1) return 'Touch'
  if (range === -2) return 'Unlimited'
  return `${range} ft.`
}

function formatComponents(spell: any): string {
  const parts: string[] = []
  if (spell?.hasVocalComponent) parts.push('V')
  if (spell?.hasSomaticComponent) parts.push('S')
  if (spell?.hasMaterialComponent) parts.push('M')
  return parts.join(', ')
}

const showOverrideModal = ref(false);
const overrideType = ref("");
const overrideTarget = ref(0);
const overrideSaving = ref(false);

const abilityOverride = ref({scoreBonus: 0, savingThrowBonus: 0, overrideSavingThrowProficiency: null as number | null});
const skillOverride = ref({bonus: 0, overrideProficiency: null as number | null, advantageState: 0});
const acOverride = ref({baseArmorClass: 0, armorClassBonus: 0});
const initOverride = ref({initiativeBonus: 0});
const speedOverride = ref({movementSpeed: 0, swimmingSpeed: 0, flyingSpeed: 0});
const hpOverrideForm = ref({hitPointBonus: 0, overriddenMaximumHitPoints: null as number | null});
const defenseNewDamageType = ref(0);
const defenseNewState = ref(1);

const overrideTitle = computed(() => {
  switch (overrideType.value) {
    case "ability": return `Override ${abilityScoreLabels[overrideTarget.value] ?? "Ability"}`;
    case "skill": return `Override ${skillLabels[overrideTarget.value] ?? "Skill"}`;
    case "ac": return "Override Armor Class";
    case "initiative": return "Override Initiative";
    case "speed": return "Override Speed";
    case "hp": return "Override Hit Points";
    case "defense": return "Manage Defenses";
    default: return "Override";
  }
});

function openOverride(type: string, targetId?: number) {
  overrideType.value = type;
  overrideTarget.value = targetId ?? 0;
  const c = character.value;
  if (!c) return;

  if (type === "ability") {
    const ab = c.abilities?.find((a: any) => a.ability === targetId);
    abilityOverride.value = {
      scoreBonus: ab?.scoreBonus ?? 0,
      savingThrowBonus: ab?.savingThrowBonus ?? 0,
      overrideSavingThrowProficiency: ab?.overrideSavingThrowProficiency ?? null,
    };
  } else if (type === "skill") {
    const sk = c.skills?.find((s: any) => s.skill === targetId);
    skillOverride.value = {
      bonus: sk?.bonus ?? 0,
      overrideProficiency: sk?.overrideProficiency ?? null,
      advantageState: sk?.advantageState ?? 0,
    };
  } else if (type === "ac") {
    acOverride.value = {
      baseArmorClass: c.baseArmorClass ?? 10,
      armorClassBonus: c.armorClassBonus ?? 0,
    };
  } else if (type === "initiative") {
    initOverride.value = {initiativeBonus: c.initiativeBonus ?? 0};
  } else if (type === "speed") {
    speedOverride.value = {
      movementSpeed: c.movementSpeed ?? 30,
      swimmingSpeed: c.swimmingSpeed ?? 0,
      flyingSpeed: c.flyingSpeed ?? 0,
    };
  } else if (type === "hp") {
    hpOverrideForm.value = {
      hitPointBonus: c.hitPointBonus ?? 0,
      overriddenMaximumHitPoints: c.overriddenMaximumHitPoints ?? null,
    };
  }

  showOverrideModal.value = true;
}

async function saveAbilityOverride() {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    const abilities: CharacterAbilityData[] = (character.value.abilities ?? []).map((a: any) => {
      if (a.ability === overrideTarget.value) {
        return {
          ability: a.ability,
          rawScore: a.rawScore,
          scoreBonus: abilityOverride.value.scoreBonus,
          rawSavingThrowProficiency: a.rawSavingThrowProficiency,
          overrideSavingThrowProficiency: abilityOverride.value.overrideSavingThrowProficiency,
          savingThrowBonus: abilityOverride.value.savingThrowBonus,
        };
      }
      return {
        ability: a.ability,
        rawScore: a.rawScore,
        scoreBonus: a.scoreBonus,
        rawSavingThrowProficiency: a.rawSavingThrowProficiency,
        overrideSavingThrowProficiency: a.overrideSavingThrowProficiency,
        savingThrowBonus: a.savingThrowBonus,
      };
    });
    await putCharactersIdAbilities(characterId.value, {abilities});
    showOverrideModal.value = false;
    showToast({message: "Ability override saved."});
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to save ability override."});
  } finally {
    overrideSaving.value = false;
  }
}

async function saveSkillOverride() {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    const skills: CharacterSkillData[] = (character.value.skills ?? []).map((s: any) => {
      if (s.skill === overrideTarget.value) {
        return {
          skill: s.skill,
          bonus: skillOverride.value.bonus,
          rawProficiency: s.rawProficiency,
          overrideProficiency: skillOverride.value.overrideProficiency,
          advantageState: skillOverride.value.advantageState,
        };
      }
      return {
        skill: s.skill,
        bonus: s.bonus,
        rawProficiency: s.rawProficiency,
        overrideProficiency: s.overrideProficiency,
        advantageState: s.advantageState,
      };
    });
    await putCharactersIdSkills(characterId.value, {skills});
    showOverrideModal.value = false;
    showToast({message: "Skill override saved."});
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to save skill override."});
  } finally {
    overrideSaving.value = false;
  }
}

async function saveAcOverride() {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    await putCharactersId(characterId.value, {
      name: character.value.name,
      baseArmorClass: acOverride.value.baseArmorClass,
      armorClassBonus: acOverride.value.armorClassBonus,
    });
    showOverrideModal.value = false;
    showToast({message: "AC override saved."});
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to save AC override."});
  } finally {
    overrideSaving.value = false;
  }
}

async function saveInitOverride() {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    await putCharactersId(characterId.value, {
      name: character.value.name,
      initiativeBonus: initOverride.value.initiativeBonus,
    });
    showOverrideModal.value = false;
    showToast({message: "Initiative override saved."});
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to save initiative override."});
  } finally {
    overrideSaving.value = false;
  }
}

async function saveSpeedOverride() {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    await putCharactersId(characterId.value, {
      name: character.value.name,
      movementSpeed: speedOverride.value.movementSpeed,
      swimmingSpeed: speedOverride.value.swimmingSpeed,
      flyingSpeed: speedOverride.value.flyingSpeed,
    });
    showOverrideModal.value = false;
    showToast({message: "Speed override saved."});
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to save speed override."});
  } finally {
    overrideSaving.value = false;
  }
}

async function saveHpOverride() {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    await putCharactersId(characterId.value, {
      name: character.value.name,
      hitPointBonus: hpOverrideForm.value.hitPointBonus,
      overriddenMaximumHitPoints: hpOverrideForm.value.overriddenMaximumHitPoints,
    });
    showOverrideModal.value = false;
    showToast({message: "HP override saved."});
    await fetchCharacter();
  } catch {
    showToast({variant: "danger", message: "Failed to save HP override."});
  } finally {
    overrideSaving.value = false;
  }
}

async function addDefense() {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    const defenses = {...(character.value.damageDefenses ?? {})};
    const key = String(defenseNewDamageType.value);
    if (!defenses[key]) defenses[key] = [];
    if (!defenses[key].includes(defenseNewState.value)) {
      defenses[key] = [...defenses[key], defenseNewState.value];
    }
    await putCharactersId(characterId.value, {
      name: character.value.name,
      damageDefenses: defenses,
    });
    character.value.damageDefenses = defenses;
    showToast({message: "Defense added."});
  } catch {
    showToast({variant: "danger", message: "Failed to add defense."});
  } finally {
    overrideSaving.value = false;
  }
}

async function removeDefense(damageType: string, stateIdx: number) {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    const defenses = {...(character.value.damageDefenses ?? {})};
    if (defenses[damageType]) {
      defenses[damageType] = defenses[damageType].filter((_: any, i: number) => i !== stateIdx);
      if (defenses[damageType].length === 0) delete defenses[damageType];
    }
    await putCharactersId(characterId.value, {
      name: character.value.name,
      damageDefenses: defenses,
    });
    character.value.damageDefenses = defenses;
    showToast({message: "Defense removed."});
  } catch {
    showToast({variant: "danger", message: "Failed to remove defense."});
  } finally {
    overrideSaving.value = false;
  }
}

function saveOverride() {
  switch (overrideType.value) {
    case "ability": return saveAbilityOverride();
    case "skill": return saveSkillOverride();
    case "ac": return saveAcOverride();
    case "initiative": return saveInitOverride();
    case "speed": return saveSpeedOverride();
    case "hp": return saveHpOverride();
  }
}
</script>

<template>
  <div class="sheet">
    <div class="sheet__header">
      <button class="sheet__back" @click="goBack" aria-label="Back">
        <i class="fas fa-arrow-left"/>
      </button>

      <div class="sheet__header-title">
        <h1 class="sheet__title">{{ character?.name ?? "Loading..." }}</h1>
        <p v-if="character" class="sheet__meta">
          <span class="sheet__meta-line">{{ getRaceName() }} • {{ getClassSummary() }}</span>
          <span class="sheet__meta-line">• {{ getBackgroundName() }}</span>
        </p>
      </div>

      <UiButton size="sm" @click="goToEdit">
        <i class="fas fa-pen"/> Edit
      </UiButton>
    </div>

    <div v-if="loading" class="sheet__loading">
      <i class="fas fa-spinner fa-spin"/> Loading character...
    </div>

    <template v-else-if="character">
      <section class="hero">
        <div class="hero__left">
          <Avatar :character="character" allow-edit class="hero__avatar"/>
          <div class="hero__badges">
            <UiBadge v-if="character.level" :label="`Level ${character.level}`" variant="accent"/>
            <UiBadge v-if="character.alignment !== undefined" :label="alignmentLabels[character.alignment] ?? ''"
                     variant="muted"/>
            <UiBadge v-if="character.size !== undefined" :label="sizeLabels[character.size] ?? ''" variant="muted"/>
          </div>
        </div>

        <!--        <div class="hero__right">
                  <div class="quickline">
                    <div class="quickchip">
                      <span class="quickchip__k">Walking</span>
                      <span class="quickchip__v">{{ character.movementSpeed }} ft</span>
                    </div>
                    <div class="quickchip" v-if="(character.swimmingSpeed ?? 0) > 0">
                      <span class="quickchip__k">Swimming</span>
                      <span class="quickchip__v">{{ character.swimmingSpeed }} ft</span>
                    </div>
                    <div class="quickchip" v-if="(character.flyingSpeed ?? 0) > 0">
                      <span class="quickchip__k">Flying</span>
                      <span class="quickchip__v">{{ character.flyingSpeed }} ft</span>
                    </div>
                    <div class="quickchip quickchip&#45;&#45;accent">
                      <span class="quickchip__k">Proficiency</span>
                      <span class="quickchip__v">+{{ character.proficiencyBonus }}</span>
                    </div>
                  </div>
                </div>-->
      </section>

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

          <div class="combat-card__sub">
            <span>Hit Dice {{ character.currentHitDie }}/{{ character.maxHitDie }}</span>
          </div>

          <div class="combat-card__sub hp-subline">
            <span>Hit Dice {{ character.currentHitDie }}/{{ character.maxHitDie }}</span>

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

      <UiGrid :cols="1" :cols-lg="3" :gap="1" class="main-grid">
        <div class="col">
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
                  <span v-for="a in character.specificArmorProficiencies ?? []" :key="a" class="chip chip--muted">{{
                      a
                    }}</span>
                </div>
              </div>

              <div class="profs__section">
                <div class="profs__title">Weapons</div>
                <div class="profs__items">
                  <span
                    v-if="!(character.weaponProficiencies?.length ?? 0) && !(character.specificWeaponProficiencies?.length ?? 0)"
                    class="chips__empty">None</span>
                  <span v-for="w in character.weaponProficiencies ?? []" :key="w" class="chip chip--muted">{{
                      w
                    }}</span>
                  <span v-for="w in character.specificWeaponProficiencies ?? []" :key="w" class="chip chip--muted">{{
                      w
                    }}</span>
                </div>
              </div>

              <div class="profs__section">
                <div class="profs__title">Tools</div>
                <div class="profs__items">
                  <span
                    v-if="!(character.toolProficiencies?.length ?? 0) && !(character.specificToolProficiencies?.length ?? 0)"
                    class="chips__empty">None</span>
                  <span v-for="t in character.toolProficiencies ?? []" :key="t" class="chip chip--muted">{{ t }}</span>
                  <span v-for="t in character.specificToolProficiencies ?? []" :key="t" class="chip chip--muted">{{
                      t
                    }}</span>
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
        </div>

        <div class="col col-2">
          <div class="tabs">
            <button class="tab" :class="{ 'is-active': activeTab === 'actions' }" @click="activeTab = 'actions'">
              Actions
            </button>
            <button class="tab" :class="{ 'is-active': activeTab === 'features' }" @click="activeTab = 'features'">
              Features
            </button>
            <button v-if="spellcastingClasses.length || !!(character.spells?.length)" class="tab" :class="{ 'is-active': activeTab === 'spells' }" @click="activeTab = 'spells'">
              Spells
            </button>
            <button class="tab" :class="{ 'is-active': activeTab === 'inventory' }" @click="activeTab = 'inventory'">
              Inventory
            </button>
            <button class="tab" :class="{ 'is-active': activeTab === 'background' }" @click="activeTab = 'background'">
              Background
            </button>
            <button class="tab" :class="{ 'is-active': activeTab === 'notes' }" @click="activeTab = 'notes'">
              Notes
            </button>
          </div>

          <div class="tabpanel">
            <div v-if="activeTab === 'actions'" class="tabpanel__body">
              <div class="actions-tab">
                <div class="feature-section__title">Weapon Attacks</div>
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
            </div>

            <div v-else-if="activeTab === 'spells'" class="tabpanel__body">
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
            </div>
            <div v-else-if="activeTab === 'features'" class="tabpanel__body">
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
            </div>

            <div v-else-if="activeTab === 'background'" class="tabpanel__body">
              <div v-if="character.background?.background" class="bg-tab">
                <div class="bg-tab__header">
                  <h3 class="bg-tab__name">{{ character.background.background.name }}</h3>
                  <span class="bg-tab__source">{{ sourceLabels[character.background.background.source ?? 0] }}</span>
                </div>

                <div v-if="bgSkills.length" class="bg-tab__section">
                  <div class="bg-tab__label">Skill Proficiencies</div>
                  <div class="chips__wrap">
                    <span v-for="s in bgSkills" :key="s" class="chip chip--muted">{{ s }}</span>
                  </div>
                </div>

                <div v-if="bgTools.length" class="bg-tab__section">
                  <div class="bg-tab__label">Tool Proficiencies</div>
                  <div class="chips__wrap">
                    <span v-for="t in bgTools" :key="t" class="chip chip--muted">{{ t }}</span>
                  </div>
                </div>

                <div v-if="bgAbilityIncreases.length" class="bg-tab__section">
                  <div class="bg-tab__label">Ability Score Increases</div>
                  <div class="chips__wrap">
                    <span v-for="a in bgAbilityIncreases" :key="a.label" class="chip chip--info">
                      {{ a.label }} +{{ a.value }}
                    </span>
                  </div>
                </div>

                <div v-if="bgLanguages.length" class="bg-tab__section">
                  <div class="bg-tab__label">Languages</div>
                  <div class="chips__wrap">
                    <span v-for="l in bgLanguages" :key="l" class="chip chip--info">{{ l }}</span>
                  </div>
                </div>
              </div>
              <div v-else class="bg-tab__none">No background selected.</div>
            </div>

            <div v-else-if="activeTab === 'notes'" class="tabpanel__body notes-tab">
              <span v-if="notesSaving" class="notes-tab__saving">
                <i class="fas fa-spinner fa-spin"/> Saving...
              </span>

              <div class="notes-tab__section">
                <div class="notes-tab__section-header">
                  <div class="bg-tab__label">Personality Traits</div>
                  <button class="notes-tab__add" @click="addListItem(notesForm.personalityTraits)">
                    <i class="fas fa-plus"/>
                  </button>
                </div>
                <div class="notes-tab__list">
                  <div v-for="(_, i) in notesForm.personalityTraits" :key="'pt' + i" class="notes-tab__list-row">
                    <input
                      class="notes-tab__inline-input"
                      v-model="notesForm.personalityTraits[i]"
                      placeholder="Personality trait..."
                      @input="debouncedSaveNotes()"
                    />
                    <button class="notes-tab__remove" @click="removeListItem(notesForm.personalityTraits, i)">
                      <i class="fas fa-times"/>
                    </button>
                  </div>
                  <div v-if="!notesForm.personalityTraits.length" class="notes-tab__empty-hint">None yet.</div>
                </div>
              </div>

              <div class="notes-tab__section">
                <div class="notes-tab__section-header">
                  <div class="bg-tab__label">Ideals</div>
                  <button class="notes-tab__add" @click="addListItem(notesForm.ideals)">
                    <i class="fas fa-plus"/>
                  </button>
                </div>
                <div class="notes-tab__list">
                  <div v-for="(_, i) in notesForm.ideals" :key="'id' + i" class="notes-tab__list-row">
                    <input
                      class="notes-tab__inline-input"
                      v-model="notesForm.ideals[i]"
                      placeholder="Ideal..."
                      @input="debouncedSaveNotes()"
                    />
                    <button class="notes-tab__remove" @click="removeListItem(notesForm.ideals, i)">
                      <i class="fas fa-times"/>
                    </button>
                  </div>
                  <div v-if="!notesForm.ideals.length" class="notes-tab__empty-hint">None yet.</div>
                </div>
              </div>

              <div class="notes-tab__section">
                <div class="notes-tab__section-header">
                  <div class="bg-tab__label">Bonds</div>
                  <button class="notes-tab__add" @click="addListItem(notesForm.bonds)">
                    <i class="fas fa-plus"/>
                  </button>
                </div>
                <div class="notes-tab__list">
                  <div v-for="(_, i) in notesForm.bonds" :key="'bo' + i" class="notes-tab__list-row">
                    <input
                      class="notes-tab__inline-input"
                      v-model="notesForm.bonds[i]"
                      placeholder="Bond..."
                      @input="debouncedSaveNotes()"
                    />
                    <button class="notes-tab__remove" @click="removeListItem(notesForm.bonds, i)">
                      <i class="fas fa-times"/>
                    </button>
                  </div>
                  <div v-if="!notesForm.bonds.length" class="notes-tab__empty-hint">None yet.</div>
                </div>
              </div>

              <div class="notes-tab__section">
                <div class="notes-tab__section-header">
                  <div class="bg-tab__label">Flaws</div>
                  <button class="notes-tab__add" @click="addListItem(notesForm.flaws)">
                    <i class="fas fa-plus"/>
                  </button>
                </div>
                <div class="notes-tab__list">
                  <div v-for="(_, i) in notesForm.flaws" :key="'fl' + i" class="notes-tab__list-row">
                    <input
                      class="notes-tab__inline-input"
                      v-model="notesForm.flaws[i]"
                      placeholder="Flaw..."
                      @input="debouncedSaveNotes()"
                    />
                    <button class="notes-tab__remove" @click="removeListItem(notesForm.flaws, i)">
                      <i class="fas fa-times"/>
                    </button>
                  </div>
                  <div v-if="!notesForm.flaws.length" class="notes-tab__empty-hint">None yet.</div>
                </div>
              </div>

              <div class="detail-divider"/>

              <div class="notes-tab__section">
                <div class="bg-tab__label">Appearance</div>
                <div class="notes-tab__appearance-grid">
                  <div class="notes-tab__field">
                    <label class="notes-tab__field-label">Eyes</label>
                    <input class="notes-tab__inline-input" v-model="notesForm.eyes" placeholder="Eye color..."
                           @input="debouncedSaveNotes()"/>
                  </div>
                  <div class="notes-tab__field">
                    <label class="notes-tab__field-label">Hair</label>
                    <input class="notes-tab__inline-input" v-model="notesForm.hair" placeholder="Hair color..."
                           @input="debouncedSaveNotes()"/>
                  </div>
                  <div class="notes-tab__field">
                    <label class="notes-tab__field-label">Skin</label>
                    <input class="notes-tab__inline-input" v-model="notesForm.skin" placeholder="Skin color..."
                           @input="debouncedSaveNotes()"/>
                  </div>
                  <div class="notes-tab__field">
                    <label class="notes-tab__field-label">Faith</label>
                    <input class="notes-tab__inline-input" v-model="notesForm.faith" placeholder="Faith / Deity..."
                           @input="debouncedSaveNotes()"/>
                  </div>
                  <div class="notes-tab__field">
                    <div class="notes-tab__field-label-row">
                      <label class="notes-tab__field-label">Height</label>
                      <button class="notes-tab__unit-toggle" @click="heightUnit = heightUnit === 'ft' ? 'm' : 'ft'">
                        {{ heightUnit === 'ft' ? 'ft/in' : 'cm' }}
                      </button>
                    </div>
                    <div v-if="heightUnit === 'ft'" class="notes-tab__height-row">
                      <input class="notes-tab__inline-input notes-tab__inline-input--sm" type="number" min="0"
                             :value="heightFeet"
                             @input="heightFeet = Number(($event.target as HTMLInputElement).value); debouncedSaveNotes()"/>
                      <span class="notes-tab__unit-label">ft</span>
                      <input class="notes-tab__inline-input notes-tab__inline-input--sm" type="number" min="0" max="11"
                             :value="heightInches"
                             @input="heightInches = Number(($event.target as HTMLInputElement).value); debouncedSaveNotes()"/>
                      <span class="notes-tab__unit-label">in</span>
                    </div>
                    <div v-else class="notes-tab__height-row">
                      <input class="notes-tab__inline-input notes-tab__inline-input--sm" type="number" min="0"
                             :value="displayHeightCm"
                             @input="displayHeightCm = Number(($event.target as HTMLInputElement).value); debouncedSaveNotes()"/>
                      <span class="notes-tab__unit-label">cm</span>
                    </div>
                  </div>
                  <div class="notes-tab__field">
                    <div class="notes-tab__field-label-row">
                      <label class="notes-tab__field-label">Weight</label>
                      <button class="notes-tab__unit-toggle" @click="weightUnit = weightUnit === 'lb' ? 'kg' : 'lb'">
                        {{ weightUnit === 'lb' ? 'lb' : 'kg' }}
                      </button>
                    </div>
                    <div class="notes-tab__height-row">
                      <input
                        v-if="weightUnit === 'lb'"
                        class="notes-tab__inline-input notes-tab__inline-input--sm"
                        type="number" min="0"
                        v-model.number="notesForm.weightInPounds"
                        @input="debouncedSaveNotes()"
                      />
                      <input
                        v-else
                        class="notes-tab__inline-input notes-tab__inline-input--sm"
                        type="number" min="0" step="0.1"
                        :value="displayWeightKg"
                        @input="displayWeightKg = Number(($event.target as HTMLInputElement).value); debouncedSaveNotes()"
                      />
                      <span class="notes-tab__unit-label">{{ weightUnit }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="notes-tab__section">
                <div class="bg-tab__label">Appearance</div>
                <textarea
                  class="notes-textarea"
                  v-model="notesForm.appearance"
                  placeholder="Scars, tattoos, distinguishing marks..."
                  rows="3"
                  @input="debouncedSaveNotes()"
                />
              </div>

              <div class="detail-divider"/>

              <div class="notes-tab__section">
                <div class="bg-tab__label">Backstory</div>
                <textarea
                  class="notes-textarea"
                  v-model="notesForm.backstory"
                  placeholder="Your character's backstory..."
                  rows="6"
                  @input="debouncedSaveNotes()"
                />
              </div>

              <div class="detail-divider"/>

              <div class="notes-tab__section">
                <div class="bg-tab__label">Notes</div>
                <textarea
                  class="notes-textarea"
                  v-model="notesForm.notes"
                  placeholder="Session notes, reminders, or anything else..."
                  rows="6"
                  @input="debouncedSaveNotes()"
                />
              </div>
            </div>

            <div v-else-if="activeTab === 'inventory'" class="tabpanel__body">
              <div class="inv-currency">
                <div class="inv-currency__title">Currency</div>
                <div class="inv-currency__grid">
                  <div class="inv-coin" v-for="c in [
                    { key: 'gp', label: 'GP', ref: editGp, cls: 'coin--gp' },
                    { key: 'sp', label: 'SP', ref: editSp, cls: 'coin--sp' },
                    { key: 'cp', label: 'CP', ref: editCp, cls: 'coin--cp' },
                    { key: 'ep', label: 'EP', ref: editEp, cls: 'coin--ep' },
                  ]" :key="c.key">
                    <button class="inv-coin__btn" @click="adjustCurrency(c.key as any, -1)"><i class="fas fa-minus"/>
                    </button>
                    <span class="inv-coin__val" :class="c.cls">{{ c.ref }}</span>
                    <span class="inv-coin__label">{{ c.label }}</span>
                    <button class="inv-coin__btn" @click="adjustCurrency(c.key as any, 1)"><i class="fas fa-plus"/>
                    </button>
                  </div>
                </div>
                <div class="inv-currency__total">Total: {{ totalGpValue.toFixed(2) }} GP</div>
              </div>

              <div v-if="attunedCount > 3" class="inv-warning">
                <i class="fas fa-exclamation-triangle"/>
                You have {{ attunedCount }} attuned items. RAW limits attunement to 3.
              </div>

              <template v-if="hasItems">
                <div v-for="group in ([
                  { key: 'equipped', label: 'Equipped', items: equippedItems },
                  { key: 'attuned', label: 'Attuned', items: attunedItems },
                  { key: 'other', label: 'Other', items: otherItems },
                ] as const)" :key="group.key">
                  <div v-if="group.items.length" class="inv-group">
                    <div class="inv-group__title">{{ group.label }}</div>
                    <div class="inv-list">
                      <div v-for="ci in group.items" :key="ci.id" class="inv-row">
                        <div class="inv-row__info">
                          <span class="inv-row__name">
                            {{ ci.item?.name ?? "Unknown" }}
                            <span v-if="ci.quantity > 1" class="inv-row__qty">x{{ ci.quantity }}</span>
                          </span>
                          <div class="inv-row__tags">
                            <UiBadge v-if="ci.item?.type !== undefined" :label="itemTypeLabels[ci.item.type] ?? ''"
                                     variant="muted"/>
                            <UiBadge v-if="ci.item?.rarity !== undefined && ci.item.rarity > 0"
                                     :label="rarityLabels[ci.item.rarity] ?? ''" variant="info"/>
                            <span v-if="ci.item?.weightInOunces" class="inv-row__weight">{{
                                (Number(ci.item.weightInOunces) / 16).toFixed(1)
                              }} lb</span>
                          </div>
                          <div v-if="ci.maxCharges > 0" class="charges-tracker">
                            <span class="charges-tracker__label">Charges</span>
                            <div class="slot-tracker">
                              <button
                                v-for="p in ci.maxCharges"
                                :key="p"
                                class="slot-pip"
                                :class="{ 'slot-pip--used': p <= (ci.chargesUsed ?? 0) }"
                                @click="toggleCharge(ci, p <= (ci.chargesUsed ?? 0) ? -1 : 1)"
                              />
                            </div>
                          </div>
                        </div>
                        <div class="inv-row__actions">
                          <button class="inv-action" :class="{ 'is-active': ci.equipped }" title="Equip"
                                  @click="toggleEquip(ci)">
                            <i class="fas fa-shield-alt"/>
                          </button>
                          <button v-if="ci.item?.requiresAttunement" class="inv-action"
                                  :class="{ 'is-active': ci.attuned }" title="Attune" @click="toggleAttune(ci)">
                            <i class="fas fa-hand-sparkles"/>
                          </button>
                          <button class="inv-action" title="Edit" @click="openEditItem(ci)">
                            <i class="fas fa-pen"/>
                          </button>
                          <button class="inv-action inv-action--danger" title="Remove" @click="removeItem(ci)">
                            <i class="fas fa-trash"/>
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <button class="inv-add-btn" @click="openAddItemModal">
                  <i class="fas fa-plus"/> Add Item
                </button>
              </template>

              <div v-else class="inv-empty">
                <i class="fas fa-box-open inv-empty__icon"/>
                <p>Your inventory is empty.</p>
                <div class="inv-empty__actions">
                  <UiButton v-if="hasStartingEquipment" @click="openStartingEquipModal">
                    <i class="fas fa-scroll"/> Choose Starting Equipment
                  </UiButton>
                  <UiButton variant="secondary" @click="openAddItemModal">
                    <i class="fas fa-plus"/> Add Items Manually
                  </UiButton>
                </div>
              </div>
            </div>

            <div v-else class="tabpanel__body">
              <div class="tabpanel__empty">Select a tab.</div>
            </div>
          </div>

        </div>
      </UiGrid>
    </template>

    <UiModal v-model="showAddItemModal" title="Add Item" :close-on-backdrop="true" :close-on-esc="true"
             class="inv-modal">
      <div class="inv-modal__search">
        <UiInput v-model="addItemSearch" placeholder="Search items..." @input="onAddItemSearchInput"/>
      </div>
      <div class="inv-modal__qty-row">
        <span class="inv-modal__qty-label">Quantity:</span>
        <button class="inv-coin__btn" @click="addItemQuantity = Math.max(1, addItemQuantity - 1)"><i
          class="fas fa-minus"/></button>
        <span class="inv-modal__qty-val">{{ addItemQuantity }}</span>
        <button class="inv-coin__btn" @click="addItemQuantity++"><i class="fas fa-plus"/></button>
      </div>
      <div v-if="addItemLoading" class="inv-modal__loading"><i class="fas fa-spinner fa-spin"/> Searching...</div>
      <div v-else class="inv-modal__results">
        <button v-for="item in addItemResults" :key="item.id" class="inv-modal__item" @click="addItemToCharacter(item)">
          <span class="inv-modal__item-name">{{ item.name }}</span>
          <div class="inv-modal__item-tags">
            <UiBadge v-if="item.type !== undefined" :label="itemTypeLabels[item.type] ?? ''" variant="muted"/>
            <UiBadge v-if="item.rarity !== undefined && item.rarity > 0" :label="rarityLabels[item.rarity] ?? ''"
                     variant="info"/>
          </div>
        </button>
        <div v-if="!addItemResults.length" class="inv-modal__empty">No items found.</div>
      </div>
    </UiModal>

    <UiModal v-model="showEditItemModal" :title="editingItem?.item?.name ?? 'Edit Item'" :close-on-backdrop="true"
             :close-on-esc="true" class="inv-modal">
      <div v-if="editingItem" class="inv-edit">
        <div class="inv-edit__field">
          <label class="inv-edit__label">Quantity</label>
          <div class="inv-edit__spinner">
            <button class="inv-coin__btn" @click="editQuantity = Math.max(1, editQuantity - 1)"><i
              class="fas fa-minus"/></button>
            <span class="inv-modal__qty-val">{{ editQuantity }}</span>
            <button class="inv-coin__btn" @click="editQuantity++"><i class="fas fa-plus"/></button>
          </div>
        </div>
        <div class="inv-edit__field">
          <label class="inv-edit__label">Notes</label>
          <UiInput v-model="editNotes" placeholder="Item notes..."/>
        </div>
        <div class="inv-edit__field">
          <label class="inv-edit__label">Max Charges</label>
          <div class="inv-edit__spinner">
            <button class="inv-coin__btn" @click="editMaxCharges = Math.max(0, editMaxCharges - 1)"><i class="fas fa-minus"/></button>
            <span class="inv-modal__qty-val">{{ editMaxCharges }}</span>
            <button class="inv-coin__btn" @click="editMaxCharges++"><i class="fas fa-plus"/></button>
          </div>
        </div>
        <div class="inv-edit__toggles">
          <button class="inv-toggle" :class="{ 'is-active': editEquipped }" @click="editEquipped = !editEquipped">
            <i class="fas fa-shield-alt"/> Equipped
          </button>
          <button v-if="editingItem.item?.requiresAttunement" class="inv-toggle" :class="{ 'is-active': editAttuned }"
                  @click="editAttuned = !editAttuned">
            <i class="fas fa-hand-sparkles"/> Attuned
          </button>
        </div>
      </div>
      <template #footer>
        <div class="inv-edit__footer">
          <button class="inv-action inv-action--danger-btn" @click="removeItem(editingItem)">
            <i class="fas fa-trash"/> Remove
          </button>
          <UiButton @click="saveEditItem">Save</UiButton>
        </div>
      </template>
    </UiModal>

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

    <UiModal v-model="showOverrideModal" :title="overrideTitle" :close-on-backdrop="true" :close-on-esc="true" class="override-modal">
      <div v-if="overrideType === 'ability'" class="override-fields">
        <div class="override-field">
          <label class="override-field__label">Score Bonus</label>
          <input type="number" class="override-field__input" v-model.number="abilityOverride.scoreBonus"/>
        </div>
        <div class="override-field">
          <label class="override-field__label">Saving Throw Bonus</label>
          <input type="number" class="override-field__input" v-model.number="abilityOverride.savingThrowBonus"/>
        </div>
        <div class="override-field">
          <label class="override-field__label">Saving Throw Proficiency Override</label>
          <select class="override-field__select" :value="abilityOverride.overrideSavingThrowProficiency ?? ''" @change="abilityOverride.overrideSavingThrowProficiency = ($event.target as HTMLSelectElement).value === '' ? null : Number(($event.target as HTMLSelectElement).value)">
            <option value="">Auto</option>
            <option :value="0">Not Proficient</option>
            <option :value="1">Half</option>
            <option :value="2">Proficient</option>
            <option :value="3">Expertise</option>
          </select>
        </div>
      </div>

      <div v-else-if="overrideType === 'skill'" class="override-fields">
        <div class="override-field">
          <label class="override-field__label">Bonus</label>
          <input type="number" class="override-field__input" v-model.number="skillOverride.bonus"/>
        </div>
        <div class="override-field">
          <label class="override-field__label">Proficiency Override</label>
          <select class="override-field__select" :value="skillOverride.overrideProficiency ?? ''" @change="skillOverride.overrideProficiency = ($event.target as HTMLSelectElement).value === '' ? null : Number(($event.target as HTMLSelectElement).value)">
            <option value="">Auto</option>
            <option :value="0">Not Proficient</option>
            <option :value="1">Half</option>
            <option :value="2">Proficient</option>
            <option :value="3">Expertise</option>
          </select>
        </div>
        <div class="override-field">
          <label class="override-field__label">Advantage State</label>
          <select class="override-field__select" v-model.number="skillOverride.advantageState">
            <option :value="0">None</option>
            <option :value="1">Advantage</option>
            <option :value="2">Disadvantage</option>
          </select>
        </div>
      </div>

      <div v-else-if="overrideType === 'ac'" class="override-fields">
        <div class="override-field">
          <label class="override-field__label">Base AC</label>
          <input type="number" class="override-field__input" v-model.number="acOverride.baseArmorClass"/>
        </div>
        <div class="override-field">
          <label class="override-field__label">AC Bonus</label>
          <input type="number" class="override-field__input" v-model.number="acOverride.armorClassBonus"/>
        </div>
      </div>

      <div v-else-if="overrideType === 'initiative'" class="override-fields">
        <div class="override-field">
          <label class="override-field__label">Initiative Bonus</label>
          <input type="number" class="override-field__input" v-model.number="initOverride.initiativeBonus"/>
        </div>
      </div>

      <div v-else-if="overrideType === 'speed'" class="override-fields">
        <div class="override-field">
          <label class="override-field__label">Walking Speed</label>
          <input type="number" class="override-field__input" v-model.number="speedOverride.movementSpeed"/>
        </div>
        <div class="override-field">
          <label class="override-field__label">Swimming Speed</label>
          <input type="number" class="override-field__input" v-model.number="speedOverride.swimmingSpeed"/>
        </div>
        <div class="override-field">
          <label class="override-field__label">Flying Speed</label>
          <input type="number" class="override-field__input" v-model.number="speedOverride.flyingSpeed"/>
        </div>
      </div>

      <div v-else-if="overrideType === 'hp'" class="override-fields">
        <div class="override-field">
          <label class="override-field__label">HP Bonus</label>
          <input type="number" class="override-field__input" v-model.number="hpOverrideForm.hitPointBonus"/>
        </div>
        <div class="override-field">
          <label class="override-field__label">Override Max HP</label>
          <input type="number" class="override-field__input" :value="hpOverrideForm.overriddenMaximumHitPoints ?? ''" @input="hpOverrideForm.overriddenMaximumHitPoints = ($event.target as HTMLInputElement).value === '' ? null : Number(($event.target as HTMLInputElement).value)" placeholder="Leave empty for auto"/>
        </div>
      </div>

      <div v-else-if="overrideType === 'defense'" class="override-fields">
        <div class="defense-list">
          <template v-for="(states, dmgType) in (character?.damageDefenses ?? {})" :key="dmgType">
            <div v-for="(state, idx) in states" :key="`${dmgType}-${idx}`" class="defense-row">
              <span class="chip chip--info">{{ damageTypeLabels[Number(dmgType)] ?? dmgType }} — {{ defenseStateLabels[state] ?? state }}</span>
              <button class="feature-item__remove" @click="removeDefense(String(dmgType), idx)"><i class="fas fa-times"/></button>
            </div>
          </template>
          <div v-if="!Object.keys(character?.damageDefenses ?? {}).length" class="chips__empty">No defenses.</div>
        </div>
        <div class="defense-add-row">
          <select class="override-field__select" v-model.number="defenseNewDamageType">
            <option v-for="(label, id) in damageTypeLabels" :key="id" :value="Number(id)">{{ label }}</option>
          </select>
          <select class="override-field__select" v-model.number="defenseNewState">
            <option :value="1">Resistance</option>
            <option :value="2">Immunity</option>
            <option :value="3">Vulnerability</option>
          </select>
          <button class="inv-coin__btn" @click="addDefense" style="min-width: 2rem; min-height: 2rem"><i class="fas fa-plus"/></button>
        </div>
      </div>

      <template v-if="overrideType !== 'defense'" #footer>
        <div class="override-actions">
          <UiButton @click="saveOverride" :disabled="overrideSaving">
            <i v-if="overrideSaving" class="fas fa-spinner fa-spin"/>
            Save
          </UiButton>
        </div>
      </template>
    </UiModal>

    <UiModal v-model="showStartingEquipModal" title="Starting Equipment" :close-on-backdrop="true" :close-on-esc="true"
             class="inv-modal inv-modal--wide" style="max-width: min(600px, 90vw)">
      <div v-if="startingEquipLoading" class="inv-modal__loading"><i class="fas fa-spinner fa-spin"/> Loading items...
      </div>
      <div v-else class="inv-starting">
        <div v-for="group in getStartingItemChoices()" :key="group.label" class="inv-starting__group">
          <h4 class="inv-starting__group-title">{{ group.label }}</h4>
          <div v-for="choice in group.choices" :key="choice.id" class="inv-starting__choice">
            <div v-if="choice.operator === 0" class="inv-starting__and">
              <div class="inv-starting__and-label">You receive:</div>
              <div v-for="si in (choice.items ?? [])" :key="si.id" class="inv-starting__item">
                <span>{{ getStartItemLabel(si) }}</span>
                <select v-if="needsSelection(si)" class="inv-starting__select"
                        :value="startingEquipWeaponSelections[si.id!] ?? ''"
                        @change="startingEquipWeaponSelections[si.id!] = ($event.target as HTMLSelectElement).value">
                  <option value="">Select...</option>
                  <option
                    v-for="wi in (si.type === 1 ? getItemsForWeaponTypes(si.weaponTypes ?? []) : getItemsForWeaponProperties(si.weaponProperties ?? []))"
                    :key="wi.id" :value="wi.id">{{ wi.name }}
                  </option>
                </select>
              </div>
            </div>
            <div v-else class="inv-starting__or">
              <div class="inv-starting__or-label">Choose one:</div>
              <div v-for="si in (choice.items ?? [])" :key="si.id" class="inv-starting__radio-row">
                <label class="inv-starting__radio">
                  <input type="radio" :name="`choice-${choice.id}`" :value="si.id"
                         :checked="startingEquipSelections[choice.id!] === si.id"
                         @change="startingEquipSelections[choice.id!] = si.id!"/>
                  <span>{{ getStartItemLabel(si) }}</span>
                </label>
                <select v-if="needsSelection(si) && startingEquipSelections[choice.id!] === si.id"
                        class="inv-starting__select" :value="startingEquipWeaponSelections[si.id!] ?? ''"
                        @change="startingEquipWeaponSelections[si.id!] = ($event.target as HTMLSelectElement).value">
                  <option value="">Select...</option>
                  <option
                    v-for="wi in (si.type === 1 ? getItemsForWeaponTypes(si.weaponTypes ?? []) : getItemsForWeaponProperties(si.weaponProperties ?? []))"
                    :key="wi.id" :value="wi.id">{{ wi.name }}
                  </option>
                </select>
              </div>
            </div>
          </div>
        </div>
      </div>
      <template #footer>
        <div class="inv-edit__footer">
          <button class="modal-btn modal-btn--secondary" @click="showStartingEquipModal = false">Cancel</button>
          <UiButton @click="confirmStartingEquipment">Confirm</UiButton>
        </div>
      </template>
    </UiModal>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.sheet {
  max-width: 1180px;
  margin: 0 auto;
  padding: 0 $space-4 $space-8;
  display: flex;
  flex-direction: column;
  gap: $space-4;
}

.sheet__header {
  display: flex;
  align-items: center;
  gap: $space-3;
  padding-top: $space-2;
}

.sheet__back {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;

  &:hover {
    border-color: $color-border-strong;
  }

  &:focus-visible {
    outline: none;
    border-color: $color-accent-alt;
    box-shadow: 0 0 0 2px $color-bg-elevated, 0 0 0 3px $color-focus;
  }
}

.sheet__header-title {
  flex: 1;
  min-width: 0;
}

.sheet__title {
  font-size: 1.55rem;
  font-weight: 900;
  margin: 0;
}

.sheet__meta {
  margin: $space-1 0 0 0;
  color: $color-text-muted;
  display: flex;
  flex-wrap: wrap;
  gap: $space-1;
}

.sheet__meta-line {
  white-space: nowrap;
}

.sheet__loading {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;
}

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
}

.hero__badges {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: $space-2;
}

.hero__right {
  z-index: 1;
}

.quickline {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
  justify-content: flex-end;
}

.quickchip {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  padding: 0.35rem 0.7rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: rgba(5, 8, 20, 0.25);
}

.quickchip__k {
  font-size: 0.8rem;
  color: $color-text-muted;
}

.quickchip__v {
  font-weight: 900;
  color: $color-text;
}

.quickchip--accent {
  border-color: rgba(249, 115, 22, 0.35);
  background: rgba(249, 115, 22, 0.10);
}

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

.survival-mini {
  margin-top: $space-3;
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.survival-mini__row {
  display: flex;
  align-items: center;
  gap: $space-2;
  flex-wrap: wrap;
}

.survival-mini__k {
  color: $color-text-muted;
  font-size: 0.8rem;
}

.survival-mini__v {
  font-weight: 900;
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

  &--green {
    background: #22c55e;
  }

  &--yellow {
    background: #eab308;
  }

  &--red {
    background: #ef4444;
  }
}

.hp-actions {
  display: flex;
  gap: $space-2;
  margin-top: $space-3;
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

  &--heal {
    color: #22c55e;
    border-color: rgba(34, 197, 94, 0.35);

    &:hover {
      background: rgba(34, 197, 94, 0.10);
    }
  }

  &--damage {
    color: #ef4444;
    border-color: rgba(239, 68, 68, 0.35);

    &:hover {
      background: rgba(239, 68, 68, 0.10);
    }
  }

  &--temp {
    color: $color-accent-alt;
    border-color: rgba(56, 189, 248, 0.35);

    &:hover {
      background: rgba(56, 189, 248, 0.10);
    }
  }

  &--lg {
    padding: $space-2 $space-3;
    font-size: 0.85rem;
    flex: 1;
    justify-content: center;
  }
}

.hp-modal :deep(.modal) {
  max-width: 420px;
}

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

  &:focus {
    outline: none;
    border-color: $color-accent;
  }

  &::-webkit-inner-spin-button,
  &::-webkit-outer-spin-button {
    -webkit-appearance: none;
    margin: 0;
  }

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

  &:hover {
    border-color: rgba(251, 191, 36, 0.35);
    color: $color-text;
  }

  i {
    font-size: 1rem;
    color: $color-border-strong;
    transition: color 150ms ease, transform 150ms ease;
  }

  &:hover i {
    transform: scale(1.1);
  }
}

.inspiration-star--active {
  color: #fbbf24 !important;
  filter: drop-shadow(0 0 4px rgba(251, 191, 36, 0.40));
}

@media (max-width: 640px) {
  .combat-card--hp .combat-card__main {
    font-size: 1.35rem;
  }

  .hp-actions { display: none; }

  .hp-bar {
    margin-top: $space-2;
  }
}

.exhaustion-row__label {
  font-size: 0.78rem;
  color: $color-text-muted;
  min-width: 5.5rem;
}

.exhaustion-row__pips {
  display: inline-flex;
  gap: 0.35rem;
}

.exhaustion-pip {
  width: 0.65rem;
  height: 0.65rem;
  border-radius: 999px;
  border: 1px solid $color-border-strong;
  background: rgba(17, 24, 39, 0.6);
  cursor: pointer;
  transition: transform 120ms ease, background 120ms ease;

  &:hover {
    transform: scale(1.3);
  }

  &.on {
    border-color: rgba(249, 115, 22, 0.65);
    background: rgba(249, 115, 22, 0.30);
    box-shadow: 0 0 0 2px rgba(249, 115, 22, 0.10);
  }
}

.inspiration-row {
  display: flex;
  align-items: center;
  gap: $space-2;
  margin-top: $space-2;
  cursor: pointer;
  padding: $space-1 0;

  &:hover .inspiration-star {
    transform: scale(1.2);
  }
}

.inspiration-row__label {
  font-size: 0.78rem;
  color: $color-text-muted;
}

.inspiration-star {
  font-size: 1.1rem;
  color: $color-border-strong;
  transition: transform 150ms ease, color 150ms ease;

  &--active {
    color: #fbbf24;
    filter: drop-shadow(0 0 4px rgba(251, 191, 36, 0.40));
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

  &:hover {
    color: $color-danger;
  }
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

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.conditions-modal :deep(.modal) {
  max-width: 480px;
}

.condition-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: $space-2;

  @media (max-width: 520px) {
    grid-template-columns: repeat(2, 1fr);
  }
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

  &:hover {
    border-color: $color-border-strong;
    color: $color-text;
  }

  &--active {
    border-color: rgba(249, 115, 22, 0.55);
    background: rgba(249, 115, 22, 0.12);
    color: $color-accent;
    font-weight: 600;
  }
}

.main-grid {
  align-items: start;
}

.col {
  display: flex;
  flex-direction: column;
  gap: $space-3;
  min-width: 0;

  &.col-2 {
    grid-column: span 2;
  }
}

.col--empty {
  display: none;
}

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

.coin--gp {
  color: #fbbf24;
}

.coin--sp {
  color: #9ca3af;
}

.coin--cp {
  color: #f59e0b;
}

.coin--ep {
  color: #a7f3d0;
}

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

.divider {
  height: 1px;
  background: rgba(55, 65, 81, 0.6);
  margin: $space-2 0;
}

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

.tabs {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
  padding: $space-2;
  border-radius: $radius-lg;
  border: 1px solid $color-border-subtle;
  background: rgba(5, 8, 20, 0.22);
}

.tabs--secondary {
  margin-top: $space-3;
}

.tab {
  height: 2.2rem;
  padding: 0 $space-3;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text-muted;
  cursor: pointer;
  transition: background-color 150ms ease, border-color 150ms ease, color 150ms ease;

  &:hover {
    color: $color-text;
    background: rgba(249, 115, 22, 0.10);
  }

  &.is-active {
    color: $color-primary-strong;
    background: $color-accent-soft;
    border-color: rgba(249, 115, 22, 0.35);
  }
}

.tabpanel {
  margin-top: $space-2;
  border-radius: $radius-lg;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  overflow: hidden;
}

.tabpanel__body {
  padding: $space-3;
}

.tabpanel__empty {
  color: $color-text-muted;
  padding: $space-4;
  text-align: center;
}

.list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
  max-height: 520px;
  overflow-y: auto;
  padding-right: 2px;
}

.list-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-3;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
}

.list-row__name {
  font-size: 0.92rem;
  color: $color-text;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.list-row__muted {
  color: $color-text-muted;
  font-size: 0.82rem;
  margin-left: $space-2;
}

.list-row__badges {
  display: inline-flex;
  flex-wrap: wrap;
  gap: $space-2;
  justify-content: flex-end;
  flex: 0 0 auto;
}

.feats-list {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
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

.chip--muted {
  color: $color-text-muted;
}

.chip--info {
  border-color: rgba(56, 189, 248, 0.35);
  background: rgba(56, 189, 248, 0.10);
  color: $color-accent-alt;
}

.inv-currency {
  margin-bottom: $space-4;
  padding-bottom: $space-3;
  border-bottom: 1px solid $color-border-subtle;
}

.inv-currency__title {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  margin-bottom: $space-2;
}

.inv-currency__grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: $space-2;

  @media (max-width: 640px) {
    grid-template-columns: repeat(2, 1fr);
  }
}

.inv-coin {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: $space-2;
  padding: $space-2;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: rgba(5, 8, 20, 0.25);
}

.inv-coin__btn {
  width: 1.75rem;
  height: 1.75rem;
  min-width: 1.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  font-size: 0.75rem;
  color: $color-text;

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.inv-coin__val {
  font-weight: 900;
  font-size: 1.1rem;
  min-width: 1.5rem;
  text-align: center;
}

.inv-coin__label {
  font-size: 0.75rem;
  color: $color-text-muted;
  font-weight: 600;
}

.inv-currency__total {
  margin-top: $space-2;
  font-size: 0.8rem;
  color: $color-text-muted;
  text-align: right;
}

.inv-warning {
  display: flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2 $space-3;
  margin-bottom: $space-3;
  background: rgba($color-warning, 0.08);
  border: 1px solid rgba($color-warning, 0.3);
  border-radius: $radius-md;
  font-size: 0.85rem;
  color: $color-warning;
}

.inv-group {
  margin-bottom: $space-3;
}

.inv-group__title {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  margin-bottom: $space-2;
}

.inv-list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.inv-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;

  @media (max-width: 520px) {
    flex-direction: column;
    align-items: flex-start;
  }
}

.inv-row__info {
  display: flex;
  flex-direction: column;
  gap: $space-1;
  min-width: 0;
  flex: 1;
}

.inv-row__name {
  font-size: 0.92rem;
  color: $color-text;
  font-weight: 500;
}

.inv-row__qty {
  color: $color-text-muted;
  font-size: 0.82rem;
  margin-left: $space-1;
}

.inv-row__tags {
  display: flex;
  flex-wrap: wrap;
  gap: $space-1;
  align-items: center;
}

.inv-row__weight {
  font-size: 0.75rem;
  color: $color-text-muted;
}

.inv-row__actions {
  display: flex;
  gap: $space-1;
  flex-shrink: 0;
}

.inv-action {
  width: 2rem;
  height: 2rem;
  min-width: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.8rem;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-border-strong;
    color: $color-text;
  }

  &.is-active {
    border-color: $color-accent;
    color: $color-accent;
    background: rgba($color-accent, 0.05);
  }

  &--danger:hover {
    border-color: $color-danger;
    color: $color-danger;
  }

  &--danger-btn {
    width: auto;
    height: auto;
    padding: $space-2 $space-3;
    gap: $space-2;
    font-size: 0.85rem;
    border-color: rgba($color-danger, 0.3);
    color: $color-danger;

    &:hover {
      background: rgba($color-danger, 0.08);
    }
  }
}

.inv-add-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: $space-2;
  width: 100%;
  padding: $space-3;
  margin-top: $space-3;
  background: transparent;
  border: 1px dashed $color-border-subtle;
  border-radius: $radius-md;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.875rem;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.inv-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: $space-3;
  padding: $space-6;
  text-align: center;
  color: $color-text-muted;
}

.inv-empty__icon {
  font-size: 2.5rem;
  opacity: 0.4;
}

.inv-empty__actions {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
  justify-content: center;
}

.inv-modal :deep(.modal) {
  max-width: 560px;
}

.inv-modal--wide :deep(.modal) {
  max-width: 680px;
}

.inv-modal__search {
  margin-bottom: $space-3;
}

.inv-modal__qty-row {
  display: flex;
  align-items: center;
  gap: $space-2;
  margin-bottom: $space-3;
}

.inv-modal__qty-label {
  font-size: 0.85rem;
  color: $color-text-muted;
}

.inv-modal__qty-val {
  font-weight: 900;
  min-width: 1.5rem;
  text-align: center;
}

.inv-modal__loading {
  text-align: center;
  padding: $space-4;
  color: $color-text-muted;
}

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

  &:hover {
    border-color: $color-accent;
    background: rgba($color-accent, 0.05);
  }
}

.inv-modal__item-name {
  font-weight: 500;
  font-size: 0.9rem;
}

.inv-modal__item-tags {
  display: flex;
  gap: $space-1;
  flex-shrink: 0;
}

.inv-modal__empty {
  text-align: center;
  padding: $space-4;
  color: $color-text-muted;
}

.inv-edit {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.inv-edit__field {
  display: flex;
  flex-direction: column;
  gap: $space-1;
}

.inv-edit__label {
  font-size: 0.8rem;
  color: $color-text-muted;
  font-weight: 600;
  text-transform: uppercase;
}

.inv-edit__spinner {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.inv-edit__toggles {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
}

.inv-toggle {
  display: flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.85rem;
  transition: all 150ms ease;
  min-height: 2.75rem;

  &:hover {
    border-color: $color-border-strong;
    color: $color-text;
  }

  &.is-active {
    border-color: $color-accent;
    color: $color-accent;
    background: rgba($color-accent, 0.05);
  }
}

.inv-edit__footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: $space-2;
}

.inv-starting {
  display: flex;
  flex-direction: column;
  gap: $space-4;
}

.inv-starting__group-title {
  font-size: 0.95rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
}

.inv-starting__choice {
  margin-bottom: $space-3;
  padding: $space-3;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  background: $color-surface-alt;
}

.inv-starting__and-label,
.inv-starting__or-label {
  font-size: 0.8rem;
  color: $color-text-muted;
  font-weight: 600;
  text-transform: uppercase;
  margin-bottom: $space-2;
}

.inv-starting__item {
  display: flex;
  align-items: center;
  gap: $space-2;
  padding: $space-1 0;
  font-size: 0.9rem;
  flex-wrap: wrap;
}

.inv-starting__radio-row {
  display: flex;
  align-items: center;
  gap: $space-2;
  padding: $space-1 0;
  flex-wrap: wrap;
}

.inv-starting__radio {
  display: flex;
  align-items: center;
  gap: $space-2;
  cursor: pointer;
  font-size: 0.9rem;

  input[type="radio"] {
    accent-color: $color-accent;
    width: 1rem;
    height: 1rem;
  }
}

.inv-starting__select {
  padding: $space-1 $space-2;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  background: $color-surface;
  color: $color-text;
  font-size: 0.85rem;
  max-width: 240px;

  &:focus {
    border-color: $color-accent;
    outline: none;
  }
}

.modal-btn {
  padding: $space-2 $space-4;
  border-radius: $radius-md;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 150ms ease;

  &--secondary {
    background: transparent;
    border: 1px solid $color-border-subtle;
    color: $color-text-muted;

    &:hover {
      border-color: $color-border-strong;
      color: $color-text;
    }
  }
}

@media (max-width: 900px) {
  .main-grid :deep(.ui-grid) {
    grid-template-columns: 1fr !important;
  }
}

.bg-tab {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.bg-tab__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-3;
}

.bg-tab__name {
  font-size: 1.1rem;
  font-weight: 700;
  margin: 0;
}

.bg-tab__source {
  font-size: 0.75rem;
  color: $color-text-muted;
  padding: $space-1 $space-2;
  background: $color-surface-alt;
  border-radius: $radius-sm;
}

.bg-tab__section {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.bg-tab__label {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 600;
  margin-top: $space-3;
}

.bg-tab__none {
  color: $color-text-muted;
  padding: $space-4;
  text-align: center;
}

.detail-divider {
  height: 1px;
  background: $color-border-subtle;
  margin: $space-3 0;
}

.notes-tab {
  position: relative;
}

.notes-tab__saving {
  position: absolute;
  top: $space-2;
  right: $space-3;
  font-size: 0.8rem;
  color: $color-text-muted;
}

.notes-tab__section {
  display: flex;
  flex-direction: column;
  gap: $space-2;
  margin-bottom: $space-3;
}

.notes-tab__section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.notes-tab__add {
  width: 1.5rem;
  height: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: 1px dashed $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.7rem;

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.notes-tab__list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.notes-tab__list-row {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.notes-tab__inline-input {
  flex: 1;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text;
  font-size: 0.9rem;
  font-family: inherit;

  &::placeholder {
    color: $color-text-muted;
  }

  &:focus {
    outline: none;
    border-color: $color-accent;
  }

  &--sm {
    flex: 0 0 auto;
    width: 4rem;
    text-align: center;
    padding: $space-2;
  }
}

.notes-tab__remove {
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

  &:hover {
    border-color: $color-danger;
    color: $color-danger;
  }
}

.notes-tab__empty-hint {
  font-size: 0.85rem;
  color: $color-text-muted;
  font-style: italic;
  padding: $space-1 0;
}

.notes-tab__appearance-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: $space-2;
}

.notes-tab__field {
  display: flex;
  flex-direction: column;
  gap: $space-1;
}

.notes-tab__field-label {
  font-size: 0.72rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  font-weight: 600;
}

.notes-tab__field-label-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
}

.notes-tab__unit-toggle {
  padding: 1px $space-1;
  background: transparent;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-xs;
  cursor: pointer;
  font-size: 0.65rem;
  color: $color-text-muted;
  font-weight: 600;
  text-transform: uppercase;

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.notes-tab__height-row {
  display: flex;
  align-items: center;
  gap: $space-1;
}

.notes-tab__unit-label {
  font-size: 0.8rem;
  color: $color-text-muted;
  font-weight: 500;
}

.notes-textarea {
  width: 100%;
  padding: $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text;
  font-size: 0.9rem;
  font-family: inherit;
  line-height: 1.6;
  resize: vertical;

  &::placeholder {
    color: $color-text-muted;
  }

  &:focus {
    outline: none;
    border-color: $color-accent;
  }
}

@media (max-width: 640px) {
  .sheet {
    padding: 0 $space-3 $space-6;
  }
  .sheet__title {
    font-size: 1.25rem;
  }
  .sheet__meta {
    display: none;
  }

  .hero {
    padding: $space-3;
  }
  .hero__left {
    flex-direction: column;
    align-items: center;
  }
  .hero__badges {
    justify-content: center;
  }
  .quickline {
    justify-content: center;
  }
}

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

  &:hover {
    background: rgba(249, 115, 22, 0.04);
  }
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

.feature-item--expanded .feature-item__desc {
  display: block;
}

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

  &:hover {
    border-color: $color-danger;
    color: $color-danger;
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

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
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

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.override-modal :deep(.modal) {
  max-width: 420px;
}

.override-fields {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.override-field {
  display: flex;
  flex-direction: column;
  gap: $space-1;
}

.override-field__label {
  font-size: 0.8rem;
  color: $color-text-muted;
  font-weight: 600;
  text-transform: uppercase;
}

.override-field__input {
  width: 100%;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text;
  font-size: 1rem;
  font-weight: 700;
  font-family: inherit;
  text-align: center;

  &:focus {
    outline: none;
    border-color: $color-accent;
  }

  &::-webkit-inner-spin-button,
  &::-webkit-outer-spin-button {
    -webkit-appearance: none;
    margin: 0;
  }

  -moz-appearance: textfield;
}

.override-field__select {
  padding: $space-2 $space-3;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  background: $color-surface-alt;
  color: $color-text;
  font-size: 0.9rem;
  font-family: inherit;

  &:focus {
    border-color: $color-accent;
    outline: none;
  }
}

.override-actions {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: $space-2;
}

.defense-list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
  margin-bottom: $space-3;
}

.defense-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
}

.defense-add-row {
  display: flex;
  align-items: center;
  gap: $space-2;
  flex-wrap: wrap;
}

.spells-tab {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.spell-class-section {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.spell-class-header {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.spell-class-header__title {
  font-size: 1rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: $color-text-muted;
}

.spell-class-header__sub {
  font-size: 0.85rem;
  color: $color-text-muted;
}

.spell-stats {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.spell-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  background: $color-bg-elevated;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  padding: 0.5rem 1rem;
  min-width: 70px;
}

.spell-stat__label {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: $color-text-muted;
}

.spell-stat__value {
  font-size: 1.1rem;
  font-weight: 600;
}

.spell-level-section {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.spell-level-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.25rem 0;
  border-bottom: 1px solid $color-border-subtle;
  margin-bottom: 0.25rem;
}

.spell-level-header__name {
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: $color-text-muted;
}

.slot-tracker {
  display: flex;
  gap: 0.35rem;
}

.slot-pip {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 2px solid $color-accent;
  background: transparent;
  cursor: pointer;
  padding: 0;
  transition: background 0.15s;

  &--used {
    background: $color-accent;
  }

  &:hover {
    opacity: 0.8;
  }
}

.spell-card {
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  background: $color-bg-elevated;
  overflow: hidden;

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.5rem 0.75rem;
    cursor: pointer;
  }

  &__name {
    font-weight: 500;
  }

  &__badges {
    display: flex;
    gap: 0.35rem;
  }

  &__body {
    display: none;
    padding: 0.5rem 0.75rem 0.75rem;
    border-top: 1px solid $color-border-subtle;
  }

  &--expanded &__body {
    display: block;
  }

  &__meta {
    display: flex;
    gap: 0.75rem;
    flex-wrap: wrap;
    font-size: 0.8rem;
    color: $color-text-muted;
    margin-bottom: 0.5rem;
  }

  &__desc {
    font-size: 0.85rem;
    color: $color-text-muted;
    line-height: 1.6;
    margin-bottom: 0.5rem;
  }

  &__actions {
    display: flex;
    gap: 0.5rem;
    justify-content: flex-end;
  }

  &__action {
    background: $color-surface;
    border: 1px solid $color-border-subtle;
    border-radius: $radius-sm;
    padding: 0.25rem 0.75rem;
    font-size: 0.8rem;
    cursor: pointer;
    color: $color-text;

    &:hover {
      background: $color-accent;
      color: #fff;
    }

    &--danger:hover {
      background: $color-danger;
    }
  }
}

.spell-add-btn {
  align-self: flex-start;
  background: transparent;
  border: 1px dashed $color-border-subtle;
  border-radius: $radius-md;
  padding: 0.5rem 1rem;
  font-size: 0.85rem;
  color: $color-text-muted;
  cursor: pointer;

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.add-spell-modal {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;

  &__list {
    max-height: 350px;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }

  &__item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.5rem 0.75rem;
    border: 1px solid $color-border-subtle;
    border-radius: $radius-sm;
    cursor: pointer;

    &:hover {
      background: $color-bg-elevated;
    }
  }

  &__name {
    font-weight: 500;
  }

  &__info {
    display: flex;
    gap: 0.35rem;
  }

  &__loading,
  &__empty {
    text-align: center;
    color: $color-text-muted;
    padding: 1rem;
    font-size: 0.85rem;
  }
}

.actions-tab {
  display: flex;
  flex-direction: column;
  gap: $space-3;
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

  &--unarmed {
    border-style: dashed;
    opacity: 0.85;
  }

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: $space-2 $space-3;
    border-bottom: 1px solid $color-border-subtle;
  }

  &__name { font-weight: 600; font-size: 0.92rem; }

  &__atk {
    font-weight: 700;
    font-size: 1rem;
    color: $color-accent;
  }

  &__body {
    padding: $space-2 $space-3;
    display: flex;
    flex-direction: column;
    gap: $space-2;
  }

  &__damages { display: flex; gap: $space-3; flex-wrap: wrap; }
  &__damage { font-size: 0.88rem; font-weight: 500; }

  &__meta {
    display: flex;
    align-items: center;
    gap: $space-1;
    flex-wrap: wrap;
  }

  &__range {
    font-size: 0.8rem;
    color: $color-text-muted;
  }
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

.charges-tracker {
  display: flex;
  align-items: center;
  gap: $space-2;
  margin-top: $space-1;

  &__label {
    font-size: 0.72rem;
    color: $color-text-muted;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }
}
</style>