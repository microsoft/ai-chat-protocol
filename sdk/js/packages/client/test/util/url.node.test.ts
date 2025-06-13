// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { describe, expect, test } from "vitest";
import { toAbsoluteUrl } from "../../src/util/url.js";

describe("toAbsoluteUrl", () => {
  test("should return all URLs unchanged (current implementation)", () => {
    // The current implementation just returns the URL as-is
    expect(toAbsoluteUrl("https://example.com/api")).toBe("https://example.com/api");
    expect(toAbsoluteUrl("http://localhost:3000/chat")).toBe("http://localhost:3000/chat");
    expect(toAbsoluteUrl("/api/chat")).toBe("/api/chat");
    expect(toAbsoluteUrl("")).toBe("");
    expect(toAbsoluteUrl("/")).toBe("/");
    expect(toAbsoluteUrl("//example.com/api")).toBe("//example.com/api");
  });
});