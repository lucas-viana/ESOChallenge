/**
 * Serviço de Cosméticos
 * Responsável pela lógica de negócio relacionada aos cosméticos
 * Seguindo o princípio da Responsabilidade Única (SOLID)
 */

import { httpClient } from './httpClient.service'
import { API_CONFIG } from '@/config/api.config'
import type {
  Cosmetic,
  ApiResponse,
  CosmeticFilters,
  PaginationParams,
  SearchResponse,
} from '@/types/cosmetic.types'

export class CosmeticService {
  /**
   * Busca todos os cosméticos
   */
  async getAllCosmetics(signal?: AbortSignal): Promise<Cosmetic[]> {
    try {
      const response = await httpClient.get<ApiResponse<Cosmetic[]>>(
        API_CONFIG.ENDPOINTS.COSMETICS.ALL,
        signal,
      )
      return response.data || []
    } catch (error) {
      console.error('Erro ao buscar todos os cosméticos:', error)
      throw error
    }
  }

  /**
   * Busca cosméticos novos
   */
  async getNewCosmetics(signal?: AbortSignal): Promise<Cosmetic[]> {
    try {
      const response = await httpClient.get<ApiResponse<Cosmetic[]>>(
        API_CONFIG.ENDPOINTS.COSMETICS.NEW,
        signal,
      )
      return response.data || []
    } catch (error) {
      console.error('Erro ao buscar cosméticos novos:', error)
      throw error
    }
  }

  /**
   * Busca cosméticos da loja
   */
  async getShopCosmetics(signal?: AbortSignal): Promise<Cosmetic[]> {
    try {
      const response = await httpClient.get<ApiResponse<Cosmetic[]>>(
        API_CONFIG.ENDPOINTS.COSMETICS.SHOP,
        signal,
      )
      return response.data || []
    } catch (error) {
      console.error('Erro ao buscar cosméticos da loja:', error)
      throw error
    }
  }

  /**
   * Busca cosmético por ID
   */
  async getCosmeticById(id: string, signal?: AbortSignal): Promise<Cosmetic | null> {
    try {
      const response = await httpClient.get<ApiResponse<Cosmetic>>(
        API_CONFIG.ENDPOINTS.COSMETICS.BY_ID(id),
        signal,
      )
      return response.data || null
    } catch (error) {
      console.error(`Erro ao buscar cosmético ${id}:`, error)
      throw error
    }
  }

  /**
   * Busca cosméticos com filtros avançados, paginação e ordenação
   */
  async searchCosmetics(
    filters: CosmeticFilters & PaginationParams,
    signal?: AbortSignal,
  ): Promise<SearchResponse> {
    try {
      const response = await httpClient.post<any>(
        `${API_CONFIG.ENDPOINTS.COSMETICS.ALL}/search`,
        filters,
        signal,
      )

      // A API retorna { success, data, pagination, filters }
      // Precisamos mapear para o formato esperado
      return {
        items: response.data || [],
        pagination: response.pagination,
        filters: response.filters,
      }
    } catch (error) {
      console.error('Erro ao buscar cosméticos com filtros:', error)
      throw error
    }
  }
}

// Singleton instance
export const cosmeticService = new CosmeticService()
