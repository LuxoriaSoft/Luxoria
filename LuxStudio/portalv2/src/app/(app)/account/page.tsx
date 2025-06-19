'use client'

import { Heading, Subheading } from '@/components/heading'
import { useUser } from '@/hooks/useUser'
import { Avatar } from '@/components/avatar'
import { Text } from '@/components/text'

export default function AccountPage() {
  const { user, loading, error } = useUser()

  if (loading) return <Text>Loading...</Text>
  if (error || !user) return <Text className="text-red-500">Failed to load user data.</Text>

  return (
    <div className="p-6 max-w-xl mx-auto space-y-6">
      <Heading>My Account</Heading>
      <Subheading>Personal Information</Subheading>

      <div className="flex items-center gap-4">
        <Avatar
          src={
            user.avatarFileName
              ? `${process.env.NEXT_PUBLIC_API_URL}/auth/avatar/${user.avatarFileName}`
              : '/users/default.jpg'
          }
          className="size-20 border border-white"
          square
          alt={user.username}
        />
        <div>
          <p className="text-lg font-semibold">{user.username}</p>
          <p className="text-sm text-gray-500">{user.email}</p>
        </div>
      </div>

      <div className="mt-6">
        <p className="text-sm text-zinc-500">
          Account created: {user.createdAt ? new Date(user.createdAt).toLocaleDateString() : '—'}
        </p>
        <p className="text-sm text-zinc-500">
          Last update: {user.updatedAt ? new Date(user.updatedAt).toLocaleDateString() : '—'}
        </p>
      </div>
    </div>
  )
}
