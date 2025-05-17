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
      <div class="mb-4">
        <label for="avatar" class="block mb-2 text-sm font-medium">Avatar (optionnel)</label>
        <input
          type="file"
          id="avatar"
          @change="handleAvatarChange"
          accept="image/png, image/jpeg"
          class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white"
        />
      </div>
      <div v-if="avatarPreviewUrl" class="mb-4">
        <label class="block mb-2 text-sm font-medium">Aperçu de l’avatar</label>
        <img
          :src="avatarPreviewUrl"
          alt="Aperçu de l’avatar"
          class="w-24 h-24 rounded-full border border-gray-600 object-cover"
        />
      </div>
      <button type="submit" class="w-full bg-blue-600 hover:bg-blue-500 text-white font-bold py-2 px-4 rounded">
        Register
      </button>
      <p v-if="errorMessage" class="text-red-500 mt-4 text-sm">{{ errorMessage }}</p>

      <!-- Redirect to login -->
      <p class="text-sm text-gray-300 mt-4 text-center">
        Already have an account?
        <router-link to="/" class="text-blue-400 hover:underline">Log in.</router-link>
      </p>
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
      avatarFile: null,
      avatarPreviewUrl: null,
    };
  },
  methods: {
    async handleRegister() {
      const authService = new AuthService();

      try {
        await authService.register(this.username, this.email, this.password);
        const token = await authService.loginAndGetToken(this.username, this.password);

      if (this.avatarFile instanceof File) {
        const formData = new FormData();
        formData.append("file", this.avatarFile);

        await fetch("http://localhost:5269/auth/upload-avatar", {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
          },
          body: formData,
        });
      }

        alert("Account created!");
        this.$router.push("/");
      } catch (error) {
        this.errorMessage = error.message || "Registration failed.";
      }
    },

    handleAvatarChange(event) {
      const file = event.target.files[0];
      this.avatarFile = file;

      if (this.avatarPreviewUrl) {
        URL.revokeObjectURL(this.avatarPreviewUrl);
      }

      if (file) {
        this.avatarPreviewUrl = URL.createObjectURL(file);
      }
    },
  },
  beforeUnmount() {
    if (this.avatarPreviewUrl) {
      URL.revokeObjectURL(this.avatarPreviewUrl);
    }
  },
};
</script>

<style scoped>
/* Styles similar to Login.vue */
</style>
