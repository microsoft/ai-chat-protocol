// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// The configuration for a chat completion request.
    /// </summary>
    public class ChatCompletionOptions : IUtf8JsonSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatCompletionOptions"/> class.
        /// </summary>
        /// <param name="messages"> The collection of context messages associated with this completion request. </param>
        /// <param name="stream"> Indicates whether the completion is a streaming or non-streaming completion. </param>
        /// <param name="sessionState">
        /// The state of the conversation session. This state originates from the chat service.
        /// The client must send back this data unchanged in subsequent requests, until the chat
        /// service sends a new one. The chat service uses this data to implement stateful services, such as remembering previous
        /// conversations or user preferences. The client library does not process this string, but passes it through to
        /// the chat service as the value of the "session_state" root JSON element in the request payload.
        /// </param>
        /// <param name="context">
        /// Additional chat context. It can be used to define parameters such as temperature,
        /// functions, or customer_info. These settings are specific to the chat service and therefore are not exposed
        /// as settable class properties. The client library does not process this string, but passes it through to
        /// the chat service as the value of the "context" root JSON element in the request payload.
        /// </param>
        /// <remarks>
        /// There could be three formats to the string passed in for context or sessionState:
        /// 1. To pass a single integer value, define as per this example:
        /// string sessionState = "3";
        /// 2. To pass a single string value, define it as per this example:
        /// string sessionState = "\"44967C86-6D52-4F47-B418-82A31C520F3C\"";
        /// 3. To pass a full JSON string, it should start with "{" and end with "}", for example:
        /// string sessionState = "{\"overrides\":{\"temperature\":0.5,\"top\":1,\"retrieval_mode\":\"text\"}}"; .
        /// </remarks>
        public ChatCompletionOptions(IList<ChatMessage> messages, bool stream = false, string? sessionState = null, string? context = null)
        {
            this.Messages = messages;
            this.Stream = stream;
            this.SessionState = sessionState;
            this.Context = context;
        }

        /// <summary>
        /// Gets the collection of context messages associated with this completion request.
        /// </summary>
        public IList<ChatMessage> Messages { get; }

        /// <summary>
        /// Gets a value indicating whether the completion is a streaming or non-streaming.
        /// </summary>
        /// <remarks>Enable streaming only if the service supported <see href="https://github.com/ndjson/ndjson-spec">Newline Delimited JSON (NDJSON)</see> response format.</remarks>
        public bool Stream { get; } = false;

        /// <summary>
        /// Gets the state of the conversation session.
        /// </summary>
        public string? SessionState { get; } = null;

        /// <summary>
        /// Gets additional chat context.
        /// </summary>
        public string? Context { get; } = null;

        /// <summary> A string representation of the ChatCompletionOptions object for console or logging printout. </summary>
        /// <returns> A string representation of the ChatCompletionOptions object. </returns>
        public override string ToString()
        {
            string output = $"ChatCompletionOptions: {this.Messages.Count} Messages, Stream: {this.Stream}, SessionState: {this.SessionState}, Context: {this.Context}";

            foreach (ChatMessage chatMessage in this.Messages)
            {
                output += $"\n{chatMessage}";
            }

            return output;
        }

        /// <summary>
        /// Write the content of this object to a JSON Writer.
        /// </summary>
        /// <param name="writer"> The JSON writer to be updated.</param>
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("messages");
            writer.WriteStartArray();
            foreach (ChatMessage chatMessage in this.Messages)
            {
                ((IUtf8JsonSerializable)chatMessage).Write(writer);
            }

            writer.WriteEndArray();
            writer.WriteBoolean("stream", this.Stream);

            if (this.SessionState != null)
            {
                JsonElement root = JsonDocument.Parse(this.SessionState).RootElement;
                writer.WritePropertyName("session_state");
                root.WriteTo(writer);
            }

            if (this.Context != null)
            {
                JsonElement root = JsonDocument.Parse(this.Context).RootElement;
                writer.WritePropertyName("context");
                root.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Serialize this object to a JSON string.
        /// </summary>
        /// <returns> A JSON string representing the contents of this <see cref="ChatCompletionOptions"/> object. </returns>
        internal string SerializeToJson()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                {
                    ((IUtf8JsonSerializable)this).Write(writer);
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /*
        /// <summary> Convert into a Utf8JsonRequestContent. </summary>
        internal virtual RequestContent ToRequestContent()
        {
            var content = new Utf8JsonRequestContent();
            content.JsonWriter.WriteObjectValue(this);
            return content;
        }
        */
    }
}
