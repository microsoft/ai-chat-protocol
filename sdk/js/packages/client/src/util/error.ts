// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { AIChatErrorResponse } from "../model/index.js";

export function isErrorResponse(
  response: unknown,
): response is AIChatErrorResponse {
  return (
    typeof response === "object" && response !== null && "error" in response
  );
}
