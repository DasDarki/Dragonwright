<script setup lang="ts">
import {ref, computed, watch} from "vue";
import {useToast} from "@/composables/useToast";
import {putCharactersId, putCharactersIdAbilities, putCharactersIdSkills} from "@/api";
import type {CharacterAbilityData, CharacterSkillData} from "@/api";
import {abilityScoreLabels, skillLabels, damageTypeLabels, defenseStateLabels} from "@/content/enums";
import UiModal from "@/components/ui/UiModal.vue";
import UiButton from "@/components/ui/UiButton.vue";

const character = defineModel<any>("character", {required: true});

const props = defineProps<{
  characterId: string;
  visible: boolean;
  type: string;
  target: number;
}>();

const emit = defineEmits<{
  "update:visible": [value: boolean];
  refresh: [];
}>();

const {showToast} = useToast();

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
  switch (props.type) {
    case "ability": return `Override ${abilityScoreLabels[props.target] ?? "Ability"}`;
    case "skill": return `Override ${skillLabels[props.target] ?? "Skill"}`;
    case "ac": return "Override Armor Class";
    case "initiative": return "Override Initiative";
    case "speed": return "Override Speed";
    case "hp": return "Override Hit Points";
    case "defense": return "Manage Defenses";
    default: return "Override";
  }
});

watch(() => props.visible, (v) => {
  if (!v || !character.value) return;
  const c = character.value;

  if (props.type === "ability") {
    const ab = c.abilities?.find((a: any) => a.ability === props.target);
    abilityOverride.value = {
      scoreBonus: ab?.scoreBonus ?? 0,
      savingThrowBonus: ab?.savingThrowBonus ?? 0,
      overrideSavingThrowProficiency: ab?.overrideSavingThrowProficiency ?? null,
    };
  } else if (props.type === "skill") {
    const sk = c.skills?.find((s: any) => s.skill === props.target);
    skillOverride.value = {
      bonus: sk?.bonus ?? 0,
      overrideProficiency: sk?.overrideProficiency ?? null,
      advantageState: sk?.advantageState ?? 0,
    };
  } else if (props.type === "ac") {
    acOverride.value = {
      baseArmorClass: c.baseArmorClass ?? 10,
      armorClassBonus: c.armorClassBonus ?? 0,
    };
  } else if (props.type === "initiative") {
    initOverride.value = {initiativeBonus: c.initiativeBonus ?? 0};
  } else if (props.type === "speed") {
    speedOverride.value = {
      movementSpeed: c.movementSpeed ?? 30,
      swimmingSpeed: c.swimmingSpeed ?? 0,
      flyingSpeed: c.flyingSpeed ?? 0,
    };
  } else if (props.type === "hp") {
    hpOverrideForm.value = {
      hitPointBonus: c.hitPointBonus ?? 0,
      overriddenMaximumHitPoints: c.overriddenMaximumHitPoints ?? null,
    };
  }
});

function closeModal() {
  emit("update:visible", false);
}

async function saveAbilityOverride() {
  if (!character.value) return;
  overrideSaving.value = true;
  try {
    const abilities: CharacterAbilityData[] = (character.value.abilities ?? []).map((a: any) => {
      if (a.ability === props.target) {
        return {
          ability: a.ability, rawScore: a.rawScore,
          scoreBonus: abilityOverride.value.scoreBonus,
          rawSavingThrowProficiency: a.rawSavingThrowProficiency,
          overrideSavingThrowProficiency: abilityOverride.value.overrideSavingThrowProficiency,
          savingThrowBonus: abilityOverride.value.savingThrowBonus,
        };
      }
      return {
        ability: a.ability, rawScore: a.rawScore, scoreBonus: a.scoreBonus,
        rawSavingThrowProficiency: a.rawSavingThrowProficiency,
        overrideSavingThrowProficiency: a.overrideSavingThrowProficiency,
        savingThrowBonus: a.savingThrowBonus,
      };
    });
    await putCharactersIdAbilities(props.characterId, {abilities});
    closeModal();
    showToast({message: "Ability override saved."});
    emit("refresh");
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
      if (s.skill === props.target) {
        return {
          skill: s.skill, bonus: skillOverride.value.bonus, rawProficiency: s.rawProficiency,
          overrideProficiency: skillOverride.value.overrideProficiency,
          advantageState: skillOverride.value.advantageState,
        };
      }
      return {
        skill: s.skill, bonus: s.bonus, rawProficiency: s.rawProficiency,
        overrideProficiency: s.overrideProficiency, advantageState: s.advantageState,
      };
    });
    await putCharactersIdSkills(props.characterId, {skills});
    closeModal();
    showToast({message: "Skill override saved."});
    emit("refresh");
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
    await putCharactersId(props.characterId, {
      name: character.value.name,
      baseArmorClass: acOverride.value.baseArmorClass,
      armorClassBonus: acOverride.value.armorClassBonus,
    });
    closeModal();
    showToast({message: "AC override saved."});
    emit("refresh");
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
    await putCharactersId(props.characterId, {
      name: character.value.name,
      initiativeBonus: initOverride.value.initiativeBonus,
    });
    closeModal();
    showToast({message: "Initiative override saved."});
    emit("refresh");
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
    await putCharactersId(props.characterId, {
      name: character.value.name,
      movementSpeed: speedOverride.value.movementSpeed,
      swimmingSpeed: speedOverride.value.swimmingSpeed,
      flyingSpeed: speedOverride.value.flyingSpeed,
    });
    closeModal();
    showToast({message: "Speed override saved."});
    emit("refresh");
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
    await putCharactersId(props.characterId, {
      name: character.value.name,
      hitPointBonus: hpOverrideForm.value.hitPointBonus,
      overriddenMaximumHitPoints: hpOverrideForm.value.overriddenMaximumHitPoints,
    });
    closeModal();
    showToast({message: "HP override saved."});
    emit("refresh");
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
    await putCharactersId(props.characterId, {
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
    await putCharactersId(props.characterId, {
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
  switch (props.type) {
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
  <UiModal :model-value="visible" @update:model-value="emit('update:visible', $event)" :title="overrideTitle" :close-on-backdrop="true" :close-on-esc="true" class="override-modal">
    <div v-if="type === 'ability'" class="override-fields">
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

    <div v-else-if="type === 'skill'" class="override-fields">
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

    <div v-else-if="type === 'ac'" class="override-fields">
      <div class="override-field">
        <label class="override-field__label">Base AC</label>
        <input type="number" class="override-field__input" v-model.number="acOverride.baseArmorClass"/>
      </div>
      <div class="override-field">
        <label class="override-field__label">AC Bonus</label>
        <input type="number" class="override-field__input" v-model.number="acOverride.armorClassBonus"/>
      </div>
    </div>

    <div v-else-if="type === 'initiative'" class="override-fields">
      <div class="override-field">
        <label class="override-field__label">Initiative Bonus</label>
        <input type="number" class="override-field__input" v-model.number="initOverride.initiativeBonus"/>
      </div>
    </div>

    <div v-else-if="type === 'speed'" class="override-fields">
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

    <div v-else-if="type === 'hp'" class="override-fields">
      <div class="override-field">
        <label class="override-field__label">HP Bonus</label>
        <input type="number" class="override-field__input" v-model.number="hpOverrideForm.hitPointBonus"/>
      </div>
      <div class="override-field">
        <label class="override-field__label">Override Max HP</label>
        <input type="number" class="override-field__input" :value="hpOverrideForm.overriddenMaximumHitPoints ?? ''" @input="hpOverrideForm.overriddenMaximumHitPoints = ($event.target as HTMLInputElement).value === '' ? null : Number(($event.target as HTMLInputElement).value)" placeholder="Leave empty for auto"/>
      </div>
    </div>

    <div v-else-if="type === 'defense'" class="override-fields">
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

    <template v-if="type !== 'defense'" #footer>
      <div class="override-actions">
        <UiButton @click="saveOverride" :disabled="overrideSaving">
          <i v-if="overrideSaving" class="fas fa-spinner fa-spin"/>
          Save
        </UiButton>
      </div>
    </template>
  </UiModal>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

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

.chips__empty {
  color: $color-text-muted;
  font-size: 0.9rem;
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
</style>
