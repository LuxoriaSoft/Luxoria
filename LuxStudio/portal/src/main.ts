import { createApp } from 'vue';
import './style.css';
import App from './App.vue';
import router from './router'; // Import the router

/**
 * Creates and initializes the Vue application.
 * Applies global configurations such as routing.
 */
createApp(App)
  .use(router) // Use the router for navigation
  .mount('#app'); // Mount the app to the #app element
