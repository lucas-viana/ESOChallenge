/**
 * Tipos e interfaces relacionados aos cosméticos do Fortnite
 * Seguindo o princípio da Single Responsibility (SOLID)
 */

export interface CosmeticType {
  value: string
  displayValue: string
}

export interface CosmeticRarity {
  value: string
  displayValue: string
}

export interface CosmeticSeries {
  value: string
  image: string
}

export interface CosmeticImages {
  smallIcon?: string
  icon?: string
  featured?: string
}

export interface Bundle {
  name: string
  info: string
  image: string
}

export interface Cosmetic {
  id: string
  name: string
  description: string
  type?: CosmeticType
  rarity?: CosmeticRarity
  series?: CosmeticSeries
  images?: CosmeticImages
  added?: string
  price: number
  isInShop: boolean
  isNew: boolean
  bundle?: Bundle
  isBundle?: boolean
  containedItemIds?: string[]
  containedItemsImages?: string[]
  containedItems?: ContainedItem[]
}

export interface ContainedItem {
  id: string
  name: string
  type?: CosmeticType
  rarity?: CosmeticRarity
  image?: string
}

export interface ApiResponse<T> {
  success: boolean
  data: T
  count?: number
  message?: string
}

export interface ErrorResponse {
  success: boolean
  message: string
  error?: string
}

// Filtros e Paginação
export interface CosmeticFilters {
  searchTerm?: string
  types?: string[]
  rarities?: string[]
  addedAfter?: string
  addedBefore?: string
  onlyNew?: boolean
  onlyInShop?: boolean
  excludeBundles?: boolean
  minPrice?: number
  maxPrice?: number
  sortBy?: 'name' | 'price' | 'rarity' | 'added'
  sortOrder?: 'asc' | 'desc'
}

export interface PaginationParams {
  page: number
  pageSize: number
}

export interface PaginationInfo {
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface FilterMetadata {
  availableTypes: Record<string, number>
  availableRarities: Record<string, number>
  minPriceAvailable: number
  maxPriceAvailable: number
}

export interface SearchResponse {
  success: boolean
  data: Cosmetic[]
  pagination: PaginationInfo
  filters: FilterMetadata
}

const bundleCosmetics: Cosmetic[] = Object.entries(bundleGroups).map(([bundleId, bundleItems]) => {
  const bundleParent = bundleItems.find(i => i.cosmeticId === bundleId)
  const base = bundleParent || bundleItems[0]
  return {
    id: bundleId,
    name: base.name,
    description: base.description || '',
    type: {
      value: base.type.toLowerCase(),
      displayValue: base.type
    },
    rarity: {
      value: base.rarity.toLowerCase(),
      displayValue: base.rarity
    },
    series: undefined,
    images: {
      icon: base.imageUrl || '',
      smallIcon: undefined,
      featured: undefined
    },
    added: base.purchasedAt,
    price: bundleParent ? bundleParent.price : 0, // <-- sempre usa o preço do pai
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
    bundle: undefined
  }
})
