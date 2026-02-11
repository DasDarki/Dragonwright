import type {IFormFile, StoredFile} from "@/api";
import {useToast} from "@/composables/useToast.ts";
import {apiUrl} from "@/api/http.ts";

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB
const ALLOWED_TYPES = ["image/jpeg", "image/png", "image/gif", "image/webp", "image/avif", "image/bmp"];

export function useImageUrl(file?: StoredFile): string|null {
  const id = file?.id;
  if (!id) return null;
  return `${apiUrl}/files/${id}`;
}

export function useImageUpload() {
  const toast = useToast();

  return new Promise<IFormFile|null>(resolve => {
    const input = document.createElement("input");
    input.type = "file";
    input.style.display = "none";
    input.accept = ALLOWED_TYPES.join(",");
    document.body.appendChild(input);

    input.addEventListener("change", () => {
      const file = input.files?.[0];
      if (!file) {
        resolve(null);
        document.body.removeChild(input);
        return;
      }

      if (!ALLOWED_TYPES.includes(file.type)) {
        toast.showToast({
          message: "Unsupported file type. Please select an image."
        });
        resolve(null);
        document.body.removeChild(input);
        return;
      }

      if (file.size > MAX_FILE_SIZE) {
        toast.showToast({
          message: "File is too large. Please select a file smaller than 5 MB."
        });
        resolve(null);
        document.body.removeChild(input);
        return;
      }

      resolve(file);
      document.body.removeChild(input);
    });
    input.click();
  });
}