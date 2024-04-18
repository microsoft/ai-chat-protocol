// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  Client,
  getClient,
  isKeyCredential,
  KeyCredential,
  RequestParameters,
  TokenCredential,
} from "@typespec/ts-http-runtime";
import { getAsyncIterable } from "./util/ndjson.js";
import { asStream } from "./util/stream.js";
import {
  AIChatClientOptions,
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatCompletionOperationOptions,
  AIChatMessage,
} from "./model/index.js";
import { toAbsoluteUrl } from "./util/url.js";

/* Replace with a version provided by the ts-http-runtime library once that is provided. */
function isTokenCredential(credential: unknown): credential is TokenCredential {
  const castCredential = credential as {
    getToken: unknown;
    signRequest: unknown;
  };
  return (
    castCredential &&
    typeof castCredential.getToken === "function" &&
    (castCredential.signRequest === undefined ||
      castCredential.getToken.length > 0)
  );
}

function isCredential(
  credential: unknown,
): credential is TokenCredential | KeyCredential {
  return isTokenCredential(credential) || isKeyCredential(credential);
}

export class AIChatProtocolClient {
  private client: Client;

  constructor(endpoint: string, options?: AIChatClientOptions);
  constructor(
    endpoint: string,
    credential: TokenCredential | KeyCredential,
    options?: AIChatClientOptions,
  );
  constructor(
    endpoint: string,
    arg1?: TokenCredential | KeyCredential | AIChatClientOptions,
    arg2?: AIChatClientOptions,
  ) {
    const absoluteEndpoint = toAbsoluteUrl(endpoint);
    if (isCredential(arg1)) {
      this.client = getClient(absoluteEndpoint, arg1, arg2);
    } else {
      this.client = getClient(absoluteEndpoint, arg1);
    }
  }

  async getCompletion(
    messages: AIChatMessage[],
    options: AIChatCompletionOperationOptions = {},
  ): Promise<AIChatCompletion> {
    const request: RequestParameters = {
      headers: {
        "Content-Type": "application/json",
      },
      body: {
        messages: messages,
        stream: false,
        context: options.context,
        sessionState: options.sessionState,
      },
    };
    const response = await this.client.path("/").post(request, options);
    if (!/2\d\d/.test(response.status)) {
      throw new Error(`Request failed with status code ${response.status}`);
    }
    return response.body as AIChatCompletion;
  }

  async getStreamedCompletion(
    messages: AIChatMessage[],
    options: AIChatCompletionOperationOptions = {},
  ): Promise<AsyncIterable<AIChatCompletionDelta>> {
    const request: RequestParameters = {
      headers: {
        "Content-Type": "application/json",
      },
      body: {
        messages: messages,
        stream: true,
        context: options.context,
        sessionState: options.sessionState,
      },
    };
    const response = await asStream(
      this.client.path("/stream").post(request, options),
    );
    if (!/2\d\d/.test(response.status)) {
      throw new Error(`Request failed with status code ${response.status}`);
    }

    return getAsyncIterable<AIChatCompletionDelta>(response.body);
  }
}
