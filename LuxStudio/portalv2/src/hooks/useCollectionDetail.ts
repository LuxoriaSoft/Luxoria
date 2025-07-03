import { useEffect, useRef, useState } from 'react'
import { useParams } from 'next/navigation'
import { Collection, ChatMessage, CollectionService } from '@/services/collection.services'
import { ChatService } from '@/services/chat.services'
import { useUser } from '@/hooks/useUser'
import * as signalR from '@microsoft/signalr'

export function useCollectionDetail() {
  const { id } = useParams<{ id: string }>()
  const { user } = useUser()
  const [collection, setCollection] = useState<Collection | null>(null)
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [loading, setLoading] = useState(true)
  const chatContainerRef = useRef<HTMLDivElement>(null)
  const connectionRef = useRef<signalR.HubConnection | null>(null)

  // Load collection + messages once
  useEffect(() => {
    CollectionService.fetchCollection(id)
      .then((data) => {
        setCollection(data)
        setMessages(
          (data.chatMessages || []).map((msg) => ({
            ...msg,
            isMine: msg.senderUsername === user?.username,
          }))
        )
      })
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [id, user])

  useEffect(() => {
    if (!user?.username) return // ⛔️ n'initialise pas SignalR trop tôt

    const token = localStorage.getItem('token') || ''
    const conn = ChatService.createConnection(id, token)
    connectionRef.current = conn

    const handleReceive = (sender: string, message: string, avatar: string, sentAt: string) => {
      const isMine = sender === user.username // ✅ ici user est garanti
      setMessages(prev => [
        ...prev,
        {
          senderUsername: sender,
          senderEmail: '',
          message,
          sentAt,
          avatarFileName: avatar,
          isMine,
        },
      ])
      setTimeout(() => {
        chatContainerRef.current?.scrollTo({
          top: chatContainerRef.current.scrollHeight,
          behavior: 'smooth',
        })
      }, 0)
    }

    conn.on('ReceiveMessage', handleReceive)

    return () => {
      conn.off('ReceiveMessage', handleReceive)
      ChatService.closeConnection(conn, id)
    }
  }, [id, user?.username]) // ✅ on attend bien que user soit prêt

  return {
    id,
    collection,
    setCollection,
    messages,
    setMessages,
    loading,
    chatContainerRef,
  }
}
