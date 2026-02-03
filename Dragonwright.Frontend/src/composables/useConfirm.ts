import { ref, readonly } from 'vue'

interface ConfirmOptions {
  title?: string
  message?: string
  confirmText?: string
  cancelText?: string
  danger?: boolean
}

interface ConfirmState extends ConfirmOptions {
  resolve?: (value: boolean) => void
  open: boolean
}

const state = ref<ConfirmState>({
  open: false,
  title: undefined,
  message: undefined,
  confirmText: 'OK',
  cancelText: 'Abbrechen',
  danger: false
})

export function useConfirm() {
  function confirm(options: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      state.value = {
        open: true,
        confirmText: 'Yes',
        cancelText: 'No',
        ...options,
        resolve
      }
    })
  }

  function close(result: boolean) {
    if (state.value.resolve) {
      state.value.resolve(result)
    }
    state.value = { ...state.value, open: false, resolve: undefined }
  }

  return {
    state: readonly(state),
    confirm,
    close
  }
}
