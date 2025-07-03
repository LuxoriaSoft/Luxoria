'use client'

import { useEffect, useState } from 'react'
import { Heading } from '@/components/heading'
import { ApplicationLayout } from '@/app/(app)/application-layout'
import { getEvents } from '@/data'
import { AdminService, ActivityLog } from '@/services/admin.services'

export default function AdminActivityLogsPage() {
  const [logs, setLogs] = useState<ActivityLog[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [events, setEvents] = useState<Awaited<ReturnType<typeof getEvents>>>([])

  useEffect(() => {
    getEvents().then(setEvents)
    const fetchLogs = async () => {
      try {
        const data = await AdminService.getActivityLogs()
        setLogs(data)
      } catch (err: any) {
        console.error(err)
        setError('Failed to load logs.')
      } finally {
        setLoading(false)
      }
    }
    fetchLogs()
  }, [])

  return (
    <ApplicationLayout events={events}>
      <div className="p-6 space-y-6">
        <Heading>Admin - Activity Logs</Heading>

        {loading && <p className="text-zinc-400">Loading activity logs...</p>}
        {error && <p className="text-red-500">{error}</p>}

        {!loading && logs.length === 0 && (
          <p className="text-zinc-400">No activity logs found.</p>
        )}

        {!loading && logs.length > 0 && (
          <div className="overflow-x-auto border border-zinc-700 rounded-lg">
            <table className="min-w-full text-left">
              <thead className="bg-zinc-800 text-zinc-300">
                <tr>
                  <th className="p-3">Timestamp</th>
                  <th className="p-3">Performed By</th>
                  <th className="p-3">Action</th>
                  <th className="p-3">Details</th>
                </tr>
              </thead>
              <tbody className="text-zinc-100">
                {logs.map((log) => (
                  <tr key={log.id} className="border-b border-zinc-700 hover:bg-zinc-800">
                    <td className="p-3 whitespace-nowrap">{new Date(log.timestamp).toLocaleString()}</td>
                    <td className="p-3">{log.performedBy || 'Unknown'}</td>
                    <td className="p-3">{log.action}</td>
                    <td className="p-3">{log.details}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </ApplicationLayout>
  )
}
