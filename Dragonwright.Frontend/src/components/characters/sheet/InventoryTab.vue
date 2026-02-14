<script setup lang="ts">
import {ref, computed, watch} from "vue";
import {useToast} from "@/composables/useToast";
import {
  putCharactersId, getItems,
  postCharactersIdItems, putCharactersIdItemsItemId, deleteCharactersIdItemsItemId
} from "@/api";
import type {Item, StartItemChoice} from "@/api";
import {
  itemTypeLabels, rarityLabels, weaponTypeLabels, weaponPropertyLabels, currencyLabels
} from "@/content/enums";
import UiButton from "@/components/ui/UiButton.vue";
import UiBadge from "@/components/ui/UiBadge.vue";
import UiModal from "@/components/ui/UiModal.vue";
import UiInput from "@/components/ui/UiInput.vue";

const character = defineModel<any>("character", {required: true});
const props = defineProps<{ characterId: string }>();
const emit = defineEmits<{ refresh: [] }>();

const {showToast} = useToast();

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
    await putCharactersId(props.characterId, {
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

async function toggleEquip(charItem: any) {
  try {
    await putCharactersIdItemsItemId(props.characterId, charItem.id, {
      quantity: charItem.quantity, notes: charItem.notes,
      attuned: charItem.attuned, equipped: !charItem.equipped,
      maxCharges: charItem.maxCharges ?? 0, chargesUsed: charItem.chargesUsed ?? 0,
    });
    charItem.equipped = !charItem.equipped;
  } catch {
    showToast({variant: "danger", message: "Failed to update item."});
  }
}

async function toggleAttune(charItem: any) {
  try {
    await putCharactersIdItemsItemId(props.characterId, charItem.id, {
      quantity: charItem.quantity, notes: charItem.notes,
      attuned: !charItem.attuned, equipped: charItem.equipped,
      maxCharges: charItem.maxCharges ?? 0, chargesUsed: charItem.chargesUsed ?? 0,
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
    const clampedCharges = Math.min(editingItem.value.chargesUsed ?? 0, editMaxCharges.value);
    await putCharactersIdItemsItemId(props.characterId, editingItem.value.id, {
      quantity: editQuantity.value, notes: editNotes.value,
      attuned: editAttuned.value, equipped: editEquipped.value,
      maxCharges: editMaxCharges.value, chargesUsed: clampedCharges,
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
    await deleteCharactersIdItemsItemId(props.characterId, charItem.id);
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
    await postCharactersIdItems(props.characterId, {
      itemId: item.id, quantity: addItemQuantity.value, attuned: false, equipped: false,
    });
    showToast({message: `Added ${item.name}.`});
    addItemQuantity.value = 1;
    emit("refresh");
  } catch {
    showToast({variant: "danger", message: "Failed to add item."});
  }
}

async function toggleCharge(ci: any, delta: number) {
  const max = ci.maxCharges ?? 0;
  const newVal = Math.max(0, Math.min(max, (ci.chargesUsed ?? 0) + delta));
  ci.chargesUsed = newVal;
  try {
    await putCharactersIdItemsItemId(props.characterId, ci.id, {
      quantity: ci.quantity, notes: ci.notes,
      attuned: ci.attuned, equipped: ci.equipped,
      maxCharges: ci.maxCharges, chargesUsed: newVal,
    });
  } catch {
    showToast({variant: "danger", message: "Failed to update charges."});
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
              await postCharactersIdItems(props.characterId, {
                itemId: weaponId, quantity: 1, attuned: false, equipped: false
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
    emit("refresh");
  } catch {
    showToast({variant: "danger", message: "Failed to add starting equipment."});
  } finally {
    startingEquipLoading.value = false;
  }
}

watch(() => character.value, () => {
  syncCurrencyFromCharacter();
}, {immediate: true});
</script>

<template>
  <div class="inv-currency">
    <div class="inv-currency__title">Currency</div>
    <div class="inv-currency__grid">
      <div class="inv-coin" v-for="c in [
        { key: 'gp', label: 'GP', ref: editGp, cls: 'coin--gp' },
        { key: 'sp', label: 'SP', ref: editSp, cls: 'coin--sp' },
        { key: 'cp', label: 'CP', ref: editCp, cls: 'coin--cp' },
        { key: 'ep', label: 'EP', ref: editEp, cls: 'coin--ep' },
      ]" :key="c.key">
        <button class="inv-coin__btn" @click="adjustCurrency(c.key as any, -1)"><i class="fas fa-minus"/></button>
        <span class="inv-coin__val" :class="c.cls">{{ c.ref }}</span>
        <span class="inv-coin__label">{{ c.label }}</span>
        <button class="inv-coin__btn" @click="adjustCurrency(c.key as any, 1)"><i class="fas fa-plus"/></button>
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
                <UiBadge v-if="ci.item?.type !== undefined" :label="itemTypeLabels[ci.item.type] ?? ''" variant="muted"/>
                <UiBadge v-if="ci.item?.rarity !== undefined && ci.item.rarity > 0" :label="rarityLabels[ci.item.rarity] ?? ''" variant="info"/>
                <span v-if="ci.item?.weightInOunces" class="inv-row__weight">{{ (Number(ci.item.weightInOunces) / 16).toFixed(1) }} lb</span>
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
              <button class="inv-action" :class="{ 'is-active': ci.equipped }" title="Equip" @click="toggleEquip(ci)">
                <i class="fas fa-shield-alt"/>
              </button>
              <button v-if="ci.item?.requiresAttunement" class="inv-action" :class="{ 'is-active': ci.attuned }" title="Attune" @click="toggleAttune(ci)">
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

  <UiModal v-model="showAddItemModal" title="Add Item" :close-on-backdrop="true" :close-on-esc="true" class="inv-modal">
    <div class="inv-modal__search">
      <UiInput v-model="addItemSearch" placeholder="Search items..." @input="onAddItemSearchInput"/>
    </div>
    <div class="inv-modal__qty-row">
      <span class="inv-modal__qty-label">Quantity:</span>
      <button class="inv-coin__btn" @click="addItemQuantity = Math.max(1, addItemQuantity - 1)"><i class="fas fa-minus"/></button>
      <span class="inv-modal__qty-val">{{ addItemQuantity }}</span>
      <button class="inv-coin__btn" @click="addItemQuantity++"><i class="fas fa-plus"/></button>
    </div>
    <div v-if="addItemLoading" class="inv-modal__loading"><i class="fas fa-spinner fa-spin"/> Searching...</div>
    <div v-else class="inv-modal__results">
      <button v-for="item in addItemResults" :key="item.id" class="inv-modal__item" @click="addItemToCharacter(item)">
        <span class="inv-modal__item-name">{{ item.name }}</span>
        <div class="inv-modal__item-tags">
          <UiBadge v-if="item.type !== undefined" :label="itemTypeLabels[item.type] ?? ''" variant="muted"/>
          <UiBadge v-if="item.rarity !== undefined && item.rarity > 0" :label="rarityLabels[item.rarity] ?? ''" variant="info"/>
        </div>
      </button>
      <div v-if="!addItemResults.length" class="inv-modal__empty">No items found.</div>
    </div>
  </UiModal>

  <UiModal v-model="showEditItemModal" :title="editingItem?.item?.name ?? 'Edit Item'" :close-on-backdrop="true" :close-on-esc="true" class="inv-modal">
    <div v-if="editingItem" class="inv-edit">
      <div class="inv-edit__field">
        <label class="inv-edit__label">Quantity</label>
        <div class="inv-edit__spinner">
          <button class="inv-coin__btn" @click="editQuantity = Math.max(1, editQuantity - 1)"><i class="fas fa-minus"/></button>
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
        <button v-if="editingItem.item?.requiresAttunement" class="inv-toggle" :class="{ 'is-active': editAttuned }" @click="editAttuned = !editAttuned">
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

  <UiModal v-model="showStartingEquipModal" title="Starting Equipment" :close-on-backdrop="true" :close-on-esc="true"
           class="inv-modal inv-modal--wide" style="max-width: min(600px, 90vw)">
    <div v-if="startingEquipLoading" class="inv-modal__loading"><i class="fas fa-spinner fa-spin"/> Loading items...</div>
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
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.coin--gp { color: #fbbf24; }
.coin--sp { color: #9ca3af; }
.coin--cp { color: #f59e0b; }
.coin--ep { color: #a7f3d0; }

.inv-currency {
  margin-bottom: $space-4;
  padding-bottom: $space-3;
  border-bottom: 1px solid $color-border-subtle;
}

.inv-currency__title {
  font-size: 0.75rem; color: $color-text-muted; text-transform: uppercase;
  letter-spacing: 0.08em; margin-bottom: $space-2;
}

.inv-currency__grid {
  display: grid; grid-template-columns: repeat(4, 1fr); gap: $space-2;
  @media (max-width: 640px) { grid-template-columns: repeat(2, 1fr); }
}

.inv-coin {
  display: flex; align-items: center; justify-content: center; gap: $space-2;
  padding: $space-2; border-radius: $radius-md;
  border: 1px solid $color-border-subtle; background: rgba(5, 8, 20, 0.25);
}

.inv-coin__btn {
  width: 1.75rem; height: 1.75rem; min-width: 1.75rem;
  display: flex; align-items: center; justify-content: center;
  background: $color-surface; border: 1px solid $color-border-subtle;
  border-radius: $radius-sm; cursor: pointer; font-size: 0.75rem; color: $color-text;
  &:hover { border-color: $color-accent; color: $color-accent; }
}

.inv-coin__val { font-weight: 900; font-size: 1.1rem; min-width: 1.5rem; text-align: center; }
.inv-coin__label { font-size: 0.75rem; color: $color-text-muted; font-weight: 600; }
.inv-currency__total { margin-top: $space-2; font-size: 0.8rem; color: $color-text-muted; text-align: right; }

.inv-warning {
  display: flex; align-items: center; gap: $space-2; padding: $space-2 $space-3;
  margin-bottom: $space-3; background: rgba($color-warning, 0.08);
  border: 1px solid rgba($color-warning, 0.3); border-radius: $radius-md;
  font-size: 0.85rem; color: $color-warning;
}

.inv-group { margin-bottom: $space-3; }

.inv-group__title {
  font-size: 0.75rem; color: $color-text-muted; text-transform: uppercase;
  letter-spacing: 0.08em; margin-bottom: $space-2;
}

.inv-list { display: flex; flex-direction: column; gap: $space-2; }

.inv-row {
  display: flex; align-items: center; justify-content: space-between; gap: $space-2;
  padding: $space-2 $space-3; border-radius: $radius-md;
  border: 1px solid $color-border-subtle; background: $color-surface-alt;
  @media (max-width: 520px) { flex-direction: column; align-items: flex-start; }
}

.inv-row__info { display: flex; flex-direction: column; gap: $space-1; min-width: 0; flex: 1; }
.inv-row__name { font-size: 0.92rem; color: $color-text; font-weight: 500; }
.inv-row__qty { color: $color-text-muted; font-size: 0.82rem; margin-left: $space-1; }
.inv-row__tags { display: flex; flex-wrap: wrap; gap: $space-1; align-items: center; }
.inv-row__weight { font-size: 0.75rem; color: $color-text-muted; }
.inv-row__actions { display: flex; gap: $space-1; flex-shrink: 0; }

.inv-action {
  width: 2rem; height: 2rem; min-width: 2rem;
  display: flex; align-items: center; justify-content: center;
  background: $color-surface; border: 1px solid $color-border-subtle;
  border-radius: $radius-sm; cursor: pointer; color: $color-text-muted;
  font-size: 0.8rem; transition: all 150ms ease;
  &:hover { border-color: $color-border-strong; color: $color-text; }
  &.is-active { border-color: $color-accent; color: $color-accent; background: rgba($color-accent, 0.05); }
  &--danger:hover { border-color: $color-danger; color: $color-danger; }
  &--danger-btn {
    width: auto; height: auto; padding: $space-2 $space-3; gap: $space-2;
    font-size: 0.85rem; border-color: rgba($color-danger, 0.3); color: $color-danger;
    &:hover { background: rgba($color-danger, 0.08); }
  }
}

.inv-add-btn {
  display: flex; align-items: center; justify-content: center; gap: $space-2;
  width: 100%; padding: $space-3; margin-top: $space-3;
  background: transparent; border: 1px dashed $color-border-subtle;
  border-radius: $radius-md; cursor: pointer; color: $color-text-muted;
  font-size: 0.875rem; transition: all 150ms ease;
  &:hover { border-color: $color-accent; color: $color-accent; }
}

.inv-empty {
  display: flex; flex-direction: column; align-items: center; gap: $space-3;
  padding: $space-6; text-align: center; color: $color-text-muted;
}
.inv-empty__icon { font-size: 2.5rem; opacity: 0.4; }
.inv-empty__actions { display: flex; flex-wrap: wrap; gap: $space-2; justify-content: center; }

.inv-modal :deep(.modal) { max-width: 560px; }
.inv-modal--wide :deep(.modal) { max-width: 680px; }
.inv-modal__search { margin-bottom: $space-3; }
.inv-modal__qty-row { display: flex; align-items: center; gap: $space-2; margin-bottom: $space-3; }
.inv-modal__qty-label { font-size: 0.85rem; color: $color-text-muted; }
.inv-modal__qty-val { font-weight: 900; min-width: 1.5rem; text-align: center; }
.inv-modal__loading { text-align: center; padding: $space-4; color: $color-text-muted; }

.inv-modal__results {
  max-height: 320px; overflow-y: auto; display: flex; flex-direction: column; gap: $space-1;
}

.inv-modal__item {
  display: flex; align-items: center; justify-content: space-between; gap: $space-2;
  padding: $space-2 $space-3; border-radius: $radius-md;
  border: 1px solid $color-border-subtle; background: $color-surface-alt;
  cursor: pointer; color: $color-text; text-align: left; transition: all 150ms ease;
  &:hover { border-color: $color-accent; background: rgba($color-accent, 0.05); }
}

.inv-modal__item-name { font-weight: 500; font-size: 0.9rem; }
.inv-modal__item-tags { display: flex; gap: $space-1; flex-shrink: 0; }
.inv-modal__empty { text-align: center; padding: $space-4; color: $color-text-muted; }

.inv-edit { display: flex; flex-direction: column; gap: $space-3; }
.inv-edit__field { display: flex; flex-direction: column; gap: $space-1; }
.inv-edit__label { font-size: 0.8rem; color: $color-text-muted; font-weight: 600; text-transform: uppercase; }
.inv-edit__spinner { display: flex; align-items: center; gap: $space-2; }
.inv-edit__toggles { display: flex; flex-wrap: wrap; gap: $space-2; }

.inv-toggle {
  display: flex; align-items: center; gap: $space-2; padding: $space-2 $space-3;
  border-radius: $radius-md; border: 1px solid $color-border-subtle;
  background: $color-surface; cursor: pointer; color: $color-text-muted;
  font-size: 0.85rem; transition: all 150ms ease; min-height: 2.75rem;
  &:hover { border-color: $color-border-strong; color: $color-text; }
  &.is-active { border-color: $color-accent; color: $color-accent; background: rgba($color-accent, 0.05); }
}

.inv-edit__footer { display: flex; justify-content: space-between; align-items: center; gap: $space-2; }

.inv-starting { display: flex; flex-direction: column; gap: $space-4; }
.inv-starting__group-title { font-size: 0.95rem; font-weight: 600; margin: 0 0 $space-2 0; }

.inv-starting__choice {
  margin-bottom: $space-3; padding: $space-3;
  border: 1px solid $color-border-subtle; border-radius: $radius-md; background: $color-surface-alt;
}

.inv-starting__and-label, .inv-starting__or-label {
  font-size: 0.8rem; color: $color-text-muted; font-weight: 600;
  text-transform: uppercase; margin-bottom: $space-2;
}

.inv-starting__item {
  display: flex; align-items: center; gap: $space-2; padding: $space-1 0;
  font-size: 0.9rem; flex-wrap: wrap;
}

.inv-starting__radio-row {
  display: flex; align-items: center; gap: $space-2; padding: $space-1 0; flex-wrap: wrap;
}

.inv-starting__radio {
  display: flex; align-items: center; gap: $space-2; cursor: pointer; font-size: 0.9rem;
  input[type="radio"] { accent-color: $color-accent; width: 1rem; height: 1rem; }
}

.inv-starting__select {
  padding: $space-1 $space-2; border: 1px solid $color-border-subtle;
  border-radius: $radius-sm; background: $color-surface; color: $color-text;
  font-size: 0.85rem; max-width: 240px;
  &:focus { border-color: $color-accent; outline: none; }
}

.modal-btn {
  padding: $space-2 $space-4; border-radius: $radius-md; font-size: 0.875rem;
  font-weight: 500; cursor: pointer; transition: all 150ms ease;
  &--secondary {
    background: transparent; border: 1px solid $color-border-subtle; color: $color-text-muted;
    &:hover { border-color: $color-border-strong; color: $color-text; }
  }
}

.charges-tracker {
  display: flex; align-items: center; gap: $space-2; margin-top: $space-1;
  &__label { font-size: 0.72rem; color: $color-text-muted; text-transform: uppercase; letter-spacing: 0.05em; }
}

.slot-tracker { display: flex; gap: 0.35rem; }

.slot-pip {
  width: 18px; height: 18px; border-radius: 50%;
  border: 2px solid $color-accent; background: transparent;
  cursor: pointer; padding: 0; transition: background 0.15s;
  &--used { background: $color-accent; }
  &:hover { opacity: 0.8; }
}
</style>
