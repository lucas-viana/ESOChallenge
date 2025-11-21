import api from './api'
import type { NewsApiResponse } from '@/types/news'

/**
 * Service for Fortnite news operations
 */
class NewsService {
  /**
   * Gets the latest Fortnite news for all game modes (BR, STW, Creative)
   */
  async getNews(): Promise<NewsApiResponse> {
    const response = await api.get<NewsApiResponse>('/news')
    return response.data
  }
}

export default new NewsService()
