<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen && cosmetic" class="modal-overlay" @click="closeModal">
        <div class="modal-container" @click.stop>
          <button class="modal-close" @click="closeModal">✕</button>

          <div class="modal-content">
            <!-- Imagem Grande com Carrossel para Bundles -->
            <div class="modal-image-section">
              <!-- Carrossel para Bundles -->
              <div v-if="cosmetic.isBundle && hasCarouselImages" class="modal-carousel">
                <button
                  class="carousel-btn carousel-btn--prev"
                  @click.stop="previousImage"
                  :disabled="currentImageIndex === 0"
                  aria-label="Item anterior"
                >
                  ❮
                </button>

                <img
                  :src="carouselImages[currentImageIndex]"
                  :alt="`${getCurrentBundleItemData.name} ${currentImageIndex + 1}`"
                  class="modal-image"
                  @error="handleImageError"
                />
                <button
                  class="carousel-btn carousel-btn--next"
                  @click.stop="nextImage"
                  :disabled="currentImageIndex === carouselImages.length - 1"
                  aria-label="Próximo item"
                >
                  ❯
                </button>

                <!-- Indicadores do Carrossel -->
                <div class="carousel-indicators">
                  <button
                    v-for="(_, index) in carouselImages"
                    :key="index"
                    class="carousel-indicator"
                    :class="{ active: currentImageIndex === index }"
                    @click.stop="goToImage(index)"
                    :aria-label="`Ver item ${index + 1}`"
                  />
                </div>
              </div>

              <!-- Imagem Única (não bundle) -->
              <img
                v-else
                :src="imageUrl"
                :alt="cosmetic.name"
                class="modal-image"
                @error="handleImageError"
              />

              <div
                v-if="getCurrentBundleItemData.rarity"
                class="modal-rarity"
                :class="`rarity-${rarityClass}`"
              >
                {{ getCurrentBundleItemData.rarity.displayValue }}
              </div>
            </div>

            <!-- Detalhes -->
            <div class="modal-details">
              <h2 class="modal-title">{{ getCurrentBundleItemData.name }}</h2>

              <p v-if="cosmetic.description" class="modal-description">
                {{ cosmetic.description }}
              </p>

              <div class="modal-info-grid">
                <div class="info-item">
                  <span class="info-label">Tipo</span>
                  <span class="info-value">{{
                    getCurrentBundleItemData.type?.displayValue || 'N/A'
                  }}</span>
                </div>

                <div class="info-item">
                  <span class="info-label">Raridade</span>
                  <span class="info-value">{{
                    getCurrentBundleItemData.rarity?.displayValue || 'N/A'
                  }}</span>
                </div>

                <div v-if="cosmetic.series" class="info-item">
                  <span class="info-label">Série</span>
                  <span class="info-value">{{ cosmetic.series.value }}</span>
                </div>

                <div class="info-item">
                  <span class="info-label">Adicionado</span>
                  <span class="info-value">{{ formattedDate }}</span>
                </div>

                <div v-if="cosmetic.isBundle" class="info-item">
                  <span class="info-label">Bundle</span>
                  <span class="info-value">{{ cosmetic.containedItemIds?.length || 0 }} itens</span>
                </div>

                <div class="info-item">
                  <span class="info-label">Disponível</span>
                  <span class="info-value">{{ cosmetic.isInShop ? 'Sim' : 'Não' }}</span>
                </div>
              </div>

              <!-- Preço e Compra/Devolução -->
              <div class="modal-footer">
                <div class="modal-price">{{ formattedPrice }}</div>

                <button
                  v-if="showPurchaseButton && cosmetic.price > 0"
                  class="modal-btn-purchase"
                  @click="handlePurchase"
                  :disabled="isPurchasing"
                >
                  {{ isPurchasing ? 'Comprando...' : '🛒 Comprar Agora' }}
                </button>

                <button
                  v-if="showRefundButton"
                  class="modal-btn-refund"
                  @click="handleRefund"
                  :disabled="isRefunding"
                >
                  {{ isRefunding ? 'Processando...' : '💰 Devolver Item' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Cosmetic } from '@/types/cosmetic.types'

interface Props {
  cosmetic: Cosmetic | null
  isOpen: boolean
  showPurchaseButton?: boolean
  showRefundButton?: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'purchased', cosmeticId: string): void
  (e: 'refund', cosmeticId: string): void
}

const props = withDefaults(defineProps<Props>(), {
  showPurchaseButton: false,
  showRefundButton: false,
})

const emit = defineEmits<Emits>()

// Estado local
const isPurchasing = ref(false)
const isRefunding = ref(false)
const currentImageIndex = ref(0)
const fallbackImage = 'https://via.placeholder.com/400?text=No+Image'

// Carousel computados
const hasCarouselImages = computed(() => {
  return (
    props.cosmetic?.isBundle &&
    props.cosmetic?.containedItemsImages &&
    props.cosmetic.containedItemsImages.length > 0
  )
})

const carouselImages = computed(() => {
  return props.cosmetic?.containedItemsImages || []
})

// Obter dados do item atual do bundle
const getCurrentBundleItemData = computed(() => {
  if (
    !props.cosmetic?.isBundle ||
    !props.cosmetic?.containedItems ||
    props.cosmetic.containedItems.length === 0
  ) {
    return {
      name: props.cosmetic?.name || '',
      type: props.cosmetic?.type,
      rarity: props.cosmetic?.rarity,
    }
  }

  const currentItem = props.cosmetic.containedItems[currentImageIndex.value]
  return {
    name: currentItem?.name || props.cosmetic.name,
    type: currentItem?.type || props.cosmetic.type,
    rarity: currentItem?.rarity || props.cosmetic.rarity,
  }
})

// Carousel métodos
const nextImage = () => {
  if (currentImageIndex.value < carouselImages.value.length - 1) {
    currentImageIndex.value++
  }
}

const previousImage = () => {
  if (currentImageIndex.value > 0) {
    currentImageIndex.value--
  }
}

const goToImage = (index: number) => {
  currentImageIndex.value = index
}

const imageUrl = computed(() => {
  if (!props.cosmetic) return fallbackImage
  return props.cosmetic.images?.featured || props.cosmetic.images?.icon || fallbackImage
})

const rarityClass = computed(() => {
  // eslint-disable-next-line unicorn/prefer-string-replace-all
  return props.cosmetic?.rarity?.value?.toLowerCase().replace(/\s+/g, '-') || 'common'
})

const formattedPrice = computed(() => {
  if (!props.cosmetic?.price || props.cosmetic.price === 0) {
    return 'Grátis'
  }
  return `${props.cosmetic.price.toLocaleString('pt-BR')} V-Bucks`
})

const formattedDate = computed(() => {
  if (!props.cosmetic?.added) return 'N/A'
  return new Date(props.cosmetic.added).toLocaleDateString('pt-BR')
})

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  target.src = fallbackImage
}

const closeModal = () => {
  emit('close')
}

const handlePurchase = async () => {
  if (isPurchasing.value || !props.cosmetic) return

  isPurchasing.value = true
  try {
    emit('purchased', props.cosmetic.id)
    setTimeout(() => {
      emit('close')
    }, 1500)
  } finally {
    setTimeout(() => {
      isPurchasing.value = false
    }, 2000)
  }
}

const handleRefund = async () => {
  if (isRefunding.value || !props.cosmetic) return

  isRefunding.value = true
  try {
    emit('refund', props.cosmetic.id)
    setTimeout(() => {
      emit('close')
    }, 1500)
  } finally {
    setTimeout(() => {
      isRefunding.value = false
    }, 2000)
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
}

.modal-container {
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.98) 0%, rgba(22, 33, 62, 0.98) 100%);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  max-width: 900px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
}

.modal-close {
  position: absolute;
  top: 16px;
  right: 16px;
  width: 40px;
  height: 40px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  color: #fff;
  font-size: 1.5rem;
  cursor: pointer;
  transition: all 0.3s ease;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-close:hover {
  background: rgba(255, 59, 59, 0.8);
  transform: rotate(90deg);
}

.modal-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 32px;
  padding: 32px;
}

.modal-image-section {
  position: relative;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
  border-radius: 16px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Carrossel do Modal */
.modal-carousel {
  position: relative;
  width: 100%;
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
  border: 2px solid rgba(255, 255, 255, 0.3);
  color: white;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  cursor: pointer;
  font-size: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
  z-index: 5;
}

.carousel-btn:hover:not(:disabled) {
  background: rgba(102, 126, 234, 0.9);
  border-color: rgba(255, 255, 255, 0.5);
  transform: translateY(-50%) scale(1.1);
}

.carousel-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.carousel-btn--prev {
  left: 16px;
}

.carousel-btn--next {
  right: 16px;
}

.carousel-indicators {
  position: absolute;
  bottom: 16px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  gap: 8px;
  z-index: 5;
}

.carousel-indicator {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.4);
  border: 1px solid rgba(255, 255, 255, 0.6);
  cursor: pointer;
  transition: all 0.3s ease;
  padding: 0;
}

.carousel-indicator:hover {
  background: rgba(255, 255, 255, 0.7);
  transform: scale(1.2);
}

.carousel-indicator.active {
  background: rgba(102, 126, 234, 1);
  border-color: white;
  width: 24px;
  border-radius: 5px;
}

.modal-image {
  width: 100%;
  height: auto;
  object-fit: contain;
  max-height: 500px;
}

.modal-rarity {
  position: absolute;
  top: 16px;
  left: 16px;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 700;
  text-transform: uppercase;
  color: #fff;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(8px);
}

.rarity-common {
  background: linear-gradient(135deg, #9e9e9e 0%, #757575 100%);
}
.rarity-uncommon {
  background: linear-gradient(135deg, #4caf50 0%, #388e3c 100%);
}
.rarity-rare {
  background: linear-gradient(135deg, #2196f3 0%, #1976d2 100%);
}
.rarity-epic {
  background: linear-gradient(135deg, #9c27b0 0%, #7b1fa2 100%);
}
.rarity-legendary {
  background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%);
}
.rarity-marvel {
  background: linear-gradient(135deg, #e53935 0%, #c62828 100%);
}
.rarity-dc {
  background: linear-gradient(135deg, #1e88e5 0%, #1565c0 100%);
}
.rarity-icon {
  background: linear-gradient(135deg, #00bcd4 0%, #0097a7 100%);
}
.rarity-starwars {
  background: linear-gradient(135deg, #212121 0%, #000000 100%);
}

.modal-details {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.modal-title {
  font-size: 2rem;
  font-weight: 800;
  color: #fff;
  margin: 0;
  line-height: 1.2;
}

.modal-description {
  color: #b4b4b4;
  font-size: 1rem;
  line-height: 1.6;
  margin: 0;
}

.modal-info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 12px;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.info-label {
  font-size: 0.75rem;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-weight: 600;
}

.info-value {
  font-size: 0.875rem;
  color: #fff;
  font-weight: 600;
}

.modal-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding-top: 20px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  margin-top: auto;
}

.modal-price {
  font-size: 1.5rem;
  font-weight: 800;
  color: #ffd700;
}

.modal-btn-purchase {
  padding: 14px 28px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  border-radius: 12px;
  color: #fff;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.modal-btn-purchase:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6);
}

.modal-btn-purchase:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.modal-btn-refund {
  padding: 14px 28px;
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  border: none;
  border-radius: 12px;
  color: #fff;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(239, 68, 68, 0.4);
}

.modal-btn-refund:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(239, 68, 68, 0.6);
}

.modal-btn-refund:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Transitions */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: scale(0.9);
}

/* Responsive */
@media (max-width: 768px) {
  .modal-content {
    grid-template-columns: 1fr;
    gap: 24px;
    padding: 20px;
  }

  .modal-title {
    font-size: 1.5rem;
  }

  .modal-info-grid {
    grid-template-columns: 1fr;
  }

  .modal-footer {
    flex-direction: column;
    align-items: stretch;
  }

  .modal-btn-purchase {
    width: 100%;
  }
}
</style>
