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

  if (!await authStore.loadUser()) {
    authStore.clearAuth()
    return next('/login')
  }

  if (to.path === '/logout') {
    await authStore.logout()
    return next('/login')
  }

  next()
})

export default router
