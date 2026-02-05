<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import UiFieldTip from '@/components/ui/UiFieldTip.vue'
import type { FieldTip } from '@/components/ui/UiFieldTip.vue'

type Size = 'sm' | 'md' | 'lg'

type Id = string | number

type FetchParams = {
  page: number
  pageSize: number
  search?: string
}

type AnyOrvalResponse = { data?: any; status?: number; headers?: Headers } | any

const props = defineProps<{
  label?: string
  placeholder?: string
  size?: Size
  disabled?: boolean
  tip?: FieldTip
  clearable?: boolean

  pageSize?: number
  minSearchLength?: number

  fetchFn: (params: FetchParams) => Promise<AnyOrvalResponse>
  getByIdFn?: (id: Id) => Promise<AnyOrvalResponse>

  getOptionLabel: (entity: any) => string
  getOptionValue: (entity: any) => Id
}>()

const model = defineModel<Id | null>({ default: null })

const open = ref(false)
const loading = ref(false)
const loadingMore = ref(false)
const error = ref<string | null>(null)

const search = ref('')
const items = ref<any[]>([])
const page = ref(1)
const hasMore = ref(true)

const rootEl = ref<HTMLElement | null>(null)
const listEl = ref<HTMLElement | null>(null)

const sizeClass = computed(() => `input--${props.size ?? 'md'}`)
const pageSize = computed(() => props.pageSize ?? 20)
const minSearchLength = computed(() => props.minSearchLength ?? 0)

function extractBody(res: AnyOrvalResponse) {
  if (res && typeof res === 'object' && 'data' in res) return (res as any).data
  return res
}

function extractItems(body: any): { items: any[]; hasMore: boolean } {
  if (Array.isArray(body)) return { items: body, hasMore: false }
  if (body && typeof body === 'object' && Array.isArray(body.items)) {
    const totalPages = typeof body.totalPages === 'number' ? body.totalPages : undefined
    const pageNum = typeof body.page === 'number' ? body.page : page.value
    return { items: body.items, hasMore: totalPages ? pageNum < totalPages : body.items.length === pageSize.value }
  }
  return { items: [], hasMore: false }
}

function isSelectedLoaded() {
  if (model.value == null) return true
  return items.value.some(e => props.getOptionValue(e) === model.value)
}

async function ensureSelectedLoaded() {
  if (model.value == null) return
  if (isSelectedLoaded()) return
  if (!props.getByIdFn) return

  try {
    const res = await props.getByIdFn(model.value)
    const body = extractBody(res)
    if (!body) return

    const entity = body?.items?.[0] ?? body
    if (!entity) return

    const id = props.getOptionValue(entity)
    if (id !== model.value) return

    items.value = [entity, ...items.value.filter(e => props.getOptionValue(e) !== id)]
  } catch {}
}

async function loadPage(p: number, mode: 'reset' | 'append') {
  const q = search.value.trim()
  if (q.length > 0 && q.length < minSearchLength.value) {
    items.value = []
    hasMore.value = false
    return
  }

  if (mode === 'reset') {
    loading.value = true
    error.value = null
  } else {
    loadingMore.value = true
    error.value = null
  }

  try {
    const res = await props.fetchFn({
      page: p,
      pageSize: pageSize.value,
      ...(q ? { search: q } : {}),
    })

    const body = extractBody(res)
    const extracted = extractItems(body)

    if (mode === 'reset') {
      items.value = extracted.items
      page.value = p
      hasMore.value = extracted.hasMore
      await ensureSelectedLoaded()
    } else {
      const merged = [...items.value, ...extracted.items]
      const seen = new Set<Id>()
      items.value = merged.filter(e => {
        const id = props.getOptionValue(e)
        if (seen.has(id)) return false
        seen.add(id)
        return true
      })
      page.value = p
      hasMore.value = extracted.hasMore
      await ensureSelectedLoaded()
    }
  } catch {
    error.value = 'Failed to load items.'
    if (mode === 'reset') {
      items.value = []
      hasMore.value = false
    }
  } finally {
    loading.value = false
    loadingMore.value = false
  }
}

async function openDropdown() {
  if (props.disabled) return
  open.value = true
  if (items.value.length === 0) await loadPage(1, 'reset')
  await ensureSelectedLoaded()
}

function closeDropdown() {
  open.value = false
}

function toggle() {
  if (open.value) closeDropdown()
  else openDropdown()
}

const selectedLabel = computed(() => {
  if (model.value == null) return ''
  const found = items.value.find(e => props.getOptionValue(e) === model.value)
  return found ? props.getOptionLabel(found) : ''
})

function selectEntity(entity: any) {
  model.value = props.getOptionValue(entity)
  closeDropdown()
}

function clearSelection(e: Event) {
  e.stopPropagation()
  model.value = null
}

let searchTimeout: ReturnType<typeof setTimeout> | null = null
watch(search, () => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    loadPage(1, 'reset')
  }, 250)
})

watch(model, async () => {
  await ensureSelectedLoaded()
})

function onDocClick(e: MouseEvent) {
  const el = rootEl.value
  if (!el) return
  if (e.target instanceof Node && el.contains(e.target)) return
  closeDropdown()
}

async function onListScroll() {
  const el = listEl.value
  if (!el) return
  if (!open.value) return
  if (loading.value || loadingMore.value) return
  if (!hasMore.value) return

  const threshold = 80
  const nearBottom = el.scrollTop + el.clientHeight >= el.scrollHeight - threshold
  if (!nearBottom) return

  await loadPage(page.value + 1, 'append')
}

onMounted(() => {
  document.addEventListener('click', onDocClick)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocClick)
  if (searchTimeout) clearTimeout(searchTimeout)
})

const optionList = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return items.value
  return items.value.filter(e => props.getOptionLabel(e).toLowerCase().includes(q))
})
</script>

<template>
  <div ref="rootEl" class="field">
    <label v-if="label" class="field__label">
      {{ label }}<UiFieldTip v-if="tip" v-bind="tip" />
    </label>

    <button
      type="button"
      class="input entity-select__trigger"
      :class="[sizeClass]"
      :disabled="disabled"
      @click="toggle"
    >
      <span v-if="selectedLabel" class="entity-select__value">{{ selectedLabel }}</span>
      <span v-else class="entity-select__placeholder">{{ placeholder ?? 'Select…' }}</span>
      <span class="entity-select__controls">
        <span
          v-if="clearable && model != null"
          class="entity-select__clear"
          title="Clear"
          @click="clearSelection"
        >×</span>
        <span class="entity-select__chev">▾</span>
      </span>
    </button>

    <div v-if="open" class="entity-select__popover">
      <div class="entity-select__search">
        <input
          class="input"
          :class="[sizeClass]"
          v-model="search"
          :placeholder="`Search…`"
          :disabled="loading"
        />
      </div>

      <div
        ref="listEl"
        class="entity-select__list"
        @scroll="onListScroll"
      >
        <div v-if="loading" class="entity-select__state">Loading…</div>
        <div v-else-if="error" class="entity-select__state">{{ error }}</div>
        <div v-else-if="optionList.length === 0" class="entity-select__state">No results</div>

        <button
          v-for="e in optionList"
          :key="String(getOptionValue(e))"
          type="button"
          class="entity-select__option"
          :class="[getOptionValue(e) === model ? 'is-selected' : '']"
          @click="selectEntity(e)"
        >
          <span class="entity-select__option-label">{{ getOptionLabel(e) }}</span>
          <span v-if="getOptionValue(e) === model" class="entity-select__check">✓</span>
        </button>

        <div v-if="loadingMore" class="entity-select__state">Loading more…</div>
      </div>
    </div>
  </div>
</template>