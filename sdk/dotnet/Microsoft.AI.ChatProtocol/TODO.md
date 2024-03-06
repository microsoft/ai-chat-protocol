# List of work items

## SDK

- Add support for JSON-L streaming
- Use class names that are different than AOAI SDK
- Change name space (to Microsoft.AI.Chat instead of Microsoft.AI.ChatProtocol?)
- Do we need the Arguments.cs class?
- Expose HTTP request/response details via APIs. Should I expose HttpResponseMessage to the caller? When I updated the test app to get chatCompletion.Response.RequestMessage?.Content?.ReadAsStringAsync().Result I got an exception saying the Content was already disposed. Should I just expose response status code, response headers and raw JSON as separate properties?

## Unit testing

- Test parsing invalid JSON (null/missing JSON element). Validate the correct exception is thrown.

## Functional (end-to-end) testing

- Test with CancellationToken

## Sample code

- See TODOs in code

## Ref docs

- Script to convert XML to HTML using docfx

- ## NuGet package
