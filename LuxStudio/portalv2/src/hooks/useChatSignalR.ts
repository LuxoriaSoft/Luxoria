import { useEffect, useRef } from 'react'
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'

export function useChatSignalR(collectionId: string, onNewMessage: (msg: any) => void) {
  const connectionRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/hubs/chat?collectionId=${collectionId}`)
      .withAutomaticReconnect()
      .build()

    connectionRef.current = conn

    conn.on('ReceiveMessage', (username, message, avatarFileName, sentAt, photoId) => {
      console.log('📩 Message reçu via SignalR', message)
      onNewMessage({ username, message, avatarFileName, sentAt, photoId })
    })

    const startConnection = async () => {
      if (conn.state === HubConnectionState.Disconnected) {
        try {
          await conn.start()
          console.log('✅ SignalR connected')
          await conn.invoke('JoinCollection', collectionId)
        } catch (err) {
          console.error('❌ SignalR connection error:', err)
        }
      }
    }

    startConnection()

    return () => {
      conn.stop().catch(err => console.error('❌ SignalR stop error:', err))
    }
  }, [collectionId, onNewMessage])
}
