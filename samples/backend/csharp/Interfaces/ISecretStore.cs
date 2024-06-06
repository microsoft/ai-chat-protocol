// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Backend.Interfaces;

public interface ISecretStore
{
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
