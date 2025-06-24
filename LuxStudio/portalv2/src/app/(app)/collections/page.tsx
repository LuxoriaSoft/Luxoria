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
import { AuthService } from '@/services/auth'

export default function CollectionsPage() {
  const [collections, setCollections] = useState<Collection[]>([])
  const [user, setUser] = useState<{ role: number; email: string } | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [tokenReady, setTokenReady] = useState<boolean | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newCollectionName, setNewCollectionName] = useState('');
  const [newCollectionDescription, setNewCollectionDescription] = useState('');
  const [otherEmails, setOtherEmails] = useState<string[]>([]);
  const [newEmailInput, setNewEmailInput] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [currentCollection, setCurrentCollection] = useState<Collection | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [collectionToDelete, setCollectionToDelete] = useState<Collection | null>(null);


  useEffect(() => {
    const token = document.cookie.includes('token=')

    if (!token) {
      setTokenReady(false)
      return
    }

    setTokenReady(true)
    

    const fetchData = async () => {
      try {
        const data = await CollectionService.getCollections()
        setCollections(data)

        // Récupération du user connecté
        const userData = await AuthService.whoAmI()
        const roleNumber = userData.role === 'Client' ? 0 : 1
        setUser({ role: roleNumber, email: userData.email }) 
      } catch (err: any) {
        setError(err.message || 'Failed to load collections')
      } finally {
        setLoading(false)
      }
    }

    fetchData()
  }, [])

    const resetModal = () => {
      setIsModalOpen(false);
      setIsEditing(false);
      setCurrentCollection(null);
      setNewCollectionName('');
      setNewCollectionDescription('');
      setOtherEmails([]);
      setNewEmailInput('');
    };

    const handleDeleteCollection = async () => {
      if (!collectionToDelete) return;

      try {
        await CollectionService.deleteCollection(collectionToDelete.id);
        const updated = await CollectionService.getCollections();
        setCollections(updated);
        setIsDeleteModalOpen(false);
        setCollectionToDelete(null);
      } catch (error) {
        alert("Error deleting collection");
      }
    };


    const handleEditCollection = async () => {
    if (!currentCollection) return;

    try {
      const currentEmail = user?.email ?? ""; 
      const emailsToSend = [currentEmail, ...otherEmails.filter(email => email !== currentEmail)];
      await CollectionService.updateCollection(currentCollection.id, {
        name: newCollectionName,
        description: newCollectionDescription,
        allowedEmails: emailsToSend,
      });
      const updated = await CollectionService.getCollections();
      setCollections(updated);
      resetModal();
    } catch (err) {
      alert("Error updating collection");
    }
  };

  const handleCreateCollection = async () => {
    try {
      const currentEmail = user?.email ?? ""; 
      const emailsToSend = [currentEmail, ...otherEmails];
      await CollectionService.createCollection({
        name: newCollectionName,
        description: newCollectionDescription,
        allowedEmails: emailsToSend,
      });
      const updated = await CollectionService.getCollections();
      setCollections(updated);
      setIsModalOpen(false);
      setNewCollectionName('');
      setNewCollectionDescription('');
      setOtherEmails([]);
      setNewEmailInput('');
    } catch (err) {
      alert("Error creating collection");
    }
  };


  if (tokenReady === null) return null // Attend qu'on sache si un token est présent
  if (tokenReady === false) return null // Token absent, middleware gère la redirection

  return (
    <>
      {/* MODALE DE CRÉATION DE COLLECTION */}
      {isModalOpen && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
          <div className="bg-zinc-800 text-white p-6 rounded-lg max-w-sm w-full space-y-4">
            <h2 className="text-lg font-bold"> {isEditing ? 'Edit collection' : 'Create a new collection'} </h2>
            <Input
              placeholder="Collection Name"
              value={newCollectionName}
              onChange={(e) => setNewCollectionName(e.target.value)}
              className="bg-zinc-700 text-white placeholder-zinc-400"
            />
            <Input
              placeholder="Collection Description"
              value={newCollectionDescription}
              onChange={(e) => setNewCollectionDescription(e.target.value)}
              className="bg-zinc-700 text-white placeholder-zinc-400"
            />
            <div>
              <Input
                placeholder="Add another email"
                value={newEmailInput}
                onChange={(e) => setNewEmailInput(e.target.value)}
                className="bg-zinc-700 text-white placeholder-zinc-400"
              />
              <Button
                onClick={() => {
                  if (newEmailInput && !otherEmails.includes(newEmailInput)) {
                    setOtherEmails([...otherEmails, newEmailInput]);
                    setNewEmailInput('');
                  }
                }}
                className="mt-2"
              >
                Add
              </Button>
            </div>

            {otherEmails.length > 0 && (
              <div className="mt-2 space-y-1">
                <p>Additional Emails:</p>
                <ul className="list-disc pl-4 space-y-1">
                  {otherEmails.map((email) => (
                    <li key={email} className="flex items-center justify-between">
                      {email}
                      <Button
                        onClick={() => {
                          setOtherEmails(otherEmails.filter((e) => e !== email));
                        }}
                        className="ml-2"
                      >
                        remove
                      </Button>
                    </li>
                  ))}
                </ul>
              </div>
            )}
            <div className="flex justify-end gap-2">
              <Button onClick={() => setIsModalOpen(false)}>Cancel</Button>
              <Button
                onClick={isEditing ? handleEditCollection : handleCreateCollection}
              >
                {isEditing ? 'Save Changes' : 'Create'}
              </Button>
            </div>
          </div>
        </div>
      )}
      {/* MODALE DE CONFIRMATION DE SUPPRESSION */}
      {isDeleteModalOpen && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
          <div className="bg-zinc-800 text-white p-6 rounded-lg max-w-sm w-full space-y-4">
            <h2 className="text-lg font-bold">Are you sure you want to delete this collection?</h2>
            <div className="flex justify-end gap-2">
              <Button onClick={() => setIsDeleteModalOpen(false)}>Cancel</Button>
              <Button onClick={handleDeleteCollection}>Delete</Button>
            </div>
          </div>
        </div>
      )}
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
        {user?.role === 1 && (
          <Button onClick={() => setIsModalOpen(true)}>Create collection</Button>
        )}
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
                    <Link href={`/collections/${col.id}`}>{col.name}</Link>
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
                    <DropdownItem href={`/collections/${col.id}`}>View</DropdownItem>
                    <DropdownItem
                      onClick={() => {
                        setCurrentCollection(col);
                        setNewCollectionName(col.name);
                        setNewCollectionDescription(col.description ?? '');
                        setOtherEmails(col.allowedEmails ?? []);
                        setIsEditing(true);
                        setIsModalOpen(true);
                      }}
                    >
                      Edit
                    </DropdownItem>
                    <DropdownItem
                      onClick={() => {
                        setCollectionToDelete(col);
                        setIsDeleteModalOpen(true);
                      }}
                    >
                      Delete
                    </DropdownItem>
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
