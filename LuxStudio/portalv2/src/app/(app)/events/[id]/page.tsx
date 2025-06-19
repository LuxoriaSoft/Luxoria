'use client'

import { useRef, useState, useEffect } from 'react'
import { Avatar } from '@/components/avatar'
import { Button } from '@/components/button'
import { Heading, Subheading } from '@/components/heading'
import { Input } from '@/components/input'
import { Text } from '@/components/text'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/dialog'
import { useUser } from '@/hooks/useUser'
import { useCollectionDetail } from '@/hooks/useCollectionDetail'
import { CollectionService } from '@/services/collection.services'

export default function CollectionDetail() {
  const {
    id,
    collection,
    setCollection,
    messages,
    setMessages,
    loading,
    chatContainerRef,
  } = useCollectionDetail()

  const [chatMessage, setChatMessage] = useState('')
  const [mentionVisible, setMentionVisible] = useState(false)
  const [mentionQuery, setMentionQuery] = useState('')
  const [filteredEmails, setFilteredEmails] = useState<string[]>([])
  const [modalIndex, setModalIndex] = useState<number | null>(0)
  const [inviteOpen, setInviteOpen] = useState(false)
  const [inviteEmail, setInviteEmail] = useState('')
  const [inviteMessage, setInviteMessage] = useState('')
  const [inviteError, setInviteError] = useState(false)

  const fileInputRef = useRef<HTMLInputElement>(null)
  const scrollRef = useRef<HTMLDivElement>(null)
  const { user } = useUser()
  const API_URL = process.env.NEXT_PUBLIC_API_URL

  useEffect(() => {
    const match = chatMessage.match(/@([\w.-]*)$/)
    if (match) {
      const query = match[1].toLowerCase()
      const filtered = (collection?.allowedEmails || []).filter(
        email => email.toLowerCase().includes(query) && email !== user?.email
      )
      setMentionQuery(query)
      setFilteredEmails(filtered)
      setMentionVisible(filtered.length > 0)
    } else {
      setMentionVisible(false)
    }
  }, [chatMessage, collection, user])

  const insertMention = (email: string) => {
    const updated = chatMessage.replace(/@([\w.-]*)$/, `@${email} `)
    setChatMessage(updated)
    setMentionVisible(false)
  }

  const handleSendMessage = async () => {
    if (!chatMessage.trim() || !user) return
    try {
      await CollectionService.sendChatMessage(id, {
        senderEmail: user.email,
        senderUsername: user.username,
        message: chatMessage.trim(),
      })
      setChatMessage('')
    } catch (err) {
      console.error('Error sending message:', err)
    }
  }

  const handleInvite = async () => {
    if (!inviteEmail) return
    try {
      const res = await CollectionService.addUserToCollection(id, inviteEmail)
      setInviteMessage(res.message || 'User added successfully.')
      setInviteError(false)
      setCollection(prev => prev ? { ...prev, allowedEmails: [...prev.allowedEmails, inviteEmail] } : prev)
      setInviteEmail('')
    } catch (err: any) {
      setInviteMessage(err.response?.data || 'Error adding user.')
      setInviteError(true)
    }
  }

  if (loading) return <Text>Loading...</Text>
  if (!collection) return <Text className="text-red-500">Collection not found.</Text>

  const selectedImage = collection.photos[modalIndex ?? 0]

  return (
    <div className="p-6 max-w-screen-xl mx-auto space-y-10">
      <div className="flex justify-between items-start">
        <div>
          <Heading>{collection.name}</Heading>
          <Text>{collection.description || 'No description.'}</Text>
        </div>
        <div className="flex gap-2">
          <Button onClick={() => setInviteOpen(true)}>Invite</Button>
          <Button className="btn-secondary">Download</Button>
        </div>
      </div>

      <div className="flex gap-6">
        <div className="flex-1 border rounded overflow-hidden">
          {selectedImage && (
            <img src={selectedImage.filePath} alt="Current" className="w-full max-h-[600px] object-contain" />
          )}
        </div>
        <div className="w-1/3 flex flex-col border rounded p-4 bg-white max-h-[600px] overflow-y-auto">
          <Subheading>Chat</Subheading>
          <div
            ref={chatContainerRef}
            className="flex-1 space-y-4 overflow-y-auto my-4"
          >
            {messages.map((msg, index) => (
              <div
                key={index}
                className={`flex items-end gap-3 ${msg.isMine ? 'flex-row-reverse' : ''}`}
              >
                {msg.avatarFileName && (
                  <Avatar
                    src={`${API_URL}/auth/avatar/${msg.avatarFileName}`}
                    alt={msg.senderUsername}
                    className="w-10 h-10"
                    square
                  />
                )}
                <div className={`max-w-xs sm:max-w-md ${msg.isMine ? 'text-left' : 'text-right'}`}>
                  <div className="text-sm font-semibold text-zinc-800">
                    {msg.isMine ? 'You' : msg.senderUsername}{' '}
                    <span className="text-xs text-zinc-400 ml-1">
                      {new Date(msg.sentAt).toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </span>
                  </div>
                  <div
                    className={`mt-1 inline-block rounded-xl px-4 py-2 text-sm ${
                      msg.isMine ? 'bg-blue-600 text-white' : 'bg-zinc-100 text-zinc-800'
                    }`}
                  >
                    {msg.message}
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="relative mt-4 flex gap-2">
            <div className="relative w-full">
              <Input
                placeholder="Your message..."
                value={chatMessage}
                onChange={(e) => setChatMessage(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSendMessage()}
              />
              {mentionVisible && (
                <div className="absolute bottom-full mb-2 w-full max-h-40 overflow-y-auto rounded border bg-white p-2 shadow z-50">
                  {filteredEmails.map(email => (
                    <div
                      key={email}
                      className="cursor-pointer px-3 py-1 text-sm text-zinc-700 hover:bg-zinc-100"
                      onClick={() => insertMention(email)}
                    >
                      {email}
                    </div>
                  ))}
                </div>
              )}
            </div>
            <Button onClick={handleSendMessage}>Send</Button>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-8 gap-4 pt-6">
        {collection.photos.map((photo, index) => (
          <div
            key={photo.id}
            onClick={() => setModalIndex(index)}
            className={`relative cursor-pointer border rounded overflow-hidden shadow ${modalIndex === index ? 'ring-2 ring-blue-500' : ''}`}
          >
            <img src={photo.filePath} alt="Image" className="w-full h-24 object-cover" />
          </div>
        ))}
      </div>

      <Dialog open={inviteOpen} onClose={setInviteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="text-xl font-bold text-center">Invite a user</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 mt-4">
            <Input
              placeholder="Enter user's email..."
              value={inviteEmail}
              onChange={(e) => setInviteEmail(e.target.value)}
            />
            <Button onClick={handleInvite} className="w-full">Send invite</Button>
            {inviteMessage && (
              <Text className={`text-sm text-center ${inviteError ? 'text-red-500' : 'text-green-600'}`}>{inviteMessage}</Text>
            )}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
