import { http, HttpResponse } from 'msw';
import { setupWorker } from 'msw/browser';
import { expect, test } from 'vitest';

import { AIChatProtocolClient } from '../dist/browser/client.js';
import { AIChatCompletionRequest, AIChatCompletion, AIChatClientOptions } from '../dist/browser/model/index.js';

const handlers = [
    http.post<{}, AIChatCompletionRequest> ("https://my.test.com/api/chat", ({ request } ) => {
        request.json();
        const response: AIChatCompletion = { message: {
            content: "Hello, world!",
            role: 'assistant',
        } };
        return HttpResponse.json(response);
    }),
    http.post("https://my.test.com/api/chat/stream", () => {
        const stream = new ReadableStream({
            start(controller) {
                controller.enqueue(new TextEncoder().encode('{"message":"Hello, world!"}\n'));
                controller.close();
            },
        });
        return new HttpResponse(stream, {headers: {"Content-Type": "application/x-ndjson"}});
    }),

];

const server = setupWorker(...handlers);
await server.start();

test("Can send getCompletionRequest", async () => {

    const client = new AIChatProtocolClient("https://my.test.com/api/chat", {});
    const response = await client.getCompletion([
        { content: "Hello, world!", role: "user" },
    ]);
    expect(response).toEqual({ message: { content: "Hello, world!", role: "assistant" } });
});


