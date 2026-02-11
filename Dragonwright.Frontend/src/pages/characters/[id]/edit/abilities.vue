<script setup lang="ts">
import { ref, inject, watch, computed } from 'vue'
import { useToast } from '@/composables/useToast'
import { useAutoSave } from '@/composables/useAutoSave'
import { putCharactersIdAbilities } from '@/api'
import { abilityScoreLabels, abilityScoreGenerationLabels } from '@/content/enums'
import UiInput from '@/components/ui/UiInput.vue'

const { showToast } = useToast()

const character = inject<any>('character')
const characterId = inject<any>('characterId')

const STANDARD_ARRAY = [15, 14, 13, 12, 10, 8]
const POINT_BUY_TOTAL = 27
const POINT_BUY_MIN = 8
const POINT_BUY_MAX = 15

const POINT_BUY_COSTS: Record<number, number> = {
  8: 0, 9: 1, 10: 2, 11: 3, 12: 4, 13: 5, 14: 7, 15: 9
}

const abilities = ref<Record<number, number>>({
  0: 10, 1: 10, 2: 10, 3: 10, 4: 10, 5: 10
})

const standardArrayAssignments = ref<Record<number, number | null>>({
  0: null, 1: null, 2: null, 3: null, 4: null, 5: null
})

const generationMethod = computed(() => character.value?.abilityScoreGenerationMethod ?? 1)

const generationMethodLabel = computed(() => abilityScoreGenerationLabels[generationMethod.value] ?? 'Unknown')

const pointsSpent = computed(() => {
  if (generationMethod.value !== 1) return 0
  return Object.values(abilities.value).reduce((total, score) => {
    return total + (POINT_BUY_COSTS[score] ?? 0)
  }, 0)
})

const pointsRemaining = computed(() => POINT_BUY_TOTAL - pointsSpent.value)

const availableStandardArrayValues = computed(() => {
  const used = Object.values(standardArrayAssignments.value).filter(v => v !== null) as number[]
  return STANDARD_ARRAY.filter(v => !used.includes(v))
})

const autoSaveEnabled = ref(false)

const { isSaving } = useAutoSave({
  source: () => ({
    abilities: abilities.value,
    standardArrayAssignments: standardArrayAssignments.value
  }),
  save: saveToBackend,
  delay: 1000,
  enabled: autoSaveEnabled
})

function getModifier(score: number): string {
  const mod = Math.floor((score - 10) / 2)
  return mod >= 0 ? `+${mod}` : `${mod}`
}

function canIncrease(abilityIndex: number): boolean {
  if (generationMethod.value === 3) return true
  if (generationMethod.value === 1) {
    const currentScore = abilities.value[abilityIndex] ?? 8
    if (currentScore >= POINT_BUY_MAX) return false
    const nextCost = (POINT_BUY_COSTS[currentScore + 1] ?? 0) - (POINT_BUY_COSTS[currentScore] ?? 0)
    return nextCost <= pointsRemaining.value
  }
  return false
}

function canDecrease(abilityIndex: number): boolean {
  if (generationMethod.value === 3) return true
  if (generationMethod.value === 1) {
    return (abilities.value[abilityIndex] ?? 8) > POINT_BUY_MIN
  }
  return false
}

function increaseAbility(abilityIndex: number) {
  if (!canIncrease(abilityIndex)) return
  const current = abilities.value[abilityIndex] ?? 8
  abilities.value[abilityIndex] = current + 1
  abilities.value = { ...abilities.value }
}

function decreaseAbility(abilityIndex: number) {
  if (!canDecrease(abilityIndex)) return
  const current = abilities.value[abilityIndex] ?? 8
  abilities.value[abilityIndex] = current - 1
  abilities.value = { ...abilities.value }
}

function setManualScore(abilityIndex: number, value: string) {
  const num = parseInt(value, 10)
  if (!isNaN(num) && num >= 1 && num <= 30) {
    abilities.value[abilityIndex] = num
    abilities.value = { ...abilities.value }
  }
}

function assignStandardArrayValue(abilityIndex: number, value: number | null) {
  if (value === null) {
    standardArrayAssignments.value[abilityIndex] = null
    standardArrayAssignments.value = { ...standardArrayAssignments.value }
    return
  }

  for (const key of Object.keys(standardArrayAssignments.value)) {
    if (standardArrayAssignments.value[Number(key)] === value) {
      standardArrayAssignments.value[Number(key)] = null
    }
  }
  standardArrayAssignments.value[abilityIndex] = value
  standardArrayAssignments.value = { ...standardArrayAssignments.value }
}

function getDisplayScore(abilityIndex: number): number {
  if (generationMethod.value === 0) {
    return standardArrayAssignments.value[abilityIndex] ?? 10
  }
  return abilities.value[abilityIndex] ?? 10
}

function getRollBinding(abilityIndex: number) {
  return {
    modifier: 0,
    onRoll: (_result: number, rawRoll: number) => {
      abilities.value[abilityIndex] = rawRoll
      abilities.value = { ...abilities.value }
    }
  }
}

async function saveToBackend() {
  if (!characterId?.value) return
  try {
    const abilityData = Object.keys(abilities.value).map(key => {
      const abilityIndex = Number(key)
      let rawScore: number

      if (generationMethod.value === 0) {
        rawScore = standardArrayAssignments.value[abilityIndex] ?? 10
      } else {
        rawScore = abilities.value[abilityIndex] ?? 10
      }

      const existing = character.value?.abilities?.find((a: any) => a.ability === abilityIndex)

      return {
        ability: abilityIndex,
        rawScore,
        scoreBonus: existing?.scoreBonus ?? 0,
        rawSavingThrowProficiency: existing?.rawSavingThrowProficiency ?? 0,
        overrideSavingThrowProficiency: existing?.overrideSavingThrowProficiency ?? null,
        savingThrowBonus: existing?.savingThrowBonus ?? 0
      }
    })

    await putCharactersIdAbilities(characterId.value, { abilities: abilityData })
    updateLocalCharacter()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to save abilities.' })
  }
}

function updateLocalCharacter() {
  if (!character.value) return
  character.value.abilities = Object.keys(abilities.value).map(key => {
    const abilityIndex = Number(key)
    let rawScore: number

    if (generationMethod.value === 0) {
      rawScore = standardArrayAssignments.value[abilityIndex] ?? 10
    } else {
      rawScore = abilities.value[abilityIndex] ?? 10
    }

    const existing = character.value?.abilities?.find((a: any) => a.ability === abilityIndex)

    return {
      ability: abilityIndex,
      rawScore,
      scoreBonus: existing?.scoreBonus ?? 0,
      rawSavingThrowProficiency: existing?.rawSavingThrowProficiency ?? 0,
      overrideSavingThrowProficiency: existing?.overrideSavingThrowProficiency ?? null,
      savingThrowBonus: existing?.savingThrowBonus ?? 0
    }
  })
}

function loadFromCharacter() {
  if (!character.value?.abilities) {
    autoSaveEnabled.value = true
    return
  }

  for (const ability of character.value.abilities) {
    const abilityIndex = ability.ability ?? 0
    abilities.value[abilityIndex] = ability.rawScore ?? 10

    if (STANDARD_ARRAY.includes(ability.rawScore)) {
      standardArrayAssignments.value[abilityIndex] = ability.rawScore
    }
  }
  autoSaveEnabled.value = true
}

watch(() => character.value, loadFromCharacter, { immediate: true })
</script>

<template>
  <div class="abilities-stage">
    <div class="abilities-stage__header-row">
      <div>
        <h2 class="abilities-stage__title">Ability Scores</h2>
        <p class="abilities-stage__desc">Set your character's ability scores using {{ generationMethodLabel }}.</p>
      </div>
      <span v-if="isSaving" class="abilities-stage__saving">
        <i class="fas fa-spinner fa-spin" /> Saving...
      </span>
    </div>

    <div v-if="generationMethod === 1" class="abilities-stage__points">
      <div class="points-display">
        <span class="points-display__label">Points Remaining:</span>
        <span class="points-display__value" :class="{ 'is-zero': pointsRemaining === 0, 'is-over': pointsRemaining < 0 }">
          {{ pointsRemaining }} / {{ POINT_BUY_TOTAL }}
        </span>
      </div>
      <p class="abilities-stage__hint">
        Each ability starts at 8. Increasing costs 1 point per level up to 13, then 2 more for 14 (7 total) and 2 more for 15 (9 total).
      </p>
    </div>

    <div v-if="generationMethod === 0" class="abilities-stage__standard-array">
      <div class="standard-array-values">
        <span class="standard-array-values__label">Available Values:</span>
        <span
          v-for="val in availableStandardArrayValues"
          :key="val"
          class="standard-array-value"
        >
          {{ val }}
        </span>
        <span v-if="availableStandardArrayValues.length === 0" class="standard-array-value is-empty">
          All assigned
        </span>
      </div>
    </div>

    <div v-if="generationMethod === 2" class="abilities-stage__roll-hint">
      <p class="abilities-stage__hint">
        Click on a score to roll 1d20 and set it as the ability score. Right-click for advantage/disadvantage.
      </p>
    </div>

    <div class="abilities-grid">
      <div v-for="(label, index) in abilityScoreLabels" :key="index" class="ability-card">
        <div class="ability-card__header">
          <span class="ability-card__name">{{ label }}</span>
          <span class="ability-card__abbrev">{{ label.slice(0, 3).toUpperCase() }}</span>
        </div>

        <div class="ability-card__score">
          <span
            v-if="generationMethod === 2"
            v-roll="getRollBinding(Number(index))"
            class="ability-card__value ability-card__value--rollable"
          >
            {{ getDisplayScore(Number(index)) }}
          </span>
          <span v-else class="ability-card__value">{{ getDisplayScore(Number(index)) }}</span>
          <span class="ability-card__modifier">{{ getModifier(getDisplayScore(Number(index))) }}</span>
        </div>

        <div v-if="generationMethod === 3" class="ability-card__manual">
          <UiInput
            :model-value="String(abilities[Number(index)])"
            type="number"
            min="1"
            max="30"
            class="ability-card__input"
            @update:model-value="setManualScore(Number(index), $event)"
          />
        </div>

        <div v-else-if="generationMethod === 1" class="ability-card__pointbuy">
          <button
            class="ability-card__btn"
            :disabled="!canDecrease(Number(index))"
            @click="decreaseAbility(Number(index))"
          >
            <i class="fas fa-minus" />
          </button>
          <span class="ability-card__cost">
            {{ POINT_BUY_COSTS[abilities[Number(index)] ?? 8] ?? 0 }} pts
          </span>
          <button
            class="ability-card__btn"
            :disabled="!canIncrease(Number(index))"
            @click="increaseAbility(Number(index))"
          >
            <i class="fas fa-plus" />
          </button>
        </div>

        <div v-else-if="generationMethod === 0" class="ability-card__standard">
          <select
            class="ability-card__select"
            :value="standardArrayAssignments[Number(index)] ?? ''"
            @change="assignStandardArrayValue(Number(index), ($event.target as HTMLSelectElement).value ? Number(($event.target as HTMLSelectElement).value) : null)"
          >
            <option value="">Select...</option>
            <option
              v-for="val in STANDARD_ARRAY"
              :key="val"
              :value="val"
              :disabled="!availableStandardArrayValues.includes(val) && standardArrayAssignments[Number(index)] !== val"
            >
              {{ val }}
            </option>
          </select>
        </div>

        <div v-else class="ability-card__readonly">
          <span class="ability-card__readonly-hint">Click score to roll</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.abilities-stage__header-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: $space-4;
}

.abilities-stage__title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0 0 $space-1 0;
}

.abilities-stage__desc {
  color: $color-text-muted;
  margin: 0;
}

.abilities-stage__saving {
  font-size: 0.875rem;
  color: $color-text-muted;
}

.abilities-stage__points {
  margin-bottom: $space-4;
  padding: $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
}

.points-display {
  display: flex;
  align-items: center;
  gap: $space-2;
  margin-bottom: $space-2;
}

.points-display__label {
  font-weight: 600;
}

.points-display__value {
  font-size: 1.25rem;
  font-weight: 700;
  color: $color-accent;

  &.is-zero {
    color: $color-success;
  }

  &.is-over {
    color: $color-danger;
  }
}

.abilities-stage__hint {
  font-size: 0.875rem;
  color: $color-text-muted;
  margin: 0;
}

.abilities-stage__roll-hint {
  margin-bottom: $space-4;
  padding: $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
}

.abilities-stage__standard-array {
  margin-bottom: $space-4;
}

.standard-array-values {
  display: flex;
  align-items: center;
  gap: $space-2;
  flex-wrap: wrap;
}

.standard-array-values__label {
  font-weight: 600;
  font-size: 0.875rem;
}

.standard-array-value {
  padding: $space-1 $space-2;
  background: $color-accent-soft;
  border: 1px solid $color-accent;
  border-radius: $radius-sm;
  font-weight: 600;
  font-size: 0.875rem;
  color: $color-accent;

  &.is-empty {
    background: $color-surface-alt;
    border-color: $color-border-subtle;
    color: $color-text-muted;
  }
}

.abilities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: $space-3;
}

.ability-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: $space-3;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
}

.ability-card__header {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: $space-2;
}

.ability-card__name {
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  color: $color-text-muted;
}

.ability-card__abbrev {
  font-size: 0.625rem;
  color: $color-text-soft;
}

.ability-card__score {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: $space-3;
}

.ability-card__value {
  font-size: 2rem;
  font-weight: 700;
  line-height: 1;

  &--rollable {
    cursor: pointer;
    user-select: none;

    &:hover {
      color: $color-accent;
    }
  }
}

.ability-card__modifier {
  font-size: 1rem;
  font-weight: 600;
  color: $color-accent;
  margin-top: $space-1;
}

.ability-card__manual {
  width: 100%;
}

.ability-card__input {
  text-align: center;
}

.ability-card__pointbuy {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.ability-card__btn {
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  font-size: 0.75rem;
  color: $color-text;

  &:hover:not(:disabled) {
    border-color: $color-accent;
    color: $color-accent;
  }

  &:disabled {
    opacity: 0.3;
    cursor: not-allowed;
  }
}

.ability-card__cost {
  font-size: 0.75rem;
  color: $color-text-muted;
  min-width: 3rem;
  text-align: center;
}

.ability-card__standard {
  width: 100%;
}

.ability-card__select {
  width: 100%;
  padding: $space-2;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-sm;
  color: $color-text;
  font-size: 0.875rem;
  cursor: pointer;

  &:focus {
    outline: none;
    border-color: $color-accent;
  }

  option {
    background: $color-surface;
    color: $color-text;
  }
}

.ability-card__readonly {
  text-align: center;
}

.ability-card__readonly-hint {
  font-size: 0.75rem;
  color: $color-text-muted;
  font-style: italic;
}

@media (max-width: 640px) {
  .abilities-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
