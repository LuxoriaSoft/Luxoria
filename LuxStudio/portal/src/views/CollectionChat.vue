<template>
  <div class="p-6 max-w-4xl mx-auto">
    <h1 class="text-2xl font-bold mb-4">Chat – {{ collection?.name }}</h1>
    <div class="h-96 overflow-y-auto border rounded p-4 bg-white mb-4" ref="chatContainer">
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

    <div class="relative flex gap-2">
      <input
        v-model="chatMessage"
        placeholder="Your message..."
        class="input input-bordered flex-1"
        @keyup.enter="sendMessage"
      />
      <button @click="sendMessage" class="btn btn-primary">Send</button>

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

    <!-- Image selection modal -->
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
            class="relative border rounded p-2 hover:ring-2 hover:ring-blue-500"
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
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watchEffect, computed } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'

// Read API URL from global config
const API_URL = window.appConfig?.API_URL || ''

const route = useRoute()
const collection = ref(null)
const username = ref('User')
const userEmail = ref('')
const chatMessage = ref('')
const messages = ref([])
const chatContainer = ref(null)
let connection

const mentionPopupVisible = ref(false)
const mentionQuery = ref('')
const filteredUsers = ref([])

const chatImageModalVisible = ref(false)
const chatModalImageSrc = ref('')
const chatModalImageName = ref('')

const allowedUsers = computed(() => collection.value?.allowedEmails || [])

const imageModalVisible = ref(false)
const imageQuery = ref('')
const selectedImages = ref([])
const imageSearchResults = computed(() => {
  const q = imageQuery.value.toLowerCase()
  return collection.value?.photos.filter(p => p.filePath.toLowerCase().includes(q)) || []
})

function openChatImageModal(filename) {
  chatModalImageName.value = filename
  chatModalImageSrc.value = `${API_URL}/api/collection/image/${filename}`
  chatImageModalVisible.value = true
}

watchEffect(() => {
  const val = chatMessage.value
  // @mention logic
  const mentionMatch = val.match(/@([\w.-]*)$/)
  if (mentionMatch && allowedUsers.value.length) {
    mentionQuery.value = mentionMatch[1].toLowerCase()
    mentionPopupVisible.value = true
    filteredUsers.value = allowedUsers.value
      .filter(u => u.email.toLowerCase().includes(mentionQuery.value) && u.email.toLowerCase() !== userEmail.value.toLowerCase())
      .map(u => ({ email: u.email }))
  } else {
    mentionPopupVisible.value = false
  }
  // #image logic
  const hashMatch = val.match(/#([\w.-]*)$/)
  if (hashMatch) {
    imageQuery.value = hashMatch[1].toLowerCase()
    imageModalVisible.value = true
  }
})

function selectMention(email) {
  chatMessage.value = chatMessage.value.replace(/@[\w.-]*$/, `@${email} `)
  mentionPopupVisible.value = false
}

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

function formatMessage(text) {
  return text.replace(/#([\w.\-]+\.(jpg|jpeg|png))/gi, (_, f) =>
    `<span class="text-blue-500 underline cursor-pointer" onclick="window.__openImageModal && window.__openImageModal('${f}')">#${f}</span>`
  )
}

function scrollToBottom() {
  setTimeout(() => { chatContainer.value.scrollTop = chatContainer.value.scrollHeight }, 0)
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

onMounted(async () => {
  window.__openImageModal = openChatImageModal

  const token = localStorage.getItem('token')
  const id = route.params.id
  try {
    const whoami = await axios.get(`${API_URL}/auth/whoami`, { headers: { Authorization: `Bearer ${token}` } })
    username.value = whoami.data.username || 'User'
    userEmail.value = whoami.data.email || ''

    const res = await axios.get(`${API_URL}/api/collection/${id}`, { headers: { Authorization: `Bearer ${token}` } })
    collection.value = res.data
    messages.value = collection.value.chatMessages.map(m => ({
      sender: m.senderUsername,
      avatar: m.avatarFileName,
      text: m.message,
      sentAt: m.sentAt,
      isMine: m.senderUsername === username.value
    }))
  } catch (err) {
    console.error('Error loading chat:', err)
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_URL}/hubs/chat`, { accessTokenFactory: () => localStorage.getItem('token') })
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveMessage', (sender, text, avatar, sentAt) => {
    messages.value.push({ sender, text, avatar, sentAt, isMine: sender === username.value })
    scrollToBottom()
  })

  await connection.start()
  await connection.invoke('JoinCollection', route.params.id)
})

onUnmounted(() => {
  delete window.__openImageModal
  if (connection) {
    connection.invoke('LeaveCollection', route.params.id)
    connection.stop()
  }
})
</script>
