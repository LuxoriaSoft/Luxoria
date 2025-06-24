import { useEffect, useState } from 'react'
import api from '@/services/api'

export type User = {
  username: string
  email: string
  avatarFileName?: string
  createdAt?: string
  updatedAt?: string
  role?: number
}

export function useUser() {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const fetchUser = async () => {
      try {
        const token = localStorage.getItem('token')
        if (!token) return

        const res = await api.get('/auth/whoami', {
          headers: { Authorization: `Bearer ${token}` },
        })

        setUser(res.data)
      } catch (err) {
        console.error('Failed to fetch user', err)
        setError('Impossible de charger les données utilisateur.')
      } finally {
        setLoading(false)
      }
    }

    fetchUser()
  }, [])

  return { user, loading, error }
}
