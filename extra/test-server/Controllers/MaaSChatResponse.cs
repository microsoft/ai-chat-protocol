// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Diagnostics;
using Azure.Core.Sse;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Tracing;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Text;

namespace Azure.AI.Chat.SampleService;

internal class MaaSChatResponseBaseClass
{
    protected readonly HttpClient _httpClient;
    private readonly ChatProtocolCompletionOptions _options;
    private readonly AzureEventSourceListener _listener;

    internal MaaSChatResponseBaseClass(HttpClient httpClient, ChatProtocolCompletionOptions options)
    {
        _httpClient = httpClient;
        _options = options;
        _listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);
    }

    internal JObject BuildRequestBody(bool stream)
    {
        // Get system/user/assistant messages from the client, and build the corresponding
        // message to the backend service
        JArray messages = new JArray();
        foreach (ChatProtocolMessage chatMessage in _options.Messages)
        {
            messages.Add(
                new JObject(
                    new JProperty("role", chatMessage.Role.ToString()),
                    new JProperty("content", chatMessage.Content)
                ));
        }

        JObject requestBody = new JObject(
            new JProperty("messages", messages),
            new JProperty("stream", stream)
        );

        return requestBody;
    }

    internal void PrintHttpRequest(StringContent content)
    {
        Console.WriteLine("HTTP Request URL: " + _httpClient.BaseAddress);
        Console.WriteLine("Headers:");
        foreach (var header in _httpClient.DefaultRequestHeaders)
        {
            Console.WriteLine(header.Key + ": " + string.Join(", ", header.Value));
        }
        foreach (var header in content.Headers)
        {
            Console.WriteLine(header.Key + ": " + string.Join(", ", header.Value));
        }
        Console.WriteLine("Body: " + content.ReadAsStringAsync().Result);
    }

    internal static void PrintHttpResponse(HttpResponseMessage response)
    {
        Console.WriteLine("\nHTTP response:");
        Console.WriteLine(response);
        string body = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine($"Body: {body}");
    }
}

internal class MaaSChatResponse: MaaSChatResponseBaseClass, IActionResult
{
    internal MaaSChatResponse(HttpClient httpClient, ChatProtocolCompletionOptions options)
        : base(httpClient, options)
    {
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        // Example: "{\"input_data\":{\"input_string\":[{\"role\":\"user\",\"content\":\"how many feet in a mile?\"}]}}";
        string requestBody = BuildRequestBody(false).ToString();

        var content = new StringContent(requestBody);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        PrintHttpRequest(content);
        HttpResponseMessage response = await _httpClient.PostAsync("", content);
        PrintHttpResponse(response);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The request failed with status code: {response.StatusCode}");
        }

        string stringResponse = await response.Content.ReadAsStringAsync();
        JObject jObjectResponse = JObject.Parse(stringResponse);

        // Example of response:
        // {"choices":[{"finish_reason":"stop","index":0,"message":{"content":"  There are 5,280 feet in a mile.","role":"assistant"}}],"created":1033408,"id":"02f77102-7e66-4dd7-98a0-8850e4aad32a","object":"chat.completion","usage":{"completion_tokens":15,"prompt_tokens":16,"total_tokens":31}}

        JToken? firstChoice = jObjectResponse["choices"]?.First;
        ChatProtocolCompletion completion = new()
        {
            FinishReason = (string?)firstChoice?["finish_reason"] ?? "",
            Message = new ChatProtocolMessage
            {
                Content = (string?)firstChoice?["message"]?["content"] ?? "",
                Role = (string?)firstChoice?["message"]?["role"] ?? ""
            },
        };

        HttpResponse httpResponse = context.HttpContext.Response;
        httpResponse.StatusCode = (int)HttpStatusCode.OK;
        httpResponse.ContentType = "application/json";
        await httpResponse.WriteAsync(JsonSerializer.Serialize(completion), Encoding.UTF8);
    }
}

internal class MaaSStreamingChatResponse : MaaSChatResponseBaseClass, IActionResult
{
    internal MaaSStreamingChatResponse(HttpClient client, ChatProtocolCompletionOptions options)
        : base(client, options)
    {
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        // For example: {\"messages\": [{\"role\":\"system\",\"content\":\"You are an AI assistant that helps people find information.\"},{\"role\":\"user\",\"content\":\"Give me ten reasons to regularly exercise.\"}],\"stream\": true}
        string requestBody = BuildRequestBody(true).ToString();

        var content = new StringContent(requestBody);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        PrintHttpRequest(content);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = content
        };

        // Read response from the back-end service until you get all headers
        HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        Console.WriteLine("\nHTTP response:");
        Console.WriteLine(response);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The request failed with status code: {response.StatusCode}");
        }

        // Proper response to the client
        HttpResponse httpResponse = context.HttpContext.Response;
        httpResponse.StatusCode = (int)HttpStatusCode.OK;
        httpResponse.ContentType = "text/event-stream";

        using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
        string? role = null;

        try
        {
            using SseReader sseReader = new SseReader(streamReader);

            // Read one line of response at a time from the back-end service
            // Example first line:  data: {"id":"9af3749a-1ce8-4ddd-b6df-8c2824dd83a4","object":"","created":0,"model":"","choices":[{"finish_reason":null,"index":0,"delta":{"role":"assistant"}}]}
            // Example other lines: data: {"id":"9af3749a-1ce8-4ddd-b6df-8c2824dd83a4","object":"chat.completion.chunk","created":1047904,"model":"","choices":[{"finish_reason":null,"index":0,"delta":{"content":" reasons"}}]}
            // Last line:           data: [DONE]
            // Note that "role" is only available in the first line.
            while (true)
            {
                SseLine? sseEvent = await sseReader.TryReadSingleFieldEventAsync().ConfigureAwait(false);

                if (sseEvent is not null)
                {
                    Console.WriteLine($"{sseEvent.ToString()}");

                    ReadOnlyMemory<char> name = sseEvent.Value.FieldName;
                    if (!name.Span.SequenceEqual("data".AsSpan()))
                    {
                        throw new InvalidDataException();
                    }

                    ReadOnlyMemory<char> value = sseEvent.Value.FieldValue;
                    if (value.Span.SequenceEqual("[DONE]".AsSpan()))
                    {
                        break;
                    }

                    JObject jObjectResponse = JObject.Parse(value.ToString());

                    if (role == null)
                    {
                        role = (string?)jObjectResponse["choices"]?[0]?["delta"]?["role"];
                    }

                    if (role == null)
                    {
                        throw new InvalidDataException();
                    }

                    ChatProtocolCompletionChunk completion = new()
                    {
                        Delta = new ChatProtocolMessageDelta
                        {
                            Content = (string?)jObjectResponse["choices"]?[0]?["delta"]?["content"],
                            Role = role
                        },
                        FinishReason = (string?)jObjectResponse["choices"]?[0]?["finish_reason"]
                    };

                    await httpResponse.WriteAsync($"{JsonSerializer.Serialize(completion)}\n", Encoding.UTF8);
                }
            }
        }
        finally
        {
            // Always dispose the stream immediately once enumeration is complete for any reason
            streamReader.Dispose();
        }

        // TODO: Chat protocol supports JSONL response. We don't end with a DONE string ("data: [DONE]" is how you end a SSE payload, not JSONL)
        await httpResponse.WriteAsync("[DONE]\n");
    }
}
