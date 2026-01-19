import type { SignalRService } from '@/services/signalr'

declare global {
  interface Window {
    __app_chat_signalr_service__?: SignalRService
  }
}

export {}
