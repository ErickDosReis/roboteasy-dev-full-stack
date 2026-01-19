import { apiAuth, apiChatMessage } from '@/services/api'
import { signalRService } from '@/services/signalr'
import type {
  AuthResponse,
  ChatMessage,
  LoginRequest,
  OnlineUserDto,
  ReceivedMessageDto,
  RegisterRequest,
  UserProfile,
} from '@/types/chat'
import type { AxiosError, AxiosResponse } from 'axios'
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useChatStore = defineStore('chat', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const currentUser = ref<UserProfile | null>(null)
  const onlineUsers = ref<OnlineUserDto[]>([])
  const activeChatUserId = ref<string | null>(localStorage.getItem('activeChatId'))
  const messages = ref<Record<string, ChatMessage[]>>({})

  async function register(data: RegisterRequest): Promise<boolean> {
    try {
      const response: AxiosResponse<void> = await apiAuth.post<void>('/register', data)

      return response.status === 201
    } catch (error: unknown) {
      const axiosErr = error as AxiosError

      console.error('Erro no registro:', axiosErr.message)

      throw error
    }
  }

  async function login(data: LoginRequest): Promise<boolean> {
    try {
      const response = await apiAuth.post<AuthResponse>('/login', data)

      const accessToken = response.data.token

      token.value = accessToken

      localStorage.setItem('token', accessToken)

      await signalRService.start(accessToken)

      onlineUsers.value = await signalRService.getOnlineUsers()

      await fetchCurrentUser()

      setupListeners()
      return true
    } catch (error) {
      console.error('Erro no login:', error)
      return false
    }
  }

  async function fetchCurrentUser() {
    try {
      const response = await apiAuth.get<UserProfile>('/me')
      currentUser.value = response.data
    } catch (error) {
      console.error('Falha ao obter perfil do usuário', error)
      logout()
    }
  }

  async function checkAuth() {
    if (!token.value) return

    try {
      await fetchCurrentUser()

      try {
        await signalRService.start(token.value)

        // Pequeno delay para garantir que o servidor processou a conexão antes de pedir a lista
        await new Promise((r) => setTimeout(r, 500))

        onlineUsers.value = await signalRService.getOnlineUsers()

        setupListeners()
      } catch (wsError) {
        console.error('Aviso: Token válido, mas falha ao conectar no Chat em Tempo Real.', wsError)
        // Não chamamos logout() aqui! O usuário pode ver o histórico mesmo offline do socket.
      }
      setupListeners()
    } catch (error) {
      console.error('Sessão inválida ou expirada.', error)
      logout()
    }
  }

  function logout() {
    token.value = null
    currentUser.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('activeChatId')
    signalRService.stop()
  }

  function setupListeners() {
    signalRService.onReceiveMessage((dto: ReceivedMessageDto) => {
      const isMine = dto.fromUserId === currentUser.value?.userId
      const chatPartnerId = isMine ? activeChatUserId.value! : dto.fromUserId
      const newMessage: ChatMessage = {
        id: crypto.randomUUID(),
        senderId: dto.fromUserId,
        senderName: dto.fromUserName,
        text: dto.receivedMessage,
        timestamp: new Date(dto.sentAtUtc),
        isMine: isMine,
      }
      addMessage(chatPartnerId, newMessage)
    })

    signalRService.onUserOnline((userId: string, userName: string) => {
      if (userId !== currentUser.value?.userId) {
        if (!onlineUsers.value.find((u) => u.userId === userId)) {
          onlineUsers.value.push({ userId, userName })
        }
      }
    })
  }

  function sendMessage(text: string) {
    if (!activeChatUserId.value || !currentUser.value) return
    signalRService.sendMessageToUser(activeChatUserId.value, text)

    const optimisticMsg: ChatMessage = {
      senderId: currentUser.value.userId,
      senderName: currentUser.value.userName,
      text: text,
      timestamp: new Date(),
      isMine: true,
    }

    addMessage(activeChatUserId.value, optimisticMsg)
  }

  async function fetchMessages(targetUserId: string) {
    try {
      const response = await apiChatMessage.get<ChatMessage[]>(`/${targetUserId}`)

      messages.value[targetUserId] = response.data.map((m) => ({
        ...m,
        timestamp: new Date(m.timestamp),
      }))
    } catch (error) {
      console.error('Erro ao baixar histórico:', error)
    }
  }

  function addMessage(userId: string, msg: ChatMessage) {
    if (!messages.value[userId]) messages.value[userId] = []
    messages.value[userId].push(msg)
  }

  async function selectUser(userId: string) {
    activeChatUserId.value = userId
    localStorage.setItem('activeChatId', userId)
    // Assim que seleciona o usuário, baixa o histórico
    await fetchMessages(userId)
  }

  return {
    login,
    token,
    logout,
    messages,
    register,
    checkAuth,
    selectUser,
    sendMessage,
    currentUser,
    onlineUsers,
    fetchMessages,
    activeChatUserId,
  }
})
