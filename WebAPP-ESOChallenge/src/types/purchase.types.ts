export interface PurchaseRequest {
  cosmeticId: string
}

export interface PurchaseResponse {
  success: boolean
  message: string
  remainingVBucks: number
  purchasedCosmetic?: PurchasedCosmetic
}

export interface RefundRequest {
  cosmeticId: string
}

export interface RefundResponse {
  success: boolean
  message: string
  refundedAmount: number
  remainingVBucks: number
}

export interface PurchasedCosmetic {
  id: string
  name: string
  description?: string
  purchasePrice: number
  purchasedAt: string
  isRefunded: boolean
  refundedAt?: string
  bundleId?: string
  images?: {
    icon?: string
    featured?: string
  }
  type?: string
  rarity?: string
}

export interface PurchaseHistoryItem {
  cosmeticId: string
  cosmeticName: string
  price: number
  purchasedAt: string
  isRefunded: boolean
  refundedAt?: string
  thumbnailUrl?: string
}

export interface PurchaseHistoryResponse {
  purchases: PurchaseHistoryItem[]
  totalSpent: number
  totalRefunded: number
}

export interface UserProfile {
  userId: string
  username?: string
  email?: string
  ownedCosmetics: PurchasedCosmetic[]
  totalItems: number
}

export interface VBucksResponse {
  vbucks: number
}
