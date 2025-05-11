<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">{{ collection?.name }}</h1>
    <p class="mb-6 text-gray-600">{{ collection?.description || 'Aucune description.' }}</p>

    <!-- Images de la collection -->
    <div v-if="collection?.photos?.length" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 mb-8">
      <div v-for="photo in collection.photos" :key="photo.id" class="relative border rounded overflow-hidden shadow">
        <img :src="photo.filePath" alt="Image" class="w-full h-48 object-cover" />
        <button
          @click="deleteImage(photo.id)"
          class="absolute top-2 right-2 bg-red-600 text-white text-sm px-2 py-1 rounded hover:bg-red-700"
        >
          Supprimer
        </button>
      </div>
    </div>
    <div v-else class="text-gray-400 mb-8">Aucune image pour cette collection.</div>

    <!-- Upload d’image -->
    <div class="mb-8">
      <h2 class="text-lg font-semibold mb-2">Ajouter une image</h2>
      <input type="file" ref="fileInput" class="file-input file-input-bordered w-full max-w-xs" />
      <button @click="handleUpload" class="btn btn-primary mt-2">Téléverser</button>
      <p class="text-sm mt-2 text-green-600" v-if="uploadMessage">{{ uploadMessage }}</p>
    </div>

    <!-- Ajout utilisateur -->
    <div class="mb-8">
      <h2 class="text-lg font-semibold mb-2">Ajouter un utilisateur à cette collection</h2>
      <input v-model="newEmail" placeholder="Email" class="input input-bordered w-full max-w-xs" />
      <button @click="addUser" class="btn btn-secondary mt-2">Partager</button>
      <p class="text-sm mt-2 text-green-600" v-if="shareMessage">{{ shareMessage }}</p>
    </div>

    <!-- Section Chat -->
    <div class="mb-8">
      <h2 class="text-lg font-semibold mb-2">Chat (à venir)</h2>
      <p class="text-gray-400">Interface de chat temps réel à implémenter.</p>
    </div>

    <!-- Utilisateurs autorisés -->
    <div>
      <h2 class="text-lg font-semibold mb-2">Utilisateurs autorisés</h2>
      <ul class="list-disc pl-5 text-sm text-gray-600">
        <li v-for="email in collection?.allowedEmails?.map(e => e.email)" :key="email">{{ email }}</li>
      </ul>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'

const route = useRoute()
const collection = ref(null)
const loading = ref(true)
const fileInput = ref(null)
const uploadMessage = ref("")
const newEmail = ref("")
const shareMessage = ref("")

onMounted(async () => {
  const token = localStorage.getItem('token')
  const id = route.params.id

  try {
    const response = await axios.get(`http://localhost:5269/api/collection/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    collection.value = response.data
  } catch (err) {
    console.error('Erreur chargement collection :', err)
  } finally {
    loading.value = false
  }
})

async function handleUpload() {
  const file = fileInput.value?.files[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const token = localStorage.getItem('token')
    const response = await axios.post(
      `http://localhost:5269/api/collection/${route.params.id}/upload`,
      formData,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'multipart/form-data'
        }
      }
    )
    uploadMessage.value = 'Image ajoutée avec succès.'
    collection.value.photos.push(response.data)
  } catch (err) {
    console.error("Erreur d'upload :", err)
    uploadMessage.value = 'Erreur lors de l’upload.'
  }
}

async function deleteImage(photoId) {
  const token = localStorage.getItem('token')
  try {
    await axios.delete(`http://localhost:5269/api/photo/${photoId}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
    collection.value.photos = collection.value.photos.filter(p => p.id !== photoId)
  } catch (err) {
    console.error("Erreur lors de la suppression de l’image :", err)
  }
}

async function addUser() {
  if (!newEmail.value) return

  try {
    const token = localStorage.getItem('token')
    await axios.patch(
      `http://localhost:5269/api/collection/${route.params.id}/allowedEmails`,
      { email: newEmail.value }, // ← objet JSON
      {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        }
      }
    )

    shareMessage.value = 'Utilisateur ajouté avec succès.'
    collection.value.allowedEmails.push({ email: newEmail.value })
    newEmail.value = ''
  } catch (err) {
    console.error("Erreur lors de l'ajout :", err)
    shareMessage.value = 'Erreur lors du partage.'
  }
}
</script>
