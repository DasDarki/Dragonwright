import type { FieldTip } from '@/components/ui/UiFieldTip.vue'

export const commonTips = {
  name: {
    title: 'Name',
    body: 'The official or homebrew name for this entity. For official content, use the exact name as printed in the source book.',
  } satisfies FieldTip,

  source: {
    title: 'Source',
    body: 'Which ruleset this content belongs to.\n\n"Legacy 2014" refers to the original 5th Edition (PHB 2014). "Official 2024" refers to the revised 2024 rules. "Homebrew" is custom user-created content.',
  } satisfies FieldTip,

  description: {
    title: 'Description',
    body: 'The full text describing this entity\'s effects, lore, and mechanics. Include all relevant rules text, including effects at higher levels or special conditions.',
  } satisfies FieldTip,

  displayOrder: {
    title: 'Display Order',
    body: 'Controls the visual sort order when multiple items are listed together. Lower numbers appear first. Items with the same order are sorted alphabetically.',
  } satisfies FieldTip,

  requiredLevel: {
    title: 'Required Character Level',
    body: 'The minimum character level (not class level) at which this feature becomes available. Most features are tied to class level progression.',
    examples: ['Level 1 for starting features', 'Level 3 for subclass features', 'Level 20 for capstone abilities'],
  } satisfies FieldTip,

  featureType: {
    title: 'Feature Type',
    body: 'How this feature behaves mechanically.\n\nPassive: Always active, no action required.\nActive: Requires an action, bonus action, or reaction to use.\nSpellcasting: Grants or modifies spellcasting ability.\nChoice: Presents options the player must choose between.',
  } satisfies FieldTip,
}

export const spellTips = {
  level: {
    title: 'Spell Level',
    body: 'The power tier of the spell, from Cantrip (0) to 9th level. Cantrips can be cast at will without using a spell slot. Leveled spells require expending a spell slot of the spell\'s level or higher.',
    examples: ['Cantrip: Fire Bolt, Mage Hand', '1st Level: Magic Missile, Shield', '9th Level: Wish, Power Word Kill'],
  } satisfies FieldTip,

  school: {
    title: 'School of Magic',
    body: 'The school categorizes the spell\'s magical nature.\n\nAbjuration: Protection and warding.\nConjuration: Creating objects or transporting creatures.\nDivination: Revealing information.\nEnchantment: Influencing minds.\nEvocation: Manipulating energy (fire, lightning, etc.).\nIllusion: Deceiving the senses.\nNecromancy: Manipulating life and death.\nTransmutation: Changing properties of creatures or objects.',
  } satisfies FieldTip,

  vocalComponent: {
    title: 'Vocal Component (V)',
    body: 'The spell requires the caster to speak firmly. A character who is gagged, silenced, or in an area of magical silence cannot cast spells with a vocal component.',
  } satisfies FieldTip,

  somaticComponent: {
    title: 'Somatic Component (S)',
    body: 'The spell requires specific hand gestures. The caster must have at least one free hand to perform them, or be holding a spellcasting focus in that hand.',
  } satisfies FieldTip,

  materialComponent: {
    title: 'Material Component (M)',
    body: 'The spell requires specific physical materials. If a cost is listed, the material must be obtained; otherwise a component pouch or spellcasting focus can substitute. Materials with a stated cost are consumed unless the description says otherwise.',
    examples: ['A pinch of sulfur (no cost — focus can replace)', 'A diamond worth 300 gp (must have the actual diamond)', 'A gem worth 1,000 gp, which the spell consumes'],
  } satisfies FieldTip,

  materialComponents: {
    title: 'Material Component Details',
    body: 'Describe the specific material(s) required, including any gold piece value. Note if the material is consumed by the casting.',
    examples: ['a tiny strip of white cloth', 'a diamond worth at least 500 gp', 'ruby dust worth 50 gp, which the spell consumes'],
  } satisfies FieldTip,

  concentration: {
    title: 'Concentration',
    body: 'A concentration spell requires the caster to maintain focus. If you cast another concentration spell, the first one ends immediately. Taking damage forces a Constitution saving throw (DC 10 or half the damage, whichever is higher) — failure ends the spell. Being incapacitated or killed also ends concentration.',
  } satisfies FieldTip,

  ritual: {
    title: 'Ritual Casting',
    body: 'A ritual spell can be cast without expending a spell slot by adding 10 minutes to the casting time. The spell must still be prepared or known (depending on class). Ritual casting is optional — you can always use a spell slot instead for the normal casting time.',
  } satisfies FieldTip,

  attackType: {
    title: 'Attack Type',
    body: 'Whether the spell requires an attack roll against the target.\n\nNone: No attack roll (may use saving throw or auto-hit).\nMelee: Requires a melee spell attack roll, using reach.\nRanged: Requires a ranged spell attack roll, with disadvantage on adjacent targets.',
  } satisfies FieldTip,

  save: {
    title: 'Saving Throw',
    body: 'The ability score the target uses to resist the spell. The DC is set by the caster\'s spellcasting ability. Common saves:\n\nDexterity: Dodging area effects.\nConstitution: Resisting physical effects.\nWisdom: Resisting mental effects.\nCharisma: Resisting banishment or possession.',
  } satisfies FieldTip,

  range: {
    title: 'Range (feet)',
    body: 'The maximum distance from the caster to the target or point of origin. Use 0 for Self or Touch spells. "Self" means the spell originates from the caster. "Touch" means the caster must physically touch the target.',
    examples: ['0 (Self or Touch)', '30 (short range, e.g., Healing Word)', '120 (standard combat range, e.g., Fireball)', '300 (long range, e.g., Fly)'],
  } satisfies FieldTip,

  areaOfEffect: {
    title: 'Area of Effect',
    body: 'The shape of the spell\'s affected area.\n\nCone: Emanates in a widening triangle from the caster.\nCube: A cubic area measured from a point.\nCylinder: A vertical circular area.\nLine: A straight line from the caster.\nSphere: A radius measured from a central point.',
  } satisfies FieldTip,

  areaSize: {
    title: 'Area Size (feet)',
    body: 'The size of the area of effect in feet. This represents the radius for spheres and cylinders, the length for cones and lines, or the side length for cubes.',
    examples: ['15 (Burning Hands cone)', '20 (Fireball sphere radius)', '30 (Lightning Bolt line)'],
  } satisfies FieldTip,

  castingTimes: {
    title: 'Casting Times',
    body: 'How long it takes to cast the spell. Most combat spells use 1 Action. Some use a Bonus Action for quick casting or a Reaction in response to a trigger. Ritual and non-combat spells may take minutes or hours.',
    examples: ['1 Action (most spells)', '1 Bonus Action (Healing Word, Misty Step)', '1 Reaction (Shield, Counterspell)', '1 Minute (ritual spells)', '1 Hour (complex spells like Find Familiar)'],
  } satisfies FieldTip,

  durations: {
    title: 'Durations',
    body: 'How long the spell\'s effect persists.\n\nInstantaneous: Effect happens and is done (damage, healing).\nRounds: Each round is 6 seconds of in-game time.\nConcentration: Lasts until concentration is broken or the duration ends.\nUntil Dispelled: Permanent until removed by Dispel Magic or similar.',
    examples: ['Instantaneous (Fire Bolt)', '1 Round (Shocking Grasp)', '1 Minute = 10 Rounds (most combat buffs)', '1 Hour (Mage Armor)', '8 Hours (long-duration utility)'],
  } satisfies FieldTip,

  damageTypes: {
    title: 'Damage Types',
    body: 'The type(s) of damage the spell deals. Creatures may have resistance (half damage), immunity (no damage), or vulnerability (double damage) to specific types.',
  } satisfies FieldTip,

  conditions: {
    title: 'Conditions',
    body: 'Status effects the spell can impose on targets. Each condition has specific rules about what the affected creature can and cannot do.\n\nCommon ones: Frightened (disadvantage while source is visible), Restrained (speed 0, disadvantage on attacks), Stunned (incapacitated, auto-fail STR/DEX saves).',
  } satisfies FieldTip,

  damageRolls: {
    title: 'Damage Rolls',
    body: 'The dice rolled to determine damage. Specify the number of dice, dice size, any flat bonus, and the damage type.\n\nFor spells that scale (cantrips or at higher levels), enter the base damage. Scaling is typically described in the spell description.',
    examples: ['1d10 Fire (Fire Bolt)', '8d6 Fire (Fireball)', '2d6 Radiant (Guiding Bolt)', '3d8+modifier Necrotic (Vampiric Touch)'],
  } satisfies FieldTip,

  tags: {
    title: 'Tags',
    body: 'Optional custom tags for categorization and filtering. Separate multiple tags with commas. Tags help organize content beyond the standard school/level classification.',
    examples: ['healing, support', 'damage, area-of-effect', 'utility, exploration, ritual'],
  } satisfies FieldTip,
}

export const itemTips = {
  type: {
    title: 'Item Type',
    body: 'The general category this item belongs to. This determines which properties and fields are relevant.\n\nWeapon: Shows weapon-specific fields (damage, properties, mastery).\nArmor/Shield: Shows AC and armor-specific fields.\nTool: Shows tool type selection.\nOther types are general equipment with no additional fields.',
  } satisfies FieldTip,

  rarity: {
    title: 'Rarity',
    body: 'How rare and powerful the item is. Rarity roughly correlates with the character level at which the item is appropriate.\n\nCommon: Minor utility, suitable from level 1.\nUncommon: Moderate benefit, levels 1–5.\nRare: Significant power, levels 5–10.\nVery Rare: Major power, levels 11–15.\nLegendary: Extraordinary, levels 15+.\nArtifact: Unique, world-shaping items.',
  } satisfies FieldTip,

  isMagical: {
    title: 'Magical',
    body: 'Whether this item is considered magical for rules purposes. Magical items can overcome resistance/immunity to nonmagical damage and are affected by spells like Detect Magic and Dispel Magic. Some magical items glow faintly or have other telltale properties.',
  } satisfies FieldTip,

  requiresAttunement: {
    title: 'Requires Attunement',
    body: 'Whether a creature must attune to the item to use its magical properties. Attuning requires a short rest spent focusing on the item. A creature can be attuned to a maximum of 3 items at a time. Some items require specific classes or alignments to attune.',
  } satisfies FieldTip,

  isConsumable: {
    title: 'Consumable',
    body: 'Whether the item is destroyed or used up when activated. Potions, scrolls, and some wondrous items are consumable. Once used, the item is gone.',
    examples: ['Potion of Healing (consumed on use)', 'Spell Scroll (consumed on casting)', 'Bead of Force (one-time use)'],
  } satisfies FieldTip,

  isBackpack: {
    title: 'Container',
    body: 'Whether this item can hold other items inside it (like a backpack, bag, chest, or Bag of Holding). Container items may have special capacity rules.',
  } satisfies FieldTip,

  weight: {
    title: 'Weight (ounces)',
    body: 'The item\'s weight in ounces. The standard 5e rules use pounds (16 ounces = 1 pound). A character can typically carry 15 × their Strength score in pounds.',
    examples: ['16 oz = 1 lb (Dagger)', '48 oz = 3 lb (Shortsword)', '96 oz = 6 lb (Longsword)', '288 oz = 18 lb (Heavy armor)'],
  } satisfies FieldTip,

  value: {
    title: 'Value (copper pieces)',
    body: 'The item\'s base value in copper pieces (cp). The D&D currency system:\n\n1 pp = 10 gp = 100 sp = 1000 cp\n\nFor example, 50 gp = 5000 cp.',
    examples: ['200 cp = 2 gp (Dagger)', '1000 cp = 10 gp (Shortsword)', '5000 cp = 50 gp (Chain Mail)', '150000 cp = 1500 gp (Plate Armor)'],
  } satisfies FieldTip,

  weaponType: {
    title: 'Weapon Type',
    body: 'The weapon\'s category, which determines who is proficient with it.\n\nSimple Melee: Basic melee weapons anyone can use (clubs, daggers).\nSimple Ranged: Basic ranged weapons (crossbow, shortbow).\nMartial Melee: Advanced melee weapons requiring training (longsword, greataxe).\nMartial Ranged: Advanced ranged weapons (longbow, hand crossbow).',
  } satisfies FieldTip,

  mastery: {
    title: 'Weapon Mastery (2024)',
    body: 'A 2024 rules feature. Each weapon has a mastery property that martial classes can unlock.\n\nCleave: Hit a second adjacent creature.\nGraze: Deal ability modifier damage on a miss.\nNick: Extra attack as part of light weapon attacks.\nPush: Push the target 10 feet away.\nSap: Give disadvantage on the target\'s next attack.\nSlow: Reduce the target\'s speed by 10 feet.\nTopple: Force a STR save or knock prone.\nVex: Gain advantage on next attack against the target.',
  } satisfies FieldTip,

  weaponProperties: {
    title: 'Weapon Properties',
    body: 'Special traits that modify how the weapon is used.\n\nAmmunition: Requires ammo, has a range.\nFinesse: Use STR or DEX for attack/damage.\nHeavy: Small creatures have disadvantage.\nLight: Can dual-wield with another light weapon.\nLoading: One attack per action regardless of Extra Attack.\nReach: Melee range extends to 10 ft.\nThrown: Can throw for ranged attacks.\nTwo-Handed: Requires both hands.\nVersatile: Can use one or two hands (larger damage die).',
  } satisfies FieldTip,

  weaponRange: {
    title: 'Range (feet)',
    body: 'The normal range for ranged or thrown weapons. Attacks beyond this range (up to Max Range) have disadvantage. Melee weapons with the Thrown property use this range when thrown.',
    examples: ['20 ft normal (Dagger thrown)', '80 ft normal (Shortbow)', '150 ft normal (Longbow)'],
  } satisfies FieldTip,

  weaponMaxRange: {
    title: 'Maximum Range (feet)',
    body: 'The maximum distance at which a ranged attack can be made, with disadvantage. Attacks beyond this range automatically miss.',
    examples: ['60 ft max (Dagger thrown)', '320 ft max (Shortbow)', '600 ft max (Longbow)'],
  } satisfies FieldTip,

  attackBonus: {
    title: 'Attack Bonus',
    body: 'A flat bonus added to attack rolls with this weapon, on top of the normal ability modifier + proficiency bonus. Most mundane weapons have +0. Magic weapons typically grant +1, +2, or +3.',
  } satisfies FieldTip,

  damageBonusAbility: {
    title: 'Damage Bonus Ability',
    body: 'The ability score whose modifier is added to damage rolls. Usually Strength for melee or Dexterity for ranged. Finesse weapons can use either.',
  } satisfies FieldTip,

  baseArmorClass: {
    title: 'Base Armor Class',
    body: 'The base AC provided by the armor before any ability modifiers.\n\nLight armor: AC + full DEX modifier.\nMedium armor: AC + DEX modifier (max +2).\nHeavy armor: AC only (no DEX bonus).\nWithout armor, base AC is 10 + DEX modifier.',
    examples: ['11 (Leather — light)', '14 (Chain Shirt — medium)', '16 (Chain Mail — heavy)', '18 (Plate — heavy)'],
  } satisfies FieldTip,

  acBonus: {
    title: 'AC Bonus',
    body: 'An additional bonus to AC on top of the base. Shields give +2 AC. Magic armor may give +1, +2, or +3.',
  } satisfies FieldTip,

  acBonusAbility: {
    title: 'AC Bonus Ability',
    body: 'The ability score whose modifier is added to AC. For most armor, this is Dexterity. Some special features use other abilities.',
  } satisfies FieldTip,

  maxAcBonusFromAbility: {
    title: 'Max AC Bonus from Ability',
    body: 'The maximum ability modifier that can be added to this armor\'s AC. Medium armor typically caps the Dexterity bonus at +2. Light armor and no armor have no cap (set to 0). Heavy armor doesn\'t add DEX at all.',
    examples: ['0 (no cap — light armor)', '2 (medium armor)', '0 with no ability set (heavy armor — no DEX bonus)'],
  } satisfies FieldTip,

  stealthDisadvantage: {
    title: 'Disadvantage on Stealth',
    body: 'If checked, wearing this armor imposes disadvantage on Dexterity (Stealth) checks. This is common for medium and heavy armors that are noisy or cumbersome.',
  } satisfies FieldTip,

  donTime: {
    title: 'Don Time (seconds)',
    body: 'How long it takes to put on this armor. Light armor: ~60 seconds (1 minute). Medium/Heavy armor: 300–600 seconds (5–10 minutes). Shields: 6 seconds (1 action).',
  } satisfies FieldTip,

  doffTime: {
    title: 'Doff Time (seconds)',
    body: 'How long it takes to remove this armor. Light armor: ~60 seconds. Medium/Heavy armor: 60–300 seconds (1–5 minutes). Shields: 6 seconds (1 action).',
  } satisfies FieldTip,

  requiredAbility: {
    title: 'Required Ability Score',
    body: 'Some heavy armor requires a minimum Strength score. If the wearer doesn\'t meet the requirement, their speed is reduced by 10 feet.',
    examples: ['Strength 13 (Chain Mail)', 'Strength 15 (Splint, Plate)'],
  } satisfies FieldTip,

  requiredAbilityValue: {
    title: 'Required Score Value',
    body: 'The minimum score needed in the required ability. A character whose score is below this threshold suffers a speed penalty.',
  } satisfies FieldTip,

  toolType: {
    title: 'Tool Type',
    body: 'The specific type of tool this item represents. Proficiency with a tool lets a character add their proficiency bonus to ability checks made with it. Artisan\'s tools are used for crafting; other tools have specific uses.',
  } satisfies FieldTip,
}

export const classTips = {
  hitDie: {
    title: 'Hit Die',
    body: 'The die type rolled to determine hit points gained each level. Also used during short rests to recover HP. The die size reflects the class\'s hardiness.\n\nA class gains one Hit Die per level. During a short rest, you can roll any number of your unspent Hit Dice and regain that many HP (plus CON modifier per die). Half your total Hit Dice are recovered on a long rest.',
    examples: ['d6 (Sorcerer, Wizard)', 'd8 (Bard, Cleric, Rogue, Warlock)', 'd10 (Fighter, Paladin, Ranger)', 'd12 (Barbarian)'],
  } satisfies FieldTip,

  baseHp: {
    title: 'Base HP at 1st Level',
    body: 'The fixed hit points a character receives at 1st level (before adding Constitution modifier). This is always the maximum value of the Hit Die.',
    examples: ['6 (d6 classes)', '8 (d8 classes)', '10 (d10 classes)', '12 (d12 classes)'],
  } satisfies FieldTip,

  fixHpPerLevel: {
    title: 'Fixed HP Per Level After 1st',
    body: 'The fixed number of HP gained per level when not rolling. This is the average of the Hit Die rounded up, used as an alternative to rolling for HP.\n\nPlayers choose to either roll the Hit Die or take this fixed value each level.',
    examples: ['4 (d6 average)', '5 (d8 average)', '6 (d10 average)', '7 (d12 average)'],
  } satisfies FieldTip,

  hpModifierAbility: {
    title: 'HP Modifier Ability',
    body: 'The ability score whose modifier is added to HP each level. For virtually all classes, this is Constitution. The modifier is added once per level (including 1st level).',
  } satisfies FieldTip,

  primaryAbilityScores: {
    title: 'Primary Ability Scores',
    body: 'The most important ability scores for this class. These guide players on where to allocate their highest scores during character creation and which abilities to improve with ASIs.',
    examples: ['Strength & Constitution (Barbarian)', 'Dexterity & Wisdom (Monk, Ranger)', 'Intelligence (Wizard)', 'Charisma (Bard, Sorcerer, Warlock)'],
  } satisfies FieldTip,

  savingThrows: {
    title: 'Saving Throw Proficiencies',
    body: 'Each class grants proficiency in two saving throws. This lets the character add their proficiency bonus to those saves. By design, every class gets one "strong" save (DEX, CON, or WIS) and one "weak" save (STR, INT, or CHA).',
  } satisfies FieldTip,

  skillCount: {
    title: 'Skill Proficiency Count',
    body: 'The number of skills the character can choose from the class\'s skill list at 1st level. Most classes choose 2. Bards and Rangers choose 3. Rogues choose 4.',
  } satisfies FieldTip,

  skillOptions: {
    title: 'Skill Proficiency Options',
    body: 'The list of skills this class can choose proficiency in at 1st level. The player picks a number equal to the Skill Proficiency Count from this list.',
  } satisfies FieldTip,

  toolProficiencies: {
    title: 'Tool Proficiencies',
    body: 'Tools the class is automatically proficient with. Proficiency lets the character add their proficiency bonus to ability checks using the tool.',
    examples: ['Thieves\' Tools (Rogue)', 'Herbalism Kit (Druid)', 'Three musical instruments (Bard)'],
  } satisfies FieldTip,

  armorProficiencies: {
    title: 'Armor Proficiencies',
    body: 'Armor types the class can wear without penalty. Wearing armor you\'re not proficient with causes disadvantage on ability checks, saving throws, and attack rolls involving STR or DEX, and prevents spellcasting.',
  } satisfies FieldTip,

  weaponProficiencies: {
    title: 'Weapon Proficiencies',
    body: 'Weapon types the class is proficient with. Proficiency lets the character add their proficiency bonus to attack rolls with those weapons.',
  } satisfies FieldTip,

  features: {
    title: 'Class Features',
    body: 'Abilities gained at specific class levels. Features define what makes each class unique — from a Fighter\'s Action Surge to a Wizard\'s Arcane Recovery.\n\nFeatures are gained at the class level listed, not the character\'s total level (relevant for multiclassing).',
  } satisfies FieldTip,

  subclasses: {
    title: 'Subclasses',
    body: 'Specializations within the class, typically chosen at level 3. Each subclass grants unique features at specific levels, further differentiating characters of the same class.',
    examples: ['Fighter: Champion, Battle Master, Eldritch Knight', 'Wizard: School of Evocation, Abjuration, etc.', 'Rogue: Thief, Assassin, Arcane Trickster'],
  } satisfies FieldTip,

  featureOptional: {
    title: 'Optional Feature',
    body: 'If checked, this feature is not automatically gained — the player may choose whether to take it. Optional features were introduced in Tasha\'s Cauldron of Everything as alternatives to existing features.',
  } satisfies FieldTip,

  featureHasOptions: {
    title: 'Has Options',
    body: 'Whether this feature presents sub-choices to the player. For example, Fighting Style lets you choose one style from a list. Eldritch Invocations let you pick from available invocations.',
  } satisfies FieldTip,

  featureShowLevel: {
    title: 'Show Required Level',
    body: 'Whether to display the level requirement in the character builder and on the character sheet. Useful for features gained at specific levels. Disable for sub-features that inherit the parent\'s level.',
  } satisfies FieldTip,

  featureHideBuilder: {
    title: 'Hide in Builder',
    body: 'If checked, this feature won\'t appear in the character builder interface. Useful for internal/system features that are applied automatically.',
  } satisfies FieldTip,

  featureHideSheet: {
    title: 'Hide in Character Sheet',
    body: 'If checked, this feature won\'t appear on the character sheet. Useful for features whose effects are already represented elsewhere (like proficiency grants).',
  } satisfies FieldTip,

  subclassCanCastSpells: {
    title: 'Can Cast Spells',
    body: 'Whether this subclass grants spellcasting ability. Some subclasses give spells to otherwise non-casting classes.',
    examples: ['Eldritch Knight (Fighter subclass — learns wizard spells)', 'Arcane Trickster (Rogue subclass — learns wizard spells)'],
  } satisfies FieldTip,

  subclassSpellcastingAbility: {
    title: 'Spellcasting Ability',
    body: 'The ability score used for this subclass\'s spellcasting. Determines spell save DC (8 + proficiency + ability modifier) and spell attack bonus (proficiency + ability modifier).',
    examples: ['Intelligence (Eldritch Knight, Arcane Trickster)', 'Wisdom (Ranger, Cleric subclasses)', 'Charisma (Paladin, Bard subclasses)'],
  } satisfies FieldTip,

  subclassKnowsAllSpells: {
    title: 'Knows All Spells',
    body: 'If checked, the subclass has access to its entire spell list without needing to learn individual spells. Clerics and Druids work this way — they can prepare any spell from their list each day.',
  } satisfies FieldTip,

  subclassPrepareType: {
    title: 'Spell Prepare Type',
    body: 'How the subclass prepares spells.\n\nPrepared: Choose a subset of known/available spells each long rest.\nAlways Prepared: Certain spells are always prepared and don\'t count against the preparation limit.',
  } satisfies FieldTip,

  subclassLearnType: {
    title: 'Spell Learn Type',
    body: 'How the subclass acquires new spells.\n\nKnown: Learns a fixed number of spells as they level up. Can swap one spell when leveling.\nSpellbook: Starts with spells in a book and can copy new ones found during adventures (Wizard-specific).',
  } satisfies FieldTip,

  subclassSelectionLevel: {
    title: 'Subclass Selection Level',
    body: 'The class level at which a character must choose their subclass.\n\nMost classes choose at level 3. Some exceptions:\n• Cleric, Sorcerer, Warlock: Level 1\n• Wizard: Level 2\n• Set to 0 if the class has no subclasses.',
    examples: ['0 (no subclass)', '1 (Cleric, Warlock)', '2 (Wizard)', '3 (most classes)'],
  } satisfies FieldTip,

  standardArray: {
    title: 'Standard Array',
    body: 'The standard array is a set of six ability score values that players can assign to their abilities during character creation.\n\nThe default D&D 5e standard array is [15, 14, 13, 12, 10, 8]. This field allows classes to define a custom standard array if desired.',
    examples: ['15, 14, 13, 12, 10, 8 (default)', '16, 14, 14, 12, 10, 8 (custom)'],
  } satisfies FieldTip,

  multiclassingRequirements: {
    title: 'Multiclassing Requirements',
    body: 'Minimum ability scores required to multiclass INTO this class. Players must meet all requirements (AND logic).\n\nTypically requires 13 in the class\'s primary ability score(s). Some classes have alternative requirements (OR logic) for flexibility.',
    examples: ['Fighter: STR 13 OR DEX 13', 'Paladin: STR 13 AND CHA 13', 'Monk: DEX 13 AND WIS 13'],
  } satisfies FieldTip,
}

export const raceTips = {
  creatureType: {
    title: 'Creature Type',
    body: 'The creature type determines how certain spells and effects interact with this race. Most playable races are Humanoid. Some newer races have different types.\n\nThis matters for spells like Hold Person (only affects Humanoids), Detect Evil and Good, and effects that target specific creature types.',
    examples: ['Humanoid (most races)', 'Fey (Eladrin, Satyr)', 'Construct (Warforged)', 'Celestial (Custom origins)'],
  } satisfies FieldTip,

  traits: {
    title: 'Racial Traits',
    body: 'Innate abilities and features all members of this race possess. These can be passive bonuses (darkvision, damage resistance), active abilities (breath weapon, innate spellcasting), or proficiencies.\n\nSome traits may have a level requirement — they unlock as the character grows in power.',
  } satisfies FieldTip,
}

export const backgroundTips = {
  languageCount: {
    title: 'Language Count',
    body: 'The number of additional languages the character learns from this background, beyond Common and any racial languages. The player typically chooses from the standard language list.',
  } satisfies FieldTip,

  abilityScoreIncreases: {
    title: 'Ability Score Increases',
    body: 'In the 2024 rules, backgrounds grant ability score increases (instead of races). Typically +2 to one score and +1 to another, or +1 to three scores. Select which ability scores are eligible.',
  } satisfies FieldTip,

  skillProficiencies: {
    title: 'Skill Proficiencies',
    body: 'Skills the character gains proficiency in from this background. Each background typically grants 2 skill proficiencies, representing the character\'s past training and experience.',
    examples: ['Insight & Religion (Acolyte)', 'Deception & Stealth (Criminal)', 'Athletics & Survival (Outlander)'],
  } satisfies FieldTip,

  characteristics: {
    title: 'Characteristics',
    body: 'Suggested personality traits, ideals, bonds, and flaws that help flesh out a character\'s personality.\n\nPersonality Trait: A distinctive behavior or mannerism.\nIdeal: A belief or principle the character strives for.\nBond: A connection to a person, place, or event.\nFlaw: A weakness or vice.\n\nPlayers roll or choose from these tables during character creation.',
  } satisfies FieldTip,
}

export const languageTips = {
  name: {
    title: 'Language Name',
    body: 'The name of the language as known in the game world. Standard languages are spoken by common races. Exotic languages are rarer and tied to specific creature types or planes of existence.',
    examples: ['Standard: Common, Elvish, Dwarvish, Giant, Gnomish, Goblin, Halfling, Orc', 'Exotic: Abyssal, Celestial, Draconic, Deep Speech, Infernal, Primordial, Sylvan, Undercommon'],
  } satisfies FieldTip,
}

export const featTips = {
  featLevel: {
    title: 'Feat Level',
    body: 'Determines when the feat becomes available. Level 1 feats can be taken at character creation. Level 4+ feats require higher character levels.',
    examples: ['1 (Origin feats in 2024 rules)', '4 (General feats)', '8 (Epic Boons in older editions)'],
  } satisfies FieldTip,

  isRepeatable: {
    title: 'Repeatable',
    body: 'If checked, this feat can be taken multiple times. Each time grants the benefits again (unless stated otherwise). Most feats cannot be repeated.',
  } satisfies FieldTip,

  prerequisiteDescription: {
    title: 'Prerequisite Description',
    body: 'A text description of any requirements to take this feat. This is displayed to players and can describe complex requirements not captured by the other fields.',
    examples: ['Proficiency with a martial weapon', 'The ability to cast at least one spell', 'Dexterity 13 or higher'],
  } satisfies FieldTip,

  prerequisiteAbilityScore: {
    title: 'Prerequisite Ability',
    body: 'Some feats require a minimum ability score. Select the ability that must meet the minimum.',
  } satisfies FieldTip,

  prerequisiteAbilityScoreMinimum: {
    title: 'Prerequisite Minimum',
    body: 'The minimum score required in the selected ability. Usually 13 or 15.',
  } satisfies FieldTip,

  prerequisiteSpellcasting: {
    title: 'Requires Spellcasting',
    body: 'If checked, the character must be able to cast at least one spell to take this feat.',
  } satisfies FieldTip,

  abilityScoreOptions: {
    title: 'Ability Score Options',
    body: 'Which ability scores can be increased by this feat. The player chooses one from the selected options.',
  } satisfies FieldTip,

  abilityScoreIncrease: {
    title: 'Ability Score Increase',
    body: 'The amount the chosen ability score increases. Usually +1. The score cannot exceed 20 through this increase.',
  } satisfies FieldTip,

  options: {
    title: 'Feat Options',
    body: 'Sub-choices within the feat. Some feats let you pick one option from a list.',
  } satisfies FieldTip,
}

export const timeValueTip: FieldTip = {
  title: 'Time Value',
  body: 'The numeric amount paired with the time unit. For actions and reactions, the value is typically 1. For longer durations, enter the number of minutes, hours, or days.',
}

export const timeUnitTip: FieldTip = {
  title: 'Time Unit',
  body: 'The unit of time measurement.\n\nAction: Standard action in combat (6 seconds).\nBonus Action: Quick action on the same turn.\nReaction: Triggered by a specific event.\nRound: 6 seconds of game time.\nMinute: 10 rounds of combat.\nInstantaneous: Happens and is done.\nUntil Dispelled: Lasts indefinitely.\nSpecial: See the spell/feature description.',
}

export const diceCountTip: FieldTip = {
  title: 'Dice Count',
  body: 'The number of dice to roll. In "2d6", the dice count is 2.',
}

export const diceValueTip: FieldTip = {
  title: 'Dice Value',
  body: 'The type of die (number of sides). In "2d6", the dice value is 6. Standard D&D dice are d4, d6, d8, d10, d12, and d20.',
}

export const damageBonusTip: FieldTip = {
  title: 'Damage Bonus',
  body: 'A flat number added to the damage roll. This is in addition to any ability modifier. Magic weapons often grant +1, +2, or +3.',
}
