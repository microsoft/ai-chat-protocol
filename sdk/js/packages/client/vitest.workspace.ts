import { defineWorkspace } from "vitest/config";

export default defineWorkspace([
  {
    test: {
      name: "node",
      environment: 'node',
      include: ["test/**/*.{node,common}.test.ts"],
      setupFiles: ["./test/setup/node.ts"],
    },
  },
  {
    test: {
      name: "browser",
      environment: 'jsdom',
      include: ["test/**/*.{browser,common}.test.ts"],
      alias: [
        {
          find: /^(.+)\/stream.js/,
          replacement: "$1/stream-browser.mjs",
        },
        {
          find: /^(.+)\/url.js/,
          replacement: "$1/url-browser.mjs",
        },
      ],
      browser: {
        enabled: true,
        provider: "playwright",
        name: "chromium",
        headless: true,
      },
      setupFiles: ["./test/setup/browser.ts"],
    },
  },
]);
