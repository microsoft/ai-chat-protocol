# Microsoft AI Chat Protocol SDK

The Microsoft AI Chat Protocol library allows you to easily build AI Chat interfaces.

*Note: we are currently in public preview. Your feedback is greatly appreciated as we get ready to be generally available!*

With the AI Chat Protocol, you will be able to:

* Develop client-facing AI chat interfaces, components, and applications in JavaScript/TypeScript (more languages to follow!)
* Consume LLM-powered deployed inference backends and middle tiers with ease, either synchronously or by streaming
* Easily incorporate HTTP middleware for logging, authentication, and more.

The AI Chat Protocol SDK is designed to easily consume AI backends that conform to the [AI Chat Protocol API](aka.ms/chatprotocol) without any additional code. By agreeing on a standard API contract, server-side code becomes modular and the AI backend consumption process remains the same on the client-side.

## Getting Started

Our comprehensive getting started guide is coming soon! Be sure to check out the samples and API specification for more details.

* [Samples](https://github.com/microsoft/ai-chat-protocol/tree/main/samples)
* [API Specification](https://aka.ms/chatprotocol)
* [Samples on Azure](https://github.com/microsoft/ai-chat-protocol/tree/main?tab=readme-ov-file#samples-on-azure)

To take a look locally, install the library via npm:

```bash
npm install @microsoft/ai-chat-protocol
```

Create the client object:

```javascript
const client = new AIChatProtocolClient("/api/chat");
```

Stream completions to your UI:

```javascript
let sessionState = undefined;

// add any logic to handle state here
function setSessionState(value) {
    sessionState = value;
}

const message: AIChatMessage = {
    role: "user",
    content: "Hello World!",
};

const result = await client.getStreamedCompletion([message], {
    sessionState: sessionState,
});

for await (const response of result) {
    if (response.sessionState) {
        //do something with the session state returned
    }
    if (response.delta.role) {
        // do something with the information about the role
    }
    if (response.delta.content) { 
        // do something with the content of the message
    }
}
```
