import { defineWorkspace } from "vitest/config";

export default defineWorkspace([
  {
    test: {
      name: "node",
      include: ["test/**/*.{node,shared}.test.ts"],
      coverage: {
        provider: 'v8',
        reporter: ['text', 'json', 'html', 'lcov'],
        reportsDirectory: './coverage',
        exclude: [
          'node_modules/**',
          'dist/**',
          'test/**',
          '**/*.d.ts',
          '**/*.config.*',
          '**/mockData.ts',
          'src/index.ts', // Just re-exports
        ],
        include: ['src/**/*.ts'],
        all: true,
        lines: 80,
        functions: 80,
        branches: 80,
        statements: 80,
      },
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
