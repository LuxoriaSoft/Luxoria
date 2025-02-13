<template>
  <div class="max-w-md mx-auto p-6 bg-gray-800 text-white rounded shadow">
    <h1 class="text-2xl font-bold mb-6 text-center">Register</h1>
    <form @submit.prevent="handleRegister">
      <div class="mb-4">
        <label for="username" class="block mb-2 text-sm font-medium">Username</label>
        <input
          type="text"
          id="username"
          v-model="username"
          class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white"
          placeholder="Enter your username"
          required
        />
      </div>
      <div class="mb-4">
        <label for="email" class="block mb-2 text-sm font-medium">Email</label>
        <input
          type="email"
          id="email"
          v-model="email"
          class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white"
          placeholder="Enter your email"
          required
        />
      </div>
      <div class="mb-4">
        <label for="password" class="block mb-2 text-sm font-medium">Password</label>
        <input
          type="password"
          id="password"
          v-model="password"
          class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white"
          placeholder="Enter your password"
          required
        />
      </div>
      <button type="submit" class="w-full bg-blue-600 hover:bg-blue-500 text-white font-bold py-2 px-4 rounded">
        Register
      </button>
      <p v-if="errorMessage" class="text-red-500 mt-4 text-sm">{{ errorMessage }}</p>
    </form>
  </div>
</template>

<script>
import { AuthService } from "../services/auth_services";

export default {
  data() {
    return {
      username: "",
      email: "",
      password: "",
      errorMessage: "",
    };
  },
  methods: {
    /**
     * Handles user registration by calling the authentication service.
     * On success, redirects to the login page. On failure, displays an error message.
     */
    async handleRegister() {
      const authService = new AuthService();
      try {
        const message = await authService.register(this.username, this.email, this.password);
        alert(message); // Display success message
        this.$router.push("/"); // Redirect to login page
      } catch (error) {
        this.errorMessage = error.message; // Set error message on failure
      }
    },
  },
};
</script>

<style scoped>
/* Styles similar to Login.vue */
</style>
