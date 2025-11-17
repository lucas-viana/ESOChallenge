import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type {
  PurchasedCosmetic,
  PurchaseResponse,
  RefundResponse,
  PurchaseHistoryResponse
} from '@/types/purchase.types'
import { purchaseService } from '@/services/purchase.service'

export const usePurchaseStore = defineStore('purchase', () => {
  // State
  const vbucks = ref<number>(0)
  const ownedCosmetics = ref<PurchasedCosmetic[]>([])
  const purchaseHistory = ref<PurchaseHistoryResponse | null>(null)
  const isLoading = ref<boolean>(false)
  const error = ref<string | null>(null)

  // Getters
  const hasVBucks = computed(() => vbucks.value > 0)
  const ownedCosmeticIds = computed(() => 
    ownedCosmetics.value.map(c => c.id)
  )

  // Actions
  async function fetchVBucks() {
    try {
      isLoading.value = true
      error.value = null
      vbucks.value = await purchaseService.getVBucks()
    } catch (err: any) {
      error.value = err.message || 'Erro ao buscar V-Bucks'
      console.error('Error fetching V-Bucks:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchOwnedCosmetics() {
    try {
      isLoading.value = true
      error.value = null
      ownedCosmetics.value = await purchaseService.getMyCosmetics()
    } catch (err: any) {
      error.value = err.message || 'Erro ao buscar itens'
      console.error('Error fetching owned cosmetics:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function purchaseCosmetic(cosmeticId: string): Promise<PurchaseResponse> {
    try {
      isLoading.value = true
      error.value = null
      
      const response = await purchaseService.purchaseCosmetic(cosmeticId)
      
      if (response.success) {
        // Update V-Bucks
        vbucks.value = response.remainingVBucks
        
        // Add to owned cosmetics
        if (response.purchasedCosmetic) {
          ownedCosmetics.value.unshift(response.purchasedCosmetic)
        }
      }
      
      return response
    } catch (err: any) {
      error.value = err.message || 'Erro ao realizar compra'
      console.error('Error purchasing cosmetic:', err)
      return {
        success: false,
        message: error.value || 'Erro desconhecido',
        remainingVBucks: vbucks.value
      }
    } finally {
      isLoading.value = false
    }
  }

  async function refundCosmetic(cosmeticId: string): Promise<RefundResponse> {
    try {
      isLoading.value = true
      error.value = null
      
      const response = await purchaseService.refundCosmetic(cosmeticId)
      
      if (response.success) {
        // Update V-Bucks
        vbucks.value = response.remainingVBucks
        
        // Remove from owned cosmetics
        ownedCosmetics.value = ownedCosmetics.value.filter(c => c.id !== cosmeticId)
      }
      
      return response
    } catch (err: any) {
      error.value = err.message || 'Erro ao realizar reembolso'
      console.error('Error refunding cosmetic:', err)
      return {
        success: false,
        message: error.value || 'Erro desconhecido',
        refundedAmount: 0,
        remainingVBucks: vbucks.value
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchPurchaseHistory() {
    try {
      isLoading.value = true
      error.value = null
      purchaseHistory.value = await purchaseService.getPurchaseHistory()
    } catch (err: any) {
      error.value = err.message || 'Erro ao buscar histórico'
      console.error('Error fetching purchase history:', err)
    } finally {
      isLoading.value = false
    }
  }

  function ownsCosmetic(cosmeticId: string): boolean {
    return ownedCosmeticIds.value.includes(cosmeticId)
  }

  function canAfford(price: number): boolean {
    return vbucks.value >= price
  }

  function reset() {
    vbucks.value = 0
    ownedCosmetics.value = []
    purchaseHistory.value = null
    error.value = null
  }

  return {
    // State
    vbucks,
    ownedCosmetics,
    purchaseHistory,
    isLoading,
    error,
    
    // Getters
    hasVBucks,
    ownedCosmeticIds,
    
    // Actions
    fetchVBucks,
    fetchOwnedCosmetics,
    purchaseCosmetic,
    refundCosmetic,
    fetchPurchaseHistory,
    ownsCosmetic,
    canAfford,
    reset
  }
})
