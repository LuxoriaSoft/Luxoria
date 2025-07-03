'use client'

import { useParams } from 'next/navigation'
import { useCollectionDetail } from '@/hooks/useCollectionDetail'
import { useUser } from '@/hooks/useUser'
import { useChatSignalR } from '@/hooks/useChatSignalR'
import { useState } from 'react'
import { Input } from '@/components/input'
import { Button } from '@/components/button'
import { Text } from '@/components/text'
import { CollectionService } from '@/services/collection.services'

export default function PhotoChatPage() {
  const params = useParams()
  const id = params.id as string
  const photoId = params.photoid as string

  const { collection, messages, setMessages, chatContainerRef } = useCollectionDetail()
  const { user } = useUser()
  const [chatMessage, setChatMessage] = useState('')
  const API_URL = process.env.NEXT_PUBLIC_API_URL

const handleSendMessage = async () => {
  if (!chatMessage.trim() || !user) return
  try {
    await CollectionService.sendChatMessage(id, {
      senderEmail: user.email,
      senderUsername: user.username,
      message: chatMessage.trim(),
      photoId,
    })

    // Ajoute localement le message dans la liste pour affichage immédiat
    setMessages(prev => [
      ...prev,
      {
        senderUsername: user.username,
        senderEmail: user.email,
        message: chatMessage.trim(),
        sentAt: new Date().toISOString(),
        avatarFileName: '', // ou user.avatar si tu as ça
        photoId,
        isMine: true,
      }
    ])

    setChatMessage('')
  } catch (err) {
    console.error('Error sending message:', err)
  }
}

  if (!collection) return <Text className="text-red-500">Collection not found.</Text>

  const filteredMessages = messages.filter(msg => msg.photoId === photoId)

  return (
    <div className="flex flex-col min-h-screen w-full bg-zinc-900 text-white">
      {/* Zone de messages */}
      <div
        ref={chatContainerRef}
        className="flex-1 overflow-y-auto p-4 space-y-4"
      >
        {filteredMessages.length === 0 && (
          <Text className="text-zinc-400">Welcome to the chat room!</Text>
        )}

{filteredMessages.map((msg, index) => (
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
