// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.Text.Json;

    /// <summary> The representation of a single generated completion. </summary>
    public class ChatCompletion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatCompletion"/> class.
        /// </summary>
        /// <param name="message"> The chat message for a given chat completion. </param>
        /// <param name="finishReason"> The reason this chat completion completed its generation. </param>
        /// <param name="sessionState"> The state of the conversation session. </param>
        /// <param name="context"> Additional chat context. </param>
        internal ChatCompletion(ChatMessage message, ChatFinishReason finishReason, string? sessionState = null, string? context = null)
        {
            this.Message = message;
            this.SessionState = sessionState;
            this.Context = context;
            this.FinishReason = finishReason;
        }

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
        /// A string representation of the <see cref="ChatCompletion"/> object for console or logging printout.
        /// </summary>
        /// <returns>
        /// A string representation of the <see cref="ChatCompletion"/> object.
        /// </returns>
        public override string ToString()
        {
            return $"ChatCompletion(Message: {this.Message}, FinishReason: {this.FinishReason}, SessionState: {this.SessionState}, Context: {this.Context})";
        }

        /// <summary>
        /// Returns a new ChatCompletion object representing the data read from the input JSON element.
        /// </summary>
        /// <param name="element"> The JSON element to deserialize. </param>
        /// <returns> The deserialized ChatCompletion object. </returns>
        internal static ChatCompletion DeserializeChatCompletion(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                throw new Exception("Null JSON element in `choices`");
            }

            // Mandatory
            ChatMessage message = element.TryGetProperty("message", out JsonElement jsonMessage)
                ? ChatMessage.DeserializeChatMessage(jsonMessage)
                : throw new Exception("Missing JSON `message` element");

            // Mandatory
            ChatFinishReason finishReason = element.TryGetProperty("finish_reason", out JsonElement jsonFinishReason)
                ? new ChatFinishReason(jsonFinishReason.GetString())
                : throw new Exception("Missing JSON `finish_reason` element");

            // Optional
            string? sessionState = element.TryGetProperty("session_state", out JsonElement jsonSessionState) ?
                ((jsonSessionState.ValueKind == JsonValueKind.Null) ? null : jsonSessionState.GetRawText())
                : null;

            // Optional
            string? context = element.TryGetProperty("context", out JsonElement jsonContext) ?
                ((jsonContext.ValueKind == JsonValueKind.Null) ? null : jsonContext.GetRawText())
                : null;

            return new ChatCompletion(message, finishReason, sessionState, context);
        }
    }
}
