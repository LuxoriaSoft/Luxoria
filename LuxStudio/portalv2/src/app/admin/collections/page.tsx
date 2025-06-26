'use client'

import { useEffect, useState, useCallback } from 'react'
import { Button } from '@/components/button'
import { Input } from '@/components/input'
import { Heading } from '@/components/heading'
import { Divider } from '@/components/divider'
import { AuthService } from '@/services/auth'
import { AdminService, AdminCollection } from '@/services/admin.services' // À créer
import { ApplicationLayout } from '@/app/(app)/application-layout'
import { getEvents } from '@/data'

export default function AdminCollectionsPage() {
  const [collections, setCollections] = useState<AdminCollection[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [events, setEvents] = useState<Awaited<ReturnType<typeof getEvents>>>([])

  useEffect(() => {
    getEvents().then(setEvents)
  }, [])

  const fetchCollections = useCallback(async () => {
    const data = await AdminService.getCollections(search)
    setCollections(data)
    setLoading(false)
  }, [search])

  useEffect(() => {
    fetchCollections()
  }, [fetchCollections])

  const handleDeleteCollection = async (collectionId: string) => {
    if (!confirm('Are you sure you want to delete this collection?')) return
    await AdminService.deleteCollection(collectionId)
    fetchCollections()
  }

  return (
    <ApplicationLayout events={events}>
      <div className="p-6">
        <Heading>Admin - Collection Management</Heading>
        <div className="mt-4 flex max-w-xl">
          <Input
            placeholder="Search by ID, Name or User Email..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="bg-zinc-800 text-white placeholder-zinc-400 rounded-lg flex-1"
          />
        </div>

        {loading && <p className="mt-6 text-zinc-400">Loading collections...</p>}

        {!loading && (
          <table className="mt-6 w-full text-left rounded-lg overflow-hidden border border-zinc-700">
            <thead className="bg-zinc-800 text-zinc-300">
              <tr>
                <th className="p-3">ID</th>
                <th className="p-3">Name</th>
                <th className="p-3">Allowed Emails</th>
                <th className="p-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              {collections.map((col) => (
                <tr key={col.id} className="border-b border-zinc-700 hover:bg-zinc-800 text-zinc-100">
                  <td className="p-3">{col.id}</td>
                  <td className="p-3">{col.name}</td>
                  <td className="p-3">{col.allowedEmails.join(', ')}</td>
                  <td className="p-3">
                    <Button
                      className="bg-rose-600 hover:bg-rose-500 rounded"
                      onClick={() => handleDeleteCollection(col.id)}
                    >
                      Delete
                    </Button>
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
