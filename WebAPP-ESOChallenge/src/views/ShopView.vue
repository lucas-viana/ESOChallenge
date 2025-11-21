<template>
  <div class="shop-view">
    <!-- Header -->
    <header class="shop-header">
      <div class="shop-header-content">
        <h1 class="shop-title">🛒 Loja do Fortnite</h1>
        <p class="shop-subtitle">Descubra os itens cosméticos disponíveis</p>
      </div>
    </header>

    <!-- Main Container: FilterSidebar + Content -->
    <div class="shop-main-container">
      <div class="shop-content-wrapper">
        <!-- FilterSidebar -->
        <div class="sidebar-wrapper">
          <FilterSidebar
            :filters="filters"
            :metadata="cosmeticStore.searchMetadata"
            :hide-shop-filter="true"
            @update:filters="updateFilters"
            @apply="performSearch"
          />
        </div>

        <!-- Content Area -->
        <div class="items-wrapper">
          <main class="shop-content">
            <!-- Loading State -->
            <LoadingSpinner
              v-if="cosmeticStore.isSearching"
              message="Carregando itens da loja..."
            />

            <!-- Error State -->
            <ErrorMessage
              v-else-if="cosmeticStore.error"
              :message="cosmeticStore.error"
              @retry="performSearch"
            />

            <!-- Empty State -->
            <div v-else-if="!searchResults || searchResults.length === 0" class="empty-state">
              <div class="empty-icon">🛍️</div>
              <h3 class="empty-title">Nenhum item encontrado</h3>
              <p class="empty-description">Tente ajustar os filtros para ver mais resultados</p>
            </div>

            <!-- Scrollable Grid Container -->
            <div v-else-if="searchResults && searchResults.length > 0" class="grid-container">
              <div class="shop-grid">
                <CosmeticCardCompact
                  v-for="cosmetic in searchResults"
                  :key="cosmetic.id"
                  :cosmetic="cosmetic"
                  :show-purchase-button="authStore.isAuthenticated && !isItemOwned(cosmetic)"
                  @click="openDetailModal"
                  @purchased="openPurchaseConfirmation"
                />
              </div>
            </div>

            <!-- Paginação -->
            <nav v-if="totalPages > 1 && !cosmeticStore.isSearching" class="pagination">
              <button
                class="pagination-btn"
                :disabled="!hasPreviousPage"
                @click="goToPage(currentPage - 1)"
              >
                ← Anterior
              </button>

              <div class="pagination-numbers">
                <button
                  v-for="page in visiblePages"
                  :key="page"
                  :class="[
                    'pagination-number',
                    { active: page === currentPage, ellipsis: page === -1 },
                  ]"
                  :disabled="page === -1"
                  @click="page !== -1 && goToPage(page)"
                >
                  {{ page === -1 ? '...' : page }}
                </button>
              </div>

              <button
                class="pagination-btn"
                :disabled="!hasNextPage"
                @click="goToPage(currentPage + 1)"
              >
                Próxima →
              </button>
            </nav>
          </main>
        </div>
      </div>
    </div>

    <!-- Modal de Detalhes -->
    <CosmeticDetailModal
      :cosmetic="selectedCosmetic"
      :is-open="isModalOpen"
      :show-purchase-button="authStore.isAuthenticated && !isItemOwned(selectedCosmetic)"
      @close="closeDetailModal"
      @purchased="openPurchaseConfirmation"
    />

    <!-- Modal de Confirmação de Compra -->
    <PurchaseConfirmationModal
      :cosmetic="cosmeticToPurchase"
      :is-open="isConfirmationModalOpen"
      :current-v-bucks="purchaseStore.vbucks"
      :is-purchasing="isPurchasing"
      @close="closePurchaseConfirmation"
      @confirm="confirmPurchase"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useCosmeticStore } from '@/stores/cosmetic.store'
import { usePurchaseStore } from '@/stores/purchase.store'
import { useAuthStore } from '@/stores/auth.store'
import FilterSidebar from '@/components/FilterSidebar.vue'
import CosmeticCardCompact from '@/components/CosmeticCardCompact.vue'
import CosmeticDetailModal from '@/components/CosmeticDetailModal.vue'
import PurchaseConfirmationModal from '@/components/PurchaseConfirmationModal.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ErrorMessage from '@/components/ErrorMessage.vue'
import type { CosmeticFilters, Cosmetic } from '@/types/cosmetic.types'

const cosmeticStore = useCosmeticStore()
const purchaseStore = usePurchaseStore()
const authStore = useAuthStore()

// Modal State
const selectedCosmetic = ref<Cosmetic | null>(null)
const isModalOpen = ref(false)

// Purchase Confirmation Modal State
const cosmeticToPurchase = ref<Cosmetic | null>(null)
const isConfirmationModalOpen = ref(false)
const isPurchasing = ref(false)

// Filters State
// IMPORTANTE: onlyInShop está sempre true na loja - não pode ser modificado pelo usuário
const filters = ref<CosmeticFilters>({
  searchTerm: '',
  types: [],
  rarities: [],
  onlyInShop: true, // FIXO: Loja sempre mostra apenas itens disponíveis no momento
  excludeBundles: false,
  sortBy: 'price',
  sortOrder: 'desc',
})

// Computed
const searchResults = computed(() => cosmeticStore.searchResults)
const currentPage = computed(() => cosmeticStore.searchPagination.page)
const totalCount = computed(() => cosmeticStore.searchPagination.totalCount)
const totalPages = computed(() => cosmeticStore.searchPagination.totalPages)
const hasPreviousPage = computed(() => cosmeticStore.searchPagination.hasPreviousPage)
const hasNextPage = computed(() => cosmeticStore.searchPagination.hasNextPage)

const visiblePages = computed(() => {
  const pages: number[] = []
  const current = currentPage.value
  const total = totalPages.value

  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    pages.push(1)
    if (current > 3) pages.push(-1)
    for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
      pages.push(i)
    }
    if (current < total - 2) pages.push(-1)
    pages.push(total)
  }

  return pages
})

// Methods
const updateFilters = (newFilters: CosmeticFilters) => {
  // Garantir que onlyInShop sempre permaneça true na loja
  filters.value = { ...newFilters, onlyInShop: true }
}

const performSearch = async () => {
  await cosmeticStore.searchCosmetics(filters.value, 1)
}

const openDetailModal = (cosmetic: Cosmetic) => {
  selectedCosmetic.value = cosmetic
  isModalOpen.value = true
}

const closeDetailModal = () => {
  isModalOpen.value = false
  selectedCosmetic.value = null
}

const openPurchaseConfirmation = (cosmeticId: string) => {
  const cosmetic = searchResults.value?.find((c) => c.id === cosmeticId)
  if (cosmetic) {
    cosmeticToPurchase.value = cosmetic
    isConfirmationModalOpen.value = true
  }
}

const closePurchaseConfirmation = () => {
  if (!isPurchasing.value) {
    isConfirmationModalOpen.value = false
    cosmeticToPurchase.value = null
  }
}

const confirmPurchase = async (cosmeticId: string) => {
  if (!authStore.isAuthenticated || isPurchasing.value) return

  isPurchasing.value = true
  try {
    const response = await purchaseStore.purchaseCosmetic(cosmeticId)

    if (response.success) {
      console.log('Purchase successful:', response.message)
      // Fecha modais imediatamente ANTES de atualizar
      isPurchasing.value = false
      closePurchaseConfirmation()
      closeDetailModal()
      // Atualiza a busca em background
      performSearch()
    } else {
      console.error('Purchase failed:', response.message)
      isPurchasing.value = false
    }
  } catch (error) {
    console.error('Error during purchase:', error)
    isPurchasing.value = false
  }
}

const isItemOwned = (cosmetic: Cosmetic | null): boolean => {
  if (!cosmetic) return false
  return purchaseStore.ownedCosmetics.some((owned) => owned.id === cosmetic.id)
}

const goToPage = async (page: number) => {
  if (page < 1 || page > totalPages.value) return
  await cosmeticStore.searchCosmetics(filters.value, page)
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

// Lifecycle
onMounted(async () => {
  // Busca inicial com filtros de loja
  await performSearch()

  // Load user purchase data if authenticated
  if (authStore.isAuthenticated) {
    await Promise.all([purchaseStore.fetchVBucks(), purchaseStore.fetchOwnedCosmetics()])
  }
})
</script>

<style scoped>
.shop-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
}

.shop-header {
  margin-bottom: 1%;
  text-align: center;
}

.shop-header-content {
  max-width: 800px;
  margin: 0 auto;
}

.shop-title {
  font-size: 3rem;
  font-weight: 900;
  background: linear-gradient(135deg, #ffd700 0%, #ffed4e 50%, #ffd700 100%);
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

.shop-subtitle {
  color: #cbd5e0;
  font-size: 1.125rem;
  margin: 0;
}

/* Main Container */
.shop-main-container {
  max-width: 1600px;
  margin: 0 auto;
  padding: 0 24px;
}

.shop-content-wrapper {
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
.shop-content {
  display: flex;
  flex-direction: column;
  gap: 20px;
  height: 100%;
  overflow: hidden;
}

.shop-stats-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
  padding: 16px 20px;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  flex-shrink: 0;
}

.stats-info {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.content-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0;
}

.results-count {
  color: #b4b4b4;
  font-size: 0.875rem;
  margin: 0;
}

.page-indicator {
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  color: #b4b4b4;
  font-size: 0.875rem;
  font-weight: 600;
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
.shop-grid {
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

/* Pagination */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  padding: 16px 0;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  flex-shrink: 0;
}

.pagination-btn {
  padding: 10px 20px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: #ffffff;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.pagination-btn:hover:not(:disabled) {
  background: rgba(102, 126, 234, 0.2);
  border-color: #667eea;
  transform: translateY(-2px);
}

.pagination-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.pagination-numbers {
  display: flex;
  gap: 8px;
}

.pagination-number {
  width: 40px;
  height: 40px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: #b4b4b4;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.pagination-number:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
}

.pagination-number.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-color: #667eea;
  color: #ffffff;
}

.pagination-number.ellipsis {
  cursor: default;
  background: transparent;
  border-color: transparent;
  pointer-events: none;
}

.pagination-number.ellipsis:hover {
  background: transparent;
  border-color: transparent;
  transform: none;
}

/* Responsive */
@media (max-width: 1024px) {
  .shop-content-wrapper {
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

  .shop-grid {
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: 16px;
  }
}

@media (max-width: 768px) {
  .shop-view {
    padding: 60px 0 24px;
  }

  .shop-main-container {
    padding: 0 16px;
  }

  .shop-title {
    font-size: 2rem;
  }

  .shop-subtitle {
    font-size: 1rem;
  }

  .shop-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  }

  .pagination-numbers {
    display: none;
  }
}
</style>
