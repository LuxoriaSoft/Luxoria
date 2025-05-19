import axios from 'axios';

/**
 * Authentication Service
 * This class handles user authentication by communicating with the backend API.
 */
export class AuthService {
  private readonly apiUrl: string;

  /**
   * Initializes the authentication service with the API base URL.
   */
  constructor() {
    this.apiUrl = window.appConfig.API_URL;
  }

  /**
   * Registers a new user by sending their details to the API.
   * @param username - The username of the new user.
   * @param email - The email address of the new user.
   * @param password - The password chosen by the new user.
   * @returns A success message if the registration is successful.
   * @throws An error message if the registration fails.
   */
  async register(username: string, email: string, password: string): Promise<string> {
    try {
      const response = await axios.post(`${this.apiUrl}/Auth/register`, {
        username,
        email,
        password,
      });

      if (response.status === 200) {
        return response.data; // Success message from the API
      } else {
        throw new Error("Registration failed.");
      }
    } catch (error: any) {
      throw new Error(error.response?.data || "An error occurred during registration.");
    }
  }

  /**
   * Logs in a user by sending their credentials to the API.
   * @param username - The username of the user.
   * @param password - The password of the user.
   * @param captchaToken - The CAPTCHA verification token.
   * @returns A JWT token if the login is successful.
   * @throws An error message if the login fails.
   */
  async login(username: string, password: string, captchaToken: string): Promise<string> {
    try {
      const response = await axios.post(`${this.apiUrl}/Auth/login`, {
        username,
        password,
        captchaToken,
      });

      if (response.status === 200) {
        return response.data.token; // Return the JWT token
      } else {
        throw new Error('Login failed.');
      }
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'An error occurred during login.');
    }
  }

  /**
   * Logs in without CAPTCHA and returns the JWT token.
   * Used during internal flows like uploading an avatar after registration.
   * @param username - The username.
   * @param password - The password.
   * @returns The JWT token.
   */
  async loginAndGetToken(username: string, password: string): Promise<string> {
    try {
      const response = await axios.post(`${this.apiUrl}/Auth/login`, {
        username,
        password,
        captchaToken: "", // Empty if not using CAPTCHA here
      });

      if (response.status === 200) {
        return response.data.token;
      } else {
        throw new Error("Login failed.");
      }
    } catch (error: any) {
      throw new Error(error.response?.data?.message || "An error occurred during login.");
    }
  }
}
