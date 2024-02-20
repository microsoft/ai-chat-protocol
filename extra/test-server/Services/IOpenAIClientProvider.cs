// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.AI.OpenAI;

namespace Azure.AI.Chat.SampleService.Services;

public interface IOpenAIClientProvider
{
    OpenAIClient GetClient();

    string GetDeployment();
}