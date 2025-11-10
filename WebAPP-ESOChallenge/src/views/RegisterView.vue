<!-- 
  RegisterView
  Tela de registro seguindo Clean Architecture
-->

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { validateRegisterForm } from '@/utils/validators'
import FormInput from '@/components/auth/FormInput.vue'
import AuthButton from '@/components/auth/AuthButton.vue'
import type { ValidationError } from '@/types/auth.types'

const router = useRouter()
const authStore = useAuthStore()

// Estado local do formulário
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const validationErrors = ref<ValidationError[]>([])

// Computed para erros específicos de cada campo
const emailError = computed(() => 
  validationErrors.value.find(e => e.field === 'email')?.message ?? ''
)

const passwordError = computed(() => 
  validationErrors.value.find(e => e.field === 'password')?.message ?? ''
)

const confirmPasswordError = computed(() => 
  validationErrors.value.find(e => e.field === 'confirmPassword')?.message ?? ''
)

/**
 * Valida o formulário antes de submeter
 */
function validateForm(): boolean {
  const result = validateRegisterForm(email.value, password.value, confirmPassword.value)
  validationErrors.value = result.errors
  return result.isValid
}

/**
 * Submete o formulário de registro
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
    await authStore.register({
      email: email.value,
      password: password.value,
      confirmPassword: confirmPassword.value
    })

    // Redirecionar para página de cosméticos após registro
    router.push('/cosmetics')
  } catch (error) {
    // Erro já está no store
    console.error('Register error:', error)
  }
}

/**
 * Navega para página de login
 */
function goToLogin() {
  router.push('/login')
}
</script>

<template>
  <div class="register-view">
    <div class="register-container">
      <div class="register-card">
        <!-- Header -->
        <div class="register-header">
          <h1 class="register-title">Criar Conta 🚀</h1>
          <p class="register-subtitle">
            Registre-se para acessar todos os recursos
          </p>
        </div>

        <!-- Formulário -->
        <form @submit.prevent="handleSubmit" class="register-form">
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

          <FormInput
            v-model="confirmPassword"
            type="password"
            label="Confirmar Senha"
            placeholder="••••••••"
            :error="confirmPasswordError"
            :disabled="authStore.isLoading"
            required
          />

          <!-- Requisitos de senha -->
          <div class="password-requirements">
            <p class="password-requirements__title">A senha deve conter:</p>
            <ul class="password-requirements__list">
              <li>Mínimo 6 caracteres</li>
              <li>Pelo menos 1 letra maiúscula</li>
              <li>Pelo menos 1 letra minúscula</li>
              <li>Pelo menos 1 número</li>
            </ul>
          </div>

          <!-- Erro geral da API -->
          <div v-if="authStore.error" class="register-error" role="alert">
            <span class="register-error__icon">⚠️</span>
            <span>{{ authStore.error }}</span>
          </div>

          <!-- Botões -->
          <div class="register-actions">
            <AuthButton
              type="submit"
              variant="primary"
              :is-loading="authStore.isLoading"
              :disabled="authStore.isLoading"
              full-width
            >
              {{ authStore.isLoading ? 'Criando conta...' : 'Criar Conta' }}
            </AuthButton>
          </div>
        </form>

        <!-- Link para login -->
        <div class="register-footer">
          <p class="register-footer__text">
            Já tem uma conta?
            <button
              type="button"
              class="register-footer__link"
              :disabled="authStore.isLoading"
              @click="goToLogin"
            >
              Faça login
            </button>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.register-view {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0f0f1e 0%, #1a1a2e 50%, #16213e 100%);
  padding: 24px;
}

.register-container {
  width: 100%;
  max-width: 440px;
}

.register-card {
  background: rgba(255, 255, 255, 0.03);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  padding: 40px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.register-header {
  text-align: center;
  margin-bottom: 32px;
}

.register-title {
  font-size: 2rem;
  font-weight: 800;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 8px;
}

.register-subtitle {
  color: #9ca3af;
  font-size: 0.875rem;
}

.register-form {
  display: flex;
  flex-direction: column;
}

.password-requirements {
  background: rgba(102, 126, 234, 0.05);
  border: 1px solid rgba(102, 126, 234, 0.2);
  border-radius: 8px;
  padding: 12px 16px;
  margin-bottom: 20px;
}

.password-requirements__title {
  font-size: 0.75rem;
  font-weight: 600;
  color: #9ca3af;
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.password-requirements__list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.password-requirements__list li {
  font-size: 0.875rem;
  color: #d1d5db;
  padding-left: 20px;
  position: relative;
}

.password-requirements__list li::before {
  content: '✓';
  position: absolute;
  left: 0;
  color: #667eea;
  font-weight: bold;
}

.register-error {
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

.register-error__icon {
  font-size: 1.25rem;
}

.register-actions {
  margin-top: 8px;
}

.register-footer {
  margin-top: 24px;
  text-align: center;
  padding-top: 24px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.register-footer__text {
  color: #9ca3af;
  font-size: 0.875rem;
}

.register-footer__link {
  background: none;
  border: none;
  color: #667eea;
  font-weight: 600;
  cursor: pointer;
  padding: 0;
  margin-left: 4px;
  transition: color 0.2s ease;
}

.register-footer__link:hover:not(:disabled) {
  color: #764ba2;
  text-decoration: underline;
}

.register-footer__link:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 480px) {
  .register-card {
    padding: 24px;
  }

  .register-title {
    font-size: 1.5rem;
  }
}
</style>
