// app/(app)/events/page.tsx

'use client'

import { useEffect, useState } from 'react'
import { CollectionService, Collection } from '@/services/collection.services'
import { Heading } from '@/components/heading'
import { Input, InputGroup } from '@/components/input'
import { Select } from '@/components/select'
import { Button } from '@/components/button'
import { Divider } from '@/components/divider'
import { Link } from '@/components/link'
import { Dropdown, DropdownButton, DropdownItem, DropdownMenu } from '@/components/dropdown'

export default function CollectionsPage() {
  const [collections, setCollections] = useState<Collection[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const fetchData = async () => {
      try {
        const data = await CollectionService.getCollections()
        setCollections(data)
      } catch (err: any) {
        setError(err.message || 'Failed to load collections')
      } finally {
        setLoading(false)
      }
    }

    fetchData()
  }, [])

  return (
    <>
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div className="max-sm:w-full sm:flex-1">
          <Heading>Your collections</Heading>
          <div className="mt-4 flex max-w-xl gap-4">
            <div className="flex-1">
              <InputGroup>
                <Input name="search" placeholder="Search collections…" />
              </InputGroup>
            </div>
            <div>
              <Select name="sort_by">
                <option value="name">Sort by name</option>
                <option value="date">Sort by date</option>
              </Select>
            </div>
          </div>
        </div>
        <Button>Create collection</Button>
      </div>

      {loading && <p className="mt-10 text-center text-zinc-400">Loading...</p>}
      {error && <p className="mt-10 text-center text-red-500">{error}</p>}
      {!loading && !error && collections.length === 0 && (
        <p className="mt-10 text-center text-zinc-500">No collections available.</p>
      )}
      {!loading && !error && collections.length > 0 && (
        <ul className="mt-10">
          {collections.map((col, index) => (
            <li key={col.id}>
              <Divider soft={index > 0} />
              <div className="flex items-center justify-between py-6">
                <div className="space-y-1.5">
                  <div className="text-base/6 font-semibold">
                    <Link href={`/events/${col.id}`}>{col.name}</Link>
                  </div>
                  <div className="text-xs/6 text-zinc-500">
                    {col.description || 'No description.'}
                  </div>
                </div>
                <Dropdown>
                  <DropdownButton plain aria-label="More options">
                    <span className="text-lg">⋮</span>
                  </DropdownButton>
                  <DropdownMenu anchor="bottom end">
                    <DropdownItem href={`/events/${col.id}`}>View</DropdownItem>
                    <DropdownItem>Edit</DropdownItem>
                    <DropdownItem>Delete</DropdownItem>
                  </DropdownMenu>
                </Dropdown>
              </div>
            </li>
          ))}
        </ul>
      )}
    </>
  )
}
