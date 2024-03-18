// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.AI.Chat.SampleService.Services;
using Microsoft.AspNetCore.Mvc;

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
        return GlobalSettings.backendChatService switch
        {
            BackendChatService.MaaS => new MaaSChatResponse(_maaSClientProvider.GetClient(), options),
            BackendChatService.Llama2MaaP => new Llama2MaaPChatResponse(_llama2ClientProvider.GetClient(), _llama2ClientProvider.GetDeployment(), options),
            BackendChatService.AzureOpenAI => new OpenAIChatResponse(_openAiClientProvider.GetClient(), _openAiClientProvider.GetDeployment(), options),
            _ => throw new Exception("There is no support for this backend chat service"),
        };
    }

    [Route("stream")]
    [HttpPost]
    public IActionResult CreateStreaming(ChatProtocolCompletionOptions options)
    {
        Console.WriteLine($"\n========== Using backend chat service: {GlobalSettings.backendChatService} ==========\n");
        return GlobalSettings.backendChatService switch
        {
            BackendChatService.MaaS => new MaaSStreamingChatResponse(_maaSClientProvider.GetClient(), options),
            BackendChatService.Llama2MaaP => new BadRequestObjectResult("Not supported"),
            BackendChatService.AzureOpenAI => new OpenAIStreamingChatResponse(_openAiClientProvider.GetClient(), _openAiClientProvider.GetDeployment(), options),
            _ => throw new Exception("There is no support for this backend chat service"),
        };
    }
}
