/**
 * Authentication Service
 * Serviço responsável por todas as operações de autenticação
 * Segue Single Responsibility Principle
 */

import { httpClient } from './httpClient.service'
import { storage, STORAGE_KEYS } from '@/utils/storage'
import type { LoginRequest, RegisterRequest, AuthResponse, User } from '@/types/auth.types'

/**
 * Interface do serviço de autenticação
 * Segue Interface Segregation Principle
 */
export interface IAuthService {
  login(credentials: LoginRequest): Promise<User>
  register(userData: RegisterRequest): Promise<User>
  logout(): void
  getCurrentUser(): User | null
  isAuthenticated(): boolean
  getToken(): string | null
}

/**
 * Implementação do serviço de autenticação
 */
class AuthService implements IAuthService {
  private readonly LOGIN_ENDPOINT = '/api/auth/login'
  private readonly REGISTER_ENDPOINT = '/api/auth/register'

  /**
   * Realiza login do usuário
   * @param credentials Credenciais de login
   * @returns Dados do usuário autenticado
   */
  async login(credentials: LoginRequest): Promise<User> {
    try {
      const response = await httpClient.post<AuthResponse, LoginRequest>(
        this.LOGIN_ENDPOINT,
        credentials
      )

      const user = this.createUserFromResponse(response)
      this.saveUserData(user)

      return user
    } catch (error) {
      throw new Error(this.getErrorMessage(error))
    }
  }

  /**
   * Realiza registro de novo usuário
   * @param userData Dados do novo usuário
   * @returns Dados do usuário registrado
   */
  async register(userData: RegisterRequest): Promise<User> {
    try {
      const response = await httpClient.post<AuthResponse, RegisterRequest>(
        this.REGISTER_ENDPOINT,
        userData
      )

      const user = this.createUserFromResponse(response)
      this.saveUserData(user)

      return user
    } catch (error) {
      throw new Error(this.getErrorMessage(error))
    }
  }

  /**
   * Realiza logout do usuário
   */
  logout(): void {
    storage.removeItem(STORAGE_KEYS.AUTH_TOKEN)
    storage.removeItem(STORAGE_KEYS.USER_DATA)
  }

  /**
   * Retorna o usuário autenticado atual
   * @returns Dados do usuário ou null se não autenticado
   */
  getCurrentUser(): User | null {
    try {
      const userData = storage.getItem(STORAGE_KEYS.USER_DATA)
      
      if (!userData) {
        return null
      }

      const user: User = JSON.parse(userData)

      // Verificar se o token expirou
      if (this.isTokenExpired(user.expiresAt)) {
        this.logout()
        return null
      }

      return user
    } catch (error) {
      console.error('Error getting current user:', error)
      this.logout()
      return null
    }
  }

  /**
   * Verifica se o usuário está autenticado
   * @returns true se autenticado, false caso contrário
   */
  isAuthenticated(): boolean {
    return this.getCurrentUser() !== null
  }

  /**
   * Retorna o token de autenticação
   * @returns Token ou null se não autenticado
   */
  getToken(): string | null {
    const user = this.getCurrentUser()
    return user?.token ?? null
  }

  /**
   * Cria objeto User a partir da resposta da API
   * @param response Resposta da API
   * @returns Objeto User
   */
  private createUserFromResponse(response: AuthResponse): User {
    return {
      email: response.email,
      token: response.token,
      expiresAt: new Date(response.expiresAt)
    }
  }

  /**
   * Salva dados do usuário no storage
   * @param user Dados do usuário
   */
  private saveUserData(user: User): void {
    storage.setItem(STORAGE_KEYS.AUTH_TOKEN, user.token)
    storage.setItem(STORAGE_KEYS.USER_DATA, JSON.stringify(user))
  }

  /**
   * Verifica se o token está expirado
   * @param expiresAt Data de expiração
   * @returns true se expirado, false caso contrário
   */
  private isTokenExpired(expiresAt: Date): boolean {
    return new Date(expiresAt) < new Date()
  }

  /**
   * Extrai mensagem de erro
   * @param error Erro capturado
   * @returns Mensagem de erro formatada
   */
  private getErrorMessage(error: unknown): string {
    if (error instanceof Error) {
      return error.message
    }
    return 'Erro ao processar autenticação'
  }
}

/**
 * Singleton do serviço de autenticação
 */
export const authService: IAuthService = new AuthService()
