/**
 * Vue Router Configuration
 * Configuração de rotas com proteção de autenticação
 */

import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import HomeView from '../views/HomeView.vue'

/**
 * Definição de rotas da aplicação
 */
const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'home',
    component: HomeView,
    meta: {
      requiresAuth: false,
      title: 'Home'
    }
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Login',
      hideForAuth: true // Esconder para usuários já autenticados
    }
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('../views/RegisterView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Criar Conta',
      hideForAuth: true
    }
  },
  {
    path: '/cosmetics',
    name: 'cosmetics',
    component: () => import('../views/CosmeticsView.vue'),
    meta: {
      requiresAuth: false, // Temporariamente false para MVP
      title: 'Cosméticos'
    }
  },
  {
    path: '/about',
    name: 'about',
    component: () => import('../views/AboutView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Sobre'
    }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

/**
 * Navigation Guard
 * Protege rotas que requerem autenticação
 */
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.meta.requiresAuth as boolean
  const hideForAuth = to.meta.hideForAuth as boolean

  // Atualizar título da página
  const title = to.meta.title as string | undefined
  document.title = `${title || 'Fortnite API'} | Fortnite API`

  // Se a rota requer autenticação e usuário não está autenticado
  if (requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } })
    return
  }

  // Se a rota deve ser escondida para usuários autenticados (login/register)
  if (hideForAuth && authStore.isAuthenticated) {
    next({ name: 'cosmetics' })
    return
  }

  next()
})

export default router
