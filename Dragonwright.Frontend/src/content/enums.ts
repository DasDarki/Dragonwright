import type { SelectOption } from '@/components/ui/UiSelect.vue'

function toOptions(map: Record<number, string>): SelectOption[] {
  return Object.entries(map).map(([value, label]) => ({ label, value: Number(value) }))
}

export const sourceLabels: Record<number, string> = {
  0: 'Legacy 2014',
  1: 'Official 2024',
  2: 'Homebrew',
}
export const sourceOptions = toOptions(sourceLabels)

export const sourceBadge: Record<number, { label: string; variant: 'accent' | 'info' | 'muted' }> = {
  0: { label: 'Legacy', variant: 'muted' },
  1: { label: 'Official', variant: 'accent' },
  2: { label: 'Homebrew', variant: 'info' },
}

export const spellLevelLabels: Record<number, string> = {
  0: 'Cantrip',
  1: '1st Level', 2: '2nd Level', 3: '3rd Level', 4: '4th Level',
  5: '5th Level', 6: '6th Level', 7: '7th Level', 8: '8th Level', 9: '9th Level',
}
export const spellLevelOptions = toOptions(spellLevelLabels)

export const spellSchoolLabels: Record<number, string> = {
  0: 'Abjuration', 1: 'Conjuration', 2: 'Divination', 3: 'Enchantment',
  4: 'Evocation', 5: 'Illusion', 6: 'Necromancy', 7: 'Transmutation',
}
export const spellSchoolOptions = toOptions(spellSchoolLabels)

export const attackTypeLabels: Record<number, string> = {
  0: 'None', 1: 'Melee', 2: 'Ranged',
}
export const attackTypeOptions = toOptions(attackTypeLabels)

export const abilityScoreLabels: Record<number, string> = {
  0: 'Strength', 1: 'Dexterity', 2: 'Constitution',
  3: 'Intelligence', 4: 'Wisdom', 5: 'Charisma',
}
export const abilityScoreOptions = toOptions(abilityScoreLabels)

export const itemTypeLabels: Record<number, string> = {
  0: 'Armor', 1: 'Weapon', 2: 'Potion', 3: 'Ring',
  4: 'Rod', 5: 'Scroll', 6: 'Staff', 7: 'Wand',
  8: 'Wondrous Item', 9: 'Adventuring Gear', 10: 'Tool',
  11: 'Ammunition', 12: 'Shield',
}
export const itemTypeOptions = toOptions(itemTypeLabels)

export const rarityLabels: Record<number, string> = {
  0: 'Common', 1: 'Uncommon', 2: 'Rare',
  3: 'Very Rare', 4: 'Legendary', 5: 'Artifact',
}
export const rarityOptions = toOptions(rarityLabels)

export const weaponTypeLabels: Record<number, string> = {
  0: 'Simple Melee', 1: 'Simple Ranged', 2: 'Martial Melee', 3: 'Martial Ranged',
}
export const weaponTypeOptions = toOptions(weaponTypeLabels)

export const weaponPropertyLabels: Record<number, string> = {
  0: 'Ammunition', 1: 'Finesse', 2: 'Heavy', 3: 'Light',
  4: 'Loading', 5: 'Range', 6: 'Reach', 7: 'Special',
  8: 'Thrown', 9: 'Two-Handed', 10: 'Versatile', 11: 'Nick',
  12: 'Graze', 13: 'Cleave', 14: 'Topple', 15: 'Vex',
  16: 'Slow', 17: 'Sap', 18: 'Push',
}
export const weaponPropertyOptions = toOptions(weaponPropertyLabels)

export const masteryLabels: Record<number, string> = {
  0: 'Cleave', 1: 'Graze', 2: 'Nick', 3: 'Push',
  4: 'Sap', 5: 'Slow', 6: 'Topple', 7: 'Vex',
}
export const masteryOptions = toOptions(masteryLabels)

export const damageTypeLabels: Record<number, string> = {
  0: 'Acid', 1: 'Bludgeoning', 2: 'Cold', 3: 'Fire',
  4: 'Force', 5: 'Lightning', 6: 'Necrotic', 7: 'Piercing',
  8: 'Poison', 9: 'Psychic', 10: 'Radiant', 11: 'Slashing',
  12: 'Thunder',
}
export const damageTypeOptions = toOptions(damageTypeLabels)

export const conditionLabels: Record<number, string> = {
  0: 'Blinded', 1: 'Charmed', 2: 'Deafened', 3: 'Exhaustion',
  4: 'Frightened', 5: 'Grappled', 6: 'Incapacitated', 7: 'Invisible',
  8: 'Paralyzed', 9: 'Petrified', 10: 'Poisoned', 11: 'Prone',
  12: 'Restrained', 13: 'Stunned', 14: 'Unconscious',
}
export const conditionOptions = toOptions(conditionLabels)

export const shapeLabels: Record<number, string> = {
  0: 'Cone', 1: 'Cube', 2: 'Cylinder', 3: 'Line', 4: 'Sphere',
}
export const shapeOptions = toOptions(shapeLabels)

export const timeUnitLabels: Record<number, string> = {
  0: 'Action', 1: 'Bonus Action', 2: 'Reaction', 3: 'Minute',
  4: 'Hour', 5: 'Round', 6: 'Day', 7: 'Instantaneous',
  8: 'Until Dispelled', 9: 'Special',
}
export const timeUnitOptions = toOptions(timeUnitLabels)

export const skillLabels: Record<number, string> = {
  0: 'Acrobatics', 1: 'Animal Handling', 2: 'Arcana', 3: 'Athletics',
  4: 'Deception', 5: 'History', 6: 'Insight', 7: 'Intimidation',
  8: 'Investigation', 9: 'Medicine', 10: 'Nature', 11: 'Perception',
  12: 'Performance', 13: 'Persuasion', 14: 'Religion', 15: 'Sleight of Hand',
  16: 'Stealth', 17: 'Survival',
}
export const skillOptions = toOptions(skillLabels)

export const toolLabels: Record<number, string> = {
  0: "Alchemist's Supplies", 1: "Brewer's Supplies", 2: "Calligrapher's Supplies",
  3: "Carpenter's Tools", 4: "Cartographer's Tools", 5: "Cobbler's Tools",
  6: "Cook's Utensils", 7: "Glassblower's Tools", 8: "Jeweler's Tools",
  9: "Leatherworker's Tools", 10: "Mason's Tools", 11: "Painter's Supplies",
  12: "Potter's Tools", 13: "Smith's Tools", 14: "Tinker's Tools",
  15: "Weaver's Tools", 16: "Woodcarver's Tools",
  17: 'Disguise Kit', 18: 'Forgery Kit', 19: "Herbalism Kit",
  20: "Navigator's Tools", 21: "Poisoner's Kit", 22: "Thieves' Tools",
  23: 'Bagpipes', 24: 'Drum', 25: 'Dulcimer', 26: 'Flute',
  27: 'Lute', 28: 'Lyre', 29: 'Horn', 30: 'Pan Flute',
  31: 'Shawm', 32: 'Viol',
  33: 'Dice Set', 34: 'Dragonchess Set', 35: 'Playing Card Set',
  36: "Three-Dragon Ante Set",
}
export const toolOptions = toOptions(toolLabels)

export const creatureTypeLabels: Record<number, string> = {
  0: 'Aberration', 1: 'Beast', 2: 'Celestial', 3: 'Construct',
  4: 'Dragon', 5: 'Elemental', 6: 'Fey', 7: 'Fiend',
  8: 'Giant', 9: 'Humanoid', 10: 'Monstrosity', 11: 'Ooze',
  12: 'Plant', 13: 'Undead',
}
export const creatureTypeOptions = toOptions(creatureTypeLabels)

export const featureTypeLabels: Record<number, string> = {
  0: 'Passive', 1: 'Active', 2: 'Spellcasting', 3: 'Choice',
}
export const featureTypeOptions = toOptions(featureTypeLabels)

export const resetTypeLabels: Record<number, string> = {
  0: 'Short Rest', 1: 'Long Rest', 2: 'Dawn', 3: 'None',
}
export const resetTypeOptions = toOptions(resetTypeLabels)

export const actionTypeLabels: Record<number, string> = {
  0: 'Action', 1: 'Bonus Action', 2: 'Reaction', 3: 'Free Action',
  4: 'Movement', 5: 'Other',
}
export const actionTypeOptions = toOptions(actionTypeLabels)

export const characteristicsTypeLabels: Record<number, string> = {
  0: 'Personality Trait', 1: 'Ideal', 2: 'Bond', 3: 'Flaw',
}
export const characteristicsTypeOptions = toOptions(characteristicsTypeLabels)

export const spellPrepareTypeLabels: Record<number, string> = {
  0: 'Prepared', 1: 'Always Prepared',
}
export const spellPrepareTypeOptions = toOptions(spellPrepareTypeLabels)

export const spellLearnTypeLabels: Record<number, string> = {
  0: 'Known', 1: 'Spellbook',
}
export const spellLearnTypeOptions = toOptions(spellLearnTypeLabels)
