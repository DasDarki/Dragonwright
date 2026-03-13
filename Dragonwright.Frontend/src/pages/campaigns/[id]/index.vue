<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTitle } from '@/composables/useTitle'
import { useToast } from '@/composables/useToast'
import { useConfirm } from '@/composables/useConfirm'
import { useAuthStore } from '@/stores/auth'
import { useImageUrl } from '@/composables/useFileUpload'
import {
  getCampaign,
  updateCampaign,
  deleteCampaign,
  regenerateInviteCode,
  leaveCampaign,
  kickMember,
  linkCharacter,
  setCharacterVisibility,
} from '@/api/campaigns'
import type { CampaignResponse, CampaignMemberResponse } from '@/api/campaigns'
import { getCharacters } from '@/api'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiBadge from '@/components/ui/UiBadge.vue'

const route = useRoute()
const router = useRouter()
const { showToast } = useToast()
const { confirm } = useConfirm()
const auth = useAuthStore()

const campaignId = route.params.id as string
const campaign = ref<CampaignResponse | null>(null)
const loading = ref(true)

useTitle(() => campaign.value?.name ?? 'Campaign')

// Edit state
const editing = ref(false)
const editName = ref('')
const editDesc = ref('')
const saving = ref(false)

// Link character modal
const showLinkModal = ref(false)
const myCharacters = ref<any[]>([])
const loadingChars = ref(false)
const selectedCharId = ref<string | null>(null)
const linking = ref(false)

// Visibility
const visibilityLabels: Record<string, string> = {
  Private: 'Private',
  CampaignPrivate: 'Campaign Private',
  SemiPublic: 'Semi-Public',
  Public: 'Public',
}
const visibilityValues: Record<string, number> = {
  Private: 0,
  CampaignPrivate: 1,
  SemiPublic: 2,
  Public: 3,
}
const visibilityOptions = ['Private', 'CampaignPrivate', 'SemiPublic', 'Public']

const currentUserId = computed(() => auth.loggedInUser?.id)

const myMembership = computed(() =>
  campaign.value?.members.find(m => m.user.id === currentUserId.value)
)

const isGm = computed(() => campaign.value?.isGameMaster ?? false)

const inviteUrl = computed(() => {
  if (!campaign.value) return ''
  return campaign.value.inviteCode
})

async function fetchCampaign() {
  loading.value = true
  try {
    const res = await getCampaign(campaignId)
    const data = (res as any).data
    if ((res as any).status >= 400 || !data) {
      showToast({ variant: 'danger', message: 'Campaign not found.' })
      await router.push('/campaigns')
      return
    }
    campaign.value = data
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load campaign.' })
    await router.push('/campaigns')
  } finally {
    loading.value = false
  }
}

function startEdit() {
  editName.value = campaign.value?.name ?? ''
  editDesc.value = campaign.value?.description ?? ''
  editing.value = true
}

async function saveEdit() {
  if (!editName.value.trim()) return
  saving.value = true
  try {
    const res = await updateCampaign(campaignId, { name: editName.value.trim(), description: editDesc.value.trim() })
    campaign.value = (res as any).data
    editing.value = false
    showToast({ variant: 'success', message: 'Campaign updated.' })
  } catch {
    showToast({ variant: 'danger', message: 'Failed to update campaign.' })
  } finally {
    saving.value = false
  }
}

async function handleRegenInvite() {
  const ok = await confirm({
    title: 'Regenerate Invite Code',
    message: 'The old invite code will stop working. Continue?',
    confirmText: 'Regenerate',
    cancelText: 'Cancel',
  })
  if (!ok) return
  try {
    const res = await regenerateInviteCode(campaignId)
    const data = (res as any).data
    if (campaign.value && data?.inviteCode) {
      campaign.value.inviteCode = data.inviteCode
    }
    showToast({ variant: 'success', message: 'Invite code regenerated.' })
  } catch {
    showToast({ variant: 'danger', message: 'Failed to regenerate invite code.' })
  }
}

async function copyInviteCode() {
  try {
    await navigator.clipboard.writeText(campaign.value?.inviteCode ?? '')
    showToast({ variant: 'success', message: 'Invite code copied!' })
  } catch {
    showToast({ variant: 'danger', message: 'Failed to copy.' })
  }
}

async function handleDelete() {
  const ok = await confirm({
    title: 'Delete Campaign',
    message: `Delete "${campaign.value?.name}"? This cannot be undone.`,
    confirmText: 'Delete',
    cancelText: 'Cancel',
    danger: true,
  })
  if (!ok) return
  try {
    await deleteCampaign(campaignId)
    showToast({ variant: 'success', message: 'Campaign deleted.' })
    await router.push('/campaigns')
  } catch {
    showToast({ variant: 'danger', message: 'Failed to delete campaign.' })
  }
}

async function handleLeave() {
  const ok = await confirm({
    title: 'Leave Campaign',
    message: 'Are you sure you want to leave this campaign?',
    confirmText: 'Leave',
    cancelText: 'Cancel',
    danger: true,
  })
  if (!ok) return
  try {
    await leaveCampaign(campaignId)
    showToast({ variant: 'success', message: 'You left the campaign.' })
    await router.push('/campaigns')
  } catch {
    showToast({ variant: 'danger', message: 'Failed to leave campaign.' })
  }
}

async function handleKick(member: CampaignMemberResponse) {
  const ok = await confirm({
    title: 'Kick Member',
    message: `Kick ${member.user.username} from the campaign?`,
    confirmText: 'Kick',
    cancelText: 'Cancel',
    danger: true,
  })
  if (!ok) return
  try {
    await kickMember(campaignId, member.id)
    showToast({ variant: 'success', message: `${member.user.username} kicked.` })
    await fetchCampaign()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to kick member.' })
  }
}

async function openLinkModal() {
  showLinkModal.value = true
  loadingChars.value = true
  try {
    const res = await getCharacters({ page: 1, pageSize: 100 })
    const body = (res as any).data
    myCharacters.value = body?.items ?? []
    selectedCharId.value = myMembership.value?.character?.id ?? null
  } catch {
    myCharacters.value = []
  } finally {
    loadingChars.value = false
  }
}

async function handleLink() {
  linking.value = true
  try {
    await linkCharacter(campaignId, selectedCharId.value)
    showToast({ variant: 'success', message: selectedCharId.value ? 'Character linked.' : 'Character unlinked.' })
    showLinkModal.value = false
    await fetchCampaign()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to link character.' })
  } finally {
    linking.value = false
  }
}

async function handleVisibilityChange(value: string) {
  try {
    await setCharacterVisibility(campaignId, visibilityValues[value])
    showToast({ variant: 'success', message: 'Visibility updated.' })
    await fetchCampaign()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to update visibility.' })
  }
}

function avatarUrl(avatarId?: string | null) {
  return avatarId ? useImageUrl(avatarId) : null
}

function memberAvatarInitial(m: CampaignMemberResponse) {
  return m.user.username.slice(0, 1).toUpperCase()
}

onMounted(fetchCampaign)
</script>

<template>
  <div v-if="loading" class="campaign-detail__loading">
    <i class="fas fa-spinner fa-spin" /> Loading...
  </div>

  <div v-else-if="campaign" class="campaign-detail">
    <!-- Header -->
    <div class="campaign-detail__header">
      <div class="campaign-detail__header-left">
        <button class="campaign-detail__back" @click="router.push('/campaigns')">
          <i class="fas fa-arrow-left" />
        </button>
        <div>
          <h1 v-if="!editing" class="campaign-detail__title">
            {{ campaign.name }}
            <button v-if="isGm" class="campaign-detail__edit-btn" title="Edit" @click="startEdit">
              <i class="fas fa-pen" />
            </button>
          </h1>
          <div v-else class="campaign-detail__edit-form">
            <UiInput v-model="editName" placeholder="Campaign name" />
            <textarea v-model="editDesc" class="form-textarea" rows="2" placeholder="Description..." />
            <div class="campaign-detail__edit-actions">
              <UiButton size="sm" variant="secondary" @click="editing = false">Cancel</UiButton>
              <UiButton size="sm" :loading="saving" :disabled="!editName.trim()" @click="saveEdit">Save</UiButton>
            </div>
          </div>
          <p v-if="!editing && campaign.description" class="campaign-detail__desc">{{ campaign.description }}</p>
        </div>
      </div>

      <div class="campaign-detail__header-right">
        <UiBadge :label="isGm ? 'Game Master' : 'Player'" :variant="isGm ? 'accent' : 'info'" />
        <UiButton v-if="!isGm" size="sm" variant="danger" @click="handleLeave">
          <i class="fas fa-sign-out-alt" /> Leave
        </UiButton>
        <UiButton v-if="isGm" size="sm" variant="danger" @click="handleDelete">
          <i class="fas fa-trash" /> Delete
        </UiButton>
      </div>
    </div>

    <!-- Invite Code (GM only) -->
    <div v-if="isGm" class="campaign-detail__invite">
      <span class="campaign-detail__invite-label">Invite Code:</span>
      <code class="campaign-detail__invite-code">{{ inviteUrl }}</code>
      <UiButton size="sm" variant="ghost" @click="copyInviteCode" title="Copy">
        <i class="fas fa-copy" />
      </UiButton>
      <UiButton size="sm" variant="ghost" @click="handleRegenInvite" title="Regenerate">
        <i class="fas fa-rotate" />
      </UiButton>
    </div>

    <!-- My Character (Player only) -->
    <div v-if="!isGm && myMembership" class="campaign-detail__my-char">
      <h2 class="campaign-detail__section-title">My Character</h2>
      <div class="campaign-detail__my-char-info">
        <div v-if="myMembership.character && !myMembership.character.isHidden" class="campaign-detail__char-card">
          <span class="campaign-detail__char-avatar" :class="[!avatarUrl(myMembership.character.avatarId) && 'is-empty']">
            <img v-if="avatarUrl(myMembership.character.avatarId)" :src="avatarUrl(myMembership.character.avatarId)!" alt="" />
            <span v-else>{{ (myMembership.character.name ?? '?').slice(0, 1).toUpperCase() }}</span>
          </span>
          <span class="campaign-detail__char-name">{{ myMembership.character.name }}</span>
          <span v-if="myMembership.character.level" class="campaign-detail__char-level">Lv {{ myMembership.character.level }}</span>
        </div>
        <span v-else class="campaign-detail__no-char">No character linked</span>

        <div class="campaign-detail__my-char-actions">
          <UiButton size="sm" variant="secondary" @click="openLinkModal">
            <i class="fas fa-link" /> {{ myMembership.character ? 'Change' : 'Link Character' }}
          </UiButton>

          <div v-if="myMembership.character" class="campaign-detail__visibility-select">
            <label class="campaign-detail__vis-label">Visibility:</label>
            <select
              :value="myMembership.characterVisibility"
              class="campaign-detail__vis-dropdown"
              @change="handleVisibilityChange(($event.target as HTMLSelectElement).value)"
            >
              <option v-for="opt in visibilityOptions" :key="opt" :value="opt">
                {{ visibilityLabels[opt] }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Members List -->
    <div class="campaign-detail__members">
      <h2 class="campaign-detail__section-title">
        Members
        <span class="campaign-detail__member-count">({{ campaign.members.length }})</span>
      </h2>

      <!-- GM info -->
      <div class="member-row member-row--gm">
        <span class="member-row__avatar" :class="[!avatarUrl(campaign.gameMaster.avatarId) && 'is-empty']">
          <img v-if="avatarUrl(campaign.gameMaster.avatarId)" :src="avatarUrl(campaign.gameMaster.avatarId)!" alt="" />
          <span v-else>{{ campaign.gameMaster.username.slice(0, 1).toUpperCase() }}</span>
        </span>
        <span class="member-row__name">{{ campaign.gameMaster.username }}</span>
        <UiBadge label="GM" variant="accent" />
      </div>

      <!-- Players -->
      <div v-for="m in campaign.members" :key="m.id" class="member-row">
        <span class="member-row__avatar" :class="[!avatarUrl(m.user.avatarId) && 'is-empty']">
          <img v-if="avatarUrl(m.user.avatarId)" :src="avatarUrl(m.user.avatarId)!" alt="" />
          <span v-else>{{ memberAvatarInitial(m) }}</span>
        </span>

        <span class="member-row__name">{{ m.user.username }}</span>

        <!-- Character info -->
        <div v-if="m.character" class="member-row__char">
          <template v-if="m.character.isHidden">
            <span class="member-row__hidden">???</span>
          </template>
          <template v-else>
            <RouterLink
              v-if="m.character.id"
              :to="`/characters/${m.character.id}`"
              class="member-row__char-link"
              @click.stop
            >
              {{ m.character.name ?? '???' }}
            </RouterLink>
            <span v-if="m.character.level != null" class="member-row__char-level">Lv {{ m.character.level }}</span>
          </template>
        </div>
        <span v-else class="member-row__no-char">No character</span>

        <UiBadge
          :label="visibilityLabels[m.characterVisibility] ?? m.characterVisibility"
          variant="muted"
        />

        <button
          v-if="isGm"
          class="member-row__kick"
          title="Kick"
          @click="handleKick(m)"
        >
          <i class="fas fa-user-minus" />
        </button>
      </div>

      <p v-if="campaign.members.length === 0" class="campaign-detail__no-members">
        No players have joined yet. Share the invite code!
      </p>
    </div>

    <!-- Link Character Modal -->
    <UiModal v-model="showLinkModal" title="Link Character" close-on-backdrop close-on-esc>
      <div v-if="loadingChars" class="campaign-detail__loading-inline">
        <i class="fas fa-spinner fa-spin" /> Loading characters...
      </div>
      <div v-else-if="myCharacters.length === 0" class="campaign-detail__no-chars-msg">
        You have no characters. Create one first!
      </div>
      <div v-else class="campaign-detail__char-list">
        <label
          v-for="ch in myCharacters"
          :key="ch.id"
          class="campaign-detail__char-option"
          :class="[selectedCharId === ch.id && 'is-selected']"
        >
          <input type="radio" name="char" :value="ch.id" v-model="selectedCharId" class="sr-only" />
          <span class="campaign-detail__char-option-name">{{ ch.name }}</span>
          <span v-if="ch.level > 0" class="campaign-detail__char-option-level">Lv {{ ch.level }}</span>
        </label>
        <label class="campaign-detail__char-option" :class="[selectedCharId === null && 'is-selected']">
          <input type="radio" name="char" :value="null" v-model="selectedCharId" class="sr-only" />
          <span class="campaign-detail__char-option-name">Unlink character</span>
        </label>
      </div>
      <template #footer>
        <UiButton variant="secondary" @click="showLinkModal = false">Cancel</UiButton>
        <UiButton :loading="linking" @click="handleLink">Confirm</UiButton>
      </template>
    </UiModal>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.campaign-detail {
  max-width: 900px;
  margin: 0 auto;
  padding: 0 $space-4;
}

.campaign-detail__loading {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;
}

.campaign-detail__loading-inline {
  padding: $space-4;
  text-align: center;
  color: $color-text-muted;
}

.campaign-detail__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: $space-4;
  margin-bottom: $space-4;
  flex-wrap: wrap;
}

.campaign-detail__header-left {
  display: flex;
  align-items: flex-start;
  gap: $space-3;
  min-width: 0;
  flex: 1;
}

.campaign-detail__back {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  margin-top: $space-1;

  &:hover {
    border-color: $color-border-strong;
  }
}

.campaign-detail__header-right {
  display: flex;
  align-items: center;
  gap: $space-2;
  flex-shrink: 0;
}

.campaign-detail__title {
  font-size: 1.75rem;
  font-weight: 700;
  color: $color-text;
  margin: 0;
  display: flex;
  align-items: center;
  gap: $space-2;
}

.campaign-detail__edit-btn {
  background: none;
  border: none;
  color: $color-text-muted;
  cursor: pointer;
  font-size: 0.9rem;

  &:hover {
    color: $color-accent;
  }
}

.campaign-detail__edit-form {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.campaign-detail__edit-actions {
  display: flex;
  gap: $space-2;
}

.campaign-detail__desc {
  font-size: 0.95rem;
  color: $color-text-muted;
  margin: $space-1 0 0 0;
}

.campaign-detail__invite {
  display: flex;
  align-items: center;
  gap: $space-2;
  padding: $space-3;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-lg;
  margin-bottom: $space-4;
  flex-wrap: wrap;
}

.campaign-detail__invite-label {
  font-size: 0.875rem;
  color: $color-text-muted;
  font-weight: 600;
}

.campaign-detail__invite-code {
  font-family: monospace;
  font-size: 1rem;
  color: $color-accent;
  background: rgba(249, 115, 22, 0.08);
  padding: $space-1 $space-2;
  border-radius: $radius-md;
  user-select: all;
}

.campaign-detail__section-title {
  font-size: 1.2rem;
  font-weight: 700;
  color: $color-text;
  margin: 0 0 $space-3 0;
}

.campaign-detail__member-count {
  font-weight: 400;
  color: $color-text-muted;
  font-size: 1rem;
}

.campaign-detail__my-char {
  margin-bottom: $space-4;
  padding: $space-4;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-lg;
}

.campaign-detail__my-char-info {
  display: flex;
  align-items: center;
  gap: $space-3;
  flex-wrap: wrap;
}

.campaign-detail__char-card {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.campaign-detail__char-avatar {
  width: 2rem;
  height: 2rem;
  border-radius: $radius-pill;
  overflow: hidden;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
  font-weight: 700;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &.is-empty {
    background: rgba(249, 115, 22, 0.12);
    border-color: rgba(249, 115, 22, 0.25);
    color: $color-accent;
  }
}

.campaign-detail__char-name {
  font-weight: 600;
  color: $color-text;
}

.campaign-detail__char-level {
  font-size: 0.8rem;
  color: $color-accent;
  font-weight: 600;
}

.campaign-detail__no-char {
  font-size: 0.9rem;
  color: $color-text-soft;
  font-style: italic;
}

.campaign-detail__my-char-actions {
  display: flex;
  align-items: center;
  gap: $space-3;
  margin-left: auto;
  flex-wrap: wrap;
}

.campaign-detail__visibility-select {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.campaign-detail__vis-label {
  font-size: 0.8rem;
  color: $color-text-muted;
  font-weight: 600;
}

.campaign-detail__vis-dropdown {
  padding: $space-1 $space-2;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text;
  font-size: 0.85rem;
  cursor: pointer;

  &:focus {
    outline: none;
    border-color: $color-accent-alt;
  }
}

.campaign-detail__members {
  margin-bottom: $space-6;
}

.campaign-detail__no-members {
  color: $color-text-soft;
  font-style: italic;
  padding: $space-3;
}

.member-row {
  display: flex;
  align-items: center;
  gap: $space-3;
  padding: $space-3;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-lg;
  margin-bottom: $space-2;

  &--gm {
    border-color: rgba(249, 115, 22, 0.25);
    background: rgba(249, 115, 22, 0.04);
  }
}

.member-row__avatar {
  width: 2rem;
  height: 2rem;
  border-radius: $radius-pill;
  overflow: hidden;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
  font-weight: 700;
  flex-shrink: 0;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &.is-empty {
    background: rgba(249, 115, 22, 0.12);
    border-color: rgba(249, 115, 22, 0.25);
    color: $color-accent;
  }
}

.member-row__name {
  font-weight: 600;
  color: $color-text;
  min-width: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.member-row__char {
  display: flex;
  align-items: center;
  gap: $space-2;
  margin-left: auto;
}

.member-row__char-link {
  color: $color-accent-alt;
  text-decoration: none;
  font-weight: 500;

  &:hover {
    text-decoration: underline;
  }
}

.member-row__char-level {
  font-size: 0.8rem;
  color: $color-accent;
  font-weight: 600;
}

.member-row__hidden {
  color: $color-text-soft;
  font-style: italic;
}

.member-row__no-char {
  color: $color-text-soft;
  font-style: italic;
  font-size: 0.85rem;
  margin-left: auto;
}

.member-row__kick {
  width: 2rem;
  height: 2rem;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: transparent;
  color: $color-text-muted;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 150ms ease;
  flex-shrink: 0;

  &:hover {
    border-color: $color-danger;
    color: $color-danger;
    background: rgba(239, 68, 68, 0.08);
  }
}

.campaign-detail__no-chars-msg {
  padding: $space-4;
  text-align: center;
  color: $color-text-soft;
}

.campaign-detail__char-list {
  display: flex;
  flex-direction: column;
  gap: $space-2;
}

.campaign-detail__char-option {
  display: flex;
  align-items: center;
  gap: $space-3;
  padding: $space-3;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-md;
  cursor: pointer;
  transition: border-color 150ms ease, background 150ms ease;

  &:hover {
    border-color: $color-border-strong;
  }

  &.is-selected {
    border-color: $color-accent;
    background: rgba(249, 115, 22, 0.06);
  }
}

.campaign-detail__char-option-name {
  font-weight: 600;
  color: $color-text;
}

.campaign-detail__char-option-level {
  font-size: 0.8rem;
  color: $color-accent;
  font-weight: 600;
  margin-left: auto;
}

.form-textarea {
  width: 100%;
  padding: $space-2 $space-3;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text;
  font-family: inherit;
  font-size: 0.9rem;
  resize: vertical;

  &:focus {
    outline: none;
    border-color: $color-accent-alt;
    box-shadow: 0 0 0 2px $color-bg-elevated, 0 0 0 3px $color-focus;
  }
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border-width: 0;
}

@media (max-width: 640px) {
  .campaign-detail {
    padding: 0 $space-3;
  }

  .campaign-detail__header {
    flex-direction: column;
  }

  .campaign-detail__my-char-actions {
    margin-left: 0;
    width: 100%;
  }

  .member-row {
    flex-wrap: wrap;
  }

  .member-row__char {
    margin-left: 0;
  }
}
</style>
