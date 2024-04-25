// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { setupServer } from "msw/node";
import { handlers } from "./handlers.js";
import { afterEach, beforeAll } from "vitest";

const server = setupServer(...handlers);

beforeAll(async () => {
  server.listen();
  return async () => server.close();
});

afterEach(() => server.resetHandlers());
