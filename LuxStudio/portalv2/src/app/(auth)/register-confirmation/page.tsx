'use client'
import { Suspense } from 'react'
import RegisterConfirmationClient from './RegisterConfirmationClient'

export default function RegisterConfirmationPage() {
  return (
    <Suspense fallback={<div className="text-center p-6">⏳ Chargement...</div>}>
      <RegisterConfirmationClient />
    </Suspense>
  )
}
