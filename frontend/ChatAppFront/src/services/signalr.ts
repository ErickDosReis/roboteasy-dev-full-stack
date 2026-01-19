import type { OnlineUserDto, ReceivedMessageDto } from '@/types/chat'
import * as signalR from '@microsoft/signalr'

const GLOBAL_KEY = '__app_chat_signalr_service__' as const

export class SignalRService {
  private connection: signalR.HubConnection | null = null

  private url: string

  constructor() {
    this.url = `/hubs/chat`
  }

  public async start(token: string): Promise<void> {
    const globalService = window[GLOBAL_KEY] //const globalService = getGlobalSignalRService()

    if (globalService && globalService instanceof SignalRService) {
      console.warn('Encontrada instância antiga do SignalR. Encerrando para evitar duplicação...')
      await globalService.stop()
    }

    if (this.connection) {
      await this.stop()
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.url, {
        accessTokenFactory: () => token,

        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .build()

    try {
      await this.connection.start()

      console.log('SignalR Conectado via WebSocket')
      window[GLOBAL_KEY] = this
    } catch (error) {
      console.error('Erro ao conectar SignalR:', error)
      throw error
    }
  }

  public async stop(): Promise<void> {
    if (!this.connection) {
      if (window[GLOBAL_KEY] === this) delete window[GLOBAL_KEY]
      return
    }

    try {
      this.connection.off('ReceiveMessage')
      this.connection.off('UserOnline')
      this.connection.off('UserOffline')

      await this.connection.stop()
      console.log('Conexão SignalR encerrada.')
    } catch (error) {
      console.error('Erro ao parar conexão:', error)
    } finally {
      this.connection = null

      if (window[GLOBAL_KEY] === this) delete window[GLOBAL_KEY]
    }
  }

  public async sendMessageToUser(toUserId: string, sentMessage: string): Promise<void> {
    if (!this.connection) return
    try {
      await this.connection.invoke('SendMessageToUser', {
        toUserId,
        sentMessage,
      })
    } catch (error) {
      console.error('Erro ao enviar mensagem:', error)
    }
  }

  public async getOnlineUsers(): Promise<OnlineUserDto[]> {
    if (!this.connection) return []
    return await this.connection.invoke<OnlineUserDto[]>('GetOnlineUsers')
  }

  public onReceiveMessage(callback: (msg: ReceivedMessageDto) => void) {
    if (!this.connection) return
    this.connection.off('ReceiveMessage')
    this.connection.on('ReceiveMessage', callback)
  }

  public onUserOnline(callback: (userId: string, userName: string) => void) {
    if (!this.connection) return
    this.connection.off('UserOnline')
    this.connection.on('UserOnline', callback)
  }
  public onUserOffline(callback: (userId: string) => void) {
    if (!this.connection) return
    this.connection.off('UserOffline')
    this.connection.on('UserOffline', callback)
  }
}
export const signalRService = new SignalRService()
