// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Backend.Interfaces;

public class InMemoryStore<T> : IStateStore<T>
{
    private readonly Dictionary<Guid, T> _store = new Dictionary<Guid, T>();

    public Task<T?> GetStateAsync(Guid sessionId)
    {
        _store.TryGetValue(sessionId, out var state);
        return Task.FromResult(state);
    }

    public Task SetStateAsync(Guid sessionId, T state)
    {
        _store[sessionId] = state;
        return Task.CompletedTask;
    }

    public Task RemoveStateAsync(Guid sessionId)
    {
        _store.Remove(sessionId);
        return Task.CompletedTask;
    }
}
