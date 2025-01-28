<template>
  <div class="flex flex-col items-center justify-center min-h-screen bg-gray-100">
    <div class="bg-white shadow-lg rounded-lg p-6 w-96 text-center">
      <h2 class="text-2xl font-bold mb-4">Link Your Account</h2>
      <p class="text-gray-600 mb-6">
        Connect your account using our SSO system.
      </p>
      <button @click="redirectToSSO" class="bg-blue-600 text-white font-bold py-2 px-4 rounded hover:bg-blue-500">
        Link Account
      </button>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      clientId: "fece71c8-afe9-4a35-bbed-267f6995f8f3", // Remplacez par votre client ID
      redirectUri: "http://localhost:5678", // URL de redirection après succès
      state: null, // Valeur aléatoire pour la sécurité
    };
  },
  methods: {
    async redirectToSSO() {
      const token = localStorage.getItem("token"); // Récupérer le token JWT depuis le localStorage

      if (!token) {
        // Si l'utilisateur n'est pas connecté, redirigez vers la page de connexion
        this.$router.push({ path: "/", query: { redirect: this.$route.fullPath } });
        return;
      }

      // Construire l'URL d'autorisation sans le token JWT dans la query
      const authorizationUrl = `http://localhost:5269/sso/authorize?clientId=${this.clientId}&responseType=code&redirectUri=${encodeURIComponent(this.redirectUri)}&state=${this.state}`;

      try {
        const response = await fetch(authorizationUrl, {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`, // Envoyer le token dans l'en-tête
          },
        });

        if (response.ok) {
          const data = await response.json();
          if (data.redirectUrl) {
            window.location.href = data.redirectUrl; // Se téléporter vers l'URL renvoyée par l'API
          }
        } else {
          console.error("Authorization failed", await response.json());
        }
      } catch (error) {
        console.error("Error during authorization request:", error);
      }
    },
    generateState() {
      // Générer une chaîne aléatoire pour le paramètre "state"
      return Math.random().toString(36).substring(2) + Date.now().toString(36);
    },
  },
  created() {
    // Générer un état aléatoire lors de la création du composant
    this.state = this.generateState();
  },
  beforeRouteEnter(to, from, next) {
    // Vérifier si l'utilisateur est connecté avant d'accéder à la page
    const token = localStorage.getItem("token");
    if (!token) {
      next({ path: "/", query: { redirect: to.fullPath } }); // Rediriger vers la page de connexion si non connecté
    } else {
      next();
    }
  },
};
</script>

<style scoped>
/* Ajoutez des styles personnalisés si nécessaire */
</style>