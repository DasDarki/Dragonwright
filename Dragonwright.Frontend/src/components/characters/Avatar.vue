<script setup lang="ts">
import {computed, onMounted, onUnmounted, ref} from "vue";
import {useImageUpload, useImageUrl} from "@/composables/useFileUpload.ts";
import {useToast} from "@/composables/useToast.ts";
import {unwrapOrThrow} from "@/api/result.ts";
import {type IFormFile, putCharactersIdAvatar, type StoredFile} from "@/api";

const toast = useToast();

const props = defineProps<{
  allowEdit?: boolean;
  character: any
}>();

const errored = ref(false);
const avatarUrl = computed(() => {
  if (errored.value) return null;
  return useImageUrl(props.character?.avatar);
});

const shiftDown = ref(false);

onMounted(() => {
  window.addEventListener('keydown', onKeyDown);
  window.addEventListener('keyup', onKeyUp);
  window.addEventListener('blur', onWindowBlur);
});

onUnmounted(() => {
  window.removeEventListener('keydown', onKeyDown);
  window.removeEventListener('keyup', onKeyUp);
  window.removeEventListener('blur', onWindowBlur);
});

function onKeyDown(event: KeyboardEvent) {
  if (event.key.toLowerCase() === 'shift') {
    shiftDown.value = true;
  }
}

function onKeyUp(event: KeyboardEvent) {
  if (event.key.toLowerCase() === 'shift') {
    shiftDown.value = false;
  }
}

function onWindowBlur() {
  shiftDown.value = false;
}

function onAvatarClick() {
  if (props.allowEdit && shiftDown.value) {
    useImageUpload().then(async file => {
      if (!file) return;
      if (!await setAvatar(file)) {
        toast.showToast({ variant: "danger", message: "Failed to upload avatar." });
      } else {
        errored.value = false;
        toast.showToast({ variant: "success", message: "Avatar updated." });
      }
    });
  }
}

async function setAvatar(file: IFormFile): Promise<boolean> {
  try {
    props.character.avatar = unwrapOrThrow<StoredFile>(await putCharactersIdAvatar(props.character.id, {file}) as any);
    return true;
  } catch (e) {
    console.error(e);
    return false;
  }
}
</script>

<template>
  <div class="character-card__avatar" :class="{editable: shiftDown && allowEdit}" :style="!allowEdit ? {cursor: 'pointer'} : {}" @click="onAvatarClick">
    <img v-if="avatarUrl" :src="avatarUrl" alt="" @error="errored = true" />
    <span v-else class="character-card__avatar-fallback">
      {{ character.name?.slice(0, 1).toUpperCase() }}
    </span>
  </div>
</template>

<style scoped lang="scss">
@use "@/styles/variables.scss" as *;

.character-card__avatar {
  flex-shrink: 0;
  width: 4rem;
  height: 4rem;
  border-radius: $radius-lg;
  overflow: hidden;
  background: $color-surface-alt;
  border: 1px solid $color-border-subtle;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: default;

  img {
    width: 100%;
    height: 100%;
    object-position: top;
    object-fit: cover;
  }

  &.editable {
    cursor: pointer;
  }
}

.character-card__avatar-fallback {
  font-size: 1.5rem;
  font-weight: 700;
  color: $color-accent;
  user-select: none;
}
</style>