<template>
    <div :style="style">
      <div v-html="markdownContent"></div>
    </div>
  </template>

  <script setup>
  import { onMounted, ref } from 'vue';
  import { marked } from 'marked'; // Use marked for markdown parsing

  const props = defineProps({
    style: Object,
  });

  const markdownContent = ref('');

  onMounted(async () => {
    const response = await fetch('/README.md');
    const text = await response.text();
    markdownContent.value = marked(text);
  });
  </script>