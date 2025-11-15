/**
 * Validation Utilities
 * Funções de validação seguindo Single Responsibility Principle
 */

import type { ValidationResult, ValidationError } from '@/types/auth.types'

/**
 * Valida se um email é válido
 * @param email Email a ser validado
 * @returns true se válido, false caso contrário
 */
export const isValidEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailRegex.test(email)
}

/**
 * Valida se uma senha é forte
 * Critérios: mínimo 6 caracteres, pelo menos 1 letra maiúscula, 1 minúscula e 1 número
 * @param password Senha a ser validada
 * @returns true se válida, false caso contrário
 */
export const isValidPassword = (password: string): boolean => {
  const minLength = password.length >= 6
  const hasUpperCase = /[A-Z]/.test(password)
  const hasLowerCase = /[a-z]/.test(password)
  const hasNumber = /[0-9]/.test(password)
  
  return minLength && hasUpperCase && hasLowerCase && hasNumber
}

/**
 * Valida formulário de login
 * @param email Email do usuário
 * @param password Senha do usuário
 * @returns Resultado da validação com erros (se houver)
 */
export const validateLoginForm = (email: string, password: string): ValidationResult => {
  const errors: ValidationError[] = []

  // Validar email
  if (!email.trim()) {
    errors.push({ field: 'email', message: 'Email é obrigatório' })
  } else if (!isValidEmail(email)) {
    errors.push({ field: 'email', message: 'Email inválido' })
  }

  // Validar senha
  if (!password) {
    errors.push({ field: 'password', message: 'Senha é obrigatória' })
  } else if (password.length < 6) {
    errors.push({ field: 'password', message: 'Senha deve ter no mínimo 6 caracteres' })
  }

  return {
    isValid: errors.length === 0,
    errors
  }
}

/**
 * Valida formulário de registro
 * @param email Email do usuário
 * @param password Senha do usuário
 * @param confirmPassword Confirmação da senha
 * @returns Resultado da validação com erros (se houver)
 */
export const validateRegisterForm = (
  email: string,
  password: string,
  confirmPassword: string
): ValidationResult => {
  const errors: ValidationError[] = []

  // Validar email
  if (!email.trim()) {
    errors.push({ field: 'email', message: 'Email é obrigatório' })
  } else if (!isValidEmail(email)) {
    errors.push({ field: 'email', message: 'Email inválido' })
  }

  // Validar senha
  if (!password) {
    errors.push({ field: 'password', message: 'Senha é obrigatória' })
  } else if (!isValidPassword(password)) {
    errors.push({
      field: 'password',
      message: 'Senha deve ter no mínimo 6 caracteres, 1 maiúscula, 1 minúscula e 1 número'
    })
  }

  // Validar confirmação de senha
  if (!confirmPassword) {
    errors.push({ field: 'confirmPassword', message: 'Confirmação de senha é obrigatória' })
  } else if (password !== confirmPassword) {
    errors.push({ field: 'confirmPassword', message: 'As senhas não coincidem' })
  }

  return {
    isValid: errors.length === 0,
    errors
  }
}
