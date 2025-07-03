import { redirect } from 'next/dist/server/api-utils'
import { NextResponse } from 'next/server'
import type { NextRequest } from 'next/server'

export function middleware(request: NextRequest) {
  const token = request.cookies.get('token')?.value
  const { pathname, searchParams, search, origin } = request.nextUrl
  const publicPaths = ['/login', '/register', '/register-confirmation', '/config/api']

  const isPublicPage = publicPaths.some(path => pathname.startsWith(path))

  if (
    pathname.startsWith('/teams/') ||
    pathname.startsWith('/flags/') ||
    pathname.startsWith('/_next/') ||
    pathname === '/favicon.ico'
  ) {
    return NextResponse.next()
  }

  if (!token && !isPublicPage) {
    const url = request.nextUrl.clone()
    url.pathname = '/login'
    url.search = `redirect=${pathname}${encodeURIComponent(search)}`
    
    return NextResponse.redirect(url)
  }
  return NextResponse.next()
}

export const config = {
  matcher: [
    '/((?!_next/static|_next/image|favicon.ico).*)',
  ],
}
