<template>
  <div class="app-container">
    <Chat style="flex: 1" />
    <Readme style="flex: 1" />
  </div>
</template>

<script setup>
import Chat from './components/Chat.vue';
import Readme from './components/Readme.vue';
</script>

<style scoped>
.app-container {
  display: flex;
}
</style>
