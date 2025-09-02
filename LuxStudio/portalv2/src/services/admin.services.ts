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

export interface ActivityLog {
    id: string;
    action: string;
    performedBy: string;
    details: string;
    timestamp: string;
}

export interface CollectionReport {
  id: string
  collectionId: string
  collectionName: string
  reportedBy: string
  reason: string
  createdAt: string
}

export interface UserReport {
  id: string
  collectionId: string
  collectionName: string
  reportedUserEmail: string
  reportedBy: string
  reason: string
  createdAt: string
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

    static async inviteUser(email: string, role: number): Promise<void> {
        const token = localStorage.getItem('token')
        if (!token) throw new Error('No token found')

        const res = await api.post('/admin/users/invite', { email, role }, {
            headers: { Authorization: `Bearer ${token}` },
        })

        if (res.status < 200 || res.status >= 300) {
            throw new Error(res.data?.message || 'Failed to invite user')
        }
    }

    static async deleteUserReport(reportId: string): Promise<void> {
        const token = localStorage.getItem('token')
        if (!token) throw new Error('No token found')
        await api.delete(`/admin/reports/users/${reportId}`, {
            headers: { Authorization: `Bearer ${token}` },
        })
    }

    static async getActivityLogs(): Promise<ActivityLog[]> {
        const token = localStorage.getItem('token')
        if (!token) throw new Error('No token found')

        const res = await api.get('/admin/logs', {
            headers: { Authorization: `Bearer ${token}` },
        })

        return res.data
    }

    static async getCollectionReports(): Promise<CollectionReport[]> {
        const token = localStorage.getItem('token')
        if (!token) throw new Error('No token found')

        const res = await api.get('/admin/reports/collections', {
            headers: { Authorization: `Bearer ${token}` },
        })

        return res.data
        }

        static async getUserReports(): Promise<UserReport[]> {
    const token = localStorage.getItem('token')
    if (!token) throw new Error('No token found')

    const res = await api.get('/admin/reports/users', {
        headers: { Authorization: `Bearer ${token}` },
    })

    return res.data
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
