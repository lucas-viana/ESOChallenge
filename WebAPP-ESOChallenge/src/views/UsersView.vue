<template>
  <div class="users-view">
    <!-- Header -->
    <header class="users-header">
      <div class="users-header-content">
        <h1 class="users-title">👥 Perfis de Usuários</h1>
        <p class="users-subtitle">Explore os perfis e coleções de todos os jogadores</p>
      </div>
    </header>

    <!-- Main Container -->
    <div class="users-main-container">
      <div class="users-content-wrapper">
        <main class="users-content">
          <!-- Loading State -->
          <div v-if="loading" class="loading-state">
            <div class="spinner"></div>
            <p>Carregando usuários...</p>
          </div>

          <!-- Error State -->
          <div v-else-if="error" class="error-state">
            <p>{{ error }}</p>
          </div>

          <!-- Empty State -->
          <div v-else-if="users.length === 0" class="empty-state">
            <div class="empty-icon">👤</div>
            <h3 class="empty-title">Nenhum usuário cadastrado</h3>
            <p class="empty-description">Ainda não há jogadores cadastrados no sistema</p>
          </div>

          <!-- Users Grid -->
          <div v-else class="users-grid">
            <div
              v-for="user in users"
              :key="user.id"
              class="user-card"
              @click="openUserProfile(user.id)"
            >
              <div class="user-avatar">
                <span class="avatar-icon">👤</span>
              </div>
              <div class="user-info">
                <h3 class="user-name">
                  {{ getUserDisplayName(user) }}
                </h3>
                <p class="user-email">{{ user.email }}</p>
                <div class="user-stats">
                  <div class="stat">
                    <span class="stat-icon">✨</span>
                    <span class="stat-value">{{ user.totalCosmetics }}</span>
                    <span class="stat-label">cosméticos</span>
                  </div>
                  <div class="stat">
                    <span class="stat-icon">💰</span>
                    <span class="stat-value">{{ user.vBucks.toLocaleString() }}</span>
                    <span class="stat-label">V-Bucks</span>
                  </div>
                </div>
              </div>
              <div class="card-arrow">→</div>
            </div>
          </div>
        </main>
      </div>
    </div>

    <!-- User Profile Modal -->
    <UserProfileModal
      :is-open="isModalOpen"
      :user-id="selectedUserId"
      @close="closeModal"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import userService from '@/services/userService'
import UserProfileModal from '@/components/UserProfileModal.vue'
import type { UserProfile } from '@/types/user'

const users = ref<UserProfile[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const isModalOpen = ref(false)
const selectedUserId = ref<string | null>(null)

const fetchUsers = async () => {
  try {
    loading.value = true
    error.value = null
    users.value = await userService.getAllUsers()
  } catch (err) {
    console.error('Error fetching users:', err)
    error.value = 'Erro ao carregar usuários'
  } finally {
    loading.value = false
  }
}

const getUserDisplayName = (user: UserProfile): string => {
  if (user.firstName && user.lastName) {
    return `${user.firstName} ${user.lastName}`
  }
  if (user.firstName) {
    return user.firstName
  }
  return user.email?.split('@')[0] || 'Usuário'
}

const openUserProfile = (userId: string) => {
  selectedUserId.value = userId
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
  selectedUserId.value = null
}

onMounted(() => {
  fetchUsers()
})
</script>

<style scoped>
.users-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
}

/* Header */
.users-header {
  padding: 2rem 0;
  text-align: center;
}

.users-header-content {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 24px;
}

.users-title {
  font-size: 3rem;
  font-weight: 900;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0 0 16px 0;
}

.users-subtitle {
  color: #cbd5e0;
  font-size: 1.125rem;
  margin: 0;
}

/* Main Container */
.users-main-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 24px 24px;
}

.users-content-wrapper {
  background: rgba(26, 26, 46, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 40px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  min-height: calc(100vh - 280px);
}

.users-content {
  width: 100%;
}

/* Loading, Error, Empty States */
.loading-state,
.error-state,
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: rgba(255, 255, 255, 0.7);
}

.empty-icon {
  font-size: 5rem;
  margin-bottom: 1.5rem;
  opacity: 0.5;
}

.empty-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: rgba(255, 255, 255, 0.9);
  margin-bottom: 0.5rem;
}

.empty-description {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.6);
}

.spinner {
  width: 48px;
  height: 48px;
  border: 4px solid rgba(255, 255, 255, 0.1);
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.error-state p {
  color: #ef4444;
  font-weight: 600;
}

/* Users Grid */
.users-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

.user-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1.25rem;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
}

.user-card:hover {
  background: rgba(102, 126, 234, 0.1);
  border-color: rgba(102, 126, 234, 0.3);
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.2);
}

.user-avatar {
  width: 70px;
  height: 70px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.avatar-icon {
  font-size: 2.5rem;
}

.user-info {
  flex: 1;
  min-width: 0;
}

.user-name {
  font-size: 1.25rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0 0 0.25rem 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-email {
  font-size: 0.875rem;
  color: rgba(255, 255, 255, 0.6);
  margin: 0 0 0.75rem 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-stats {
  display: flex;
  gap: 1.5rem;
}

.stat {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.875rem;
}

.stat-icon {
  font-size: 1.125rem;
}

.stat-value {
  font-weight: 700;
  color: #ffffff;
}

.stat-label {
  color: rgba(255, 255, 255, 0.6);
}

.card-arrow {
  font-size: 1.5rem;
  color: rgba(255, 255, 255, 0.3);
  transition: all 0.3s ease;
}

.user-card:hover .card-arrow {
  color: #667eea;
  transform: translateX(4px);
}

/* Responsive */
@media (max-width: 768px) {
  .users-title {
    font-size: 2rem;
  }

  .users-subtitle {
    font-size: 1rem;
  }

  .users-content-wrapper {
    padding: 24px;
  }

  .users-grid {
    grid-template-columns: 1fr;
  }

  .user-card {
    padding: 1.25rem;
  }

  .user-avatar {
    width: 60px;
    height: 60px;
  }

  .avatar-icon {
    font-size: 2rem;
  }
}
</style>
