import RegisterClient from './RegisterClient'
import { Suspense } from 'react'

export default function RegisterPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <RegisterClient />
    </Suspense>
  )
}
