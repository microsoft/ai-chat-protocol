// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System;
    using System.Text.Json;

    /// <summary>
    /// Represents an incremental item of new data in a streaming response to a chat completion request.
    /// </summary>
    public partial class ChatCompletionDelta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatCompletionDelta"/> class.
        /// </summary>
        /// <param name="role">The <see cref="ChatRole"/> associated with this update.</param>
        /// <param name="contentUpdate">The content fragment associated with this update.</param>
        /// <param name="finishReason">The <see cref="ChatFinishReason"/> associated with this update.</param>
        /// <param name="sessionState"> The state of the conversation session. </param>
        /// <param name="context"> Additional chat context. </param>
        internal ChatCompletionDelta(
            ChatRole? role = null,
            string? contentUpdate = null,
            ChatFinishReason? finishReason = null,
            string? sessionState = null,
            string? context = null)
        {
            this.Role = role;
            this.ContentUpdate = contentUpdate;
            this.FinishReason = finishReason;
            this.SessionState = sessionState;
            this.Context = context;
        }

        /// <summary>
        /// Gets the <see cref="ChatRole"/> associated with this update.
        /// </summary>
        /// <remarks>
        /// <see cref="ChatRole"/> assignment typically occurs in a single update across a streamed Chat Completions
        /// choice and the value should be considered to be persist for all subsequent updates.
        /// </remarks>
        public ChatRole? Role { get; }

        /// <summary>
        /// Gets the content fragment associated with this update.
        /// </summary>
        /// <remarks>
        /// Each update contains only a small number of tokens. When presenting or reconstituting a full, streamed
        /// response, all <see cref="ContentUpdate"/> values should be combined.
        /// </remarks>
        public string? ContentUpdate { get; }

        /// <summary>
        /// Gets the <see cref="ChatFinishReason"/> associated with this update.
        /// </summary>
        /// <remarks>
        /// <see cref="ChatFinishReason"/> assignment typically appears in the final streamed update message associated
        /// with a choice.
        /// </remarks>
        public ChatFinishReason? FinishReason { get; }

        /// <summary>
        /// Gets the state of the conversation session.
        /// </summary>
        public string? SessionState { get; } = null;

        /// <summary>
        /// Gets additional chat context.
        /// </summary>
        public string? Context { get; } = null;

        /// <summary>
        /// A string representation of the <see cref="ChatCompletionDelta"/> object for console or logging printout.
        /// </summary>
        /// <returns>
        /// A string representation of the <see cref="ChatCompletionDelta"/> object.
        /// </returns>
        public override string ToString()
        {
            return $"StreamingChatUpdate(ContentUpdate: `{this.ContentUpdate}`, Role: {this.Role}, FinishReason: {this.FinishReason})";
        }

        /// <summary>
        /// Deserializes a <see cref="ChatCompletionDelta"/> from a <see cref="JsonElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="JsonElement"/> to deserialize.</param>
        /// <returns>A <see cref="ChatCompletionDelta"/> deserialized from the <see cref="JsonElement"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the <see cref="JsonElement"/> is null or not in the expected format.</exception>
        internal static ChatCompletionDelta DeserializeStreamingChatUpdate(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            ChatRole? role = null;
            string? contentUpdate = null;
            ChatFinishReason? finishReason = null;
            string? context = null;
            string? sessionState = null;

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (property.NameEquals("finish_reason"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        finishReason = null;
                        continue;
                    }

                    finishReason = property.Value.GetString() switch
                    {
                        "stop" => ChatFinishReason.Stopped,
                        _ => throw new ArgumentException(nameof(finishReason)),
                    };

                    continue;
                }

                if (property.NameEquals("context"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        context = null;
                        continue;
                    }

                    context = property.Value.GetString();
                    continue;
                }

                if (property.NameEquals("session_state"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        sessionState = null;
                        continue;
                    }

                    sessionState = property.Value.GetString();
                    continue;
                }

                if (property.NameEquals("delta"))
                {
                    foreach (JsonProperty deltaProperty in property.Value.EnumerateObject())
                    {
                        if (deltaProperty.NameEquals("role"))
                        {
                            if (deltaProperty.Value.ValueKind == JsonValueKind.Null)
                            {
                                role = null;
                                continue;
                            }

                            role = deltaProperty.Value.GetString() switch
                            {
                                "system" => ChatRole.System,
                                "user" => ChatRole.User,
                                "assistant" => ChatRole.Assistant,
                                _ => throw new ArgumentException(nameof(role)),
                            };

                            continue;
                        }

                        if (deltaProperty.NameEquals("content"))
                        {
                            contentUpdate = deltaProperty.Value.GetString();
                            continue;
                        }
                    }
                }
            }

            return new ChatCompletionDelta(role, contentUpdate, finishReason, sessionState, context);
        }
    }
}
