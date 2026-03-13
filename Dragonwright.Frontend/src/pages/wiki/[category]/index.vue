<script setup lang="ts">
import {computed} from "vue";
import {useRoute, useRouter} from "vue-router";
import {useTitle} from "@/composables/useTitle";
import {useContentList} from "@/composables/useContentList";
import {getContentType} from "@/content/types";
import {sourceBadge, spellSchoolLabels, spellLevelLabels, itemTypeLabels, rarityLabels, creatureTypeLabels, languageTypeLabels} from "@/content/enums";
import ContentListHeader from "@/components/content/ContentListHeader.vue";
import ContentListTable from "@/components/content/ContentListTable.vue";
import ContentPagination from "@/components/content/ContentPagination.vue";
import UiBadge from "@/components/ui/UiBadge.vue";
import type {ColumnDef} from "@/components/content/ContentListTable.vue";

const route = useRoute();
const router = useRouter();

const category = computed(() => route.params.category as string);
const typeDef = computed(() => getContentType(category.value));

useTitle(computed(() => typeDef.value?.label ?? "Wiki"));

const {
  items, loading, page, totalPages, totalCount,
  search, source, goToPage,
} = useContentList<any>(typeDef.value!);

const columns = computed<ColumnDef[]>(() => {
  switch (category.value) {
    case "spells":
      return [
        {key: "name", label: "Name"},
        {key: "level", label: "Level", hideOnMobile: true},
        {key: "school", label: "School", hideOnMobile: true},
      ];
    case "items":
      return [
        {key: "name", label: "Name"},
        {key: "type", label: "Type", hideOnMobile: true},
        {key: "rarity", label: "Rarity", hideOnMobile: true},
      ];
    case "classes":
      return [
        {key: "name", label: "Name"},
        {key: "hitDie", label: "Hit Die", hideOnMobile: true},
      ];
    case "races":
      return [
        {key: "name", label: "Name"},
        {key: "type", label: "Type", hideOnMobile: true},
      ];
    case "feats":
      return [
        {key: "name", label: "Name"},
        {key: "featLevel", label: "Level", hideOnMobile: true},
      ];
    case "backgrounds":
      return [
        {key: "name", label: "Name"},
      ];
    case "languages":
      return [
        {key: "name", label: "Name"},
        {key: "type", label: "Type", hideOnMobile: true},
      ];
    default:
      return [{key: "name", label: "Name"}];
  }
});

function onView(item: any) {
  if (item.id) router.push(`/wiki/${category.value}/${item.id}`);
}

function formatCellValue(col: string, value: any): string {
  if (value == null) return "\u2014";
  switch (col) {
    case "school": return spellSchoolLabels[value] ?? "\u2014";
    case "level": return value === 0 ? "Cantrip" : `Level ${value}`;
    case "hitDie": return `d${value}`;
    case "featLevel": return value ? `Level ${value}` : "\u2014";
    case "rarity": return rarityLabels[value] ?? "\u2014";
    default:
      if (col === "type" && category.value === "items") return itemTypeLabels[value] ?? "\u2014";
      if (col === "type" && category.value === "races") return creatureTypeLabels[value] ?? "\u2014";
      if (col === "type" && category.value === "languages") return languageTypeLabels[value] ?? "\u2014";
      return String(value);
  }
}
</script>

<template>
  <div v-if="typeDef">
    <div class="content-page__header">
      <RouterLink to="/wiki" class="content-page__back">
        <i class="fas fa-arrow-left"/>
      </RouterLink>
      <h1 class="content-page__title">{{ typeDef.label }}</h1>
    </div>

    <ContentListHeader
      v-model:search="search"
      v-model:source="source"
      :show-source-filter="typeDef.hasSourceFilter"
      :show-new-button="false"
    />

    <ContentListTable
      :items="items"
      :columns="columns"
      :loading="loading"
      :empty-icon="typeDef.icon"
      :empty-title="`No ${typeDef.label.toLowerCase()} found`"
      empty-subtitle="Try adjusting your search or filters."
    >
      <template #cell-name="{item}">
        <RouterLink class="content-table__link" :to="`/wiki/${category}/${item.id}`">
          {{ item.name }}
        </RouterLink>
        <UiBadge
          v-if="item.source !== undefined && sourceBadge[item.source]"
          :label="sourceBadge[item.source]!.label"
          :variant="sourceBadge[item.source]!.variant"
          style="margin-left: 0.5rem"
        />
      </template>
      <template v-for="col in columns.filter(c => c.key !== 'name')" #[`cell-${col.key}`]="{value}">
        {{ formatCellValue(col.key, value) }}
      </template>
      <template #actions="{item}">
        <button class="content-table__action-btn" title="View" @click="onView(item)">
          <i class="fas fa-eye"/>
        </button>
      </template>
    </ContentListTable>

    <ContentPagination
      :page="page"
      :total-pages="totalPages"
      :total-count="totalCount"
      @update:page="goToPage"
    />
  </div>
  <div v-else class="wiki-not-found">
    <p>Category not found.</p>
    <RouterLink to="/wiki">Back to Wiki</RouterLink>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables" as *;

.wiki-not-found {
  text-align: center;
  padding: $space-8;
  color: $color-text-muted;

  a {
    color: $color-accent-alt;
  }
}
</style>
