<template>
  <article class="cosmetic-card-compact" @click="$emit('click', cosmetic)">
    <!-- Carrossel para Bundles -->
    <div v-if="cosmetic.isBundle && hasCarouselImages" class="card-carousel-wrapper">
      <button
        class="carousel-btn carousel-btn--prev"
        @click.stop="previousImage"
        :disabled="currentImageIndex === 0"
        aria-label="Item anterior"
      >
        ❮
      </button>

      <div class="card-image-wrapper">
        <img
          :src="carouselImages[currentImageIndex]"
          :alt="`${getCurrentBundleItemData.name} ${currentImageIndex + 1}`"
          class="card-image"
          loading="lazy"
          @error="handleImageError"
        />

        <div
          v-if="getCurrentBundleItemData.rarity"
          class="rarity-badge"
          :class="`rarity-${rarityClass}`"
        >
          {{ getCurrentBundleItemData.rarity.displayValue }}
        </div>

        <!-- Badge de Novo -->
        <div v-if="cosmetic.isNew" class="new-badge">
          ✨ NOVO
        </div>

        <!-- Indicadores -->
        <div class="carousel-indicators">
          <span
            v-for="(_, index) in carouselImages"
            :key="index"
            class="carousel-indicator"
            :class="{ active: currentImageIndex === index }"
            @click.stop="goToImage(index)"
          />
        </div>
      </div>

      <button
        class="carousel-btn carousel-btn--next"
        @click.stop="nextImage"
        :disabled="currentImageIndex === carouselImages.length - 1"
        aria-label="Próximo item"
      >
        ❯
      </button>
    </div>

    <!-- Imagem Única (não bundle) -->
    <div v-else class="card-image-wrapper">
      <img
        :src="imageUrl"
        :alt="cosmetic.name"
        class="card-image"
        loading="lazy"
        @error="handleImageError"
      />

      <div v-if="cosmetic.rarity" class="rarity-badge" :class="`rarity-${rarityClass}`">
        {{ cosmetic.rarity.displayValue }}
      </div>

      <!-- Badge de Novo -->
      <div v-if="cosmetic.isNew" class="new-badge">
        ✨ NOVO
      </div>

      <!-- Badge de Bundle -->
      <div v-if="cosmetic.isBundle && cosmetic.bundle" class="bundle-badge">
        📦 {{ cosmetic.bundle.name }}
      </div>
      <div v-else-if="cosmetic.isBundle" class="bundle-badge">
        📦 Bundle
        <span v-if="cosmetic.containedItemIds" class="bundle-count">
          ({{ cosmetic.containedItemIds.length }} itens)
        </span>
      </div>
    </div>

    <!-- Informações -->
    <div class="card-content">
      <h3 class="card-title">{{ getCurrentBundleItemData.name }}</h3>

      <div class="card-footer">
        <span class="card-price">{{ formattedPrice }}</span>

        <button
          v-if="showPurchaseButton && cosmetic.price > 0"
          class="btn-purchase"
          @click.stop="handlePurchase"
          :disabled="isPurchasing"
        >
          {{ isPurchasing ? '...' : '🛒' }}
        </button>

        <button
          v-if="showRefundButton"
          class="btn-refund"
          @click.stop="handleRefund"
          :disabled="isRefunding"
        >
          {{ isRefunding ? '...' : '💰' }}
        </button>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Cosmetic } from '@/types/cosmetic.types'

interface Props {
  cosmetic: Cosmetic
  showPurchaseButton?: boolean
  showRefundButton?: boolean
}

interface Emits {
  (e: 'click', cosmetic: Cosmetic): void
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
const fallbackImage = 'https://via.placeholder.com/150?text=No+Image'

// Carousel computados
const hasCarouselImages = computed(() => {
  return (
    cosmetic.value.isBundle &&
    cosmetic.value.containedItemsImages &&
    cosmetic.value.containedItemsImages.length > 0
  )
})

const carouselImages = computed(() => {
  return cosmetic.value.containedItemsImages || []
})

// Obter dados do item atual do bundle
const getCurrentBundleItemData = computed(() => {
  if (
    !cosmetic.value.isBundle ||
    !cosmetic.value.containedItems ||
    cosmetic.value.containedItems.length === 0
  ) {
    return {
      name: cosmetic.value.name,
      type: cosmetic.value.type,
      rarity: cosmetic.value.rarity,
    }
  }

  const currentItem = cosmetic.value.containedItems[currentImageIndex.value]
  return {
    name: currentItem?.name || cosmetic.value.name,
    type: currentItem?.type || cosmetic.value.type,
    rarity: currentItem?.rarity || cosmetic.value.rarity,
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
  return cosmetic.value.images?.icon || cosmetic.value.images?.featured || fallbackImage
})

const rarityClass = computed(() => {
  // eslint-disable-next-line unicorn/prefer-string-replace-all
  return (
    getCurrentBundleItemData.value.rarity?.value?.toLowerCase().replace(/\s+/g, '-') || 'common'
  )
})

const formattedPrice = computed(() => {
  if (!cosmetic.value.price || cosmetic.value.price === 0) {
    return 'Grátis'
  }
  return `${cosmetic.value.price.toLocaleString('pt-BR')} V-Bucks`
})

const cosmetic = computed(() => props.cosmetic)

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  target.src = fallbackImage
}

const handlePurchase = async () => {
  if (isPurchasing.value) return

  isPurchasing.value = true
  try {
    emit('purchased', cosmetic.value.id)
  } finally {
    setTimeout(() => {
      isPurchasing.value = false
    }, 1000)
  }
}

const handleRefund = async () => {
  if (isRefunding.value) return

  isRefunding.value = true
  try {
    emit('refund', cosmetic.value.id)
  } finally {
    setTimeout(() => {
      isRefunding.value = false
    }, 1000)
  }
}
</script>

<style scoped>
.cosmetic-card-compact {
  background: rgba(26, 26, 46, 0.8);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
}

.cosmetic-card-compact:hover {
  transform: translateY(-4px);
  border-color: rgba(102, 126, 234, 0.5);
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.2);
}

/* Wrapper do Carrossel */
.card-carousel-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.card-image-wrapper {
  position: relative;
  width: 100%;
  aspect-ratio: 1;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
  overflow: hidden;
}

/* Botões do Carrossel */
.carousel-btn {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(0, 0, 0, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  cursor: pointer;
  font-size: 0.875rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  z-index: 3;
}

.carousel-btn:hover:not(:disabled) {
  background: rgba(102, 126, 234, 0.8);
  border-color: rgba(255, 255, 255, 0.4);
  transform: translateY(-50%) scale(1.1);
}

.carousel-btn:disabled {
  opacity: 0.2;
  cursor: not-allowed;
}

.carousel-btn--prev {
  left: 4px;
}

.carousel-btn--next {
  right: 4px;
}

/* Indicadores do Carrossel */
.carousel-indicators {
  position: absolute;
  bottom: 8px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  gap: 4px;
  z-index: 2;
}

.carousel-indicator {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.4);
  border: 1px solid rgba(255, 255, 255, 0.6);
  cursor: pointer;
  transition: all 0.2s ease;
}

.carousel-indicator:hover {
  background: rgba(255, 255, 255, 0.7);
}

.carousel-indicator.active {
  background: rgba(102, 126, 234, 1);
  border-color: white;
  width: 14px;
  border-radius: 3px;
}

.card-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.cosmetic-card-compact:hover .card-image {
  transform: scale(1.05);
}

.rarity-badge {
  position: absolute;
  top: 8px;
  right: 8px;
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 0.65rem;
  font-weight: 700;
  text-transform: uppercase;
  color: #fff;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(4px);
  z-index: 2;
}

.new-badge {
  position: absolute;
  top: 8px;
  left: 8px;
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 0.65rem;
  font-weight: 800;
  text-transform: uppercase;
  color: #ffffff;
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  box-shadow: 0 2px 8px rgba(245, 158, 11, 0.5);
  backdrop-filter: blur(4px);
  z-index: 3;
  animation: newBadgePulse 2s ease-in-out infinite;
}

@keyframes newBadgePulse {
  0%,
  100% {
    transform: scale(1);
    box-shadow: 0 2px 8px rgba(245, 158, 11, 0.5);
  }
  50% {
    transform: scale(1.05);
    box-shadow: 0 4px 12px rgba(245, 158, 11, 0.7);
  }
}

.bundle-badge {
  position: absolute;
  top: 8px;
  left: 8px;
  background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
  color: white;
  padding: 6px 10px;
  border-radius: 6px;
  font-size: 0.7rem;
  font-weight: 700;
  z-index: 2;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
  display: flex;
  align-items: center;
  gap: 4px;
}

.bundle-count {
  font-size: 0.65rem;
  opacity: 0.9;
}

/* Cores de Raridade */
.rarity-common {
  border-left: 3px solid #9e9e9e;
}
.rarity-uncommon {
  border-left: 3px solid #4caf50;
}
.rarity-rare {
  border-left: 3px solid #2196f3;
}
.rarity-epic {
  border-left: 3px solid #9c27b0;
}
.rarity-legendary {
  border-left: 3px solid #ff9800;
}
.rarity-marvel {
  border-left: 3px solid #e53935;
}
.rarity-dc {
  border-left: 3px solid #1e88e5;
}
.rarity-icon {
  border-left: 3px solid #00bcd4;
}
.rarity-starwars {
  border-left: 3px solid #212121;
}
.rarity-gaminglegends {
  border-left: 3px solid #00e676;
}

.card-content {
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
}

.card-title {
  font-size: 0.875rem;
  font-weight: 700;
  color: #fff;
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  margin-top: auto;
}

.card-price {
  font-size: 0.75rem;
  font-weight: 600;
  color: #ffd700;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.btn-purchase {
  width: 32px;
  height: 32px;
  padding: 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-purchase:hover:not(:disabled) {
  transform: scale(1.1);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.btn-purchase:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-refund {
  width: 32px;
  height: 32px;
  padding: 0;
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-refund:hover:not(:disabled) {
  transform: scale(1.1);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.4);
}

.btn-refund:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .card-title {
    font-size: 0.75rem;
  }

  .card-price {
    font-size: 0.7rem;
  }

  .btn-purchase {
    width: 28px;
    height: 28px;
    font-size: 0.875rem;
  }
}
</style>
