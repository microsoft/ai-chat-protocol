# List of work items

## Contacts

- For anything related to .NET System.ClientModel (SCM) contact Anne Loomis Thompson.

## SDK

- Should I use the serialization/deserialization provided with System.ClientModel?
  (see https://github.com/Azure/azure-sdk-for-net/blob/863cec65455e1f54fa9d3131c779d1f1c6072174/sdk/core/System.ClientModel/README.md).
- Support `session_state` and `context` fields - Desice what object type they are (at the moment they are strings). Add tests.
- Use class names that are different than AOAI SDK and the new unbranded OAI SDK
- Decide on name space (use Microsoft.AI.Chat instead of Microsoft.AI.ChatProtocl?)
- Should there be an easy way to get read-only HTTP request details using public APIs, like we have in Java?. The alternative is to use policy. Should I expose HttpResponseMessage to the caller? I had a conversation with Anne on this. I need this to validate test results and I think developers will also want read-access to request details. When I updated the test app to get chatCompletion.Response.RequestMessage?.Content?.ReadAsStringAsync().Result I got an exception saying the Content was already disposed. Should I just expose response status code, response headers and raw JSON as separate properties?

## Unit testing

### Error tests

- Test parsing invalid JSON (null/missing JSON element). Validate the correct exception is thrown.
- Test parsing empty JSON lines in the response (we should silently skip them)

## Functional (end-to-end) testing

- Test overriding default User-Agent header
- Test that on service-originated-error, an exception is thrown and the error message contains HTTP status code and message from service
- Test service-originated-error mid way during response streaming (see [here](https://github.com/Azure-Samples/ai-chat-app-protocol?tab=readme-ov-file#error-response-1))

## Sample code


## Ref docs

- Script to convert XML to HTML using docfx

## NuGet package
