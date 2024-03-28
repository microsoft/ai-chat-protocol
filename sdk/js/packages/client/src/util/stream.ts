// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  HttpBrowserStreamResponse,
  HttpNodeStreamResponse,
  StreamableMethod,
} from "@typespec/ts-http-runtime";

import { Readable } from "node:stream";

export function asStream(
  method: StreamableMethod,
): Promise<HttpBrowserStreamResponse> {
  return method.asNodeStream().then((response: HttpNodeStreamResponse) => {
    return {
      ...response,
      body: Readable.toWeb(response.body as Readable),
    };
  });
}
