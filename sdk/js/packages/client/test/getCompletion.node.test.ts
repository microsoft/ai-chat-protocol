import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { expect, test } from 'vitest';

import { AIChatProtocolClient } from '../dist/commonjs/client.js';
import { AIChatCompletion } from '../src/index.js';

const handlers = [
    http.post("https://my.test.com/api/chat", () => {
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

const server = setupServer(...handlers);
server.listen();

test("Can send getCompletionRequest", async () => {

    const client = new AIChatProtocolClient("https://my.test.com/api/chat");
    const response = await client.getCompletion([
        { content: "Hello, world!", role: "user" },
    ]);
    expect(response).toEqual({ message: { content: "Hello, world!", role: "assistant" } });
});


