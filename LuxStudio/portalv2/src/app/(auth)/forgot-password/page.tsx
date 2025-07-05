'use client'

import { useState } from 'react'
import { LuxoriaLogo } from '@/components/LuxoriaLogo'
import { Text, Strong, TextLink } from '@/components/text'
import { Heading } from '@/components/heading'
import { Field, Label } from '@/components/fieldset'
import { Input } from '@/components/input'
import { Button } from '@/components/button'

export default function ForgotPassword() {
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setMessage('')
    try {
      const res = await fetch('/api/auth/forgot-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email }),
      })

      const data = await res.json()
      if (res.ok) {
        setMessage(data.message || 'Check your email for reset instructions')
      } else {
        setMessage(data.error || 'Error sending reset email')
      }
    } catch {
      setMessage('Error sending reset email')
    } finally {
      setLoading(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="grid w-full max-w-sm grid-cols-1 gap-8">
      <LuxoriaLogo className="h-12 w-auto" />
      <Heading>Reset your password</Heading>
      <Text>Enter your email and we’ll send you a link to reset your password.</Text>
      <Field>
        <Label>Email</Label>
        <Input
          type="email"
          name="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
      </Field>
      <Button type="submit" className="w-full" disabled={loading}>
        {loading ? 'Sending...' : 'Reset password'}
      </Button>
      {message && <Text>{message}</Text>}
      <Text>
        Don’t have an account?{' '}
        <TextLink href="/register">
          <Strong>Sign up</Strong>
        </TextLink>
      </Text>
    </form>
  )
}
