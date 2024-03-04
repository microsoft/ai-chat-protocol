// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Tracing;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Text;

namespace Azure.AI.Chat.SampleService;

internal class Llama2MaaPChatResponseBaseClass
{
    protected readonly HttpClient _httpClient;
    protected readonly string? _deployment;
    private readonly ChatProtocolCompletionOptions _options;
    private readonly AzureEventSourceListener _listener;

    internal Llama2MaaPChatResponseBaseClass(HttpClient httpClient, string? deployment, ChatProtocolCompletionOptions options)
    {
        _httpClient = httpClient;
        _deployment = deployment;
        _options = options;
        _listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);
    }

    internal JObject BuildRequestBody()
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

        JObject requestBody;
        if (GlobalSettings.backendChatService == BackendChatService.Llama2MaaP)
        {
            requestBody = new JObject(
                // Place messages from the client here
                new JProperty("input_data",
                    new JObject(
                        new JProperty("input_string", messages)
                    )
                ),
                // Add additional parameters to overwrite model defaults
                new JProperty("parameters",
                    new JObject(
                        //new JProperty("temperature", "0.6"),
                        //new JProperty("top_p", "0.9"),
                        //new JProperty("do_sample", "true"),
                        new JProperty("max_new_tokens", 2048)
                    )
                )
            );
        }
        else
        {
            requestBody = new JObject(
                // Place messages from the client here
                new JProperty("messages", messages)
            );
        }

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

internal class Llama2MaaPChatResponse: Llama2MaaPChatResponseBaseClass, IActionResult
{
    internal Llama2MaaPChatResponse(HttpClient client, string? deployment, ChatProtocolCompletionOptions options) 
        : base(client, deployment, options)
    {
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        // Example: "{\"input_data\":{\"input_string\":[{\"role\":\"user\",\"content\":\"how many feet in a mile?\"}]}}";
        string requestBody = BuildRequestBody().ToString();

        var content = new StringContent(requestBody);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        if (GlobalSettings.backendChatService == BackendChatService.Llama2MaaP && String.IsNullOrEmpty(_deployment))
        {
            content.Headers.Add("azureml-model-deployment", _deployment);
        }

        PrintHttpRequest(content);

        HttpResponseMessage response = await _httpClient.PostAsync("", content);

        PrintHttpResponse(response);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"The request failed with status code: {response.StatusCode}");
        }

        string stringResponse = await response.Content.ReadAsStringAsync();
        JObject jObjectResponse = JObject.Parse(stringResponse);
        var chatChoice = new ChatProtocolChoice();

        // Example of response: "{\"output\": \"There are 5,280 feet in a mile.\"}"
        chatChoice = new ChatProtocolChoice
        {
            Index = 0,
            FinishReason = "stop",
            Message = new ChatProtocolMessage
            {
                Content = (string?)jObjectResponse["output"] ?? "",
                Role = "assistant"
            },
        };

        var completion = new ChatProtocolCompletion
        {
            Choices = new List<ChatProtocolChoice> { chatChoice }
        };

        HttpResponse httpResponse = context.HttpContext.Response;
        httpResponse.StatusCode = (int)HttpStatusCode.OK;
        httpResponse.ContentType = "application/json";
        await httpResponse.WriteAsync(JsonSerializer.Serialize(completion), Encoding.UTF8);
    }
}
/* Note: Azure does not yet support streaming mode for MaaP
internal class Llama2MaaPStreamingChatResponse : Llama2MaaPChatResponseBaseClass, IActionResult
{
    internal Llama2MaaPStreamingChatResponse(HttpClient client, string? deployment, ChatProtocolCompletionOptions options)
        : base(client, deployment, options)
    {
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        const string chunkedResponseLineStartsWith = "data: {";
        const string chunkedResponseLineEndsWith = "}";
        const string chunkedResponseLineFinal = "data: [DONE]";

        // For example: {\"messages\": [{\"role\":\"system\",\"content\":\"You are an AI assistant that helps people find information.\"},{\"role\":\"user\",\"content\":\"Give me ten reasons to regularly exercise.\"}],\"stream\": true}
        string requestBody = BuildRequestBody().ToString();

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

        // Read one line of response at a time from the back-end service
        // Example first line:  data: {"id":"9af3749a-1ce8-4ddd-b6df-8c2824dd83a4","object":"","created":0,"model":"","choices":[{"finish_reason":null,"index":0,"delta":{"role":"assistant"}}]}
        // Example other lines: data: {"id":"9af3749a-1ce8-4ddd-b6df-8c2824dd83a4","object":"chat.completion.chunk","created":1047904,"model":"","choices":[{"finish_reason":null,"index":0,"delta":{"content":" reasons"}}]}
        // Last line:           data: [DONE]
        // Note that "role" is only available in the first line.
        while (!streamReader.EndOfStream)
        {
            string? stringResponse = await streamReader.ReadLineAsync();

            if (String.IsNullOrWhiteSpace(stringResponse))
                continue;

            Console.WriteLine($"{stringResponse}");

            if (stringResponse == chunkedResponseLineFinal)
                break;

            if (!stringResponse.StartsWith(chunkedResponseLineStartsWith) ||
                !stringResponse.EndsWith(chunkedResponseLineEndsWith))
            {
                throw new Exception($"Malformed chunk response from the service: {stringResponse}");
            }

            JObject jObjectResponse = JObject.Parse(stringResponse.Substring(chunkedResponseLineStartsWith.Length - 1));

            if (role == null)
            {
                role = (string?)jObjectResponse["choices"]?[0]?["delta"]?["role"];
            }

            if (role == null)
            {
                throw new Exception($"Malformed chunk response from the service. Missing 'role': {stringResponse}");
            }

            ChoiceProtocolChoiceDelta delta = new()
            {
                Index = Int32.Parse((string?)jObjectResponse["choices"]?[0]?["index"] ?? "0"),
                Delta = new ChatProtocolMessageDelta
                {
                    Content = (string?)jObjectResponse["choices"]?[0]?["delta"]?["content"],
                    Role = role
                },
                FinishReason = (string?)jObjectResponse["choices"]?[0]?["finish_reason"]
            };

            ChatProtocolCompletionChunk completion = new()
            {
                Choices = new List<ChoiceProtocolChoiceDelta> { delta }
            };

            await httpResponse.WriteAsync($"{JsonSerializer.Serialize(completion)}\n", Encoding.UTF8);
        }
        await httpResponse.WriteAsync("[DONE]\n");
    }
}
*/
