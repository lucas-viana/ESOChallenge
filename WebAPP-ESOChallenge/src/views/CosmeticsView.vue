<template>
  <div class="cosmetics-view">
    <!-- Header -->
    <header class="cosmetics-header">
      <h1 class="cosmetics-title">Fortnite Cosméticos</h1>
      <p class="cosmetics-subtitle">
        Explore os cosméticos mais recentes do Fortnite
      </p>

      <!-- Filtros -->
      <div class="cosmetics-filters">
        <button
          v-for="filter in filters"
          :key="filter.value"
          :class="['filter-btn', { active: activeFilter === filter.value }]"
          @click="handleFilterChange(filter.value)"
        >
          {{ filter.label }}
        </button>
      </div>
    </header>

    <!-- Loading State -->
    <LoadingSpinner v-if="cosmeticStore.loading" message="Carregando cosméticos..." />

    <!-- Error State -->
    <ErrorMessage
      v-else-if="cosmeticStore.error"
      :message="cosmeticStore.error"
      @retry="handleRetry"
    />

    <!-- Empty State -->
    <div v-else-if="!cosmeticStore.hasCosmetics" class="empty-state">
      <div class="empty-icon">📦</div>
      <h3 class="empty-title">Nenhum cosmético encontrado</h3>
      <p class="empty-description">
        Tente carregar os dados ou verifique sua conexão.
      </p>
      <button @click="handleRetry" class="load-btn">
        Carregar Cosméticos
      </button>
    </div>

    <!-- Cosmetics Grid -->
    <div v-else class="cosmetics-container">
      <!-- Stats -->
      <div class="cosmetics-stats">
        <span class="stat-item">
          Total: <strong>{{ cosmeticStore.cosmeticsCount }}</strong> itens
        </span>
      </div>

      <!-- Grid de Cards -->
      <div class="cosmetics-grid">
        <CosmeticCard
          v-for="cosmetic in cosmeticStore.cosmetics"
          :key="cosmetic.id"
          :cosmetic="cosmetic"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useCosmeticStore } from '@/stores/cosmetic.store'
import CosmeticCard from '@/components/CosmeticCard.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ErrorMessage from '@/components/ErrorMessage.vue'

/**
 * View principal de Cosméticos
 * Componente "Smart" que orquestra a lógica e os componentes de apresentação
 * Segue o padrão Container/Presentational Components
 */

// State Management
const cosmeticStore = useCosmeticStore()

// Tipos de filtro
type FilterType = 'new' | 'all' | 'shop'

interface Filter {
  value: FilterType
  label: string
  action: () => Promise<void>
}

// Estado local
const activeFilter = ref<FilterType>('new')

const filters: Filter[] = [
  {
    value: 'new',
    label: 'Novos',
    action: () => cosmeticStore.fetchNewCosmetics(),
  },
  {
    value: 'all',
    label: 'Todos',
    action: () => cosmeticStore.fetchAllCosmetics(),
  },
  {
    value: 'shop',
    label: 'Loja',
    action: () => cosmeticStore.fetchShopCosmetics(),
  },
]

// Métodos
async function handleFilterChange(filterValue: FilterType) {
  activeFilter.value = filterValue
  const filter = filters.find((f) => f.value === filterValue)
  if (filter) {
    await filter.action()
  }
}

async function handleRetry() {
  const filter = filters.find((f) => f.value === activeFilter.value)
  if (filter) {
    await filter.action()
  }
}

// Lifecycle Hooks
onMounted(async () => {
  // Carrega cosméticos novos ao montar o componente
  await cosmeticStore.fetchNewCosmetics()
})

onUnmounted(() => {
  // Cancela requisições pendentes ao desmontar
  cosmeticStore.cancelRequest()
})
</script>

<style scoped>
.cosmetics-view {
  min-height: 100vh;
  padding: 24px;
  background: linear-gradient(135deg, #0f0f1e 0%, #1a1a2e 100%);
}

.cosmetics-header {
  text-align: center;
  margin-bottom: 48px;
}

.cosmetics-title {
  font-size: 3rem;
  font-weight: 800;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0 0 16px 0;
}

.cosmetics-subtitle {
  font-size: 1.25rem;
  color: #9ca3af;
  margin: 0 0 32px 0;
}

.cosmetics-filters {
  display: flex;
  gap: 12px;
  justify-content: center;
  flex-wrap: wrap;
}

.filter-btn {
  padding: 12px 24px;
  background: rgba(255, 255, 255, 0.05);
  color: #9ca3af;
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(255, 255, 255, 0.2);
  color: #ffffff;
}

.filter-btn.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-color: #667eea;
  color: #ffffff;
  box-shadow: 0 4px 16px rgba(102, 126, 234, 0.4);
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 96px 24px;
  text-align: center;
}

.empty-icon {
  font-size: 4rem;
}

.empty-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0;
}

.empty-description {
  font-size: 1rem;
  color: #9ca3af;
  margin: 0;
  max-width: 400px;
}

.load-btn {
  margin-top: 16px;
  padding: 14px 32px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.3s ease;
}

.load-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.4);
}

.load-btn:active {
  transform: translateY(0);
}

.cosmetics-container {
  max-width: 1400px;
  margin: 0 auto;
}

.cosmetics-stats {
  display: flex;
  justify-content: center;
  margin-bottom: 32px;
  padding: 16px;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  backdrop-filter: blur(10px);
}

.stat-item {
  font-size: 1rem;
  color: #9ca3af;
}

.stat-item strong {
  color: #ffffff;
  font-weight: 700;
}

.cosmetics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
  padding-bottom: 48px;
}

/* Responsividade */
@media (max-width: 768px) {
  .cosmetics-view {
    padding: 16px;
  }

  .cosmetics-title {
    font-size: 2rem;
  }

  .cosmetics-subtitle {
    font-size: 1rem;
  }

  .cosmetics-grid {
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: 16px;
  }

  .filter-btn {
    padding: 10px 20px;
    font-size: 0.875rem;
  }
}

@media (max-width: 480px) {
  .cosmetics-grid {
    grid-template-columns: 1fr;
  }
}
</style>
