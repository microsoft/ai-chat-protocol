import { http, HttpResponse } from "msw";
import { AIChatCompletion } from "../../src/index.js";
import { AIChatCompletionDelta } from "../../dist/browser/index.js";

const handlers = [
  http.post("https://my.test.com/api/chat", async ({ request }) => {
    const payload = await request.json();
    const response: AIChatCompletion = {
      message: {
        content: "Hello, world!",
        role: "assistant",
      },
      context: {
        payload,
      },
    };
    return HttpResponse.json(response);
  }),
  http.post("https://my.test.com/api/chat/stream", async ({ request }) => {
    const payload = await request.json();
    const deltas: AIChatCompletionDelta[] = [
      {
        delta: {
          content: "Hello",
          role: "assistant",
        },
        context: {
          payload,
        },
      },
      {
        delta: {
          content: ", ",
          role: "assistant",
        },
      },
      {
        delta: {
          content: "world",
          role: "assistant",
        },
      },
      {
        delta: {
          content: "!",
          role: "assistant",
        },
      },
    ];
    const stream = new ReadableStream({
      start(controller) {
        for (const delta of deltas) {
          controller.enqueue(
            new TextEncoder().encode(JSON.stringify(delta) + "\r\n"),
          );
        }
        controller.close();
      },
    });
    return new HttpResponse(stream, {
      headers: { "Content-Type": "application/x-ndjson" },
    });
  }),
];

export { handlers };
