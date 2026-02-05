export enum ModifierType {
  Bonus = 0,
  Damage = 1,
  Advantage = 2,
  Disadvantage = 3,
  Resistance = 4,
  Immunity = 5,
  Vulnerability = 6,
  Sense = 7,
  Set = 8,
  HalfProficiency = 9,
  Proficiency = 10,
  Expertise = 11,
  Language = 12,
  Feat = 13,
  CarryingCapacity = 14,
  NaturalWeapon = 15,
  StealthDisadvantage = 16,
  SpeedReduction = 17,
  SpeedIncrease = 18,
  MeleeWeaponAttack = 19,
  RangedWeaponAttack = 20,
  WeaponProperty = 21,
  HalfProficiencyRoundUp = 22,
  FavoredEnemy = 23,
  Ignore = 24,
  EldritchBlast = 25,
  ReplaceDamageType = 26,
  TwiceProficiency = 27,
  MonkWeapon = 28,
  Protection = 29,
  StackingBonus = 30,
  SetBase = 31,
  IgnoreWeaponProperty = 32,
  Size = 33,
  WeaponMastery = 34,
  EnableFeature = 35,
  ReplaceWeaponAbility = 36,
}

export interface BonusSubtype {
  $type: 'Bonus'
  target: number
  abilityScore?: number | null
  skill?: number | null
  movementType?: number | null
  spellLevel?: number | null
}

export interface DamageSubtype {
  $type: 'Damage'
  damageType: number
  onCriticalOnly: boolean
  meleeOnly: boolean
  rangedOnly: boolean
  spellOnly: boolean
  oncePerTurn: boolean
  restrictToWeaponId?: string | null
}

export interface AdvantageSubtype {
  $type: 'Advantage'
  target: number
  abilityScore?: number | null
  skill?: number | null
  againstCreatureType?: number | null
  whenAfflictedByCondition?: number | null
  whenHidden: boolean
  onFirstAttackPerTurn: boolean
}

export interface DisadvantageSubtype {
  $type: 'Disadvantage'
  target: number
  abilityScore?: number | null
  skill?: number | null
  whenWithinMeleeRange: boolean
  whenInSunlight: boolean
}

export interface ResistanceSubtype {
  $type: 'Resistance'
  damageTypes: number[]
  allDamage: boolean
  nonMagicalBludgeoningPiercingSlashing: boolean
}

export interface ImmunitySubtype {
  $type: 'Immunity'
  damageTypes: number[]
  conditions: number[]
  disease: boolean
  poison: boolean
  magicalSleep: boolean
}

export interface VulnerabilitySubtype {
  $type: 'Vulnerability'
  damageTypes: number[]
}

export interface SenseSubtype {
  $type: 'Sense'
  senseType: number
  rangeInFeet: number
}

export interface SetSubtype {
  $type: 'Set'
  target: number
  abilityScore?: number | null
  movementType?: number | null
  value: number
}

export interface HalfProficiencySubtype {
  $type: 'HalfProficiency'
  target: number
  abilityScore?: number | null
  skill?: number | null
  tool?: number | null
  allSkills: boolean
  allAbilityChecks: boolean
}

export interface ProficiencySubtype {
  $type: 'Proficiency'
  target: number
  skill?: number | null
  tool?: number | null
  savingThrow?: number | null
  weaponType?: number | null
  specificWeaponId?: string | null
  armorType?: number | null
}

export interface ExpertiseSubtype {
  $type: 'Expertise'
  target: number
  skill?: number | null
  tool?: number | null
  requiresProficiency: boolean
}

export interface LanguageSubtype {
  $type: 'Language'
  languageId?: string | null
  anyLanguage: boolean
  languageCount: number
}

export interface FeatSubtype {
  $type: 'Feat'
  featId?: string | null
  anyFeat: boolean
  maxFeatLevel?: number | null
}

export interface CarryingCapacitySubtype {
  $type: 'CarryingCapacity'
  multiplier: number
  countAsSizeLarger: boolean
  countAsSizeSmaller: boolean
}

export interface NaturalWeaponSubtype {
  $type: 'NaturalWeapon'
  weaponName: string
  damageType: number
  damageDiceCount: number
  damageDiceValue: number
  abilityScoreOverride?: number | null
  useStrengthOrDexterity: boolean
  reachInFeet: number
}

export interface StealthDisadvantageSubtype {
  $type: 'StealthDisadvantage'
  fromArmor: boolean
}

export interface SpeedReductionSubtype {
  $type: 'SpeedReduction'
  movementType?: number | null
  allMovementTypes: boolean
  reductionInFeet: number
  setToZero: boolean
}

export interface SpeedIncreaseSubtype {
  $type: 'SpeedIncrease'
  movementType?: number | null
  allMovementTypes: boolean
  increaseInFeet: number
  equalToWalkingSpeed: boolean
}

export interface MeleeWeaponAttackSubtype {
  $type: 'MeleeWeaponAttack'
  attackBonus: number
  damageBonus: number
  additionalDamageType?: number | null
  additionalDamageDiceCount: number
  additionalDamageDiceValue: number
  additionalDamageFixed: number
  requiresWeaponProperty?: number | null
  restrictToWeaponId?: string | null
}

export interface RangedWeaponAttackSubtype {
  $type: 'RangedWeaponAttack'
  attackBonus: number
  damageBonus: number
  additionalDamageType?: number | null
  additionalDamageDiceCount: number
  additionalDamageDiceValue: number
  additionalDamageFixed: number
  requiresWeaponProperty?: number | null
  restrictToWeaponId?: string | null
  ignoreLongRangeDisadvantage: boolean
}

export interface WeaponPropertySubtype {
  $type: 'WeaponProperty'
  property: number
  restrictToWeaponId?: string | null
  restrictToWeaponType?: number | null
}

export interface HalfProficiencyRoundUpSubtype {
  $type: 'HalfProficiencyRoundUp'
  target: number
  abilityScore?: number | null
  skill?: number | null
  tool?: number | null
  allSkills: boolean
  allAbilityChecks: boolean
}

export interface FavoredEnemySubtype {
  $type: 'FavoredEnemy'
  creatureTypes: number[]
  advantageOnSurvivalToTrack: boolean
  advantageOnIntelligenceToRecall: boolean
  languageId?: string | null
}

export interface IgnoreSubtype {
  $type: 'Ignore'
  conditions: number[]
  damageTypes: number[]
  difficultTerrain: boolean
  heavyArmorStealthDisadvantage: boolean
  heavyArmorStrengthRequirement: boolean
  loadingProperty: boolean
  coverBonus: boolean
  halfCover: boolean
  threeQuartersCover: boolean
}

export interface EldritchBlastSubtype {
  $type: 'EldrichBlast'
  additionalDamageFixed: number
  additionalDamageAbilityModifier?: number | null
  pushDistanceInFeet: number
  pullDistanceInFeet: number
  additionalRangeInFeet: number
  reduceSpeedByTen: boolean
}

export interface ReplaceDamageTypeSubtype {
  $type: 'ReplaceDamageType'
  originalDamageType?: number | null
  newDamageType: number
  allDamageTypes: boolean
  spellsOnly: boolean
  weaponsOnly: boolean
}

export interface TwiceProficiencySubtype {
  $type: 'TwiceProficiency'
  target: number
  skill?: number | null
  tool?: number | null
  abilityScore?: number | null
  requiresProficiency: boolean
}

export interface MonkWeaponSubtype {
  $type: 'MonkWeapon'
  weaponId?: string | null
  weaponType?: number | null
  requiresProperty?: number | null
  allSimpleMeleeWeapons: boolean
  shortswords: boolean
}

export interface ProtectionSubtype {
  $type: 'Protection'
  damageTypes: number[]
  allMagicalDamage: boolean
  allNonMagicalDamage: boolean
  damageReductionFixed: number
  damageReductionPerCharacterLevel: number
  damageReductionAbilityModifier?: number | null
}

export interface StackingBonusSubtype {
  $type: 'StackingBonus'
  target: number
  abilityScore?: number | null
  skill?: number | null
  movementType?: number | null
  stackGroup: string
}

export interface SetBaseSubtype {
  $type: 'SetBase'
  target: number
  abilityScore?: number | null
  movementType?: number | null
  baseValue: number
  addAbilityModifier?: number | null
  addDexterityModifierCapped: boolean
  dexterityModifierCap: number
}

export interface IgnoreWeaponPropertySubtype {
  $type: 'IgnoreWeaponProperty'
  property: number
  restrictToWeaponId?: string | null
  restrictToWeaponType?: number | null
}

export interface SizeSubtype {
  $type: 'Size'
  size: number
  isIncrease: boolean
  isDecrease: boolean
  sizeSteps: number
}

export interface WeaponMasterySubtype {
  $type: 'WeaponMastery'
  mastery: number
  restrictToWeaponId?: string | null
  restrictToWeaponType?: number | null
  anyWeaponWithMasteryProperty: boolean
}

export interface EnableFeatureSubtype {
  $type: 'EnableFeature'
  featureKey: string
  twoWeaponFighting: boolean
  unarmoredDefense: boolean
  jackOfAllTrades: boolean
  remarkableAthlete: boolean
  sneakAttack: boolean
  rageDamageBonus: boolean
  martialArtsDie: boolean
  wildShape: boolean
  channelDivinity: boolean
  extraAttack: boolean
  extraAttackCount: number
}

export interface ReplaceWeaponAbilitySubtype {
  $type: 'ReplaceWeaponAbility'
  newAbilityScore: number
  meleeOnly: boolean
  rangedOnly: boolean
  restrictToWeaponId?: string | null
  restrictToWeaponType?: number | null
  requiresProperty?: number | null
  useHigherOfStrengthOrDexterity: boolean
}

export type ModifierSubtype =
  | BonusSubtype
  | DamageSubtype
  | AdvantageSubtype
  | DisadvantageSubtype
  | ResistanceSubtype
  | ImmunitySubtype
  | VulnerabilitySubtype
  | SenseSubtype
  | SetSubtype
  | HalfProficiencySubtype
  | ProficiencySubtype
  | ExpertiseSubtype
  | LanguageSubtype
  | FeatSubtype
  | CarryingCapacitySubtype
  | NaturalWeaponSubtype
  | StealthDisadvantageSubtype
  | SpeedReductionSubtype
  | SpeedIncreaseSubtype
  | MeleeWeaponAttackSubtype
  | RangedWeaponAttackSubtype
  | WeaponPropertySubtype
  | HalfProficiencyRoundUpSubtype
  | FavoredEnemySubtype
  | IgnoreSubtype
  | EldritchBlastSubtype
  | ReplaceDamageTypeSubtype
  | TwiceProficiencySubtype
  | MonkWeaponSubtype
  | ProtectionSubtype
  | StackingBonusSubtype
  | SetBaseSubtype
  | IgnoreWeaponPropertySubtype
  | SizeSubtype
  | WeaponMasterySubtype
  | EnableFeatureSubtype
  | ReplaceWeaponAbilitySubtype

export interface Modifier {
  id?: string
  type: ModifierType
  subtype?: ModifierSubtype | null
  abilityScore?: number | null
  diceCount: number
  diceValue: number
  fixedValue: number
  details: string
  duration?: { count: number; unit: number } | null
  applyOnMulticlass: boolean
}

export function createDefaultSubtype(type: ModifierType): ModifierSubtype {
  switch (type) {
    case ModifierType.Bonus:
      return { $type: 'Bonus', target: 0, abilityScore: null, skill: null, movementType: null, spellLevel: null }
    case ModifierType.Damage:
      return { $type: 'Damage', damageType: 0, onCriticalOnly: false, meleeOnly: false, rangedOnly: false, spellOnly: false, oncePerTurn: false, restrictToWeaponId: null }
    case ModifierType.Advantage:
      return { $type: 'Advantage', target: 0, abilityScore: null, skill: null, againstCreatureType: null, whenAfflictedByCondition: null, whenHidden: false, onFirstAttackPerTurn: false }
    case ModifierType.Disadvantage:
      return { $type: 'Disadvantage', target: 0, abilityScore: null, skill: null, whenWithinMeleeRange: false, whenInSunlight: false }
    case ModifierType.Resistance:
      return { $type: 'Resistance', damageTypes: [], allDamage: false, nonMagicalBludgeoningPiercingSlashing: false }
    case ModifierType.Immunity:
      return { $type: 'Immunity', damageTypes: [], conditions: [], disease: false, poison: false, magicalSleep: false }
    case ModifierType.Vulnerability:
      return { $type: 'Vulnerability', damageTypes: [] }
    case ModifierType.Sense:
      return { $type: 'Sense', senseType: 0, rangeInFeet: 60 }
    case ModifierType.Set:
      return { $type: 'Set', target: 0, abilityScore: null, movementType: null, value: 0 }
    case ModifierType.HalfProficiency:
      return { $type: 'HalfProficiency', target: 0, abilityScore: null, skill: null, tool: null, allSkills: false, allAbilityChecks: false }
    case ModifierType.Proficiency:
      return { $type: 'Proficiency', target: 0, skill: null, tool: null, savingThrow: null, weaponType: null, specificWeaponId: null, armorType: null }
    case ModifierType.Expertise:
      return { $type: 'Expertise', target: 0, skill: null, tool: null, requiresProficiency: true }
    case ModifierType.Language:
      return { $type: 'Language', languageId: null, anyLanguage: false, languageCount: 1 }
    case ModifierType.Feat:
      return { $type: 'Feat', featId: null, anyFeat: false, maxFeatLevel: null }
    case ModifierType.CarryingCapacity:
      return { $type: 'CarryingCapacity', multiplier: 1, countAsSizeLarger: false, countAsSizeSmaller: false }
    case ModifierType.NaturalWeapon:
      return { $type: 'NaturalWeapon', weaponName: '', damageType: 0, damageDiceCount: 1, damageDiceValue: 4, abilityScoreOverride: null, useStrengthOrDexterity: false, reachInFeet: 5 }
    case ModifierType.StealthDisadvantage:
      return { $type: 'StealthDisadvantage', fromArmor: false }
    case ModifierType.SpeedReduction:
      return { $type: 'SpeedReduction', movementType: null, allMovementTypes: false, reductionInFeet: 0, setToZero: false }
    case ModifierType.SpeedIncrease:
      return { $type: 'SpeedIncrease', movementType: null, allMovementTypes: false, increaseInFeet: 0, equalToWalkingSpeed: false }
    case ModifierType.MeleeWeaponAttack:
      return { $type: 'MeleeWeaponAttack', attackBonus: 0, damageBonus: 0, additionalDamageType: null, additionalDamageDiceCount: 0, additionalDamageDiceValue: 0, additionalDamageFixed: 0, requiresWeaponProperty: null, restrictToWeaponId: null }
    case ModifierType.RangedWeaponAttack:
      return { $type: 'RangedWeaponAttack', attackBonus: 0, damageBonus: 0, additionalDamageType: null, additionalDamageDiceCount: 0, additionalDamageDiceValue: 0, additionalDamageFixed: 0, requiresWeaponProperty: null, restrictToWeaponId: null, ignoreLongRangeDisadvantage: false }
    case ModifierType.WeaponProperty:
      return { $type: 'WeaponProperty', property: 0, restrictToWeaponId: null, restrictToWeaponType: null }
    case ModifierType.HalfProficiencyRoundUp:
      return { $type: 'HalfProficiencyRoundUp', target: 0, abilityScore: null, skill: null, tool: null, allSkills: false, allAbilityChecks: false }
    case ModifierType.FavoredEnemy:
      return { $type: 'FavoredEnemy', creatureTypes: [], advantageOnSurvivalToTrack: true, advantageOnIntelligenceToRecall: true, languageId: null }
    case ModifierType.Ignore:
      return { $type: 'Ignore', conditions: [], damageTypes: [], difficultTerrain: false, heavyArmorStealthDisadvantage: false, heavyArmorStrengthRequirement: false, loadingProperty: false, coverBonus: false, halfCover: false, threeQuartersCover: false }
    case ModifierType.EldritchBlast:
      return { $type: 'EldrichBlast', additionalDamageFixed: 0, additionalDamageAbilityModifier: null, pushDistanceInFeet: 0, pullDistanceInFeet: 0, additionalRangeInFeet: 0, reduceSpeedByTen: false }
    case ModifierType.ReplaceDamageType:
      return { $type: 'ReplaceDamageType', originalDamageType: null, newDamageType: 0, allDamageTypes: false, spellsOnly: false, weaponsOnly: false }
    case ModifierType.TwiceProficiency:
      return { $type: 'TwiceProficiency', target: 0, skill: null, tool: null, abilityScore: null, requiresProficiency: true }
    case ModifierType.MonkWeapon:
      return { $type: 'MonkWeapon', weaponId: null, weaponType: null, requiresProperty: null, allSimpleMeleeWeapons: false, shortswords: false }
    case ModifierType.Protection:
      return { $type: 'Protection', damageTypes: [], allMagicalDamage: false, allNonMagicalDamage: false, damageReductionFixed: 0, damageReductionPerCharacterLevel: 0, damageReductionAbilityModifier: null }
    case ModifierType.StackingBonus:
      return { $type: 'StackingBonus', target: 0, abilityScore: null, skill: null, movementType: null, stackGroup: '' }
    case ModifierType.SetBase:
      return { $type: 'SetBase', target: 0, abilityScore: null, movementType: null, baseValue: 0, addAbilityModifier: null, addDexterityModifierCapped: false, dexterityModifierCap: 0 }
    case ModifierType.IgnoreWeaponProperty:
      return { $type: 'IgnoreWeaponProperty', property: 0, restrictToWeaponId: null, restrictToWeaponType: null }
    case ModifierType.Size:
      return { $type: 'Size', size: 2, isIncrease: false, isDecrease: false, sizeSteps: 1 }
    case ModifierType.WeaponMastery:
      return { $type: 'WeaponMastery', mastery: 0, restrictToWeaponId: null, restrictToWeaponType: null, anyWeaponWithMasteryProperty: false }
    case ModifierType.EnableFeature:
      return { $type: 'EnableFeature', featureKey: '', twoWeaponFighting: false, unarmoredDefense: false, jackOfAllTrades: false, remarkableAthlete: false, sneakAttack: false, rageDamageBonus: false, martialArtsDie: false, wildShape: false, channelDivinity: false, extraAttack: false, extraAttackCount: 1 }
    case ModifierType.ReplaceWeaponAbility:
      return { $type: 'ReplaceWeaponAbility', newAbilityScore: 0, meleeOnly: false, rangedOnly: false, restrictToWeaponId: null, restrictToWeaponType: null, requiresProperty: null, useHigherOfStrengthOrDexterity: false }
  }
}

export function createDefaultModifier(): Modifier {
  return {
    type: ModifierType.Bonus,
    subtype: createDefaultSubtype(ModifierType.Bonus),
    abilityScore: null,
    diceCount: 0,
    diceValue: 0,
    fixedValue: 0,
    details: '',
    duration: null,
    applyOnMulticlass: true,
  }
}

export function getModifierSummary(modifier: Modifier): string {
  const typeLabel = modifierTypeLabels[modifier.type] ?? 'Unknown'
  if (modifier.fixedValue) {
    const sign = modifier.fixedValue > 0 ? '+' : ''
    return `${typeLabel} (${sign}${modifier.fixedValue})`
  }
  if (modifier.diceCount && modifier.diceValue) {
    return `${typeLabel} (${modifier.diceCount}d${modifier.diceValue})`
  }
  return typeLabel
}

import { modifierTypeLabels } from './enums'
