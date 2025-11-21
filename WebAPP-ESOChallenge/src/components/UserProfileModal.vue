<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen" class="modal-overlay" @click.self="closeModal">
        <div class="modal-container">
          <div class="modal-header">
            <h2 class="modal-title">👤 Perfil do Usuário</h2>
            <button class="modal-close" @click="closeModal" aria-label="Fechar">×</button>
          </div>

          <div class="modal-body">
            <!-- Loading State -->
            <div v-if="loading" class="loading-state">
              <div class="spinner"></div>
              <p>Carregando perfil...</p>
            </div>

            <!-- Error State -->
            <div v-else-if="error" class="error-state">
              <p>{{ error }}</p>
            </div>

            <!-- Profile Content -->
            <div v-else-if="userProfile" class="profile-content">
              <!-- User Info -->
              <div class="profile-header">
                <div class="profile-avatar">
                  <span class="avatar-icon">👤</span>
                </div>
                <div class="profile-info">
                  <h3 class="profile-name">{{ getUserDisplayName() }}</h3>
                  <p class="profile-email">{{ userProfile.email }}</p>
                  <div class="profile-balance">
                    <span class="balance-icon">💰</span>
                    <span class="balance-amount">{{ userProfile.vBucks.toLocaleString() }}</span>
                    <span class="balance-label">V-Bucks</span>
                  </div>
                </div>
              </div>

              <!-- Cosmetics Section -->
              <div class="cosmetics-section">
                <h4 class="section-title">
                  ✨ Cosméticos ({{ userProfile.cosmetics.length }})
                </h4>

                <div v-if="userProfile.cosmetics.length === 0" class="empty-cosmetics">
                  <p>Este usuário ainda não possui cosméticos</p>
                </div>

                <div v-else class="cosmetics-grid">
                  <CosmeticCardCompact
                    v-for="cosmetic in convertedCosmetics"
                    :key="cosmetic.id"
                    :cosmetic="cosmetic"
                    :show-purchase-button="false"
                    :show-refund-button="false"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import userService from '@/services/userService'
import CosmeticCardCompact from '@/components/CosmeticCardCompact.vue'
import type { UserProfileDetails, UserCosmetic } from '@/types/user'
import type { Cosmetic } from '@/types/cosmetic.types'

interface Props {
  isOpen: boolean
  userId: string | null
}

const props = defineProps<Props>()
const emit = defineEmits<{
  close: []
}>()

const userProfile = ref<UserProfileDetails | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

// Agrupa itens de bundle em um único Cosmetic com carrossel
const convertedCosmetics = computed<Cosmetic[]>(() => {
  if (!userProfile.value) return []

  const cosmetics = userProfile.value.cosmetics
  
  // Identificar bundles pais (tipo "Pacote" ou "bundle")
  const bundleParents = new Map<string, UserCosmetic>()
  for (const uc of cosmetics) {
    if (uc.type.toLowerCase() === 'pacote' || uc.type.toLowerCase() === 'bundle') {
      bundleParents.set(uc.cosmeticId, uc)
    }
  }

  // Agrupa por bundleId
  const bundleGroups: { [key: string]: UserCosmetic[] } = {}
  const singles: UserCosmetic[] = []
  
  for (const uc of cosmetics) {
    // Pular bundles pais (não devem aparecer como cards individuais)
    if (bundleParents.has(uc.cosmeticId)) {
      continue
    }
    
    // Se tem bundleId, agrupar
    if ('bundleId' in uc && uc.bundleId) {
      const bid = uc.bundleId as string
      if (!bundleGroups[bid]) bundleGroups[bid] = []
      bundleGroups[bid].push(uc)
    } else {
      singles.push(uc)
    }
  }

  // Converte bundles agrupados
  const bundleCosmetics: Cosmetic[] = Object.entries(bundleGroups).map(([bundleId, bundleItems]) => {
    // Buscar informações do bundle pai
    const bundleParent = bundleParents.get(bundleId)
    const first = bundleItems[0]!
    
    // Usar dados do bundle pai se existir, senão usar do primeiro item
    const bundleName = bundleParent?.name || first.name
    const bundlePrice = bundleParent?.price || bundleItems.reduce((sum, item) => sum + item.price, 0)
    const bundleImage = bundleParent?.imageUrl || first.imageUrl
    
    return {
      id: bundleId,
      name: bundleName,
      description: bundleParent?.description || first.description || '',
      type: {
        value: 'bundle',
        displayValue: 'Pacote'
      },
      rarity: {
        value: bundleParent?.rarity.toLowerCase() || first.rarity.toLowerCase(),
        displayValue: bundleParent?.rarity || first.rarity
      },
      series: undefined,
      images: {
        icon: bundleImage || '',
        smallIcon: undefined,
        featured: undefined
      },
      added: bundleParent?.purchasedAt || first.purchasedAt,
      price: bundlePrice,
      isInShop: false,
      isNew: false,
      isBundle: true,
      containedItemIds: bundleItems.map(i => i.cosmeticId),
      containedItemsImages: bundleItems.map(i => i.imageUrl || ''),
      containedItems: bundleItems.map(i => ({
        id: i.cosmeticId,
        name: i.name,
        type: {
          value: i.type.toLowerCase(),
          displayValue: i.type
        },
        rarity: {
          value: i.rarity.toLowerCase(),
          displayValue: i.rarity
        },
        image: i.imageUrl || ''
      })),
      bundle: bundleParent ? {
        name: bundleParent.name,
        info: bundleParent.description || '',
        image: bundleParent.imageUrl || ''
      } : undefined
    }
  })

  // Converte itens avulsos
  const singleCosmetics: Cosmetic[] = singles.map(uc => ({
    id: uc.cosmeticId,
    name: uc.name,
    description: uc.description || '',
    type: {
      value: uc.type.toLowerCase(),
      displayValue: uc.type
    },
    rarity: {
      value: uc.rarity.toLowerCase(),
      displayValue: uc.rarity
    },
    series: undefined,
    images: {
      icon: uc.imageUrl || '',
      smallIcon: undefined,
      featured: undefined
    },
    added: uc.purchasedAt,
    price: uc.price,
    isInShop: false,
    isNew: false,
    isBundle: false,
    containedItemIds: undefined,
    containedItemsImages: undefined,
    containedItems: undefined,
    bundle: undefined
  }))

  return [...bundleCosmetics, ...singleCosmetics]
})

const fetchUserProfile = async () => {
  if (!props.userId) return

  try {
    loading.value = true
    error.value = null
    userProfile.value = await userService.getUserProfile(props.userId)
  } catch (err) {
    console.error('Error fetching user profile:', err)
    error.value = 'Erro ao carregar perfil do usuário'
  } finally {
    loading.value = false
  }
}

const getUserDisplayName = (): string => {
  if (!userProfile.value) return ''

  const { firstName, lastName, email } = userProfile.value

  if (firstName && lastName) {
    return `${firstName} ${lastName}`
  }
  if (firstName) {
    return firstName
  }
  return email?.split('@')[0] || 'Usuário'
}

const closeModal = () => {
  emit('close')
  // Reset state after animation
  setTimeout(() => {
    userProfile.value = null
    error.value = null
  }, 300)
}

// Watch for modal open and userId changes
watch(
  () => [props.isOpen, props.userId],
  ([newIsOpen, newUserId]) => {
    if (newIsOpen && newUserId) {
      fetchUserProfile()
    }
  },
  { immediate: true }
)
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.75);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal-container {
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
  max-width: 900px;
  width: 100%;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.modal-header {
  padding: 1.5rem 2rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0;
}

.modal-close {
  background: none;
  border: none;
  color: rgba(255, 255, 255, 0.6);
  font-size: 2rem;
  line-height: 1;
  cursor: pointer;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
  border-radius: 8px;
}

.modal-close:hover {
  background: rgba(255, 255, 255, 0.18);
  color: #aaaaaa;
}

.modal-body {
  padding: 2rem;
  overflow-y: auto;
  flex: 1;
}

/* Loading, Error States */
.loading-state,
.error-state {
  text-align: center;
  padding: 3rem 1rem;
  color: rgba(255, 255, 255, 0.7);
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

/* Profile Header */
.profile-header {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  margin-bottom: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.profile-avatar {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.avatar-icon {
  font-size: 3.5rem;
}

.profile-info {
  flex: 1;
}

.profile-name {
  font-size: 1.75rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0 0 0.5rem 0;
}

.profile-email {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.6);
  margin: 0 0 1rem 0;
}

.profile-balance {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-weight: 700;
  color: #1a1a2e;
}

.balance-icon {
  font-size: 1.25rem;
}

.balance-amount {
  font-size: 1.125rem;
}

.balance-label {
  font-size: 0.875rem;
}

/* Cosmetics Section */
.cosmetics-section {
  margin-top: 1.5rem;
}

.section-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0 0 1.5rem 0;
}

.empty-cosmetics {
  text-align: center;
  padding: 3rem 1rem;
  color: rgba(255, 255, 255, 0.5);
}

.cosmetics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
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

.modal-enter-active .modal-container,
.modal-leave-active .modal-container {
  transition: transform 0.3s ease;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: scale(0.9);
}

/* Responsive */
@media (max-width: 768px) {
  .modal-container {
    max-height: 95vh;
  }

  .modal-body {
    padding: 1.5rem;
  }

  .profile-header {
    flex-direction: column;
    text-align: center;
  }

  .profile-avatar {
    width: 80px;
    height: 80px;
  }

  .avatar-icon {
    font-size: 2.5rem;
  }

  .cosmetics-grid {
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  }
}
</style>
