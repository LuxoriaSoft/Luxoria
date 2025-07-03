'use client'

import { useCollectionDetail } from '@/hooks/useCollectionDetail'
import { useUser } from '@/hooks/useUser'
import { Input } from '@/components/input'
import { Button } from '@/components/button'
import { Text } from '@/components/text'
import { useEffect, useState } from 'react'
import { CollectionService } from '@/services/collection.services'

export default function CollectionChatPage() {
  const { id, collection, messages, setMessages, chatContainerRef } = useCollectionDetail()
  const { user } = useUser()
  const [chatMessage, setChatMessage] = useState('')
  const [mentionVisible, setMentionVisible] = useState(false)
  const [mentionQuery, setMentionQuery] = useState('')
  const [filteredEmails, setFilteredEmails] = useState<string[]>([])

  // Gestion des mentions
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

  // Scroll auto vers le bas à chaque nouveau message
  useEffect(() => {
    if (chatContainerRef.current) {
      setTimeout(() => {
        chatContainerRef.current!.scrollTo({
          top: chatContainerRef.current!.scrollHeight,
          behavior: 'smooth',
        })
      }, 50)
    }
  }, [messages])

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

      // Ajout local pour afficher direct le message envoyé
      setMessages(prev => [
        ...prev,
        {
          senderUsername: user.username,
          senderEmail: user.email,
          message: chatMessage.trim(),
          sentAt: new Date().toISOString(),
          avatarFileName: '', // si tu as avatar, mets-le ici
          isMine: true,
        },
      ])

      setChatMessage('')
    } catch (err) {
      console.error('Error sending message:', err)
    }
  }

  if (!collection) return <Text className="text-red-500">Collection not found.</Text>

  return (
    <div className="flex flex-col min-h-screen w-full bg-zinc-900 text-white">
      {/* Zone de messages */}
      <div
        ref={chatContainerRef}
        className="flex-1 overflow-y-auto p-4 space-y-4"
      >
        {messages.length === 0 && (
          <Text className="text-zinc-400">Welcome to the chat room!</Text>
        )}

        {messages.map((msg, index) => (
          <div
            key={index}
            className={`flex items-end gap-3 w-full ${
              msg.isMine ? 'justify-end' : 'justify-start'
            }`}
          >
            <div className="flex flex-col max-w-xs sm:max-w-md text-left">
              <div className={`flex items-center text-sm font-semibold ${msg.isMine ? 'justify-end' : 'justify-start'}`}>
                <span className={`${msg.isMine ? 'text-blue-400' : 'text-red-400'}`}>
                  {msg.isMine ? 'You' : msg.senderUsername}
                </span>
                <span className="ml-2 text-xs text-zinc-500">
                  {new Date(msg.sentAt).toLocaleTimeString([], {
                    hour: '2-digit',
                    minute: '2-digit',
                  })}
                </span>
              </div>
              <div
                className={`mt-1 inline-block rounded-xl px-4 py-2 text-sm break-words max-w-xs w-max ${
                  msg.isMine ? 'bg-zinc-800 text-white text-right ml-auto' : 'bg-zinc-800 text-white text-left'
                }`}
              >
                {msg.message}
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Input sticky en bas */}
      <div className="sticky bottom-0 bg-zinc-800 p-3 flex gap-2 border-t border-zinc-700">
        <div className="relative flex-1">
          <Input
            placeholder="Send a message"
            value={chatMessage}
            onChange={(e) => setChatMessage(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSendMessage()}
            className="text-white bg-zinc-700 border-zinc-600 focus:border-purple-500 placeholder-zinc-400"
          />
          {mentionVisible && (
            <div className="absolute bottom-full mb-2 w-full max-h-40 overflow-y-auto rounded border bg-zinc-800 p-2 shadow z-50">
              {filteredEmails.map(email => (
                <div
                  key={email}
                  className="cursor-pointer px-3 py-1 text-sm text-white hover:bg-zinc-700"
                  onClick={() => insertMention(email)}
                >
                  {email}
                </div>
              ))}
            </div>
          )}
        </div>
        <Button
          onClick={handleSendMessage}
          className="bg-purple-600 hover:bg-purple-700 text-white font-bold"
        >
          Chat
        </Button>
      </div>
    </div>
  )
}
