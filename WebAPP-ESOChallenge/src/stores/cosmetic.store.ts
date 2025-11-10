/**
 * Store Pinia para gerenciamento de estado dos cosméticos
 * Centraliza o estado da aplicação (Padrão Flux/Redux)
 * Segue os princípios de Clean Architecture
 */

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { cosmeticService } from '@/services/cosmetic.service'
import type { Cosmetic } from '@/types/cosmetic.types'

export const useCosmeticStore = defineStore('cosmetic', () => {
  // State (Dados reativos)
  const cosmetics = ref<Cosmetic[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const abortController = ref<AbortController | null>(null)

  // Getters (Computados - dados derivados do estado)
  const cosmeticsCount = computed(() => cosmetics.value.length)
  
  const hasCosmetics = computed(() => cosmetics.value.length > 0)
  
  const cosmeticsByRarity = computed(() => {
    const grouped = new Map<string, Cosmetic[]>()
    for (const cosmetic of cosmetics.value) {
      const rarity = cosmetic.rarity?.value || 'unknown'
      if (!grouped.has(rarity)) {
        grouped.set(rarity, [])
      }
      grouped.get(rarity)?.push(cosmetic)
    }
    return grouped
  })

  // Actions (Métodos que modificam o estado)
  
  /**
   * Reseta o estado para valores iniciais
   */
  function resetState() {
    cosmetics.value = []
    loading.value = false
    error.value = null
  }

  /**
   * Cancela requisição em andamento
   */
  function cancelRequest() {
    if (abortController.value) {
      abortController.value.abort()
      abortController.value = null
    }
  }

  /**
   * Busca todos os cosméticos
   */
  async function fetchAllCosmetics() {
    try {
      loading.value = true
      error.value = null
      cancelRequest()

      abortController.value = new AbortController()
      const data = await cosmeticService.getAllCosmetics(abortController.value.signal)
      
      cosmetics.value = data
    } catch (err) {
      if (err instanceof Error) {
        // Não seta erro se foi abortado pelo usuário
        if (err.name !== 'AbortError') {
          error.value = err.message
          console.error('Erro ao buscar cosméticos:', err)
        }
      }
    } finally {
      loading.value = false
      abortController.value = null
    }
  }

  /**
   * Busca cosméticos novos
   */
  async function fetchNewCosmetics() {
    try {
      loading.value = true
      error.value = null
      cancelRequest()

      abortController.value = new AbortController()
      const data = await cosmeticService.getNewCosmetics(abortController.value.signal)
      
      cosmetics.value = data
    } catch (err) {
      if (err instanceof Error) {
        if (err.name !== 'AbortError') {
          error.value = err.message
          console.error('Erro ao buscar cosméticos novos:', err)
        }
      }
    } finally {
      loading.value = false
      abortController.value = null
    }
  }

  /**
   * Busca cosméticos da loja
   */
  async function fetchShopCosmetics() {
    try {
      loading.value = true
      error.value = null
      cancelRequest()

      abortController.value = new AbortController()
      const data = await cosmeticService.getShopCosmetics(abortController.value.signal)
      
      cosmetics.value = data
    } catch (err) {
      if (err instanceof Error) {
        if (err.name !== 'AbortError') {
          error.value = err.message
          console.error('Erro ao buscar cosméticos da loja:', err)
        }
      }
    } finally {
      loading.value = false
      abortController.value = null
    }
  }

  /**
   * Busca cosmético por ID
   */
  async function getCosmeticById(id: string): Promise<Cosmetic | null> {
    try {
      loading.value = true
      error.value = null

      return await cosmeticService.getCosmeticById(id)
    } catch (err) {
      if (err instanceof Error) {
        error.value = err.message
        console.error('Erro ao buscar cosmético:', err)
      }
      return null
    } finally {
      loading.value = false
    }
  }

  // Retorna o estado e as ações
  return {
    // State
    cosmetics,
    loading,
    error,
    
    // Getters
    cosmeticsCount,
    hasCosmetics,
    cosmeticsByRarity,
    
    // Actions
    fetchAllCosmetics,
    fetchNewCosmetics,
    fetchShopCosmetics,
    getCosmeticById,
    resetState,
    cancelRequest,
  }
})
