import { defineWorkspace } from "vitest/config";

export default defineWorkspace([
  {
    test: {
      name: "node",
      include: ["test/**/*.{node,common}.test.ts"],
      setupFiles: ["./test/setup/node.ts"],
    },
  },
  {
    test: {
      name: "browser",
      include: ["test/**/*.{browser,common}.test.ts"],
      alias: [
        {
          find: "./util/stream.js",
          replacement: "./util/stream-browser.mjs",
        },
        {
          find: "./util/url.js",
          replacement: "./util/url-browser.mjs",
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
