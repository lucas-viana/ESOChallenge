<!--
  LoginView
  Tela de login seguindo Clean Architecture
-->

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { validateLoginForm } from '@/utils/validators'
import FormInput from '@/components/auth/FormInput.vue'
import AuthButton from '@/components/auth/AuthButton.vue'
import type { ValidationError } from '@/types/auth.types'

const router = useRouter()
const authStore = useAuthStore()

// Estado local do formulário
const email = ref('')
const password = ref('')
const validationErrors = ref<ValidationError[]>([])

// Computed para erros específicos de cada campo
const emailError = computed(() =>
  validationErrors.value.find(e => e.field === 'email')?.message ?? ''
)

const passwordError = computed(() =>
  validationErrors.value.find(e => e.field === 'password')?.message ?? ''
)

/**
 * Valida o formulário antes de submeter
 */
function validateForm(): boolean {
  const result = validateLoginForm(email.value, password.value)
  validationErrors.value = result.errors
  return result.isValid
}

/**
 * Submete o formulário de login
 */
async function handleSubmit() {
  // Limpar erros anteriores
  authStore.clearError()
  validationErrors.value = []

  // Validar formulário
  if (!validateForm()) {
    return
  }

  try {
    await authStore.login({
      email: email.value,
      password: password.value
    })

    // Redirecionar para página de cosméticos após login
    router.push('/shop')
  } catch (error) {
    // Erro já está no store
    console.error('Login error:', error)
  }
}

/**
 * Navega para página de registro
 */
function goToRegister() {
  router.push('/register')
}
</script>

<template>
  <div class="login-view">
    <div class="login-container">
      <div class="login-card">
        <!-- Header -->
        <div class="login-header">
          <h1 class="login-title">Bem-vindo de volta! 🎮</h1>
          <p class="login-subtitle">
            Faça login para acessar sua conta
          </p>
        </div>

        <!-- Formulário -->
        <form @submit.prevent="handleSubmit" class="login-form">
          <FormInput
            v-model="email"
            type="email"
            label="Email"
            placeholder="seu@email.com"
            :error="emailError"
            :disabled="authStore.isLoading"
            required
          />

          <FormInput
            v-model="password"
            type="password"
            label="Senha"
            placeholder="••••••••"
            :error="passwordError"
            :disabled="authStore.isLoading"
            required
          />

          <!-- Erro geral da API -->
          <div v-if="authStore.error" class="login-error" role="alert">
            <span class="login-error__icon">⚠️</span>
            <span>{{ authStore.error }}</span>
          </div>

          <!-- Botões -->
          <div class="login-actions">
            <AuthButton
              type="submit"
              variant="primary"
              :is-loading="authStore.isLoading"
              :disabled="authStore.isLoading"
              full-width
            >
              {{ authStore.isLoading ? 'Entrando...' : 'Entrar' }}
            </AuthButton>
          </div>
        </form>

        <!-- Link para registro -->
        <div class="login-footer">
          <p class="login-footer__text">
            Não tem uma conta?
            <button
              type="button"
              class="login-footer__link"
              :disabled="authStore.isLoading"
              @click="goToRegister"
            >
              Registre-se
            </button>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-view {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0f0f1e 0%, #1a1a2e 50%, #16213e 100%);
  padding: 24px;
}

.login-container {
  width: 100%;
  max-width: 440px;
}

.login-card {
  background: rgba(255, 255, 255, 0.03);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  padding: 40px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.login-header {
  text-align: center;
  margin-bottom: 32px;
}

.login-title {
  font-size: 2rem;
  font-weight: 800;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 8px;
}

.login-subtitle {
  color: #9ca3af;
  font-size: 0.875rem;
}

.login-form {
  display: flex;
  flex-direction: column;
}

.login-error {
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: 8px;
  padding: 12px 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  color: #fca5a5;
  font-size: 0.875rem;
  margin-bottom: 20px;
}

.login-error__icon {
  font-size: 1.25rem;
}

.login-actions {
  margin-top: 8px;
}

.login-footer {
  margin-top: 24px;
  text-align: center;
  padding-top: 24px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.login-footer__text {
  color: #9ca3af;
  font-size: 0.875rem;
}

.login-footer__link {
  background: none;
  border: none;
  color: #667eea;
  font-weight: 600;
  cursor: pointer;
  padding: 0;
  margin-left: 4px;
  transition: color 0.2s ease;
}

.login-footer__link:hover:not(:disabled) {
  color: #764ba2;
  text-decoration: underline;
}

.login-footer__link:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 480px) {
  .login-card {
    padding: 24px;
  }

  .login-title {
    font-size: 1.5rem;
  }
}
</style>
