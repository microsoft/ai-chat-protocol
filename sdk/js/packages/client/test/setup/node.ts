// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { setupServer } from "msw/node";
import { handlers } from "./handlers.js";
import { afterAll, afterEach, beforeAll } from "vitest";

const server = setupServer(...handlers);

beforeAll(() => server.listen());

afterAll(() => server.close());

afterEach(() => server.resetHandlers());
