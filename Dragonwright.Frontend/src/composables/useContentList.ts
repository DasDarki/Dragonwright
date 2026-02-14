import { ref, computed, watch } from 'vue'
import type { SourceType } from '@/api'
import type { ContentTypeDefinition } from '@/content/types'
import { useToast } from '@/composables/useToast'
import { useConfirm } from '@/composables/useConfirm'

export interface PaginatedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export function useContentList<T extends { id?: string; name: string }>(
  typeDef: ContentTypeDefinition,
) {
  const items = ref<T[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const page = ref(1)
  const pageSize = ref(20)
  const totalCount = ref(0)
  const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

  const search = ref('')
  const source = ref<SourceType | undefined>(undefined)

  const { showToast } = useToast()
  const { confirm } = useConfirm()

  async function fetchItems() {
    loading.value = true
    error.value = null
    try {
      const params: Record<string, any> = {
        page: page.value,
        pageSize: pageSize.value,
      }
      if (search.value) params.search = search.value
      if (source.value !== undefined) params.source = source.value

      const res = await typeDef.fetchFn(params)

      // Orval generates `data: void` for paginated list responses,
      // so we cast the raw response data to our expected shape.
      const body = (res as any).data as PaginatedResult<T> | T[] | undefined

      if (Array.isArray(body)) {
        items.value = body
        totalCount.value = body.length
      } else if (body && typeof body === 'object' && 'items' in body) {
        const paginated = body as PaginatedResult<T>
        items.value = paginated.items
        totalCount.value = paginated.totalCount
      } else {
        items.value = []
        totalCount.value = 0
      }
    } catch {
      error.value = `Failed to load ${typeDef.label.toLowerCase()}.`
      items.value = []
      totalCount.value = 0
    } finally {
      loading.value = false
    }
  }

  async function deleteItem(item: T) {
    const ok = await confirm({
      title: `Delete ${typeDef.singular}`,
      message: `Are you sure you want to delete "${item.name}"? This action cannot be undone.`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      danger: true,
    })

    if (!ok) return

    try {
      await typeDef.deleteFn(item.id!)
      showToast({ variant: 'success', message: `${typeDef.singular} deleted.` })
      await fetchItems()
    } catch {
      showToast({ variant: 'danger', message: `Failed to delete ${typeDef.singular.toLowerCase()}.` })
    }
  }

  function goToPage(p: number) {
    if (p < 1 || p > totalPages.value) return
    page.value = p
  }

  watch([page, source], () => fetchItems())

  let searchTimeout: ReturnType<typeof setTimeout> | null = null
  watch(search, () => {
    if (searchTimeout) clearTimeout(searchTimeout)
    searchTimeout = setTimeout(() => {
      page.value = 1
      fetchItems()
    }, 300)
  })

  fetchItems()

  return {
    items,
    loading,
    error,
    page,
    pageSize,
    totalCount,
    totalPages,
    search,
    source,
    fetchItems,
    deleteItem,
    goToPage,
  }
}
