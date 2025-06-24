// services/collection.service.ts
import api from '@/services/api'

export interface Photo {
  id: string
  filePath: string
  status: number
}

export interface ChatMessage {
  senderUsername: string
  senderEmail: string
  message: string
  sentAt: string
  avatarFileName?: string
  isMine?: boolean
}

export interface Collection {
  id: string
  name: string
  description?: string
  photos: Photo[]
  allowedEmails: string[]
  chatMessages: ChatMessage[]
}

export class CollectionService {
  static async getCollections(): Promise<Collection[]> {
    const token = localStorage.getItem('token')
    if (!token) throw new Error('No token found')

    const res = await api.get('/collection', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })

    return res.data
  }

  static async fetchCollection(id: string): Promise<Collection> {
    const token = localStorage.getItem('token')
    if (!token) throw new Error('No token found')

    const res = await api.get(`/collection/${id}`, {
      headers: { Authorization: `Bearer ${token}` },
    })

    return res.data
  }

  static async updateCollection(
    id: string,
    data: { name: string; description: string; allowedEmails?: string[] }
    ): Promise<void> {
    const token = localStorage.getItem('token');
    if (!token) {
        throw new Error('No token found');
    }

    await api.put(`/collection/${id}`, data, {
        headers: {
        Authorization: `Bearer ${token}`,
        },
    });
  }

  static async deleteCollection(id: string): Promise<void> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('No token found');
    }
    await api.delete(`/collection/${id}`, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  static async uploadPhoto(id: string, file: File): Promise<Photo> {
    const formData = new FormData()
    formData.append('file', file)

    const token = localStorage.getItem('token')
    if (!token) throw new Error('No token found')

    const res = await api.post(`/collection/${id}/upload`, formData, {
      headers: { Authorization: `Bearer ${token}` },
    })

    return res.data
  }

  static async addUserToCollection(id: string, email: string): Promise<{ message: string }> {
    const token = localStorage.getItem('token')
    if (!token) throw new Error('No token found')

    const res = await api.patch(
      `/collection/${id}/allowedEmails`,
      { email },
      {
        headers: { Authorization: `Bearer ${token}` },
      }
    )

    return res.data
  }

  static async sendChatMessage(id: string, data: {
    senderEmail: string
    senderUsername: string
    message: string
  }): Promise<void> {
    const token = localStorage.getItem('token')
    if (!token) throw new Error('No token found')

    await api.post(`/collection/${id}/chat`, data, {
      headers: { Authorization: `Bearer ${token}` },
    })
  }

  static async createCollection(data: {
    name: string;
    description: string;
    allowedEmails?: string[];
  }): Promise<Collection> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('No token found');
    }

    const res = await api.post('/collection/create', data, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    return res.data;
  }
}
