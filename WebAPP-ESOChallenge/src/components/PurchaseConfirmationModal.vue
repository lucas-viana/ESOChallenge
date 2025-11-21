<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen && cosmetic" class="confirmation-overlay" @click="handleCancel">
        <div class="confirmation-container" @click.stop>
          <div class="confirmation-content">
            <!-- Ícone de Confirmação -->
            <div class="confirmation-icon">🛒</div>

            <!-- Título -->
            <h2 class="confirmation-title">Confirmar Compra</h2>

            <!-- Informações do Item -->
            <div class="item-preview">
              <img
                :src="imageUrl"
                :alt="cosmetic.name"
                class="item-image"
                @error="handleImageError"
              />
              <div class="item-info">
                <h3 class="item-name">{{ cosmetic.name }}</h3>
                <div v-if="cosmetic.rarity" class="item-rarity" :class="`rarity-${rarityClass}`">
                  {{ cosmetic.rarity.displayValue }}
                </div>
              </div>
            </div>

            <!-- Detalhes da Compra -->
            <div class="purchase-details">
              <div class="detail-row">
                <span class="detail-label">Preço:</span>
                <span class="detail-value price">{{ formattedPrice }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">V-Bucks Disponíveis:</span>
                <span class="detail-value">{{ currentVBucks.toLocaleString('pt-BR') }}</span>
              </div>
              <div class="detail-row total">
                <span class="detail-label">Saldo Após Compra:</span>
                <span class="detail-value" :class="{ negative: remainingVBucks < 0 }">
                  {{ remainingVBucks.toLocaleString('pt-BR') }} V-Bucks
                </span>
              </div>
            </div>

            <!-- Aviso de saldo insuficiente -->
            <div v-if="!canAfford" class="warning-message">
              ⚠️ V-Bucks insuficientes para esta compra
            </div>

            <!-- Botões de Ação -->
            <div class="confirmation-actions">
              <button class="btn-cancel" @click="handleCancel" :disabled="isPurchasing">
                Cancelar
              </button>
              <button
                class="btn-confirm"
                @click="handleConfirm"
                :disabled="!canAfford || isPurchasing"
              >
                {{ isPurchasing ? 'Processando...' : 'Confirmar Compra' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Cosmetic } from '@/types/cosmetic.types'

interface Props {
  cosmetic: Cosmetic | null
  isOpen: boolean
  currentVBucks: number
  isPurchasing?: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'confirm', cosmeticId: string): void
}

const props = withDefaults(defineProps<Props>(), {
  isPurchasing: false,
})

const emit = defineEmits<Emits>()

const fallbackImage = 'https://via.placeholder.com/120?text=No+Image'

const imageUrl = computed(() => {
  if (!props.cosmetic) return fallbackImage
  return props.cosmetic.images?.icon || props.cosmetic.images?.featured || fallbackImage
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

const remainingVBucks = computed(() => {
  return props.currentVBucks - (props.cosmetic?.price || 0)
})

const canAfford = computed(() => {
  return remainingVBucks.value >= 0
})

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  target.src = fallbackImage
}

const handleCancel = () => {
  if (!props.isPurchasing) {
    emit('close')
  }
}

const handleConfirm = () => {
  if (props.cosmetic && canAfford.value && !props.isPurchasing) {
    emit('confirm', props.cosmetic.id)
  }
}
</script>

<style scoped>
.confirmation-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.85);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
}

.confirmation-container {
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.98) 0%, rgba(18, 18, 35, 0.98) 100%);
  border: 1px solid rgba(102, 126, 234, 0.3);
  border-radius: 20px;
  max-width: 500px;
  width: 100%;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
  animation: slideUp 0.3s ease;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.confirmation-content {
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  align-items: center;
}

.confirmation-icon {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.2) 0%, rgba(118, 75, 162, 0.2) 100%);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 3rem;
}

.confirmation-title {
  margin: 0;
  font-size: 1.75rem;
  font-weight: 700;
  color: #ffffff;
  text-align: center;
}

.item-preview {
  display: flex;
  gap: 16px;
  align-items: center;
  padding: 16px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 12px;
  width: 100%;
}

.item-image {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 8px;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
}

.item-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.item-name {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: #ffffff;
}

.item-rarity {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  width: fit-content;
}

/* Rarities */
.rarity-common {
  background: rgba(150, 150, 150, 0.2);
  color: #b4b4b4;
  border: 1px solid rgba(150, 150, 150, 0.3);
}
.rarity-uncommon {
  background: rgba(76, 175, 80, 0.2);
  color: #81c784;
  border: 1px solid rgba(76, 175, 80, 0.3);
}
.rarity-rare {
  background: rgba(33, 150, 243, 0.2);
  color: #64b5f6;
  border: 1px solid rgba(33, 150, 243, 0.3);
}
.rarity-epic {
  background: rgba(156, 39, 176, 0.2);
  color: #ba68c8;
  border: 1px solid rgba(156, 39, 176, 0.3);
}
.rarity-legendary {
  background: rgba(255, 152, 0, 0.2);
  color: #ffb74d;
  border: 1px solid rgba(255, 152, 0, 0.3);
}
.rarity-mythic {
  background: rgba(255, 235, 59, 0.2);
  color: #fff176;
  border: 1px solid rgba(255, 235, 59, 0.3);
}

.purchase-details {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px;
  background: rgba(0, 0, 0, 0.2);
  border-radius: 12px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.detail-row.total {
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  margin-top: 4px;
}

.detail-label {
  color: #8b92b0;
  font-size: 0.9rem;
}

.detail-value {
  color: #ffffff;
  font-weight: 600;
  font-size: 0.95rem;
}

.detail-value.price {
  color: #ffd700;
  font-size: 1.125rem;
}

.detail-value.negative {
  color: #ff5252;
}

.warning-message {
  width: 100%;
  padding: 12px;
  background: rgba(255, 82, 82, 0.1);
  border: 1px solid rgba(255, 82, 82, 0.3);
  border-radius: 8px;
  color: #ff8a80;
  font-size: 0.9rem;
  text-align: center;
}

.confirmation-actions {
  display: flex;
  gap: 12px;
  width: 100%;
}

.btn-cancel,
.btn-confirm {
  flex: 1;
  padding: 14px 24px;
  border-radius: 12px;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  border: none;
}

.btn-cancel {
  background: rgba(255, 255, 255, 0.05);
  color: #b4b4b4;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.btn-cancel:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.08);
  border-color: rgba(255, 255, 255, 0.2);
}

.btn-confirm {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.btn-confirm:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
}

.btn-confirm:disabled,
.btn-cancel:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Modal Transitions */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

/* Responsive */
@media (max-width: 768px) {
  .confirmation-content {
    padding: 24px;
  }

  .confirmation-title {
    font-size: 1.5rem;
  }

  .item-preview {
    flex-direction: column;
    text-align: center;
  }

  .confirmation-actions {
    flex-direction: column;
  }
}
</style>
