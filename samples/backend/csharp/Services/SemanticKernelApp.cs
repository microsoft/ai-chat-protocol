// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using Azure.Identity;
using Microsoft.SemanticKernel;

using Backend.Interfaces;
using Backend.Model;

namespace Backend.Services;

internal record LLMConfig;
internal record OpenAIConfig(string Model, string Key): LLMConfig;
internal record AzureOpenAIConfig(string Deployment, string Endpoint): LLMConfig;

internal struct SemanticKernelConfig
{
    internal LLMConfig LLMConfig { get; private init; }

    internal static async Task<SemanticKernelConfig> CreateAsync(ISecretStore secretStore, CancellationToken cancellationToken)
    {
        var useAzureOpenAI = await secretStore.GetSecretAsync("UseAzureOpenAI", cancellationToken).ContinueWith(task => bool.Parse(task.Result));
        if (useAzureOpenAI)
        {
            var azureDeployment = await secretStore.GetSecretAsync("AzureDeployment", cancellationToken);
            var azureEndpoint = await secretStore.GetSecretAsync("AzureEndpoint", cancellationToken);

            return new SemanticKernelConfig
            {
                LLMConfig = new AzureOpenAIConfig(azureDeployment, azureEndpoint),
            };
        }
        else
        {
            var apiKey = await secretStore.GetSecretAsync("APIKey", cancellationToken);
            var model = await secretStore.GetSecretAsync("Model", cancellationToken);
            return new SemanticKernelConfig
            {
                LLMConfig = new OpenAIConfig(model, apiKey),
            };
        }
    }
}

internal class SemanticKernelSession : ISemanticKernelSession
{
    private readonly Kernel _kernel;
    private readonly IStateStore<string> _stateStore;

    public Guid Id { get; private set; }

    internal SemanticKernelSession(Kernel kernel, IStateStore<string> stateStore, Guid sessionId)
    {
        _kernel = kernel;
        _stateStore = stateStore;
        Id = sessionId;
    }

    const string prompt = @"
        ChatBot can have a conversation with you about any topic.
        It can give explicit instructions or say 'I don't know' if it does not know the answer.

        {{$history}}
        User: {{$userInput}}
        ChatBot:";

    public async Task<AIChatCompletion> ProcessRequest(AIChatRequest message)
    {
        var chatFunction = _kernel.CreateFunctionFromPrompt(prompt);
        var userInput = message.Messages.Last();
        string history = await _stateStore.GetStateAsync(Id) ?? "";
        /* TODO: Add support for text+image content */
        var arguments = new KernelArguments()
        {
            ["history"] = history,
            ["userInput"] = userInput.Content,
        };
        var botResponse = await chatFunction.InvokeAsync(_kernel, arguments);
        var updatedHistory = $"{history}\nUser: {userInput.Content}\nChatBot: {botResponse}";
        await _stateStore.SetStateAsync(Id, updatedHistory);
        return new AIChatCompletion(Message: new AIChatMessage
        {
            Role = AIChatRole.Assistant,
            Content = $"{botResponse}",
        })
        {
            SessionState = Id,
        };
    }

    public async IAsyncEnumerable<AIChatCompletionDelta> ProcessStreamingRequest(AIChatRequest message)
    {
        var chatFunction = _kernel.CreateFunctionFromPrompt(prompt);
        var userInput = message.Messages.Last();
        string history = await _stateStore.GetStateAsync(Id) ?? "";
        var arguments = new KernelArguments()
        {
            ["history"] = history,
            ["userInput"] = userInput.Content,
        };
        var streamedBotResponse = chatFunction.InvokeStreamingAsync(_kernel, arguments);
        StringBuilder response = new();
        await foreach (var botResponse in streamedBotResponse)
        {
            response.Append(botResponse);
            yield return new AIChatCompletionDelta(Delta: new AIChatMessageDelta
            {
                Role = AIChatRole.Assistant,
                Content = $"{botResponse}",
            })
            {
                SessionState = Id,
            };
        }
        var updatedHistory = $"{history}\nUser: {userInput.Content}\nChatBot: {response}";
        await _stateStore.SetStateAsync(Id, updatedHistory);
    }

}

public class SemanticKernelApp : ISemanticKernelApp
{
    private readonly ISecretStore _secretStore;
    private readonly IStateStore<string> _stateStore;
    private readonly Lazy<Task<Kernel>> _kernel;

    private async Task<Kernel> InitKernel()
    {
        var config = await SemanticKernelConfig.CreateAsync(_secretStore, CancellationToken.None);
        var builder = Kernel.CreateBuilder();
        if (config.LLMConfig is AzureOpenAIConfig azureOpenAIConfig)
        {
            if (azureOpenAIConfig.Deployment is null || azureOpenAIConfig.Endpoint is null)
            {
                throw new InvalidOperationException("AzureOpenAI is enabled but AzureDeployment and AzureEndpoint are not set.");
            }
            builder.AddAzureOpenAIChatCompletion(azureOpenAIConfig.Deployment, azureOpenAIConfig.Endpoint, new DefaultAzureCredential());
        }
        else if (config.LLMConfig is OpenAIConfig openAIConfig)
        {
            if (openAIConfig.Model is null || openAIConfig.Key is null)
            {
                throw new InvalidOperationException("AzureOpenAI is disabled but Model and APIKey are not set.");
            }
            builder.AddOpenAIChatCompletion(openAIConfig.Model, openAIConfig.Key);
        }
        else
        {
            throw new InvalidOperationException("Unsupported LLMConfig type.");
        }
        return builder.Build();
    }

    public SemanticKernelApp(ISecretStore secretStore, IStateStore<string> stateStore)
    {
        _secretStore = secretStore;
        _stateStore = stateStore;
        _kernel = new(() => Task.Run(InitKernel));
    }

    public async Task<ISemanticKernelSession> CreateSession(Guid sessionId)
    {
        var kernel = await _kernel.Value;
        return new SemanticKernelSession(kernel, _stateStore, sessionId);
    }

    public async Task<ISemanticKernelSession> GetSession(Guid sessionId)
    {
        var kernel = await _kernel.Value;
        var state = await _stateStore.GetStateAsync(sessionId);
        if (state is null)
        {
            throw new KeyNotFoundException($"Session {sessionId} not found.");
        }
        return new SemanticKernelSession(kernel, _stateStore, sessionId);
    }
}
