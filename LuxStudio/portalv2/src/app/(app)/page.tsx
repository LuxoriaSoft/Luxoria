'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useUser } from '@/hooks/useUser'

export default function Home() {
  const router = useRouter()
  const { user } = useUser()

  useEffect(() => {
    const timer = setTimeout(() => {
      router.replace('/collections')
    }, 0)

    return () => clearTimeout(timer)
  }, [router])

  if (!user) {
    return <div>Loading user info...</div>
  }

  return (
    <div>
      <h1>Hello {user.username}</h1>
      <p>Loading...</p>
    </div>
  )
}
