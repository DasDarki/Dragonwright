import {getCurrentInstance, onUnmounted} from "vue";

const BASE_TITLE = "Dragonwright";

export function useTitle(addition?: string) {
  const before = document.title;
  if (addition) {
    document.title = `${addition} | ${BASE_TITLE}`;
  } else {
    document.title = BASE_TITLE;
  }

  if (getCurrentInstance()) {
    onUnmounted(() => {
      document.title = before;
    });
  }
}