import { NextResponse } from 'next/server'

export async function GET() {
  const signalrUrl = process.env.NEXT_PUBLIC_SIGNALR_URL

  return NextResponse.json({
    url: signalrUrl,
  })
}
