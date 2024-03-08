// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  StreamableMethod,
  HttpBrowserStreamResponse,
} from "@typespec/ts-http-runtime";

export function asStream(
  method: StreamableMethod,
): Promise<HttpBrowserStreamResponse> {
  return method.asBrowserStream();
}
