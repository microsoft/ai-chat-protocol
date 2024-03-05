import { HttpClient } from "./http/client.js";
import { HttpMiddleware } from "./http/interfaces.js";
import {
  AIChatMessage,
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatCompletionOptions,
} from "./models/index.js";
import { getAsyncIterable } from "./util/ndjson.js";

export class AIChatProtocolClient {
  private endpoint: string;
  private httpClient: HttpClient;
  private middleware?: HttpMiddleware;

  constructor(endpoint: string);
  constructor(endpoint: string, middleware: HttpMiddleware);
  constructor(endpoint: string, middleware?: HttpMiddleware) {
    this.endpoint = endpoint;
    this.middleware = middleware;
    this.httpClient = new HttpClient();
  }

  private composeMiddleware(
    middleware?: HttpMiddleware,
  ): HttpMiddleware | undefined {
    if (this.middleware && middleware) {
      return async (request) => {
        const first = await this.middleware(request);
        return middleware(first);
      };
    } else if (this.middleware) {
      return this.middleware;
    } else if (middleware) {
      return middleware;
    } else {
      return undefined;
    }
  }

  getCompletion(messages: AIChatMessage[]): Promise<AIChatCompletion>;
  getCompletion(
    messages: AIChatMessage[],
    middleware: HttpMiddleware,
  ): Promise<AIChatCompletion>;
  getCompletion(
    messages: AIChatMessage[],
    options: AIChatCompletionOptions,
  ): Promise<AIChatCompletion>;
  getCompletion(
    messages: AIChatMessage[],
    options: AIChatCompletionOptions,
    middleware?: HttpMiddleware,
  ): Promise<AIChatCompletion>;
  async getCompletion(
    messages: AIChatMessage[],
    arg1?: AIChatCompletionOptions | HttpMiddleware,
    arg2?: HttpMiddleware,
  ): Promise<AIChatCompletion> {
    const options: AIChatCompletionOptions = (arg1 as AIChatCompletionOptions)
      ? (arg1 as AIChatCompletionOptions)
      : {};
    const middleware = this.composeMiddleware(
      (arg1 as HttpMiddleware)
        ? (arg1 as HttpMiddleware)
        : (arg2 as HttpMiddleware),
    );

    const response = await this.httpClient.send(
      {
        method: "POST",
        url: new URL(this.endpoint),
        headers: {
          "Content-Type": "application/json",
        },
        body: {
          type: "object",
          body: {
            messages: messages,
            stream: false,
            ...options,
          },
        },
      },
      options.signal,
      middleware,
    );
    if (response.status !== 200) {
      throw new Error(`Request failed with status code ${response.status}`);
    }
    const reader = response.body.getReader();
    const payload = await reader.read();
    const serialized = new TextDecoder().decode(payload.value);
    return JSON.parse(serialized) as AIChatCompletion;
  }

  getStreamedCompletion(
    messages: AIChatMessage[],
  ): Promise<AsyncIterable<AIChatCompletionDelta>>;
  getStreamedCompletion(
    messages: AIChatMessage[],
    middleware?: HttpMiddleware,
  ): Promise<AsyncIterable<AIChatCompletionDelta>>;
  getStreamedCompletion(
    messages: AIChatMessage[],
    options: AIChatCompletionOptions,
  ): Promise<AsyncIterable<AIChatCompletionDelta>>;
  getStreamedCompletion(
    messages: AIChatMessage[],
    options: AIChatCompletionOptions,
    middleware?: HttpMiddleware,
  ): Promise<AsyncIterable<AIChatCompletionDelta>>;
  async getStreamedCompletion(
    messages: AIChatMessage[],
    arg1?: AIChatCompletionOptions | HttpMiddleware,
    arg2?: HttpMiddleware,
  ): Promise<AsyncIterable<AIChatCompletionDelta>> {
    const options: AIChatCompletionOptions = (arg1 as AIChatCompletionOptions)
      ? (arg1 as AIChatCompletionOptions)
      : {};
    const middleware = this.composeMiddleware(
      (arg1 as HttpMiddleware)
        ? (arg1 as HttpMiddleware)
        : (arg2 as HttpMiddleware),
    );
    const response = await this.httpClient.send(
      {
        method: "POST",
        url: new URL(this.endpoint),
        headers: {
          "Content-Type": "application/json",
        },
        body: {
          type: "object",
          body: {
            messages: messages,
            stream: true,
            ...options,
          },
        },
      },
      options.signal,
      middleware,
    );
    if (response.status !== 200) {
      throw new Error(`Request failed with status code ${response.status}`);
    }
    return getAsyncIterable<AIChatCompletionDelta>(response.body);
  }
}
