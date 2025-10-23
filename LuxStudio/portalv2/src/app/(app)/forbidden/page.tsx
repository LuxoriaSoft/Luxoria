// src/pages/forbidden.tsx
import Link from 'next/link'

export default function ForbiddenPage() {
  return (
    <div className="flex flex-col items-center justify-center h-screen text-center p-6">
      <h1 className="text-4xl font-bold mb-4">403 - Accès interdit</h1>
      <p className="mb-6">Vous n'avez pas la permission d'accéder à cette page.</p>
    {/*  <Link href="/">
        <a className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
          Retour à l'accueil
        </a>
      </Link>*/}
    </div>
  )
}
