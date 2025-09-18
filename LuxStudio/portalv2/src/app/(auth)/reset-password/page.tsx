'use client'

import { useState } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { LuxoriaLogo } from '@/components/LuxoriaLogo'
import { Text, Strong, TextLink } from '@/components/text'
import { Heading } from '@/components/heading'
import { Field, Label } from '@/components/fieldset'
import { Input } from '@/components/input'
import { Button } from '@/components/button'

export default function ResetPassword() {
  const searchParams = useSearchParams()
  const token = searchParams.get('token') || ''
  const decodedToken = decodeURIComponent(token)
  const router = useRouter()

  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)

const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault()
  setLoading(true)
  setMessage('')

  try {
    const res = await fetch('/api/auth/reset-password', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token: decodedToken, newPassword: password }),      
    })

    let data
    try {
      data = await res.json()
    } catch (jsonErr) {
      setMessage(`Invalid JSON response: ${jsonErr}`)
      setLoading(false)
      return
    }

    if (res.ok) {
      
      setMessage('Password reset successfully! You can now login.')
        setTimeout(() => {
          router.push('/login')
        }, 2000)
    } else {
      setMessage(data.error || JSON.stringify(data) || 'Error resetting password')
    }
  } catch (error) {
    setMessage(`Network error: ${error}`)
  } finally {
    setLoading(false)
  }
}


  return (
    <form onSubmit={handleSubmit} className="grid w-full max-w-sm grid-cols-1 gap-8">
      <LuxoriaLogo className="h-12 w-auto" />
      <Heading>Reset your password</Heading>
      <Field>
        <Label>New Password</Label>
        <Input
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          required
        />
      </Field>
      <Field>
        <Label>Confirm Password</Label>
        <Input
          type="password"
          value={confirmPassword}
          onChange={e => setConfirmPassword(e.target.value)}
          required
        />
      </Field>
      <Button type="submit" className="w-full" disabled={loading}>
        {loading ? 'Resetting...' : 'Reset Password'}
      </Button>
      {message && <Text>{message}</Text>}
      <Text>
        Remembered your password?{' '}
        <TextLink href="/login">
          <Strong>Sign in</Strong>
        </TextLink>
      </Text>
    </form>
  )
}
