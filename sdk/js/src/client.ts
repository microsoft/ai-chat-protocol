import { HttpClient } from "./http/client.js";
import { HttpMiddleware } from "./http/interfaces.js";
import {
  AIChatMessage,
  AIChatCompletion,
  AIChatCompletionDelta,
  AIChatComplationOptions,
} from "./models/index.js";
import { getAsyncIterable } from "./util/ndjson.js";

export class AIChatProtocolClient {
  private endpoint: string;
  private httpClient: HttpClient;
  constructor(endpoint: string) {
    this.endpoint = endpoint;
    this.httpClient = new HttpClient();
  }

  getCompletion(messages: AIChatMessage[]): Promise<AIChatCompletion>;
  getCompletion(
    messages: AIChatMessage[],
    middleware: HttpMiddleware,
  ): Promise<AIChatCompletion>;
  getCompletion(
    messages: AIChatMessage[],
    options: AIChatComplationOptions,
  ): Promise<AIChatCompletion>;
  getCompletion(
    messages: AIChatMessage[],
    options: AIChatComplationOptions,
    middleware?: HttpMiddleware,
  ): Promise<AIChatCompletion>;
  async getCompletion(
    messages: AIChatMessage[],
    arg1?: AIChatComplationOptions | HttpMiddleware,
    arg2?: HttpMiddleware,
  ): Promise<AIChatCompletion> {
    const options: AIChatComplationOptions = (arg1 as AIChatComplationOptions)
      ? (arg1 as AIChatComplationOptions)
      : {};
    const middleware = (arg1 as HttpMiddleware)
      ? (arg1 as HttpMiddleware)
      : (arg2 as HttpMiddleware);

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
    options: AIChatComplationOptions,
  ): Promise<AsyncIterable<AIChatCompletionDelta>>;
  getStreamedCompletion(
    messages: AIChatMessage[],
    options: AIChatComplationOptions,
    middleware?: HttpMiddleware,
  ): Promise<AsyncIterable<AIChatCompletionDelta>>;
  async getStreamedCompletion(
    messages: AIChatMessage[],
    arg1?: AIChatComplationOptions | HttpMiddleware,
    arg2?: HttpMiddleware,
  ): Promise<AsyncIterable<AIChatCompletionDelta>> {
    const options: AIChatComplationOptions = (arg1 as AIChatComplationOptions)
      ? (arg1 as AIChatComplationOptions)
      : {};
    const middleware = (arg1 as HttpMiddleware)
      ? (arg1 as HttpMiddleware)
      : (arg2 as HttpMiddleware);
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
      middleware,
    );
    if (response.status !== 200) {
      throw new Error(`Request failed with status code ${response.status}`);
    }
    return getAsyncIterable<AIChatCompletionDelta>(response.body);
  }
}
