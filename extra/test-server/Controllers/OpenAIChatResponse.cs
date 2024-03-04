// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.AI.OpenAI;
using Azure.Core.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Tracing;
using System.Net;
using System.Text.Json;
using System.Text;

namespace Azure.AI.Chat.SampleService;

internal class OpenAIChatResponseBaseClass
{
    protected readonly OpenAIClient _client;
    private readonly string _deployment;
    private readonly ChatProtocolCompletionOptions _options;
    private readonly AzureEventSourceListener _listener;

    internal OpenAIChatResponseBaseClass(OpenAIClient client, string deployment, ChatProtocolCompletionOptions options)
    {
        _client = client;
        _deployment = deployment;
        _options = options;
        _listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);
    }

    internal ChatCompletionsOptions GetChatCompletionsOptions()
    {
        var messages = new List<ChatRequestMessage>();

        foreach (ChatProtocolMessage chatMessage in _options.Messages)
        {
            switch (chatMessage.Role)
            {
                // See https://learn.microsoft.com/dotnet/api/azure.ai.openai.chatrequestmessage?view=azure-dotnet-preview
                case "system":
                    messages.Add(new ChatRequestSystemMessage(chatMessage.Content));
                    break;
                case "user":
                    messages.Add(new ChatRequestUserMessage(chatMessage.Content));
                    break;
                case "assistant":
                    messages.Add(new ChatRequestAssistantMessage(chatMessage.Content));
                    break;
                case "tool": // TODO
                case "function": // TODO (but deprecated)
                default:
                    // This will result in "HTTP/1.1 500 Internal Server Error" response to the client
                    throw new Exception("Invalid role value");
            }
        }

        return new ChatCompletionsOptions(_deployment, messages);
    }
}

internal class OpenAIChatResponse: OpenAIChatResponseBaseClass, IActionResult
{
    internal OpenAIChatResponse(OpenAIClient client, string deployment, ChatProtocolCompletionOptions options)
        : base(client, deployment, options)
    {
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        // Throws Azure.RequestFailedException if the response is not successful (HTTP status code is not in the 200s)
        Response<ChatCompletions> chatCompletionsResponse =
            await _client.GetChatCompletionsAsync(GetChatCompletionsOptions());

        ChatProtocolCompletion completion = new()
        {
            Choices = chatCompletionsResponse.Value.Choices.Select(choice => new ChatProtocolChoice
            {
                Index = choice.Index,
                Message = new ChatProtocolMessage
                {
                    Content = choice.Message.Content,
                    Role = choice.Message.Role.ToString()
                },
                FinishReason = choice.FinishReason?.ToString() ?? ""
            }).ToList()
        };

        HttpResponse httpResponse = context.HttpContext.Response;
        httpResponse.StatusCode = (int)HttpStatusCode.OK;
        httpResponse.ContentType = "application/json";
        await httpResponse.WriteAsync(JsonSerializer.Serialize(completion), Encoding.UTF8);
    }
}

internal class OpenAIStreamingChatResponse : OpenAIChatResponseBaseClass, IActionResult
{
    internal OpenAIStreamingChatResponse(OpenAIClient client, string deployment, ChatProtocolCompletionOptions options)
        : base(client, deployment, options)
    {
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        // Throws Azure.RequestFailedException if the response is not successful (HTTP status code is not in the 200s)
        StreamingResponse<StreamingChatCompletionsUpdate> streamingChatCompletionsUpdateResponse =
            await _client.GetChatCompletionsStreamingAsync(GetChatCompletionsOptions());

        HttpResponse httpResponse = context.HttpContext.Response;
        httpResponse.StatusCode = (int)HttpStatusCode.OK;
        httpResponse.ContentType = "text/event-stream";

        await foreach (StreamingChatCompletionsUpdate chatUpdate in streamingChatCompletionsUpdateResponse)
        {
            if (!chatUpdate.ChoiceIndex.HasValue)
            {
                continue;
            }

            int choiceIndex = chatUpdate.ChoiceIndex.Value;

            ChoiceProtocolChoiceDelta delta = new()
            {
                Index = choiceIndex,
                Delta = new ChatProtocolMessageDelta
                {
                    Content = chatUpdate.ContentUpdate,
                    Role = chatUpdate.Role?.ToString()
                },
                FinishReason = chatUpdate.FinishReason?.ToString()
            };

            ChatProtocolCompletionChunk completion = new()
            {
                Choices = new List<ChoiceProtocolChoiceDelta> { delta }
            };

            await httpResponse.WriteAsync($"{JsonSerializer.Serialize(completion)}\n", Encoding.UTF8);
        }
    }
}
