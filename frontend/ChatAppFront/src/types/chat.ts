// DTOs de Entrada (casam com LoginRequestDto e RegisterRequestDto)
export interface LoginRequest {
  username: string
  password: string // Opcional no front se formos usar só para simular
}

// O objeto que vem no evento "ReceiveMessage" do Hub
export interface ReceivedMessageDto {
  fromUserId: string
  fromUserName: string
  receivedMessage: string
  sentAtUtc: string // DateTime vem como string ISO no JSON
}

// O objeto retornado por "GetOnlineUsers"
export interface OnlineUserDto {
  userId: string
  userName: string
  connectionId?: string
}

// Representação interna para nossa Store
export interface ChatMessage {
  id?: string
  senderId: string
  senderName: string
  text: string
  timestamp: Date
  isMine: boolean
}

// Espelho do C#: RegisterRequestDto
export interface RegisterRequest {
  username: string
  email: string
  password: string
}

// Resposta do endpoint /login
export interface AuthResponse {
  token: string
}

// Resposta do endpoint /me
export interface UserProfile {
  userId: string
  userName: string
}

// Adicione ao final de src/types/auth.ts

export interface IdentityError {
  code: string
  description: string
}

// Interface para controlar os erros no formulário visual
export interface RegisterFormErrors {
  username: string[]
  email: string[]
  password: string[]
  general: string[]
}

export type IdentityErrorResponse = IdentityError[]
