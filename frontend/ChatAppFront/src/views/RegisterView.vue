<script setup lang="ts">
import { useChatStore } from '@/stores/chatStore'
import type { IdentityError, IdentityErrorResponse, RegisterFormErrors } from '@/types/chat'
import { isAxiosError, isIdentityErrorArray } from '@/utils'
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'

const chatStore = useChatStore()
const router = useRouter()

const form = reactive({ username: '', email: '', password: '' })
const isLoading = ref(false)

const formErrors = reactive<RegisterFormErrors>({
  username: [],
  email: [],
  password: [],
  general: [],
})

const clearErrors = () => {
  formErrors.username = []
  formErrors.email = []
  formErrors.password = []
  formErrors.general = []
}

const handleRegister = async () => {
  clearErrors()
  isLoading.value = true
  try {
    await chatStore.register({ ...form })
    alert('Conta criada com sucesso! Redirecionando para login...')
    router.push('/')
  } catch (error: unknown) {
    if (isAxiosError<IdentityErrorResponse>(error)) {
      const data = error.response?.data
      if (isIdentityErrorArray(data)) {
        mapIdentityErrorsToFields(data)
        return
      }
      formErrors.general.push(error.response ? 'Erro no servidor.' : 'Falha de conexão.')
    } else {
      formErrors.general.push('Ocorreu um erro inesperado.')
    }
  } finally {
    isLoading.value = false
  }
}

const mapIdentityErrorsToFields = (errors: IdentityError[]) => {
  errors.forEach((err) => {
    const code = err.code.toLowerCase()
    if (code.includes('password')) formErrors.password.push(err.description)
    else if (code.includes('username') || code.includes('user'))
      formErrors.username.push(err.description)
    else if (code.includes('email')) formErrors.email.push(err.description)
    else formErrors.general.push(err.description)
  })
}
</script>

<template>
  <div class="padded">
    <h2>Criar Conta</h2>

    <div v-if="formErrors.general.length" class="alert-box">
      <ul>
        <li v-for="(err, idx) in formErrors.general" :key="idx">{{ err }}</li>
      </ul>
    </div>

    <form @submit.prevent="handleRegister">
      <div class="form-group">
        <input
          v-model="form.username"
          type="text"
          placeholder="Usuário"
          :class="{ 'input-error': formErrors.username.length }"
          required
          class="form-input"
        />
        <small v-for="err in formErrors.username" :key="err" class="error-msg">{{ err }}</small>
      </div>

      <div class="form-group">
        <input
          v-model="form.email"
          type="email"
          placeholder="Email"
          :class="{ 'input-error': formErrors.email.length }"
          required
          class="form-input"
        />
        <small v-for="err in formErrors.email" :key="err" class="error-msg">{{ err }}</small>
      </div>

      <div class="form-group">
        <input
          v-model="form.password"
          type="password"
          placeholder="Senha"
          :class="{ 'input-error': formErrors.password.length }"
          required
          class="form-input"
        />
        <small v-for="err in formErrors.password" :key="err" class="error-msg">{{ err }}</small>
      </div>

      <button type="submit" :disabled="isLoading" class="btn-primary w-full">
        {{ isLoading ? 'Criando conta...' : 'Cadastrar' }}
      </button>
    </form>

    <p class="switch-auth">Já tem conta? <router-link to="/">Faça Login</router-link></p>
  </div>
</template>

<style scoped>
/* Estilos locais apenas para ajustes finos de formulário */
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
.input-error {
  border-color: #dc3545;
  background-color: #fff8f8;
}
.error-msg {
  display: block;
  color: #dc3545;
  font-size: 0.8rem;
  margin-top: 4px;
}

.alert-box {
  background: #ffe6e6;
  border: 1px solid #ffcccc;
  color: #cc0000;
  padding: 10px;
  border-radius: 4px;
  margin-bottom: 15px;
  font-size: 0.9rem;
}
.w-full {
  width: 100%;
}

.switch-auth {
  margin-top: 20px;
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
