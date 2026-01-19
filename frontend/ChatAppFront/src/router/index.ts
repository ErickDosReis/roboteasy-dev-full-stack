import { createRouter, createWebHistory } from 'vue-router'

import ChatView from '../views/ChatView.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import UsersView from '../views/UsersView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),

  routes: [
    {
      path: '/',
      name: 'login',
      component: LoginView,
      meta: { cardSize: 'size-sm' },
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
      meta: { cardSize: 'size-sm' },
    },
    {
      path: '/users',
      name: 'users',
      component: UsersView,
      meta: { cardSize: 'size-md' },
    },
    {
      path: '/chat',
      name: 'chat',
      component: ChatView,
      meta: { cardSize: 'size-lg' },
    },
  ],
})

router.beforeEach((to, from, next) => {
  const publicPages = ['/', '/register']

  const authRequired = !publicPages.includes(to.path)

  const loggedIn = localStorage.getItem('token')

  if (authRequired && !loggedIn) {
    next('/')
  } else {
    next()
  }
})

export default router
