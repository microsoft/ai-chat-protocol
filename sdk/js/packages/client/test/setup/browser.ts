// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { setupWorker } from "msw/browser";
import { handlers } from "./handlers.js";
import { afterAll, afterEach, beforeAll } from "vitest";

const server = setupWorker(...handlers);

beforeAll(async () => {
  await server.start({ quiet: true });
});

afterAll(() => server.stop());

afterEach(() => server.resetHandlers());
