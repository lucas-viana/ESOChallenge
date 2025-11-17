import type {
  PurchaseRequest,
  PurchaseResponse,
  RefundRequest,
  RefundResponse,
  PurchasedCosmetic,
  PurchaseHistoryResponse,
  UserProfile,
  VBucksResponse
} from '@/types/purchase.types'
import { httpClient } from './httpClient.service'

const API_BASE = '/api/purchases'

export const purchaseService = {
  /**
   * Purchase a cosmetic item
   */
  async purchaseCosmetic(cosmeticId: string): Promise<PurchaseResponse> {
    const request: PurchaseRequest = { cosmeticId }
    return await httpClient.post<PurchaseResponse>(API_BASE, request)
  },

  /**
   * Get all cosmetics owned by the current user
   */
  async getMyCosmetics(): Promise<PurchasedCosmetic[]> {
    return await httpClient.get<PurchasedCosmetic[]>(`${API_BASE}/my-cosmetics`)
  },

  /**
   * Refund a previously purchased cosmetic
   */
  async refundCosmetic(cosmeticId: string): Promise<RefundResponse> {
    const request: RefundRequest = { cosmeticId }
    return await httpClient.post<RefundResponse>(`${API_BASE}/refund`, request)
  },

  /**
   * Get purchase history for the current user
   */
  async getPurchaseHistory(): Promise<PurchaseHistoryResponse> {
    return await httpClient.get<PurchaseHistoryResponse>(`${API_BASE}/history`)
  },

  /**
   * Get V-Bucks balance for the current user
   */
  async getVBucks(): Promise<number> {
    const response = await httpClient.get<VBucksResponse>(`${API_BASE}/vbucks`)
    return response.vbucks
  },

  /**
   * Get public profile of any user
   */
  async getUserProfile(userId: string): Promise<UserProfile> {
    return await httpClient.get<UserProfile>(`${API_BASE}/users/${userId}/profile`)
  }
}
