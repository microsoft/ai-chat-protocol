// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json;

namespace Microsoft.AI.ChatProtocol
{
    /// <summary> A single, role-attributed text message within a chat completion interaction. </summary>
    public class TextChatMessage : ChatMessage
    {
        /// <summary> Initializes a new instance of TextChatMessage. </summary>
        /// <param name="role"> The role associated with the message. </param>
        /// <param name="content"> The text associated with the message. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="content"/> is null. </exception>
        public TextChatMessage(ChatRole role, string content) : base(role)
        {
            Argument.AssertNotNull(content, nameof(content));

            Kind = MessageKind.Text;
            Content = content;
        }

        /// <summary> Initializes a new instance of TextChatMessage. </summary>
        /// <param name="kind"> The type of the message. If not specified, the message is assumed to be text. </param>
        /// <param name="role"> The role associated with the message. </param>
        /// <param name="sessionState">
        /// Field that allows the chat app to store and retrieve data, the structure of such data is dependent on the backend
        /// being used. The client must send back the data in this field unchanged in subsequent requests, until the chat app
        /// sends a new one. The data in this field can be used to implement stateful services, such as remembering previous
        /// conversations or user preferences.
        /// </param>
        /// <param name="content"> The text associated with the message. </param>
        internal TextChatMessage(MessageKind kind, ChatRole role, Byte[] sessionState, String content) : base(kind, role, sessionState)
        {
            Content = content;
        }

        /// <summary> The text associated with the message. </summary>
        public String Content { get; set; }

        public override void Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", Kind.ToString());
            writer.WriteString("role", Role.ToString());
            writer.WriteString("content", Content);
            writer.WriteEndObject();
        }
    }
}
