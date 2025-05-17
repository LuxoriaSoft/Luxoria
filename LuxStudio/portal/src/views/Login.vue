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

      <!-- Redirect to register -->
      <p class="text-sm text-gray-300 mt-4 text-center">
        No account?
        <router-link to="/register" class="text-blue-400 hover:underline">Sign up here.</router-link>
      </p>
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
      captchaToken: '',
      errorMessage: '',
    };
  },
  mounted() {
    this.$nextTick(() => {
      if (window.hcaptcha) {
        window.hcaptcha.render(this.$refs.hcaptcha, {
          sitekey: "3406257c-d7d0-4ca2-93ec-dc3cf6346ac4",
          callback: (token) => {
            this.captchaToken = token;
          }
        });
      }
    });
  },
  methods: {
    async handleLogin() {
      const authService = new AuthService();
      try {
        const token = await authService.login(
          this.username,
          this.password,
          this.captchaToken
        );
        localStorage.setItem('token', token);
        const redirect = this.$route.query.redirect || '/dashboard';
        this.$router.push(redirect);
      } catch (error) {
        this.errorMessage = error.message || 'An unexpected error occurred.';
      }
    },
  },
};
</script>
