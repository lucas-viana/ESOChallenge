export interface UserProfile {
  id: string
  email: string
  firstName?: string
  lastName?: string
  vBucks: number
  totalCosmetics: number
}

export interface UserProfileDetails {
  id: string
  email: string
  firstName?: string
  lastName?: string
  vBucks: number
  cosmetics: UserCosmetic[]
}

export interface UserCosmetic {
  cosmeticId: string
  name: string
  description?: string
  type: string
  rarity: string
  imageUrl?: string
  price: number
  purchasedAt: string
}
