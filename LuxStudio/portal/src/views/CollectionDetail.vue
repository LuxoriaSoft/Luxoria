<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">{{ collection?.name }}</h1>
    <p class="mb-6 text-gray-600">{{ collection?.description || 'Aucune description.' }}</p>

    <!-- Galerie scrollable avec flèches -->
    <div class="relative mb-8">
      <button
        @click="scrollLeft"
        class="absolute left-0 top-1/2 -translate-y-1/2 bg-black/50 text-white px-2 py-1 z-10 rounded hover:bg-black"
      >
        ◀
      </button>

      <div
        ref="scrollContainer"
        class="flex gap-4 overflow-x-auto pb-4 scroll-smooth"
        style="scrollbar-width: none;"
      >
        <div
          v-for="(photo, index) in collection?.photos"
          :key="photo.id"
          class="min-w-[250px] max-w-[250px] flex-shrink-0 relative border rounded overflow-hidden shadow cursor-pointer"
          @click="openModal(index)"
        >
          <img :src="photo.filePath" alt="Image" class="w-full h-48 object-cover" />
        </div>
      </div>

      <button
        @click="scrollRight"
        class="absolute right-0 top-1/2 -translate-y-1/2 bg-black/50 text-white px-2 py-1 z-10 rounded hover:bg-black"
      >
        ▶
      </button>
    </div>

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
      <p class="text-sm mt-2" :class="shareMessageClass" v-if="shareMessage">{{ shareMessage }}</p>
    </div>

    <!-- Chat temps réel -->
    <div class="mb-8 p-4 border rounded shadow bg-white">
      <h2 class="text-lg font-semibold mb-2">Discussion</h2>
      <p class="text-sm text-gray-500 mb-4">
        Connecté en tant que : <strong>{{ username.value }}</strong>
      </p>

      <div class="h-64 overflow-y-auto mb-4 border p-2 rounded bg-gray-50" ref="chatContainer">
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

      <!-- Input message avec popup @mention -->
      <div class="relative flex gap-2">
        <input
          v-model="chatMessage"
          placeholder="Votre message..."
          class="input input-bordered flex-1"
          @keyup.enter="sendMessage"
        />
        <button @click="sendMessage" class="btn btn-primary">Envoyer</button>

        <!-- Popup mention uniquement si @ est détecté -->
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
    </div>

    <!-- Utilisateurs autorisés -->
    <div>
      <h2 class="text-lg font-semibold mb-2">Utilisateurs autorisés</h2>
      <ul class="list-disc pl-5 text-sm text-gray-600">
        <li v-for="email in collection?.allowedEmails?.map(e => e.email)" :key="email">{{ email }}</li>
      </ul>
    </div>

    <!-- Modale pour l’image -->
    <div
      v-if="modalVisible"
      class="fixed inset-0 bg-black/80 flex items-center justify-center z-50"
      @click.self="closeModal"
    >
      <div class="relative w-full max-w-4xl p-4">
        <button @click="closeModal" class="absolute top-4 right-4 text-white text-2xl">✕</button>
        <div class="text-center text-white mb-4">{{ modalImageName }}</div>
        <div class="flex items-center justify-center gap-4">
          <button @click="prevModalImage" class="text-white text-3xl">◀</button>
          <img
            :src="modalImageSrc"
            :alt="modalImageName"
            class="max-h-[80vh] max-w-full rounded shadow"
          />
          <button @click="nextModalImage" class="text-white text-3xl">▶</button>
        </div>
      </div>
    </div>
    <!-- Modale de sélection d’images avec # -->
    <div
      v-if="imageModalVisible"
      class="fixed inset-0 bg-black/70 z-50 flex items-center justify-center"
      @click.self="imageModalVisible = false"
    >
      <div class="bg-white rounded-lg shadow-lg max-w-3xl w-full p-6 relative">
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
            class="relative border rounded cursor-pointer hover:ring-2 hover:ring-blue-500"
            :class="{'ring-2 ring-blue-600': selectedImages.includes(photo)}"
            @click="toggleImageSelection(photo)"
          >
            <img :src="photo.filePath" alt="image" class="w-full h-32 object-cover rounded" />
            <p class="text-xs p-1 truncate">{{ photo.filePath.split('/').pop() }}</p>
          </div>
        </div>

        <div class="flex justify-end gap-2 mt-6">
          <button class="btn" @click="imageModalVisible = false">Annuler</button>
          <button class="btn btn-primary" @click="confirmImageSelection">Valider</button>
        </div>
      </div>
    </div>
    <!-- Modale d’image pour le chat -->
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
  </div>
</template>



<script setup>
import { ref, onMounted, onUnmounted, watch, computed, watchEffect } from 'vue'
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
const scrollContainer = ref(null)
// État pour le système de mention
const mentionPopupVisible = ref(false)
const mentionQuery = ref("")
const filteredUsers = ref([])
const allowedUsers = computed(() => collection.value?.allowedEmails || [])
// État pour la recherche d'images avec #
const imageModalVisible = ref(false)
const imageQuery = ref("")
const selectedImages = ref([])
const imageSearchResults = computed(() => {
  const query = imageQuery.value.toLowerCase()
  return collection.value?.photos.filter(photo =>
    photo.filePath.toLowerCase().includes(query)
  ) || []
})

const chatImageModalVisible = ref(false)
const chatModalImageSrc = ref("")
const chatModalImageName = ref("")

function openChatImageModal(filename) {
  chatModalImageName.value = filename
  chatModalImageSrc.value = `http://localhost:5269/api/collection/image/${filename}`
  chatImageModalVisible.value = true
}



watchEffect(() => {
  const value = chatMessage.value

  // @mention
  const mentionMatch = value.match(/@([\w.-]*)$/)
  if (mentionMatch && allowedUsers.value.length > 0) {
    mentionQuery.value = mentionMatch[1].toLowerCase()
    mentionPopupVisible.value = true
    filteredUsers.value = allowedUsers.value
      .filter(email =>
        typeof email === 'string' &&
        email.toLowerCase().includes(mentionQuery.value) &&
        email.toLowerCase() !== userEmail.value.toLowerCase()
      )
      .map(email => ({ email }))
  } else {
    mentionPopupVisible.value = false
  }

  // #image
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

function formatMessage(text) {
  const imageRegex = /#([\w\-.]+\.(jpg|jpeg|png))/gi
  return text.replace(imageRegex, (_, filename) => {
    return `<span class="text-blue-500 underline cursor-pointer" onclick="window.__openImageModal && window.__openImageModal('${filename}')">#${filename}</span>`
  })
}

onMounted(() => {
  window.__openImageModal = openChatImageModal
})
onUnmounted(() => {
  delete window.__openImageModal
})



function selectMention(email) {
  // Remplace uniquement la dernière mention partielle
  chatMessage.value = chatMessage.value.replace(/@[\w.-]*$/, `@${email} `)
  mentionPopupVisible.value = false
}

function scrollLeft() {
  scrollContainer.value.scrollLeft -= 300
}

function scrollRight() {
  scrollContainer.value.scrollLeft += 300
}

const modalVisible = ref(false)
const currentModalIndex = ref(0)

const modalImageSrc = computed(() =>
  collection.value?.photos[currentModalIndex.value]?.filePath || ""
)
const modalImageName = computed(() => {
  const src = modalImageSrc.value
  return src ? src.split("/").pop() : "Image"
})

function openModal(index) {
  currentModalIndex.value = index
  modalVisible.value = true
}

function closeModal() {
  modalVisible.value = false
}

function prevModalImage() {
  if (currentModalIndex.value > 0) currentModalIndex.value--
}

function nextModalImage() {
  if (
    currentModalIndex.value < (collection.value?.photos.length || 0) - 1
  )
    currentModalIndex.value++
}


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
      senderEmail: m.senderEmail,
      avatar: m.avatarFileName ?? 'default_avatar.jpg',
      text: m.message,
      sentAt: m.sentAt,
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

  connection.on('ReceiveMessage', (sender, text, avatar, sentAt) => {
    messages.value.push({ sender, text, avatar, sentAt, isMine: sender === username.value })
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

function formatTime(dateStr) {
  const d = new Date(dateStr);
  return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function getAvatarUrl(filename) {
  if (!filename) return '/default_avatar.jpg';
  return `http://localhost:5269/auth/avatar/${filename}`;
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

const shareMessageClass = ref("text-green-600")

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
    shareMessageClass.value = 'text-green-600'
    collection.value.allowedEmails.push({ email: newEmail.value })
    newEmail.value = ''
  } catch (err) {
    console.error("Erreur lors de l'ajout :", err)
    shareMessage.value = err.response?.data || 'Erreur lors du partage.'
    shareMessageClass.value = 'text-red-500'
  }
}
</script>
