<script setup lang="ts">
import { useChatStore } from '@/stores/chatStore'
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const chatStore = useChatStore()
const router = useRouter()

const availableUsers = computed(() => {
  return chatStore.onlineUsers.filter((u) => u.userId !== chatStore.currentUser?.userId)
})

onMounted(() => chatStore.checkAuth())

const handleSelectUser = (userId: string) => {
  chatStore.selectUser(userId)
  router.push('/chat')
}

const handleLogout = () => {
  chatStore.logout()
  router.push('/')
}
</script>

<template>
  <div class="users-content">
    <header class="panel-header">
      <div class="user-info">
        <h3>Olá, {{ chatStore.currentUser?.userName }}</h3>
        <span class="status-badge">● Online</span>
      </div>
      <button @click="handleLogout" class="logout-btn">Sair</button>
    </header>

    <main class="panel-body">
      <h2 class="section-title">Usuários Disponíveis ({{ availableUsers.length }})</h2>

      <div v-if="availableUsers.length === 0" class="empty-state">
        <p>Nenhum usuário online.</p>
        <small>Aguardando conexões...</small>
      </div>

      <ul v-else class="user-list">
        <li
          v-for="user in availableUsers"
          :key="user.userId"
          @click="handleSelectUser(user.userId)"
          class="user-item"
        >
          <div class="avatar">{{ user.userName.charAt(0).toUpperCase() }}</div>
          <div class="user-details">
            <span class="name">{{ user.userName }}</span>
            <span class="action-text">Clique para conversar</span>
          </div>
          <div class="status-dot"></div>
        </li>
      </ul>
    </main>
  </div>
</template>

<style scoped>
/* Define a altura total para ocupar 100% do card pai
  e organiza header/body em coluna
*/
.users-content {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.panel-header {
  padding: 1.5rem;
  border-bottom: 1px solid var(--border-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0; /* Header não encolhe */
}

.user-info h3 {
  margin: 0;
  color: var(--text-primary);
  font-size: 1.2rem;
}
.status-badge {
  font-size: 0.85rem;
  color: var(--primary-color);
  font-weight: 600;
}

.logout-btn {
  background: white;
  border: 1px solid #dc3545;
  color: #dc3545;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  transition: 0.2s;
}
.logout-btn:hover {
  background: #dc3545;
  color: white;
}

.panel-body {
  flex: 1; /* Ocupa o resto do espaço e permite scroll */
  padding: 1.5rem;
  overflow-y: auto;
}

.section-title {
  font-size: 1rem;
  color: var(--text-secondary);
  margin-bottom: 1rem;
  text-transform: uppercase;
  font-weight: 600;
}

.user-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.user-item {
  display: flex;
  align-items: center;
  padding: 12px;
  border-radius: 6px;
  margin-bottom: 8px;
  cursor: pointer;
  transition: background 0.2s;
}
.user-item:hover {
  background-color: #f8f9fa;
}

.avatar {
  width: 42px;
  height: 42px;
  background: #e0e0e0;
  color: #555;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  margin-right: 15px;
}

.user-details {
  flex: 1;
  display: flex;
  flex-direction: column;
}
.name {
  font-weight: 600;
  color: var(--text-primary);
}
.action-text {
  font-size: 0.8rem;
  color: var(--text-muted);
}
.status-dot {
  width: 8px;
  height: 8px;
  background: var(--primary-color);
  border-radius: 50%;
}

.empty-state {
  text-align: center;
  margin-top: 3rem;
  color: var(--text-muted);
}
</style>
