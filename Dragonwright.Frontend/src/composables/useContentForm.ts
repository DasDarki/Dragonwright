import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from '@/composables/useToast'

export interface ContentFormOptions<T> {
  typeLabel: string
  listRoute: string
  editRoute: string
  id?: string
  defaultData: () => T
  getFn: (id: string) => Promise<any>
  createFn: (data: T) => Promise<any>
  updateFn: (id: string, data: T) => Promise<any>
  onAfterSave?: (entityId: string) => Promise<void>
}

export function useContentForm<T>(options: ContentFormOptions<T>) {
  const router = useRouter()
  const { showToast } = useToast()

  const form = ref<T>(options.defaultData()) as import('vue').Ref<T>
  const loading = ref(false)
  const saving = ref(false)
  const entityId = ref<string | undefined>(options.id)
  const isEdit = computed(() => !!entityId.value)

  async function load() {
    if (!entityId.value) return
    loading.value = true
    try {
      const res = await options.getFn(entityId.value)
      const data = (res as any).data ?? res
      form.value = { ...options.defaultData(), ...data }
    } catch {
      showToast({ variant: 'danger', message: `Failed to load ${options.typeLabel.toLowerCase()}.` })
      router.push(options.listRoute)
    } finally {
      loading.value = false
    }
  }

  async function save() {
    saving.value = true
    try {
      if (isEdit.value) {
        await options.updateFn(entityId.value!, form.value)
        if (options.onAfterSave) await options.onAfterSave(entityId.value!)
        showToast({ variant: 'success', message: `${options.typeLabel} updated.` })
      } else {
        const res = await options.createFn(form.value)
        const data = (res as any).data ?? res
        const newId = String(data.id ?? data.Id ?? '')
        entityId.value = newId
        if (options.onAfterSave) await options.onAfterSave(newId)
        showToast({ variant: 'success', message: `${options.typeLabel} created.` })
        router.replace(`${options.editRoute}/${newId}`)
      }
    } catch {
      showToast({ variant: 'danger', message: `Failed to save ${options.typeLabel.toLowerCase()}.` })
    } finally {
      saving.value = false
    }
  }

  function cancel() {
    router.push(options.listRoute)
  }

  onMounted(load)

  return { form, loading, saving, isEdit, entityId, save, cancel, reload: load }
}
