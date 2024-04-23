import { fileURLToPath } from "url";
import { defineWorkspace } from "vitest/config";

export default defineWorkspace([
  {
    test: {
      name: "node",
      include: ["test/**/*.{node,shared}.test.ts"],
    },
  },
  {
    test: {
      name: "browser",
      include: ["test/**/*.{browser,shared}.test.ts"],
      browser: {
        enabled: true,
        provider: "playwright",
        name: "chromium",
        headless: true,
      },
    },
  },
]);
