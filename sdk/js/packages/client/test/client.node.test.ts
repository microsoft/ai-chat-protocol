// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { describe, expect, test, vi, beforeEach, afterEach } from "vitest";
import { AIChatProtocolClient } from "../src/client.js";
import {
  AIChatMessage,
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatError,
} from "../src/model/index.js";
import { Client } from "@typespec/ts-http-runtime";

// Mock the ts-http-runtime
vi.mock("@typespec/ts-http-runtime", () => ({
  getClient: vi.fn(),
  createFile: vi.fn((data) => data),
  operationOptionsToRequestParameters: vi.fn((options) => {
    // Extract only the request parameters, not the options themselves
    const params: any = {};
    if (options?.requestOptions) {
      Object.assign(params, options.requestOptions);
    }
    return params;
  }),
  randomUUID: vi.fn(() => "test-uuid"),
  isKeyCredential: vi.fn((cred) => cred && typeof cred.key === "string"),
}));

// Mock the utility functions
vi.mock("../src/util/jsonl.js", () => ({
  getAsyncIterable: vi.fn(),
}));

vi.mock("../src/util/stream.js", () => ({
  asStream: vi.fn(),
}));

vi.mock("../src/util/url.js", () => ({
  toAbsoluteUrl: vi.fn((url) => {
    if (url.startsWith("http")) return url;
    return `http://localhost${url}`;
  }),
}));

vi.mock("../src/util/error.js", () => ({
  isErrorResponse: vi.fn((body) => body && typeof body === "object" && "error" in body),
}));

describe("AIChatProtocolClient", () => {
  let mockClient: any;
  let mockPath: any;
  let mockPost: any;

  beforeEach(async () => {
    // Reset all mocks
    vi.clearAllMocks();

    // Setup mock client chain
    mockPost = vi.fn();
    mockPath = vi.fn(() => ({ post: mockPost }));
    mockClient = { path: mockPath };

    // Mock getClient to return our mock client
    const { getClient } = vi.mocked(await import("@typespec/ts-http-runtime"));
    getClient.mockReturnValue(mockClient as Client);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe("constructor", () => {
    test("should create client with endpoint only", () => {
      const client = new AIChatProtocolClient("/api/chat");
      expect(client).toBeInstanceOf(AIChatProtocolClient);
    });

    test("should create client with absolute URL", () => {
      const client = new AIChatProtocolClient("https://api.example.com/chat");
      expect(client).toBeInstanceOf(AIChatProtocolClient);
    });

    test("should create client with credential", async () => {
      const { getClient } = vi.mocked(await import("@typespec/ts-http-runtime"));
      const credential = { key: "test-key" };
      const client = new AIChatProtocolClient("/api/chat", credential);
      
      expect(getClient).toHaveBeenCalledWith(
        "http://localhost",
        credential,
        expect.objectContaining({ allowInsecureConnection: true })
      );
      expect(client).toBeInstanceOf(AIChatProtocolClient);
    });

    test("should create client with token credential", async () => {
      const { getClient } = vi.mocked(await import("@typespec/ts-http-runtime"));
      const tokenCredential = {
        getToken: vi.fn().mockResolvedValue({ token: "test-token", expiresOnTimestamp: 0 }),
      };
      const client = new AIChatProtocolClient("https://api.example.com/chat", tokenCredential);
      
      expect(getClient).toHaveBeenCalledWith(
        "https://api.example.com",
        tokenCredential,
        expect.objectContaining({ allowInsecureConnection: false })
      );
      expect(client).toBeInstanceOf(AIChatProtocolClient);
    });

    test("should allow insecure connection for localhost", async () => {
      const { getClient } = vi.mocked(await import("@typespec/ts-http-runtime"));
      new AIChatProtocolClient("http://localhost:3000/chat");
      
      expect(getClient).toHaveBeenCalledWith(
        "http://localhost:3000",
        expect.objectContaining({ allowInsecureConnection: true })
      );
    });

    test("should not allow insecure connection for non-localhost", async () => {
      const { getClient } = vi.mocked(await import("@typespec/ts-http-runtime"));
      new AIChatProtocolClient("https://api.example.com/chat");
      
      expect(getClient).toHaveBeenCalledWith(
        "https://api.example.com",
        expect.objectContaining({ allowInsecureConnection: false })
      );
    });
  });

  describe("getCompletion", () => {
    test("should successfully get completion", async () => {
      const messages: AIChatMessage[] = [
        { role: "user", content: "Hello, world!" },
      ];
      
      const expectedCompletion: AIChatCompletion = {
        message: { role: "assistant", content: "Hello! How can I help you?" },
        sessionState: { conversationId: "123" },
        context: { model: "gpt-4" },
      };

      mockPost.mockResolvedValue({
        status: "200",
        body: expectedCompletion,
      });

      const client = new AIChatProtocolClient("/api/chat");
      const result = await client.getCompletion(messages);

      expect(mockPath).toHaveBeenCalledWith("/api/chat");
      expect(mockPost).toHaveBeenCalledWith({
        contentType: "application/json",
        body: {
          messages,
          context: undefined,
          sessionState: undefined,
        },
      });
      expect(result).toEqual(expectedCompletion);
    });

    test("should include options in request", async () => {
      const messages: AIChatMessage[] = [
        { role: "user", content: "Hello" },
      ];
      
      const options = {
        context: { temperature: 0.7 },
        sessionState: { userId: "user123" },
      };

      mockPost.mockResolvedValue({
        status: "200",
        body: { message: { role: "assistant", content: "Hi" } },
      });

      const client = new AIChatProtocolClient("/api/chat");
      await client.getCompletion(messages, options);

      expect(mockPost).toHaveBeenCalledWith({
        contentType: "application/json",
        body: {
          messages,
          context: options.context,
          sessionState: options.sessionState,
        },
      });
    });

    test("should handle multipart request with files", async () => {
      const messages: AIChatMessage[] = [
        {
          role: "user",
          content: "Analyze this image",
          files: [
            {
              data: new Uint8Array([1, 2, 3]),
              contentType: "image/png",
            },
          ],
        },
      ];

      mockPost.mockResolvedValue({
        status: "200",
        body: { message: { role: "assistant", content: "I see an image" } },
      });

      const client = new AIChatProtocolClient("/api/chat");
      await client.getCompletion(messages);

      expect(mockPost).toHaveBeenCalledWith(
        expect.objectContaining({
          contentType: "multipart/form-data; boundary=---Part-test-uuid",
          body: expect.arrayContaining([
            expect.objectContaining({
              dispositionType: "form-data; name=json",
              contentType: "application/json",
            }),
            expect.objectContaining({
              dispositionType: 'form-data; name="messages[0].files[0]"',
              contentType: "image/png",
            }),
          ]),
        })
      );
    });

    test("should throw AIChatError on non-2xx response", async () => {
      const errorResponse = {
        error: {
          code: "INVALID_REQUEST",
          message: "Invalid request format",
        },
      };

      mockPost.mockResolvedValue({
        status: "400",
        body: errorResponse,
      });

      const client = new AIChatProtocolClient("/api/chat");
      const messages: AIChatMessage[] = [{ role: "user", content: "Hello" }];

      await expect(client.getCompletion(messages)).rejects.toEqual({
        code: "INVALID_REQUEST",
        message: "Invalid request format",
      });
    });

    test("should throw generic error on non-2xx response without error body", async () => {
      mockPost.mockResolvedValue({
        status: "500",
        body: "Internal Server Error",
      });

      const client = new AIChatProtocolClient("/api/chat");
      const messages: AIChatMessage[] = [{ role: "user", content: "Hello" }];

      await expect(client.getCompletion(messages)).rejects.toEqual({
        code: "500",
        message: "Request failed with status code 500",
      });
    });
  });

  describe("getStreamedCompletion", () => {
    test("should successfully stream completion deltas", async () => {
      const messages: AIChatMessage[] = [
        { role: "user", content: "Tell me a story" },
      ];

      const deltas: AIChatCompletionDelta[] = [
        { delta: { content: "Once " } },
        { delta: { content: "upon " } },
        { delta: { content: "a time..." } },
      ];

      // Mock the stream response
      const { asStream } = vi.mocked(await import("../src/util/stream.js"));
      const { getAsyncIterable } = vi.mocked(await import("../src/util/jsonl.js"));

      const mockStream = new ReadableStream();
      asStream.mockResolvedValue({
        status: "200",
        body: mockStream,
      });

      // Create async iterable from deltas
      async function* deltaGenerator() {
        for (const delta of deltas) {
          yield delta;
        }
      }
      getAsyncIterable.mockReturnValue(deltaGenerator());

      const client = new AIChatProtocolClient("/api/chat");
      const result = await client.getStreamedCompletion(messages);

      expect(mockPath).toHaveBeenCalledWith("/api/chat/stream");
      expect(asStream).toHaveBeenCalled();

      // Collect streamed deltas
      const collectedDeltas: AIChatCompletionDelta[] = [];
      for await (const delta of result) {
        collectedDeltas.push(delta);
      }

      expect(collectedDeltas).toEqual(deltas);
    });

    test("should handle streaming error response", async () => {
      const messages: AIChatMessage[] = [
        { role: "user", content: "Hello" },
      ];

      const { asStream } = vi.mocked(await import("../src/util/stream.js"));
      
      // Create a mock stream with error data
      const errorData = JSON.stringify({
        error: { code: "STREAM_ERROR", message: "Streaming failed" },
      });
      const mockStream = new ReadableStream({
        start(controller) {
          controller.enqueue(new TextEncoder().encode(errorData));
          controller.close();
        },
      });

      asStream.mockResolvedValue({
        status: "500",
        body: mockStream,
      });

      const client = new AIChatProtocolClient("/api/chat");

      await expect(client.getStreamedCompletion(messages)).rejects.toEqual({
        code: "STREAM_ERROR",
        message: "Streaming failed",
      });
    });

    test("should include options in streamed request", async () => {
      const messages: AIChatMessage[] = [
        { role: "user", content: "Hello" },
      ];
      
      const options = {
        context: { stream: true },
        sessionState: { conversationId: "123" },
      };

      const { asStream } = vi.mocked(await import("../src/util/stream.js"));
      const { getAsyncIterable } = vi.mocked(await import("../src/util/jsonl.js"));

      asStream.mockResolvedValue({
        status: "200",
        body: new ReadableStream(),
      });

      async function* emptyGenerator() {
        // Empty generator
      }
      getAsyncIterable.mockReturnValue(emptyGenerator());

      const client = new AIChatProtocolClient("/api/chat");
      await client.getStreamedCompletion(messages, options);

      // Check that asStream was called with the correct post call
      const postCall = asStream.mock.calls[0][0];
      expect(postCall).toBe(mockPost.mock.results[0].value);
      
      expect(mockPost).toHaveBeenCalledWith({
        contentType: "application/json",
        body: {
          messages,
          context: options.context,
          sessionState: options.sessionState,
        },
      });
    });

    test("should handle multipart streaming request with files", async () => {
      const messages: AIChatMessage[] = [
        {
          role: "user",
          content: "Stream analysis of this",
          files: [
            {
              data: new Uint8Array([4, 5, 6]),
              contentType: "application/pdf",
            },
          ],
        },
      ];

      const { asStream } = vi.mocked(await import("../src/util/stream.js"));
      const { getAsyncIterable } = vi.mocked(await import("../src/util/jsonl.js"));

      asStream.mockResolvedValue({
        status: "200",
        body: new ReadableStream(),
      });

      async function* emptyGenerator() {
        // Empty generator
      }
      getAsyncIterable.mockReturnValue(emptyGenerator());

      const client = new AIChatProtocolClient("/api/chat");
      await client.getStreamedCompletion(messages);

      expect(mockPost).toHaveBeenCalledWith(
        expect.objectContaining({
          contentType: "multipart/form-data; boundary=---Part-test-uuid",
          body: expect.arrayContaining([
            expect.objectContaining({
              dispositionType: "form-data; name=json",
            }),
            expect.objectContaining({
              dispositionType: 'form-data; name="messages[0].files[0]"',
              contentType: "application/pdf",
            }),
          ]),
        })
      );
    });
  });

  describe("edge cases", () => {
    test("should handle empty messages array", async () => {
      const messages: AIChatMessage[] = [];

      mockPost.mockResolvedValue({
        status: "200",
        body: { message: { role: "assistant", content: "No input provided" } },
      });

      const client = new AIChatProtocolClient("/api/chat");
      const result = await client.getCompletion(messages);

      expect(result).toBeDefined();
      expect(mockPost).toHaveBeenCalledWith({
        contentType: "application/json",
        body: {
          messages: [],
          context: undefined,
          sessionState: undefined,
        },
      });
    });

    test("should handle messages with multiple files", async () => {
      const messages: AIChatMessage[] = [
        {
          role: "user",
          content: "First message with file",
          files: [
            { data: new Uint8Array([1]), contentType: "image/png" },
            { data: new Uint8Array([2]), contentType: "image/jpeg" },
          ],
        },
        {
          role: "assistant",
          content: "Response without file",
        },
        {
          role: "user",
          content: "Second message with file",
          files: [
            { data: new Uint8Array([3]), contentType: "application/pdf" },
          ],
        },
      ];

      mockPost.mockResolvedValue({
        status: "200",
        body: { message: { role: "assistant", content: "Processed all files" } },
      });

      const client = new AIChatProtocolClient("/api/chat");
      await client.getCompletion(messages);

      expect(mockPost).toHaveBeenCalledWith(
        expect.objectContaining({
          contentType: "multipart/form-data; boundary=---Part-test-uuid",
          body: expect.arrayContaining([
            expect.objectContaining({
              dispositionType: "form-data; name=json",
            }),
            expect.objectContaining({
              dispositionType: 'form-data; name="messages[0].files[0]"',
            }),
            expect.objectContaining({
              dispositionType: 'form-data; name="messages[0].files[1]"',
            }),
            expect.objectContaining({
              dispositionType: 'form-data; name="messages[2].files[0]"',
            }),
          ]),
        })
      );
    });

    test("should handle very long content", async () => {
      const longContent = "x".repeat(10000);
      const messages: AIChatMessage[] = [
        { role: "user", content: longContent },
      ];

      mockPost.mockResolvedValue({
        status: "200",
        body: { message: { role: "assistant", content: "Processed long content" } },
      });

      const client = new AIChatProtocolClient("/api/chat");
      const result = await client.getCompletion(messages);

      expect(result).toBeDefined();
      expect(mockPost).toHaveBeenCalledWith(
        expect.objectContaining({
          body: expect.objectContaining({
            messages: [{ role: "user", content: longContent }],
          }),
        })
      );
    });

    test("should handle special characters in content", async () => {
      const specialContent = 'Hello "world" with \'quotes\' and \n newlines \t tabs';
      const messages: AIChatMessage[] = [
        { role: "user", content: specialContent },
      ];

      mockPost.mockResolvedValue({
        status: "200",
        body: { message: { role: "assistant", content: "Handled special chars" } },
      });

      const client = new AIChatProtocolClient("/api/chat");
      await client.getCompletion(messages);

      expect(mockPost).toHaveBeenCalledWith(
        expect.objectContaining({
          body: expect.objectContaining({
            messages: [{ role: "user", content: specialContent }],
          }),
        })
      );
    });

    test("should handle network timeout gracefully", async () => {
      mockPost.mockRejectedValue(new Error("Network timeout"));

      const client = new AIChatProtocolClient("/api/chat");
      const messages: AIChatMessage[] = [{ role: "user", content: "Hello" }];

      await expect(client.getCompletion(messages)).rejects.toThrow("Network timeout");
    });

    test("should handle malformed response gracefully", async () => {
      mockPost.mockResolvedValue({
        status: "200",
        body: null,
      });

      const client = new AIChatProtocolClient("/api/chat");
      const messages: AIChatMessage[] = [{ role: "user", content: "Hello" }];

      const result = await client.getCompletion(messages);
      expect(result).toBeNull();
    });
  });
});