<script setup lang="ts">
import {ref, computed, watch} from "vue";
import {useToast} from "@/composables/useToast";
import {putCharactersId} from "@/api";

const character = defineModel<any>("character", {required: true});
const props = defineProps<{ characterId: string }>();

const {showToast} = useToast();

const notesForm = ref({
  personalityTraits: [] as string[],
  ideals: [] as string[],
  bonds: [] as string[],
  flaws: [] as string[],
  backstory: "",
  notes: "",
  appearance: "",
  skin: "",
  hair: "",
  eyes: "",
  faith: "",
  heightInInches: 0,
  weightInPounds: 0,
});

const heightUnit = ref<"ft" | "m">("ft");
const weightUnit = ref<"lb" | "kg">("lb");
let notesTimer: ReturnType<typeof setTimeout> | null = null;
const notesSaving = ref(false);

const displayHeightCm = computed({
  get: () => Math.round(notesForm.value.heightInInches * 2.54),
  set: (cm: number) => { notesForm.value.heightInInches = Math.round(cm / 2.54); },
});

const displayWeightKg = computed({
  get: () => +(notesForm.value.weightInPounds * 0.453592).toFixed(1),
  set: (kg: number) => { notesForm.value.weightInPounds = Math.round(kg / 0.453592); },
});

const heightFeet = computed({
  get: () => Math.floor(notesForm.value.heightInInches / 12),
  set: (v: number) => { notesForm.value.heightInInches = v * 12 + (notesForm.value.heightInInches % 12); },
});

const heightInches = computed({
  get: () => notesForm.value.heightInInches % 12,
  set: (v: number) => { notesForm.value.heightInInches = heightFeet.value * 12 + v; },
});

function syncNotesFromCharacter() {
  if (!character.value) return;
  const c = character.value;
  notesForm.value = {
    personalityTraits: [...(c.personalityTraits ?? [])],
    ideals: [...(c.ideals ?? [])],
    bonds: [...(c.bonds ?? [])],
    flaws: [...(c.flaws ?? [])],
    backstory: c.backstory ?? "",
    notes: c.notes ?? "",
    appearance: c.appearance ?? "",
    skin: c.skin ?? "",
    hair: c.hair ?? "",
    eyes: c.eyes ?? "",
    faith: c.faith ?? "",
    heightInInches: c.heightInInches ?? 0,
    weightInPounds: c.weightInPounds ?? 0,
  };
}

function debouncedSaveNotes() {
  if (notesTimer) clearTimeout(notesTimer);
  notesTimer = setTimeout(saveNotesForm, 1000);
}

async function saveNotesForm() {
  if (!character.value) return;
  notesSaving.value = true;
  try {
    const f = notesForm.value;
    await putCharactersId(props.characterId, {
      name: character.value.name,
      personalityTraits: f.personalityTraits.filter(t => t.trim()),
      ideals: f.ideals.filter(t => t.trim()),
      bonds: f.bonds.filter(t => t.trim()),
      flaws: f.flaws.filter(t => t.trim()),
      backstory: f.backstory,
      notes: f.notes,
      appearance: f.appearance,
      skin: f.skin,
      hair: f.hair,
      eyes: f.eyes,
      faith: f.faith,
      heightInInches: f.heightInInches,
      weightInPounds: f.weightInPounds,
    });
    const c = character.value;
    c.personalityTraits = [...f.personalityTraits.filter(t => t.trim())];
    c.ideals = [...f.ideals.filter(t => t.trim())];
    c.bonds = [...f.bonds.filter(t => t.trim())];
    c.flaws = [...f.flaws.filter(t => t.trim())];
    c.backstory = f.backstory;
    c.notes = f.notes;
    c.appearance = f.appearance;
    c.skin = f.skin;
    c.hair = f.hair;
    c.eyes = f.eyes;
    c.faith = f.faith;
    c.heightInInches = f.heightInInches;
    c.weightInPounds = f.weightInPounds;
  } catch {
    showToast({variant: "danger", message: "Failed to save."});
  } finally {
    notesSaving.value = false;
  }
}

function addListItem(list: string[]) {
  list.push("");
  debouncedSaveNotes();
}

function removeListItem(list: string[], index: number) {
  list.splice(index, 1);
  debouncedSaveNotes();
}

watch(() => character.value, () => {
  syncNotesFromCharacter();
}, {immediate: true});
</script>

<template>
  <div class="notes-tab">
    <span v-if="notesSaving" class="notes-tab__saving">
      <i class="fas fa-spinner fa-spin"/> Saving...
    </span>

    <div class="notes-tab__section">
      <div class="notes-tab__section-header">
        <div class="bg-tab__label">Personality Traits</div>
        <button class="notes-tab__add" @click="addListItem(notesForm.personalityTraits)">
          <i class="fas fa-plus"/>
        </button>
      </div>
      <div class="notes-tab__list">
        <div v-for="(_, i) in notesForm.personalityTraits" :key="'pt' + i" class="notes-tab__list-row">
          <input
            class="notes-tab__inline-input"
            v-model="notesForm.personalityTraits[i]"
            placeholder="Personality trait..."
            @input="debouncedSaveNotes()"
          />
          <button class="notes-tab__remove" @click="removeListItem(notesForm.personalityTraits, i)">
            <i class="fas fa-times"/>
          </button>
        </div>
        <div v-if="!notesForm.personalityTraits.length" class="notes-tab__empty-hint">None yet.</div>
      </div>
    </div>

    <div class="notes-tab__section">
      <div class="notes-tab__section-header">
        <div class="bg-tab__label">Ideals</div>
        <button class="notes-tab__add" @click="addListItem(notesForm.ideals)">
          <i class="fas fa-plus"/>
        </button>
      </div>
      <div class="notes-tab__list">
        <div v-for="(_, i) in notesForm.ideals" :key="'id' + i" class="notes-tab__list-row">
          <input
            class="notes-tab__inline-input"
            v-model="notesForm.ideals[i]"
            placeholder="Ideal..."
            @input="debouncedSaveNotes()"
          />
          <button class="notes-tab__remove" @click="removeListItem(notesForm.ideals, i)">
            <i class="fas fa-times"/>
          </button>
        </div>
        <div v-if="!notesForm.ideals.length" class="notes-tab__empty-hint">None yet.</div>
      </div>
    </div>

    <div class="notes-tab__section">
      <div class="notes-tab__section-header">
        <div class="bg-tab__label">Bonds</div>
        <button class="notes-tab__add" @click="addListItem(notesForm.bonds)">
          <i class="fas fa-plus"/>
        </button>
      </div>
      <div class="notes-tab__list">
        <div v-for="(_, i) in notesForm.bonds" :key="'bo' + i" class="notes-tab__list-row">
          <input
            class="notes-tab__inline-input"
            v-model="notesForm.bonds[i]"
            placeholder="Bond..."
            @input="debouncedSaveNotes()"
          />
          <button class="notes-tab__remove" @click="removeListItem(notesForm.bonds, i)">
            <i class="fas fa-times"/>
          </button>
        </div>
        <div v-if="!notesForm.bonds.length" class="notes-tab__empty-hint">None yet.</div>
      </div>
    </div>

    <div class="notes-tab__section">
      <div class="notes-tab__section-header">
        <div class="bg-tab__label">Flaws</div>
        <button class="notes-tab__add" @click="addListItem(notesForm.flaws)">
          <i class="fas fa-plus"/>
        </button>
      </div>
      <div class="notes-tab__list">
        <div v-for="(_, i) in notesForm.flaws" :key="'fl' + i" class="notes-tab__list-row">
          <input
            class="notes-tab__inline-input"
            v-model="notesForm.flaws[i]"
            placeholder="Flaw..."
            @input="debouncedSaveNotes()"
          />
          <button class="notes-tab__remove" @click="removeListItem(notesForm.flaws, i)">
            <i class="fas fa-times"/>
          </button>
        </div>
        <div v-if="!notesForm.flaws.length" class="notes-tab__empty-hint">None yet.</div>
      </div>
    </div>

    <div class="detail-divider"/>

    <div class="notes-tab__section">
      <div class="bg-tab__label">Appearance</div>
      <div class="notes-tab__appearance-grid">
        <div class="notes-tab__field">
          <label class="notes-tab__field-label">Eyes</label>
          <input class="notes-tab__inline-input" v-model="notesForm.eyes" placeholder="Eye color..."
                 @input="debouncedSaveNotes()"/>
        </div>
        <div class="notes-tab__field">
          <label class="notes-tab__field-label">Hair</label>
          <input class="notes-tab__inline-input" v-model="notesForm.hair" placeholder="Hair color..."
                 @input="debouncedSaveNotes()"/>
        </div>
        <div class="notes-tab__field">
          <label class="notes-tab__field-label">Skin</label>
          <input class="notes-tab__inline-input" v-model="notesForm.skin" placeholder="Skin color..."
                 @input="debouncedSaveNotes()"/>
        </div>
        <div class="notes-tab__field">
          <label class="notes-tab__field-label">Faith</label>
          <input class="notes-tab__inline-input" v-model="notesForm.faith" placeholder="Faith / Deity..."
                 @input="debouncedSaveNotes()"/>
        </div>
        <div class="notes-tab__field">
          <div class="notes-tab__field-label-row">
            <label class="notes-tab__field-label">Height</label>
            <button class="notes-tab__unit-toggle" @click="heightUnit = heightUnit === 'ft' ? 'm' : 'ft'">
              {{ heightUnit === 'ft' ? 'ft/in' : 'cm' }}
            </button>
          </div>
          <div v-if="heightUnit === 'ft'" class="notes-tab__height-row">
            <input class="notes-tab__inline-input notes-tab__inline-input--sm" type="number" min="0"
                   :value="heightFeet"
                   @input="heightFeet = Number(($event.target as HTMLInputElement).value); debouncedSaveNotes()"/>
            <span class="notes-tab__unit-label">ft</span>
            <input class="notes-tab__inline-input notes-tab__inline-input--sm" type="number" min="0" max="11"
                   :value="heightInches"
                   @input="heightInches = Number(($event.target as HTMLInputElement).value); debouncedSaveNotes()"/>
            <span class="notes-tab__unit-label">in</span>
          </div>
          <div v-else class="notes-tab__height-row">
            <input class="notes-tab__inline-input notes-tab__inline-input--sm" type="number" min="0"
                   :value="displayHeightCm"
                   @input="displayHeightCm = Number(($event.target as HTMLInputElement).value); debouncedSaveNotes()"/>
            <span class="notes-tab__unit-label">cm</span>
          </div>
        </div>
        <div class="notes-tab__field">
          <div class="notes-tab__field-label-row">
            <label class="notes-tab__field-label">Weight</label>
            <button class="notes-tab__unit-toggle" @click="weightUnit = weightUnit === 'lb' ? 'kg' : 'lb'">
              {{ weightUnit === 'lb' ? 'lb' : 'kg' }}
            </button>
          </div>
          <div class="notes-tab__height-row">
            <input
              v-if="weightUnit === 'lb'"
              class="notes-tab__inline-input notes-tab__inline-input--sm"
              type="number" min="0"
              v-model.number="notesForm.weightInPounds"
              @input="debouncedSaveNotes()"
            />
            <input
              v-else
              class="notes-tab__inline-input notes-tab__inline-input--sm"
              type="number" min="0" step="0.1"
              :value="displayWeightKg"
              @input="displayWeightKg = Number(($event.target as HTMLInputElement).value); debouncedSaveNotes()"
            />
            <span class="notes-tab__unit-label">{{ weightUnit }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="notes-tab__section">
      <div class="bg-tab__label">Appearance</div>
      <textarea
        class="notes-textarea"
        v-model="notesForm.appearance"
        placeholder="Scars, tattoos, distinguishing marks..."
        rows="3"
        @input="debouncedSaveNotes()"
      />
    </div>

    <div class="detail-divider"/>

    <div class="notes-tab__section">
      <div class="bg-tab__label">Backstory</div>
      <textarea
        class="notes-textarea"
        v-model="notesForm.backstory"
        placeholder="Your character's backstory..."
        rows="6"
        @input="debouncedSaveNotes()"
      />
    </div>

    <div class="detail-divider"/>

    <div class="notes-tab__section">
      <div class="bg-tab__label">Notes</div>
      <textarea
        class="notes-textarea"
        v-model="notesForm.notes"
        placeholder="Session notes, reminders, or anything else..."
        rows="6"
        @input="debouncedSaveNotes()"
      />
    </div>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.notes-tab {
  position: relative;
}

.notes-tab__saving {
  position: absolute;
  top: $space-2;
  right: $space-3;
  font-size: 0.8rem;
  color: $color-text-muted;
}

.notes-tab__section {
  display: flex;
  flex-direction: column;
  gap: $space-2;
  margin-bottom: $space-3;
}

.notes-tab__section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.notes-tab__add {
  width: 1.5rem;
  height: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: 1px dashed $color-border-subtle;
  border-radius: $radius-sm;
  cursor: pointer;
  color: $color-text-muted;
  font-size: 0.7rem;

  &:hover { border-color: $color-accent; color: $color-accent; }
}

.notes-tab__list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.notes-tab__list-row {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.notes-tab__inline-input {
  flex: 1;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text;
  font-size: 0.9rem;
  font-family: inherit;

  &::placeholder { color: $color-text-muted; }
  &:focus { outline: none; border-color: $color-accent; }

  &--sm {
    flex: 0 0 auto;
    width: 4rem;
    text-align: center;
    padding: $space-2;
  }
}

.notes-tab__remove {
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

  &:hover { border-color: $color-danger; color: $color-danger; }
}

.notes-tab__empty-hint {
  font-size: 0.85rem;
  color: $color-text-muted;
  font-style: italic;
  padding: $space-1 0;
}

.notes-tab__appearance-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: $space-2;
}

.notes-tab__field {
  display: flex;
  flex-direction: column;
  gap: $space-1;
}

.notes-tab__field-label {
  font-size: 0.72rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  font-weight: 600;
}

.notes-tab__field-label-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-2;
}

.notes-tab__unit-toggle {
  padding: 1px $space-1;
  background: transparent;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-xs;
  cursor: pointer;
  font-size: 0.65rem;
  color: $color-text-muted;
  font-weight: 600;
  text-transform: uppercase;

  &:hover { border-color: $color-accent; color: $color-accent; }
}

.notes-tab__height-row {
  display: flex;
  align-items: center;
  gap: $space-1;
}

.notes-tab__unit-label {
  font-size: 0.8rem;
  color: $color-text-muted;
  font-weight: 500;
}

.notes-textarea {
  width: 100%;
  padding: $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text;
  font-size: 0.9rem;
  font-family: inherit;
  line-height: 1.6;
  resize: vertical;

  &::placeholder { color: $color-text-muted; }
  &:focus { outline: none; border-color: $color-accent; }
}

.bg-tab__label {
  font-size: 0.75rem;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 600;
  margin-top: $space-3;
}

.detail-divider {
  height: 1px;
  background: $color-border-subtle;
  margin: $space-3 0;
}
</style>
