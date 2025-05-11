<template>
  <div class="min-h-screen bg-base-200">
    <div class="navbar bg-base-100 shadow">
      <div class="flex-1">
        <a class="btn btn-ghost text-xl">LuxStudio</a>
      </div>
      <div class="flex-none">
        <button class="btn btn-error btn-sm" @click="logout">Logout</button>
      </div>
    </div>

    <main class="p-6">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div class="card bg-base-100 shadow-xl">
          <div class="card-body">
            <h2 class="card-title">Collections</h2>
            <p>Gérez toutes vos collections partagées.</p>
            <div class="card-actions justify-end">
              <router-link to="/collections" class="btn btn-primary">Voir</router-link>
            </div>
          </div>
        </div>
        <!-- Autres cards ici -->
      </div>
    </main>
  </div>
</template>

<script>
export default {
  methods: {
    logout() {
      localStorage.removeItem("token");
      this.$router.push("/");
    },
  },
};
</script>
