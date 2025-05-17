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
      clientId: "fece71c8-afe9-4a35-bbed-267f6995f8f3", // Replace with your client ID
      redirectUri: window.appConfig.redirectUri, // Redirect URI from appConfig
      state: null, // Random state value for security
    };
  },
  methods: {
    /**
     * Redirects the user to the SSO authorization endpoint.
     * Ensures the user is authenticated before proceeding.
     */
    async redirectToSSO() {
      const token = localStorage.getItem("token"); // Retrieve JWT token from localStorage

      if (!token) {
        // If the user is not logged in, redirect to the login page
        this.$router.push({ path: "/", query: { redirect: this.$route.fullPath } });
        return;
      }

      // Construct the authorization URL without appending the JWT token in the query
      const authorizationUrl = `${window.appConfig.apiUrl}/sso/authorize?clientId=${this.clientId}&responseType=code&redirectUri=${encodeURIComponent(this.redirectUri)}&state=${this.state}`;

      try {
        const response = await fetch(authorizationUrl, {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`, // Send the token in the header
          },
        });

        if (response.ok) {
          const data = await response.json();
          if (data.redirectUrl) {
            window.location.href = data.redirectUrl; // Redirect to the API-provided URL
          }
        } else {
          console.error("Authorization failed", await response.json());
        }
      } catch (error) {
        console.error("Error during authorization request:", error);
      }
    },
    
    /**
     * Generates a random string for the "state" parameter to enhance security.
     * @returns {string} A unique random string.
     */
    generateState() {
      return Math.random().toString(36).substring(2) + Date.now().toString(36);
    },
  },
  created() {
    // Generate a random state value when the component is created
    this.state = this.generateState();
  },
  beforeRouteEnter(to, from, next) {
    // Ensure the user is logged in before allowing access to the page
    const token = localStorage.getItem("token");
    if (!token) {
      next({ path: "/", query: { redirect: to.fullPath } }); // Redirect to login page if not authenticated
    } else {
      next();
    }
  },
};
</script>

<style scoped>
/* Add custom styles if needed */
</style>
