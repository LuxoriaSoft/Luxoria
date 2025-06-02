<script setup lang="ts">
import { useRoute } from 'vue-router'
import { computed } from 'vue'
import Header from './components/Header.vue'

const route = useRoute()

const isAuthRoute = computed(() =>
  route.path.startsWith('/login') || route.path.startsWith('/register')
)

</script>

<template>
  <div id="app">
    <Header v-if="!isAuthRoute" />
    <router-view />
  </div>
</template>

<style>
/* Styles globaux, si nécessaire */
</style>
