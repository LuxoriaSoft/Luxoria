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
<!-- Chat temps réel -->
<div class="mb-8 p-4 border rounded shadow bg-white">
  <h2 class="text-lg font-semibold mb-2">Discussion</h2>
  <p class="text-sm text-gray-500 mb-4">
    Connecté en tant que : <strong>{{ username.value }}</strong>
  </p>
  <p class="text-xs text-gray-500">[Debug] Vous êtes : {{ username }}</p>

  <div class="h-64 overflow-y-auto mb-4 border p-2 rounded bg-gray-50" ref="chatContainer">
    <div
      v-for="(msg, index) in messages"
      :key="index"
      class="mb-2 flex"
      :class="{
        'justify-end': msg.isMine,
        'justify-start': !msg.isMine
      }"
    >
      <div
        class="max-w-xs px-3 py-2 rounded-lg text-sm shadow"
        :class="{
          'bg-blue-600 text-white': msg.isMine,
          'bg-gray-200 text-gray-900': !msg.isMine
        }"
      >
        <p class="font-semibold text-xs mb-1 text-right" v-if="msg.isMine">
          Vous
        </p>
        <p class="font-semibold text-xs mb-1" v-else>{{ msg.sender }}</p>
        <p>{{ msg.text }}</p>
      </div>
    </div>

  </div>

  <div class="flex gap-2">
    <input
      v-model="chatMessage"
      placeholder="Votre message..."
      class="input input-bordered flex-1"
      @keyup.enter="sendMessage"
    />
    <button @click="sendMessage" class="btn btn-primary">Envoyer</button>
  </div>
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
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'

const route = useRoute()
const collection = ref(null)
const loading = ref(true)
const fileInput = ref(null)
const uploadMessage = ref("")
const newEmail = ref("")
const shareMessage = ref("")

// Chat
const chatMessage = ref('')
const messages = ref([])
const chatContainer = ref(null)
let connection = null
const username = ref('Utilisateur')
const userEmail = ref('')

onMounted(async () => {
  const token = localStorage.getItem('token')
  const id = route.params.id

  try {
    // 1. Fetch user first (needed for comparison)
    const whoamiRes = await axios.get('http://localhost:5269/auth/whoami', {
      headers: { Authorization: `Bearer ${token}` }
    })
    username.value = whoamiRes.data.username || 'Utilisateur'
    userEmail.value = whoamiRes.data.userEmail || ''

    // 2. Fetch collection after having username
    const response = await axios.get(`http://localhost:5269/api/collection/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    collection.value = response.data

    // 3. Map messages with proper comparison
    messages.value = collection.value.chatMessages.map(m => ({
      sender: m.senderUsername,
      text: m.message,
      isMine: m.senderUsername === username.value
    }))
  } catch (err) {
    console.error('Erreur lors du chargement ou de l’identification :', err)
  } finally {
    loading.value = false
  }

  // SignalR setup (inchangé)
  connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5269/hubs/chat', {
      accessTokenFactory: () => localStorage.getItem('token')
    })
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveMessage', (sender, text) => {
    messages.value.push({ sender, text, isMine: sender === username.value })
    scrollToBottom()
  })

  await connection.start()
  await connection.invoke('JoinCollection', id)
})


onUnmounted(() => {
  if (connection) {
    connection.invoke('LeaveCollection', route.params.id)
    connection.stop()
  }
})

function scrollToBottom() {
  setTimeout(() => {
    chatContainer.value.scrollTop = chatContainer.value.scrollHeight
  }, 0)
}

async function sendMessage() {
  if (!chatMessage.value.trim()) return;

  const token = localStorage.getItem('token');
  const collectionId = route.params.id;

  try {
    await axios.post(
      `http://localhost:5269/api/collection/${collectionId}/chat`,
      {
        senderEmail: userEmail.value,
        senderUsername: username.value,
        message: chatMessage.value.trim(),
      },
      {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      }
    );

    chatMessage.value = '';
  } catch (err) {
    console.error('Erreur lors de l’envoi du message :', err);
  }
}




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
      { email: newEmail.value },
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
