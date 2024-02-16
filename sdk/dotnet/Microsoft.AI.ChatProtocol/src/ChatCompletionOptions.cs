// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Microsoft.AI.ChatProtocol
{
    /// <summary> The configuration for a chat completion request. </summary>
    public class ChatCompletionOptions : IUtf8JsonSerializable
    {
        /// <summary> Initializes a new instance of ChatCompletionOptions. </summary>
        /// <param name="messages"> The collection of context messages associated with this completion request. </param>
        /// <param name="stream"> Indicates whether the completion is a streaming or non-streaming completion. </param>
        /// <param name="sessionState">
        /// Field that allows the chat app to store and retrieve data, the structure of such data is dependent on the backend
        /// being used. The client must send back the data in this field unchanged in subsequent requests, until the chat app
        /// sends a new one. The data in this field can be used to implement stateful services, such as remembering previous
        /// conversations or user preferences.
        /// </param>
        /// <param name="context">
        /// Context allows the chat app to receive extra parameters from the client, such as temperature, functions, or
        /// customer_info. These parameters are specific to the chat app and not understood by the generic clients.
        /// </param>
        internal ChatCompletionOptions(IList<ChatMessage> messages, bool stream, Byte[] sessionState, IDictionary<string, Byte[]> context)
        {
            Messages = messages;
            Stream = stream;
            SessionState = sessionState;
            Context = context;
        }

        /// <summary> Initializes a new instance of ChatCompletionOptions. </summary>
        /// <param name="messages"> The collection of context messages associated with this completion request. </param>
        /// <param name="sessionState"> Backend-specific information for the tracking of a session. </param>
        /// <param name="context"> Backend-specific context or arguments. </param>
        public ChatCompletionOptions(IList<ChatMessage> messages, Byte[] sessionState, IDictionary<string, Byte[]> context)
        {
            Argument.AssertNotNull(messages, nameof(messages));
            Messages = messages;
            SessionState = sessionState;
            Context = context;
        }

        /// <summary> Initializes a new instance of ChatCompletionOptions. </summary>
        /// <param name="messages"> The collection of context messages associated with this completion request. </param>
        /// <param name="context"> Backend-specific context or arguments. </param>
        public ChatCompletionOptions(IList<ChatMessage> messages, IDictionary<String, Byte[]> context)
        {
            Argument.AssertNotNull(messages, nameof(messages));
            Messages = messages;
            Context = context;
        }

        /// <summary>
        /// The collection of context messages associated with this completion request.
        /// Please note <see cref="ChatMessage"/> is the base class. According to the scenario, a derived class of the base class might need to be assigned here, or this property needs to be casted to one of the possible derived classes.
        /// The available derived classes include <see cref="TextChatMessage"/>.
        /// </summary>
        public IList<ChatMessage> Messages { get; }

        /// <summary>
        /// Field that allows the chat app to store and retrieve data, the structure of such data is dependent on the backend
        /// being used. The client must send back the data in this field unchanged in subsequent requests, until the chat app
        /// sends a new one. The data in this field can be used to implement stateful services, such as remembering previous
        /// conversations or user preferences.
        /// <para>
        /// To assign an object to this property use <see cref="Byte[].FromObjectAsJson{T}(T, System.Text.Json.JsonSerializerOptions?)"/>.
        /// </para>
        /// <para>
        /// To assign an already formatted json string to this property use <see cref="Byte[].FromString(string)"/>.
        /// </para>
        /// <para>
        /// Examples:
        /// <list type="bullet">
        /// <item>
        /// <term>Byte[].FromObjectAsJson("foo")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>Byte[].FromString("\"foo\"")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>Byte[].FromObjectAsJson(new { key = "value" })</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// <item>
        /// <term>Byte[].FromString("{\"key\": \"value\"}")</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        public Byte[] SessionState { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Context allows the chat app to receive extra parameters from the client, such as temperature, functions, or
        /// customer_info. These parameters are specific to the chat app and not understood by the generic clients.
        /// <para>
        /// To assign an object to the value of this property use <see cref="Byte[].FromObjectAsJson{T}(T, System.Text.Json.JsonSerializerOptions?)"/>.
        /// </para>
        /// <para>
        /// To assign an already formatted json string to this property use <see cref="Byte[].FromString(string)"/>.
        /// </para>
        /// <para>
        /// Examples:
        /// <list type="bullet">
        /// <item>
        /// <term>Byte[].FromObjectAsJson("foo")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>Byte[].FromString("\"foo\"")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>Byte[].FromObjectAsJson(new { key = "value" })</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// <item>
        /// <term>Byte[].FromString("{\"key\": \"value\"}")</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        public IDictionary<String, Byte[]> Context { get; } = new Dictionary<string, byte[]> { };

        /// <summary>
        /// TODO
        /// </summary>
        public bool Stream { get; } = false;

        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("messages"u8);
            writer.WriteStartArray();
            foreach (var item in Messages)
            {
                writer.WriteObjectValue(item);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("stream"u8);
            writer.WriteBooleanValue(Stream);
            if (SessionState != Array.Empty<byte>())
            {
                writer.WritePropertyName("sessionState"u8);
#if NET6_0_OR_GREATER
                writer.WriteRawValue(SessionState);
#else
                using (JsonDocument document = JsonDocument.Parse(SessionState))
                {
                    JsonSerializer.Serialize(writer, document.RootElement);
                }
#endif
            }
            if (Context.Count > 0)
            {
                writer.WritePropertyName("context"u8);
                writer.WriteStartObject();
                foreach (var item in Context)
                {
                    writer.WritePropertyName(item.Key);
                    if (item.Value == null)
                    {
                        writer.WriteNullValue();
                        continue;
                    }
#if NET6_0_OR_GREATER
                    writer.WriteRawValue(item.Value);
#else
                    using (JsonDocument document = JsonDocument.Parse(item.Value))
                    {
                        JsonSerializer.Serialize(writer, document.RootElement);
                    }
#endif
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
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
