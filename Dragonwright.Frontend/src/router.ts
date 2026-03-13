import { createRouter, createWebHistory } from 'vue-router'
import { routes, handleHotUpdate } from 'vue-router/auto-routes'
import {useAuthStore} from "@/stores/auth.ts";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

if (import.meta.hot) {
  handleHotUpdate(router)
}

router.beforeEach(async (to, _, next) => {
  if (to.path === '/login') {
    return next()
  }

  const authStore = useAuthStore()
  if (!authStore.isAuthenticated) {
    return next('/login')
  }

  if (to.path === '/logout') {
    await authStore.logout()
    return next('/login')
  }

  // Only load user if we don't have one yet (avoid re-fetching on every navigation)
  if (!authStore.loggedInUser) {
    if (!await authStore.loadUser()) {
      // Don't call logout() here — it would trigger another API call that may also 401.
      // Just clear local auth state and redirect.
      authStore.clearAuth()
      return next('/login')
    }
  }

  next()
})

export default router
