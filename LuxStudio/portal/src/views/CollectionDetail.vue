<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">{{ collection?.name }}</h1>
    <p class="mb-6 text-gray-600">{{ collection?.description || 'No description.' }}</p>

    <!-- Scrollable gallery with arrows -->
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

    <!-- Image upload -->
    <div class="mb-8">
      <h2 class="text-lg font-semibold mb-2">Add an image</h2>
      <input type="file" ref="fileInput" class="file-input file-input-bordered w-full max-w-xs" />
      <button @click="handleUpload" class="btn btn-primary mt-2">Upload</button>
      <p class="text-sm mt-2 text-green-600" v-if="uploadMessage">{{ uploadMessage }}</p>
    </div>

    <!-- Add user -->
    <div class="mb-8">
      <h2 class="text-lg font-semibold mb-2">Add a user to this collection</h2>
      <input v-model="newEmail" placeholder="Email" class="input input-bordered w-full max-w-xs" />
      <button @click="addUser" class="btn btn-secondary mt-2">Share</button>
      <p class="text-sm mt-2" :class="shareMessageClass" v-if="shareMessage">{{ shareMessage }}</p>
    </div>

    <!-- Real-time chat -->
    <div class="mb-8 p-4 border rounded shadow bg-white">
      <h2 class="text-lg font-semibold mb-2">Chat</h2>
      <div class="h-64 overflow-y-auto mb-4 border p-2 rounded bg-gray-50" ref="chatContainer">
        <div
          v-for="(msg, index) in messages"
          :key="index"
          :class="['chat', msg.isMine ? 'chat-end' : 'chat-start']"
        >
          <!-- Avatar: only show when msg.avatar is not NULL -->
          <div v-if="msg.avatar" class="chat-image avatar">
            <div class="w-10 rounded-full overflow-hidden">
              <img
                :src="`${API_URL}/auth/avatar/${msg.avatar}`"
                alt="avatar"
                class="w-full h-full object-cover"
              />
            </div>
          </div>
          <div class="chat-header">
            {{ msg.isMine ? 'You' : msg.sender }}
            <time class="text-xs opacity-50 ml-2">{{ formatTime(msg.sentAt) }}</time>
          </div>
          <div class="chat-bubble" :class="msg.isMine ? 'bg-blue-600 text-white' : ''">
            <span v-html="formatMessage(msg.text)"></span>
          </div>
        </div>
      </div>

      <!-- Message input with @mention & #image -->
      <div class="relative flex gap-2">
        <input
          v-model="chatMessage"
          placeholder="Your message..."
          class="input input-bordered flex-1"
          @keyup.enter="sendMessage"
        />
        <button @click="sendMessage" class="btn btn-primary">Send</button>

        <!-- Mention popup -->
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

    <!-- Authorized users -->
    <div>
      <h2 class="text-lg font-semibold mb-2">Authorized users</h2>
      <ul class="list-disc pl-5 text-sm text-gray-600">
        <li v-for="email in collection?.allowedEmails?.map(e => e.email)" :key="email">{{ email }}</li>
      </ul>
    </div>

    <!-- Image modal -->
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
    <!-- #image selection modal -->
    <div
      v-if="imageModalVisible"
      class="fixed inset-0 bg-black/70 z-50 flex items-center justify-center"
      @click.self="imageModalVisible = false"
    >
      <div class="bg-white rounded-lg shadow-lg max-w-4xl w-full p-6 relative">
        <h3 class="text-lg font-semibold mb-4">Select one or more images</h3>

        <input
          v-model="imageQuery"
          type="text"
          placeholder="Search images..."
          class="input input-bordered w-full mb-4"
        />

        <div class="grid grid-cols-3 gap-4 max-h-96 overflow-y-auto">
          <div
            v-for="photo in imageSearchResults"
            :key="photo.id"
            class="relative border rounded p-2 cursor-pointer hover:ring-2 hover:ring-blue-500"
            :class="{'ring-2 ring-blue-600': selectedImages.includes(photo)}"
            @click="toggleImageSelection(photo)"
          >
            <img :src="photo.filePath" alt="image" class="w-full h-32 object-cover rounded mb-2" />
            <p class="text-xs truncate">{{ photo.filePath.split('/').pop() }}</p>
            <div class="mt-2">
              <label class="text-xs font-medium">Status:</label>
              <select
                v-model="photo.status"
                class="select select-sm select-bordered w-full mt-1"
                @change="updatePhotoStatus(photo)"
              >
                <option :value="0">Pending</option>
                <option :value="1">To edit</option>
                <option :value="2">Approved</option>
                <option :value="3">Delete</option>
              </select>
            </div>
          </div>
        </div>

        <div class="flex justify-end gap-2 mt-6">
          <button class="btn" @click="imageModalVisible = false">Cancel</button>
          <button class="btn btn-primary" @click="confirmImageSelection">Confirm</button>
        </div>
      </div>
    </div>
    <router-link
      :to="`/collections/${collection?.id}/chat`"
      class="btn btn-sm btn-outline mt-2"
    >
      Go to dedicated chat
    </router-link>
    <!-- Chat image modal -->
    <div
      v-if="chatImageModalVisible"
      class="fixed inset-0 bg-black/80 z-50 flex items-center justify-center"
      @click.self="chatImageModalVisible = false"
    >
      <div class="bg-white p-4 rounded shadow max-w-3xl w-full relative">
        <button class="absolute top-2 right-2 text-gray-600 text-2xl" @click="chatImageModalVisible = false">✕</button>
        <div class="text-center mb-4 font-semibold">{{ chatModalImageName }}</div>
        <img :src="chatModalImageSrc" alt="Chat image" class="max-h-[80vh] mx-auto rounded shadow" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch, computed, watchEffect } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'

// Safely read API URL from global config
const API_URL = window.appConfig?.API_URL || ''

const route = useRoute()
const collection = ref(null)
const loading = ref(true)
const fileInput = ref(null)
const uploadMessage = ref('')
const newEmail = ref('')
const shareMessage = ref('')
const shareMessageClass = ref('text-green-600')

// Chat
const chatMessage = ref('')
const messages = ref([])
const chatContainer = ref(null)
let connection = null
const username = ref('User')
const userEmail = ref('')
const scrollContainer = ref(null)

// Mention state
const mentionPopupVisible = ref(false)
const mentionQuery = ref('')
const filteredUsers = ref([])
const allowedUsers = computed(() => collection.value?.allowedEmails || [])

// #image search
const imageModalVisible = ref(false)
const imageQuery = ref('')
const selectedImages = ref([])
const imageSearchResults = computed(() => {
  const q = imageQuery.value.toLowerCase()
  return collection.value?.photos.filter(photo => photo.filePath.toLowerCase().includes(q)) || []
})

// Chat image modal
const chatImageModalVisible = ref(false)
const chatModalImageSrc = ref('')
const chatModalImageName = ref('')

function openChatImageModal(filename) {
  chatModalImageName.value = filename
  chatModalImageSrc.value = `${API_URL}/api/collection/image/${filename}`
  chatImageModalVisible.value = true
}

function formatMessage(text) {
  const imgRe = /#([\w\-.]+\.(jpg|jpeg|png))/gi
  return text.replace(imgRe, (_, f) => 
    `<span class="text-blue-500 underline cursor-pointer" onclick="window.__openImageModal && window.__openImageModal('${f}')">#${f}</span>`
  )
}

onMounted(() => {
  window.__openImageModal = openChatImageModal
})
onUnmounted(() => {
  delete window.__openImageModal
})

function selectMention(email) {
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
const modalImageSrc = computed(() => collection.value?.photos[currentModalIndex.value]?.filePath || '')
const modalImageName = computed(() => modalImageSrc.value.split('/').pop() || 'Image')
function openModal(idx) { currentModalIndex.value = idx; modalVisible.value = true }
function closeModal() { modalVisible.value = false }
function prevModalImage() { if (currentModalIndex.value > 0) currentModalIndex.value-- }
function nextModalImage() { if (currentModalIndex.value < (collection.value?.photos.length || 0) - 1) currentModalIndex.value++ }

watchEffect(() => {
  const val = chatMessage.value
  const mentionMatch = val.match(/@([\w.-]*)$/)
  if (mentionMatch && allowedUsers.value.length) {
    mentionQuery.value = mentionMatch[1].toLowerCase()
    mentionPopupVisible.value = true
    filteredUsers.value = allowedUsers.value
      .filter(e => e.email.toLowerCase().includes(mentionQuery.value) && e.email.toLowerCase() !== userEmail.value.toLowerCase())
      .map(e => ({ email: e.email }))
  } else {
    mentionPopupVisible.value = false
  }

  const hashMatch = val.match(/#([\w.-]*)$/)
  if (hashMatch) {
    imageQuery.value = hashMatch[1].toLowerCase()
    imageModalVisible.value = true
  }
})

function toggleImageSelection(photo) {
  const i = selectedImages.value.indexOf(photo)
  if (i > -1) selectedImages.value.splice(i, 1)
  else selectedImages.value.push(photo)
}
function confirmImageSelection() {
  const tags = selectedImages.value.map(p => `#${p.filePath.split('/').pop()}`)
  chatMessage.value = chatMessage.value.replace(/#[\w.-]*$/, tags.join(' '))
  imageModalVisible.value = false
  selectedImages.value = []
}

function formatTime(dateStr) {
  return new Date(dateStr).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

async function sendMessage() {
  if (!chatMessage.value.trim()) return
  const token = localStorage.getItem('token')
  const id = route.params.id
  try {
    await axios.post(`${API_URL}/api/collection/${id}/chat`, {
      senderEmail: userEmail.value,
      senderUsername: username.value,
      message: chatMessage.value.trim()
    }, { headers: { Authorization: `Bearer ${token}` } })
    chatMessage.value = ''
  } catch (err) {
    console.error('Error sending message:', err)
  }
}

async function handleUpload() {
  const file = fileInput.value?.files[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  const token = localStorage.getItem('token')
  try {
    const res = await axios.post(`${API_URL}/api/collection/${route.params.id}/upload`, formData, {
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'multipart/form-data' }
    })
    uploadMessage.value = 'Image uploaded successfully.'
    collection.value.photos.push(res.data)
  } catch (err) {
    console.error('Upload error:', err)
    uploadMessage.value = 'Error uploading image.'
  }
}

async function updatePhotoStatus(photo) {
  const token = localStorage.getItem('token')
  try {
    await axios.patch(`${API_URL}/api/collection/photo/${photo.id}/status`, { status: photo.status }, {
      headers: { Authorization: `Bearer ${token}` }
    })
  } catch (err) {
    console.error('Error updating status:', err)
  }
}

async function deleteImage(photoId) {
  const token = localStorage.getItem('token')
  try {
    await axios.delete(`${API_URL}/api/photo/${photoId}`, { headers: { Authorization: `Bearer ${token}` } })
    collection.value.photos = collection.value.photos.filter(p => p.id !== photoId)
  } catch (err) {
    console.error('Error deleting image:', err)
  }
}

async function addUser() {
  if (!newEmail.value) return
  const token = localStorage.getItem('token')
  try {
    await axios.patch(`${API_URL}/api/collection/${route.params.id}/allowedEmails`, { email: newEmail.value }, {
      headers: { Authorization: `Bearer ${token}` }
    })
    shareMessage.value = 'User added successfully.'
    shareMessageClass.value = 'text-green-600'
    collection.value.allowedEmails.push({ email: newEmail.value })
    newEmail.value = ''
  } catch (err) {
    console.error('Error sharing:', err)
    shareMessage.value = err.response?.data || 'Error adding user.'
    shareMessageClass.value = 'text-red-500'
  }
}

onMounted(async () => {
  const token = localStorage.getItem('token')
  const id = route.params.id
  try {
    const whoamiRes = await axios.get(`${API_URL}/auth/whoami`, { headers: { Authorization: `Bearer ${token}` } })
    username.value = whoamiRes.data.username || 'User'
    userEmail.value = whoamiRes.data.email || ''

    const res = await axios.get(`${API_URL}/api/collection/${id}`, { headers: { Authorization: `Bearer ${token}` } })
    collection.value = res.data
    console.log(collection.value.chatMessages)
    messages.value = collection.value.chatMessages.map(m => ({
      sender: m.senderUsername,
      senderEmail: m.senderEmail,
      avatar: m.avatarFileName,
      text: m.message,
      sentAt: m.sentAt,
      isMine: m.senderUsername === username.value
    }))
  } catch (err) {
    console.error('Error loading data:', err)
  } finally {
    loading.value = false
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_URL}/hubs/chat`, { accessTokenFactory: () => localStorage.getItem('token') })
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveMessage', (sender, text, avatar, sentAt) => {
    messages.value.push({ sender, text, avatar, sentAt, isMine: sender === username.value })
    setTimeout(() => { chatContainer.value.scrollTop = chatContainer.value.scrollHeight }, 0)
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
</script>
