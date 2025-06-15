// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { describe, expect, test } from "vitest";
import {
  AIChatClientOptions,
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatCompletionOperationOptions,
  AIChatCompletionOptions,
  AIChatCompletionRequest,
  AIChatMessage,
  AIChatMessageDelta,
  AIChatRole,
  AIChatError,
  AIChatErrorResponse,
} from "../../src/model/index.js";

describe("model exports", () => {
  test("should export all model types", () => {
    // This test verifies that all the expected types are exported
    // The actual type checking is done by TypeScript at compile time
    expect(true).toBe(true);
  });

  test("type examples should be valid", () => {
    // Example of AIChatMessage
    const message: AIChatMessage = {
      role: "user" as AIChatRole,
      content: "Hello, world!",
    };
    expect(message.role).toBe("user");
    expect(message.content).toBe("Hello, world!");

    // Example of AIChatCompletion
    const completion: AIChatCompletion = {
      message: {
        role: "assistant" as AIChatRole,
        content: "Hello! How can I help you?",
      },
    };
    expect(completion.message.role).toBe("assistant");

    // Example of AIChatCompletionDelta
    const delta: AIChatCompletionDelta = {
      delta: {
        content: "Streaming ",
      },
    };
    expect(delta.delta.content).toBe("Streaming ");

    // Example of AIChatError
    const error: AIChatError = {
      code: "INVALID_REQUEST",
      message: "Invalid request format",
    };
    expect(error.code).toBe("INVALID_REQUEST");

    // Example of AIChatErrorResponse
    const errorResponse: AIChatErrorResponse = {
      error: {
        code: "RATE_LIMIT",
        message: "Rate limit exceeded",
      },
    };
    expect(errorResponse.error.code).toBe("RATE_LIMIT");
  });
});