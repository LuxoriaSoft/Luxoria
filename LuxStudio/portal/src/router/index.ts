import { createRouter, createWebHistory } from 'vue-router';
import Login from '../views/Login.vue';
import Register from '../views/Register.vue';
import Dashboard from '../views/Dashboard.vue';
import LinkAccount from '../views/SSO_Authorize.vue';
import Collections from '../views/Collections.vue';

/**
 * Defines the application routes.
 */
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
    path: '/collections',
    name: 'Collections',
    component: Collections,
    meta: { requiresAuth: true }
  }
];

/**
 * Creates the Vue Router instance with history mode enabled.
 */
const router = createRouter({
  history: createWebHistory(),
  routes,
});

/**
 * Global navigation guard to check authentication before entering protected routes.
 */
router.beforeEach(async (to, _, next) => {
  let token = localStorage.getItem("token");

  /**
   * Checks if the provided JWT token is expired.
   * @param {string} token - The JWT token.
   * @returns {boolean} - True if token is expired, false otherwise.
   */
  const isTokenExpired = (token: string | null): boolean => {
    if (!token) return true;
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      return payload.exp * 1000 < Date.now();
    } catch (e) {
      return true;
    }
  };

  // If the route requires authentication, check the token validity
  if (to.meta.requiresAuth) {
    if (!token || isTokenExpired(token)) {
      console.log("Token expired, redirecting to login...");
      return next({ path: "/", query: { redirect: to.fullPath } });
    }
  }

  next(); // Proceed to the next route
});


export default router;