import { NextRequest, NextResponse } from 'next/server'

export async function POST(request: NextRequest) {
  console.log("API route /api/auth/forgot-password called");
  try {
    const { email } = await request.json();
    console.log("Email received:", email);

    const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auth/forgot-password`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email }),
    });

    const data = await response.json();
    console.log("Backend response:", data);
    return NextResponse.json(data, { status: response.status });
  } catch (error) {
    console.error("Error in API route:", error);
    return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
  }
}

