'use client'

import { useEffect, useState, useCallback } from 'react'
import { Button } from '@/components/button'
import { Input } from '@/components/input'
import { AuthService } from '@/services/auth'
import { AdminService, User } from '@/services/admin.services'
import { Heading } from '@/components/heading'
import { Divider } from '@/components/divider'
import { ApplicationLayout } from '@/app/(app)/application-layout' // ✅ Import du layout global
import { getEvents } from '@/data'
import { useRouter } from 'next/navigation'

export default function AdminUsersPage() {
  const router = useRouter()
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [events, setEvents] = useState<Awaited<ReturnType<typeof getEvents>>>([])
  const [inviteEmail, setInviteEmail] = useState('')
  const [inviteRole, setInviteRole] = useState<0 | 1>(0)
  const [inviteModalOpen, setInviteModalOpen] = useState(false);


  useEffect(() => {
    getEvents().then(setEvents)
  }, [])

  const handleInviteUser = async () => {
  if (!inviteEmail) {
    alert('Please enter an email address.')
    return
  }
  try {
    await AdminService.inviteUser(inviteEmail, inviteRole)
    alert(`Invitation sent to ${inviteEmail}`)
    setInviteEmail('')
    setInviteRole(0)
    fetchUsers()
  } catch (error: any) {
    alert(error.message || 'Failed to send invitation.')
  }
}


  const fetchUsers = useCallback(async () => {
    setLoading(true)
    try {
      const data = await AdminService.getUsers(search)
      setUsers(data)
    } catch (error: any) {
      // Si l’erreur vient de l’interceptor et qu’on a un 403
      if (error.isForbidden) {
        router.replace('/forbidden')
      } else {
        console.error(error)
      }
    } finally {
      setLoading(false)
    }
  }, [search, router])

  useEffect(() => {
    fetchUsers()
  }, [fetchUsers])

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

        <div className="mt-4 flex max-w-xl gap-2">
          <Input
            placeholder="Search by email or username..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="bg-zinc-800 text-white placeholder-zinc-400 rounded-lg flex-1"
          />
          <Button
            className="bg-blue-600 hover:bg-blue-500 rounded"
            onClick={() => setInviteModalOpen(true)}
          >
            Invite User
          </Button>
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

        {inviteModalOpen && (
          <div className="fixed inset-0 flex items-center justify-center z-50 bg-black bg-opacity-50">
            <div className="bg-zinc-900 p-6 rounded-lg max-w-sm w-full">
              <Heading level={2} className="text-lg mb-4 text-white">Invite a new user</Heading>
              <div className="flex flex-col gap-4">
                <Input
                  placeholder="User email..."
                  value={inviteEmail}
                  onChange={(e) => setInviteEmail(e.target.value)}
                  className="bg-zinc-800 text-white placeholder-zinc-400 rounded-lg"
                />
                <select
                  value={inviteRole}
                  onChange={(e) => setInviteRole(Number(e.target.value) as 0 | 1)}
                  className="bg-zinc-800 text-white rounded-lg p-2"
                >
                  <option value={0}>Client</option>
                  <option value={1}>Photographer</option>
                </select>
                <div className="flex gap-2 justify-end">
                  <Button
                    className="bg-gray-600 hover:bg-gray-500 rounded"
                    onClick={() => setInviteModalOpen(false)}
                  >
                    Cancel
                  </Button>
                  <Button
                    className="bg-blue-600 hover:bg-blue-500 rounded"
                    onClick={handleInviteUser}
                  >
                    Send Invitation
                  </Button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </ApplicationLayout>
  )
}