import { readonly, ref } from 'vue'

export type ToastVariant = 'info' | 'success' | 'warning' | 'danger'

export interface ToastOptions {
  title?: string
  message: string
  variant?: ToastVariant
  duration?: number
}

export interface Toast extends ToastOptions {
  id: number
}

const toasts = ref<Toast[]>([])
let counter = 0

function showToast(options: ToastOptions) {
  const id = ++counter
  const toast: Toast = {
    id,
    variant: options.variant ?? 'info',
    duration: options.duration ?? 4000,
    ...options
  }

  toasts.value.push(toast)

  if (toast.duration! > 0) {
    setTimeout(() => dismissToast(id), toast.duration)
  }

  return id
}

function dismissToast(id: number) {
  toasts.value = toasts.value.filter((t) => t.id !== id)
}

export function useToast() {
  return {
    toasts: readonly(toasts),
    showToast,
    dismissToast
  }
}
