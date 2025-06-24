'use client'

import { useEffect, useState } from 'react'
import { Button } from '@/components/button'
import { Input } from '@/components/input'
import { AuthService } from '@/services/auth'
import { AdminService, User } from '@/services/admin.services'
import { Heading } from '@/components/heading'
import { Divider } from '@/components/divider'
import { ApplicationLayout } from '@/app/(app)/application-layout' // ✅ Import du layout global
import { getEvents } from '@/data'

export default function AdminUsersPage({ events }: { events: Awaited<ReturnType<typeof getEvents>> }) {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')

  const fetchUsers = async () => {
    const data = await AdminService.getUsers(search)
    setUsers(data)
    setLoading(false)
  }

  useEffect(() => {
    fetchUsers()
  }, [search])

  const handleResetPassword = async (userId: string) => {
    await AdminService.resetPassword(userId)
    alert('Password reset link sent!')
  }

  const handleBlockUser = async (userId: string) => {
    await AdminService.blockUser(userId)
    fetchUsers()
  }

  const handleUnblockUser = async (userId: string) => {
    await AdminService.unblockUser(userId)
    fetchUsers()
  }

  return (
    <ApplicationLayout events={events}>
      <div className="p-6">
        <Heading>Admin - User Management</Heading>
        <div className="mt-4 flex max-w-xl">
          <Input
            placeholder="Search by email or username..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="bg-zinc-800 text-white placeholder-zinc-400 rounded-lg flex-1"
          />
        </div>

        {loading && <p className="mt-6 text-zinc-400">Loading users...</p>}

        {!loading && (
          <table className="mt-6 w-full text-left rounded-lg overflow-hidden border border-zinc-700">
            <thead className="bg-zinc-800 text-zinc-300">
              <tr>
                <th className="p-3">Username</th>
                <th className="p-3">Email</th>
                <th className="p-3">Role</th>
                <th className="p-3">Blocked?</th>
                <th className="p-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr
                  key={user.id}
                  className="border-b border-zinc-700 hover:bg-zinc-800 text-zinc-100"
                >
                  <td className="p-3">{user.username}</td>
                  <td className="p-3">{user.email}</td>
                  <td className="p-3">{user.role}</td>
                  <td className="p-3">{user.isBlocked ? 'Yes' : 'No'}</td>
                  <td className="p-3 flex flex-wrap gap-2">
                    <Button
                      className="bg-zinc-700 hover:bg-zinc-600 rounded"
                      onClick={() => handleResetPassword(user.id)}
                    >
                      Reset Password
                    </Button>
                    {user.isBlocked ? (
                      <Button
                        className="bg-emerald-600 hover:bg-emerald-500 rounded"
                        onClick={() => handleUnblockUser(user.id)}
                      >
                        Unblock
                      </Button>
                    ) : (
                      <Button
                        className="bg-rose-600 hover:bg-rose-500 rounded"
                        onClick={() => handleBlockUser(user.id)}
                      >
                        Block
                      </Button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </ApplicationLayout>
  )
}
