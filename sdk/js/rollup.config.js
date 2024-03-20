import dts from "rollup-plugin-dts";

export default [
  {
    input: ["dist/browser/index.js"],
    plugins: [dts()],
    output: {
      format: "umd",
      file: "dist/bundle/chat-protocol.umd.d.ts",
      name: "ChatProtocol",
    },
  },
  {
    input: ["dist/browser/index.js"],
    output: {
      format: "umd",
      sourcemap: true,
      file: "dist/bundle/chat-protocol.umd.js",
      name: "ChatProtocol",
    },
  },
];
