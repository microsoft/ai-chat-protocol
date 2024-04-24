// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { expect, test } from "vitest";

import { AIChatProtocolClient } from "../dist/commonjs/index.js";

test("Can send getCompletionRequest", async () => {
  const client = new AIChatProtocolClient("https://my.test.com/api/chat");
  const response = await client.getCompletion([
    { content: "Hello, world!", role: "user" },
  ]);
  expect(response).toEqual({
    message: { content: "Hello, world!", role: "assistant" },
  });
});
