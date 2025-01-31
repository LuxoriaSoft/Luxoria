<template>
  <div class="login-container">
    <h1>Login</h1>
    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <label for="username">Username</label>
        <input 
          type="text" 
          id="username" 
          v-model="username" 
          placeholder="Enter your username" 
          required
        />
      </div>
      <div class="form-group">
        <label for="password">Password</label>
        <input 
          type="password" 
          id="password" 
          v-model="password" 
          placeholder="Enter your password" 
          required
        />
      </div>
      <!-- hCaptcha Widget -->
      <div class="form-group">
        <div ref="hcaptcha" class="h-captcha"></div>
      </div>
      <button type="submit" class="btn" :disabled="!captchaToken">Login</button>
      <p v-if="errorMessage" class="error-message">{{ errorMessage }}</p>
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
      captchaToken: '', // Stocke le token hCaptcha
      errorMessage: '',
    };
  },
  mounted() {
    this.$nextTick(() => {
      if (window.hcaptcha) {
        window.hcaptcha.render(this.$refs.hcaptcha, {
          sitekey: "3406257c-d7d0-4ca2-93ec-dc3cf6346ac4",
          callback: (token) => {
            this.captchaToken = token; // Stocke le token validé par hCaptcha
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
        localStorage.setItem('token', token); // Stocke le token dans localStorage
        const redirect = this.$route.query.redirect || '/dashboard';
        this.$router.push(redirect);
      } catch (error) {
        this.errorMessage = error.message || 'An unexpected error occurred.';
      }
    },
  },
};
</script>

<style scoped>
.login-container {
  max-width: 400px;
  margin: 0 auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 5px;
  background-color: #f9f9f9;
}

h1 {
  text-align: center;
  margin-bottom: 20px;
}

.form-group {
  margin-bottom: 15px;
}

label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

input {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  box-sizing: border-box;
}

button {
  width: 100%;
  padding: 10px;
  background-color: #007BFF;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

button:hover {
  background-color: #0056b3;
}

.error-message {
  color: red;
  margin-top: 10px;
}
</style>
