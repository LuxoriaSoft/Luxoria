// services/auth.service.ts
import api from '@/services/api'

export class AuthService {
  static async login(username: string, password: string, captchaToken: string): Promise<string> {
    try {
      const response = await api.post('/auth/login', {
        username,
        password,
        captchaToken,
      })

      return response.data.token
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Login failed.')
    }
  }

  static async requestVerification(username: string, email: string, password: string): Promise<void> {
    try {
      await api.post('/auth/request-verification', {
        username,
        email,
        password,
      })
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Registration failed.')
    }
  }

  static async verifyCode(email: string, code: string): Promise<void> {
    try {
      await api.post('/auth/verify-code', {
        email,
        code,
      })
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Invalid code.')
    }
  }

    static async uploadAvatar(file: File, token: string): Promise<void> {
    const formData = new FormData()
    formData.append('file', file)

    try {
        await api.post('/auth/upload-avatar', formData, {
        headers: {
            Authorization: `Bearer ${token}`,
        },
        })
    } catch (error: any) {
        throw new Error(error.response?.data?.message || 'Avatar upload failed.')
    }
    }


  static async getUserEmailById(id: string): Promise<string> {
    try {
      const response = await api.get(`/auth/user/${id}`)
      return response.data.email
    } catch (error: any) {
      throw new Error('Unable to fetch email.')
    }
  }
}
