<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="isOpen" class="confirm-overlay" @click.self="handleCancel">
        <div class="confirm-modal">
          <div class="confirm-icon">{{ icon }}</div>
          <h2 class="confirm-title">{{ title }}</h2>
          <p class="confirm-message">{{ message }}</p>

          <div class="confirm-actions">
            <button class="btn-cancel" @click="handleCancel">
              {{ cancelText }}
            </button>
            <button class="btn-confirm" :class="`btn-${type}`" @click="handleConfirm">
              {{ confirmText }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
interface Props {
  isOpen: boolean
  title?: string
  message: string
  confirmText?: string
  cancelText?: string
  type?: 'danger' | 'warning' | 'info'
  icon?: string
}

const props = withDefaults(defineProps<Props>(), {
  title: 'Confirmar Ação',
  confirmText: 'Confirmar',
  cancelText: 'Cancelar',
  type: 'warning',
  icon: '⚠️',
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

const handleConfirm = () => {
  emit('confirm')
}

const handleCancel = () => {
  emit('cancel')
}
</script>

<style scoped>
.confirm-overlay {
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
  z-index: 10001;
  padding: 20px;
}

.confirm-modal {
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.98) 0%, rgba(22, 33, 62, 0.98) 100%);
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 40px;
  max-width: 450px;
  width: 100%;
  text-align: center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
  animation: modalSlideIn 0.3s ease-out;
}

@keyframes modalSlideIn {
  from {
    transform: scale(0.9) translateY(-20px);
    opacity: 0;
  }
  to {
    transform: scale(1) translateY(0);
    opacity: 1;
  }
}

.confirm-icon {
  font-size: 3.5rem;
  margin-bottom: 16px;
  animation: iconBounce 0.5s ease-out;
}

@keyframes iconBounce {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-10px);
  }
}

.confirm-title {
  color: #ffffff;
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 0 12px 0;
}

.confirm-message {
  color: #b4b4b4;
  font-size: 1rem;
  line-height: 1.5;
  margin: 0 0 32px 0;
}

.confirm-actions {
  display: flex;
  gap: 12px;
}

.btn-cancel,
.btn-confirm {
  flex: 1;
  padding: 14px 24px;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-cancel {
  background: rgba(255, 255, 255, 0.05);
  color: #b4b4b4;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.btn-cancel:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
}

.btn-confirm {
  color: #ffffff;
}

.btn-danger {
  background: linear-gradient(135deg, #f44336 0%, #c62828 100%);
}

.btn-danger:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(244, 67, 54, 0.4);
}

.btn-warning {
  background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%);
}

.btn-warning:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(255, 152, 0, 0.4);
}

.btn-info {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.btn-info:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
}

.btn-confirm:active {
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
  .confirm-modal {
    padding: 32px 24px;
  }

  .confirm-icon {
    font-size: 3rem;
  }

  .confirm-title {
    font-size: 1.25rem;
  }

  .confirm-message {
    font-size: 0.875rem;
  }

  .confirm-actions {
    flex-direction: column-reverse;
  }
}
</style>
