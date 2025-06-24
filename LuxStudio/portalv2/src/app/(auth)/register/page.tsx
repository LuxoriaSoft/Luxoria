'use client'

import { useState, useRef, useEffect } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Logo } from '@/app/logo'
import { Button } from '@/components/button'
import { Field, Label } from '@/components/fieldset'
import { Heading } from '@/components/heading'
import { Input } from '@/components/input'
import { Strong, Text, TextLink } from '@/components/text'
import { AuthService } from '@/services/auth'

export default function Register() {
  const router = useRouter()
  const searchParams = useSearchParams()

  const [step, setStep] = useState<'form' | 'code'>('form')
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [avatarFile, setAvatarFile] = useState<File | null>(null)
  const [avatarPreviewUrl, setAvatarPreviewUrl] = useState<string | null>(null)
  const [verificationCode, setVerificationCode] = useState('')
  const [errorMessage, setErrorMessage] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const fileInputRef = useRef<HTMLInputElement>(null)
  const [role, setRole] = useState<0 | 1>(0)


  useEffect(() => {
    const queryEmail = searchParams.get('email')
    if (searchParams.get('prefilled') === 'true' && queryEmail) {
      setEmail(queryEmail)
    }
  }, [searchParams])

  useEffect(() => {
    return () => {
      if (avatarPreviewUrl) URL.revokeObjectURL(avatarPreviewUrl)
    }
  }, [avatarPreviewUrl])

  const handleAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    setAvatarFile(file)
    const preview = URL.createObjectURL(file)
    setAvatarPreviewUrl(preview)
  }

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault()
    if (isSubmitting) return
    setIsSubmitting(true)
    setErrorMessage('')
    try {
      await AuthService.requestVerification(username, email, password, role)
      setStep('code')
    } catch (err: any) {
      setErrorMessage(err.message || 'Unexpected error')
    } finally {
      setIsSubmitting(false)
    }
  }

  const verifyCode = async (e: React.FormEvent) => {
    e.preventDefault()
    if (isSubmitting) return
    setIsSubmitting(true)
    setErrorMessage('')
    try {
      if (verificationCode.length !== 6) throw new Error('Code must be 6 digits')

      await AuthService.verifyCode(email, verificationCode)

      const token = await AuthService.login(email, password, '') // Pas de captcha ici
      localStorage.setItem('token', token)

      if (avatarFile) {
        await AuthService.uploadAvatar(avatarFile, token)
      }

      router.push('/dashboard')
    } catch (err: any) {
      setErrorMessage(err.message || 'Error during verification')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={step === 'form' ? handleRegister : verifyCode} className="grid w-full max-w-sm grid-cols-1 gap-8">
      <Logo className="h-6 text-zinc-950 dark:text-white forced-colors:text-[CanvasText]" />
      <Heading>{step === 'form' ? 'Create your account' : 'Enter verification code'}</Heading>

      {step === 'form' && (
        <>
          <Field>
            <Label>Username</Label>
            <Input value={username} onChange={(e) => setUsername(e.target.value)} required />
          </Field>
          <Field>
            <Label>Email</Label>
            <Input value={email} onChange={(e) => setEmail(e.target.value)} required />
          </Field>
          <Field>
            <Label>Password</Label>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </Field>
          <Field>
            <Label>Avatar (optional)</Label>
            <Input
              type="file"
              ref={fileInputRef}
              onChange={handleAvatarChange}
              accept="image/png, image/jpeg"
            />
          </Field>
          {avatarPreviewUrl && (
            <img
              src={avatarPreviewUrl}
              alt="Avatar preview"
              className="w-20 h-20 rounded-full object-cover border"
            />
          )}
          <Field>
            <Label>Account Type</Label>
            <select
              value={role}
              onChange={(e) => setRole(Number(e.target.value) as 0 | 1)}
              className="w-full rounded border p-2"
            >
              <option value={0}>Client</option>
              <option value={1}>Photographer</option>
            </select>
          </Field>
        </>
      )}

      {step === 'code' && (
        <Field>
          <Label>Verification Code</Label>
          <Input
            value={verificationCode}
            onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
            required
            maxLength={6}
          />
        </Field>
      )}

      {errorMessage && <Text className="text-red-500 text-sm">{errorMessage}</Text>}
      {successMessage && <Text className="text-green-500 text-sm">{successMessage}</Text>}
      <Button type="submit" className="w-full" disabled={isSubmitting}>
        {isSubmitting ? 'Please wait...' : step === 'form' ? 'Register' : 'Confirm Code'}
      </Button>

      <Text>
        Already have an account?{' '}
        <TextLink href="/login">
          <Strong>Log in</Strong>
        </TextLink>
      </Text>
    </form>
  )
}
