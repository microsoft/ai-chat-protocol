// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Backend.Model;

namespace Backend.Interfaces;
public interface ISemanticKernelSession
{
    Guid Id { get; }
    Task<AIChatCompletion> ProcessRequest(AIChatRequest request);
    IAsyncEnumerable<AIChatCompletionDelta> ProcessStreamingRequest(AIChatRequest request);
}
