import { httpClient } from './httpClient.service'
import type { UserProfile, UserProfileDetails } from '@/types/user'

class UserService {
  async getAllUsers(): Promise<UserProfile[]> {
    return await httpClient.get<UserProfile[]>('/api/admin/users')
  }

  async getUserProfile(userId: string): Promise<UserProfileDetails> {
    return await httpClient.get<UserProfileDetails>(`/api/admin/users/${userId}`)
  }
}

export default new UserService()
