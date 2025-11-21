<template>
  <div class="inventory-view">
    <!-- Header -->
    <header class="inventory-header">
      <div class="inventory-header-content">
        <h1 class="inventory-title">🎒 Inventário</h1>
        <p class="inventory-subtitle">Sua coleção de cosméticos do Fortnite</p>
      </div>
    </header>

    <!-- Main Container: Sidebar + Content -->
    <div class="inventory-main-container">
      <div class="inventory-content-wrapper">
        <!-- FilterSidebar -->
        <div class="sidebar-wrapper">
          <FilterSidebar
            :filters="filters"
            :metadata="filterMetadata"
            :hide-shop-filter="true"
            @update:filters="updateFilters"
            @apply="applyFilters"
          />
        </div>

        <!-- Content Area -->
        <div class="items-wrapper">
          <main class="inventory-content">
            <!-- Loading State -->
            <LoadingSpinner v-if="purchaseStore.isLoading" message="Carregando inventário..." />

            <!-- Error State -->
            <ErrorMessage v-else-if="purchaseStore.error" :message="purchaseStore.error" />

            <!-- Empty State -->
            <div v-else-if="ownedCosmetics.length === 0" class="empty-state">
              <div class="empty-icon">📦</div>
              <h3 class="empty-title">Nenhum item adquirido ainda</h3>
              <p class="empty-description">Visite a loja para começar sua coleção!</p>
              <router-link to="/shop" class="btn-shop">🛒 Ir para a Loja</router-link>
            </div>

            <!-- Scrollable Grid Container -->
            <div v-else class="grid-container">
              <div class="inventory-grid">
                <CosmeticCardCompact
                  v-for="cosmetic in filteredCosmetics"
                  :key="cosmetic.id"
                  :cosmetic="cosmetic"
                  :show-purchase-button="false"
                  :show-refund-button="true"
                  @click="openDetailModal"
                  @refund="handleRefund"
                />
              </div>
            </div>
          </main>
        </div>
      </div>
    </div>

    <!-- Modal de Detalhes -->
    <CosmeticDetailModal
      :cosmetic="selectedCosmetic"
      :is-open="isModalOpen"
      :show-purchase-button="false"
      :show-refund-button="true"
      @close="closeDetailModal"
      @refund="handleRefund"
    />

    <!-- Modal de Feedback de Devolução -->
    <FeedbackModal
      :is-open="feedbackModal.isOpen"
      :type="feedbackModal.type"
      :title="feedbackModal.title"
      :message="feedbackModal.message"
      :details="feedbackModal.details"
      @close="closeFeedbackModal"
    />

    <!-- Modal de Confirmação de Devolução -->
    <ConfirmModal
      :is-open="confirmModal.isOpen"
      :title="confirmModal.title"
      :message="confirmModal.message"
      :icon="confirmModal.icon"
      :type="confirmModal.type"
      :confirm-text="confirmModal.confirmText"
      :cancel-text="confirmModal.cancelText"
      @confirm="processRefund"
      @cancel="closeConfirmModal"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usePurchaseStore } from '@/stores/purchase.store'
import FilterSidebar from '@/components/FilterSidebar.vue'
import CosmeticCardCompact from '@/components/CosmeticCardCompact.vue'
import CosmeticDetailModal from '@/components/CosmeticDetailModal.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ErrorMessage from '@/components/ErrorMessage.vue'
import FeedbackModal from '@/components/FeedbackModal.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import type { Cosmetic, CosmeticFilters, FilterMetadata } from '@/types/cosmetic.types'

const purchaseStore = usePurchaseStore()

// Modal State
const selectedCosmetic = ref<Cosmetic | null>(null)
const isModalOpen = ref(false)

// Feedback Modal State
const feedbackModal = ref({
  isOpen: false,
  type: 'success' as 'success' | 'error' | 'warning' | 'info',
  title: '',
  message: '',
  details: undefined as Record<string, string | number> | undefined,
})

// Confirm Modal State
const confirmModal = ref({
  isOpen: false,
  title: 'Confirmar Devolução',
  message: 'Tem certeza que deseja devolver este item? O valor será reembolsado em V-Bucks.',
  icon: '🔄',
  type: 'danger' as 'danger' | 'warning' | 'info',
  confirmText: 'Devolver',
  cancelText: 'Cancelar',
})

// ID do item a ser devolvido
const refundItemId = ref<string | null>(null)

// Filters State
const filters = ref<CosmeticFilters>({
  searchTerm: '',
  types: [],
  rarities: [],
  onlyNew: false,
  onlyInShop: false,
  excludeBundles: false,
  sortBy: 'added',
  sortOrder: 'desc',
})

// Converter PurchasedCosmetic para Cosmetic
const toCosmetic = (purchased: any): Cosmetic => ({
  ...purchased,
  price: purchased.purchasePrice || 0,
  isAvailable: true,
  rarity: purchased.rarity
    ? {
        value: purchased.rarity,
        displayValue: purchased.rarity,
        backendValue: purchased.rarity,
      }
    : undefined,
  type: purchased.type
    ? {
        value: purchased.type,
        displayValue: purchased.type,
        backendValue: purchased.type,
      }
    : undefined,
  // Preservar dados de bundle
  isBundle: purchased.isBundle || false,
  bundle: purchased.bundle,
  containedItemIds: purchased.containedItemIds,
  containedItems: purchased.containedItems,
  containedItemsImages: purchased.containedItemsImages,
})

// Computed
const ownedCosmetics = computed(() => {
  const items = purchaseStore.ownedCosmetics

  // Debug: ver estrutura dos dados
  console.log('📦 Owned Cosmetics Raw:', items)

  const bundleMap = new Map<string, any>()
  const result: any[] = []

  // Primeiro passo: capturar dados dos bundles pai
  for (const item of items) {
    if (item.id.startsWith('BUNDLE_')) {
      // Este é o BUNDLE PAI - tem o preço total
      bundleMap.set(item.id, {
        id: item.id,
        name: item.name || item.id.replace('BUNDLE_', '').replace(/_/g, ' '),
        price: item.purchasePrice || 0,
        purchasePrice: item.purchasePrice || 0,
        purchasedAt: item.purchasedAt,
        isAvailable: true,
        isBundle: true,
        containedItems: [],
        containedItemsImages: [],
        images: item.images,
        rarity: item.rarity
          ? {
              value: item.rarity,
              displayValue: item.rarity,
              backendValue: item.rarity,
            }
          : undefined,
        type: {
          value: 'Bundle',
          displayValue: 'Bundle',
          backendValue: 'Bundle',
        },
        bundle: {
          name: item.name || item.id.replace('BUNDLE_', '').replace(/_/g, ' '),
          info: item.description || 'Pacote com múltiplos itens',
          image: item.images?.icon || '',
        },
      })
    }
  }

  // Segundo passo: processar itens
  for (const item of items) {
    // Ignorar bundles pai (já processados)
    if (item.id.startsWith('BUNDLE_')) {
      continue
    }

    // Se o item tem bundleId, adicionar ao bundle existente
    if (item.bundleId && bundleMap.has(item.bundleId)) {
      const bundle = bundleMap.get(item.bundleId)!
      bundle.containedItems.push({
        name: item.name,
        type: item.type
          ? {
              value: item.type,
              displayValue: item.type,
              backendValue: item.type,
            }
          : undefined,
        rarity: item.rarity
          ? {
              value: item.rarity,
              displayValue: item.rarity,
              backendValue: item.rarity,
            }
          : undefined,
      })
      bundle.containedItemsImages.push(item.images?.icon || 'https://via.placeholder.com/200')
    }
    // Item individual (não faz parte de bundle)
    else if (!item.bundleId) {
      result.push(toCosmetic(item))
    }
  }

  // Adicionar bundles ao resultado
  result.push(...Array.from(bundleMap.values()))

  console.log('📦 Processed Cosmetics:', result)

  return result
})

// Aplicar filtros localmente
const filteredCosmetics = computed(() => {
  let items = [...ownedCosmetics.value]

  // Filtro de busca por nome
  if (filters.value.searchTerm) {
    const searchLower = filters.value.searchTerm.toLowerCase()
    items = items.filter((item) => item.name?.toLowerCase().includes(searchLower))
  }

  // Filtro por tipo
  if (filters.value.types && filters.value.types.length > 0) {
    items = items.filter((item) => {
      const itemType = item.type?.value || item.type?.displayValue || ''
      return filters.value.types!.some((t) => itemType.toLowerCase() === t.toLowerCase())
    })
  }

  // Filtro por raridade
  if (filters.value.rarities && filters.value.rarities.length > 0) {
    items = items.filter((item) => {
      const itemRarity = item.rarity?.value || item.rarity?.displayValue || ''
      return filters.value.rarities!.some((r) => itemRarity.toLowerCase() === r.toLowerCase())
    })
  }

  // Filtro de bundles
  if (filters.value.excludeBundles) {
    items = items.filter((item) => !item.isBundle)
  }

  // Filtro de preço
  if (filters.value.minPrice !== undefined) {
    items = items.filter((item) => (item.price || 0) >= filters.value.minPrice!)
  }
  if (filters.value.maxPrice !== undefined) {
    items = items.filter((item) => (item.price || 0) <= filters.value.maxPrice!)
  }

  // Filtro de data
  if (filters.value.addedAfter) {
    const afterDate = new Date(filters.value.addedAfter)
    items = items.filter((item) => {
      const itemDate = new Date((item as any).purchasedAt || (item as any).added || 0)
      return itemDate >= afterDate
    })
  }
  if (filters.value.addedBefore) {
    const beforeDate = new Date(filters.value.addedBefore)
    items = items.filter((item) => {
      const itemDate = new Date((item as any).purchasedAt || (item as any).added || 0)
      return itemDate <= beforeDate
    })
  }

  // Ordenação
  const sortBy = filters.value.sortBy || 'added'
  const sortOrder = filters.value.sortOrder || 'desc'

  items.sort((a, b) => {
    let compareValue = 0

    switch (sortBy) {
      case 'name':
        compareValue = (a.name || '').localeCompare(b.name || '')
        break
      case 'price':
        compareValue = (a.price || 0) - (b.price || 0)
        break
      case 'rarity': {
        const rarityOrder = ['common', 'uncommon', 'rare', 'epic', 'legendary']
        const aRarity = (a.rarity?.value || '').toLowerCase()
        const bRarity = (b.rarity?.value || '').toLowerCase()
        compareValue = rarityOrder.indexOf(aRarity) - rarityOrder.indexOf(bRarity)
        break
      }
      case 'added':
      default: {
        const aDate = new Date((a as any).purchasedAt || (a as any).added || 0).getTime()
        const bDate = new Date((b as any).purchasedAt || (b as any).added || 0).getTime()
        compareValue = aDate - bDate
        break
      }
    }

    return sortOrder === 'asc' ? compareValue : -compareValue
  })

  return items
})

// Metadados para FilterSidebar
const filterMetadata = computed<FilterMetadata>(() => {
  const types: Record<string, number> = {}
  const rarities: Record<string, number> = {}
  let maxPrice = 0
  let minPrice = Infinity

  for (const item of ownedCosmetics.value) {
    // Tipos
    const type = item.type?.value || item.type?.displayValue || 'Unknown'
    types[type] = (types[type] || 0) + 1

    // Raridades
    const rarity = item.rarity?.value || item.rarity?.displayValue || 'Common'
    rarities[rarity] = (rarities[rarity] || 0) + 1

    // Preço máximo e mínimo
    if (item.price) {
      if (item.price > maxPrice) maxPrice = item.price
      if (item.price < minPrice) minPrice = item.price
    }
  }

  return {
    availableTypes: types,
    availableRarities: rarities,
    minPriceAvailable: minPrice === Infinity ? 0 : minPrice,
    maxPriceAvailable: maxPrice,
  }
})

// Methods
const updateFilters = (newFilters: CosmeticFilters) => {
  filters.value = { ...newFilters }
}

const applyFilters = () => {
  // Filtros são aplicados automaticamente via computed
}

const openDetailModal = (cosmetic: Cosmetic) => {
  selectedCosmetic.value = cosmetic
  isModalOpen.value = true
}

const closeDetailModal = () => {
  isModalOpen.value = false
  selectedCosmetic.value = null
}

const closeFeedbackModal = () => {
  feedbackModal.value.isOpen = false
}

const closeConfirmModal = () => {
  confirmModal.value.isOpen = false
  refundItemId.value = null
}

const handleRefund = async (cosmeticId: string) => {
  console.log('🎯 handleRefund chamado com ID:', cosmeticId)

  // Verificar se o item é parte de um bundle
  const item = ownedCosmetics.value.find((c) => c.id === cosmeticId)

  console.log('🔍 Item encontrado:', item)

  if (!item) {
    feedbackModal.value = {
      isOpen: true,
      type: 'error',
      title: 'Erro',
      message: 'Item não encontrado no inventário.',
      details: undefined,
    }
    return
  }

  // Se o item tem bundleId, não pode ser devolvido individualmente
  if ((item as any).bundleId) {
    feedbackModal.value = {
      isOpen: true,
      type: 'warning',
      title: 'Item de Pacote',
      message: 'Este item faz parte de um pacote e não pode ser devolvido individualmente. Para devolver, selecione o pacote completo.',
      details: undefined,
    }
    return
  }

  // Armazena o ID e abre modal de confirmação
  console.log('✅ Armazenando ID para devolução:', cosmeticId)
  refundItemId.value = cosmeticId
  confirmModal.value.isOpen = true
}

const processRefund = async () => {
  if (!refundItemId.value) return

  console.log('💳 processRefund - ID armazenado:', refundItemId.value)

  // IMPORTANTE: Salvar o ID antes de fechar o modal (que limpa refundItemId)
  const itemId = refundItemId.value

  // Fecha modal de confirmação
  closeConfirmModal()

  try {
    console.log('📤 Enviando requisição de devolução para ID:', itemId)
    const response = await purchaseStore.refundCosmetic(itemId)

    console.log('📥 Resposta recebida:', response)

    // Fecha modal de detalhes se estiver aberto
    closeDetailModal()

    if (response.success) {
      // Atualiza dados
      await Promise.all([purchaseStore.fetchOwnedCosmetics(), purchaseStore.fetchVBucks()])

      // Mostra feedback de sucesso
      feedbackModal.value = {
        isOpen: true,
        type: 'success',
        title: 'Devolução Realizada! 🎉',
        message: response.message || 'Item devolvido com sucesso!',
        details: {
          'V-Bucks Reembolsados': response.refundedAmount || 0,
          'Saldo Atual': response.remainingVBucks || 0,
        },
      }
    } else {
      // Mostra feedback de erro
      feedbackModal.value = {
        isOpen: true,
        type: 'error',
        title: 'Erro na Devolução',
        message: response.message || 'Não foi possível processar a devolução.',
        details: undefined,
      }
    }
  } catch (error: any) {
    console.error('Erro ao processar devolução:', error)

    // Extrair mensagem de erro da resposta
    let errorMessage = 'Ocorreu um erro ao processar a devolução.'

    if (error.response?.data?.message) {
      errorMessage = error.response.data.message
    } else if (error.response?.data?.errors) {
      // Erros de validação do ASP.NET Core
      const errors = Object.values(error.response.data.errors).flat()
      errorMessage = errors.join(', ')
    } else if (error.message) {
      errorMessage = error.message
    }

    // Mostra feedback de erro
    feedbackModal.value = {
      isOpen: true,
      type: 'error',
      title: 'Erro Inesperado',
      message: errorMessage,
      details: error.response?.status ? {
        'Código do Erro': error.response.status,
      } : undefined,
    }
  }
}

// Lifecycle
onMounted(async () => {
  await Promise.all([purchaseStore.fetchOwnedCosmetics(), purchaseStore.fetchVBucks()])
})
</script>

<style scoped>
.inventory-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
}

.inventory-header {
  margin-bottom: 1%;
  text-align: center;
}

.inventory-header-content {
  max-width: 800px;
  margin: 0 auto;
}

.inventory-title {
  font-size: 3rem;
  font-weight: 900;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 50%, #ec4899 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0 0 16px 0;
  animation: shine 3s ease-in-out infinite;
}

@keyframes shine {
  0%,
  100% {
    filter: brightness(1);
  }
  50% {
    filter: brightness(1.2);
  }
}

.inventory-subtitle {
  color: #cbd5e0;
  font-size: 1.125rem;
  margin: 0;
}

/* Main Container */
.inventory-main-container {
  max-width: 1600px;
  margin: 0 auto;
  padding: 0 24px;
}

.inventory-content-wrapper {
  display: flex;
  gap: 24px;
  background: rgba(26, 26, 46, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 24px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  height: calc(100vh - 240px);
  overflow: hidden;
}

.sidebar-wrapper {
  flex-shrink: 0;
  height: 100%;
  display: flex;
}

.items-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
}

/* Content Area */
.inventory-content {
  display: flex;
  flex-direction: column;
  gap: 20px;
  height: 100%;
  overflow: hidden;
}

/* Grid Container with Scroll */
.grid-container {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 8px;
}

.grid-container::-webkit-scrollbar {
  width: 8px;
}

.grid-container::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 4px;
}

.grid-container::-webkit-scrollbar-thumb {
  background: rgba(102, 126, 234, 0.5);
  border-radius: 4px;
}

.grid-container::-webkit-scrollbar-thumb:hover {
  background: rgba(102, 126, 234, 0.7);
}

/* Grid */
.inventory-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
  padding-bottom: 20px;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  text-align: center;
  gap: 16px;
}

.empty-icon {
  font-size: 4rem;
  opacity: 0.5;
}

.empty-title {
  color: #ffffff;
  font-size: 1.5rem;
  margin: 0;
}

.empty-description {
  color: #b4b4b4;
  font-size: 1rem;
  margin: 0;
}

.btn-shop {
  display: inline-block;
  padding: 12px 32px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  text-decoration: none;
  border-radius: 8px;
  font-weight: 600;
  transition: all 0.3s ease;
}

.btn-shop:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
}

/* Responsive */
@media (max-width: 1024px) {
  .inventory-content-wrapper {
    flex-direction: column;
    height: auto;
    max-height: calc(100vh - 240px);
  }

  .sidebar-wrapper {
    height: auto;
    max-height: 400px;
  }

  .items-wrapper {
    flex: 1;
    min-height: 500px;
  }

  .inventory-grid {
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: 16px;
  }
}

@media (max-width: 768px) {
  .inventory-view {
    padding: 60px 0 24px;
  }

  .inventory-main-container {
    padding: 0 16px;
  }

  .inventory-title {
    font-size: 2rem;
  }

  .inventory-subtitle {
    font-size: 1rem;
  }

  .inventory-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  }
}
</style>
