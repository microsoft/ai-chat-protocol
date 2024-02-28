// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Net.Http.Headers;

namespace Azure.AI.Chat.SampleService.Services;

public class MaaSClientProvider : IMaaSClientProvider
{
    private readonly string? _key;
    private readonly string? _endpoint;

    public MaaSClientProvider()
    {
        _key = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_MAAS_KEY");
        _endpoint = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_MAAS_ENDPOINT");
    }

    public HttpClient GetClient()
    {
        if (string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_endpoint))
        {
            throw new InvalidOperationException("MaaS key and endpoint must be set");
        }

        Console.WriteLine("Creating HTTP client for MaaS.");

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
}
