'use client'

import { useRef, useState, useEffect } from 'react'
import { redirect, useRouter, useSearchParams } from 'next/navigation'
import { Button } from '@/components/button'
import { Checkbox, CheckboxField } from '@/components/checkbox'
import { Field, Label } from '@/components/fieldset'
import { Heading } from '@/components/heading'
import { Input } from '@/components/input'
import { Strong, Text, TextLink } from '@/components/text'
import { Logo } from '@/app/logo'
import { AuthService } from '@/services/auth'

export default function LoginPage() {
  const router = useRouter()
  const searchParams = useSearchParams()

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [captchaToken, setCaptchaToken] = useState('')
  const [errorMessage, setErrorMessage] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const captchaRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (window.hcaptcha && captchaRef.current) {
      window.hcaptcha.render(captchaRef.current, {
        sitekey: '3406257c-d7d0-4ca2-93ec-dc3cf6346ac4',
        callback: (token: string) => setCaptchaToken(token),
      })
    }
  }, [])

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    setErrorMessage('')
    setIsLoading(true)

    try {
      const token = await AuthService.login(username, password, captchaToken)
      localStorage.setItem('token', token)
      document.cookie = `token=${token}; path=/`

      const query = window.location.search
    
      const params = new URLSearchParams(query)
      let redirectUrl = params.get('redirect')
      router.push(redirectUrl || '/')
    } catch (error: any) {
      setErrorMessage(error.message || 'An unexpected error occurred.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-sm space-y-8">
        <div className="flex justify-center">
          <Logo className="h-6 text-zinc-950 dark:text-white forced-colors:text-[CanvasText]" />
        </div>
        <Heading level={1} className="text-center">
          Sign in to your account
        </Heading>
        <form onSubmit={handleLogin} className="space-y-6">
          <Field>
            <Label>Email</Label>
            <Input
              type="email"
              name="email"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </Field>
          <Field>
            <Label>Password</Label>
            <Input
              type="password"
              name="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </Field>
          <div ref={captchaRef} className="h-captcha"></div>
          <div className="flex items-center justify-between">
            <CheckboxField>
              <Checkbox name="remember" />
              <Label>Remember me</Label>
            </CheckboxField>
            <Text>
              <TextLink href="/forgot-password">
                <Strong>Forgot password?</Strong>
              </TextLink>
            </Text>
          </div>
          <Button type="submit" className="w-full" disabled={!captchaToken || isLoading}>
            {isLoading ? 'Logging in...' : 'Login'}
          </Button>
          {errorMessage && (
            <Text className="text-sm text-red-500">{errorMessage}</Text>
          )}
          <Text className="text-center">
            Don’t have an account?{' '}
            <TextLink href="/register">
              <Strong>Sign up</Strong>
            </TextLink>
          </Text>
        </form>
      </div>
    </div>
  )
}
