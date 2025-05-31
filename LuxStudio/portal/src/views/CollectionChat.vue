<template>
  <div class="p-6 max-w-4xl mx-auto">
    <h1 class="text-2xl font-bold mb-4">Discussion – {{ collection?.name }}</h1>

    <div class="text-sm text-gray-600 mb-4">
      Connecté en tant que : <strong>{{ username.value }}</strong>
    </div>

    <div class="h-96 overflow-y-auto border rounded p-4 bg-white mb-4" ref="chatContainer">
      <div
        v-for="(msg, index) in messages"
        :key="index"
        :class="['chat', msg.isMine ? 'chat-end' : 'chat-start']"
      >
        <div class="chat-image avatar">
          <div class="w-10 rounded-full">
            <img :src="`http://localhost:5269/auth/avatar/${msg.avatar}`" alt="avatar" />
          </div>
        </div>
        <div class="chat-header">
          {{ msg.isMine ? 'Vous' : msg.sender }}
          <time class="text-xs opacity-50 ml-2">{{ formatTime(msg.sentAt) }}</time>
        </div>
        <div class="chat-bubble" :class="msg.isMine ? 'bg-blue-600 text-white' : ''">
          <span v-html="formatMessage(msg.text)"></span>
        </div>
      </div>
    </div>

    <div class="relative flex gap-2">
      <input
        v-model="chatMessage"
        placeholder="Votre message..."
        class="input input-bordered flex-1"
        @keyup.enter="sendMessage"
      />
      <button @click="sendMessage" class="btn btn-primary">Envoyer</button>

      <div
        v-if="mentionPopupVisible"
        class="absolute bottom-full left-0 mb-2 bg-white border rounded shadow-lg w-full max-h-48 overflow-y-auto z-50"
      >
        <ul class="divide-y divide-gray-100">
          <li
            v-for="user in filteredUsers"
            :key="user.email"
            class="cursor-pointer hover:bg-gray-100 px-4 py-2 text-sm text-gray-700"
            @click="selectMention(user.email)"
          >
            {{ user.email }}
          </li>
        </ul>
      </div>
    </div>

    <!-- Modale d'image chat -->
    <div
      v-if="chatImageModalVisible"
      class="fixed inset-0 bg-black/80 z-50 flex items-center justify-center"
      @click.self="chatImageModalVisible = false"
    >
      <div class="bg-white p-4 rounded shadow max-w-3xl w-full relative">
        <button class="absolute top-2 right-2 text-gray-600 text-2xl" @click="chatImageModalVisible = false">✕</button>
        <div class="text-center mb-4 font-semibold">{{ chatModalImageName }}</div>
        <img :src="chatModalImageSrc" alt="Image du chat" class="max-h-[80vh] mx-auto rounded shadow" />
      </div>
    </div>

    <!-- Modale de sélection d’images avec # -->
    <div
      v-if="imageModalVisible"
      class="fixed inset-0 bg-black/70 z-50 flex items-center justify-center"
      @click.self="imageModalVisible = false"
    >
      <div class="bg-white rounded-lg shadow-lg max-w-4xl w-full p-6 relative">
        <h3 class="text-lg font-semibold mb-4">Sélectionner une ou plusieurs images</h3>

        <input
          v-model="imageQuery"
          type="text"
          placeholder="Rechercher une image..."
          class="input input-bordered w-full mb-4"
        />

        <div class="grid grid-cols-3 gap-4 max-h-96 overflow-y-auto">
          <div
            v-for="photo in imageSearchResults"
            :key="photo.id"
            class="relative border rounded p-2 hover:ring-2 hover:ring-blue-500"
          >
            <img :src="photo.filePath" alt="image" class="w-full h-32 object-cover rounded mb-2" />
            <p class="text-xs truncate">{{ photo.filePath.split('/').pop() }}</p>
            <div class="mt-2">
              <label class="text-xs font-medium">Statut :</label>
              <select
                v-model="photo.status"
                class="select select-sm select-bordered w-full mt-1"
                @change="updatePhotoStatus(photo)"
              >
                <option :value="0">En attente</option>
                <option :value="1">À modifier</option>
                <option :value="2">Approuvé</option>
                <option :value="3">Supprimer</option>
              </select>
            </div>
          </div>
        </div>

        <div class="flex justify-end gap-2 mt-6">
          <button class="btn" @click="imageModalVisible = false">Annuler</button>
          <button class="btn btn-primary" @click="confirmImageSelection">Valider</button>
        </div>
      </div>
    </div>
  </div>
</template>


<script setup>
import { ref, onMounted, onUnmounted, watchEffect, computed } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'

const route = useRoute()
const collection = ref(null)
const username = ref('Utilisateur')
const userEmail = ref('')
const chatMessage = ref('')
const messages = ref([])
const chatContainer = ref(null)
const connection = ref(null)

const mentionPopupVisible = ref(false)
const mentionQuery = ref('')
const filteredUsers = ref([])
const chatImageModalVisible = ref(false)
const chatModalImageSrc = ref('')
const chatModalImageName = ref('')

const allowedUsers = computed(() => collection.value?.allowedEmails || [])

const imageModalVisible = ref(false)
const imageQuery = ref("")
const selectedImages = ref([])

const imageSearchResults = computed(() => {
  const query = imageQuery.value.toLowerCase()
  return collection.value?.photos.filter(photo =>
    photo.filePath.toLowerCase().includes(query)
  ) || []
})


function openChatImageModal(filename) {
  chatModalImageName.value = filename
  chatModalImageSrc.value = `http://localhost:5269/api/collection/image/${filename}`
  chatImageModalVisible.value = true
}

watchEffect(() => {
  const value = chatMessage.value
  const mentionMatch = value.match(/@([\w.-]*)$/)
  if (mentionMatch && allowedUsers.value.length > 0) {
    mentionQuery.value = mentionMatch[1].toLowerCase()
    mentionPopupVisible.value = true
    filteredUsers.value = allowedUsers.value
      .filter(email => email.toLowerCase().includes(mentionQuery.value) && email.toLowerCase() !== userEmail.value.toLowerCase())
      .map(email => ({ email }))
  } else {
    mentionPopupVisible.value = false
  }
})

watchEffect(() => {
  const value = chatMessage.value

  // Hashtag match
  const hashtagMatch = value.match(/#([\w.-]*)$/)
  if (hashtagMatch) {
    imageQuery.value = hashtagMatch[1].toLowerCase()
    imageModalVisible.value = true
  }
})

function toggleImageSelection(photo) {
  const index = selectedImages.value.indexOf(photo)
  if (index > -1) {
    selectedImages.value.splice(index, 1)
  } else {
    selectedImages.value.push(photo)
  }
}

function confirmImageSelection() {
  const fileNames = selectedImages.value.map(p => `#${p.filePath.split("/").pop()}`)
  chatMessage.value = chatMessage.value.replace(/#([\w.-]*)$/, fileNames.join(" "))
  imageModalVisible.value = false
  selectedImages.value = []
}

async function updatePhotoStatus(photo) {
  try {
    const token = localStorage.getItem('token')
    await axios.patch(
      `http://localhost:5269/api/collection/photo/${photo.id}/status`,
      { status: photo.status },
      {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    )
    console.log(`Statut mis à jour pour la photo ${photo.id}`)
  } catch (err) {
    console.error('Erreur lors de la mise à jour du statut :', err)
  }
}


function selectMention(email) {
  chatMessage.value = chatMessage.value.replace(/@[\w.-]*$/, `@${email} `)
  mentionPopupVisible.value = false
}

function formatMessage(text) {
  const imageRegex = /#([\w\-.]+\.(jpg|jpeg|png))/gi
  return text.replace(imageRegex, (_, filename) => {
    return `<span class="text-blue-500 underline cursor-pointer" onclick="window.__openImageModal && window.__openImageModal('${filename}')">#${filename}</span>`
  })
}

function scrollToBottom() {
  setTimeout(() => {
    chatContainer.value.scrollTop = chatContainer.value.scrollHeight
  }, 0)
}

function formatTime(dateStr) {
  const d = new Date(dateStr)
  return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

async function sendMessage() {
  if (!chatMessage.value.trim()) return
  const token = localStorage.getItem('token')
  const collectionId = route.params.id

  try {
    await axios.post(`http://localhost:5269/api/collection/${collectionId}/chat`, {
      senderEmail: userEmail.value,
      senderUsername: username.value,
      message: chatMessage.value.trim(),
    }, {
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    chatMessage.value = ''
  } catch (err) {
    console.error('Erreur lors de l\u2019envoi du message :', err)
  }
}

onMounted(async () => {
  window.__openImageModal = openChatImageModal
  const token = localStorage.getItem('token')
  const id = route.params.id

  try {
    const whoamiRes = await axios.get('http://localhost:5269/auth/whoami', {
      headers: { Authorization: `Bearer ${token}` }
    })
    username.value = whoamiRes.data.username || 'Utilisateur'
    userEmail.value = whoamiRes.data.userEmail || ''

    const response = await axios.get(`http://localhost:5269/api/collection/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    collection.value = response.data

    messages.value = collection.value.chatMessages.map(m => ({
      sender: m.senderUsername,
      senderEmail: m.senderEmail,
      avatar: m.avatarFileName ?? 'default_avatar.jpg',
      text: m.message,
      sentAt: m.sentAt,
      isMine: m.senderUsername === username.value
    }))
  } catch (err) {
    console.error('Erreur lors du chargement :', err)
  }

  connection.value = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5269/hubs/chat', {
      accessTokenFactory: () => localStorage.getItem('token')
    })
    .withAutomaticReconnect()
    .build()

  connection.value.on('ReceiveMessage', (sender, text, avatar, sentAt) => {
    messages.value.push({ sender, text, avatar, sentAt, isMine: sender === username.value })
    scrollToBottom()
  })

  await connection.value.start()
  await connection.value.invoke('JoinCollection', id)
})

onUnmounted(() => {
  delete window.__openImageModal
  if (connection.value) {
    connection.value.invoke('LeaveCollection', route.params.id)
    connection.value.stop()
  }
})
</script>
