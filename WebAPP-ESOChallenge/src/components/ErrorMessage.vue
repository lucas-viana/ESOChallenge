<template>
  <div class="error-message">
    <div class="error-icon">⚠️</div>
    <h3 class="error-title">{{ title }}</h3>
    <p class="error-description">{{ message }}</p>
    <button v-if="showRetry" @click="handleRetry" class="error-retry-btn">
      Tentar Novamente
    </button>
  </div>
</template>

<script setup lang="ts">
/**
 * Componente de Erro
 * Exibe mensagens de erro de forma amigável ao usuário
 */

interface Props {
  title?: string
  message: string
  showRetry?: boolean
}

withDefaults(defineProps<Props>(), {
  title: 'Ops! Algo deu errado',
  showRetry: true,
})

const emit = defineEmits<{
  retry: []
}>()

function handleRetry() {
  emit('retry')
}
</script>

<style scoped>
.error-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 48px;
  text-align: center;
}

.error-icon {
  font-size: 4rem;
}

.error-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #ef4444;
  margin: 0;
}

.error-description {
  font-size: 1rem;
  color: #9ca3af;
  margin: 0;
  max-width: 400px;
}

.error-retry-btn {
  margin-top: 16px;
  padding: 12px 24px;
  background: #3b82f6;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s ease, transform 0.2s ease;
}

.error-retry-btn:hover {
  background: #2563eb;
  transform: translateY(-2px);
}

.error-retry-btn:active {
  transform: translateY(0);
}
</style>
