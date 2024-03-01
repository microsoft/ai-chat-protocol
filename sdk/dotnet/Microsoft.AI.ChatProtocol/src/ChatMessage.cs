// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Cryptography;
using System.Text.Json;

namespace Microsoft.AI.ChatProtocol
{
    /// <summary>
    /// A single, role-attributed message within a chat completion interaction.
    /// Please note <see cref="ChatMessage"/> is the base class. According to the scenario, a derived class of the base class might need to be assigned here, or this property needs to be casted to one of the possible derived classes.
    /// The available derived classes include <see cref="TextChatMessage"/>.
    /// </summary>
    public abstract class ChatMessage: IUtf8JsonSerializable
    {
        /// <summary> Initializes a new instance of ChatMessage. </summary>
        /// <param name="role"> The role associated with the message. </param>
        protected ChatMessage(ChatRole role)
        {
            Role = role;
            SessionState = Array.Empty<byte>();
        }

        /// <summary> Initializes a new instance of ChatMessage. </summary>
        /// <param name="kind"> The type of the message. If not specified, the message is assumed to be text. </param>
        /// <param name="role"> The role associated with the message. </param>
        /// <param name="sessionState">
        /// Field that allows the chat app to store and retrieve data, the structure of such data is dependent on the backend
        /// being used. The client must send back the data in this field unchanged in subsequent requests, until the chat app
        /// sends a new one. The data in this field can be used to implement stateful services, such as remembering previous
        /// conversations or user preferences.
        /// </param>
        internal ChatMessage(/*MessageKind kind,*/ ChatRole role, Byte[] sessionState)
        {
       //     Kind = kind;
            Role = role;
            SessionState = sessionState;
        }
/*
        /// <summary> The type of the message. If not specified, the message is assumed to be text. </summary>
        internal MessageKind Kind { get; set; }
*/
        /// <summary> The role associated with the message. </summary>
        public ChatRole Role { get; set; }
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
        public Byte[] SessionState { get; set; }

        public abstract void Write(Utf8JsonWriter writer);
    }
}
