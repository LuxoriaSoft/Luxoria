'use client'

import { Heading } from '@/components/heading'
import { ApplicationLayout } from '@/app/(app)/application-layout'
import { getEvents } from '@/data'

export default function AdminActivityLogsPage({ events }: { events: Awaited<ReturnType<typeof getEvents>> }) {
  return (
    <ApplicationLayout events={events}>
      <div className="p-6 space-y-4">
        <Heading>Admin - Activity Logs</Heading>
        <p className="text-zinc-400">Nothing to display for now but it will display app logs later</p>
      </div>
    </ApplicationLayout>
  );
}
