// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System;
    using System.Text.Json;

    /// <summary>
    /// Represents an incremental item of new data in a streaming response to a chat completion request.
    /// </summary>
    public partial class StreamingChatUpdate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingChatUpdate"/> class.
        /// </summary>
        /// <param name="choiceIndex">The choice index associated with this streamed update.</param>
        /// <param name="role">The <see cref="ChatRole"/> associated with this update.</param>
        /// <param name="contentUpdate">The content fragment associated with this update.</param>
        /// <param name="finishReason">The <see cref="ChatFinishReason"/> associated with this update.</param>
        internal StreamingChatUpdate(
            int? choiceIndex = null,
            ChatRole? role = null,
            string? contentUpdate = null,
            ChatFinishReason? finishReason = null)
        {
            this.ChoiceIndex = choiceIndex;
            this.Role = role;
            this.ContentUpdate = contentUpdate;
            this.FinishReason = finishReason;
        }

        /// <summary>
        /// Gets the <see cref="ChatRole"/> associated with this update.
        /// </summary>
        /// <remarks>
        /// Corresponds to e.g. <c>$.choices[0].delta.role</c> in the underlying REST schema.
        /// </remarks>
        // <see cref="ChatRole"/> assignment typically occurs in a single update across a streamed Chat Completions
        // choice and the value should be considered to be persist for all subsequent updates without a
        // <see cref="ChatRole"/> that bear the same <see cref="ChoiceIndex"/>.
        public ChatRole? Role { get; }

        /// <summary>
        /// Gets the content fragment associated with this update.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Corresponds to e.g. <c>$.choices[0].delta.content</c> in the underlying REST schema.
        /// </para>
        /// Each update contains only a small number of tokens. When presenting or reconstituting a full, streamed
        /// response, all <see cref="ContentUpdate"/> values for the same <see cref="ChoiceIndex"/> should be
        /// combined.
        /// </remarks>
        public string? ContentUpdate { get; }

        /// <summary>
        /// Gets the <see cref="ChatFinishReason"/> associated with this update.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Corresponds to e.g. $.choices[0].finish_reason in the underlying REST schema.
        /// </para>
        /// <para>
        /// <see cref="ChatFinishReason"/> assignment typically appears in the final streamed update message associated
        /// with a choice.
        /// </para>
        /// </remarks>
        public ChatFinishReason? FinishReason { get; }

        /// <summary>
        /// Gets the choice index associated with this streamed update.
        /// </summary>
        /// <remarks>
        /// Corresponds to e.g. <c>$.choices[0].index</c> in the underlying REST schema.
        /// </remarks>
        /*
        /// <para>
        /// Unless a value greater than <c>1</c> was provided as the <c>choiceCount</c> to
        /// <see cref="ChatClient.CompleteChatStreaming(IEnumerable{ChatRequestMessage}, int?, ChatCompletionOptions)"/>,
        /// only one choice will be generated. In that case, this value will always be 0 and may not need to be considered.
        /// </para>
        /// <para>
        /// When a value greater than <c>1</c> to that <c>choiceCount</c> is provided, this index represents
        /// which logical <c>choice</c> the <see cref="StreamingChatUpdate"/> information is associated with. In the event
        /// that a single underlying server-sent event contains multiple choices, multiple instances of
        /// <see cref="StreamingChatUpdate"/> will be created.
        /// </para>
        */
        public int? ChoiceIndex { get; }

        /// <summary>
        /// A string representation of the <see cref="StreamingChatUpdate"/> object for console or logging printout.
        /// </summary>
        /// <returns>
        /// A string representation of the <see cref="StreamingChatUpdate"/> object.
        /// </returns>
        public override string ToString()
        {
            return $"StreamingChatUpdate(ContentUpdate: `{this.ContentUpdate}`, Role: {this.Role}, Index: {this.ChoiceIndex}, FinishReason: {this.FinishReason})";
        }

        /// <summary>
        /// Deserializes a <see cref="StreamingChatUpdate"/> from a <see cref="JsonElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="JsonElement"/> to deserialize.</param>
        /// <returns>A <see cref="StreamingChatUpdate"/> deserialized from the <see cref="JsonElement"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the <see cref="JsonElement"/> is null or not in the expected format.</exception>
        internal static StreamingChatUpdate DeserializeStreamingChatUpdate(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                throw new ArgumentException(nameof(element));
            }

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (property.NameEquals("choices"))
                {
                    foreach (JsonElement choiceElement in property.Value.EnumerateArray())
                    {
                        ChatRole? role = null;
                        string? contentUpdate = null;
                        int? choiceIndex = 0;
                        ChatFinishReason? finishReason = null;

                        foreach (JsonProperty choiceProperty in choiceElement.EnumerateObject())
                        {
                            if (choiceProperty.NameEquals("index"))
                            {
                                choiceIndex = choiceProperty.Value.GetInt32();
                                continue;
                            }

                            if (choiceProperty.NameEquals("finish_reason"))
                            {
                                if (choiceProperty.Value.ValueKind == JsonValueKind.Null)
                                {
                                    finishReason = null;
                                    continue;
                                }

                                finishReason = choiceProperty.Value.GetString() switch
                                {
                                    "stop" => ChatFinishReason.Stopped,
                                    _ => throw new ArgumentException(nameof(finishReason)),
                                };
                                continue;
                            }

                            if (choiceProperty.NameEquals("delta"))
                            {
                                foreach (JsonProperty deltaProperty in choiceProperty.Value.EnumerateObject())
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

                        return new StreamingChatUpdate(choiceIndex, role, contentUpdate, finishReason);
                    }

                    continue;
                }
            }

            throw new ArgumentException("Missing `choices` element");
        }
    }
}
