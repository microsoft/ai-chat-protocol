# List of work items

## SDK

- Add support for JSON-L streaming
- Use class names that are different than AOAI SDK
- Change name space (to Microsoft.AI.Chat instead of Microsoft.AI.ChatProtocol?)
- Do we need the Arguments.cs class?
- Expose HTTP request/response details via APIs. Should I expose HttpResponseMessage to the caller? When I updated the test app to get chatCompletion.Response.RequestMessage?.Content?.ReadAsStringAsync().Result I got an exception saying the Content was already disposed. Should I just expose response status code, response headers and raw JSON as separate properties?
- Why can't I send a full JSON string as the "context" (see commented out lines in test TestGetChatCompletion

## Unit testing

- Test parsing invalid JSON (null/missing JSON element). Validate the correct exception is thrown.

## Functional (end-to-end) testing

- Test overriding default User-Agent header
- Test with CancellationToken
- Test that on service-originated-error, an exception is thrown and the error message contains HTTP status code and message from service
- Test service-originated-error mid way during response streaming (see [here](https://github.com/Azure-Samples/ai-chat-app-protocol?tab=readme-ov-file#error-response-1))

## Sample code

- See TODOs in code

## Ref docs

- Script to convert XML to HTML using docfx

- ## NuGet package
