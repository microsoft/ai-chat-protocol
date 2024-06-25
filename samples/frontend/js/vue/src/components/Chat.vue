<template>
    <div :style="style">
      <div v-for="message in messages" :key="message.id" class="message">
        {{ message.content }}
      </div>
      <input v-model="input" @keyup.enter="sendMessage" />
      <button @click="sendMessage">Send</button>
    </div>
  </template>

  <script setup>
  import { ref } from 'vue';

  const props = defineProps({
    style: Object,
  });

  const messages = ref([]);
  const input = ref('');

  const sendMessage = () => {
    messages.value.push({ id: Date.now(), content: input.value });
    input.value = '';
  };
  </script>

  <style scoped>
  .message {
    margin-bottom: 10px;
  }
  </style>