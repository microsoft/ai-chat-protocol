// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Backend.Interfaces;

namespace Backend.Services;

public class EnvVarSecretStore : ISecretStore
{
    public Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken)
    {
#if !DEBUG
        throw new ApplicationException("EnvVarSecretStore should not be used in production.");
#else
        return Task.FromResult(Environment.GetEnvironmentVariable(secretName) ?? "");
#endif
    }
}
