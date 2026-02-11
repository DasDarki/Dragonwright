<script setup lang="ts">
import { ref, inject, watch, computed } from 'vue'
import { useToast } from '@/composables/useToast'
import { putCharactersId } from '@/api'
import {
  sourceOptions,
  advancementTypeOptions,
  hitPointTypeOptions,
  abilityScoreGenerationOptions,
} from '@/content/enums'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiSwitch from '@/components/ui/UiSwitch.vue'
import UiCheckboxGroup from '@/components/ui/UiCheckboxGroup.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiGrid from '@/components/ui/layout/UiGrid.vue'

const { showToast } = useToast()

const character = inject<any>('character')
const characterId = inject<any>('characterId')
const refreshCharacter = inject<() => Promise<void>>('refreshCharacter')

const form = ref({
  name: '',
  sources: [] as number[],
  advancementType: 1,
  hitPointType: 0,
  abilityScoreGenerationMethod: 1,
  optionalClassFeatures: false,
  customizeOrigin: false,
  exceedLevelCap: false,
  allowMulticlassing: true,
  checkMulticlassingPrerequisites: true,
})

const saving = ref(false)

const hasChanges = computed(() => {
  if (!character?.value) return false
  const c = character.value
  return (
    form.value.name !== c.name ||
    JSON.stringify(form.value.sources) !== JSON.stringify(c.sources) ||
    form.value.advancementType !== c.advancementType ||
    form.value.hitPointType !== c.hitPointType ||
    form.value.abilityScoreGenerationMethod !== c.abilityScoreGenerationMethod ||
    form.value.optionalClassFeatures !== c.optionalClassFeatures ||
    form.value.customizeOrigin !== c.customizeOrigin ||
    form.value.exceedLevelCap !== c.exceedLevelCap ||
    form.value.allowMulticlassing !== c.allowMulticlassing ||
    form.value.checkMulticlassingPrerequisites !== c.checkMulticlassingPrerequisites
  )
})

function loadFromCharacter() {
  if (!character?.value) return
  const c = character.value
  form.value = {
    name: c.name ?? '',
    sources: c.sources ?? [0, 1],
    advancementType: c.advancementType ?? 1,
    hitPointType: c.hitPointType ?? 0,
    abilityScoreGenerationMethod: c.abilityScoreGenerationMethod ?? 1,
    optionalClassFeatures: c.optionalClassFeatures ?? false,
    customizeOrigin: c.customizeOrigin ?? false,
    exceedLevelCap: c.exceedLevelCap ?? false,
    allowMulticlassing: c.allowMulticlassing ?? true,
    checkMulticlassingPrerequisites: c.checkMulticlassingPrerequisites ?? true,
  }
}

watch(() => character?.value, loadFromCharacter, { immediate: true })

async function save() {
  if (!characterId?.value || !character?.value) return
  saving.value = true
  try {
    const c = character.value
    await putCharactersId(characterId.value, {
      name: form.value.name,
      avatarId: c.avatarId,
      sources: form.value.sources,
      advancementType: form.value.advancementType,
      hitPointType: form.value.hitPointType,
      abilityScoreGenerationMethod: form.value.abilityScoreGenerationMethod,
      optionalClassFeatures: form.value.optionalClassFeatures,
      customizeOrigin: form.value.customizeOrigin,
      exceedLevelCap: form.value.exceedLevelCap,
      allowMulticlassing: form.value.allowMulticlassing,
      checkMulticlassingPrerequisites: form.value.checkMulticlassingPrerequisites,
      movementSpeed: c.movementSpeed,
      swimmingSpeed: c.swimmingSpeed,
      flyingSpeed: c.flyingSpeed,
      inspirationPoints: c.inspirationPoints,
      maxHitDie: c.maxHitDie,
      currentHitDie: c.currentHitDie,
      temporaryHitPoints: c.temporaryHitPoints,
      currentHitPoints: c.currentHitPoints,
      rawMaximumHitPoints: c.rawMaximumHitPoints,
      hitPointBonus: c.hitPointBonus,
      overriddenMaximumHitPoints: c.overriddenMaximumHitPoints,
      initiativeBonus: c.initiativeBonus,
      baseArmorClass: c.baseArmorClass,
      armorClassBonus: c.armorClassBonus,
      passivePerceptionBonus: c.passivePerceptionBonus,
      passiveInvestigationBonus: c.passiveInvestigationBonus,
      passiveInsightBonus: c.passiveInsightBonus,
      xp: c.xp,
      deathSaveSuccesses: c.deathSaveSuccesses,
      deathSaveFailures: c.deathSaveFailures,
      exhaustionLevel: c.exhaustionLevel,
      conditions: c.conditions,
      damageDefenses: c.damageDefenses,
      conditionDefenses: c.conditionDefenses,
      savingThrowAdvantages: c.savingThrowAdvantages,
      savingThrowDisadvantages: c.savingThrowDisadvantages,
      blindsightRange: c.blindsightRange,
      blindsightNote: c.blindsightNote,
      darkvisionRange: c.darkvisionRange,
      darkvisionNote: c.darkvisionNote,
      tremorsenseRange: c.tremorsenseRange,
      tremorsenseNote: c.tremorsenseNote,
      truesightRange: c.truesightRange,
      truesightNote: c.truesightNote,
      languages: c.languages,
      armorProficiencies: c.armorProficiencies,
      weaponProficiencies: c.weaponProficiencies,
      toolProficiencies: c.toolProficiencies,
      countMoneyWeight: c.countMoneyWeight,
      gold: c.gold,
      electrum: c.electrum,
      silver: c.silver,
      copper: c.copper,
      arrowQuiver: c.arrowQuiver,
      boltQuiver: c.boltQuiver,
      lifestyle: c.lifestyle,
      alignment: c.alignment,
      gender: c.gender,
      size: c.size,
      age: c.age,
      heightInInches: c.heightInInches,
      weightInPounds: c.weightInPounds,
      skin: c.skin,
      hair: c.hair,
      eyes: c.eyes,
      appearance: c.appearance,
      faith: c.faith,
      personalityTraits: c.personalityTraits,
      ideals: c.ideals,
      bonds: c.bonds,
      flaws: c.flaws,
      organizations: c.organizations,
      allies: c.allies,
      enemies: c.enemies,
      backstory: c.backstory,
      notes: c.notes,
    })
    showToast({ variant: 'success', message: 'Configuration saved!' })
    if (refreshCharacter) await refreshCharacter()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to save configuration.' })
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="config-stage">
    <h2 class="config-stage__title">Character Configuration</h2>
    <p class="config-stage__desc">Set up the basic options for your character.</p>

    <div class="config-stage__section">
      <h3 class="config-stage__section-title">Basic Info</h3>
      <UiInput
        v-model="form.name"
        label="Character Name"
        placeholder="Enter character name..."
      />
    </div>

    <div class="config-stage__section">
      <h3 class="config-stage__section-title">Content Sources</h3>
      <p class="config-stage__hint">Select which content sources are available for this character.</p>
      <UiCheckboxGroup
        v-model="form.sources"
        :options="sourceOptions"
      />
    </div>

    <div class="config-stage__section">
      <h3 class="config-stage__section-title">Progression</h3>
      <UiGrid :cols="1" :cols-md="2" :gap="1">
        <UiSelect
          v-model="form.advancementType"
          label="Advancement Type"
          :options="advancementTypeOptions"
        />
        <UiSelect
          v-model="form.hitPointType"
          label="Hit Point Type"
          :options="hitPointTypeOptions"
        />
      </UiGrid>
    </div>

    <div class="config-stage__section">
      <h3 class="config-stage__section-title">Ability Scores</h3>
      <UiSelect
        v-model="form.abilityScoreGenerationMethod"
        label="Generation Method"
        :options="abilityScoreGenerationOptions"
      />
    </div>

    <div class="config-stage__section">
      <h3 class="config-stage__section-title">Optional Rules</h3>
      <div class="config-stage__switches">
        <UiSwitch v-model="form.optionalClassFeatures" label="Optional Class Features" />
        <UiSwitch v-model="form.customizeOrigin" label="Customize Your Origin (Tasha's)" />
        <UiSwitch v-model="form.exceedLevelCap" label="Allow Exceeding Level 20" />
      </div>
    </div>

    <div class="config-stage__section">
      <h3 class="config-stage__section-title">Multiclassing</h3>
      <div class="config-stage__switches">
        <UiSwitch v-model="form.allowMulticlassing" label="Allow Multiclassing" />
        <UiSwitch
          v-model="form.checkMulticlassingPrerequisites"
          label="Check Multiclassing Prerequisites"
          :disabled="!form.allowMulticlassing"
        />
      </div>
    </div>

    <div class="config-stage__actions">
      <UiButton
        :loading="saving"
        :disabled="!hasChanges"
        @click="save"
      >
        Save Configuration
      </UiButton>
    </div>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.config-stage__title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0 0 $space-1 0;
}

.config-stage__desc {
  color: $color-text-muted;
  margin: 0 0 $space-4 0;
}

.config-stage__section {
  margin-bottom: $space-5;
  padding-bottom: $space-4;
  border-bottom: 1px solid $color-border-subtle;

  &:last-of-type {
    border-bottom: none;
  }
}

.config-stage__section-title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 $space-2 0;
  color: $color-text;
}

.config-stage__hint {
  font-size: 0.875rem;
  color: $color-text-muted;
  margin: 0 0 $space-3 0;
}

.config-stage__switches {
  display: flex;
  flex-direction: column;
  gap: $space-3;
}

.config-stage__actions {
  display: flex;
  justify-content: flex-end;
  padding-top: $space-4;
}
</style>
