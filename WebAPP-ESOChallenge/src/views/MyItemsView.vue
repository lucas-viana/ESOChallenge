<template>
  <div class="my-items-view">
    <div class="container">
      <header class="page-header">
        <h1 class="page-title">💎 Meus Itens</h1>
        <p class="page-subtitle">Gerencie sua coleção de cosméticos</p>
      </header>

      <LoadingSpinner v-if="purchaseStore.isLoading" />

      <ErrorMessage v-else-if="purchaseStore.error" :message="purchaseStore.error" />

      <div v-else-if="ownedCosmetics.length === 0" class="empty-state">
        <div class="empty-icon">📦</div>
        <h2>Nenhum item adquirido ainda</h2>
        <p>Visite a loja para começar sua coleção!</p>
        <router-link to="/shop" class="btn-shop"> 🛒 Ir para a Loja </router-link>
      </div>

      <div v-else class="items-grid">
        <article v-for="cosmetic in ownedCosmetics" :key="cosmetic.id" class="item-card">
          <!-- Carrossel para bundles -->
          <div
            v-if="cosmetic.isBundle && cosmetic.bundleItems?.length > 0"
            class="item-carousel-container"
          >
            <div class="item-carousel">
              <button
                class="carousel-btn carousel-btn--prev"
                @click.stop="previousBundleImage(cosmetic.id)"
                :disabled="getBundleImageIndex(cosmetic.id) === 0"
              >
                ❮
              </button>

              <div class="carousel-images">
                <img
                  :src="
                    cosmetic.bundleItems[getBundleImageIndex(cosmetic.id)]?.images?.icon ||
                    cosmetic.bundleItems[getBundleImageIndex(cosmetic.id)]?.images?.featured
                  "
                  :alt="cosmetic.bundleItems[getBundleImageIndex(cosmetic.id)]?.name"
                  class="item-image"
                  loading="lazy"
                />
              </div>

              <button
                class="carousel-btn carousel-btn--next"
                @click.stop="nextBundleImage(cosmetic.id)"
                :disabled="getBundleImageIndex(cosmetic.id) === cosmetic.bundleItems.length - 1"
              >
                ❯
              </button>
            </div>

            <!-- Indicadores do Carrossel -->
            <div class="carousel-indicators">
              <button
                v-for="(_, index) in cosmetic.bundleItems"
                :key="index"
                class="carousel-indicator"
                :class="{ active: getBundleImageIndex(cosmetic.id) === index }"
                @click.stop="setBundleImageIndex(cosmetic.id, index)"
                :aria-label="`Ver item ${index + 1}`"
              />
            </div>

            <!-- Badge de Bundle -->
            <div class="bundle-badge-overlay">📦 PACOTE</div>
          </div>

          <!-- Imagem única para itens individuais -->
          <div v-else class="item-image-container">
            <img
              :src="cosmetic.images?.icon || cosmetic.images?.featured"
              :alt="cosmetic.name"
              class="item-image"
              loading="lazy"
            />
          </div>

          <div class="item-content">
            <h3 class="item-title">
              {{ cosmetic.isBundle ? getCurrentBundleItem(cosmetic).name : cosmetic.name }}
            </h3>
            <p v-if="cosmetic.description" class="item-description">
              {{ cosmetic.description }}
            </p>

            <div class="item-meta">
              <span
                v-if="cosmetic.isBundle ? getCurrentBundleItem(cosmetic).type : cosmetic.type"
                class="item-type"
              >
                {{ cosmetic.isBundle ? getCurrentBundleItem(cosmetic).type : cosmetic.type }}
              </span>
              <span
                v-if="cosmetic.isBundle ? getCurrentBundleItem(cosmetic).rarity : cosmetic.rarity"
                class="item-rarity"
                :class="`rarity-${(cosmetic.isBundle ? getCurrentBundleItem(cosmetic).rarity : cosmetic.rarity)?.toLowerCase()}`"
              >
                {{ cosmetic.isBundle ? getCurrentBundleItem(cosmetic).rarity : cosmetic.rarity }}
              </span>
            </div>

            <div class="item-info">
              <div class="info-row">
                <span class="info-label">Preço pago:</span>
                <span class="info-value"
                  >{{ cosmetic.purchasePrice.toLocaleString() }} V-Bucks</span
                >
              </div>
              <div class="info-row">
                <span class="info-label">Adquirido em:</span>
                <span class="info-value">{{ formatDate(cosmetic.purchasedAt) }}</span>
              </div>
              <div
                v-if="cosmetic.isBundle && cosmetic.bundleItems"
                class="info-row bundle-indicator"
              >
                <span class="bundle-badge"
                  >📦 {{ cosmetic.bundleItems.length }} itens inclusos</span
                >
              </div>
            </div>

            <button
              class="btn-refund"
              :disabled="isRefunding === cosmetic.id"
              @click="handleRefund(cosmetic.id)"
            >
              <span v-if="isRefunding === cosmetic.id">Processando...</span>
              <span v-else>💰 Devolver {{ cosmetic.isBundle ? 'Bundle' : 'Item' }}</span>
            </button>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { usePurchaseStore } from '@/stores/purchase.store'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ErrorMessage from '@/components/ErrorMessage.vue'

const purchaseStore = usePurchaseStore()
const isRefunding = ref<string | null>(null)
const bundleImageIndexes = ref<Map<string, number>>(new Map())

// Agrupar itens de bundle em um único card
const ownedCosmetics = computed(() => {
  const cosmetics = purchaseStore.ownedCosmetics
  const bundleMap = new Map<string, any>()
  const individualItems: any[] = []

  for (const cosmetic of cosmetics) {
    if (cosmetic.bundleId) {
      // É um item filho de bundle
      if (!bundleMap.has(cosmetic.bundleId)) {
        // Criar representação do bundle
        bundleMap.set(cosmetic.bundleId, {
          id: cosmetic.bundleId,
          name: `Bundle (${cosmetic.bundleId.substring(0, 20)}...)`,
          description: 'Pacote com múltiplos itens',
          purchasePrice: 0,
          purchasedAt: cosmetic.purchasedAt,
          isBundle: true,
          bundleId: cosmetic.bundleId,
          bundleItems: [],
          images: cosmetic.images,
          type: 'Bundle',
          rarity: cosmetic.rarity,
        })
      }
      // Adicionar item à lista do bundle
      const bundle = bundleMap.get(cosmetic.bundleId)
      bundle.bundleItems.push(cosmetic)
      bundle.purchasePrice += cosmetic.purchasePrice
      // Usar a primeira imagem disponível
      if (!bundle.images && cosmetic.images) {
        bundle.images = cosmetic.images
      }
    } else {
      // Item individual (verificar se não é um bundle que foi comprado diretamente)
      // Só adicionar se não existir como bundle no mapa
      const isBundleInMap = Array.from(bundleMap.values()).some((b) => b.id === cosmetic.id)
      if (!isBundleInMap) {
        individualItems.push(cosmetic)
      }
    }
  }

  // Combinar bundles e itens individuais
  return [...Array.from(bundleMap.values()), ...individualItems]
})

// Obter o item atual do bundle baseado no índice do carrossel
const getCurrentBundleItem = (cosmetic: any) => {
  if (!cosmetic.isBundle || !cosmetic.bundleItems || cosmetic.bundleItems.length === 0) {
    return cosmetic
  }
  const index = getBundleImageIndex(cosmetic.id)
  return cosmetic.bundleItems[index] || cosmetic.bundleItems[0]
}

// Métodos de navegação do carrossel
const getBundleImageIndex = (bundleId: string): number => {
  return bundleImageIndexes.value.get(bundleId) || 0
}

const setBundleImageIndex = (bundleId: string, index: number) => {
  bundleImageIndexes.value.set(bundleId, index)
}

const nextBundleImage = (bundleId: string) => {
  const current = getBundleImageIndex(bundleId)
  const bundle = ownedCosmetics.value.find((c) => c.id === bundleId)
  if (bundle?.bundleItems && current < bundle.bundleItems.length - 1) {
    setBundleImageIndex(bundleId, current + 1)
  }
}

const previousBundleImage = (bundleId: string) => {
  const current = getBundleImageIndex(bundleId)
  if (current > 0) {
    setBundleImageIndex(bundleId, current - 1)
  }
}

onMounted(async () => {
  await Promise.all([purchaseStore.fetchOwnedCosmetics(), purchaseStore.fetchVBucks()])
})

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

async function handleRefund(cosmeticId: string) {
  if (!confirm('Tem certeza que deseja devolver este item? O valor será reembolsado em V-Bucks.')) {
    return
  }

  isRefunding.value = cosmeticId

  try {
    const response = await purchaseStore.refundCosmetic(cosmeticId)
    if (response.success) {
      // Atualizar a lista de itens e o saldo após reembolso bem-sucedido
      await Promise.all([purchaseStore.fetchOwnedCosmetics(), purchaseStore.fetchVBucks()])
      alert(
        `✅ ${response.message}\n\nV-Bucks reembolsados: ${response.refundedAmount}\nSaldo atual: ${response.remainingVBucks}`,
      )
    } else {
      alert(`❌ ${response.message}`)
    }
  } catch (error: any) {
    alert(`❌ Erro ao processar reembolso: ${error.message}`)
  } finally {
    isRefunding.value = null
  }
}
</script>

<style scoped>
.my-items-view {
  min-height: 100vh;
  padding: 2rem 0;
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
}

.container {
  max-width: 1400px;
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

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 16px;
  margin: 2rem 0;
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
  margin-bottom: 2rem;
}

.btn-shop {
  display: inline-block;
  padding: 12px 32px;
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
  color: white;
  text-decoration: none;
  border-radius: 8px;
  font-weight: 600;
  transition: transform 0.3s ease;
}

.btn-shop:hover {
  transform: translateY(-2px);
}

.items-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.item-card {
  background: linear-gradient(135deg, #1e1e2e 0%, #2a2a3e 100%);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.3);
  transition: transform 0.3s ease;
}

.item-card:hover {
  transform: translateY(-4px);
}

.item-image-container {
  width: 100%;
  padding-top: 100%;
  position: relative;
  background: linear-gradient(135deg, #2d2d44 0%, #3a3a52 100%);
}

.item-carousel-container {
  width: 100%;
  padding-top: 100%;
  position: relative;
  background: linear-gradient(135deg, #2d2d44 0%, #3a3a52 100%);
}

.item-carousel {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.carousel-images {
  flex: 1;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.carousel-btn {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(0, 0, 0, 0.6);
  border: none;
  color: white;
  font-size: 1.5rem;
  padding: 0.5rem 0.75rem;
  cursor: pointer;
  z-index: 10;
  border-radius: 4px;
  transition: all 0.3s ease;
}

.carousel-btn:hover:not(:disabled) {
  background: rgba(0, 0, 0, 0.8);
  transform: translateY(-50%) scale(1.1);
}

.carousel-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.carousel-btn--prev {
  left: 8px;
}

.carousel-btn--next {
  right: 8px;
}

.carousel-indicators {
  position: absolute;
  bottom: 12px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  gap: 6px;
  z-index: 10;
}

.carousel-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.5);
  border: none;
  cursor: pointer;
  transition: all 0.3s ease;
  padding: 0;
}

.carousel-indicator:hover {
  background: rgba(255, 255, 255, 0.8);
  transform: scale(1.2);
}

.carousel-indicator.active {
  background: white;
  width: 24px;
  border-radius: 4px;
}

.bundle-badge-overlay {
  position: absolute;
  top: 12px;
  left: 12px;
  background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
  color: white;
  padding: 0.4rem 0.8rem;
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  z-index: 10;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.item-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.item-content {
  padding: 1.5rem;
}

.item-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #f1f5f9;
  margin-bottom: 0.5rem;
}

.item-description {
  font-size: 0.875rem;
  color: #94a3b8;
  margin-bottom: 1rem;
  line-height: 1.5;
}

.item-meta {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.item-type,
.item-rarity {
  padding: 4px 12px;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 600;
}

.item-type {
  background: rgba(99, 102, 241, 0.2);
  color: #a5b4fc;
}

.item-rarity {
  text-transform: uppercase;
}

.rarity-common {
  background: rgba(156, 163, 175, 0.2);
  color: #d1d5db;
}
.rarity-uncommon {
  background: rgba(34, 197, 94, 0.2);
  color: #86efac;
}
.rarity-rare {
  background: rgba(59, 130, 246, 0.2);
  color: #93c5fd;
}
.rarity-epic {
  background: rgba(168, 85, 247, 0.2);
  color: #c4b5fd;
}
.rarity-legendary {
  background: rgba(251, 191, 36, 0.2);
  color: #fde68a;
}

.item-info {
  background: rgba(0, 0, 0, 0.2);
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.info-row:last-child {
  margin-bottom: 0;
}

.info-label {
  font-size: 0.875rem;
  color: #94a3b8;
}

.info-value {
  font-size: 0.875rem;
  font-weight: 600;
  color: #f1f5f9;
}

.bundle-indicator {
  margin-top: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px solid rgba(148, 163, 184, 0.2);
}

.bundle-badge {
  display: inline-block;
  padding: 4px 12px;
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: #fff;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.bundle-refund-info {
  margin-top: 1rem;
  padding: 12px;
  background: rgba(251, 191, 36, 0.1);
  border: 1px solid rgba(251, 191, 36, 0.3);
  border-radius: 8px;
}

.bundle-message {
  margin: 0;
  font-size: 0.875rem;
  color: #fbbf24;
  text-align: center;
  line-height: 1.4;
}

.btn-refund {
  width: 100%;
  padding: 12px 24px;
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-refund:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(239, 68, 68, 0.4);
}

.btn-refund:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }

  .items-grid {
    grid-template-columns: 1fr;
  }
}
</style>
