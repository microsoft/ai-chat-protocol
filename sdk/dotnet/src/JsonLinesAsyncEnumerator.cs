// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides a set of static methods for creating and working with async enumerators that read from a stream of JSON lines.
    /// </summary>
    internal static class JsonLinesAsyncEnumerator
    {
        /// <summary>
        /// Enumerates a stream of JSON lines from the specified stream, deserializing each line into an instance of <see cref="ChatCompletionUpdate"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="elementDeserializer">The function to use to deserialize each JSON line into an instance of <see cref="ChatCompletionUpdate"/>.</param>
        /// <param name="logger">The logger to use to log the JSON lines as they are read from the stream.</param>
        /// <param name="cancellationToken">The cancellation token to use to cancel the enumeration.</param>
        /// <returns>An async enumerable that yields the deserialized elements from the stream.</returns>
        internal static async IAsyncEnumerable<ChatCompletionUpdate> EnumerateFromStream(
            Stream? stream,
            Func<JsonElement, ChatCompletionUpdate> elementDeserializer,
            ILogger? logger,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                using StreamReader reader = new StreamReader(stream);
                bool done = false;
                while (!cancellationToken.IsCancellationRequested && !done)
                {
                    string? jsonLine = await reader.ReadLineAsync().ConfigureAwait(false);

                    if (jsonLine is null)
                    {
                        break;
                    }

                    if (logger != null)
                    {
                        logger.LogHttpResponseBody(jsonLine);
                    }

                    if (!string.IsNullOrEmpty(jsonLine))
                    {
                        using JsonDocument sseMessageJson = JsonDocument.Parse(jsonLine);

                        ChatCompletionUpdate update = elementDeserializer.Invoke(sseMessageJson.RootElement);

                        if (update.FinishReason == ChatFinishReason.Stopped)
                        {
                            done = true;
                        }

                        yield return update;
                    }
                }
            }
            finally
            {
                // Always dispose the stream immediately once enumeration is complete for any reason
                stream.Dispose();
            }
        }
    }
}