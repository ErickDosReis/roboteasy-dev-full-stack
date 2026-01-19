<script setup lang="ts">
import { useChatStore } from '@/stores/chatStore'
import { onMounted } from 'vue'
import { RouterView, useRoute } from 'vue-router'

const chatStore = useChatStore()
// Hook para acessar os metadados da rota atual (cardSize)
const route = useRoute()

onMounted(() => {
  chatStore.checkAuth()
})
</script>

<template>
  <div class="app-container">
    <div class="app-card animatable" :class="route.meta.cardSize || 'size-md'">
      <RouterView v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </RouterView>
    </div>
  </div>
</template>

<style>
/* Define o comportamento do CSS global.
  Já que importamos o main.css no main.ts, não precisamos reimportar aqui.
*/

body {
  margin: 0;
  font-family: 'Inter', sans-serif; /* Garante a fonte definida nas variáveis */
  background-color: var(--bg-app); /* Evita flash branco no load */
}

/* --- Lógica da Animação --- */

/* Aplica transição em TODAS as propriedades (width, height, max-width).
  O cubic-bezier dá um efeito mais "elástico" e natural que o 'linear' ou 'ease'.
*/
.animatable {
  transition: all 0.5s cubic-bezier(0.25, 0.8, 0.25, 1);
}

/* Animação do CONTEÚDO (Fade In/Out).
  Enquanto o card estica (.animatable), o conteúdo velho some e o novo aparece.
*/
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
