import axios, { type AxiosInstance } from 'axios'

export const apiAuth = axios.create({
  baseURL: import.meta.env.VITE_API_HOST_AUTH,
})

export const apiUser = axios.create({
  baseURL: import.meta.env.VITE_API_HOST_USER,
})

export const apiChatMessage = axios.create({
  baseURL: import.meta.env.VITE_API_HOST_CHAT,
})

function setAxiosAuthInterceptor(instance: AxiosInstance) {
  instance.interceptors.request.use(
    (config) => {
      const urlToken = new URLSearchParams(window.location.search).get('token')

      const storageToken = localStorage.getItem('token')

      const token = urlToken ?? storageToken

      if (token) {
        if (urlToken) {
          localStorage.setItem('token', urlToken)

          const url = new URL(window.location.href)
          url.searchParams.delete('token')
          window.history.replaceState({}, document.title, url.toString())
        }
        config.headers = config.headers ?? {}

        console.log('Adicionando Authorization Header com Bearer token', `Bearer ${token}`)
        config.headers.Authorization = `Bearer ${token}`
      }

      return config
    },
    (error) => {
      return Promise.reject(error)
    },
  )
}
const apisList = [apiAuth, apiUser, apiChatMessage]

apisList.forEach(setAxiosAuthInterceptor)

export default apiAuth
