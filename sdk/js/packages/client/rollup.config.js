import commonjs from "@rollup/plugin-commonjs";
import dts from "rollup-plugin-dts";
import typescript from '@rollup/plugin-typescript';
import nodeResolve from "@rollup/plugin-node-resolve";
import alias from "@rollup/plugin-alias";

export default [
  {
    input: "src/index.ts",
    output: [{
      file: "dist/cjs/index.js",
      format: "cjs",
      sourcemap: true
    }, {
      file: "dist/esm/index.js",
      format: "esm",
      sourcemap: true,
    }],
    plugins: [
      nodeResolve({
        preferBuiltins: true,
      }),
      commonjs(),
      typescript(),
    ]
  },
  {
    input: "src/index.ts",
    output: [{
      file: "dist/browser/index.js",
      format: "esm",
      sourcemap: true,
    }, {
      file: "dist/iife/index.js",
      format: "iife",
      name: "ChatProtocol",
      sourcemap: true,
    }],
    plugins: [
      alias({
        entries: [
          { find: "./util/stream.js", replacement: "./util/stream-browser.mjs" },
          { find: "./util/url.js", replacement: "./util/url-browser.mjs" },
        ]
      }),
      typescript(),
      nodeResolve({
        browser: true,
        preferBuiltins: true,
      }),
    ],
  },
  {
    input: "src/index.ts",
    output: [{
      file: "dist/index.d.ts",
      format: "es",
      sourcemap: true,
    }],
    plugins: [
      dts(),
    ]
  },
];
