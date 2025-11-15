/**
 * Authentication Store
 * Store Pinia para gerenciamento de estado de autenticação
 * Segue Single Responsibility Principle
 */

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/auth.service'
import type { User, LoginRequest, RegisterRequest } from '@/types/auth.types'

/**
 * Store de autenticação usando Composition API
 */
export const useAuthStore = defineStore('auth', () => {
  // Estado
  const user = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters computados
  const isAuthenticated = computed(() => user.value !== null)
  const userEmail = computed(() => user.value?.email ?? '')

  /**
   * Inicializa o store verificando se há usuário autenticado
   */
  function initialize() {
    const currentUser = authService.getCurrentUser()
    if (currentUser) {
      user.value = currentUser
    }
  }

  /**
   * Realiza login do usuário
   * @param credentials Credenciais de login
   */
  async function login(credentials: LoginRequest): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      const loggedUser = await authService.login(credentials)
      user.value = loggedUser
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro ao fazer login'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Realiza registro de novo usuário
   * @param userData Dados do novo usuário
   */
  async function register(userData: RegisterRequest): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      const registeredUser = await authService.register(userData)
      user.value = registeredUser
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro ao registrar usuário'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Realiza logout do usuário
   */
  function logout(): void {
    authService.logout()
    user.value = null
    error.value = null
  }

  /**
   * Limpa mensagens de erro
   */
  function clearError(): void {
    error.value = null
  }

  // Inicializar ao criar a store
  initialize()

  return {
    // Estado
    user,
    isLoading,
    error,
    
    // Getters
    isAuthenticated,
    userEmail,
    
    // Actions
    login,
    register,
    logout,
    clearError,
    initialize
  }
})
