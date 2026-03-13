<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useTitle } from '@/composables/useTitle'
import { useToast } from '@/composables/useToast'
import { useConfirm } from '@/composables/useConfirm'
import { getCampaigns, createCampaign, deleteCampaign, joinCampaign } from '@/api/campaigns'
import type { CampaignListItem } from '@/api/campaigns'
import { useImageUrl } from '@/composables/useFileUpload'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiGrid from '@/components/ui/layout/UiGrid.vue'

useTitle('Campaigns')

const router = useRouter()
const { showToast } = useToast()
const { confirm } = useConfirm()

const campaigns = ref<CampaignListItem[]>([])
const loading = ref(false)
const search = ref('')
const page = ref(1)
const totalCount = ref(0)
const pageSize = 20

const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize)))

// Create modal
const showCreateModal = ref(false)
const newName = ref('')
const newDesc = ref('')
const creating = ref(false)

// Join modal
const showJoinModal = ref(false)
const inviteCode = ref('')
const joining = ref(false)

async function fetchCampaigns() {
  loading.value = true
  try {
    const params: any = { page: page.value, pageSize }
    if (search.value) params.search = search.value
    const res = await getCampaigns(params)
    const body = (res as any).data
    if (body && 'items' in body) {
      campaigns.value = body.items
      totalCount.value = body.totalCount
    } else {
      campaigns.value = []
      totalCount.value = 0
    }
  } catch {
    showToast({ variant: 'danger', message: 'Failed to load campaigns.' })
  } finally {
    loading.value = false
  }
}

let searchTimeout: ReturnType<typeof setTimeout> | null = null
watch(search, () => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => { page.value = 1; fetchCampaigns() }, 300)
})
watch(page, fetchCampaigns)
fetchCampaigns()

async function handleCreate() {
  if (!newName.value.trim()) return
  creating.value = true
  try {
    const res = await createCampaign({ name: newName.value.trim(), description: newDesc.value.trim() })
    const data = (res as any).data
    showToast({ variant: 'success', message: 'Campaign created!' })
    showCreateModal.value = false
    newName.value = ''
    newDesc.value = ''
    if (data?.id) {
      await router.push(`/campaigns/${data.id}`)
    } else {
      await fetchCampaigns()
    }
  } catch {
    showToast({ variant: 'danger', message: 'Failed to create campaign.' })
  } finally {
    creating.value = false
  }
}

async function handleJoin() {
  if (!inviteCode.value.trim()) return
  joining.value = true
  try {
    const res = await joinCampaign(inviteCode.value.trim())
    const data = (res as any).data
    if ((res as any).status >= 400) {
      showToast({ variant: 'danger', message: (data as any) || 'Failed to join campaign.' })
    } else {
      showToast({ variant: 'success', message: 'Joined campaign!' })
      showJoinModal.value = false
      inviteCode.value = ''
      if (data?.campaignId) {
        await router.push(`/campaigns/${data.campaignId}`)
      } else {
        await fetchCampaigns()
      }
    }
  } catch {
    showToast({ variant: 'danger', message: 'Failed to join campaign.' })
  } finally {
    joining.value = false
  }
}

async function handleDelete(campaign: CampaignListItem) {
  const ok = await confirm({
    title: 'Delete Campaign',
    message: `Are you sure you want to delete "${campaign.name}"? All members will be removed. This cannot be undone.`,
    confirmText: 'Delete',
    cancelText: 'Cancel',
    danger: true,
  })
  if (!ok) return
  try {
    await deleteCampaign(campaign.id)
    showToast({ variant: 'success', message: 'Campaign deleted.' })
    await fetchCampaigns()
  } catch {
    showToast({ variant: 'danger', message: 'Failed to delete campaign.' })
  }
}

function gmAvatarUrl(c: CampaignListItem) {
  return c.gameMaster.avatarId ? useImageUrl(c.gameMaster.avatarId) : null
}
</script>

<template>
  <div class="campaigns-page">
    <div class="campaigns-page__header">
      <h1 class="campaigns-page__title">Campaigns</h1>
      <div class="campaigns-page__actions">
        <UiButton variant="secondary" @click="showJoinModal = true">
          <i class="fas fa-link" /> Join
        </UiButton>
        <UiButton @click="showCreateModal = true">
          <i class="fas fa-plus" /> New Campaign
        </UiButton>
      </div>
    </div>

    <div class="campaigns-page__search">
      <UiInput v-model="search" placeholder="Search campaigns..." left-icon="fas fa-search" />
    </div>

    <div v-if="loading && campaigns.length === 0" class="campaigns-page__loading">
      <i class="fas fa-spinner fa-spin" /> Loading...
    </div>

    <UiEmptyState
      v-else-if="campaigns.length === 0"
      icon="fas fa-scroll"
      title="No campaigns yet"
      subtitle="Create a new campaign or join one with an invite code!"
    />

    <UiGrid v-else :cols="1" :cols-md="2" :cols-lg="3" :gap="1">
      <div
        v-for="c in campaigns"
        :key="c.id"
        class="campaign-card"
        @click="router.push(`/campaigns/${c.id}`)"
      >
        <div class="campaign-card__top">
          <span class="campaign-card__role-badge" :class="[c.isGameMaster ? 'is-gm' : 'is-player']">
            {{ c.isGameMaster ? 'GM' : 'Player' }}
          </span>
        </div>

        <h3 class="campaign-card__name">{{ c.name }}</h3>
        <p v-if="c.description" class="campaign-card__desc">{{ c.description }}</p>

        <div class="campaign-card__footer">
          <div class="campaign-card__gm">
            <span class="campaign-card__gm-avatar" :class="[!gmAvatarUrl(c) && 'is-empty']">
              <img v-if="gmAvatarUrl(c)" :src="gmAvatarUrl(c)!" alt="" />
              <span v-else>{{ c.gameMaster.username.slice(0, 1).toUpperCase() }}</span>
            </span>
            <span class="campaign-card__gm-name">{{ c.gameMaster.username }}</span>
          </div>
          <span class="campaign-card__members">
            <i class="fas fa-users" /> {{ c.memberCount }}
          </span>
        </div>

        <div v-if="c.isGameMaster" class="campaign-card__actions" @click.stop>
          <button class="campaign-card__action campaign-card__action--danger" title="Delete" @click="handleDelete(c)">
            <i class="fas fa-trash" />
          </button>
        </div>
      </div>
    </UiGrid>

    <div v-if="totalPages > 1" class="campaigns-page__pagination">
      <button :disabled="page <= 1" @click="page--">Previous</button>
      <span>Page {{ page }} of {{ totalPages }}</span>
      <button :disabled="page >= totalPages" @click="page++">Next</button>
    </div>

    <!-- Create Campaign Modal -->
    <UiModal v-model="showCreateModal" title="New Campaign" close-on-backdrop close-on-esc>
      <form @submit.prevent="handleCreate">
        <div class="form-group">
          <label class="form-label">Name</label>
          <UiInput v-model="newName" placeholder="Campaign name" />
        </div>
        <div class="form-group">
          <label class="form-label">Description</label>
          <textarea v-model="newDesc" class="form-textarea" placeholder="Optional description..." rows="3" />
        </div>
      </form>
      <template #footer>
        <UiButton variant="secondary" @click="showCreateModal = false">Cancel</UiButton>
        <UiButton :loading="creating" :disabled="!newName.trim()" @click="handleCreate">Create</UiButton>
      </template>
    </UiModal>

    <!-- Join Campaign Modal -->
    <UiModal v-model="showJoinModal" title="Join Campaign" close-on-backdrop close-on-esc>
      <form @submit.prevent="handleJoin">
        <div class="form-group">
          <label class="form-label">Invite Code</label>
          <UiInput v-model="inviteCode" placeholder="Enter invite code..." />
        </div>
      </form>
      <template #footer>
        <UiButton variant="secondary" @click="showJoinModal = false">Cancel</UiButton>
        <UiButton :loading="joining" :disabled="!inviteCode.trim()" @click="handleJoin">Join</UiButton>
      </template>
    </UiModal>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.campaigns-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 $space-4;
}

.campaigns-page__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-4;
  margin-bottom: $space-4;
  flex-wrap: wrap;
}

.campaigns-page__title {
  font-size: 1.75rem;
  font-weight: 700;
  color: $color-text;
  margin: 0;
}

.campaigns-page__actions {
  display: flex;
  gap: $space-2;
}

.campaigns-page__search {
  margin-bottom: $space-4;
  max-width: 400px;
}

.campaigns-page__loading {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;
}

.campaigns-page__pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: $space-4;
  margin-top: $space-6;
  color: $color-text-muted;

  button {
    padding: $space-2 $space-4;
    border-radius: $radius-md;
    border: 1px solid $color-border-subtle;
    background: $color-surface;
    color: $color-text;
    cursor: pointer;

    &:hover:not(:disabled) {
      border-color: $color-border-strong;
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }
}

.campaign-card {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: $space-2;
  padding: $space-4;
  background: $color-surface;
  border: 1px solid $color-border-subtle;
  border-radius: $radius-lg;
  cursor: pointer;
  transition: border-color 150ms ease, box-shadow 150ms ease;

  &:hover {
    border-color: $color-border-strong;
    box-shadow: $shadow-card;
  }
}

.campaign-card__top {
  display: flex;
  justify-content: flex-end;
}

.campaign-card__role-badge {
  font-size: 0.7rem;
  padding: 0.15rem 0.5rem;
  border-radius: $radius-pill;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  font-weight: 600;

  &.is-gm {
    border: 1px solid rgba(249, 115, 22, 0.35);
    background: rgba(249, 115, 22, 0.14);
    color: $color-accent;
  }

  &.is-player {
    border: 1px solid rgba(56, 189, 248, 0.35);
    background: rgba(56, 189, 248, 0.12);
    color: $color-accent-alt;
  }
}

.campaign-card__name {
  font-size: 1.15rem;
  font-weight: 600;
  color: $color-text;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.campaign-card__desc {
  font-size: 0.85rem;
  color: $color-text-muted;
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.campaign-card__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: auto;
  padding-top: $space-2;
  border-top: 1px solid $color-border-subtle;
}

.campaign-card__gm {
  display: flex;
  align-items: center;
  gap: $space-2;
}

.campaign-card__gm-avatar {
  width: 1.5rem;
  height: 1.5rem;
  border-radius: $radius-pill;
  overflow: hidden;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 0.7rem;
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

.campaign-card__gm-name {
  font-size: 0.8rem;
  color: $color-text-muted;
}

.campaign-card__members {
  font-size: 0.8rem;
  color: $color-text-muted;
  display: flex;
  align-items: center;
  gap: $space-1;
}

.campaign-card__actions {
  position: absolute;
  top: $space-3;
  left: $space-3;
}

.campaign-card__action {
  width: 2rem;
  height: 2rem;
  border-radius: $radius-md;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  color: $color-text-muted;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-border-strong;
    color: $color-text;
  }

  &--danger:hover {
    border-color: $color-danger;
    color: $color-danger;
    background: rgba(239, 68, 68, 0.08);
  }
}

.form-group {
  margin-bottom: $space-3;
}

.form-label {
  display: block;
  margin-bottom: $space-1;
  font-size: 0.875rem;
  font-weight: 600;
  color: $color-text;
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

@media (max-width: 640px) {
  .campaigns-page {
    padding: 0 $space-3;
  }
}
</style>
