<template>
  <div class="max-w-md mx-auto p-6 bg-gray-800 text-white rounded shadow">
    <h1 class="text-2xl font-bold mb-6 text-center">Register</h1>

    <!-- ÉTAPE 1 : formulaire -->
    <form v-if="step === 'form'" @submit.prevent="handleRegister">
      <!-- Champs username/email/password/avatar identiques -->
      <div class="mb-4">
        <label for="username" class="block mb-2 text-sm font-medium">Username</label>
        <input type="text" id="username" v-model="username" class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white" required />
      </div>
      <div class="mb-4">
        <label for="email" class="block mb-2 text-sm font-medium">Email</label>
        <input type="email" id="email" v-model="email" class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white" required />
      </div>
      <div class="mb-4">
        <label for="password" class="block mb-2 text-sm font-medium">Password</label>
        <input type="password" id="password" v-model="password" class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white" required />
      </div>
      <div class="mb-4">
        <label for="avatar" class="block mb-2 text-sm font-medium">Avatar (optionnel)</label>
        <input type="file" id="avatar" @change="handleAvatarChange" accept="image/png, image/jpeg" class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white" />
      </div>
      <div v-if="avatarPreviewUrl" class="mb-4">
        <label class="block mb-2 text-sm font-medium">Aperçu de l’avatar</label>
        <img :src="avatarPreviewUrl" class="w-24 h-24 rounded-full border border-gray-600 object-cover" />
      </div>
      <button type="submit" class="w-full bg-blue-600 hover:bg-blue-500 text-white font-bold py-2 px-4 rounded">
        Register
      </button>
      <p v-if="errorMessage" class="text-red-500 mt-4 text-sm">{{ errorMessage }}</p>
      <p v-if="successMessage" class="text-green-500 mt-4 text-sm">{{ successMessage }}</p>
      <p class="text-sm text-gray-300 mt-4 text-center">
        Already have an account?
        <router-link to="/" class="text-blue-400 hover:underline">Log in.</router-link>
      </p>
    </form>

    <!-- ÉTAPE 2 : vérification du code -->
    <form v-else-if="step === 'code'" @submit.prevent="verifyCode">
      <div class="mb-4">
        <label for="code" class="block mb-2 text-sm font-medium">Code de vérification</label>
        <input
          type="text"
          id="code"
          v-model="verificationCode"
          @input="onCodeInput"
          maxlength="6"
          inputmode="numeric"
          class="w-full p-2 border border-gray-600 rounded bg-gray-700 text-white"
          placeholder="Ex: 123456"
          required
        />
      </div>
      <button type="submit" class="w-full bg-green-600 hover:bg-green-500 text-white font-bold py-2 px-4 rounded">
        Valider le code
      </button>
      <button type="button" @click="resendCode" class="w-full mt-3 bg-yellow-600 hover:bg-yellow-500 text-white font-bold py-2 px-4 rounded">
        Renvoyer le code
      </button>
      <p v-if="errorMessage" class="text-red-500 mt-4 text-sm">{{ errorMessage }}</p>
      <p v-if="successMessage" class="text-green-500 mt-4 text-sm">{{ successMessage }}</p>
      <!-- Toast Notification -->
      <div v-if="toastMessage" :class="toastClass" class="toast toast-start toast-top z-50 fixed m-4">
        <div class="alert" :class="toastType">
          <span>{{ toastMessage }}</span>
        </div>
      </div>
    </form>
  </div>
</template>

<script>
import { AuthService } from "../services/auth_services";

export default {
  data() {
    return {
      step: "form",
      username: "",
      email: "",
      password: "",
      verificationCode: "",
      avatarFile: null,
      avatarPreviewUrl: null,
      errorMessage: "",
      successMessage: "",
      toastMessage: "",
      toastType: "alert-info",
      toastClass: "animate-fadeIn",
    };
  },
  methods: {
    showToast(message, type = "alert-info", duration = 3000) {
      this.toastMessage = message;
      this.toastType = type;
      setTimeout(() => {
        this.toastMessage = "";
      }, duration);
    },
    async handleRegister() {
      try {
        if (!this.username || !this.email || !this.password) {
          this.errorMessage = "Tous les champs sont obligatoires.";
          return;
        }

        const payload = {
          username: this.username,
          email: this.email,
          password: this.password,
        };

        console.log("Payload envoyé:", payload); // debug

        const res = await fetch("http://localhost:5269/auth/request-verification", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload), // ✅ JSON stringifié ici
        });

        if (!res.ok) {
          const text = await res.text();
          let data;
          try {
            data = JSON.parse(text);
          } catch (e) {
            data = { message: text };
          }
          throw new Error(data.message || "Erreur lors de la demande.");
        }

        this.step = "code";
        this.errorMessage = "";
        this.successMessage = "";
      } catch (error) {
        this.errorMessage = error.message || "Échec de l'envoi du code.";
      }
    },

    async resendCode() {
      try {
        const res = await fetch("http://localhost:5269/auth/request-verification", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            username: this.username,
            email: this.email,
            password: this.password,
          }),
        });

        if (!res.ok) {
          const data = await res.json();
          throw new Error(data.message || "Erreur lors du renvoi.");
        }

        this.successMessage = "Code renvoyé avec succès.";
        this.errorMessage = "";
      } catch (error) {
        this.errorMessage = error.message || "Erreur lors du renvoi.";
        this.successMessage = "";
      }
    },

    async verifyCode() {
      try {
        if (this.verificationCode.length !== 6) {
          this.errorMessage = "Le code doit contenir 6 chiffres.";
          return;
        }

        const res = await fetch("http://localhost:5269/auth/verify-code", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            email: this.email,
            code: this.verificationCode,
          }),
        });

        const text = await res.text();
        let data;
        try {
          data = JSON.parse(text);
        } catch {
          data = { message: text };
        }

        if (!res.ok) throw new Error(data.message || "Code incorrect.");


        const authService = new AuthService();
        const token = await authService.loginAndGetToken(this.username, this.password);

        if (this.avatarFile instanceof File) {
          const formData = new FormData();
          formData.append("file", this.avatarFile);

          await fetch("http://localhost:5269/auth/upload-avatar", {
            method: "POST",
            headers: { Authorization: `Bearer ${token}` },
            body: formData,
          });
        }

        this.showToast("Compte créé avec succès 🎉", "alert-success", 3000);
        this.$router.push("/");
      } catch (error) {
        this.showToast(error.message || "Échec de vérification.", "alert-error", 5000);
        this.successMessage = "";
        this.errorMessage = "";
      }
    },

    onCodeInput() {
      this.verificationCode = this.verificationCode.replace(/\D/g, "").slice(0, 6);
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
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fadeIn {
  animation: fadeIn 0.3s ease-out;
}
/* Styles similaires à Login.vue */
</style>
