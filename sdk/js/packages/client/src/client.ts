// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  Client,
  createFile,
  getClient,
  isKeyCredential,
  KeyCredential,
  operationOptionsToRequestParameters,
  randomUUID,
  RequestParameters,
  TokenCredential,
} from "@typespec/ts-http-runtime";
import { getAsyncIterable } from "./util/jsonl.js";
import { asStream } from "./util/stream.js";
import {
  AIChatClientOptions,
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatCompletionOperationOptions,
  AIChatMessage,
} from "./model/index.js";
import { toAbsoluteUrl } from "./util/url.js";
import { isErrorResponse } from "./util/error.js";

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

function isLocalhost(url: string): boolean {
  const parsed = new URL(url);
  return parsed.hostname === "localhost" || parsed.hostname === "127.0.0.1";
}

async function getStreamContent(
  stream: ReadableStream<Uint8Array>,
): Promise<unknown> {
  const bodyText = await new Response(stream).text();
  try {
    return JSON.parse(bodyText);
  } catch (error) {
    return bodyText;
  }
}

function handleFailedRequest(status: string, body: unknown): never {
  if (isErrorResponse(body)) {
    throw body.error;
  }
  throw {
    code: status,
    message: `Request failed with status code ${status}`,
  };
}

function splitURL(url: string): [string, string] {
  const parsed = new URL(url);
  return [parsed.origin, parsed.pathname];
}

function isUint8Array(value: unknown): value is Uint8Array {
  return value instanceof Uint8Array;
}

export class AIChatProtocolClient {
  private client: Client;

  private basePath: string;

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
    const [origin, basePath] = splitURL(absoluteEndpoint);
    this.basePath = basePath;
    const defaults: AIChatClientOptions = {
      allowInsecureConnection: isLocalhost(absoluteEndpoint),
    };
    if (isCredential(arg1)) {
      this.client = getClient(origin, arg1, { ...defaults, ...arg2 });
    } else {
      this.client = getClient(origin, { ...defaults, ...arg1 });
    }
  }

  private prepareRequest(
    messages: AIChatMessage[],
    options: AIChatCompletionOperationOptions,
  ): RequestParameters {
    const request = operationOptionsToRequestParameters(options);
    if (messages.some((message) => message.files && message.files.length > 0)) {
      const binaryParts = messages
        .map((message, index) => {
          if (message.files) {
            return message.files.map((file, fileIndex) => ({
              dispositionType: `form-data; name="messages[${index}].files[${fileIndex}]"`,
              contentType: file.contentType,
              body: isUint8Array(file.data)
                ? createFile(
                    file.data,
                    `messages[${index}].files[${fileIndex}]`,
                  )
                : file.data,
            }));
          }
          return undefined;
        })
        .flat();
      const boundary = `---Part-${randomUUID()}`;
      request.contentType = `multipart/form-data; boundary=${boundary}`;
      request.body = [
        {
          dispositionType: "form-data; name=json",
          contentType: "application/json",
          body: {
            messages: messages.map((message) => ({
              ...message,
              files: undefined,
            })),
            context: options.context,
            sessionState: options.sessionState,
          },
        },
        ...binaryParts.filter((part) => part !== undefined),
      ];
    } else {
      request.contentType = "application/json";
      request.body = {
        messages: messages,
        context: options.context,
        sessionState: options.sessionState,
      };
    }
    return request;
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
    const request = this.prepareRequest(messages, options);
    const response = await this.client.path(this.basePath).post(request);
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
    const request = this.prepareRequest(messages, options);
    const response = await asStream(
      this.client.path(`${this.basePath}/stream`).post(request),
    );
    if (!/2\d\d/.test(response.status)) {
      const body = await getStreamContent(response.body);
      handleFailedRequest(response.status, body);
    }

    return getAsyncIterable<AIChatCompletionDelta>(response.body);
  }
}
