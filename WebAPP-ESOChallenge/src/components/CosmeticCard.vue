<template>
  <article class="cosmetic-card">
    <!-- Carrossel de Imagens do Bundle -->
    <div v-if="cosmetic.isBundle && hasCarouselImages" class="cosmetic-card__carousel-container">
      <div class="cosmetic-carousel">
        <button
          class="carousel-btn carousel-btn--prev"
          @click.stop="previousImage"
          :disabled="currentImageIndex === 0"
        >
          ❮
        </button>

        <div class="carousel-images">
          <img
            :src="carouselImages[currentImageIndex]"
            :alt="`${cosmetic.name} - Item ${currentImageIndex + 1}`"
            class="cosmetic-card__image"
            loading="lazy"
            @error="handleImageError"
          />
        </div>

        <button
          class="carousel-btn carousel-btn--next"
          @click.stop="nextImage"
          :disabled="currentImageIndex === carouselImages.length - 1"
        >
          ❯
        </button>
      </div>

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

      <!-- Badge de Raridade -->
      <div
        v-if="cosmetic.rarity"
        class="cosmetic-card__rarity-badge"
        :class="`rarity-${rarityClass}`"
      >
        {{ cosmetic.rarity.displayValue }}
      </div>
    </div>

    <!-- Imagem Única (não bundle ou sem imagens de carrossel) -->
    <div v-else class="cosmetic-card__image-container">
      <img
        :src="imageUrl"
        :alt="cosmetic.name"
        class="cosmetic-card__image"
        loading="lazy"
        @error="handleImageError"
      />

      <!-- Badge de Raridade -->
      <div
        v-if="cosmetic.rarity"
        class="cosmetic-card__rarity-badge"
        :class="`rarity-${rarityClass}`"
      >
        {{ cosmetic.rarity.displayValue }}
      </div>
    </div>

    <!-- Informações do Cosmético -->
    <div class="cosmetic-card__content">
      <h3 class="cosmetic-card__title">{{ getCurrentBundleItemData.name }}</h3>

      <p v-if="cosmetic.description" class="cosmetic-card__description">
        {{ truncatedDescription }}
      </p>

      <!-- Informação de itens do bundle -->
      <div v-if="cosmetic.isBundle && cosmetic.containedItemIds" class="cosmetic-card__bundle-info">
        <span class="bundle-items-count">
          {{ cosmetic.containedItemIds.length }} itens inclusos
        </span>
      </div>

      <div class="cosmetic-card__meta">
        <span v-if="getCurrentBundleItemData.type" class="cosmetic-card__type">
          {{ getCurrentBundleItemData.type.displayValue }}
        </span>

        <span class="cosmetic-card__price">
          {{ formattedPrice }}
        </span>
      </div>

      <!-- Badge de Disponibilidade -->
      <div v-if="cosmetic.isAvailable" class="cosmetic-card__availability">
        <span class="availability-dot"></span>
        Disponível
      </div>

      <!-- Purchase Button -->
      <div v-if="showPurchaseButton" class="cosmetic-card__actions">
        <button
          v-if="!isOwned"
          class="btn-purchase"
          :class="{ 'btn-disabled': !canAfford || isPurchasing }"
          :disabled="!canAfford || isPurchasing"
          @click.stop="handlePurchase"
        >
          <span v-if="isPurchasing">Comprando...</span>
          <span v-else-if="!canAfford">V-Bucks Insuficientes</span>
          <span v-else>Comprar</span>
        </button>
        <div v-else class="owned-badge">
          <span class="check-icon">✓</span>
          Já Adquirido
        </div>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Cosmetic } from '@/types/cosmetic.types'
import { usePurchaseStore } from '@/stores/purchase.store'
import { useAuthStore } from '@/stores/auth.store'

/**
 * Componente de Card para exibir um cosmético
 * Seguindo o princípio da Responsabilidade Única (SOLID)
 * Apenas apresenta dados, não contém lógica de negócio
 */

// Props (dados recebidos do componente pai)
interface Props {
  cosmetic: Cosmetic
  maxDescriptionLength?: number
  showPurchaseButton?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  maxDescriptionLength: 100,
  showPurchaseButton: false,
})

// Emits
const emit = defineEmits<{
  purchased: [cosmeticId: string]
}>()

// Stores
const purchaseStore = usePurchaseStore()
const authStore = useAuthStore()

// Estado local
const imageError = ref(false)
const isPurchasing = ref(false)
const fallbackImage = '/placeholder-cosmetic.png'
const currentImageIndex = ref(0)

// Carousel computados
const hasCarouselImages = computed(() => {
  return props.cosmetic.containedItemsImages && props.cosmetic.containedItemsImages.length > 0
})

const carouselImages = computed(() => {
  return props.cosmetic.containedItemsImages || []
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

// Obter dados do item atual do bundle (para exibir nome, tipo, raridade dinâmicos)
const getCurrentBundleItemData = computed(() => {
  // Se não for bundle ou não tiver itens com dados completos, retornar dados do próprio cosmetic
  if (
    !props.cosmetic.isBundle ||
    !props.cosmetic.containedItems ||
    props.cosmetic.containedItems.length === 0
  ) {
    return {
      name: props.cosmetic.name,
      type: props.cosmetic.type,
      rarity: props.cosmetic.rarity,
    }
  }

  // Retornar dados do item atual baseado no índice do carrossel
  const currentItem = props.cosmetic.containedItems[currentImageIndex.value]
  return {
    name: currentItem?.name || props.cosmetic.name,
    type: currentItem?.type || props.cosmetic.type,
    rarity: currentItem?.rarity || props.cosmetic.rarity,
  }
})

// Computados (valores derivados das props)
const imageUrl = computed(() => {
  if (imageError.value) {
    return fallbackImage
  }

  // Se for um bundle e tiver uma imagem específica, use-a
  if (props.cosmetic.bundle?.image) {
    return props.cosmetic.bundle.image
  }

  return (
    props.cosmetic.images?.icon ||
    props.cosmetic.images?.smallIcon ||
    props.cosmetic.images?.featured ||
    fallbackImage
  )
})

const truncatedDescription = computed(() => {
  const desc = props.cosmetic.description
  if (!desc || desc.length <= props.maxDescriptionLength) {
    return desc
  }
  return `${desc.substring(0, props.maxDescriptionLength)}...`
})

const rarityClass = computed(() => {
  return getCurrentBundleItemData.value.rarity?.value.toLowerCase() || 'common'
})

const formattedPrice = computed(() => {
  return `${props.cosmetic.price.toLocaleString()} V-Bucks`
})

const isOwned = computed(() => {
  if (!authStore.isAuthenticated || !props.showPurchaseButton) return false
  return purchaseStore.ownsCosmetic(props.cosmetic.id)
})

const canAfford = computed(() => {
  if (!authStore.isAuthenticated) return false
  return purchaseStore.canAfford(props.cosmetic.price)
})

// Métodos
function handleImageError() {
  imageError.value = true
}

async function handlePurchase() {
  if (!authStore.isAuthenticated || isOwned.value || !canAfford.value) return

  isPurchasing.value = true

  try {
    const response = await purchaseStore.purchaseCosmetic(props.cosmetic.id)

    if (response.success) {
      emit('purchased', props.cosmetic.id)
      // Show success notification (you can use a toast library)
      alert(response.message)
    } else {
      alert(response.message)
    }
  } catch (error: any) {
    alert(error.message || 'Erro ao realizar compra')
  } finally {
    isPurchasing.value = false
  }
}
</script>

<style scoped>
.cosmetic-card {
  background: linear-gradient(135deg, #1e1e2e 0%, #2a2a3e 100%);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.3);
  transition:
    transform 0.3s ease,
    box-shadow 0.3s ease;
  cursor: pointer;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.cosmetic-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.4);
}

.cosmetic-card__image-container {
  position: relative;
  width: 100%;
  padding-top: 100%; /* Aspect ratio 1:1 */
  background: linear-gradient(135deg, #2d2d44 0%, #3a3a52 100%);
  overflow: hidden;
}

/* Carousel Container */
.cosmetic-card__carousel-container {
  position: relative;
  width: 100%;
  padding-top: 100%; /* Aspect ratio 1:1 */
  background: linear-gradient(135deg, #2d2d44 0%, #3a3a52 100%);
  overflow: hidden;
}

.cosmetic-carousel {
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

.cosmetic-card__image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.cosmetic-card:hover .cosmetic-card__image {
  transform: scale(1.1);
}

.cosmetic-card__rarity-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  backdrop-filter: blur(8px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.cosmetic-card__bundle-badge {
  position: absolute;
  top: 12px;
  left: 12px;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.4);
  backdrop-filter: blur(8px);
}

.cosmetic-card__bundle-info {
  padding: 8px 12px;
  background: rgba(99, 102, 241, 0.15);
  border-radius: 6px;
  border: 1px solid rgba(99, 102, 241, 0.3);
}

.bundle-items-count {
  font-size: 0.8rem;
  color: #a5b4fc;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 6px;
}

/* Cores por raridade */
.rarity-common {
  background: rgba(158, 158, 158, 0.9);
  color: #fff;
}

.rarity-uncommon {
  background: rgba(94, 152, 76, 0.9);
  color: #fff;
}

.rarity-rare {
  background: rgba(73, 172, 242, 0.9);
  color: #fff;
}

.rarity-epic {
  background: rgba(177, 91, 226, 0.9);
  color: #fff;
}

.rarity-legendary {
  background: rgba(211, 120, 65, 0.9);
  color: #fff;
}

.rarity-marvel,
.rarity-dc,
.rarity-icon,
.rarity-starwars {
  background: rgba(230, 126, 34, 0.9);
  color: #fff;
}

.cosmetic-card__content {
  padding: 20px;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.cosmetic-card__title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0;
  line-height: 1.3;
}

.cosmetic-card__description {
  font-size: 0.875rem;
  color: #b4b4b4;
  margin: 0;
  line-height: 1.5;
  flex: 1;
}

.cosmetic-card__meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.cosmetic-card__type {
  font-size: 0.875rem;
  color: #8b8b9e;
  font-weight: 500;
}

.cosmetic-card__price {
  font-size: 1rem;
  font-weight: 700;
  color: #ffd700;
  display: flex;
  align-items: center;
  gap: 4px;
}

.cosmetic-card__availability {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  font-size: 0.75rem;
  color: #4ade80;
  font-weight: 600;
  padding: 8px 12px;
  background: rgba(74, 222, 128, 0.1);
  border-radius: 8px;
  border: 1px solid rgba(74, 222, 128, 0.3);
}

.availability-dot {
  width: 10px;
  height: 10px;
  background: #4ade80;
  border-radius: 50%;
  animation: pulse 2s infinite;
  flex-shrink: 0;
}

@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

/* Purchase Actions */
.cosmetic-card__actions {
  margin-top: 12px;
}

.btn-purchase {
  width: 100%;
  padding: 12px 24px;
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
}

.btn-purchase:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(99, 102, 241, 0.4);
}

.btn-purchase:active:not(:disabled) {
  transform: translateY(0);
}

.btn-purchase.btn-disabled {
  background: linear-gradient(135deg, #4b5563 0%, #6b7280 100%);
  cursor: not-allowed;
  opacity: 0.6;
  box-shadow: none;
}

.owned-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 24px;
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.875rem;
}

.check-icon {
  font-size: 1.25rem;
  font-weight: bold;
}

/* Responsividade */
@media (max-width: 768px) {
  .cosmetic-card__content {
    padding: 16px;
  }

  .cosmetic-card__title {
    font-size: 1.125rem;
  }
}
</style>
