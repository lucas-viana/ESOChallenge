<template>
  <article class="cosmetic-card">
    <!-- Imagem do Cosmético -->
    <div class="cosmetic-card__image-container">
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
      <h3 class="cosmetic-card__title">{{ cosmetic.name }}</h3>
      
      <p v-if="cosmetic.description" class="cosmetic-card__description">
        {{ truncatedDescription }}
      </p>

      <div class="cosmetic-card__meta">
        <span v-if="cosmetic.type" class="cosmetic-card__type">
          {{ cosmetic.type.displayValue }}
        </span>
        
        <span class="cosmetic-card__price">
          {{ formattedPrice }}
        </span>
      </div>

      <!-- Badge de Disponibilidade -->
      <div 
        v-if="cosmetic.isAvailable" 
        class="cosmetic-card__availability"
      >
        <span class="availability-dot"></span>
        Disponível
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Cosmetic } from '@/types/cosmetic.types'

/**
 * Componente de Card para exibir um cosmético
 * Seguindo o princípio da Responsabilidade Única (SOLID)
 * Apenas apresenta dados, não contém lógica de negócio
 */

// Props (dados recebidos do componente pai)
interface Props {
  cosmetic: Cosmetic
  maxDescriptionLength?: number
}

const props = withDefaults(defineProps<Props>(), {
  maxDescriptionLength: 100,
})

// Estado local
const imageError = ref(false)
const fallbackImage = '/placeholder-cosmetic.png'

// Computados (valores derivados das props)
const imageUrl = computed(() => {
  if (imageError.value) {
    return fallbackImage
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
  return props.cosmetic.rarity?.value.toLowerCase() || 'common'
})

const formattedPrice = computed(() => {
  return `${props.cosmetic.price.toLocaleString()} V-Bucks`
})

// Métodos
function handleImageError() {
  imageError.value = true
}
</script>

<style scoped>
.cosmetic-card {
  background: linear-gradient(135deg, #1e1e2e 0%, #2a2a3e 100%);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.3);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
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
  gap: 8px;
  font-size: 0.75rem;
  color: #4ade80;
  font-weight: 600;
  padding: 8px 12px;
  background: rgba(74, 222, 128, 0.1);
  border-radius: 8px;
  border: 1px solid rgba(74, 222, 128, 0.3);
}

.availability-dot {
  width: 8px;
  height: 8px;
  background: #4ade80;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
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
