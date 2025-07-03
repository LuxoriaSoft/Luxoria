// services/chat.service.ts
import * as signalR from '@microsoft/signalr'

export class ChatService {
  static createConnection(collectionId: string, token: string) {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.NEXT_PUBLIC_SIGNALR_URL}/hubs/chat?collectionId=${collectionId}`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build()

    connection.start().then(() => {
      connection.invoke('JoinCollection', collectionId)
    }).catch(console.error)

    return connection
  }

  static closeConnection(connection: signalR.HubConnection, collectionId: string) {
    connection.invoke('LeaveCollection', collectionId).then(() => {
      connection.stop()
    }).catch(console.error)
  }
}
