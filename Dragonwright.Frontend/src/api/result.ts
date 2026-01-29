export type OrvalOk<T> = { status: number; data: T; headers: Headers };
export type OrvalErr<E = unknown> = { status: number; data: E; headers: Headers };
export type OrvalResponse<T, E = unknown> = OrvalOk<T> | OrvalErr<E>;

export function unwrapOrThrow<T, E = any>(res: OrvalResponse<T, E>): T {
  if (res.status >= 200 && res.status < 300) return res.data as T;
  throw res;
}

export function unwrap<T, E = any>(res: OrvalResponse<T, E>): T | null {
  if (res.status >= 200 && res.status < 300) return res.data as T;
  return null;
}

export function isHttpError<E = any>(e: unknown): e is OrvalErr<E> {
  return typeof e === "object" && e !== null && "status" in e && "data" in e;
}
