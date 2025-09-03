'use client'

import { useRef, useState, useEffect } from 'react'
import { Avatar } from '@/components/avatar'
import { Button } from '@/components/button'
import { Heading, Subheading } from '@/components/heading'
import { Input } from '@/components/input'
import { Text } from '@/components/text'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/dialog'
import { useUser } from '@/hooks/useUser'
import { useCollectionDetail } from '@/hooks/useCollectionDetail'
import { CollectionService, ChatMessage } from '@/services/collection.services'
import { useRouter } from 'next/navigation'
import JSZip from 'jszip'
import { saveAs } from 'file-saver'

const imageCache = new Map<string, string>()

export default function CollectionDetail() {
  const {
    id,
    collection,
    setCollection,
    messages,
    setMessages,
    loading,
    chatContainerRef,
  } = useCollectionDetail()

  const [chatMessage, setChatMessage] = useState('')
  const [mentionVisible, setMentionVisible] = useState(false)
  const [mentionQuery, setMentionQuery] = useState('')
  const [filteredEmails, setFilteredEmails] = useState<string[]>([])
  const [modalIndex, setModalIndex] = useState<number | null>(0)
  const [inviteOpen, setInviteOpen] = useState(false)
  const [inviteEmail, setInviteEmail] = useState('')
  const [inviteMessage, setInviteMessage] = useState('')
  const [inviteError, setInviteError] = useState(false)
  const [isReportUserModalOpen, setIsReportUserModalOpen] = useState(false)
  const [reportReason, setReportReason] = useState('')
  const [selectedUserToReport, setSelectedUserToReport] = useState('')
  const router = useRouter()
  const [selectedFiles, setSelectedFiles] = useState<File[]>([])
  const [uploading, setUploading] = useState(false)
  const [isSending, setIsSending] = useState(false);
  const [showStatusMenu, setShowStatusMenu] = useState(false)
  const [updatingStatus, setUpdatingStatus] = useState(false)

  const statusLabels: Record<number, string> = {
    0: 'Pending',
    1: 'ModificationReview',
    2: 'Approved',
    3: 'ToDelete',
  };

  const statusOptions: { label: string; value: number }[] = Object.entries(statusLabels).map(
  ([key, label]) => ({
    label,
    value: parseInt(key, 10),
  })
);


  const { user } = useUser()
  const API_URL = process.env.NEXT_PUBLIC_API_URL

  const updatePhotoStatus = async (newStatus: number) => {
  if (!selectedImage) return
  setUpdatingStatus(true)
  try {
    await CollectionService.updatePhotoStatus(selectedImage.id, newStatus)
    setCollection(prev => {
      if (!prev) return prev
      const photosUpdated = prev.photos.map(photo =>
        photo.id === selectedImage.id ? { ...photo, status: newStatus } : photo
      )
      return { ...prev, photos: photosUpdated }
    })
    setShowStatusMenu(false)
  } catch (error) {
    console.error('Error updating status:', error)
    alert('Erreur lors de la mise à jour du statut')
  } finally {
    setUpdatingStatus(false)
  }
}


  // Gestion mentions
  useEffect(() => {
    const match = chatMessage.match(/@([\w.-]*)$/)
    if (match) {
      const query = match[1].toLowerCase()
      const filtered = (collection?.allowedEmails || []).filter(
        email => email.toLowerCase().includes(query) && email !== user?.email
      )
      setMentionQuery(query)
      setFilteredEmails(filtered)
      setMentionVisible(filtered.length > 0)
    } else {
      setMentionVisible(false)
    }
  }, [chatMessage, collection, user])

  const insertMention = (email: string) => {
    const updated = chatMessage.replace(/@([\w.-]*)$/, `@${email} `)
    setChatMessage(updated)
    setMentionVisible(false)
  }

  const handleDownloadAllPhotos = async () => {
  if (!collection || !collection.photos.length) return

  const zip = new JSZip()
  const folder = zip.folder('photos_collection')

  try {
    if (!folder) {
      throw new Error('Failed to create zip folder.')
    }
    // Télécharge chaque image en blob et l'ajoute au zip
    await Promise.all(
      collection.photos.map(async (photo) => {
        const response = await fetch(photo.filePath)
        const blob = await response.blob()
        const fileName = photo.filePath.split('/').pop() || `photo_${photo.id}.jpg`
        folder.file(fileName, blob)
      })
    )

    const content = await zip.generateAsync({ type: 'blob' })
    saveAs(content, `${collection.name || 'collection'}_photos.zip`)
  } catch (error) {
    console.error('Error while downloading photos:', error)
    alert('Error while downloading photos')
  }
}

  // Upload photo
const handleUploadPhoto = async () => {
  if (selectedFiles.length === 0) return
  if (isSending) return;
  try {
    setIsSending(true)
    for (const file of selectedFiles) {
      await CollectionService.uploadPhoto(id, file)
    }
    alert('Photos uploaded successfully!')
    const updated = await CollectionService.fetchCollection(id)
    setCollection(updated)
    setSelectedFiles([])
  } catch (err) {
    console.error(err)
    alert('Error uploading photos')
  } finally {
    setIsSending(false)
  }
}


const handleSendMessage = async () => {
  if (!chatMessage.trim() || !user) return
  if (isSending) return  // bloque si déjà en envoi

  try {
    setIsSending(true)  // <<==== Active le loading / disable bouton

    const currentPhotoId = selectedImage?.id

    // Envoie le message
    await CollectionService.sendChatMessage(id, {
      senderEmail: user.email,
      senderUsername: user.username,
      message: chatMessage.trim(),
      photoId: currentPhotoId,
    })

    // Extraire toutes les mentions (emails précédées d’un @)
    const mentionEmails = Array.from(chatMessage.matchAll(/@([\w.-]+@[\w.-]+\.[\w.-]+)/g)).map(m => m[1]);

    // Envoie une notif mail pour chaque email mentionné
    for (const email of mentionEmails) {
      try {
        await CollectionService.sendMentionNotification(id, email, user.email, chatMessage.trim())
      } catch (e) {
        console.error(`Failed to send mention email to ${email}`, e)
      }
    }

    // Ajoute le message localement
    const newMessage: ChatMessage = {
      senderUsername: user.username,
      senderEmail: user.email,
      message: chatMessage.trim(),
      sentAt: new Date().toISOString(),
      isMine: true,
      //avatarFileName: user.avatarFileName ?? 'default_avatar.jpg',
      photoId: currentPhotoId,
    }
    setChatMessage('')
  } catch (err) {
    console.error('Error sending message:', err)
  } finally {
    setIsSending(false)  // <<==== Réactive le bouton quand fini
  }
}  



  // Invitation utilisateur
  const handleInvite = async () => {
    if (!inviteEmail) return
    try {
      const res = await CollectionService.addUserToCollection(id, inviteEmail)
      setInviteMessage(res.message || 'User added successfully.')
      setInviteError(false)
      setCollection(prev => prev ? { ...prev, allowedEmails: [...prev.allowedEmails, inviteEmail] } : prev)
      setInviteEmail('')
    } catch (err: any) {
      setInviteMessage(err.response?.data || 'Error adding user.')
      setInviteError(true)
    }
  }

  // Scroll automatique chat à chaque nouveau message
  useEffect(() => {
    if (chatContainerRef.current) {
      setTimeout(() => {
        chatContainerRef.current!.scrollTo({
          top: chatContainerRef.current!.scrollHeight,
          behavior: 'smooth',
        })
      }, 50)
    }
  }, [messages])

  if (loading) return <Text>Loading...</Text>
  if (!collection) return <Text className="text-red-500">Collection not found.</Text>

  const selectedImage = collection.photos[modalIndex ?? 0]
  const filteredMessages = messages.filter(msg => msg.photoId === selectedImage?.id)

  type Props = {
    src: string
    alt: string
    className?: string
  }

  function ProtectedImage({ src, alt, className }: Props) {
    const [blobUrl, setBlobUrl] = useState<string | null>(null)

    useEffect(() => {
      let active = true

      // Vérifie si déjà en cache
      if (imageCache.has(src)) {
        setBlobUrl(imageCache.get(src)!)
        return
      }

      const loadImage = async () => {
        try {
          const res = await fetch(src, {
            headers: {
              Authorization: `Bearer ${localStorage.getItem("token") || ""}`,
            },
          })
          if (!res.ok) throw new Error("Failed to load image")

          const blob = await res.blob()
          const objectUrl = URL.createObjectURL(blob)

          if (active) {
            setBlobUrl(objectUrl)
            imageCache.set(src, objectUrl) // met en cache
          }
        } catch (err) {
          console.error("Error loading protected image:", err)
        }
      }

      loadImage()
      return () => {
        active = false
      }
    }, [src])

    if (!blobUrl) {
      return <div className="flex items-center justify-center text-gray-400"></div>
    }

    return <img src={blobUrl} alt={alt} className={className} />
  }

  return (
    <div className="p-6 max-w-screen-xl mx-auto space-y-10">
      {/* En-tête */}
      <div className="flex justify-between items-start">
        <div>
          <Heading>{collection.name}</Heading>
          <Text>{collection.description || 'No description.'}</Text>
        </div>
        <div className="flex gap-2">
          <Button onClick={() => setInviteOpen(true)}>Invite</Button>
          <Button className="btn-secondary" onClick={handleDownloadAllPhotos}>Download</Button>
          <Button onClick={() => setIsReportUserModalOpen(true)}>Report a user</Button>
          <Button onClick={() => router.push(`/collections/${id}/chat`)}>Open Chat</Button>
        </div>
      </div>

      {/* Upload photo */}
      <div className="flex flex-col md:flex-row items-start gap-4 mt-4">
        <input
          type="file"
          accept=".png,.jpg,.jpeg"
          onChange={(e) => setSelectedFiles(Array.from(e.target.files || []))}
          className="bg-red p-2 rounded border"
        />
        <Button
          onClick={handleUploadPhoto}
          disabled={uploading || isSending || !selectedFiles}
          className="self-start"
        >
          {uploading ? 'Uploading...' : 'Upload Photo'}
        </Button>
      </div>

      {/* Modal report */}
      {isReportUserModalOpen && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
          <div className="bg-zinc-800 text-white p-6 rounded-lg max-w-sm w-full space-y-4">
            <h2 className="text-lg font-bold">Report a user in this collection</h2>
            <div>
              <label className="block text-sm mb-1">Select user:</label>
              <select
                value={selectedUserToReport}
                onChange={(e) => setSelectedUserToReport(e.target.value)}
                className="w-full p-2 rounded bg-zinc-700 text-white"
              >
                <option value="" disabled>Select a user…</option>
                {collection.allowedEmails.map(email => (
                  <option key={email} value={email}>{email}</option>
                ))}
              </select>
            </div>
            <div>
              <textarea
                className="w-full p-2 rounded bg-zinc-700 text-white placeholder-zinc-400 mt-2"
                placeholder="Describe your reason..."
                value={reportReason}
                onChange={(e) => setReportReason(e.target.value)}
                rows={4}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button onClick={() => setIsReportUserModalOpen(false)}>Cancel</Button>
              <Button
                onClick={async () => {
                  if (!selectedUserToReport || !reportReason.trim()) {
                    alert("Please select a user and provide a reason.");
                    return;
                  }
                  try {
                    await CollectionService.reportUser({
                      collectionId: collection.id,
                      reportedUserEmail: selectedUserToReport,
                      reason: reportReason.trim(),
                    });
                    alert("Report submitted successfully!");
                    setIsReportUserModalOpen(false);
                    setSelectedUserToReport('');
                    setReportReason('');
                  } catch (err) {
                    console.error(err);
                    alert("Error submitting report");
                  }
                }}
              >
                Submit Report
              </Button>
            </div>
          </div>
        </div>
      )}

      {inviteOpen && (
        <div
          className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-70 z-50"
          style={{ height: '100vh', width: '100vw' }}
        >
          <div className="bg-zinc-800 text-white p-6 rounded-lg max-w-sm w-full mx-4">
            <h2 className="text-lg font-bold mb-4">Invite a user</h2>
            <Input
              placeholder="Enter user email"
              value={inviteEmail}
              onChange={(e) => setInviteEmail(e.target.value)}
              className="bg-zinc-700 text-white placeholder-zinc-400"
            />
            <div className="flex justify-end gap-2 mt-4">
              <Button onClick={() => setInviteOpen(false)}>Cancel</Button>
              <Button onClick={handleInvite} disabled={!inviteEmail.trim()}>
                Send Invitation
              </Button>
            </div>
          </div>
        </div>
      )}

{/* Photos + Chat */}
<div className="flex gap-6">
  {/* Photo sélectionnée */}
  <div className="flex-1 border rounded overflow-hidden relative">
    {selectedImage && (
      <>
        <div className="w-full h-[600px] flex items-center justify-center relative">
          <ProtectedImage
            src={selectedImage.filePath}
            alt="Current"
          />

          {/* Bouton statut
          <button
            className="absolute top-2 right-2 bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700 transition-colors"
            onClick={() => setShowStatusMenu(prev => !prev)}
            disabled={updatingStatus}
          >
            {updatingStatus ? 'Updating...' : 'Change Status'}
          </button>

          {/* Menu déroulant 
          {showStatusMenu && (
            <div className="absolute top-10 right-2 bg-zinc-800 border border-zinc-700 rounded shadow-lg z-50">
              {statusOptions.map(({ label, value }) => (
                <div
                  key={value}
                  className={`cursor-pointer px-4 py-2 hover:bg-blue-600 ${
                    selectedImage.status === value ? 'font-bold text-[#B91F1E]' : ''
                  }`}
                  onClick={() => updatePhotoStatus(value)}
                >
                  {label}
                </div>
              ))}
            </div>
          )}*/}
        </div>
     {/* <div className="mt-2 px-4 flex items-center gap-2">
        <span className="text-sm font-semibold bg-gray-300 text-gray-900 px-2 py-1 rounded dark:bg-gray-700 dark:text-gray-100 flex items-center gap-1">
          <span>Current status :</span>
          <span>{statusLabels[selectedImage.status]}</span>
        </span>
      </div> */}
      </>
    )}
  </div>

  {/* Zone chat */}
  {/* ...le reste de ta zone chat ici... */}


      {/* Zone chat */}
      <div className="w-1/3 flex flex-col border rounded p-4 bg-zinc-900 max-h-[600px] overflow-visible text-white">
        <Subheading className="text-white">Chat</Subheading>

        {/* Messages */}
        <div
          ref={chatContainerRef}
          className="flex-1 overflow-y-auto space-y-4 p-2 max-h-[520px]"
        >
          {filteredMessages.length === 0 && (
            <Text className="text-zinc-500 text-center mt-8">No messages yet.</Text>
          )}

    {filteredMessages.map((msg, index) => (
      <div
        key={index}
        className={`flex items-end gap-3 w-full ${
          msg.isMine ? 'justify-end' : 'justify-start'
        }`}
      >
    <div className="max-w-xs sm:max-w-md">
      <div className={`text-xs font-semibold text-zinc-400 mb-1 flex items-center ${msg.isMine ? 'justify-end' : 'justify-start'}`}>
        {msg.isMine ? (
          <>
            <span className="mr-2 text-blue-400">You</span>
            <span className="text-zinc-500 text-xs">
              {new Date(msg.sentAt).toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit',
              })}
            </span>
          </>
        ) : (
          <>
            <span className="mr-2 text-red-400">{msg.senderUsername}</span>
            <span className="text-zinc-500 text-xs">
              {new Date(msg.sentAt).toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit',
              })}
            </span>
          </>
        )}
      </div>
      <div
        className={`px-4 py-2 rounded-2xl break-words text-sm max-w-xs w-max ${
          msg.isMine ? 'bg-zinc-700 text-white text-right ml-auto' : 'bg-zinc-700 text-white text-left'
        }`}
      >
        {msg.message}
      </div>
    </div>
  </div>
))}

    </div>
    

    {/* Input + Bouton envoyer */}
    <div className="flex gap-3 mt-4">
      {/* Wrapper relatif autour de l'input + popup, prend toute la largeur possible */}
      <div className="relative flex-1">
        <Input
          placeholder="Your message..."
          value={chatMessage}
          onChange={(e) => setChatMessage(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleSendMessage()}
          className="w-full rounded border border-zinc-700 px-3 py-2 bg-zinc-800 text-white placeholder-zinc-500"
          disabled={isSending}
        />
        {mentionVisible && (
          <ul className="absolute z-50 bg-zinc-800 border border-zinc-700 rounded mt-1 max-w-xs max-h-40 overflow-y-auto text-white">
            {filteredEmails.map((email) => (
              <li
                key={email}
                className="px-3 py-1 cursor-pointer hover:bg-zinc-700"
                onClick={() => insertMention(email)}
              >
                {email}
              </li>
            ))}
          </ul>
        )}
      </div>

      {/* Bouton Send reste à côté */}
      <Button
        onClick={handleSendMessage}
        disabled={isSending}
        className="bg-blue-700 hover:bg-blue-600 text-white font-semibold px-5 py-1.5 rounded text-sm flex items-center justify-center"
      >
        Send
      </Button>
    </div>
  </div>
</div>
{/* Galerie photos en bas */}
<div className="grid grid-cols-8 gap-4 pt-6">
  {collection.photos.map((photo, index) => (
    <div
      key={photo.id}
      onClick={() => setModalIndex(index)}
      className={`relative cursor-pointer border rounded overflow-hidden shadow-sm ${
        modalIndex === index ? 'ring-2 ring-[#B91F1E]' : ''
      }`}
    >
      <ProtectedImage
        src={photo.filePath}
        alt={`Photo ${index + 1}`}
      />
    </div>
  ))}
</div>

</div>
  )
}
