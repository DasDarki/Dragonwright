<script setup lang="ts">
import {ref, computed, onMounted, watch} from "vue";
import {useRoute, useRouter} from "vue-router";
import {useTitle} from "@/composables/useTitle";
import {useToast} from "@/composables/useToast";
import {getCharactersId} from "@/api";
import UiButton from "@/components/ui/UiButton.vue";
import UiGrid from "@/components/ui/layout/UiGrid.vue";
import HeroSection from "@/components/characters/sheet/HeroSection.vue";
import AbilitiesPanel from "@/components/characters/sheet/AbilitiesPanel.vue";
import CombatPanel from "@/components/characters/sheet/CombatPanel.vue";
import SkillsPanel from "@/components/characters/sheet/SkillsPanel.vue";
import SensesPanel from "@/components/characters/sheet/SensesPanel.vue";
import ProficienciesPanel from "@/components/characters/sheet/ProficienciesPanel.vue";
import OverrideModal from "@/components/characters/sheet/OverrideModal.vue";
import RestModal from "@/components/characters/sheet/RestModal.vue";
import ActionsTab from "@/components/characters/sheet/ActionsTab.vue";
import SpellsTab from "@/components/characters/sheet/SpellsTab.vue";
import FeaturesTab from "@/components/characters/sheet/FeaturesTab.vue";
import BackgroundTab from "@/components/characters/sheet/BackgroundTab.vue";
import NotesTab from "@/components/characters/sheet/NotesTab.vue";
import InventoryTab from "@/components/characters/sheet/InventoryTab.vue";

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

const showRestModal = ref(false);
const showOverrideModal = ref(false);
const overrideType = ref("");
const overrideTarget = ref(0);

function openOverride(type: string, targetId?: number) {
  overrideType.value = type;
  overrideTarget.value = targetId ?? 0;
  showOverrideModal.value = true;
}

const hasSpellcasting = computed(() => {
  return (character.value?.classes ?? []).some((cc: any) => cc.subclass?.canCastSpells) || !!(character.value?.spells?.length);
});
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

      <UiButton size="sm" @click="showRestModal = true">
        <i class="fas fa-bed"/> Rest
      </UiButton>
      <UiButton size="sm" @click="goToEdit">
        <i class="fas fa-pen"/> Edit
      </UiButton>
    </div>

    <div v-if="loading" class="sheet__loading">
      <i class="fas fa-spinner fa-spin"/> Loading character...
    </div>

    <template v-else-if="character">
      <HeroSection v-model:character="character" :character-id="characterId"/>

      <AbilitiesPanel :character="character" :open-override="openOverride"/>

      <CombatPanel v-model:character="character" :character-id="characterId" :open-override="openOverride"/>

      <UiGrid :cols="1" :cols-lg="3" :gap="1" class="main-grid">
        <div class="col">
          <SkillsPanel :character="character" :open-override="openOverride"/>
          <SensesPanel :character="character"/>
          <ProficienciesPanel :character="character"/>
        </div>

        <div class="col col-2">
          <div class="tabs">
            <button class="tab" :class="{ 'is-active': activeTab === 'actions' }" @click="activeTab = 'actions'">
              Actions
            </button>
            <button class="tab" :class="{ 'is-active': activeTab === 'features' }" @click="activeTab = 'features'">
              Features
            </button>
            <button v-if="hasSpellcasting" class="tab" :class="{ 'is-active': activeTab === 'spells' }" @click="activeTab = 'spells'">
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
              <ActionsTab :character="character"/>
            </div>

            <div v-else-if="activeTab === 'spells'" class="tabpanel__body">
              <SpellsTab v-model:character="character" :character-id="characterId" @refresh="fetchCharacter"/>
            </div>

            <div v-else-if="activeTab === 'features'" class="tabpanel__body">
              <FeaturesTab v-model:character="character" :character-id="characterId" @refresh="fetchCharacter"/>
            </div>

            <div v-else-if="activeTab === 'background'" class="tabpanel__body">
              <BackgroundTab :character="character"/>
            </div>

            <div v-else-if="activeTab === 'notes'" class="tabpanel__body">
              <NotesTab v-model:character="character" :character-id="characterId"/>
            </div>

            <div v-else-if="activeTab === 'inventory'" class="tabpanel__body">
              <InventoryTab v-model:character="character" :character-id="characterId" @refresh="fetchCharacter"/>
            </div>

            <div v-else class="tabpanel__body">
              <div class="tabpanel__empty">Select a tab.</div>
            </div>
          </div>
        </div>
      </UiGrid>
    </template>

    <OverrideModal
      v-if="character"
      v-model:character="character"
      :character-id="characterId"
      :visible="showOverrideModal"
      :type="overrideType"
      :target="overrideTarget"
      @update:visible="showOverrideModal = $event"
      @refresh="fetchCharacter"
    />

    <RestModal
      v-if="character"
      v-model:character="character"
      :character-id="characterId"
      :visible="showRestModal"
      @update:visible="showRestModal = $event"
      @refresh="fetchCharacter"
    />
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

  &:hover { border-color: $color-border-strong; }

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

.tabs {
  display: flex;
  flex-wrap: wrap;
  gap: $space-2;
  padding: $space-2;
  border-radius: $radius-lg;
  border: 1px solid $color-border-subtle;
  background: rgba(5, 8, 20, 0.22);
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

@media (max-width: 900px) {
  .main-grid :deep(.ui-grid) {
    grid-template-columns: 1fr !important;
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
}
</style>
