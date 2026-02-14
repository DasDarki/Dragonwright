<script setup lang="ts">
import {useTitle} from "@/composables/useTitle.ts";
import {LoginResult, useAuthStore} from "@/stores/auth.ts";
import UiCenterizedLayout from "@/components/ui/layout/UiCenterizedLayout.vue";
import UiCard from "@/components/ui/UiCard.vue";
import UiStack from "@/components/ui/layout/UiStack.vue";
import Logo from "@/components/Logo.vue";
import UiInput from "@/components/ui/UiInput.vue";
import {ref} from "vue";
import UiButton from "@/components/ui/UiButton.vue";
import {useToast} from "@/composables/useToast.ts";
import router from "@/router.ts";
import UiModal from "@/components/ui/UiModal.vue";

useTitle("Login");

const authStore = useAuthStore();
const toast = useToast();

const username = ref("");
const password = ref("");
const loggingIn = ref(false);

const askRegister = ref(false);
const registering = ref(false);

async function login() {
  if (loggingIn.value) return;
  if (!username.value || !password.value) {
    toast.showToast({
      variant: "warning",
      message: "Please enter a valid username and password.",
    })
    return;
  }

  if (password.value.trim().length < 8) {
    toast.showToast({
      variant: "warning",
      message: "Password must be at least 8 characters long.",
    })
    return;
  }

  loggingIn.value = true;

  try {
    const res = await authStore.login(username.value, password.value);
    if (res === LoginResult.Success) {
      await router.push('/');

      toast.showToast({
        variant: "success",
        message: "Successfully logged in.",
      });
      return;
    } else if (res === LoginResult.InvalidCredentials) {
      toast.showToast({
        variant: "danger",
        message: "Invalid username or password.",
      });
      return;
    } else if (res === LoginResult.Error) {
      toast.showToast({
        variant: "danger",
        message: "An unknown error occurred during login.",
      });
      return;
    }

    console.log("User not found, asking to register");
    askRegister.value = true;
  } finally {
    loggingIn.value = false;
  }
}

async function register() {
  if (registering.value) return;

  registering.value = true;

  try {
    await authStore.register(username.value, password.value);
    askRegister.value = false;
    await router.push('/');
    toast.showToast({
      variant: "success",
      message: "Account created and logged in successfully.",
    });
  } catch (e) {
    toast.showToast({
      variant: "danger",
      message: "Account creation failed. Please try again later.",
    });
  } finally {
    registering.value = false;
  }
}
</script>

<template>
  <UiCenterizedLayout>
    <UiCard>
      <UiStack class="login-stack" align="center" :gap="1">
        <Logo logo-size="10rem" with-text />

        <div class="field">
          <label>Username:</label>
          <UiInput v-model="username" placeholder="Username" @keydown.enter="login" />
        </div>

        <div class="field">
          <label>Password:</label>
          <UiInput v-model="password" type="password" placeholder="Password" @keydown.enter="login" />
        </div>

        <UiButton :loading="loggingIn || registering" style="margin-top: 0.5rem; width: 50%" @click="login">
          Login
        </UiButton>
      </UiStack>
    </UiCard>
  </UiCenterizedLayout>

  <UiModal v-model="askRegister" title="Create new Account" style="max-width: 40%" :close-on-backdrop="false">
    <p>
      The account with username "<strong>{{ username }}</strong>" does not exist.<br/>Would you like to create a new account with this username and the provided password?
      <br/><br/>
      <i>You will be logged in afterwards!</i>
    </p>

    <template #footer>
      <UiButton :loading="registering" variant="secondary" @click="askRegister = false">
        Cancel
      </UiButton>
      <UiButton :loading="registering" @click="register">
        Create Account
      </UiButton>
    </template>
  </UiModal>
</template>

<style scoped lang="scss">
.login-stack {
  min-width: 20rem;
}

@media (max-width: 600px) {
  .login-stack {
    min-width: 0;
  }
}
</style>