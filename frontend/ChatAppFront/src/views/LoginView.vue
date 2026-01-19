<script setup lang="ts">
import { useChatStore } from '@/stores/chatStore'
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'

const chatStore = useChatStore()
const router = useRouter()

const form = reactive({ username: '', password: '' })
const isLoading = ref(false)
const errorMessage = ref('')

const handleLogin = async () => {
  isLoading.value = true
  errorMessage.value = ''
  const success = await chatStore.login({ ...form })
  if (success) router.push('/users')
  else errorMessage.value = 'Usuário ou senha inválidos.'
  isLoading.value = false
}
</script>

<template>
  <div class="padded">
    <h2>Entrar</h2>
    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <input
          v-model="form.username"
          type="text"
          placeholder="Usuário"
          required
          class="form-input"
        />
      </div>
      <div class="form-group">
        <input
          v-model="form.password"
          type="password"
          placeholder="Senha"
          required
          class="form-input"
        />
      </div>

      <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

      <button type="submit" :disabled="isLoading" class="btn-primary w-full">
        {{ isLoading ? 'Entrando...' : 'Entrar' }}
      </button>
    </form>
    <p class="switch-auth">Não tem conta? <router-link to="/register">Cadastre-se</router-link></p>
  </div>
</template>

<style scoped>
.padded {
  padding: 2rem;
}
.form-group {
  margin-bottom: 15px;
}
.form-input {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  box-sizing: border-box;
}
.w-full {
  width: 100%;
}
.error {
  color: #dc3545;
  font-size: 0.9rem;
  margin-bottom: 10px;
}
.switch-auth {
  margin-top: 15px;
  text-align: center;
  font-size: 0.9rem;
  color: var(--text-secondary);
}
.switch-auth a {
  color: var(--primary-color);
  text-decoration: none;
  font-weight: bold;
}
</style>
