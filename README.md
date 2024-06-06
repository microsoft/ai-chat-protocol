# Microsoft AI Chat Protocol

[![NPM Package](https://img.shields.io/npm/v/@microsoft/ai-chat-protocol)](https://www.npmjs.com/package/@microsoft/ai-chat-protocol)
[![TypeScript Build](https://github.com/microsoft/ai-chat-protocol/actions/workflows/typescript-build.yml/badge.svg)](https://github.com/microsoft/ai-chat-protocol/actions/workflows/typescript-build.yml)

The Microsoft [AI Chat Protocol SDK](/sdk) is a library for easily building AI Chat interfaces from services that follow the [AI Chat Protocol API Specification](https://aka.ms/chatprotocol), both of which are located in this repository.

By agreeing on a standard API contract, AI backend consumption and evaluation can be performed easily and consistently across different services regardless of the models, orchestration tooling, or design patterns used.

*Note: we are currently in public preview. Your feedback is greatly appreciated as we get ready to be generally available!*

With the AI Chat Protocol, you will be able to:

* Develop AI chat interfaces, components, and applications in JavaScript/TypeScript (more languages to follow!)
* Consistently consume and evaluate AI inference backends and middle tiers with ease, either synchronously or by streaming
* Easily incorporate HTTP middleware for logging, authentication, and more.

**Please star the repo to show your support for this project!**

## Getting Started

Our comprehensive getting started guide is coming soon! Be sure to check out the samples and API specification for more details.

* [Samples](/samples)
* [API Specification](/spec)
* [Samples on Azure](#samples-on-azure)

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

## Samples on Azure

If you're curious on end-to-end samples hosted on Azure, the following samples utilize the AI Chat Protocol SDK on the frontend:

* [Serverless AI Chat with RAG using LangChain.js](https://github.com/Azure-Samples/serverless-chat-langchainjs)
* [Chat Application using Azure OpenAI (Python)](https://github.com/Azure-Samples/openai-chat-app-quickstart)
* [OpenAI Chat Application with Microsoft Entra Authentication (Python) - Local](https://github.com/Azure-Samples//openai-chat-app-entra-auth-local)
* [OpenAI Chat Application with Microsoft Entra Authentication (Python) - Builtin](https://github.com/Azure-Samples/openai-chat-app-entra-auth-builtin)

## Code of Conduct

This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the
[Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com)
with any additional questions or comments.

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](LICENSE) license.
