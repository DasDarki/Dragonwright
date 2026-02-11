<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { useRouter, RouterLink } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import Logo from "@/components/Logo.vue";
import {useImageUpload, useImageUrl} from "@/composables/useFileUpload.ts";
import {useToast} from "@/composables/useToast.ts";

defineProps<{
  logoSize?: string;
}>();

const router = useRouter();
const toast = useToast();
const auth = useAuthStore();

const accountOpen = ref(false);
const mobileNavOpen = ref(false);
const rootEl = ref<HTMLElement | null>(null);

const avatarErrored = ref(false);

const userName = computed(() => auth.loggedInUser?.username ?? "Account");
const roleTag = computed(() => {
  const u: any = auth.loggedInUser as any;
  const role = (u?.userRole?.toString().toLowerCase()) ?? (u?.role?.toString().toLowerCase()) ?? "0";
  if (role === "2") return "admin";
  if (role === "1") return "team";
  return "";
});

const avatarUrl = computed(() => {
  if (avatarErrored.value) return null;
  const u: any = auth.loggedInUser as any;
  return useImageUrl(u.avatar);
});

function toggleAccount() {
  accountOpen.value = !accountOpen.value;
  if (accountOpen.value) mobileNavOpen.value = false;
}

function toggleMobileNav() {
  mobileNavOpen.value = !mobileNavOpen.value;
  if (mobileNavOpen.value) accountOpen.value = false;
}

function closeAll() {
  accountOpen.value = false;
  mobileNavOpen.value = false;
}

function onDocClick(e: MouseEvent) {
  const el = rootEl.value;
  if (!el) return;
  if (e.target instanceof Node && el.contains(e.target)) return;
  closeAll();
}

async function logout() {
  closeAll();
  await auth.logout();
  await router.push("/login");
}

async function logoutAll() {
  closeAll();
  await auth.logoutAll();
  await router.push("/login");
}

function changeAvatar() {
  closeAll();

  useImageUpload().then(async file => {
    if (!file) return;
    if (!await auth.setAvatar(file)) {
      toast.showToast({ variant: "danger", message: "Failed to upload avatar." });
    } else {
      avatarErrored.value = false;
      toast.showToast({ variant: "success", message: "Avatar updated." });
    }
  })
}

function onNavClick() {
  closeAll();
}

onMounted(() => {
  document.addEventListener("click", onDocClick);
});

onBeforeUnmount(() => {
  document.removeEventListener("click", onDocClick);
});
</script>

<template>
  <header ref="rootEl" class="navbar">
    <div class="navbar__left">
      <button type="button" class="navbar__iconbtn navbar__hamburger" @click="toggleMobileNav" aria-label="Menu">
        <span class="navbar__hamburger-icon">☰</span>
      </button>

      <RouterLink class="navbar__brand" to="/" @click="onNavClick">
        <Logo :with-text="false" :logo-size="logoSize ?? '2.25rem'" />
      </RouterLink>

      <nav class="navbar__nav">
        <RouterLink class="navbar__link" to="/characters" active-class="is-active">Characters</RouterLink>
        <RouterLink class="navbar__link" to="/campaigns" active-class="is-active">Campaigns</RouterLink>
        <RouterLink class="navbar__link" to="/content" active-class="is-active">Content</RouterLink>
      </nav>

      <div v-if="mobileNavOpen" class="navbar__menu navbar__menu--left">
        <RouterLink class="navbar__menu-link" to="/characters" @click="onNavClick">Characters</RouterLink>
        <RouterLink class="navbar__menu-link" to="/campaigns" @click="onNavClick">Campaigns</RouterLink>
        <RouterLink class="navbar__menu-link" to="/content" @click="onNavClick">Content</RouterLink>
      </div>
    </div>

    <div class="navbar__right">
      <button type="button" class="navbar__account" @click="toggleAccount">
        <span class="navbar__avatar" :class="[!avatarUrl && 'is-empty']">
          <img v-if="avatarUrl" :src="avatarUrl" alt="avatar" @error="avatarErrored = true"/>
          <span v-else class="navbar__avatar-fallback">{{ userName.slice(0, 1).toUpperCase() }}</span>
        </span>

        <span class="navbar__account-meta">
          <span class="navbar__account-name">{{ userName }}</span>
          <span v-if="roleTag" class="navbar__role" :class="[`navbar__role--${roleTag}`]">
            {{ roleTag }}
          </span>
        </span>

        <span class="navbar__chev">▾</span>
      </button>

      <div v-if="accountOpen" class="navbar__menu navbar__menu--right">
        <button type="button" class="navbar__menu-item" @click="changeAvatar">Change avatar</button>
        <div class="navbar__menu-sep" />
        <button type="button" class="navbar__menu-item" @click="logout">Logout</button>
        <button type="button" class="navbar__menu-item is-danger" @click="logoutAll">Logout all sessions</button>
      </div>
    </div>
  </header>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.navbar {
  position: sticky;
  top: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: $space-4;
  padding: $space-3 $space-4;
  margin-bottom: $space-6;
  background: rgba(5, 8, 20, 0.72);
  border-bottom: 1px solid $color-border-subtle;
  backdrop-filter: blur(10px);
}

.navbar__left {
  position: relative;
  display: flex;
  align-items: center;
  gap: $space-3;
  min-width: 0;
}

.navbar__brand {
  display: inline-flex;
  align-items: center;
  text-decoration: none;
}

.navbar__nav {
  display: flex;
  align-items: center;
  gap: $space-1;
}

.navbar__link {
  display: inline-flex;
  align-items: center;
  height: 2.25rem;
  padding: 0 $space-3;
  border-radius: $radius-pill;
  color: $color-text-muted;
  text-decoration: none;
  font-size: 0.95rem;
  transition: background-color 150ms ease, color 150ms ease, border-color 150ms ease;

  &:hover {
    color: $color-text;
    background: rgba(249, 115, 22, 0.10);
  }

  &.is-active {
    color: $color-primary-strong;
    background: $color-accent-soft;
    border: 1px solid rgba(249, 115, 22, 0.25);
  }
}

.navbar__right {
  position: relative;
  display: flex;
  align-items: center;
}

.navbar__iconbtn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text;
  cursor: pointer;
  transition: border-color 150ms ease, box-shadow 150ms ease, background-color 150ms ease;

  &:hover {
    border-color: $color-border-strong;
  }

  &:focus-visible {
    outline: none;
    border-color: $color-accent-alt;
    box-shadow: 0 0 0 2px $color-bg-elevated, 0 0 0 3px $color-focus;
  }
}

.navbar__hamburger-icon {
  font-size: 1.1rem;
  line-height: 1;
}

.navbar__hamburger {
  display: none;
}

.navbar__account {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  padding: $space-2 $space-3;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  background: $color-surface;
  color: $color-text;
  cursor: pointer;
  transition: border-color 150ms ease, box-shadow 150ms ease, background-color 150ms ease;

  &:hover {
    border-color: $color-border-strong;
  }

  &:focus-visible {
    outline: none;
    border-color: $color-accent-alt;
    box-shadow: 0 0 0 2px $color-bg-elevated, 0 0 0 3px $color-focus;
  }
}

.navbar__avatar {
  width: 2rem;
  height: 2rem;
  border-radius: $radius-pill;
  overflow: hidden;
  border: 1px solid $color-border-subtle;
  background: $color-surface-alt;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &.is-empty {
    background: rgba(249, 115, 22, 0.12);
    border-color: rgba(249, 115, 22, 0.25);
  }
}

.navbar__avatar-fallback {
  font-weight: 700;
  color: $color-accent;
}

.navbar__account-meta {
  display: inline-flex;
  align-items: center;
  gap: $space-2;
  min-width: 0;
}

.navbar__account-name {
  max-width: 12rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: $color-text;
}

.navbar__role {
  font-size: 0.75rem;
  padding: 0.18rem 0.5rem;
  border-radius: $radius-pill;
  border: 1px solid $color-border-subtle;
  color: $color-text-muted;
  text-transform: uppercase;
  letter-spacing: 0.06em;

  &--admin {
    border-color: rgba(249, 115, 22, 0.35);
    background: rgba(249, 115, 22, 0.14);
    color: $color-accent;
  }

  &--team {
    border-color: rgba(56, 189, 248, 0.35);
    background: rgba(56, 189, 248, 0.12);
    color: $color-accent-alt;
  }
}

.navbar__chev {
  color: $color-text-soft;
}

.navbar__menu {
  position: absolute;
  top: calc(100% + #{$space-2});
  width: 15rem;
  border: 1px solid $color-border-subtle;
  background: $color-bg-elevated;
  border-radius: $radius-lg;
  box-shadow: $shadow-card;
  overflow: hidden;
}

.navbar__menu--right {
  right: 0;
}

.navbar__menu--left {
  left: 0;
  width: 13rem;
}

.navbar__menu-item {
  width: 100%;
  text-align: left;
  padding: $space-3 $space-3;
  background: transparent;
  border: 0;
  color: $color-text;
  cursor: pointer;
  transition: background-color 150ms ease, color 150ms ease;

  &:hover {
    background: rgba(249, 115, 22, 0.10);
  }

  &.is-danger {
    color: $color-danger;

    &:hover {
      background: rgba(239, 68, 68, 0.12);
    }
  }
}

.navbar__menu-link {
  display: block;
  padding: $space-3 $space-3;
  text-decoration: none;
  color: $color-text;
  transition: background-color 150ms ease;

  &:hover {
    background: rgba(249, 115, 22, 0.10);
  }
}

.navbar__menu-sep {
  height: 1px;
  background: $color-border-subtle;
}

@media (max-width: 768px) {
  .navbar {
    padding: $space-3;
  }

  .navbar__hamburger {
    display: inline-flex;
  }

  .navbar__nav {
    display: none;
  }

  .navbar__account-name {
    max-width: 9rem;
  }
}
</style>
