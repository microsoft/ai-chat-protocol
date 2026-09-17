import { AIChatProtocolClient } from "../client.js";
import {
  AIChatMessage,
  AIChatCompletionDelta,
  AIChatCompletion,
} from "../model/index.js";

export type ChatRequestOptions = {
  messages: AIChatMessage[];
  chunkIntervalMs: number;
  apiUrl: string;
  stream: boolean;
};

export async function getCompletion(
  options: ChatRequestOptions,
): Promise<AIChatCompletion | AsyncGenerator<AIChatCompletionDelta>> {
  const apiUrl = options.apiUrl || "";
  const client = new AIChatProtocolClient(`${apiUrl}/api/chat`);

  if (options.stream) {
    const response = await client.getStreamedCompletion(options.messages);
    return getChunksFromResponse(response, options.chunkIntervalMs);
  } else {
    return await client.getCompletion(options.messages);
  }
}

export async function* getChunksFromResponse(
  response: AsyncIterable<AIChatCompletionDelta>,
  intervalMs: number,
): AsyncGenerator<AIChatCompletionDelta> {
  for await (const chunk of response) {
    if (!chunk.delta) {
      continue;
    }

    yield new Promise<AIChatCompletionDelta>((resolve) => {
      setTimeout(() => {
        resolve(chunk);
      }, intervalMs);
    });
  }
}
