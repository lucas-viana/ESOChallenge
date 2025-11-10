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
