# Chat Protocol

Now that you are here, you can start chatting with your endpoint, just type and press the **`Send`** button or just press **`Shift+Enter`** to send your message.

## Installation

To integrate the chat protocol in your project, install the `@microsoft/ai-chat-protocol` package using npm:

```bash
npm install -s @microsoft/ai-chat-protocol
```

## Usage

### Streaming Requests

For streaming requests, use the `getStreamedCompletion` method. Here's an example:

```typescript
try {
  const result = await client.getStreamedCompletion([message], { sessionState: sessionState });

  for await (const response of result) {
    // Note: It is expected that you update your sessionState with the value you receive from your endpoint.
    // Handle your streaming responses here.
  }
} catch (e) {
  if (isChatError(e)) {
    // Handle your chat error here.
  }
}
```

### Non-Streaming Requests

For non-streaming requests, you can use the `getCompletion` method. Here's an example:

```typescript
try {
  const result = await client.getCompletion([message], { sessionState: sessionState });
  // Handle your result here.
} catch (e) {
  if (isChatError(e)) {
    // Handle your chat error here.
  }
}
```
