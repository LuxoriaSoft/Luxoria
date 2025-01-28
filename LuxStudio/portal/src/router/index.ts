import { createRouter, createWebHistory } from 'vue-router';
import Login from '../views/Login.vue';
import Register from '../views/Register.vue';
import Dashboard from '../views/Dashboard.vue';

function isTokenValid(token:string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp * 1000 > Date.now();
  } catch (e) {
    return false;
  }
}

const routes = [
  {
    path: '/',
    name: 'Login',
    component: Login,
  },
  {
    path: '/register',
    name: 'Register',
    component: Register,
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: Dashboard,
    meta: { requiresAuth: true }, // Route protégée
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token');
  const isAuthenticated = token && isTokenValid(token);

  if (to.meta.requiresAuth && !isAuthenticated) {
    // Si la route est protégée et le token est invalide ou absent
    next({ path: '/', query: { redirect: to.fullPath } }); // Redirige vers Login
  } else if ((to.name === 'Login' || to.name === 'Register') && isAuthenticated) {
    // Si l'utilisateur est connecté, empêcher l'accès à Login ou Register
    next('/dashboard');
  } else {
    // Autorise la navigation
    next();
  }
});

export default router;
