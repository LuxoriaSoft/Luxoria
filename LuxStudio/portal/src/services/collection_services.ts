import axios from 'axios'

export async function getCollections() {
  const token = localStorage.getItem('token') // ← récupère le token JWT stocké après login

  const response = await axios.get(`${window.appConfig.API_URL}/api/collection`, {
    headers: {
      Authorization: `Bearer ${token}` // ← injecte le token ici
    }
  })

  return response.data
}
