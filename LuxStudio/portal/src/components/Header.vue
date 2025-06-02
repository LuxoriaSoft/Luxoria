<!-- src/components/Header.vue -->
<template>
  <header class="bg-blue-900 shadow px-6 py-4 flex justify-between items-center">
    <h1 class="text-xl font-bold text-gray-800">Luxoria</h1>
    <nav class="flex gap-4">
      <RouterLink to="/" class="hover:text-blue-600">Accueil</RouterLink>
      <RouterLink to="/collections" class="hover:text-blue-600">Collections</RouterLink>
      <RouterLink to="/profile" class="hover:text-blue-600">Profil</RouterLink>
    </nav>
  </header>
</template>

<script setup>
import { RouterLink } from 'vue-router'
</script>
