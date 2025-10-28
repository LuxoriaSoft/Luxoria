'use client'
import { useEffect, useState } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { Text, TextLink } from '@/components/text'
import { Heading } from '@/components/heading'
import { AuthService } from '@/services/auth'

export default function RegisterConfirmationClient() {
  const searchParams = useSearchParams()
  const router = useRouter()

  const [email, setEmail] = useState('')
  const [success, setSuccess] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const [loading, setLoading] = useState(true)
  const [isVerifying, setIsVerifying] = useState(false)

  useEffect(() => {
    const id = searchParams.get('id')
    const code = searchParams.get('code')

    const verify = async () => {
      if (isVerifying) return
      setIsVerifying(true)
      setLoading(true)
      setErrorMessage('')

      if (!id || !code) {
        setErrorMessage('Lien invalide ou incomplet.') 
        setLoading(false)
        setIsVerifying(false)
        return
      }

      try {
        const fetchedEmail = await AuthService.getUserEmailById(id)
        setEmail(fetchedEmail)

        await AuthService.verifyCode(fetchedEmail, code)

        setSuccess(true)
        setLoading(false)

        setTimeout(() => {
          window.location.href = '/login'
        }, 3000)
      } catch (err: any) {
        setErrorMessage(err.message || 'Erreur lors de la validation.') 
        setSuccess(false)
        setLoading(false)
      } finally {
        setIsVerifying(false)
      }
    }

    verify()
  }, [searchParams, router, isVerifying])

  return (
    <div className="max-w-md mx-auto p-6 bg-gray-800 text-white rounded shadow text-center">
      <Heading>Validation du compte</Heading>

      {loading && <Text className="text-gray-300">⏳ Vérification en cours...</Text>}

      {!loading && success && (
        <>
          <Text className="text-green-400 font-medium text-lg mb-4">✅ Compte validé avec succès !</Text>
          <Text className="text-sm text-gray-400 mb-4">
            Redirection vers la page de connexion dans quelques secondes...
          </Text>
          <TextLink
            href="/login"
            className="inline-block bg-blue-600 hover:bg-blue-500 text-white px-4 py-2 rounded mt-2 text-sm"
          >
            Revenir à l&apos;accueil maintenant
          </TextLink>
        </>
      )}

      {!loading && !success && (
        <>
          <Text className="text-red-400 font-medium mb-2">❌ {errorMessage}</Text>
          <TextLink
            href="/login"
            className="inline-block bg-gray-700 hover:bg-gray-600 text-white px-4 py-2 rounded mt-4 text-sm"
          >
            Retour à l&apos;accueil
          </TextLink>
        </>
      )}
    </div>
  )
}
