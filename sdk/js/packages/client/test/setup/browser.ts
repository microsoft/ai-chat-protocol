// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { setupWorker } from "msw/browser";
import { handlers } from "./handlers.js";
import { afterEach, beforeAll } from "vitest";

const server = setupWorker(...handlers);

beforeAll(async () => {
  await server.start({ quiet: true });
  return async () => server.stop();
});

afterEach(() => server.resetHandlers());
