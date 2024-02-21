// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.AI.Chat.SampleService.Services;
using Microsoft.AspNetCore.Mvc;
using Azure.AI.OpenAI;

namespace Azure.AI.Chat.SampleService;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IOpenAIClientProvider _openAiClientProvider;
    private readonly ILlama2MaaPClientProvider _llama2ClientProvider;
    private readonly IMaaSClientProvider _maaSClientProvider;

    public ChatController(
        ILogger<ChatController> logger,
        IOpenAIClientProvider clientProvider,
        ILlama2MaaPClientProvider llamaClientProvider,
        IMaaSClientProvider maaSClientProvider)
    {
        _logger = logger;
        _openAiClientProvider = clientProvider;
        _llama2ClientProvider = llamaClientProvider;
        _maaSClientProvider = maaSClientProvider;
    }

    [HttpPost]
    public IActionResult Create(ChatProtocolCompletionOptions options)
    {
        Console.WriteLine($"\n========== Using backend chat service: {GlobalSettings.backendChatService} ==========\n");

        switch (GlobalSettings.backendChatService)
        {
            case BackendChatService.MaaS:
                {
                    HttpClient client = _maaSClientProvider.GetClient();
                    if (options.Stream)
                    {
                          return new MaaSStreamingChatResponse(client, options);
                    }
                    return new MaaSChatResponse(client, options);
                }
            case BackendChatService.Llama2MaaP:
            {
                HttpClient client = _llama2ClientProvider.GetClient();
                string? deployment = _llama2ClientProvider.GetDeployment();
                //TODO: Support streaming
                //if (options.Stream)
                //{
                //      return new Llama2MaaPStreamingChatResponse(client, deployment, options);
                //}
                return new Llama2MaaPChatResponse(client, deployment, options);
            }
            case BackendChatService.AzureOpenAI:
            {
                OpenAIClient client = _openAiClientProvider.GetClient();
                string deployment = _openAiClientProvider.GetDeployment();
                if (options.Stream)
                {
                    return new OpenAIStreamingChatResponse(client, deployment, options);
                }
                return new OpenAIChatResponse(client, deployment, options);
            }
            default:
                throw new Exception("There is no support for this backend chat service");
        }
    }
}
