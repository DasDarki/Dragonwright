<script setup lang="ts">
import {ref, computed, onMounted} from "vue";
import {useRoute} from "vue-router";
import {useTitle} from "@/composables/useTitle";
import {useToast} from "@/composables/useToast";
import {getContentType} from "@/content/types";
import {sourceBadge} from "@/content/enums";
import UiBadge from "@/components/ui/UiBadge.vue";
import WikiSpell from "@/components/wiki/WikiSpell.vue";
import WikiClass from "@/components/wiki/WikiClass.vue";
import WikiRace from "@/components/wiki/WikiRace.vue";
import WikiItem from "@/components/wiki/WikiItem.vue";
import WikiFeat from "@/components/wiki/WikiFeat.vue";
import WikiBackground from "@/components/wiki/WikiBackground.vue";
import WikiLanguage from "@/components/wiki/WikiLanguage.vue";

const route = useRoute();
const {showToast} = useToast();

const category = computed(() => route.params.category as string);
const entityId = computed(() => route.params.id as string);
const typeDef = computed(() => getContentType(category.value));

const entity = ref<any>(null);
const loading = ref(true);

useTitle(computed(() => entity.value?.name ?? typeDef.value?.singular ?? "Wiki"));

onMounted(async () => {
  if (!typeDef.value?.getFn) {
    loading.value = false;
    return;
  }
  try {
    const res = await typeDef.value.getFn(entityId.value);
    entity.value = (res as any).data;
  } catch {
    showToast({variant: "danger", message: `Failed to load ${typeDef.value?.singular?.toLowerCase() ?? "entry"}.`});
  } finally {
    loading.value = false;
  }
});

const detailComponent = computed(() => {
  switch (category.value) {
    case "spells": return WikiSpell;
    case "classes": return WikiClass;
    case "races": return WikiRace;
    case "items": return WikiItem;
    case "feats": return WikiFeat;
    case "backgrounds": return WikiBackground;
    case "languages": return WikiLanguage;
    default: return null;
  }
});

function copyPermalink() {
  navigator.clipboard.writeText(window.location.href);
  showToast({message: "Link copied to clipboard."});
}
</script>

<template>
  <div v-if="loading" class="wiki-loading">
    <i class="fas fa-spinner fa-spin"/> Loading...
  </div>

  <div v-else-if="!entity || !typeDef" class="wiki-not-found">
    <p>Entry not found.</p>
    <RouterLink to="/wiki">Back to Wiki</RouterLink>
  </div>

  <div v-else class="wiki-detail">
    <div class="wiki-detail__header">
      <RouterLink :to="`/wiki/${category}`" class="content-page__back">
        <i class="fas fa-arrow-left"/>
      </RouterLink>
      <div class="wiki-detail__title-area">
        <h1 class="wiki-detail__title">{{ entity.name }}</h1>
        <div class="wiki-detail__meta">
          <UiBadge
            v-if="entity.source !== undefined && sourceBadge[entity.source]"
            :label="sourceBadge[entity.source]!.label"
            :variant="sourceBadge[entity.source]!.variant"
          />
          <UiBadge :label="typeDef.singular" variant="muted"/>
          <span v-if="entity.sourceCreator?.username" class="wiki-detail__author">
            by {{ entity.sourceCreator.username }}
          </span>
        </div>
      </div>
      <button class="wiki-permalink" @click="copyPermalink" title="Copy permalink">
        <i class="fas fa-link"/>
      </button>
    </div>

    <div class="wiki-detail__body">
      <component :is="detailComponent" :entity="entity"/>
    </div>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.wiki-loading {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;
}

.wiki-not-found {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;

  a { color: $color-accent-alt; }
}

.wiki-detail {
  max-width: 52rem;
}

.wiki-detail__header {
  display: flex;
  align-items: flex-start;
  gap: $space-3;
  margin-bottom: $space-4;
}

.wiki-detail__title-area {
  flex: 1;
  min-width: 0;
}

.wiki-detail__title {
  font-family: "Cinzel", serif;
  font-size: 1.5rem;
  letter-spacing: 0.04em;
  color: $color-primary;
  margin: 0;
}

.wiki-detail__meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: $space-2;
  margin-top: $space-2;
}

.wiki-detail__author {
  font-size: 0.8rem;
  color: $color-text-muted;
}

.wiki-permalink {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: $radius-sm;
  border: 1px solid $color-border-subtle;
  background: transparent;
  color: $color-text-muted;
  cursor: pointer;
  flex-shrink: 0;
  transition: all 150ms ease;

  &:hover {
    border-color: $color-accent;
    color: $color-accent;
  }
}

.wiki-detail__body {
  display: flex;
  flex-direction: column;
  gap: $space-4;
}
</style>
