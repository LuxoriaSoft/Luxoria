import { Suspense } from 'react'
import { SSOAuthorizeContent } from './SSOAuthorizeContent'

export default function SSOAuthorizePage() {
  return (
    <Suspense fallback={<div>Loading authorization page...</div>}>
      <SSOAuthorizeContent />
    </Suspense>
  )
}
