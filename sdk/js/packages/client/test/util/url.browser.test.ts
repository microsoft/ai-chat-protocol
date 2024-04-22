// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { expect, test } from "vitest";
import { toAbsoluteUrl } from "../../src/util/url-browser.mjs";

test("An URL that is already absolute should remain the same", () => {
  const url = "https://example.com";
  expect(toAbsoluteUrl(url)).toBe(url);
});

test("An URL that is relative to the current location should be resolved", () => {
  const url = "/foo/bar";
  expect(toAbsoluteUrl(url)).toBe(`${window.location.origin}${url}`);
});
