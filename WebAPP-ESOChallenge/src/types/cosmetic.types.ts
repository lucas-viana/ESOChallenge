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
  isAvailable: boolean
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
