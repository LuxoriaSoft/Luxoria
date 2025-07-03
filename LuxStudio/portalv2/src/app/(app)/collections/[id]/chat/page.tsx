'use client'

import { useCollectionDetail } from '@/hooks/useCollectionDetail'
import { useUser } from '@/hooks/useUser'
import { Input } from '@/components/input'
import { Button } from '@/components/button'
import { Avatar } from '@/components/avatar'
import { Text } from '@/components/text'
import { useEffect, useState } from 'react'
import { CollectionService } from '@/services/collection.services'
import { useRouter } from 'next/navigation'
import { ArrowLeftIcon } from '@heroicons/react/20/solid' // pour une icône flèche


export default function CollectionChatPage() {
  const { id, collection, messages, chatContainerRef } = useCollectionDetail()
  const { user } = useUser()
  const [chatMessage, setChatMessage] = useState('')
  const [mentionVisible, setMentionVisible] = useState(false)
  const [mentionQuery, setMentionQuery] = useState('')
  const [filteredEmails, setFilteredEmails] = useState<string[]>([])
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

  if (!collection) return <Text className="text-red-500">Collection not found.</Text>
  
  return (
    <div className="p-6 max-w-screen-xl mx-auto">
      <div className="flex items-center mb-4 gap-2">
    </div>
      <h1 className="text-2xl font-bold mb-4">{collection.name} - Chat</h1>

      <div
        ref={chatContainerRef}
        className="border rounded p-4 h-[70vh] overflow-y-auto bg-white space-y-4"
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
            className="text-zinc-900"
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
  )
}
