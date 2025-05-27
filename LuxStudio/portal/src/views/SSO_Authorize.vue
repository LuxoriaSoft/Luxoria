<template>
  <div class="flex flex-col items-center justify-center min-h-screen bg-gray-100">
    <div class="bg-white shadow-lg rounded-lg p-6 w-96 text-center">
      <h2 class="text-2xl font-bold mb-4">Link Your Account</h2>
      <p class="text-gray-600 mb-2">
        You're about to link your account to this app:
      </p>
      <p class="text-blue-600 font-mono break-words mb-6">
        {{ clientId }}
      </p>
      <button
        @click="redirectToSSO"
        class="bg-blue-600 text-white font-bold py-2 px-4 rounded hover:bg-blue-500"
      >
        Link Account
      </button>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { SSOService } from '../services/sso_services';

export default defineComponent({
  data() {
    return {
      state: '',
    };
  },
  methods: {
    getQueryParam(name: string): string {
      const value = this.$route.query[name];
      if (Array.isArray(value)) return value[0] ?? '';
      return value ?? '';
    },

    generateState(): string {
      return Math.random().toString(36).substring(2) + Date.now().toString(36);
    },

    async redirectToSSO() {
      const token = localStorage.getItem('token');
      if (!token) {
        this.$router.push({
          path: '/',
          query: { redirect: this.$route.fullPath },
        });
        return;
      }

      const clientId = this.getQueryParam('clientId');
      const redirectUri = this.getQueryParam('redirectUri');
      const responseType = this.getQueryParam('responseType') || 'code';

      if (!clientId || !redirectUri) {
        alert('Missing required query parameters.');
        return;
      }

      try {
        const sso = new SSOService();
        const authUrl = sso.buildAuthorizationUrl(
          clientId,
          redirectUri,
          this.state,
          responseType
        );

        const redirectUrl = await sso.authorize(authUrl, token);
        window.location.href = redirectUrl;
      } catch (error: any) {
        console.error('SSO error:', error.message);
        alert(error.message || 'An error occurred during SSO.');
      }
    },
  },
  computed: {
    clientId(): string {
      return this.getQueryParam('clientId');
    },
  },
  created() {
    this.state = this.generateState();
  },
  beforeRouteEnter(to, _from, next) {
    const token = localStorage.getItem('token');
    if (!token) {
      next({ path: '/', query: { redirect: to.fullPath } });
    } else {
      next();
    }
  },
});
</script>

<style scoped>
.break-words {
  word-break: break-word;
}
</style>
