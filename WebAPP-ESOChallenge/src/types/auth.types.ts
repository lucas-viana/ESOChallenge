/**
 * Authentication Types
 * Interfaces e tipos relacionados à autenticação
 */

/**
 * Dados de login do usuário
 */
export interface LoginRequest {
  email: string
  password: string
}

/**
 * Dados de registro do usuário
 */
export interface RegisterRequest {
  email: string
  password: string
  confirmPassword: string
}

/**
 * Resposta de autenticação do backend
 */
export interface AuthResponse {
  token: string
  email: string
  expiresAt: string
}

/**
 * Dados do usuário autenticado
 */
export interface User {
  email: string
  token: string
  expiresAt: Date
}

/**
 * Estado de autenticação
 */
export interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

/**
 * Erros de validação
 */
export interface ValidationError {
  field: string
  message: string
}

/**
 * Resultado de validação
 */
export interface ValidationResult {
  isValid: boolean
  errors: ValidationError[]
}
