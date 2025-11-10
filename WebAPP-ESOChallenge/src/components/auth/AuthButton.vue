<!-- 
  AuthButton Component
  Botão reutilizável para formulários de autenticação
  Segue Single Responsibility Principle
-->

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  type?: 'submit' | 'button'
  variant?: 'primary' | 'secondary'
  isLoading?: boolean
  disabled?: boolean
  fullWidth?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  type: 'button',
  variant: 'primary',
  isLoading: false,
  disabled: false,
  fullWidth: false
})

const buttonClasses = computed(() => ({
  'auth-button': true,
  [`auth-button--${props.variant}`]: true,
  'auth-button--loading': props.isLoading,
  'auth-button--full-width': props.fullWidth
}))

const isDisabled = computed(() => props.disabled || props.isLoading)
</script>

<template>
  <button
    :type="type"
    :class="buttonClasses"
    :disabled="isDisabled"
  >
    <span v-if="isLoading" class="auth-button__spinner"></span>
    <span :class="{ 'auth-button__text--hidden': isLoading }">
      <slot></slot>
    </span>
  </button>
</template>

<style scoped>
.auth-button {
  position: relative;
  padding: 12px 24px;
  font-size: 1rem;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 48px;
}

.auth-button--primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  box-shadow: 0 4px 6px rgba(102, 126, 234, 0.3);
}

.auth-button--primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(102, 126, 234, 0.4);
}

.auth-button--primary:active:not(:disabled) {
  transform: translateY(0);
}

.auth-button--secondary {
  background: rgba(255, 255, 255, 0.05);
  color: #9ca3af;
  border: 2px solid rgba(255, 255, 255, 0.1);
}

.auth-button--secondary:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.08);
  color: #ffffff;
  border-color: rgba(255, 255, 255, 0.2);
}

.auth-button--full-width {
  width: 100%;
}

.auth-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.auth-button--loading {
  cursor: wait;
}

.auth-button__spinner {
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.auth-button__text--hidden {
  visibility: hidden;
}
</style>
