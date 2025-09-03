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
    id: string
    action: string
    performedBy: string
    details: string
    timestamp: string
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
    // === Users ===
    static async getUsers(search?: string): Promise<User[]> {
        const res = await api.get(`/admin/users${search ? `?search=${encodeURIComponent(search)}` : ''}`)
        return res.data
    }

    static async resetPassword(userId: string): Promise<void> {
        await api.post(`/admin/users/reset-password/${userId}`)
    }

    static async blockUser(userId: string): Promise<void> {
        await api.post(`/admin/users/block/${userId}`)
    }

    static async unblockUser(userId: string): Promise<void> {
        await api.post(`/admin/users/unblock/${userId}`)
    }

    static async inviteUser(email: string, role: number): Promise<void> {
        const res = await api.post('/admin/users/invite', { email, role })
        if (res.status < 200 || res.status >= 300) {
            throw new Error(res.data?.message || 'Failed to invite user')
        }
    }

    // === Reports ===
    static async deleteUserReport(reportId: string): Promise<void> {
        await api.delete(`/admin/reports/users/${reportId}`)
    }

    static async getActivityLogs(): Promise<ActivityLog[]> {
        const res = await api.get('/admin/logs')
        return res.data
    }

    static async getCollectionReports(): Promise<CollectionReport[]> {
        const res = await api.get('/admin/reports/collections')
        return res.data
    }

    static async getUserReports(): Promise<UserReport[]> {
        const res = await api.get('/admin/reports/users')
        return res.data
    }

    // === Collections ===
    static async getCollections(search?: string): Promise<AdminCollection[]> {
        const res = await api.get(`/admin/collections${search ? `?search=${encodeURIComponent(search)}` : ''}`)
        return res.data
    }

    static async deleteCollection(collectionId: string): Promise<void> {
        await api.delete(`/admin/collections/${collectionId}`)
    }
}
