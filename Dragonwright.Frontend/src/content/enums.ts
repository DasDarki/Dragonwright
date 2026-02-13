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

export const sourceSortOrder: Record<number, number> = { 1: 0, 2: 1, 0: 2 }

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

export const spellSourceLabels: Record<number, string> = {
  0: 'Class', 1: 'Subclass', 2: 'Race', 3: 'Background', 4: 'Feat', 5: 'Item',
}

export const modifierTypeLabels: Record<number, string> = {
  0: 'Bonus', 1: 'Damage', 2: 'Advantage', 3: 'Disadvantage',
  4: 'Resistance', 5: 'Immunity', 6: 'Vulnerability', 7: 'Sense',
  8: 'Set', 9: 'Half Proficiency', 10: 'Proficiency', 11: 'Expertise',
  12: 'Language', 13: 'Feat', 14: 'Carrying Capacity', 15: 'Natural Weapon',
  16: 'Stealth Disadvantage', 17: 'Speed Reduction', 18: 'Speed Increase',
  19: 'Melee Weapon Attack', 20: 'Ranged Weapon Attack', 21: 'Weapon Property',
  22: 'Half Proficiency (Round Up)', 23: 'Favored Enemy', 24: 'Ignore',
  25: 'Eldritch Blast', 26: 'Replace Damage Type', 27: 'Twice Proficiency',
  28: 'Monk Weapon', 29: 'Protection', 30: 'Stacking Bonus', 31: 'Set Base',
  32: 'Ignore Weapon Property', 33: 'Size', 34: 'Weapon Mastery',
  35: 'Enable Feature', 36: 'Replace Weapon Ability',
}
export const modifierTypeOptions = toOptions(modifierTypeLabels)

export const bonusTargetLabels: Record<number, string> = {
  0: 'Ability Score', 1: 'All Ability Checks', 2: 'Ability Check',
  3: 'All Saving Throws', 4: 'Saving Throw', 5: 'Skill', 6: 'Armor Class',
  7: 'Initiative', 8: 'Attack Roll', 9: 'Melee Attack Roll',
  10: 'Ranged Attack Roll', 11: 'Spell Attack Roll', 12: 'Weapon Damage',
  13: 'Melee Weapon Damage', 14: 'Ranged Weapon Damage', 15: 'Spell Damage',
  16: 'Spell Save DC', 17: 'Hit Points', 18: 'Hit Points Per Level',
  19: 'Speed', 20: 'Passive Perception', 21: 'Passive Investigation',
  22: 'Passive Insight', 23: 'Spell Slots', 24: 'Spells Known',
  25: 'Cantrips Known', 26: 'Proficiency Bonus', 27: 'Carrying Capacity',
  28: 'Death Save Success', 29: 'Death Save Failure',
}
export const bonusTargetOptions = toOptions(bonusTargetLabels)

export const proficiencyTargetLabels: Record<number, string> = {
  0: 'Skill', 1: 'Tool', 2: 'Saving Throw', 3: 'Weapon Type',
  4: 'Specific Weapon', 5: 'Armor Type', 6: 'Shield', 7: 'Initiative',
}
export const proficiencyTargetOptions = toOptions(proficiencyTargetLabels)

export const rollTargetLabels: Record<number, string> = {
  0: 'Attack Roll', 1: 'Melee Attack Roll', 2: 'Ranged Attack Roll',
  3: 'Spell Attack Roll', 4: 'All Ability Checks', 5: 'Ability Check',
  6: 'All Saving Throws', 7: 'Saving Throw', 8: 'Skill', 9: 'Initiative',
  10: 'Death Save', 11: 'Concentration Check',
}
export const rollTargetOptions = toOptions(rollTargetLabels)

export const senseTypeLabels: Record<number, string> = {
  0: 'Darkvision', 1: 'Blindsight', 2: 'Truesight',
  3: 'Tremorsense', 4: 'Superior Darkvision', 5: "Devil's Sight",
}
export const senseTypeOptions = toOptions(senseTypeLabels)

export const movementTypeLabels: Record<number, string> = {
  0: 'Burrow', 1: 'Climb', 2: 'Fly', 3: 'Swim', 4: 'Walk',
}
export const movementTypeOptions = toOptions(movementTypeLabels)

export const sizeLabels: Record<number, string> = {
  0: 'Tiny', 1: 'Small', 2: 'Medium', 3: 'Large', 4: 'Huge', 5: 'Gargantuan',
}
export const sizeOptions = toOptions(sizeLabels)

export const armorTypeLabels: Record<number, string> = {
  0: 'Unspecified', 1: 'Weapon', 2: 'Light Armor', 3: 'Medium Armor',
  4: 'Heavy Armor', 5: 'Shield',
}
export const armorTypeOptions = toOptions(armorTypeLabels)

export const languageTypeLabels: Record<number, string> = {
  0: 'Standard',
  1: 'Exotic',
}
export const languageTypeOptions = toOptions(languageTypeLabels)

export const advancementTypeLabels: Record<number, string> = {
  0: 'Experience Points',
  1: 'Milestone',
}
export const advancementTypeOptions = toOptions(advancementTypeLabels)

export const hitPointTypeLabels: Record<number, string> = {
  0: 'Fixed',
  1: 'Roll',
}
export const hitPointTypeOptions = toOptions(hitPointTypeLabels)

export const abilityScoreGenerationLabels: Record<number, string> = {
  0: 'Standard Array',
  1: 'Point Buy',
  2: 'Roll',
  3: 'Manual',
}
export const abilityScoreGenerationOptions = toOptions(abilityScoreGenerationLabels)

export const alignmentLabels: Record<number, string> = {
  0: 'Lawful Good', 1: 'Neutral Good', 2: 'Chaotic Good',
  3: 'Lawful Neutral', 4: 'True Neutral', 5: 'Chaotic Neutral',
  6: 'Lawful Evil', 7: 'Neutral Evil', 8: 'Chaotic Evil',
}
export const alignmentOptions = toOptions(alignmentLabels)

export const genderLabels: Record<number, string> = {
  0: 'Unspecified', 1: 'Male', 2: 'Female', 3: 'Non-Binary', 4: 'Other',
}
export const genderOptions = toOptions(genderLabels)

export const lifestyleLabels: Record<number, string> = {
  0: 'Wretched', 1: 'Squalid', 2: 'Poor', 3: 'Modest',
  4: 'Comfortable', 5: 'Wealthy', 6: 'Aristocratic',
}
export const lifestyleOptions = toOptions(lifestyleLabels)

export const proficiencyLabels: Record<number, string> = {
  0: 'Not Proficient', 1: 'Half Proficient', 2: 'Proficient', 3: 'Expertise',
}
export const proficiencyOptions = toOptions(proficiencyLabels)

export const startItemTypeLabels: Record<number, string> = {
  0: 'Specific Item', 1: 'Of Weapon Type', 2: 'With Weapon Property',
}

export const startItemChoiceOperatorLabels: Record<number, string> = {
  0: 'And', 1: 'Or',
}

export const defenseStateLabels: Record<number, string> = {
  0: 'None', 1: 'Resistance', 2: 'Immunity', 3: 'Vulnerability',
}
export const defenseStateOptions = toOptions(defenseStateLabels)

export const advantageStateLabels: Record<number, string> = {
  0: 'None', 1: 'Advantage', 2: 'Disadvantage',
}
export const advantageStateOptions = toOptions(advantageStateLabels)

export const currencyLabels: Record<number, string> = {
  0: 'Copper', 1: 'Silver', 2: 'Electrum', 3: 'Gold', 4: 'Platinum',
}
