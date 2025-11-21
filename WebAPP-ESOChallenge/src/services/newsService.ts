import { httpClient } from './httpClient.service'
import type { NewsApiResponse } from '@/types/news'

/**
 * Service for Fortnite news operations
 */
class NewsService {
  /**
   * Gets the latest Fortnite news for all game modes (BR, STW, Creative)
   */
  async getNews(): Promise<NewsApiResponse> {
    return await httpClient.get<NewsApiResponse>('/api/news')
  }
}

export default new NewsService()
