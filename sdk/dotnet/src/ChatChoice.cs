// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.Text.Json;

    /// <summary> The representation of a single generated completion. </summary>
    public class ChatChoice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatChoice"/> class.
        /// </summary>
        /// <param name="index"> The index of the of the chat choice, relative to the other choices in the same completion. </param>
        /// <param name="message"> The chat message for a given chat completion. </param>
        /// <param name="finishReason"> The reason this chat completion completed its generation. </param>
        /// <param name="sessionState"> The state of the conversation session. </param>
        /// <param name="context"> Additional chat context. </param>
        internal ChatChoice(long index, ChatMessage message, ChatFinishReason finishReason, string? sessionState = null, string? context = null)
        {
            this.Index = index;
            this.Message = message;
            this.SessionState = sessionState;
            this.Context = context;
            this.FinishReason = finishReason;
        }

        /// <summary>
        /// Gets the index of the of the chat choice, relative to the other choices in the same completion.
        /// </summary>
        public long Index { get; }

        /// <summary>
        /// Gets the chat message for a given chat completion.
        /// </summary>
        public ChatMessage Message { get; }

        /// <summary>
        /// Gets the reason this chat completion completed its generation.
        /// </summary>
        public ChatFinishReason FinishReason { get; }

        /// <summary>
        /// Gets the state of the conversation session.
        /// </summary>
        public string? SessionState { get; } = null;

        /// <summary>
        /// Gets additional chat context.
        /// </summary>
        public string? Context { get; } = null;

        /// <summary>
        /// A string representation of the <see cref="ChatChoice"/> object for console or logging printout.
        /// </summary>
        /// <returns>
        /// A string representation of the <see cref="ChatChoice"/> object.
        /// </returns>
        public override string ToString()
        {
            return $"ChatChoice(Index: {this.Index}, FinishReason: {this.FinishReason}, Message: {this.Message}, SessionState: {this.SessionState}, Context: {this.Context})";
        }

        /// <summary>
        /// Returns a new ChatChoice object representing the data read from the input JSON element.
        /// </summary>
        /// <param name="element"> The JSON element to deserialize. </param>
        /// <returns> The deserialized ChatChoice object. </returns>
        internal static ChatChoice DeserializeChatChoice(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                throw new Exception("Null JSON element in `choices`");
            }

            long index = element.TryGetProperty("index", out JsonElement jsonIndex)
                ? jsonIndex.GetInt64()
                : throw new Exception("Missing JSON `index` in `choices` element");

            ChatMessage message = element.TryGetProperty("message", out JsonElement jsonMessage)
                ? ChatMessage.DeserializeChatMessage(jsonMessage)
                : throw new Exception("Missing JSON `message` in `choices` element");

            ChatFinishReason finishReason = element.TryGetProperty("finish_reason", out JsonElement jsonFinishReason)
                ? new ChatFinishReason(jsonFinishReason.GetString())
                : throw new Exception("Missing JSON `finish_reason` in `choices` element");

            string? sessionState = element.TryGetProperty("session_state", out JsonElement jsonSessionState)
                ? jsonSessionState.GetRawText()
                : null;

            string? context = element.TryGetProperty("context", out JsonElement jsonContext)
                ? jsonContext.GetRawText()
                : null;

            return new ChatChoice(index, message, finishReason, sessionState, context);
        }
    }
}
