<template>
  <div class="max-w-md mx-auto p-6 bg-gray-800 text-white rounded-lg shadow-lg">
    <h1 class="text-2xl font-bold mb-6 text-center">Login</h1>
    <form @submit.prevent="handleLogin" class="space-y-4">
      <div>
        <label for="username" class="block text-sm font-medium mb-2">Username</label>
        <input 
          type="text" 
          id="username" 
          v-model="username" 
          class="w-full p-3 border border-gray-600 rounded bg-gray-700 text-white focus:ring focus:ring-blue-500"
          placeholder="Enter your username" 
          required
        />
      </div>
      <div>
        <label for="password" class="block text-sm font-medium mb-2">Password</label>
        <input 
          type="password" 
          id="password" 
          v-model="password" 
          class="w-full p-3 border border-gray-600 rounded bg-gray-700 text-white focus:ring focus:ring-blue-500"
          placeholder="Enter your password" 
          required
        />
      </div>
      <!-- hCaptcha Widget -->
      <div>
        <div ref="hcaptcha" class="h-captcha"></div>
      </div>
      <button 
        type="submit" 
        class="w-full bg-blue-600 hover:bg-blue-500 text-white font-bold py-3 px-4 rounded transition duration-300 disabled:opacity-50" 
        :disabled="!captchaToken"
      >
        Login
      </button>
      <p v-if="errorMessage" class="text-red-500 mt-4 text-sm">{{ errorMessage }}</p>
    </form>
  </div>
</template>

<script>
import { AuthService } from '../services/auth_services';

export default {
  data() {
    return {
      username: '',
      password: '',
      captchaToken: '', // Stores the hCaptcha token
      errorMessage: '',
    };
  },
  mounted() {
    this.$nextTick(() => {
      if (window.hcaptcha) {
        window.hcaptcha.render(this.$refs.hcaptcha, {
          sitekey: "3406257c-d7d0-4ca2-93ec-dc3cf6346ac4",
          callback: (token) => {
            this.captchaToken = token; // Stores the validated hCaptcha token
          }
        });
      }
    });
  },
  methods: {
    /**
     * Handles user login by calling the authentication service.
     * If successful, stores the JWT token and redirects the user.
     */
    async handleLogin() {
      const authService = new AuthService();
      try {
        const token = await authService.login(
          this.username,
          this.password,
          this.captchaToken
        );
        localStorage.setItem('token', token); // Stores the JWT token in localStorage
        const redirect = this.$route.query.redirect || '/dashboard';
        this.$router.push(redirect); // Redirects user after login
      } catch (error) {
        this.errorMessage = error.message || 'An unexpected error occurred.'; // Sets error message if login fails
      }
    },
  },
};
</script>
