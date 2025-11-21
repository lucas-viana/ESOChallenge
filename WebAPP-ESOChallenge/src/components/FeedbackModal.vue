<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="isOpen" class="feedback-overlay" @click.self="handleClose">
        <div class="feedback-modal" :class="typeClass">
          <div class="feedback-icon">
            {{ iconEmoji }}
          </div>
          <h2 class="feedback-title">{{ title }}</h2>
          <p class="feedback-message">{{ message }}</p>

          <div v-if="details" class="feedback-details">
            <div v-for="(value, key) in details" :key="key" class="detail-item">
              <span class="detail-label">{{ key }}:</span>
              <span class="detail-value">{{ value }}</span>
            </div>
          </div>

          <button class="feedback-close-btn" @click="handleClose">
            {{ closeButtonText }}
          </button>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'

interface Props {
  isOpen: boolean
  type?: 'success' | 'error' | 'warning' | 'info'
  title?: string
  message: string
  details?: Record<string, string | number>
  closeButtonText?: string
  autoClose?: boolean
  autoCloseDuration?: number
}

interface Emits {
  (e: 'close'): void
}

const props = withDefaults(defineProps<Props>(), {
  type: 'success',
  title: '',
  closeButtonText: 'Fechar',
  autoClose: false,
  autoCloseDuration: 3000,
})

const emit = defineEmits<Emits>()

const typeClass = computed(() => `feedback-${props.type}`)

const iconEmoji = computed(() => {
  switch (props.type) {
    case 'success':
      return '✅'
    case 'error':
      return '❌'
    case 'warning':
      return '⚠️'
    case 'info':
      return 'ℹ️'
    default:
      return '✅'
  }
})

const handleClose = () => {
  emit('close')
}

// Auto-close quando modal abrir
watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen && props.autoClose) {
      setTimeout(() => {
        handleClose()
      }, props.autoCloseDuration)
    }
  },
)
</script>

<style scoped>
.feedback-overlay {
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
  z-index: 10000;
  padding: 20px;
}

.feedback-modal {
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.98) 0%, rgba(22, 33, 62, 0.98) 100%);
  border: 2px solid;
  border-radius: 20px;
  padding: 40px;
  max-width: 500px;
  width: 100%;
  text-align: center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
  animation: modalSlideIn 0.3s ease-out;
}

@keyframes modalSlideIn {
  from {
    transform: translateY(-50px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.feedback-success {
  border-color: #4caf50;
}

.feedback-error {
  border-color: #f44336;
}

.feedback-warning {
  border-color: #ff9800;
}

.feedback-info {
  border-color: #2196f3;
}

.feedback-icon {
  font-size: 4rem;
  margin-bottom: 16px;
  animation: iconPulse 0.5s ease-out;
}

@keyframes iconPulse {
  0% {
    transform: scale(0);
  }
  50% {
    transform: scale(1.2);
  }
  100% {
    transform: scale(1);
  }
}

.feedback-title {
  color: #ffffff;
  font-size: 1.75rem;
  font-weight: 700;
  margin: 0 0 12px 0;
}

.feedback-message {
  color: #b4b4b4;
  font-size: 1rem;
  line-height: 1.5;
  margin: 0 0 24px 0;
}

.feedback-details {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 24px;
  text-align: left;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.detail-item:last-child {
  border-bottom: none;
}

.detail-label {
  color: #b4b4b4;
  font-size: 0.875rem;
  font-weight: 500;
}

.detail-value {
  color: #ffffff;
  font-size: 1rem;
  font-weight: 600;
}

.feedback-close-btn {
  width: 100%;
  padding: 14px 32px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.feedback-close-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
}

.feedback-close-btn:active {
  transform: translateY(0);
}

/* Transitions */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

/* Responsive */
@media (max-width: 768px) {
  .feedback-modal {
    padding: 32px 24px;
  }

  .feedback-icon {
    font-size: 3rem;
  }

  .feedback-title {
    font-size: 1.5rem;
  }

  .feedback-message {
    font-size: 0.875rem;
  }
}
</style>
