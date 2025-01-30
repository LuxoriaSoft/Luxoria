import axios from 'axios';

export class AuthService {
  private readonly apiUrl: string;

  constructor() {
    this.apiUrl = 'http://localhost:5269'; // Replace with your API URL
  }

  async refreshToken(): Promise<{ token: string; refreshToken: string } | null> {
    const refreshToken = localStorage.getItem("refreshToken");

    if (!refreshToken) {
      console.error("No refresh token found.");
      return null;
    }

    try {
      const response = await axios.post(`${this.apiUrl}/sso/refresh`, {
        refreshToken,
      });

      if (response.status === 200) {
        console.log("Token refreshed:", response.data);
        localStorage.setItem("token", response.data.access_token);
        localStorage.setItem("refreshToken", response.data.refresh_token);
        return {
          token: response.data.access_token,
          refreshToken: response.data.refresh_token,
        };
      } else {
        throw new Error("Failed to refresh token.");
      }
    } catch (error: any) {
      console.error("Error refreshing token:", error.response?.data?.error || error.message);
      return null;
    }
  }

  async register(username: string, email: string, password: string): Promise<string> {
    try {
      const response = await axios.post(`${this.apiUrl}/Auth/register`, {
        username,
        email,
        password,
      });
  
      if (response.status === 200) {
        return response.data; // Message de succès
      } else {
        throw new Error("Registration failed.");
      }
    } catch (error: any) {
      throw new Error(error.response?.data || "An error occurred during registration.");
    }
  }
  

  async login(username: string, password: string, captchaToken: string): Promise<string> {
    try {
      const response = await axios.post(`${this.apiUrl}/Auth/login`, {
        username,
        password,
        captchaToken
      });

      if (response.status === 200) {
        return response.data.token;
      } else {
        throw new Error('Login failed.');
      }
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'An error occurred during login.');
    }
  }
}

