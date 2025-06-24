'use client'

import { useEffect, useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Button } from '@/components/button'
import { Text } from '@/components/text'
import { Heading } from '@/components/heading'
import { SSOService } from '@/services/sso.services'

export default function SSOAuthorize() {
  const router = useRouter()
  const params = useSearchParams()
  const [state, setState] = useState('')
  const [clientId, setClientId] = useState('')
  const [redirectUri, setRedirectUri] = useState('')
  const [responseType, setResponseType] = useState('code')

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (!token) {
      router.push(`/?redirect=${window.location.pathname + window.location.search}`)
      return
    }

    const cid = params.get('clientId') || ''
    const redir = params.get('redirectUri') || ''
    const resp = params.get('responseType') || 'code'

    setClientId(cid)
    setRedirectUri(redir)
    setResponseType(resp)
    setState(generateState()) // <-- après les autres
  }, [params, router])

  const generateState = (): string => {
    return Math.random().toString(36).substring(2) + Date.now().toString(36)
  }

  const redirectToSSO = async () => {
    const token = localStorage.getItem('token')
    if (!token || !clientId || !redirectUri) {
      alert('Missing required parameters or token.')
      return
    }

    try {
      const sso = new SSOService()
      const authUrl = sso.buildAuthorizationUrl(clientId, redirectUri, state, responseType)
      const finalRedirect = await sso.authorize(authUrl, token)
      window.location.href = finalRedirect
    } catch (err: any) {
      console.error('SSO error:', err.message)
      alert(err.message || 'An error occurred during SSO.')
    }
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100 dark:bg-zinc-900">
      <div className="bg-white dark:bg-zinc-800 shadow-lg rounded-lg p-6 w-full max-w-md text-center">
        <Heading className="text-xl font-bold mb-4">Link Your Account</Heading>
        <Text className="text-gray-600 mb-2 dark:text-gray-300">
          You're about to link your account to this app:
        </Text>
        <p className="text-blue-600 font-mono break-words mb-6">{clientId}</p>
        <Button onClick={redirectToSSO}>Link Account</Button>
      </div>
    </div>
  )
}
