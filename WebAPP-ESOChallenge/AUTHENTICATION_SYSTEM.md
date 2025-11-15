# 🔐 Sistema de Autenticação - Documentação Técnica

## 📋 Visão Geral

Sistema completo de autenticação implementado seguindo **Clean Architecture** e princípios **SOLID**.

---

## 🏗️ Arquitetura

### **Camadas da Aplicação**

```
┌─────────────────────────────────────────────┐
│            Presentation Layer               │
│  (Views: LoginView, RegisterView)           │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│         State Management Layer              │
│        (Stores: auth.store.ts)              │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│          Business Logic Layer               │
│      (Services: auth.service.ts)            │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│        Infrastructure Layer                 │
│  (HTTP Client + Storage + Validators)       │
└─────────────────────────────────────────────┘
```

---

## 📁 Estrutura de Arquivos

```
src/
├── types/
│   └── auth.types.ts              # Interfaces TypeScript
├── utils/
│   ├── validators.ts              # Validações (SRP)
│   └── storage.ts                 # Abstração localStorage (DIP)
├── services/
│   ├── auth.service.ts            # Lógica de autenticação
│   ├── httpClient.service.ts      # Cliente HTTP com interceptor
│   └── interceptors/
│       └── auth.interceptor.ts    # Adiciona JWT automaticamente
├── stores/
│   └── auth.store.ts              # State management (Pinia)
├── components/
│   └── auth/
│       ├── FormInput.vue          # Input reutilizável
│       └── AuthButton.vue         # Botão reutilizável
├── views/
│   ├── LoginView.vue              # Tela de login
│   └── RegisterView.vue           # Tela de registro
└── router/
    └── index.ts                   # Rotas + Navigation Guards
```

---

## 🎯 Princípios SOLID Aplicados

### **1. Single Responsibility Principle (SRP)**

Cada classe/módulo tem uma única responsabilidade:

- ✅ `validators.ts`: Apenas validações
- ✅ `storage.ts`: Apenas operações de storage
- ✅ `auth.service.ts`: Apenas lógica de autenticação
- ✅ `auth.store.ts`: Apenas gerenciamento de estado
- ✅ `FormInput.vue`: Apenas renderização de input
- ✅ `AuthButton.vue`: Apenas renderização de botão

### **2. Open/Closed Principle (OCP)**

Classes abertas para extensão, fechadas para modificação:

```typescript
// ✅ Pode adicionar novos validadores sem modificar existentes
export const validateLoginForm = (email, password) => { ... }
export const validateRegisterForm = (email, password, confirmPassword) => { ... }
// Fácil adicionar validateChangePasswordForm sem quebrar nada
```

### **3. Liskov Substitution Principle (LSP)**

Interfaces podem ser substituídas por implementações:

```typescript
// Interface
interface IStorage {
  getItem(key: string): string | null
  setItem(key: string, value: string): void
  removeItem(key: string): void
}

// Implementação atual: localStorage
class LocalStorage implements IStorage { ... }

// Fácil trocar por sessionStorage ou IndexedDB
class SessionStorage implements IStorage { ... }
```

### **4. Interface Segregation Principle (ISP)**

Interfaces específicas ao invés de genéricas:

```typescript
interface IAuthService {
  login(credentials: LoginRequest): Promise<User>
  register(userData: RegisterRequest): Promise<User>
  logout(): void
  getCurrentUser(): User | null
  isAuthenticated(): boolean
  getToken(): string | null
}
```

### **5. Dependency Inversion Principle (DIP)**

Depender de abstrações, não de implementações:

```typescript
// ❌ ERRADO: Depender diretamente de localStorage
const token = localStorage.getItem('token')

// ✅ CORRETO: Depender de abstração IStorage
const token = storage.getItem(STORAGE_KEYS.AUTH_TOKEN)
```

---

## 🔄 Fluxo de Autenticação

### **Login Flow**

```
1. Usuário preenche LoginView
   ↓
2. Validação client-side (validators.ts)
   ↓
3. authStore.login() chamado
   ↓
4. authService.login() faz POST /api/auth/login
   ↓
5. httpClient adiciona headers (interceptor)
   ↓
6. Backend retorna { token, email, expiresAt }
   ↓
7. authService salva no storage
   ↓
8. authStore atualiza estado reativo
   ↓
9. Router redireciona para /cosmetics
   ↓
10. Navigation Guard permite acesso
```

### **Registro Flow**

```
1. Usuário preenche RegisterView
   ↓
2. Validação client-side (senha forte, emails coincidem)
   ↓
3. authStore.register() chamado
   ↓
4. authService.register() faz POST /api/auth/register
   ↓
5. Backend cria usuário e retorna token
   ↓
6. authService salva no storage
   ↓
7. authStore atualiza estado
   ↓
8. Router redireciona para /cosmetics
```

### **Logout Flow**

```
1. Usuário clica em "Sair" (App.vue)
   ↓
2. authStore.logout() chamado
   ↓
3. authService.logout() limpa storage
   ↓
4. authStore.user = null
   ↓
5. Router redireciona para /login (se rota protegida)
```

---

## 🛡️ Proteção de Rotas (Navigation Guards)

### **Configuração**

```typescript
{
  path: '/cosmetics',
  name: 'cosmetics',
  component: CosmeticsView,
  meta: {
    requiresAuth: true,  // ← Requer autenticação
    title: 'Cosméticos'
  }
}
```

### **Lógica do Guard**

```typescript
router.beforeEach((to, from, next) => {
  const requiresAuth = to.meta.requiresAuth
  const isAuthenticated = authStore.isAuthenticated

  if (requiresAuth && !isAuthenticated) {
    // Redirecionar para login
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else {
    next() // Permitir acesso
  }
})
```

---

## 🔑 HTTP Interceptor (JWT Automático)

### **Como Funciona**

```typescript
// auth.interceptor.ts
export function addAuthToken(config: FetchConfig): FetchConfig {
  const token = authService.getToken()

  if (token) {
    const headers = new Headers(config.headers)
    headers.set('Authorization', `Bearer ${token}`)
    return { ...config, headers }
  }

  return config
}
```

### **Uso no httpClient**

```typescript
// ANTES: Sem interceptor
const response = await fetch(url, { headers: { 'Content-Type': 'application/json' } })

// DEPOIS: Com interceptor
const config = addAuthToken({ headers: { 'Content-Type': 'application/json' } })
const response = await fetch(url, config)
// Agora tem: Authorization: Bearer eyJhbGciOiJ...
```

---

## ✅ Validações

### **Email**

```typescript
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
// ✅ valido@email.com
// ❌ invalido@
// ❌ @invalido.com
```

### **Senha Forte**

```typescript
// Critérios:
- Mínimo 6 caracteres
- Pelo menos 1 maiúscula (A-Z)
- Pelo menos 1 minúscula (a-z)
- Pelo menos 1 número (0-9)

// ✅ Senha123
// ❌ senha (falta maiúscula e número)
// ❌ 123456 (falta letras)
```

### **Confirmação de Senha**

```typescript
if (password !== confirmPassword) {
  errors.push({ field: 'confirmPassword', message: 'As senhas não coincidem' })
}
```

---

## 💾 Armazenamento (Storage)

### **Dados Salvos**

```typescript
// localStorage
{
  "auth_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user_data": "{\"email\":\"user@example.com\",\"token\":\"...\",\"expiresAt\":\"2025-11-10T...\"}"
}
```

### **Verificação de Expiração**

```typescript
const user = JSON.parse(localStorage.getItem('user_data'))

if (new Date(user.expiresAt) < new Date()) {
  // Token expirado → Logout automático
  authService.logout()
}
```

---

## 🎨 Componentes Reutilizáveis

### **FormInput**

```vue
<FormInput
  v-model="email"
  type="email"
  label="Email"
  placeholder="seu@email.com"
  :error="emailError"
  :disabled="isLoading"
  required
/>
```

**Props:**
- `modelValue`: Valor atual
- `type`: text | email | password
- `label`: Rótulo do campo
- `placeholder`: Texto de dica
- `error`: Mensagem de erro
- `required`: Campo obrigatório
- `disabled`: Desabilitar input

### **AuthButton**

```vue
<AuthButton
  type="submit"
  variant="primary"
  :is-loading="isLoading"
  full-width
>
  Entrar
</AuthButton>
```

**Props:**
- `type`: submit | button
- `variant`: primary | secondary
- `isLoading`: Mostra spinner
- `disabled`: Desabilita botão
- `fullWidth`: Largura total

---

## 🚀 Próximos Passos (Melhorias Futuras)

### **1. Refresh Token**
- Implementar refresh automático quando token expira
- Evitar logout forçado

### **2. Remember Me**
- Opção "Lembrar de mim" no login
- Usar localStorage vs sessionStorage

### **3. OAuth/Social Login**
- Login com Google
- Login com GitHub

### **4. Two-Factor Authentication (2FA)**
- Código via email
- Google Authenticator

### **5. Recuperação de Senha**
- Endpoint "Esqueci minha senha"
- Email com token de reset

---

## 📊 Testes (A Implementar)

### **Unit Tests**

```typescript
// validators.spec.ts
describe('validateLoginForm', () => {
  it('deve retornar erro se email inválido', () => {
    const result = validateLoginForm('invalido', '123456')
    expect(result.isValid).toBe(false)
    expect(result.errors[0].field).toBe('email')
  })
})

// auth.service.spec.ts
describe('AuthService', () => {
  it('deve salvar token no storage após login', async () => {
    await authService.login({ email: 'test@test.com', password: '123456' })
    expect(storage.getItem(STORAGE_KEYS.AUTH_TOKEN)).toBeTruthy()
  })
})
```

### **Integration Tests**

```typescript
// LoginView.spec.ts
describe('LoginView', () => {
  it('deve redirecionar para /cosmetics após login', async () => {
    // Mock do authStore
    // Preencher formulário
    // Clicar em "Entrar"
    // Verificar redirecionamento
  })
})
```

---

## 🔍 Debugging

### **Verificar Token no Storage**

```javascript
// Console do navegador (F12)
console.log(localStorage.getItem('auth_token'))
console.log(JSON.parse(localStorage.getItem('user_data')))
```

### **Ver Requisições HTTP**

```
DevTools → Network → Fetch/XHR
↓
Clicar em requisição → Headers
↓
Verificar: Authorization: Bearer eyJ...
```

### **Verificar Estado do Store**

```vue
<!-- Em qualquer componente -->
<template>
  <pre>{{ authStore }}</pre>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth.store'
const authStore = useAuthStore()
</script>
```

---

## 📚 Recursos Úteis

- **Vue Router Guards**: https://router.vuejs.org/guide/advanced/navigation-guards.html
- **Pinia Stores**: https://pinia.vuejs.org/core-concepts/
- **JWT Best Practices**: https://jwt.io/introduction
- **SOLID Principles**: https://www.digitalocean.com/community/conceptual-articles/s-o-l-i-d-the-first-five-principles-of-object-oriented-design

---

## ✅ Checklist de Implementação

- [x] Tipos TypeScript (auth.types.ts)
- [x] Validadores (validators.ts)
- [x] Storage abstraction (storage.ts)
- [x] Auth Service (auth.service.ts)
- [x] Auth Store (auth.store.ts)
- [x] HTTP Interceptor (auth.interceptor.ts)
- [x] FormInput Component
- [x] AuthButton Component
- [x] LoginView
- [x] RegisterView
- [x] Navigation Guards
- [x] Update App.vue (nav com logout)
- [ ] Testes unitários
- [ ] Testes de integração
- [ ] Documentação de API

---

**Autor:** Sistema implementado seguindo Clean Architecture e SOLID  
**Data:** Novembro 2025  
**Versão:** 1.0.0
