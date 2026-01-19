import { fileURLToPath, URL } from 'node:url'

import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'
import vueDevTools from 'vite-plugin-vue-devtools'

export default defineConfig({
  plugins: [vue(), vueDevTools()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    proxy: {
      //Só configurando aqui para meus testes com npm run dev
      '/api': {
        target: 'http://localhost:5127',
        changeOrigin: true,
        secure: false,
      },

      '/hubs': {
        target: 'http://localhost:5127',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
})
