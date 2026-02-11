import { ref, watch, type Ref, type WatchSource } from 'vue'

export interface UseAutoSaveOptions<T> {
  source: WatchSource<T>
  save: () => Promise<void>
  delay?: number
  enabled?: Ref<boolean>
}

export function useAutoSave<T>(options: UseAutoSaveOptions<T>) {
  const { source, save, delay = 1000, enabled } = options

  const isSaving = ref(false)
  const lastSaved = ref<Date | null>(null)

  let timeout: ReturnType<typeof setTimeout> | null = null

  const debouncedSave = async () => {
    if (enabled && !enabled.value) return

    if (timeout) {
      clearTimeout(timeout)
    }

    timeout = setTimeout(async () => {
      isSaving.value = true
      try {
        await save()
        lastSaved.value = new Date()
      } finally {
        isSaving.value = false
      }
    }, delay)
  }

  watch(source, debouncedSave, { deep: true })

  const cancelPendingSave = () => {
    if (timeout) {
      clearTimeout(timeout)
      timeout = null
    }
  }

  const saveNow = async () => {
    cancelPendingSave()
    isSaving.value = true
    try {
      await save()
      lastSaved.value = new Date()
    } finally {
      isSaving.value = false
    }
  }

  return {
    isSaving,
    lastSaved,
    cancelPendingSave,
    saveNow
  }
}
