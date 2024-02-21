// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.AI.Chat.SampleService.Services;

public interface IMaaSClientProvider
{
    HttpClient GetClient();
}