import { createRouter, createWebHistory } from 'vue-router';
import Login from '../views/Login.vue';
import Register from '../views/Register.vue';
import Dashboard from '../views/Dashboard.vue';
import LinkAccount from '../views/SSO_Authorize.vue';
import Protected from '../views/Protected.vue';

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
  { 
    path: '/sso/authorize',
    name: 'LinkAccount',
    component: LinkAccount,
    meta: { requiresAuth: true }, // Route protégée
  },
  {
    path: '/protected',
    name: 'Protected',
    component: Protected,
    meta: { requiresAuth: true }, // Route protégée
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach(async (to, from, next) => {
  let token = localStorage.getItem("token");

  if (to.meta.requiresAuth) {
    const isTokenExpired = (token) => {
      if (!token) return true;
      try {
        const payload = JSON.parse(atob(token.split(".")[1]));
        return payload.exp * 1000 < Date.now();
      } catch (e) {
        return true;
      }
    };

    if (!token || isTokenExpired(token)) {
      console.log("Token expired, trying to refresh...");

      const newToken = await authService.refreshToken();
      if (!newToken) {
        console.log("Redirecting to login...");
        return next({ path: "/", query: { redirect: to.fullPath } });
      }
    }
  }

  next();
});

export default router;