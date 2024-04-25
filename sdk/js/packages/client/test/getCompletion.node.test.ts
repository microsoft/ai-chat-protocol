// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { expect, test } from "vitest";

import { AIChatProtocolClient } from "../dist/commonjs/index.js";

test("Can send getCompletionRequest", async () => {
  const client = new AIChatProtocolClient("https://my.test.com/api/chat");
  const { message, context } = await client.getCompletion([
    { content: "Hello, world!", role: "user" },
  ]);
  expect(message).toEqual({ content: "Hello, world!", role: "assistant" });
  expect(context).toEqual({
    payload: { messages: [{ content: "Hello, world!", role: "user" }] },
  });
});
