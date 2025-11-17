<template>
  <div class="history-view">
    <div class="container">
      <header class="page-header">
        <h1 class="page-title">📜 Histórico de Compras</h1>
        <p class="page-subtitle">Visualize todas as suas transações</p>
      </header>

      <LoadingSpinner v-if="purchaseStore.isLoading" />
      
      <ErrorMessage v-else-if="purchaseStore.error" :message="purchaseStore.error" />

      <div v-else-if="history">
        <!-- Summary Cards -->
        <div class="summary-cards">
          <div class="summary-card">
            <div class="summary-icon">💰</div>
            <div class="summary-content">
              <span class="summary-label">Total Gasto</span>
              <span class="summary-value">{{ history.totalSpent.toLocaleString() }} V-Bucks</span>
            </div>
          </div>
          
          <div class="summary-card">
            <div class="summary-icon">🔄</div>
            <div class="summary-content">
              <span class="summary-label">Total Reembolsado</span>
              <span class="summary-value refund">{{ history.totalRefunded.toLocaleString() }} V-Bucks</span>
            </div>
          </div>
          
          <div class="summary-card">
            <div class="summary-icon">📊</div>
            <div class="summary-content">
              <span class="summary-label">Total de Transações</span>
              <span class="summary-value">{{ history.purchases.length }}</span>
            </div>
          </div>
        </div>

        <!-- Transaction List -->
        <div v-if="history.purchases.length === 0" class="empty-state">
          <div class="empty-icon">🛍️</div>
          <h2>Nenhuma transação ainda</h2>
          <p>Suas compras aparecerão aqui</p>
        </div>

        <div v-else class="transactions-list">
          <div
            v-for="purchase in history.purchases"
            :key="`${purchase.cosmeticId}-${purchase.purchasedAt}`"
            class="transaction-item"
            :class="{ 'refunded': purchase.isRefunded }"
          >
            <img
              v-if="purchase.thumbnailUrl"
              :src="purchase.thumbnailUrl"
              :alt="purchase.cosmeticName"
              class="transaction-thumbnail"
            />
            <div v-else class="transaction-thumbnail-placeholder">
              <span>?</span>
            </div>

            <div class="transaction-info">
              <h3 class="transaction-name">{{ purchase.cosmeticName }}</h3>
              <p class="transaction-date">{{ formatDate(purchase.purchasedAt) }}</p>
            </div>

            <div class="transaction-price">
              <span class="price-value">{{ purchase.price.toLocaleString() }}</span>
              <span class="price-label">V-Bucks</span>
            </div>

            <div class="transaction-status">
              <span v-if="purchase.isRefunded" class="status-badge refunded">
                ✓ Reembolsado
              </span>
              <span v-else class="status-badge active">
                Ativo
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { usePurchaseStore } from '@/stores/purchase.store'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ErrorMessage from '@/components/ErrorMessage.vue'

const purchaseStore = usePurchaseStore()
const history = computed(() => purchaseStore.purchaseHistory)

onMounted(async () => {
  await purchaseStore.fetchPurchaseHistory()
})

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
.history-view {
  min-height: 100vh;
  padding: 2rem 0;
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 2rem;
}

.page-header {
  text-align: center;
  margin-bottom: 3rem;
}

.page-title {
  font-size: 3rem;
  font-weight: 800;
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #ec4899 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 0.5rem;
}

.page-subtitle {
  font-size: 1.25rem;
  color: #94a3b8;
}

.summary-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.summary-card {
  background: linear-gradient(135deg, #1e1e2e 0%, #2a2a3e 100%);
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.summary-icon {
  font-size: 2.5rem;
}

.summary-content {
  display: flex;
  flex-direction: column;
}

.summary-label {
  font-size: 0.875rem;
  color: #94a3b8;
  margin-bottom: 0.25rem;
}

.summary-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: #6366f1;
}

.summary-value.refund {
  color: #ef4444;
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 16px;
}

.empty-icon {
  font-size: 5rem;
  margin-bottom: 1rem;
}

.empty-state h2 {
  font-size: 1.875rem;
  color: #f1f5f9;
  margin-bottom: 0.5rem;
}

.empty-state p {
  font-size: 1.125rem;
  color: #94a3b8;
}

.transactions-list {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 16px;
  overflow: hidden;
}

.transaction-item {
  display: grid;
  grid-template-columns: 60px 1fr auto auto;
  gap: 1rem;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  transition: background 0.3s ease;
}

.transaction-item:last-child {
  border-bottom: none;
}

.transaction-item:hover {
  background: rgba(255, 255, 255, 0.05);
}

.transaction-item.refunded {
  opacity: 0.7;
}

.transaction-thumbnail {
  width: 60px;
  height: 60px;
  border-radius: 8px;
  object-fit: cover;
}

.transaction-thumbnail-placeholder {
  width: 60px;
  height: 60px;
  border-radius: 8px;
  background: linear-gradient(135deg, #2d2d44 0%, #3a3a52 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  color: #94a3b8;
}

.transaction-info {
  flex: 1;
}

.transaction-name {
  font-size: 1.125rem;
  font-weight: 600;
  color: #f1f5f9;
  margin-bottom: 0.25rem;
}

.transaction-date {
  font-size: 0.875rem;
  color: #94a3b8;
}

.transaction-price {
  text-align: right;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.price-value {
  font-size: 1.25rem;
  font-weight: 700;
  color: #6366f1;
}

.price-label {
  font-size: 0.75rem;
  color: #94a3b8;
  text-transform: uppercase;
}

.transaction-status {
  min-width: 120px;
  text-align: right;
}

.status-badge {
  display: inline-block;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 600;
}

.status-badge.active {
  background: rgba(34, 197, 94, 0.2);
  color: #86efac;
}

.status-badge.refunded {
  background: rgba(239, 68, 68, 0.2);
  color: #fca5a5;
}

@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }

  .transaction-item {
    grid-template-columns: 50px 1fr;
    gap: 0.75rem;
  }

  .transaction-price,
  .transaction-status {
    grid-column: 2;
    justify-self: start;
    margin-top: 0.5rem;
  }
}
</style>
