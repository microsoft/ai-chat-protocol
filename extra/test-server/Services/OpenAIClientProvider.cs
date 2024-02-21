// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.AI.OpenAI;

namespace Azure.AI.Chat.SampleService.Services;

public class OpenAIClientProvider : IOpenAIClientProvider
{
    private readonly string? _azureOpenAIKey;
    private readonly string? _azureOpenAIEndpoint;
    private readonly string? _azureOpenAIDeployment;

    public OpenAIClientProvider()
    {
        _azureOpenAIKey = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_AZURE_OPENAI_KEY");
        _azureOpenAIEndpoint = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_AZURE_OPENAI_ENDPOINT");
        _azureOpenAIDeployment = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_AZURE_OPENAI_DEPLOYMENT");
    }

    public OpenAIClient GetClient()
    {
        if (_azureOpenAIKey == null || _azureOpenAIEndpoint == null)
        {
            throw new InvalidOperationException("Azure Open AI key and endpoint must be set");
        }

        var clientOptions = new OpenAIClientOptions();
        clientOptions.Diagnostics.IsLoggingEnabled = true;
        clientOptions.Diagnostics.IsLoggingContentEnabled = true;

        Console.WriteLine("Creating Azure OpenAI client.");

        return new OpenAIClient(
            new(_azureOpenAIEndpoint),
            new AzureKeyCredential(_azureOpenAIKey),
            clientOptions);
    }

    public string GetDeployment()
    {
        if (_azureOpenAIDeployment == null)
        {
            throw new InvalidOperationException("Azure Open AI deployment must be set");
        }
        return _azureOpenAIDeployment;
    }
}