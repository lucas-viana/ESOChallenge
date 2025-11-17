<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { usePurchaseStore } from '@/stores/purchase.store'
import { onMounted } from 'vue'

const authStore = useAuthStore()
const purchaseStore = usePurchaseStore()

onMounted(async () => {
  if (authStore.isAuthenticated) {
    await purchaseStore.fetchVBucks()
  }
})

function handleLogout() {
  authStore.logout()
  purchaseStore.reset()
}
</script>

<template>
  <div id="app">
    <nav class="main-nav">
      <div class="nav-container">
        <RouterLink to="/" class="nav-logo">
          🎮 Fortnite API
        </RouterLink>
        
        <div class="nav-links">
          <RouterLink to="/">Home</RouterLink>
          <RouterLink to="/shop">🛒 Loja</RouterLink>
          
          <!-- Authenticated user links -->
          <template v-if="authStore.isAuthenticated">
            <RouterLink to="/my-items">💎 Meus Itens</RouterLink>
            <RouterLink to="/purchase-history">📜 Histórico</RouterLink>
            
            <!-- V-Bucks Balance -->
            <div class="vbucks-balance">
              <span class="vbucks-icon">💰</span>
              <span class="vbucks-amount">{{ purchaseStore.vbucks.toLocaleString() }}</span>
            </div>
            
            <span class="nav-user">{{ authStore.userEmail }}</span>
            <button @click="handleLogout" class="nav-logout">
              Sair
            </button>
          </template>
          
          <!-- Guest links -->
          <template v-else>
            <RouterLink to="/about">Sobre</RouterLink>
            <RouterLink to="/login" class="nav-auth">Entrar</RouterLink>
            <RouterLink to="/register" class="nav-auth nav-auth--primary">
              Registrar
            </RouterLink>
          </template>
        </div>
      </div>
    </nav>

    <main class="main-content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
#app {
  min-height: 100%;
  background: linear-gradient(135deg, #0f0f1e 0%, #1a1a2e 100%);
}

.main-nav {
  background: rgba(15, 15, 30, 0.95);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  position: sticky;
  top: 0;
  z-index: 100;
}

.nav-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 16px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-logo {
  font-size: 1.5rem;
  font-weight: 800;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  text-decoration: none;
  transition: transform 0.2s ease;
}

.nav-logo:hover {
  transform: scale(1.05);
}

.nav-links {
  display: flex;
  gap: 24px;
}

.nav-links a {
  color: #9ca3af;
  text-decoration: none;
  font-weight: 600;
  font-size: 1rem;
  padding: 8px 16px;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.nav-links a:hover {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.05);
}

.nav-links a.router-link-active {
  color: #667eea;
  background: rgba(102, 126, 234, 0.1);
}

.nav-user {
  color: #d1d5db;
  font-size: 0.875rem;
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
}

.vbucks-balance {
  display: flex;
  align-items: center;
  gap: 6px;
  background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
  padding: 8px 16px;
  border-radius: 8px;
  font-weight: 700;
  color: #1a1a2e;
  box-shadow: 0 2px 8px rgba(251, 191, 36, 0.3);
}

.vbucks-icon {
  font-size: 1.25rem;
}

.vbucks-amount {
  font-size: 1rem;
}

.nav-logout {
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #fca5a5;
  padding: 8px 16px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.nav-logout:hover {
  background: rgba(239, 68, 68, 0.2);
  color: #ffffff;
}

.nav-auth {
  color: #9ca3af !important;
  text-decoration: none;
  font-weight: 600;
  font-size: 1rem;
  padding: 8px 16px;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.nav-auth:hover {
  color: #ffffff !important;
  background: rgba(255, 255, 255, 0.05);
}

.nav-auth--primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff !important;
}

.nav-auth--primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(102, 126, 234, 0.3);
}

.main-content {
  min-height: calc(100vh - 70px);
}

@media (max-width: 768px) {
  .nav-container {
    flex-direction: column;
    gap: 16px;
  }

  .nav-links {
    width: 100%;
    justify-content: center;
  }

  .nav-links a {
    font-size: 0.875rem;
    padding: 6px 12px;
  }
}
</style>
