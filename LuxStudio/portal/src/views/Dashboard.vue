<template>
  <div class="min-h-screen bg-gray-100 text-gray-800">
    <!-- Header Section -->
    <header class="bg-blue-600 text-white shadow">
      <div class="container mx-auto flex justify-between items-center py-4 px-6">
        <h1 class="text-2xl font-bold">Dashboard</h1>
        <button
          class="bg-white text-blue-600 font-semibold py-2 px-4 rounded hover:bg-gray-200"
          @click="logout"
        >
          Logout
        </button>
      </div>
    </header>

    <!-- Main Content Section -->
    <main class="container mx-auto py-8 px-6">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-bold mb-4">Card Title 1</h2>
          <p class="text-sm text-gray-600">This is a short description of the card content.</p>
          <button class="mt-4 bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-500">
            View Details
          </button>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-bold mb-4">Card Title 2</h2>
          <p class="text-sm text-gray-600">This is another card with different content.</p>
          <button class="mt-4 bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-500">
            View Details
          </button>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <h2 class="text-lg font-bold mb-4">Card Title 3</h2>
          <p class="text-sm text-gray-600">More content can go here for a third card.</p>
          <button class="mt-4 bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-500">
            View Details
          </button>
        </div>
      </div>
    </main>
  </div>
</template>

<script>
export default {
  methods: {
    /**
     * Logs out the user by removing the stored token
     * and redirecting to the login page.
     */
    logout() {
      const token = localStorage.getItem('token');
      if (token) {
        console.log('Logging out and clearing token...');
      }

      // Remove token from storage and redirect to login page
      localStorage.removeItem('token');
      this.$router.push('/'); // Redirection to login
    },
  },
};
</script>

<style scoped>
/* Scoped styles for the dashboard */
</style>
