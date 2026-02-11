import {defineStore} from "pinia";
import {loadAuth, saveAuth, type StoredAuth} from "@/auth/storage";
import {getDeviceId, getDeviceName} from "@/auth/device";
import {
  type AuthResponse,
  getUsersId,
  type LoginRequest,
  type LogoutRequest,
  postAuthLogin,
  postAuthLogout,
  postAuthLogoutAll,
  postAuthRefresh,
  postAuthRegister,
  type ProblemDetails, putUsersMeAvatar,
  type RefreshRequest,
  type RegisterRequest,
  type User, type UserResponse,
} from "@/api";
import {isHttpError, unwrapOrThrow} from "@/api/result";
import {setAuthAccessToken, setAuthLogoutHandler, setAuthRefreshHandler} from "@/api/http";

function isExpiringSoon(iso: string, skewMs: number) {
  const exp = Date.parse(iso);
  if (Number.isNaN(exp)) return true;
  return exp - Date.now() <= skewMs;
}

export enum LoginResult {
  Success = 'Success',
  InvalidCredentials = 'InvalidCredentials',
  UserNotFound = 'UserNotFound',
  Error = 'Error',
}

export const useAuthStore = defineStore("auth", {
  state: () => {
    const stored = loadAuth();
    return {
      accessToken: (stored?.accessToken ?? null) as string | null,
      refreshToken: (stored?.refreshToken ?? null) as string | null,
      accessTokenExpiration: (stored?.accessTokenExpiration ?? null) as string | null,
      lastAuthError: null as ProblemDetails | null,
      loggedInUser: null as User | null,
    };
  },

  getters: {
    isAuthenticated: (state) =>
      !!state.accessToken && !!state.refreshToken && !!state.accessTokenExpiration,
  },

  actions: {
    applyAuth(resp: AuthResponse) {
      this.accessToken = resp.accessToken ?? null;
      this.refreshToken = resp.refreshToken ?? null;
      this.accessTokenExpiration = resp.accessTokenExpiration ?? null;

      if (this.accessToken && this.refreshToken && this.accessTokenExpiration) {
        const snapshot: StoredAuth = {
          accessToken: this.accessToken,
          refreshToken: this.refreshToken,
          accessTokenExpiration: this.accessTokenExpiration,
        };
        saveAuth(snapshot);
        setAuthAccessToken(this.accessToken);
      } else {
        this.clearAuth();
      }
    },

    clearAuth() {
      this.accessToken = null;
      this.refreshToken = null;
      this.accessTokenExpiration = null;
      saveAuth(null);
      setAuthAccessToken(null);
    },

    clearError() {
      this.lastAuthError = null;
    },

    async register(username: string, password: string) {
      this.clearError();
      const payload: RegisterRequest = {
        username,
        password,
        deviceId: getDeviceId(),
        deviceName: getDeviceName(),
      };

      try {
        const res = await postAuthRegister(payload);
        const data = unwrapOrThrow<AuthResponse, ProblemDetails>(res as any);
        this.applyAuth(data);
      } catch (e) {
        if (isHttpError<ProblemDetails>(e)) this.lastAuthError = e.data ?? null;
        throw e;
      }
    },

    async login(username: string, password: string): Promise<LoginResult> {
      this.clearError();
      const payload: LoginRequest = {
        username,
        password,
        deviceId: getDeviceId(),
        deviceName: getDeviceName(),
      };

      try {
        const res = await postAuthLogin(payload);
        const data = unwrapOrThrow<AuthResponse, ProblemDetails>(res as any);
        this.applyAuth(data);
        return LoginResult.Success;
      } catch (e) {
        if (isHttpError<ProblemDetails>(e)) {
          if ((e.data as any)?.reason) {
            return (e.data as any).reason as LoginResult;
          }
          this.lastAuthError = e.data ?? null;
        }
        throw e;
      }
    },

    async loadUser(): Promise<boolean> {
      try {
        const res = await getUsersId("@me");
        if (res.status === 200) {
          this.loggedInUser = unwrapOrThrow<User, ProblemDetails>(res as any);
          return true;
        }

        this.loggedInUser = null;
        return false;
      } catch {
        this.loggedInUser = null;
        return false;
      }
    },

    async refresh(): Promise<boolean> {
      if (!this.accessToken || !this.refreshToken) return false;

      const payload: RefreshRequest = {
        accessToken: this.accessToken,
        refreshToken: this.refreshToken,
      };

      try {
        const res = await postAuthRefresh(payload);
        const data = unwrapOrThrow<AuthResponse, ProblemDetails>(res as any);
        this.applyAuth(data);
        return true;
      } catch {
        return false;
      }
    },

    async logout() {
      if (this.refreshToken) {
        const payload: LogoutRequest = { refreshToken: this.refreshToken };
        try {
          const res = await postAuthLogout(payload);
          if (res.status < 200 || res.status >= 300) {}
        } catch {}
      }
      this.clearAuth();
    },

    async logoutAll() {
      try {
        const res = await postAuthLogoutAll();
        if (res.status < 200 || res.status >= 300) {}
      } catch {}
      this.clearAuth();
    },

    async setAvatar(file: Blob): Promise<boolean> {
      if (!file) return false;
      if (!this.loggedInUser) return false;

      try {
        const res = unwrapOrThrow<UserResponse>(await putUsersMeAvatar({file}) as any);
        this.loggedInUser.avatar = {id: res.avatarId!};
        return true;
      } catch (e) {
        console.error(e)
        return false;
      }
    },

    initialize() {
      if (this.accessToken) setAuthAccessToken(this.accessToken);

      setAuthRefreshHandler(async () => {
        if (!this.accessTokenExpiration) return false;
        if (!isExpiringSoon(this.accessTokenExpiration, 30_000)) return true;
        return await this.refresh();
      });

      setAuthLogoutHandler(async () => {
        await this.logout();
      });
    },
  },
});
