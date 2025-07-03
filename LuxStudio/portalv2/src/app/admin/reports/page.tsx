'use client'

import { useEffect, useState } from 'react'
import { AdminService, CollectionReport, UserReport } from '@/services/admin.services'
import { ApplicationLayout } from '@/app/(app)/application-layout'
import { Heading } from '@/components/heading'
import { Divider } from '@/components/divider'
import { getEvents } from '@/data'

export default function AdminReportsPage() {
  const [reports, setReports] = useState<CollectionReport[]>([])
  const [userReports, setUserReports] = useState<UserReport[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [events, setEvents] = useState<Awaited<ReturnType<typeof getEvents>>>([])

  useEffect(() => {
    getEvents().then(setEvents)

    const fetchReports = async () => {
      try {
        const [collectionReports, userReportsData] = await Promise.all([
          AdminService.getCollectionReports(),
          AdminService.getUserReports()
        ])
        setReports(collectionReports)
        setUserReports(userReportsData)
      } catch (err: any) {
        setError(err.message || 'Failed to load reports')
      } finally {
        setLoading(false)
      }
    }

    fetchReports()
  }, [])

    const handleDeleteUserReport = async (reportId: string) => {
      if (!confirm('Are you sure you want to delete this user report?')) return
      try {
        await AdminService.deleteUserReport(reportId)
        setUserReports(prev => prev.filter(r => r.id !== reportId))
      } catch (err) {
        alert("Failed to delete user report.")
      }
    }

  return (
    <ApplicationLayout events={events}>
      <div className="p-6">
        <Heading>Admin - Collection Reports</Heading>

        {loading && <p className="mt-6 text-zinc-400">Loading reports...</p>}
        {error && <p className="mt-6 text-red-500">{error}</p>}

        {!loading && reports.length === 0 && (
          <p className="mt-6 text-zinc-400">No collection reports found.</p>
        )}

        {!loading && reports.length > 0 && (
          <table className="mt-6 w-full text-left rounded-lg overflow-hidden border border-zinc-700">
            <thead className="bg-zinc-800 text-zinc-300">
              <tr>
                <th className="p-3">Date</th>
                <th className="p-3">Collection</th>
                <th className="p-3">Reported By</th>
                <th className="p-3">Reason</th>
              </tr>
            </thead>
            <tbody>
              {reports.map((report) => (
                <tr key={report.id} className="border-b border-zinc-700 hover:bg-zinc-800 text-zinc-100">
                  <td className="p-3">{new Date(report.createdAt).toLocaleString()}</td>
                  <td className="p-3">{report.collectionName}</td>
                  <td className="p-3">{report.reportedBy}</td>
                  <td className="p-3">{report.reason}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        <Divider className="my-10" />

        <Heading>Admin - User Reports</Heading>

        {!loading && userReports.length === 0 && (
          <p className="mt-6 text-zinc-400">No user reports found.</p>
        )}

        {!loading && userReports.length > 0 && (
          <table className="mt-6 w-full text-left rounded-lg overflow-hidden border border-zinc-700">
            <thead className="bg-zinc-800 text-zinc-300">
              <tr>
                <th className="p-3">Date</th>
                <th className="p-3">Collection</th>
                <th className="p-3">Reported User</th>
                <th className="p-3">Reported By</th>
                <th className="p-3">Reason</th>
                <th className="p-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              {userReports.map((report, index) => (
                <tr key={report.id} className="border-b border-zinc-700 hover:bg-zinc-800 text-zinc-100">
                  <td className="p-3">{new Date(report.createdAt).toLocaleString()}</td>
                  <td className="p-3">{report.collectionName}</td>
                  <td className="p-3">{report.reportedUserEmail}</td>
                  <td className="p-3">{report.reportedBy}</td>
                  <td className="p-3">{report.reason}</td>
                  <td className="p-3">
                    <button
                      onClick={() => handleDeleteUserReport(report.id)}
                      className="text-rose-500 hover:text-rose-300"
                    >
                      Delete
                    </button>
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
