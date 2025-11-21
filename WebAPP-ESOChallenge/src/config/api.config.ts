/**
 * Configurações centralizadas da API
 * Facilita manutenção e testes (Princípio DRY - Don't Repeat Yourself)
 *
 * A BASE_URL é configurada através de variáveis de ambiente:
 * - .env.development: para desenvolvimento local
 * - .env.production: para build de produção
 */

export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL,
  ENDPOINTS: {
    COSMETICS: {
      ALL: '/api/cosmetics',
      NEW: '/api/cosmetics/new',
      SHOP: '/api/cosmetics/shop',
      BY_ID: (id: string) => `/api/cosmetics/${id}`,
    },
    AUTH: {
      LOGIN: '/api/auth/login',
      REGISTER: '/api/auth/register',
    },
  },
  TIMEOUT: 10000, // 10 segundos
} as const

export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
} as const
