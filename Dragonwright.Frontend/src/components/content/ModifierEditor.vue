<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiCheckboxGroup from '@/components/ui/UiCheckboxGroup.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import {
  modifierTypeOptions,
  bonusTargetOptions,
  proficiencyTargetOptions,
  rollTargetOptions,
  senseTypeOptions,
  movementTypeOptions,
  sizeOptions,
  abilityScoreOptions,
  skillOptions,
  toolOptions,
  damageTypeOptions,
  conditionOptions,
  weaponPropertyOptions,
  weaponTypeOptions,
  masteryOptions,
  creatureTypeOptions,
  timeUnitOptions,
  armorTypeOptions,
} from '@/content/enums'
import {
  type Modifier,
  type ModifierSubtype,
  ModifierType,
  createDefaultModifier,
  createDefaultSubtype,
  getModifierSummary,
} from '@/content/modifiers'

const model = defineModel<Modifier[]>({default: []})

const emit = defineEmits<{
  save: [modifier: Modifier, index: number | null]
  delete: [modifier: Modifier, index: number]
}>()

const modalOpen = ref(false)
const editIndex = ref<number | null>(null)
const modifierForm = ref<Modifier>(createDefaultModifier())

function openAdd() {
  modifierForm.value = createDefaultModifier()
  editIndex.value = null
  modalOpen.value = true
}

function openEdit(index: number) {
  const mod = model.value[index]
  if (!mod) return
  modifierForm.value = JSON.parse(JSON.stringify(mod)) as Modifier
  editIndex.value = index
  modalOpen.value = true
}

function saveModifier() {
  const newList = [...model.value]
  if (editIndex.value !== null) {
    newList[editIndex.value] = { ...modifierForm.value }
  } else {
    newList.push({ ...modifierForm.value })
  }
  model.value = newList
  emit('save', modifierForm.value, editIndex.value)
  modalOpen.value = false
}

function removeModifier(index: number) {
  const mod = model.value[index]
  if (!mod) return
  model.value = model.value.filter((_, i) => i !== index)
  emit('delete', mod, index)
}

watch(
  () => modifierForm.value.type,
  (newType, oldType) => {
    if (newType !== oldType) {
      modifierForm.value.subtype = createDefaultSubtype(newType)
    }
  }
)

const subtype = computed({
  get: () => modifierForm.value.subtype as ModifierSubtype | null,
  set: (val) => { modifierForm.value.subtype = val },
})

const showAbilityScore = computed(() => {
  const t = modifierForm.value.type
  return [ModifierType.Bonus, ModifierType.Damage, ModifierType.Set, ModifierType.StackingBonus].includes(t)
})

const showDice = computed(() => {
  const t = modifierForm.value.type
  return [ModifierType.Bonus, ModifierType.Damage, ModifierType.StackingBonus, ModifierType.Protection].includes(t)
})

const showFixedValue = computed(() => {
  const t = modifierForm.value.type
  return [
    ModifierType.Bonus, ModifierType.Damage, ModifierType.Set, ModifierType.StackingBonus,
    ModifierType.Protection, ModifierType.SetBase, ModifierType.SpeedReduction, ModifierType.SpeedIncrease,
    ModifierType.MeleeWeaponAttack, ModifierType.RangedWeaponAttack,
  ].includes(t)
})
</script>

<template>
  <div class="modifier-editor">
    <div class="modifier-editor__header">
      <span class="modifier-editor__label">Modifiers</span>
      <UiButton size="sm" left-icon="fas fa-plus" @click="openAdd">Add</UiButton>
    </div>

    <table v-if="modelValue" class="content-table content-form__nested-table">
      <thead>
        <tr>
          <th>Type</th>
          <th>Summary</th>
          <th class="content-table__actions-col">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(mod, i) in modelValue" :key="i">
          <td>{{ modifierTypeOptions.find(o => o.value === mod.type)?.label ?? 'Unknown' }}</td>
          <td>{{ getModifierSummary(mod) }}</td>
          <td class="content-table__actions-col">
            <button type="button" class="content-table__action-btn" title="Edit" @click="openEdit(i)">
              <i class="fas fa-pen" />
            </button>
            <button type="button" class="content-table__action-btn content-table__action-btn--danger" title="Delete" @click="removeModifier(i)">
              <i class="fas fa-trash" />
            </button>
          </td>
        </tr>
      </tbody>
    </table>
    <p v-else class="content-form__empty-hint">No modifiers added yet.</p>

    <UiModal v-model="modalOpen" :title="editIndex !== null ? 'Edit Modifier' : 'Add Modifier'" size="lg" close-on-backdrop close-on-esc>
      <div class="modifier-editor__modal-body">
        <UiSelect v-model="modifierForm.type" label="Modifier Type" :options="modifierTypeOptions" />

        <div class="modifier-editor__row">
          <UiInput v-if="showFixedValue" v-model="modifierForm.fixedValue" label="Fixed Value" type="number" placeholder="0" />
          <UiInput v-if="showDice" v-model="modifierForm.diceCount" label="Dice Count" type="number" placeholder="0" />
          <UiInput v-if="showDice" v-model="modifierForm.diceValue" label="Dice Value" type="number" placeholder="0" />
        </div>

        <UiSelect v-if="showAbilityScore" v-model="modifierForm.abilityScore" label="Ability Score" :options="abilityScoreOptions" placeholder="None" />

        <UiTextarea v-model="modifierForm.details" label="Details" placeholder="Additional details..." :rows="2" />

        <div class="modifier-editor__row">
          <UiInput v-model="(modifierForm.duration ??= { count: 0, unit: 0 }).count" label="Duration Count" type="number" placeholder="0" />
          <UiSelect v-model="(modifierForm.duration ??= { count: 0, unit: 0 }).unit" label="Duration Unit" :options="timeUnitOptions" />
        </div>

        <UiCheckbox v-model="modifierForm.applyOnMulticlass" label="Apply on Multiclass" />

        <div v-if="subtype" class="modifier-editor__subtype-section">
          <h4 class="modifier-editor__subtype-title">{{ modifierTypeOptions.find(o => o.value === modifierForm.type)?.label }} Options</h4>

          <template v-if="subtype.$type === 'Bonus'">
            <UiSelect v-model="subtype.target" label="Target" :options="bonusTargetOptions" />
            <UiSelect v-model="subtype.abilityScore" label="Ability Score" :options="abilityScoreOptions" placeholder="None" />
            <UiSelect v-model="subtype.skill" label="Skill" :options="skillOptions" placeholder="None" />
            <UiSelect v-model="subtype.movementType" label="Movement Type" :options="movementTypeOptions" placeholder="None" />
            <UiInput v-model="subtype.spellLevel" label="Spell Level" type="number" placeholder="None" />
          </template>

          <template v-else-if="subtype.$type === 'Damage'">
            <UiSelect v-model="subtype.damageType" label="Damage Type" :options="damageTypeOptions" />
            <UiCheckbox v-model="subtype.onCriticalOnly" label="On Critical Only" />
            <UiCheckbox v-model="subtype.meleeOnly" label="Melee Only" />
            <UiCheckbox v-model="subtype.rangedOnly" label="Ranged Only" />
            <UiCheckbox v-model="subtype.spellOnly" label="Spell Only" />
            <UiCheckbox v-model="subtype.oncePerTurn" label="Once Per Turn" />
          </template>

          <template v-else-if="subtype.$type === 'Advantage'">
            <UiSelect v-model="subtype.target" label="Target" :options="rollTargetOptions" />
            <UiSelect v-model="subtype.abilityScore" label="Ability Score" :options="abilityScoreOptions" placeholder="None" />
            <UiSelect v-model="subtype.skill" label="Skill" :options="skillOptions" placeholder="None" />
            <UiSelect v-model="subtype.againstCreatureType" label="Against Creature Type" :options="creatureTypeOptions" placeholder="Any" />
            <UiSelect v-model="subtype.whenAfflictedByCondition" label="When Afflicted By" :options="conditionOptions" placeholder="Any" />
            <UiCheckbox v-model="subtype.whenHidden" label="When Hidden" />
            <UiCheckbox v-model="subtype.onFirstAttackPerTurn" label="On First Attack Per Turn" />
          </template>

          <template v-else-if="subtype.$type === 'Disadvantage'">
            <UiSelect v-model="subtype.target" label="Target" :options="rollTargetOptions" />
            <UiSelect v-model="subtype.abilityScore" label="Ability Score" :options="abilityScoreOptions" placeholder="None" />
            <UiSelect v-model="subtype.skill" label="Skill" :options="skillOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.whenWithinMeleeRange" label="When Within Melee Range" />
            <UiCheckbox v-model="subtype.whenInSunlight" label="When In Sunlight" />
          </template>

          <template v-else-if="subtype.$type === 'Resistance'">
            <UiCheckboxGroup v-model="subtype.damageTypes" label="Damage Types" :options="damageTypeOptions" />
            <UiCheckbox v-model="subtype.allDamage" label="All Damage" />
            <UiCheckbox v-model="subtype.nonMagicalBludgeoningPiercingSlashing" label="Non-Magical B/P/S" />
          </template>

          <template v-else-if="subtype.$type === 'Immunity'">
            <UiCheckboxGroup v-model="subtype.damageTypes" label="Damage Types" :options="damageTypeOptions" />
            <UiCheckboxGroup v-model="subtype.conditions" label="Conditions" :options="conditionOptions" />
            <UiCheckbox v-model="subtype.disease" label="Disease" />
            <UiCheckbox v-model="subtype.poison" label="Poison" />
            <UiCheckbox v-model="subtype.magicalSleep" label="Magical Sleep" />
          </template>

          <template v-else-if="subtype.$type === 'Vulnerability'">
            <UiCheckboxGroup v-model="subtype.damageTypes" label="Damage Types" :options="damageTypeOptions" />
          </template>

          <template v-else-if="subtype.$type === 'Sense'">
            <UiSelect v-model="subtype.senseType" label="Sense Type" :options="senseTypeOptions" />
            <UiInput v-model="subtype.rangeInFeet" label="Range (feet)" type="number" placeholder="60" />
          </template>

          <template v-else-if="subtype.$type === 'Set'">
            <UiSelect v-model="subtype.target" label="Target" :options="bonusTargetOptions" />
            <UiSelect v-model="subtype.abilityScore" label="Ability Score" :options="abilityScoreOptions" placeholder="None" />
            <UiSelect v-model="subtype.movementType" label="Movement Type" :options="movementTypeOptions" placeholder="None" />
            <UiInput v-model="subtype.value" label="Value" type="number" placeholder="0" />
          </template>

          <template v-else-if="subtype.$type === 'HalfProficiency' || subtype.$type === 'HalfProficiencyRoundUp'">
            <UiSelect v-model="subtype.target" label="Target" :options="proficiencyTargetOptions" />
            <UiSelect v-model="subtype.abilityScore" label="Ability Score" :options="abilityScoreOptions" placeholder="None" />
            <UiSelect v-model="subtype.skill" label="Skill" :options="skillOptions" placeholder="None" />
            <UiSelect v-model="subtype.tool" label="Tool" :options="toolOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.allSkills" label="All Skills" />
            <UiCheckbox v-model="subtype.allAbilityChecks" label="All Ability Checks" />
          </template>

          <template v-else-if="subtype.$type === 'Proficiency'">
            <UiSelect v-model="subtype.target" label="Target" :options="proficiencyTargetOptions" />
            <UiSelect v-model="subtype.skill" label="Skill" :options="skillOptions" placeholder="None" />
            <UiSelect v-model="subtype.tool" label="Tool" :options="toolOptions" placeholder="None" />
            <UiSelect v-model="subtype.savingThrow" label="Saving Throw" :options="abilityScoreOptions" placeholder="None" />
            <UiSelect v-model="subtype.weaponType" label="Weapon Type" :options="weaponTypeOptions" placeholder="None" />
            <UiSelect v-model="subtype.armorType" label="Armor Type" :options="armorTypeOptions" placeholder="None" />
          </template>

          <template v-else-if="subtype.$type === 'Expertise' || subtype.$type === 'TwiceProficiency'">
            <UiSelect v-model="subtype.target" label="Target" :options="proficiencyTargetOptions" />
            <UiSelect v-model="subtype.skill" label="Skill" :options="skillOptions" placeholder="None" />
            <UiSelect v-model="subtype.tool" label="Tool" :options="toolOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.requiresProficiency" label="Requires Proficiency" />
          </template>

          <template v-else-if="subtype.$type === 'Language'">
            <UiCheckbox v-model="subtype.anyLanguage" label="Any Language" />
            <UiInput v-model="subtype.languageCount" label="Language Count" type="number" placeholder="1" />
          </template>

          <template v-else-if="subtype.$type === 'Feat'">
            <UiCheckbox v-model="subtype.anyFeat" label="Any Feat" />
            <UiInput v-model="subtype.maxFeatLevel" label="Max Feat Level" type="number" placeholder="None" />
          </template>

          <template v-else-if="subtype.$type === 'CarryingCapacity'">
            <UiInput v-model="subtype.multiplier" label="Multiplier" type="number" placeholder="1" />
            <UiCheckbox v-model="subtype.countAsSizeLarger" label="Count As Size Larger" />
            <UiCheckbox v-model="subtype.countAsSizeSmaller" label="Count As Size Smaller" />
          </template>

          <template v-else-if="subtype.$type === 'NaturalWeapon'">
            <UiInput v-model="subtype.weaponName" label="Weapon Name" placeholder="Claw" />
            <UiSelect v-model="subtype.damageType" label="Damage Type" :options="damageTypeOptions" />
            <div class="modifier-editor__row">
              <UiInput v-model="subtype.damageDiceCount" label="Dice Count" type="number" placeholder="1" />
              <UiInput v-model="subtype.damageDiceValue" label="Dice Value" type="number" placeholder="4" />
            </div>
            <UiSelect v-model="subtype.abilityScoreOverride" label="Ability Score Override" :options="abilityScoreOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.useStrengthOrDexterity" label="Use Strength or Dexterity" />
            <UiInput v-model="subtype.reachInFeet" label="Reach (feet)" type="number" placeholder="5" />
          </template>

          <template v-else-if="subtype.$type === 'StealthDisadvantage'">
            <UiCheckbox v-model="subtype.fromArmor" label="From Armor" />
          </template>

          <template v-else-if="subtype.$type === 'SpeedReduction'">
            <UiSelect v-model="subtype.movementType" label="Movement Type" :options="movementTypeOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.allMovementTypes" label="All Movement Types" />
            <UiInput v-model="subtype.reductionInFeet" label="Reduction (feet)" type="number" placeholder="0" />
            <UiCheckbox v-model="subtype.setToZero" label="Set To Zero" />
          </template>

          <template v-else-if="subtype.$type === 'SpeedIncrease'">
            <UiSelect v-model="subtype.movementType" label="Movement Type" :options="movementTypeOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.allMovementTypes" label="All Movement Types" />
            <UiInput v-model="subtype.increaseInFeet" label="Increase (feet)" type="number" placeholder="0" />
            <UiCheckbox v-model="subtype.equalToWalkingSpeed" label="Equal To Walking Speed" />
          </template>

          <template v-else-if="subtype.$type === 'MeleeWeaponAttack'">
            <div class="modifier-editor__row">
              <UiInput v-model="subtype.attackBonus" label="Attack Bonus" type="number" placeholder="0" />
              <UiInput v-model="subtype.damageBonus" label="Damage Bonus" type="number" placeholder="0" />
            </div>
            <UiSelect v-model="subtype.additionalDamageType" label="Additional Damage Type" :options="damageTypeOptions" placeholder="None" />
            <div class="modifier-editor__row">
              <UiInput v-model="subtype.additionalDamageDiceCount" label="Add. Dice Count" type="number" placeholder="0" />
              <UiInput v-model="subtype.additionalDamageDiceValue" label="Add. Dice Value" type="number" placeholder="0" />
              <UiInput v-model="subtype.additionalDamageFixed" label="Add. Fixed" type="number" placeholder="0" />
            </div>
            <UiSelect v-model="subtype.requiresWeaponProperty" label="Requires Property" :options="weaponPropertyOptions" placeholder="None" />
          </template>

          <template v-else-if="subtype.$type === 'RangedWeaponAttack'">
            <div class="modifier-editor__row">
              <UiInput v-model="subtype.attackBonus" label="Attack Bonus" type="number" placeholder="0" />
              <UiInput v-model="subtype.damageBonus" label="Damage Bonus" type="number" placeholder="0" />
            </div>
            <UiSelect v-model="subtype.additionalDamageType" label="Additional Damage Type" :options="damageTypeOptions" placeholder="None" />
            <div class="modifier-editor__row">
              <UiInput v-model="subtype.additionalDamageDiceCount" label="Add. Dice Count" type="number" placeholder="0" />
              <UiInput v-model="subtype.additionalDamageDiceValue" label="Add. Dice Value" type="number" placeholder="0" />
              <UiInput v-model="subtype.additionalDamageFixed" label="Add. Fixed" type="number" placeholder="0" />
            </div>
            <UiSelect v-model="subtype.requiresWeaponProperty" label="Requires Property" :options="weaponPropertyOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.ignoreLongRangeDisadvantage" label="Ignore Long Range Disadvantage" />
          </template>

          <template v-else-if="subtype.$type === 'WeaponProperty' || subtype.$type === 'IgnoreWeaponProperty'">
            <UiSelect v-model="subtype.property" label="Property" :options="weaponPropertyOptions" />
            <UiSelect v-model="subtype.restrictToWeaponType" label="Restrict To Weapon Type" :options="weaponTypeOptions" placeholder="Any" />
          </template>

          <template v-else-if="subtype.$type === 'FavoredEnemy'">
            <UiCheckboxGroup v-model="subtype.creatureTypes" label="Creature Types" :options="creatureTypeOptions" />
            <UiCheckbox v-model="subtype.advantageOnSurvivalToTrack" label="Advantage on Survival to Track" />
            <UiCheckbox v-model="subtype.advantageOnIntelligenceToRecall" label="Advantage on Intelligence to Recall" />
          </template>

          <template v-else-if="subtype.$type === 'Ignore'">
            <UiCheckboxGroup v-model="subtype.conditions" label="Conditions" :options="conditionOptions" />
            <UiCheckboxGroup v-model="subtype.damageTypes" label="Damage Types" :options="damageTypeOptions" />
            <UiCheckbox v-model="subtype.difficultTerrain" label="Difficult Terrain" />
            <UiCheckbox v-model="subtype.heavyArmorStealthDisadvantage" label="Heavy Armor Stealth Disadvantage" />
            <UiCheckbox v-model="subtype.heavyArmorStrengthRequirement" label="Heavy Armor Strength Requirement" />
            <UiCheckbox v-model="subtype.loadingProperty" label="Loading Property" />
            <UiCheckbox v-model="subtype.coverBonus" label="Cover Bonus" />
            <UiCheckbox v-model="subtype.halfCover" label="Half Cover" />
            <UiCheckbox v-model="subtype.threeQuartersCover" label="Three-Quarters Cover" />
          </template>

          <template v-else-if="subtype.$type === 'EldrichBlast'">
            <UiInput v-model="subtype.additionalDamageFixed" label="Additional Damage" type="number" placeholder="0" />
            <UiSelect v-model="subtype.additionalDamageAbilityModifier" label="Add Ability Modifier" :options="abilityScoreOptions" placeholder="None" />
            <div class="modifier-editor__row">
              <UiInput v-model="subtype.pushDistanceInFeet" label="Push (feet)" type="number" placeholder="0" />
              <UiInput v-model="subtype.pullDistanceInFeet" label="Pull (feet)" type="number" placeholder="0" />
            </div>
            <UiInput v-model="subtype.additionalRangeInFeet" label="Additional Range (feet)" type="number" placeholder="0" />
            <UiCheckbox v-model="subtype.reduceSpeedByTen" label="Reduce Speed by 10 ft" />
          </template>

          <template v-else-if="subtype.$type === 'ReplaceDamageType'">
            <UiSelect v-model="subtype.originalDamageType" label="Original Damage Type" :options="damageTypeOptions" placeholder="Any" />
            <UiSelect v-model="subtype.newDamageType" label="New Damage Type" :options="damageTypeOptions" />
            <UiCheckbox v-model="subtype.allDamageTypes" label="All Damage Types" />
            <UiCheckbox v-model="subtype.spellsOnly" label="Spells Only" />
            <UiCheckbox v-model="subtype.weaponsOnly" label="Weapons Only" />
          </template>

          <template v-else-if="subtype.$type === 'MonkWeapon'">
            <UiSelect v-model="subtype.weaponType" label="Weapon Type" :options="weaponTypeOptions" placeholder="None" />
            <UiSelect v-model="subtype.requiresProperty" label="Requires Property" :options="weaponPropertyOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.allSimpleMeleeWeapons" label="All Simple Melee Weapons" />
            <UiCheckbox v-model="subtype.shortswords" label="Shortswords" />
          </template>

          <template v-else-if="subtype.$type === 'Protection'">
            <UiCheckboxGroup v-model="subtype.damageTypes" label="Damage Types" :options="damageTypeOptions" />
            <UiCheckbox v-model="subtype.allMagicalDamage" label="All Magical Damage" />
            <UiCheckbox v-model="subtype.allNonMagicalDamage" label="All Non-Magical Damage" />
            <UiInput v-model="subtype.damageReductionFixed" label="Damage Reduction (Fixed)" type="number" placeholder="0" />
            <UiInput v-model="subtype.damageReductionPerCharacterLevel" label="Per Character Level" type="number" placeholder="0" />
            <UiSelect v-model="subtype.damageReductionAbilityModifier" label="Add Ability Modifier" :options="abilityScoreOptions" placeholder="None" />
          </template>

          <template v-else-if="subtype.$type === 'StackingBonus'">
            <UiSelect v-model="subtype.target" label="Target" :options="bonusTargetOptions" />
            <UiSelect v-model="subtype.abilityScore" label="Ability Score" :options="abilityScoreOptions" placeholder="None" />
            <UiSelect v-model="subtype.skill" label="Skill" :options="skillOptions" placeholder="None" />
            <UiSelect v-model="subtype.movementType" label="Movement Type" :options="movementTypeOptions" placeholder="None" />
            <UiInput v-model="subtype.stackGroup" label="Stack Group" placeholder="Group name" />
          </template>

          <template v-else-if="subtype.$type === 'SetBase'">
            <UiSelect v-model="subtype.target" label="Target" :options="bonusTargetOptions" />
            <UiSelect v-model="subtype.abilityScore" label="Ability Score" :options="abilityScoreOptions" placeholder="None" />
            <UiSelect v-model="subtype.movementType" label="Movement Type" :options="movementTypeOptions" placeholder="None" />
            <UiInput v-model="subtype.baseValue" label="Base Value" type="number" placeholder="0" />
            <UiSelect v-model="subtype.addAbilityModifier" label="Add Ability Modifier" :options="abilityScoreOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.addDexterityModifierCapped" label="Add Dex Modifier (Capped)" />
            <UiInput v-if="subtype.addDexterityModifierCapped" v-model="subtype.dexterityModifierCap" label="Dex Modifier Cap" type="number" placeholder="2" />
          </template>

          <template v-else-if="subtype.$type === 'Size'">
            <UiSelect v-model="subtype.size" label="Size" :options="sizeOptions" />
            <UiCheckbox v-model="subtype.isIncrease" label="Is Increase" />
            <UiCheckbox v-model="subtype.isDecrease" label="Is Decrease" />
            <UiInput v-model="subtype.sizeSteps" label="Size Steps" type="number" placeholder="1" />
          </template>

          <template v-else-if="subtype.$type === 'WeaponMastery'">
            <UiSelect v-model="subtype.mastery" label="Mastery" :options="masteryOptions" />
            <UiSelect v-model="subtype.restrictToWeaponType" label="Restrict To Weapon Type" :options="weaponTypeOptions" placeholder="Any" />
            <UiCheckbox v-model="subtype.anyWeaponWithMasteryProperty" label="Any Weapon With Mastery" />
          </template>

          <template v-else-if="subtype.$type === 'EnableFeature'">
            <UiInput v-model="subtype.featureKey" label="Feature Key" placeholder="custom-feature-key" />
            <UiCheckbox v-model="subtype.twoWeaponFighting" label="Two-Weapon Fighting" />
            <UiCheckbox v-model="subtype.unarmoredDefense" label="Unarmored Defense" />
            <UiCheckbox v-model="subtype.jackOfAllTrades" label="Jack of All Trades" />
            <UiCheckbox v-model="subtype.remarkableAthlete" label="Remarkable Athlete" />
            <UiCheckbox v-model="subtype.sneakAttack" label="Sneak Attack" />
            <UiCheckbox v-model="subtype.rageDamageBonus" label="Rage Damage Bonus" />
            <UiCheckbox v-model="subtype.martialArtsDie" label="Martial Arts Die" />
            <UiCheckbox v-model="subtype.wildShape" label="Wild Shape" />
            <UiCheckbox v-model="subtype.channelDivinity" label="Channel Divinity" />
            <UiCheckbox v-model="subtype.extraAttack" label="Extra Attack" />
            <UiInput v-if="subtype.extraAttack" v-model="subtype.extraAttackCount" label="Extra Attack Count" type="number" placeholder="1" />
          </template>

          <template v-else-if="subtype.$type === 'ReplaceWeaponAbility'">
            <UiSelect v-model="subtype.newAbilityScore" label="New Ability Score" :options="abilityScoreOptions" />
            <UiCheckbox v-model="subtype.meleeOnly" label="Melee Only" />
            <UiCheckbox v-model="subtype.rangedOnly" label="Ranged Only" />
            <UiSelect v-model="subtype.restrictToWeaponType" label="Restrict To Weapon Type" :options="weaponTypeOptions" placeholder="Any" />
            <UiSelect v-model="subtype.requiresProperty" label="Requires Property" :options="weaponPropertyOptions" placeholder="None" />
            <UiCheckbox v-model="subtype.useHigherOfStrengthOrDexterity" label="Use Higher of Str/Dex" />
          </template>
        </div>
      </div>

      <template #footer>
        <UiButton @click="saveModifier">Save</UiButton>
        <UiButton variant="ghost" @click="modalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>
  </div>
</template>

<style lang="scss">
.modifier-editor {
  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 0.75rem;
  }

  &__label {
    font-weight: 600;
    font-size: 0.95rem;
    color: var(--color-text);
  }

  &__modal-body {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    max-height: 60vh;
    overflow-y: auto;
    padding-right: 0.5rem;
  }

  &__row {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
    gap: 0.75rem;
  }

  &__subtype-section {
    margin-top: 1rem;
    padding-top: 1rem;
    border-top: 1px solid var(--color-border);
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }

  &__subtype-title {
    font-size: 0.9rem;
    font-weight: 600;
    color: var(--color-text-muted);
    margin: 0 0 0.25rem;
  }
}
</style>
