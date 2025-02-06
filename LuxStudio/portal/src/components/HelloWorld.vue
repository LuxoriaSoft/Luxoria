<script setup lang="ts">
import { ref } from "vue";

defineProps<{ msg: string }>();

const count = ref(0);
</script>

<template>
  <div>
    <h1 className="text-5xl font-bold">Welcome to Luxstudio</h1>
  </div>

  <h1>{{ msg }}</h1>

  <div class="container mx-auto">
    <button class="btn" @click="count++">count is {{ count }}</button>

    <div class="mockup-code">
      <pre data-prefix="$"><code>edit components/HelloWorld.vue</code></pre>
    </div>
  </div>

  <p>
    Check out
    <a href="https://vuejs.org/guide/quick-start.html#local" target="_blank">create-vue</a>, the official Vue + Vite
    starter
  </p>
  <p>
    Learn more about IDE Support for Vue in the
    <a href="https://vuejs.org/guide/scaling-up/tooling.html#ide-support" target="_blank">Vue Docs Scaling up Guide</a>.
  </p>
  <p class="read-the-docs">Click on the Vite and Vue logos to learn more</p>
</template>
