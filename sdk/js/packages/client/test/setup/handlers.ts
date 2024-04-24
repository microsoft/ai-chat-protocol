import { http, HttpResponse } from "msw";
import { AIChatCompletion, AIChatCompletionRequest } from "../../src/index.js";

const handlers = [
  http.post<{}, AIChatCompletionRequest>(
    "https://my.test.com/api/chat",
    ({ request }) => {
      request.json();
      const response: AIChatCompletion = {
        message: {
          content: "Hello, world!",
          role: "assistant",
        },
      };
      return HttpResponse.json(response);
    },
  ),
  http.post("https://my.test.com/api/chat/stream", () => {
    const stream = new ReadableStream({
      start(controller) {
        controller.enqueue(
          new TextEncoder().encode('{"message":"Hello, world!"}\n'),
        );
        controller.close();
      },
    });
    return new HttpResponse(stream, {
      headers: { "Content-Type": "application/x-ndjson" },
    });
  }),
];

export { handlers };
