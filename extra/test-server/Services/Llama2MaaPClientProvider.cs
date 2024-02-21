// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Net.Http.Headers;

namespace Azure.AI.Chat.SampleService.Services;

public class Llama2MaaPClientProvider : ILlama2MaaPClientProvider
{
    private readonly string? _key;
    private readonly string? _endpoint;
    private readonly string? _deployment;

    public Llama2MaaPClientProvider()
    {
        _key = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_KEY");
        _endpoint = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_ENDPOINT");
        _deployment = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_LLAMA2_MAAP_DEPLOYMENT");
    }

    public HttpClient GetClient()
    {
        if (string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_endpoint))
        {
            throw new InvalidOperationException("Azure Llama-2 MaaP key and endpoint must be set");
        }

        Console.WriteLine("Creating HTTP client for Llama-2 MaaP.");

        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
        };

        var httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _key);
        httpClient.BaseAddress = new Uri(_endpoint);

        return httpClient;
    }

    public string? GetDeployment()
    {
        if (string.IsNullOrEmpty(_deployment))
        {
            throw new InvalidOperationException("Azure Llama-2 MaaP deployment must be set");
        }
        return _deployment;
    }
}
