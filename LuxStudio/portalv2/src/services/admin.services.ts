import api from '@/services/api'

export interface User {
    id: string
    username: string
    email: string
    role: number
    isBlocked: boolean
}

export interface AdminCollection {
    id: string
    name: string
    allowedEmails: string[]
}

export class AdminService {
    static async getUsers(search?: string): Promise<User[]> {
        const token = localStorage.getItem('token')
        if (!token) throw new Error('No token found')
        const res = await api.get(`/admin/users${search ? `?search=${encodeURIComponent(search)}` : ''}`, {
            headers: { Authorization: `Bearer ${token}` },
        })
        return res.data
    }

    static async resetPassword(userId: string): Promise<void> {
        const token = localStorage.getItem('token')
        await api.post(`/admin/users/reset-password/${userId}`, null, {
            headers: { Authorization: `Bearer ${token}` },
        })
    }

    static async blockUser(userId: string): Promise<void> {
        const token = localStorage.getItem('token')
        await api.post(`/admin/users/block/${userId}`, null, {
            headers: { Authorization: `Bearer ${token}` },
        })
    }

    static async unblockUser(userId: string): Promise<void> {
        const token = localStorage.getItem('token')
        await api.post(`/admin/users/unblock/${userId}`, null, {
            headers: { Authorization: `Bearer ${token}` },
        })
    }
    static async getCollections(search: string): Promise<AdminCollection[]> {
        const token = localStorage.getItem('token')
        if (!token) throw new Error('No token found')

        const res = await api.get(`/admin/collections?search=${encodeURIComponent(search)}`, {
            headers: {
                Authorization: `Bearer ${token}`
            }
        })

        return res.data
    }

    static async deleteCollection(collectionId: string): Promise<void> {
        const token = localStorage.getItem('token')
        if (!token) throw new Error('No token found')

        await api.delete(`/admin/collections/${collectionId}`, {
            headers: {
                Authorization: `Bearer ${token}`
            }
        })
    }
}
