export class SSOService {
  buildAuthorizationUrl(
    clientId: string,
    redirectUri: string,
    state: string,
    responseType: string = 'code'
  ): string {
    const base = process.env.NEXT_PUBLIC_SIGNALR_URL || '' // ex: http://localhost:5269
    const params = new URLSearchParams({
      clientId,
      redirectUri,
      responseType,
      state,
    })

    return `${base}/SSO/authorize?${params.toString()}`
  }

  async authorize(authUrl: string, token: string): Promise<string> {
    const response = await fetch(authUrl, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })

    const text = await response.text()

    if (!response.ok) {
      let error = 'SSO authorization failed'
      try {
        const parsed = JSON.parse(text)
        error = parsed.error || error
      } catch {
        error += `: ${text}`
      }
      throw new Error(error)
    }

    try {
      const data = JSON.parse(text)
      return data.redirectUrl
    } catch {
      throw new Error('Invalid response from server during SSO.')
    }
  }
}
