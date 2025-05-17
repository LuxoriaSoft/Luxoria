<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">Vos collections</h1>

    <div v-if="loading" class="text-center text-gray-400">Chargement...</div>

    <div v-else-if="collections.length === 0" class="text-center text-gray-500">
      Aucune collection disponible.
    </div>

    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="col in collections"
        :key="col.id"
        class="card bg-base-100 shadow-xl"
      >
        <div class="card-body">
          <h2 class="card-title">{{ col.name }}</h2>
          <p>{{ col.description || 'Aucune description.' }}</p>
          <div class="card-actions justify-end">
            <router-link
              :to="`/collections/${col.id}`"
              class="btn btn-primary btn-sm"
            >
              Voir
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { getCollections } from '../services/collection_services'

const collections = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    collections.value = await getCollections()
  } catch (err) {
    console.error('Erreur lors du chargement des collections:', err)
  } finally {
    loading.value = false
  }
})
</script>
