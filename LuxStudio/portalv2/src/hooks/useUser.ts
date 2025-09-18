import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
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
  const router = useRouter()

  useEffect(() => {
    const fetchUser = async () => {
      try {
        const token = localStorage.getItem('token')
        if (!token) {
          setLoading(false)
          return
        }

        const res = await api.get('/auth/whoami', {
          headers: { Authorization: `Bearer ${token}` },
        })

        setUser(res.data)
      } catch (err: any) {
        console.error('Failed to fetch user', err)
        if (err.response && err.response.status === 401) {
          // Cas du compte bloqué ou token invalide : on nettoie
          localStorage.removeItem('token')
          document.cookie = 'token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
          router.push('/login')
        } else {
          setError('Impossible de charger les données utilisateur.')
        }
      } finally {
        setLoading(false)
      }
    }

    fetchUser()
  }, [router])

  return { user, loading, error }
}
