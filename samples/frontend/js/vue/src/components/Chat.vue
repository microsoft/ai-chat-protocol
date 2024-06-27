<template>
    <div class="chat-container">
        <div class="messages" ref="messagesContainer">
      <div v-for="message in messages" :key="message.id" :class="['message', message.role]">
        {{ message.content }}
      </div>
      </div>
      <div class="input-area">
          <textarea v-model="input" @keyup.shift.enter="sendMessage"></textarea>
          <button @click="sendMessage">Send</button>
      </div>
    </div>
  </template>

  <script setup>
  import { ref } from 'vue';


  const props = defineProps({
    style: Object,
  });

  const messages = ref([{ id: 1, content: "Hello there!", type: "user" },
  { id: 2, content: "Hi! How can I help you?", type: "assistant" },]);
  const input = ref('');
    const messagesContainer = ref(null);

  const sendMessage = () => {
    messages.value.push({ id: Date.now(), content: input.value });
    input.value = '';
  };

  const onUpdate = () => {
  const container = messagesContainer.value;
  if (container) {
    container.scrollTop = container.scrollHeight;
  }
};
  </script>

  <style scoped>
.message.user .message-bubble {
  background-color: #dcf8c6; /* Light green for outgoing messages */
  text-align: right;
}

.message.assistant .message-bubble {
  background-color: #ece5dd; /* Light gray for incoming messages */
  text-align: left;
}

.message-bubble {
  padding: 10px;
  margin: 5px 0;
  border-radius: 20px;
  display: inline-block;
  max-width: 80%;
}
  </style>