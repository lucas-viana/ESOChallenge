<template>
  <aside class="filter-sidebar">
    <!-- Busca -->
    <section class="filter-section">
      <h3 class="filter-title">🔍 Buscar</h3>
      <input
        v-model="localFilters.searchTerm"
        type="text"
        placeholder="Nome do cosmético..."
        class="search-input"
        @input="debouncedSearch"
      />
    </section>

    <!-- Filtros Rápidos -->
    <section class="filter-section">
      <h3 class="filter-title">⚡ Filtros Rápidos</h3>
      <div class="quick-filters">
        <label class="filter-checkbox">
          <input v-model="localFilters.onlyNew" type="checkbox" @change="applyFilters" />
          <span>✨ Apenas Novos</span>
        </label>
        <label v-if="!props.hideShopFilter" class="filter-checkbox">
          <input v-model="localFilters.onlyInShop" type="checkbox" @change="applyFilters" />
          <span>🔥 Disponíveis na Loja</span>
        </label>
        <label class="filter-checkbox">
          <input v-model="localFilters.excludeBundles" type="checkbox" @change="applyFilters" />
          <span>📦 Excluir Bundles</span>
        </label>
      </div>
    </section>

    <!-- Tipo -->
    <section class="filter-section">
      <h3 class="filter-title">🎨 Tipo</h3>
      <div class="filter-chips">
        <button
          v-for="(count, type) in metadata?.availableTypes"
          :key="type"
          :class="['chip', { active: isTypeSelected(type as string) }]"
          @click="toggleType(type as string)"
        >
          {{ formatTypeName(type as string) }}
          <span class="chip-count">{{ count }}</span>
        </button>
      </div>
    </section>

    <!-- Raridade -->
    <section class="filter-section">
      <h3 class="filter-title">💎 Raridade</h3>
      <div class="filter-chips">
        <button
          v-for="(count, rarity) in metadata?.availableRarities"
          :key="rarity"
          :class="['chip', `rarity-${rarity}`, { active: isRaritySelected(rarity as string) }]"
          @click="toggleRarity(rarity as string)"
        >
          {{ formatRarityName(rarity as string) }}
          <span class="chip-count">{{ count }}</span>
        </button>
      </div>
    </section>

    <!-- Preço -->
    <section class="filter-section">
      <h3 class="filter-title">💰 Preço (V-Bucks)</h3>
      <div class="price-range">
        <input
          v-model.number="localFilters.minPrice"
          type="number"
          placeholder="Mín"
          class="price-input"
          :min="0"
          :max="localFilters.maxPrice || metadata?.maxPriceAvailable"
          @blur="validatePrices"
        />
        <span class="price-separator">-</span>
        <input
          v-model.number="localFilters.maxPrice"
          type="number"
          placeholder="Máx"
          class="price-input"
          :min="localFilters.minPrice || 0"
          @blur="validatePrices"
        />
      </div>
      <small v-if="!hasValidFilters" class="error-text">
        Valores de preço inválidos
      </small>
    </section>

    <!-- Data de Inclusão -->
    <section class="filter-section">
      <h3 class="filter-title">📅 Data de Inclusão</h3>
      <div class="date-range">
        <input v-model="localFilters.addedAfter" type="date" class="date-input" />
        <span class="date-separator">até</span>
        <input v-model="localFilters.addedBefore" type="date" class="date-input" />
      </div>
    </section>

    <!-- Ordenação -->
    <section class="filter-section">
      <h3 class="filter-title">🔃 Ordenar Por</h3>
      <select v-model="localFilters.sortBy" class="sort-select" @change="applyFilters">
        <option value="name">Nome</option>
        <option value="price">Preço</option>
        <option value="rarity">Raridade</option>
        <option value="added">Data de Adição</option>
      </select>
      <div class="sort-order">
        <button
          :class="['sort-btn', { active: localFilters.sortOrder === 'asc' }]"
          @click="setSortOrder('asc')"
        >
          ⬆ Crescente
        </button>
        <button
          :class="['sort-btn', { active: localFilters.sortOrder === 'desc' }]"
          @click="setSortOrder('desc')"
        >
          ⬇ Decrescente
        </button>
      </div>
    </section>

    <!-- Ações -->
    <div class="filter-actions">
      <button
        class="btn-apply"
        :disabled="!hasValidFilters"
        @click="applyFilters"
      >
        Aplicar Filtros
      </button>
      <button class="btn-reset" @click="resetFilters">Limpar Tudo</button>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { reactive, watch, computed } from 'vue'
import type { CosmeticFilters, FilterMetadata } from '@/types/cosmetic.types'

interface Props {
  filters: CosmeticFilters
  metadata?: FilterMetadata
  hideShopFilter?: boolean // Esconde o filtro "Disponíveis na Loja" (usado em ShopView)
}

interface Emits {
  (e: 'update:filters', filters: CosmeticFilters): void
  (e: 'apply'): void
}

const props = withDefaults(defineProps<Props>(), {
  hideShopFilter: false,
})
const emit = defineEmits<Emits>()

// IMPORTANTE: Criar cópias dos arrays para manter reatividade independente
const localFilters = reactive<CosmeticFilters>({
  searchTerm: props.filters.searchTerm ?? '',
  types: props.filters.types ? [...props.filters.types] : [],
  rarities: props.filters.rarities ? [...props.filters.rarities] : [],
  addedAfter: props.filters.addedAfter,
  addedBefore: props.filters.addedBefore,
  onlyNew: props.filters.onlyNew ?? false,
  onlyInShop: props.filters.onlyInShop ?? false,
  excludeBundles: props.filters.excludeBundles ?? false,
  minPrice: props.filters.minPrice,
  maxPrice: props.filters.maxPrice,
  sortBy: props.filters.sortBy ?? 'added',
  sortOrder: props.filters.sortOrder ?? 'desc',
})

// Computed para validar se há filtros válidos antes de aplicar
const hasValidFilters = computed(() => {
  // Validar preços
  const hasValidMinPrice = localFilters.minPrice === undefined ||
    (typeof localFilters.minPrice === 'number' && localFilters.minPrice >= 0)
  const hasValidMaxPrice = localFilters.maxPrice === undefined ||
    (typeof localFilters.maxPrice === 'number' && localFilters.maxPrice >= 0)

  // Validar range de preços
  const validPriceRange = !localFilters.minPrice || !localFilters.maxPrice ||
    localFilters.minPrice <= localFilters.maxPrice

  // Validar datas
  const validDateRange = !localFilters.addedAfter || !localFilters.addedBefore ||
    new Date(localFilters.addedAfter) <= new Date(localFilters.addedBefore)

  return hasValidMinPrice && hasValidMaxPrice && validPriceRange && validDateRange
})

// Debounce para busca
let searchTimeout: ReturnType<typeof setTimeout>
const debouncedSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    applyFilters()
  }, 500)
}

// Métodos auxiliares
const isTypeSelected = (type: string) => localFilters.types?.includes(type) ?? false

const isRaritySelected = (rarity: string) => localFilters.rarities?.includes(rarity) ?? false

const toggleType = (type: string) => {
  if (!localFilters.types) localFilters.types = []
  const index = localFilters.types.indexOf(type)
  if (index > -1) {
    localFilters.types.splice(index, 1)
  } else {
    localFilters.types.push(type)
  }
  applyFilters()
}

const toggleRarity = (rarity: string) => {
  if (!localFilters.rarities) localFilters.rarities = []
  const index = localFilters.rarities.indexOf(rarity)
  if (index > -1) {
    localFilters.rarities.splice(index, 1)
  } else {
    localFilters.rarities.push(rarity)
  }
  applyFilters()
}

const setSortOrder = (order: 'asc' | 'desc') => {
  localFilters.sortOrder = order
  applyFilters()
}

const applyFilters = () => {
  // Limpar valores inválidos antes de aplicar
  validatePrices()
  emit('update:filters', { ...localFilters })
  emit('apply')
}

const validatePrices = () => {
  // Limpar valores negativos ou NaN
  if (localFilters.minPrice !== undefined && (Number.isNaN(localFilters.minPrice) || localFilters.minPrice < 0)) {
    localFilters.minPrice = undefined
  }
  if (localFilters.maxPrice !== undefined && (Number.isNaN(localFilters.maxPrice) || localFilters.maxPrice < 0)) {
    localFilters.maxPrice = undefined
  }

  // Ajustar se min > max
  if (localFilters.minPrice !== undefined &&
      localFilters.maxPrice !== undefined &&
      localFilters.minPrice > localFilters.maxPrice) {
    const temp = localFilters.minPrice
    localFilters.minPrice = localFilters.maxPrice
    localFilters.maxPrice = temp
  }
}

const resetFilters = () => {
  Object.assign(localFilters, {
    searchTerm: '',
    types: [],
    rarities: [],
    addedAfter: undefined,
    addedBefore: undefined,
    onlyNew: false,
    onlyInShop: false,
    excludeBundles: false,
    minPrice: undefined,
    maxPrice: undefined,
    sortBy: 'added',
    sortOrder: 'desc',
  })
  applyFilters()
}

// Formatadores
const formatTypeName = (type: string): string => {
  const names: Record<string, string> = {
    outfit: 'Traje',
    pickaxe: 'Picareta',
    glider: 'Planador',
    emote: 'Emote',
    backpack: 'Mochila',
    wrap: 'Adesivo',
    loadingscreen: 'Tela de Carregamento',
    music: 'Música',
    spray: 'Spray',
    contrail: 'Rastro',
    pet: 'Pet',
    banner: 'Banner',
    bundle: 'Pacote',
    shoe: 'Sapato',
    sidekick: 'Companheiro',
    emoji: 'Emoji',
    track: 'Música',
    drum: 'Bateria',
    mic: 'Microfone',
    wheel: 'Roda',
    petcarrier: 'Transportador',
    body: 'Corpo',
    skin: 'Visual',
    booster: 'Impulso',
    guitar: 'Guitarra',
    bass: 'Baixo',
    keyboard: 'Teclado',
    car: 'Veículo',
    instrument: 'Instrumento',
    legokit: 'Kit LEGO',
  }
  return names[type.toLowerCase()] || type
}

const formatRarityName = (rarity: string): string => {
  const names: Record<string, string> = {
    common: 'Comum',
    uncommon: 'Incomum',
    rare: 'Raro',
    epic: 'Épico',
    legendary: 'Lendário',
    marvel: 'Marvel',
    dc: 'DC',
    icon: 'Ícone',
    starwars: 'Star Wars',
    gaminglegends: 'Gaming Legends',
  }
  return names[rarity.toLowerCase()] || rarity
}

// Watch para sincronizar com props externas
// IMPORTANTE: Criar novos arrays para manter reatividade
watch(
  () => props.filters,
  (newFilters) => {
    localFilters.searchTerm = newFilters.searchTerm ?? ''
    localFilters.types = newFilters.types ? [...newFilters.types] : []
    localFilters.rarities = newFilters.rarities ? [...newFilters.rarities] : []
    localFilters.addedAfter = newFilters.addedAfter
    localFilters.addedBefore = newFilters.addedBefore
    localFilters.onlyNew = newFilters.onlyNew ?? false
    localFilters.onlyInShop = newFilters.onlyInShop ?? false
    localFilters.excludeBundles = newFilters.excludeBundles ?? false
    localFilters.minPrice = newFilters.minPrice
    localFilters.maxPrice = newFilters.maxPrice
    localFilters.sortBy = newFilters.sortBy ?? 'added'
    localFilters.sortOrder = newFilters.sortOrder ?? 'desc'
  },
  { deep: true },
)
</script>

<style scoped>
.filter-sidebar {
  width: 280px;
  background: rgba(26, 26, 46, 0.95);
  border-radius: 16px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  max-height: calc(100vh - 100px);
  overflow-y: auto;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
}

.filter-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.filter-title {
  font-size: 0.875rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.search-input,
.price-input,
.date-input,
.sort-select {
  width: 100%;
  padding: 10px 12px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: #ffffff;
  font-size: 0.875rem;
  transition: all 0.3s ease;
}

.search-input:focus,
.price-input:focus,
.date-input:focus,
.sort-select:focus {
  outline: none;
  border-color: #667eea;
  background: rgba(255, 255, 255, 0.08);
}

.quick-filters {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.filter-checkbox {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 8px;
  border-radius: 8px;
  transition: background 0.3s ease;
}

.filter-checkbox:hover {
  background: rgba(255, 255, 255, 0.05);
}

.filter-checkbox input[type='checkbox'] {
  cursor: pointer;
}

.filter-checkbox span {
  color: #b4b4b4;
  font-size: 0.875rem;
}

.filter-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.chip {
  padding: 6px 12px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  color: #b4b4b4;
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 4px;
}

.chip:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(255, 255, 255, 0.2);
}

.chip.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-color: #667eea;
  color: #ffffff;
}

.chip-count {
  font-size: 0.7rem;
  opacity: 0.8;
}

/* Cores de raridade */
.rarity-common.active {
  background: linear-gradient(135deg, #9e9e9e 0%, #757575 100%);
  border-color: #9e9e9e;
}
.rarity-uncommon.active {
  background: linear-gradient(135deg, #4caf50 0%, #388e3c 100%);
  border-color: #4caf50;
}
.rarity-rare.active {
  background: linear-gradient(135deg, #2196f3 0%, #1976d2 100%);
  border-color: #2196f3;
}
.rarity-epic.active {
  background: linear-gradient(135deg, #9c27b0 0%, #7b1fa2 100%);
  border-color: #9c27b0;
}
.rarity-legendary.active {
  background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%);
  border-color: #ff9800;
}
.rarity-marvel.active {
  background: linear-gradient(135deg, #e53935 0%, #c62828 100%);
  border-color: #e53935;
}
.rarity-dc.active {
  background: linear-gradient(135deg, #1e88e5 0%, #1565c0 100%);
  border-color: #1e88e5;
}
.rarity-icon.active {
  background: linear-gradient(135deg, #00bcd4 0%, #0097a7 100%);
  border-color: #00bcd4;
}
.rarity-starwars.active {
  background: linear-gradient(135deg, #212121 0%, #000000 100%);
  border-color: #212121;
}

.price-range,
.date-range {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.date-separator {
  color: #8b92b0;
  font-size: 0.75rem;
  text-align: center;
  padding: 4px 0;
}

.price-input {
  flex: 1;
}

.price-separator {
  color: #8b92b0;
  font-size: 0.75rem;
}

.sort-order {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}

.sort-btn {
  flex: 1;
  padding: 8px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: #b4b4b4;
  font-size: 0.75rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.sort-btn.active {
  background: #667eea;
  border-color: #667eea;
  color: #ffffff;
}

.filter-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: auto;
  padding-top: 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.btn-apply,
.btn-reset {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-apply {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
}

.btn-apply:disabled {
  background: rgba(102, 126, 234, 0.3);
  cursor: not-allowed;
  opacity: 0.5;
}

.btn-apply:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.btn-reset {
  background: rgba(255, 255, 255, 0.05);
  color: #b4b4b4;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.btn-reset:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
}

.error-text {
  display: block;
  margin-top: 4px;
  color: #ff6b6b;
  font-size: 0.75rem;
  font-weight: 500;
}

/* Scrollbar customizado */
.filter-sidebar::-webkit-scrollbar {
  width: 6px;
}

.filter-sidebar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 3px;
}

.filter-sidebar::-webkit-scrollbar-thumb {
  background: rgba(102, 126, 234, 0.5);
  border-radius: 3px;
}

.filter-sidebar::-webkit-scrollbar-thumb:hover {
  background: rgba(102, 126, 234, 0.7);
}

@media (max-width: 1024px) {
  .filter-sidebar {
    width: 100%;
    max-height: none;
  }
}
</style>
