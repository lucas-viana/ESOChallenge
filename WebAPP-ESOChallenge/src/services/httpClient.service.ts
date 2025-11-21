/**
 * Serviço HTTP genérico e reutilizável
 * Seguindo o princípio da Inversão de Dependência (SOLID)
 * Responsável por toda comunicação HTTP com interceptor de autenticação
 */

import { API_CONFIG } from '@/config/api.config'
import { addAuthToken } from '@/services/interceptors/auth.interceptor'
import type { ErrorResponse } from '@/types/cosmetic.types'

export class HttpClientService {
  private readonly baseURL: string

  constructor(baseURL: string = API_CONFIG.BASE_URL) {
    this.baseURL = baseURL
  }

  /**
   * Método privado para construir a URL completa
   */
  private buildURL(endpoint: string): string {
    return `${this.baseURL}${endpoint}`
  }

  /**
   * Método privado para tratar erros HTTP
   */
  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const errorData: ErrorResponse = await response.json().catch(() => ({
        success: false,
        message: `HTTP Error: ${response.status} ${response.statusText}`,
      }))

      throw new Error(errorData.message || 'Erro ao processar requisição')
    }

    return response.json()
  }

  /**
   * GET Request genérico com autenticação automática
   */
  async get<T>(endpoint: string, signal?: AbortSignal): Promise<T> {
    try {
      const config = addAuthToken({
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        signal,
      })

      const response = await fetch(this.buildURL(endpoint), config)

      return this.handleResponse<T>(response)
    } catch (error) {
      if (error instanceof Error) {
        throw new Error(`Falha na requisição GET: ${error.message}`)
      }
      throw error
    }
  }

  /**
   * POST Request genérico com autenticação automática
   */
  async post<T, B = unknown>(endpoint: string, body: B, signal?: AbortSignal): Promise<T> {
    try {
      const config = addAuthToken({
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(body),
        signal,
      })

      const response = await fetch(this.buildURL(endpoint), config)

      return this.handleResponse<T>(response)
    } catch (error) {
      if (error instanceof Error) {
        throw new Error(`Falha na requisição POST: ${error.message}`)
      }
      throw error
    }
  }
}

// Singleton instance - Padrão de projeto Singleton
export const httpClient = new HttpClientService()
