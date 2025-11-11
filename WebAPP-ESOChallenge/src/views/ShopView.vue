<template>
  <div class="shop-view">
    <!-- Header -->
    <header class="shop-header">
      <div class="shop-header-content">
        <h1 class="shop-title">🛒 Loja do Fortnite</h1>
        <p class="shop-subtitle">
          Descubra os itens cosméticos disponíveis
        </p>
      </div>
    </header>

    <!-- Tabs Navigation -->
    <div class="tabs-container">
      <div class="tabs">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          :class="['tab-btn', { active: activeTab === tab.id }]"
          @click="changeTab(tab.id)"
        >
          <span class="tab-emoji">{{ tab.emoji }}</span>
          <span class="tab-label">{{ tab.label }}</span>
        </button>
      </div>
    </div>

    <!-- Tab Content: Em Promoção -->
    <div v-if="activeTab === 'shop'" class="tab-content">
      <LoadingSpinner v-if="cosmeticStore.loading" message="Carregando itens da loja..." />
      <ErrorMessage v-else-if="cosmeticStore.error" :message="cosmeticStore.error" @retry="loadShopItems" />
      <div v-else-if="!cosmeticStore.hasCosmetics" class="empty-state">
        <div class="empty-icon">🛍️</div>
        <h3 class="empty-title">Loja Vazia</h3>
        <p class="empty-description">Não há itens disponíveis na loja no momento.</p>
        <button @click="loadShopItems" class="load-btn">Atualizar Loja</button>
      </div>
      <div v-else class="shop-container">
        <div class="shop-stats-card">
          <div class="stat-item">
            <span class="stat-label">Itens em Promoção</span>
            <span class="stat-value">{{ cosmeticStore.cosmeticsCount }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Última Atualização</span>
            <span class="stat-value">{{ currentDate }}</span>
          </div>
        </div>
        <div class="shop-grid">
          <CosmeticCard v-for="cosmetic in cosmeticStore.cosmetics" :key="cosmetic.id" :cosmetic="cosmetic" />
        </div>
      </div>
    </div>

    <!-- Tab Content: Novos Lançamentos -->
    <div v-if="activeTab === 'new'" class="tab-content">
      <LoadingSpinner v-if="newLoading" message="Carregando novos lançamentos..." />
      <ErrorMessage v-else-if="newError" :message="newError" @retry="loadNewItems" />
      <div v-else-if="newCosmetics.length === 0" class="empty-state">
        <div class="empty-icon">✨</div>
        <h3 class="empty-title">Nenhum Lançamento</h3>
        <p class="empty-description">Não há novos itens disponíveis no momento.</p>
        <button @click="loadNewItems" class="load-btn">Atualizar</button>
      </div>
      <div v-else class="shop-container">
        <div class="shop-stats-card">
          <div class="stat-item">
            <span class="stat-label">Novos Lançamentos</span>
            <span class="stat-value">{{ newCosmetics.length }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Última Atualização</span>
            <span class="stat-value">{{ currentDate }}</span>
          </div>
        </div>
        <div class="shop-grid">
          <CosmeticCard v-for="cosmetic in newCosmetics" :key="cosmetic.id" :cosmetic="cosmetic" />
        </div>
      </div>
    </div>

    <!-- Tab Content: Todos os Itens (com paginação) -->
    <div v-if="activeTab === 'all'" class="tab-content">
      <LoadingSpinner v-if="allLoading" message="Carregando todos os itens..." />
      <ErrorMessage v-else-if="allError" :message="allError" @retry="loadAllItems" />
      <div v-else-if="allCosmetics.length === 0" class="empty-state">
        <div class="empty-icon">📦</div>
        <h3 class="empty-title">Nenhum Item</h3>
        <p class="empty-description">Não há itens disponíveis no momento.</p>
        <button @click="loadAllItems" class="load-btn">Atualizar</button>
      </div>
      <div v-else class="shop-container">
        <div class="shop-stats-card">
          <div class="stat-item">
            <span class="stat-label">Total de Itens</span>
            <span class="stat-value">{{ allCosmetics.length }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Mostrando</span>
            <span class="stat-value">{{ displayedItems.length }} de {{ allCosmetics.length }}</span>
          </div>
        </div>
        <div class="shop-grid">
          <CosmeticCard v-for="cosmetic in displayedItems" :key="cosmetic.id" :cosmetic="cosmetic" />
        </div>
        <div v-if="hasMoreItems" class="load-more-container">
          <button @click="loadMore" class="load-more-btn">
            Carregar Mais ({{ remainingItems }} restantes)
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useCosmeticStore } from '@/stores/cosmetic.store'
import CosmeticCard from '@/components/CosmeticCard.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ErrorMessage from '@/components/ErrorMessage.vue'
import type { Cosmetic } from '@/types/cosmetic.types'

const cosmeticStore = useCosmeticStore()

// Tabs
const tabs = [
  { id: 'shop', label: 'Em Promoção', emoji: '🔥' },
  { id: 'new', label: 'Novos Lançamentos', emoji: '✨' },
  { id: 'all', label: 'Todos os Itens', emoji: '📦' }
]
const activeTab = ref('shop')

// New Items State
const newCosmetics = ref<Cosmetic[]>([])
const newLoading = ref(false)
const newError = ref<string | null>(null)

// All Items State
const allCosmetics = ref<Cosmetic[]>([])
const allLoading = ref(false)
const allError = ref<string | null>(null)
const itemsPerPage = 24
const currentPage = ref(1)

// Computed
const currentDate = computed(() => {
  const now = new Date()
  return now.toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  })
})

const displayedItems = computed(() => {
  return allCosmetics.value.slice(0, currentPage.value * itemsPerPage)
})

const hasMoreItems = computed(() => {
  return displayedItems.value.length < allCosmetics.value.length
})

const remainingItems = computed(() => {
  return allCosmetics.value.length - displayedItems.value.length
})

// Methods
async function loadShopItems() {
  await cosmeticStore.fetchShopCosmetics()
}

async function loadNewItems() {
  newLoading.value = true
  newError.value = null
  try {
    await cosmeticStore.fetchNewCosmetics()
    newCosmetics.value = cosmeticStore.cosmetics
  } catch (error) {
    newError.value = 'Erro ao carregar novos itens'
    console.error('Error loading new items:', error)
  } finally {
    newLoading.value = false
  }
}

async function loadAllItems() {
  allLoading.value = true
  allError.value = null
  currentPage.value = 1
  try {
    await cosmeticStore.fetchAllCosmetics()
    allCosmetics.value = cosmeticStore.cosmetics
  } catch (error) {
    allError.value = 'Erro ao carregar todos os itens'
    console.error('Error loading all items:', error)
  } finally {
    allLoading.value = false
  }
}

function changeTab(tabId: string) {
  activeTab.value = tabId
  
  // Load data for the active tab if not loaded yet
  if (tabId === 'new' && newCosmetics.value.length === 0 && !newLoading.value) {
    loadNewItems()
  } else if (tabId === 'all' && allCosmetics.value.length === 0 && !allLoading.value) {
    loadAllItems()
  }
}

function loadMore() {
  currentPage.value++
}

// Lifecycle
onMounted(async () => {
  await loadShopItems()
})

onUnmounted(() => {
  cosmeticStore.cancelRequest()
})
</script>

<style scoped>
.shop-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  padding: 24px;
}

.shop-header {
  margin-bottom: 32px;
  text-align: center;
}

.shop-header-content {
  max-width: 800px;
  margin: 0 auto;
}

.shop-title {
  font-size: 3.5rem;
  font-weight: 900;
  background: linear-gradient(135deg, #ffd700 0%, #ffed4e 50%, #ffd700 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  text-shadow: 0 4px 20px rgba(255, 215, 0, 0.3);
  margin: 0 0 16px 0;
  animation: shine 3s ease-in-out infinite;
}

@keyframes shine {
  0%, 100% { filter: brightness(1); }
  50% { filter: brightness(1.2); }
}

.shop-subtitle {
  font-size: 1.25rem;
  color: #cbd5e0;
  margin: 0;
}

/* Tabs */
.tabs-container {
  max-width: 1400px;
  margin: 0 auto 32px auto;
}

.tabs {
  display: flex;
  gap: 12px;
  justify-content: center;
  flex-wrap: wrap;
}

.tab-btn {
  padding: 14px 28px;
  font-size: 1rem;
  font-weight: 600;
  color: #9ca3af;
  background: rgba(255, 255, 255, 0.05);
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 8px;
}

.tab-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(102, 126, 234, 0.5);
  color: #cbd5e0;
}

.tab-btn.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-color: #667eea;
  color: white;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.tab-emoji {
  font-size: 1.25rem;
}

.tab-content {
  animation: fadeIn 0.5s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.shop-stats-card {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  padding: 24px;
  margin-bottom: 32px;
  display: flex;
  gap: 48px;
  justify-content: center;
  flex-wrap: wrap;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.stat-label {
  font-size: 0.875rem;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.shop-container {
  max-width: 1400px;
  margin: 0 auto;
}

.shop-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
  animation: fadeInUp 0.6s ease-out;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.empty-state {
  text-align: center;
  padding: 80px 20px;
  max-width: 500px;
  margin: 0 auto;
}

.empty-icon {
  font-size: 5rem;
  margin-bottom: 24px;
  animation: float 3s ease-in-out infinite;
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

.empty-title {
  font-size: 2rem;
  font-weight: 700;
  color: #f3f4f6;
  margin: 0 0 12px 0;
}

.empty-description {
  font-size: 1.125rem;
  color: #9ca3af;
  margin: 0 0 32px 0;
  line-height: 1.6;
}

.load-btn {
  padding: 14px 32px;
  font-size: 1rem;
  font-weight: 600;
  color: white;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.load-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
}

.load-btn:active {
  transform: translateY(0);
}

/* Load More Button */
.load-more-container {
  text-align: center;
  margin-top: 48px;
}

.load-more-btn {
  padding: 16px 48px;
  font-size: 1.125rem;
  font-weight: 600;
  color: white;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.load-more-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
}

.load-more-btn:active {
  transform: translateY(0);
}

@media (max-width: 768px) {
  .shop-title {
    font-size: 2.5rem;
  }

  .shop-subtitle {
    font-size: 1rem;
  }

  .shop-grid {
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: 16px;
  }

  .tab-btn {
    padding: 12px 20px;
    font-size: 0.875rem;
  }

  .shop-stats-card {
    gap: 24px;
  }

  .stat-value {
    font-size: 1.5rem;
  }
}
</style>
