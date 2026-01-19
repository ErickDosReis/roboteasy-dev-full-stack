<script setup lang="ts">
import { useChatStore } from '@/stores/chatStore'
import { computed, nextTick, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const chatStore = useChatStore()
const router = useRouter()
const messageInput = ref('')
const messagesContainer = ref<HTMLElement | null>(null)

const currentMessages = computed(() => {
  if (!chatStore.activeChatUserId) return []
  return chatStore.messages[chatStore.activeChatUserId] || []
})

const chatPartnerName = computed(() => {
  const partner = chatStore.onlineUsers.find((u) => u.userId === chatStore.activeChatUserId)
  return partner ? partner.userName : null
})

const scrollToBottom = async () => {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

watch(currentMessages, () => scrollToBottom(), { deep: true })

onMounted(async () => {
  if (!chatStore.activeChatUserId) {
    router.push('/users')
    return
  }
  await chatStore.fetchMessages(chatStore.activeChatUserId)
  scrollToBottom()
})

const handleSend = () => {
  const text = messageInput.value.trim()
  if (!text) return

  chatStore.sendMessage(text)
  messageInput.value = ''
}

const goBack = () => router.push('/users')
</script>

<template>
  <div class="chat-content">
    <header class="chat-header">
      <button @click="goBack" class="back-btn"><span class="icon">❮</span> Voltar</button>
      <div class="header-title">
        <h3>{{ chatPartnerName ?? 'Carregando...' }}</h3>
        <span class="status-indicator">Online</span>
      </div>
      <div class="header-placeholder"></div>
    </header>

    <div class="messages-area" ref="messagesContainer">
      <div v-if="currentMessages.length === 0" class="empty-state">
        <div class="empty-icon">👋</div>
        <p>
          Inicie a conversa com <strong>{{ chatPartnerName }}</strong>
        </p>
      </div>

      <div
        v-for="msg in currentMessages"
        :key="msg.id"
        class="message-row"
        :class="{ mine: msg.isMine }"
      >
        <div class="message-bubble">
          <p class="msg-text">{{ msg.text }}</p>
          <span class="timestamp">
            {{
              new Date(msg.timestamp).toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit',
              })
            }}
          </span>
        </div>
      </div>
    </div>

    <footer class="input-area">
      <form @submit.prevent="handleSend" class="input-form">
        <input
          v-model="messageInput"
          type="text"
          placeholder="Digite sua mensagem..."
          class="input-rounded msg-input"
        />
        <button type="submit" class="send-btn" :disabled="!messageInput.trim()">➤</button>
      </form>
    </footer>
  </div>
</template>

<style scoped>
/* NOVO ESTILO DA RAIZ:
  Garante que o conteúdo preencha 100% do Card pai (que está no App.vue)
  e organize os filhos (header, area, footer) em coluna.
*/
.chat-content {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.chat-header {
  padding: 15px 20px;
  border-bottom: 1px solid var(--border-light);
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-shrink: 0; /* Header não encolhe */
}

.back-btn {
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  gap: 5px;
}
.back-btn:hover {
  color: var(--text-primary);
}

.header-title {
  text-align: center;
}
.header-title h3 {
  margin: 0;
  font-size: 1.1rem;
  color: var(--text-primary);
}
.status-indicator {
  font-size: 0.75rem;
  color: var(--primary-color);
  font-weight: 500;
}
.header-placeholder {
  width: 40px;
}

.messages-area {
  flex: 1; /* Ocupa todo o espaço restante */
  padding: 20px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.message-row {
  display: flex;
  width: 100%;
}
.message-row.mine {
  justify-content: flex-end;
}

.message-bubble {
  max-width: 65%;
  padding: 10px 16px;
  border-radius: var(--border-radius-bubble);
  position: relative;
  font-size: 0.95rem;
  line-height: 1.4;
  word-wrap: break-word;
}

.message-row:not(.mine) .message-bubble {
  background-color: var(--msg-received);
  color: var(--text-primary);
  border-bottom-left-radius: 4px;
}

.message-row.mine .message-bubble {
  background-color: var(--msg-sent);
  color: var(--text-inverse);
  border-bottom-right-radius: 4px;
  box-shadow: 0 1px 2px rgba(66, 185, 131, 0.2);
}

.timestamp {
  display: block;
  font-size: 0.65rem;
  text-align: right;
  margin-top: 4px;
  opacity: 0.7;
}

.input-area {
  padding: 15px 20px;
  border-top: 1px solid var(--border-light);
  flex-shrink: 0;
}

.input-form {
  display: flex;
  gap: 12px;
  align-items: center;
}

.msg-input {
  flex: 1;
}

.send-btn {
  background: var(--primary-color);
  color: white;
  border: none;
  width: 42px;
  height: 42px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  transition: transform 0.1s;
}

.send-btn:hover:not(:disabled) {
  background: var(--primary-hover);
  transform: scale(1.05);
}

.send-btn:disabled {
  background: #ccc;
  cursor: default;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: var(--text-muted);
}
.empty-icon {
  font-size: 3rem;
  margin-bottom: 10px;
}
</style>
