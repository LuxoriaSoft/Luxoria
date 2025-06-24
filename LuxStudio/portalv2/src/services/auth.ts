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

  public static async requestVerification(
    username: string,
    email: string,
    password: string,
    role: 0 | 1
  ) {
    const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auth/request-verification`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, email, password, role }), // <-- role est bien un 0 ou 1
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(error || 'Error during verification request')
    }
    return await response.text()
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

  static async whoAmI(): Promise<{ role: string; [key: string]: any }> {
    const token = localStorage.getItem('token')
    if (!token) {
      throw new Error('No token found')
    }

    try {
      const response = await api.get('/auth/whoami', {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      })
      return response.data
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Unable to fetch user information')
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
