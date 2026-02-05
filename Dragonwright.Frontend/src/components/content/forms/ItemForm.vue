<script setup lang="ts">
import { ref, computed } from 'vue'
import { useContentForm } from '@/composables/useContentForm'
import { getItemsId, postItems, putItemsId } from '@/api'
import type { Item, AttackDamage } from '@/api'
import ContentFormLayout from '@/components/content/ContentFormLayout.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiCheckboxGroup from '@/components/ui/UiCheckboxGroup.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiButton from '@/components/ui/UiButton.vue'
import {
  sourceOptions, itemTypeOptions, rarityOptions, weaponTypeOptions,
  weaponPropertyOptions, masteryOptions, damageTypeOptions, damageTypeLabels,
  toolOptions, abilityScoreOptions,
} from '@/content/enums'
import { commonTips, itemTips, diceCountTip, diceValueTip, damageBonusTip } from '@/content/tips'

const props = defineProps<{ id?: string }>()

const { form, loading, saving, isEdit, save, cancel } = useContentForm<Item>({
  typeLabel: 'Item',
  listRoute: '/content/items',
  editRoute: '/content/items',
  id: props.id,
  defaultData: () => ({
    name: '',
    source: 1,
    description: '',
    type: 9,
    rarity: 0,
    isMagical: false,
    requiresAttunement: false,
    isConsumable: false,
    weightInOunces: 0,
    valueInCopper: 0,
    weaponType: null,
    weaponProperties: [],
    damages: [],
    damageTypes: [],
    mastery: null,
    baseArmorClass: null,
    armorClassBonus: 0,
    givesDisadvantageOnStealth: false,
    donningTimeInSeconds: 0,
    doffingTimeInSeconds: 0,
    toolType: null,
    rangeInFeet: 0,
    maximumRangeInFeet: 0,
    attackBonus: 0,
    isBackpack: false,
  }),
  getFn: getItemsId,
  createFn: postItems,
  updateFn: putItemsId,
})

const isWeapon = computed(() => Number(form.value.type) === 1)
const isArmor = computed(() => Number(form.value.type) === 0 || Number(form.value.type) === 12)
const isTool = computed(() => Number(form.value.type) === 10)

const dmgModalOpen = ref(false)
const dmgEditIndex = ref<number | null>(null)
const dmgForm = ref<AttackDamage>({ diceCount: 1, diceValue: 6, bonus: 0, damageType: 0 })

function openAddDamage() {
  dmgForm.value = { diceCount: 1, diceValue: 6, bonus: 0, damageType: 0 }
  dmgEditIndex.value = null
  dmgModalOpen.value = true
}

function openEditDamage(index: number) {
  dmgForm.value = { ...form.value.damages![index] }
  dmgEditIndex.value = index
  dmgModalOpen.value = true
}

function saveDamage() {
  if (!form.value.damages) form.value.damages = []
  if (dmgEditIndex.value !== null) {
    form.value.damages[dmgEditIndex.value] = { ...dmgForm.value }
  } else {
    form.value.damages.push({ ...dmgForm.value })
  }
  dmgModalOpen.value = false
}

function removeDamage(index: number) {
  form.value.damages?.splice(index, 1)
}
</script>

<template>
  <ContentFormLayout
    :title="isEdit ? 'Edit Item' : 'New Item'"
    back-route="/content/items"
    :loading="loading"
    :saving="saving"
    @save="save"
    @cancel="cancel"
  >
    <!-- Basic Info -->
    <div class="content-form__section">
      <h3 class="content-form__section-title">Basic Info</h3>
      <div class="content-form__row">
        <UiInput v-model="form.name" label="Name" placeholder="Item name" :tip="commonTips.name" />
        <UiSelect v-model="form.source" label="Source" :options="sourceOptions" :tip="commonTips.source" />
      </div>
      <UiTextarea v-model="form.description" label="Description" placeholder="Item description..." :rows="4" :tip="commonTips.description" />
      <div class="content-form__row">
        <UiSelect v-model="form.type" label="Type" :options="itemTypeOptions" :tip="itemTips.type" />
        <UiSelect v-model="form.rarity" label="Rarity" :options="rarityOptions" :tip="itemTips.rarity" />
      </div>
      <div class="content-form__row">
        <UiCheckbox v-model="form.isMagical" label="Magical" :tip="itemTips.isMagical" />
        <UiCheckbox v-model="form.requiresAttunement" label="Requires Attunement" :tip="itemTips.requiresAttunement" />
        <UiCheckbox v-model="form.isConsumable" label="Consumable" :tip="itemTips.isConsumable" />
        <UiCheckbox v-model="form.isBackpack" label="Container" :tip="itemTips.isBackpack" />
      </div>
      <div class="content-form__row">
        <UiInput v-model="form.weightInOunces" label="Weight (oz)" type="number" placeholder="0" :tip="itemTips.weight" />
        <UiInput v-model="form.valueInCopper" label="Value (cp)" type="number" placeholder="0" :tip="itemTips.value" />
      </div>
    </div>

    <!-- Weapon Fields -->
    <div v-if="isWeapon" class="content-form__section">
      <h3 class="content-form__section-title">Weapon Properties</h3>
      <div class="content-form__row">
        <UiSelect v-model="form.weaponType" label="Weapon Type" :options="weaponTypeOptions" :tip="itemTips.weaponType" />
        <UiSelect v-model="form.mastery" label="Mastery" :options="masteryOptions" placeholder="None" :tip="itemTips.mastery" />
      </div>
      <div class="content-form__row">
        <UiInput v-model="form.rangeInFeet" label="Range (ft)" type="number" placeholder="0" :tip="itemTips.weaponRange" />
        <UiInput v-model="form.maximumRangeInFeet" label="Max Range (ft)" type="number" placeholder="0" :tip="itemTips.weaponMaxRange" />
        <UiInput v-model="form.attackBonus" label="Attack Bonus" type="number" placeholder="0" :tip="itemTips.attackBonus" />
      </div>
      <UiSelect v-model="form.damageBonusAbility" label="Damage Bonus Ability" :options="abilityScoreOptions" placeholder="None" :tip="itemTips.damageBonusAbility" />
      <UiCheckboxGroup v-model="form.weaponProperties" label="Weapon Properties" :options="weaponPropertyOptions" :tip="itemTips.weaponProperties" />
      <UiCheckboxGroup v-model="form.damageTypes" label="Damage Types" :options="damageTypeOptions" />

      <div class="content-form__section-header" style="margin-top: 1rem">
        <h3 class="content-form__section-title">Damage Rolls</h3>
        <UiButton size="sm" left-icon="fas fa-plus" @click="openAddDamage">Add</UiButton>
      </div>
      <table v-if="form.damages?.length" class="content-table content-form__nested-table">
        <thead>
          <tr>
            <th>Dice</th>
            <th>Bonus</th>
            <th>Type</th>
            <th class="content-table__actions-col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(d, i) in form.damages" :key="i">
            <td>{{ d.diceCount }}d{{ d.diceValue }}</td>
            <td>{{ d.bonus ? `+${d.bonus}` : '—' }}</td>
            <td>{{ damageTypeLabels[d.damageType ?? 0] ?? '—' }}</td>
            <td class="content-table__actions-col">
              <button type="button" class="content-table__action-btn" title="Edit" @click="openEditDamage(i)"><i class="fas fa-pen" /></button>
              <button type="button" class="content-table__action-btn content-table__action-btn--danger" title="Delete" @click="removeDamage(i)"><i class="fas fa-trash" /></button>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else class="content-form__empty-hint">No damage rolls added.</p>
    </div>

    <!-- Armor Fields -->
    <div v-if="isArmor" class="content-form__section">
      <h3 class="content-form__section-title">Armor Properties</h3>
      <div class="content-form__row">
        <UiInput v-model="form.baseArmorClass" label="Base AC" type="number" placeholder="10" :tip="itemTips.baseArmorClass" />
        <UiInput v-model="form.armorClassBonus" label="AC Bonus" type="number" placeholder="0" :tip="itemTips.acBonus" />
      </div>
      <UiSelect v-model="form.armorClassBonusAbility" label="AC Bonus Ability" :options="abilityScoreOptions" placeholder="None" :tip="itemTips.acBonusAbility" />
      <UiInput v-model="form.maximumArmorClassBonusFromAbility" label="Max AC Bonus from Ability" type="number" placeholder="0" :tip="itemTips.maxAcBonusFromAbility" />
      <UiCheckbox v-model="form.givesDisadvantageOnStealth" label="Disadvantage on Stealth" :tip="itemTips.stealthDisadvantage" />
      <div class="content-form__row">
        <UiInput v-model="form.donningTimeInSeconds" label="Don Time (seconds)" type="number" placeholder="0" :tip="itemTips.donTime" />
        <UiInput v-model="form.doffingTimeInSeconds" label="Doff Time (seconds)" type="number" placeholder="0" :tip="itemTips.doffTime" />
      </div>
      <div class="content-form__row">
        <UiSelect v-model="form.requiredAbilityScore" label="Required Ability" :options="abilityScoreOptions" placeholder="None" :tip="itemTips.requiredAbility" />
        <UiInput v-model="form.requiredAbilityScoreValue" label="Required Score" type="number" placeholder="0" :tip="itemTips.requiredAbilityValue" />
      </div>
    </div>

    <!-- Tool Fields -->
    <div v-if="isTool" class="content-form__section">
      <h3 class="content-form__section-title">Tool Properties</h3>
      <UiSelect v-model="form.toolType" label="Tool Type" :options="toolOptions" placeholder="None" :tip="itemTips.toolType" />
    </div>

    <!-- Damage Modal -->
    <UiModal v-model="dmgModalOpen" :title="dmgEditIndex !== null ? 'Edit Damage' : 'Add Damage'" close-on-backdrop close-on-esc>
      <div class="content-form__modal-body">
        <div class="content-form__row">
          <UiInput v-model="dmgForm.diceCount" label="Dice Count" type="number" placeholder="1" :tip="diceCountTip" />
          <UiInput v-model="dmgForm.diceValue" label="Dice Value" type="number" placeholder="6" :tip="diceValueTip" />
          <UiInput v-model="dmgForm.bonus" label="Bonus" type="number" placeholder="0" :tip="damageBonusTip" />
        </div>
        <UiSelect v-model="dmgForm.damageType" label="Damage Type" :options="damageTypeOptions" />
      </div>
      <template #footer>
        <UiButton @click="saveDamage">Save</UiButton>
        <UiButton variant="ghost" @click="dmgModalOpen = false">Cancel</UiButton>
      </template>
    </UiModal>
  </ContentFormLayout>
</template>
