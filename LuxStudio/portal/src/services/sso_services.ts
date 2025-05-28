import axios from 'axios';

/**
 * Handles Single Sign-On (SSO) logic.
 */
export class SSOService {
  private readonly apiUrl: string;

  constructor() {
    this.apiUrl = window.appConfig.API_URL;
  }

  /**
   * Builds the authorization URL for initiating the SSO process.
   */
  buildAuthorizationUrl(
    clientId: string,
    redirectUri: string,
    state: string,
    responseType: string = 'code'
  ): string {
    if (!clientId || !redirectUri || !state) {
      throw new Error('Missing required parameters to build the authorization URL.');
    }

    return `${this.apiUrl}/sso/authorize?clientId=${encodeURIComponent(
      clientId
    )}&responseType=${encodeURIComponent(responseType)}&redirectUri=${encodeURIComponent(
      redirectUri
    )}&state=${encodeURIComponent(state)}`;
  }

  /**
   * Sends a request to the SSO endpoint to get a redirect URL.
   */
  async authorize(authUrl: string, token: string): Promise<string> {
    if (!token) {
      throw new Error('Missing token for authorization.');
    }

    const response = await axios.get(authUrl, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (response.status === 200 && response.data.redirectUrl) {
      return response.data.redirectUrl;
    } else {
      throw new Error('SSO authorization failed.');
    }
  }
}
