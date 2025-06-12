<template>
  <div class="max-w-md mx-auto p-6 bg-gray-800 text-white rounded shadow text-center">
    <h1 class="text-2xl font-bold mb-4">Validation du compte</h1>

    <div v-if="loading">
      <p class="text-gray-300">⏳ Vérification en cours...</p>
    </div>

    <div v-else-if="success">
      <p class="text-green-400 font-medium text-lg mb-4">✅ Compte validé avec succès !</p>
      <p class="text-sm text-gray-400 mb-4">Redirection vers la page de connexion dans quelques secondes...</p>
      <router-link
        to="/"
        class="inline-block bg-blue-600 hover:bg-blue-500 text-white px-4 py-2 rounded mt-2 text-sm"
      >
        Revenir à l'accueil maintenant
      </router-link>
    </div>

    <div v-else>
      <p class="text-red-400 font-medium mb-2">❌ {{ errorMessage }}</p>
      <router-link
        to="/"
        class="inline-block bg-gray-700 hover:bg-gray-600 text-white px-4 py-2 rounded mt-4 text-sm"
      >
        Retour à l'accueil
      </router-link>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      email: "",
      success: false,
      errorMessage: "",
      loading: true,
    };
  },
  async created() {
    const id = this.$route.query.id;
    const code = this.$route.query.code;

    if (!id || !code) {
      this.loading = false;
      this.errorMessage = "Lien invalide ou incomplet.";
      return;
    }

    try {
      const resEmail = await fetch(`${window.appConfig.API_URL}/auth/user/${id}`);
      if (!resEmail.ok) throw new Error("Impossible de récupérer l'e-mail.");
      const data = await resEmail.json();
      this.email = data.email;

      const resVerify = await fetch(`${window.appConfig.API_URL}/auth/verify-code`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          email: this.email,
          code: code,
        }),
      });

      if (!resVerify.ok) {
        const err = await resVerify.json();
        throw new Error(err.message || "Code invalide.");
      }

      this.success = true;
      this.loading = false;

      // Redirige après 3 secondes
      setTimeout(() => {
        this.$router.push("/");
      }, 3000);
    } catch (err) {
      this.errorMessage = err.message || "Erreur lors de la validation.";
      this.loading = false;
    }
  },
};
</script>
