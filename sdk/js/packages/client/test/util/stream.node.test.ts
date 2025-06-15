// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { describe, expect, test, vi } from "vitest";
import { asStream } from "../../src/util/stream.js";
import { Readable } from "node:stream";

// Mock node:stream module
vi.mock("node:stream", () => ({
  Readable: {
    toWeb: vi.fn((stream) => {
      // Mock converting Node stream to Web stream
      return new ReadableStream();
    }),
  },
}));

describe("asStream", () => {
  test("should convert node stream to web stream", async () => {
    const mockNodeStream = {} as Readable;
    const mockWebStream = new ReadableStream();
    
    const mockResponse = {
      status: "200",
      headers: { "content-type": "text/event-stream" },
      body: mockNodeStream,
    };

    const mockMethod = {
      asNodeStream: vi.fn().mockResolvedValue(mockResponse),
    };

    const result = await asStream(mockMethod as any);

    expect(mockMethod.asNodeStream).toHaveBeenCalled();
    expect(Readable.toWeb).toHaveBeenCalledWith(mockNodeStream);
    expect(result.status).toBe("200");
    expect(result.headers).toEqual({ "content-type": "text/event-stream" });
    expect(result.body).toBeInstanceOf(ReadableStream);
  });

  test("should preserve all response properties", async () => {
    const mockResponse = {
      status: "200",
      headers: { 
        "content-type": "application/json",
        "x-custom-header": "value"
      },
      body: {} as Readable,
      extra: "preserved",
    };

    const mockMethod = {
      asNodeStream: vi.fn().mockResolvedValue(mockResponse),
    };

    const result = await asStream(mockMethod as any);

    expect(result.status).toBe("200");
    expect(result.headers).toEqual(mockResponse.headers);
    expect(result.extra).toBe("preserved");
  });

  test("should handle errors from asNodeStream", async () => {
    const mockError = new Error("Stream failed");
    const mockMethod = {
      asNodeStream: vi.fn().mockRejectedValue(mockError),
    };

    await expect(asStream(mockMethod as any)).rejects.toThrow("Stream failed");
  });
});