// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Net.Http;

namespace Azure.AI.Chat.SampleService.Services;

public interface ILlama2MaaPClientProvider
{
    HttpClient GetClient();

    string? GetDeployment();
}