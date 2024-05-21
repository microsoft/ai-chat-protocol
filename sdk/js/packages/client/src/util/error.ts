// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { AIChatErrorResponse } from "../model";

export function isErrorResponse(
  response: unknown,
): response is AIChatErrorResponse {
  return (
    typeof response === "object" && response !== null && "error" in response
  );
}
