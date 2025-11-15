/**
 * HTTP Interceptor
 * Adiciona token JWT automaticamente nas requisições
 * Segue Dependency Inversion Principle
 */

import { authService } from '@/services/auth.service'

/**
 * Interface para configuração de fetch
 */
interface FetchConfig extends RequestInit {
  headers?: HeadersInit
}

/**
 * Adiciona o token JWT ao header Authorization
 * @param config Configuração do fetch
 * @returns Configuração com token adicionado
 */
export function addAuthToken(config: FetchConfig = {}): FetchConfig {
  const token = authService.getToken()

  if (token) {
    const headers = new Headers(config.headers)
    headers.set('Authorization', `Bearer ${token}`)

    return {
      ...config,
      headers
    }
  }

  return config
}

/**
 * Wrapper do fetch com interceptor de autenticação
 * @param url URL da requisição
 * @param config Configuração do fetch
 * @returns Promise com a resposta
 */
export async function authenticatedFetch(
  url: string,
  config: FetchConfig = {}
): Promise<Response> {
  const configWithAuth = addAuthToken(config)
  return fetch(url, configWithAuth)
}
