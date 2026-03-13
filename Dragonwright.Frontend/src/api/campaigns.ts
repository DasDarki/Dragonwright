import { customFetch } from './http';

// ── Types ──

export interface CampaignUserInfo {
  id: string;
  username: string;
  avatarId?: string | null;
}

export interface CampaignCharacterInfo {
  id?: string | null;
  name?: string | null;
  avatarId?: string | null;
  level?: number | null;
  isHidden: boolean;
}

export interface CampaignMemberResponse {
  id: string;
  user: CampaignUserInfo;
  joinedAt: string;
  characterVisibility: string;
  character?: CampaignCharacterInfo | null;
}

export interface CampaignResponse {
  id: string;
  name: string;
  description: string;
  inviteCode: string;
  createdAt: string;
  gameMaster: CampaignUserInfo;
  isGameMaster: boolean;
  members: CampaignMemberResponse[];
}

export interface CampaignListItem {
  id: string;
  name: string;
  description: string;
  createdAt: string;
  gameMaster: CampaignUserInfo;
  isGameMaster: boolean;
  memberCount: number;
}

export interface PaginatedCampaigns {
  items: CampaignListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// ── API calls ──

export async function getCampaigns(params?: { page?: number; pageSize?: number; search?: string }) {
  const query = new URLSearchParams();
  if (params?.page) query.set('page', String(params.page));
  if (params?.pageSize) query.set('pageSize', String(params.pageSize));
  if (params?.search) query.set('search', params.search);
  const qs = query.toString();
  return customFetch<{ status: number; data: PaginatedCampaigns }>(`/campaigns${qs ? '?' + qs : ''}`);
}

export async function getCampaign(id: string) {
  return customFetch<{ status: number; data: CampaignResponse }>(`/campaigns/${id}`);
}

export async function createCampaign(body: { name: string; description?: string }) {
  return customFetch<{ status: number; data: CampaignResponse }>('/campaigns', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
}

export async function updateCampaign(id: string, body: { name: string; description?: string }) {
  return customFetch<{ status: number; data: CampaignResponse }>(`/campaigns/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
}

export async function deleteCampaign(id: string) {
  return customFetch<{ status: number }>(`/campaigns/${id}`, { method: 'DELETE' });
}

export async function regenerateInviteCode(id: string) {
  return customFetch<{ status: number; data: { inviteCode: string } }>(`/campaigns/${id}/regenerate-invite`, {
    method: 'POST',
  });
}

export async function joinCampaign(inviteCode: string) {
  return customFetch<{ status: number; data: { campaignId: string } }>('/campaigns/join', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ inviteCode }),
  });
}

export async function leaveCampaign(id: string) {
  return customFetch<{ status: number }>(`/campaigns/${id}/leave`, { method: 'POST' });
}

export async function kickMember(campaignId: string, memberId: string) {
  return customFetch<{ status: number }>(`/campaigns/${campaignId}/members/${memberId}`, { method: 'DELETE' });
}

export async function linkCharacter(campaignId: string, characterId: string | null) {
  return customFetch<{ status: number }>(`/campaigns/${campaignId}/my-character`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ characterId }),
  });
}

export async function setCharacterVisibility(campaignId: string, visibility: number) {
  return customFetch<{ status: number }>(`/campaigns/${campaignId}/my-visibility`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ visibility }),
  });
}
