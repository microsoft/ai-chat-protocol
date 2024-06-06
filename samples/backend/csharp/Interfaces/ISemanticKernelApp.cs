// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Backend.Interfaces;

public interface ISemanticKernelApp
{
    Task<ISemanticKernelSession> CreateSession(Guid sessionId);
    Task<ISemanticKernelSession> GetSession(Guid sessionId);
}
