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
  AIChatErrorResponse,
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

function isErrorResponse(response: any): response is AIChatErrorResponse {
  return response.error !== undefined && response.error.code !== undefined;
}

function isCredential(
  credential: unknown,
): credential is TokenCredential | KeyCredential {
  return isTokenCredential(credential) || isKeyCredential(credential);
}

function isLocalhost(url: string): boolean {
  const parsed = new URL(url);
  return parsed.hostname === "localhost" || parsed.hostname === "127.0.0.1";
}

async function getStreamContent(
  stream: ReadableStream<Uint8Array>,
): Promise<any> {
  const bodyText = await new Response(stream).text();
  try {
    return JSON.parse(bodyText);
  } catch (error) {
    return bodyText;
  }
}

function handleFailedRequest(status: any, body: any): never {
  if (isErrorResponse(body)) {
    throw body.error;
  }
  throw {
    code: status,
    message: `Request failed with status code ${status}`,
  };
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
    const defaults: AIChatClientOptions = {
      allowInsecureConnection: isLocalhost(absoluteEndpoint),
    };
    if (isCredential(arg1)) {
      this.client = getClient(absoluteEndpoint, arg1, { ...defaults, ...arg2 });
    } else {
      this.client = getClient(absoluteEndpoint, { ...defaults, ...arg1 });
    }
  }

  /**
   * This method sends a request to the AIChatProtocol endpoint and returns a completion.
   * @param messages An array of AIChatMessage objects.
   * @param options An object of type AIChatCompletionOperationOptions.
   * @throws {AIChatError} Throws an AIChatError if the response status is not in the 200 range.
   * @returns A Promise that resolves to an AIChatCompletion object.
   */
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
        context: options.context,
        sessionState: options.sessionState,
      },
    };
    const response = await this.client.path("/").post(request, options);
    const { status, body } = response;
    if (!/2\d\d/.test(status)) {
      handleFailedRequest(status, body);
    }
    return body as AIChatCompletion;
  }

  /**
   * This method sends a request to the AIChatProtocol endpoint and returns a streamed completion.
   * @param messages An array of AIChatMessage objects.
   * @param options An object of type AIChatCompletionOperationOptions.
   * @throws {AIChatError} Throws an AIChatError if the response status is not in the 200 range.
   * @returns A Promise that resolves to an AsyncIterable of AIChatCompletionDelta objects.
   */
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
        context: options.context,
        sessionState: options.sessionState,
      },
    };
    const response = await asStream(
      this.client.path("/stream").post(request, options),
    );
    if (!/2\d\d/.test(response.status)) {
      const body = await getStreamContent(response.body);
      handleFailedRequest(response.status, body);
    }

    return getAsyncIterable<AIChatCompletionDelta>(response.body);
  }
}
