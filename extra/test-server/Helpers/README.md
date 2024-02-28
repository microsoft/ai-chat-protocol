# Helper classes

Classes in this folder were copied from the [.NET Azure Open AI SDK](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/openai/Azure.AI.OpenAI/src/Helpers).

Only one change was done in `SseReader.cs`: The constructor takes an input argument of type `StreamReader` instead of a `Stream`. Therefore the internal member `_stream` is not needed and was removed.

