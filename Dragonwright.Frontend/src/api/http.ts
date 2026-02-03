type RefreshFn = () => Promise<boolean>;
type LogoutFn = () => Promise<void> | void;

let accessToken: string | null = null;
let refreshHandler: RefreshFn | null = null;
let logoutHandler: LogoutFn | null = null;

const apiUrl = import.meta.env.VITE_API_URL as string ?? window.location.origin;

export function setAuthAccessToken(token: string | null) {
  accessToken = token;
}

export function setAuthRefreshHandler(fn: RefreshFn) {
  refreshHandler = fn;
}

export function setAuthLogoutHandler(fn: LogoutFn) {
  logoutHandler = fn;
}

let refreshPromise: Promise<boolean> | null = null;

async function ensureFreshToken(): Promise<boolean> {
  if (!refreshHandler) return true;
  if (refreshPromise) return refreshPromise;
  refreshPromise = (async () => {
    try {
      return await refreshHandler();
    } finally {
      refreshPromise = null;
    }
  })();
  return refreshPromise;
}

async function parseData(res: Response) {
  const ct = res.headers.get("content-type") ?? "";
  if (res.status === 204) return undefined;
  if (ct.includes("application/json") || ct.includes("text/json") || ct.includes("+json")) {
    return await res.json().catch(() => undefined);
  }
  const text = await res.text().catch(() => "");
  return text ? text : undefined;
}

export async function customFetch<T>(url: string, options: RequestInit = {}): Promise<T> {
  const u = new URL(url, apiUrl);
  const path = u.pathname;

  const isAuthLogin = path.includes("/auth/login");
  const isAuthRegister = path.includes("/auth/register");
  const isAuthRefresh = path.includes("/auth/refresh");

  const headers = new Headers(options.headers ?? {});
  if (!headers.has("Accept")) headers.set("Accept", "application/json");
  if (accessToken) headers.set("Authorization", `Bearer ${accessToken}`);

  const requestOnce = async () => {
    const res = await fetch(u, { ...options, headers });
    const data = await parseData(res);
    return { status: res.status, data, headers: res.headers } as any;
  };

  if (!isAuthLogin && !isAuthRegister && !isAuthRefresh) {
    await ensureFreshToken();
    if (accessToken) headers.set("Authorization", `Bearer ${accessToken}`);
  }

  const first = await requestOnce();

  if (
    first.status === 401 &&
    !isAuthLogin &&
    !isAuthRegister &&
    !isAuthRefresh
  ) {
    const refreshed = await ensureFreshToken();
    if (refreshed && accessToken) {
      headers.set("Authorization", `Bearer ${accessToken}`);
      const retry = await requestOnce();
      if (retry.status !== 401) return retry as T;
    }
    if (logoutHandler) await logoutHandler();
  }

  return first as T;
}
