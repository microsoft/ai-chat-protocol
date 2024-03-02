// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Data;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Microsoft.AI.ChatProtocol
{
    /// <summary>
    /// A single, role-attributed message within a chat completion interaction.
    /// </summary>
    public class ChatMessage: IUtf8JsonSerializable
    {
        /// <summary>
        /// Write this ChatMessage object into a JSON writer ("serialize" the object).
        /// </summary>
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            //     writer.WriteString("kind", Kind.ToString());
            writer.WriteString("role", Role.ToString());
            writer.WriteString("content", Content);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Create a new ChatMessage object representing a given JSON object ("deserialize").
        /// </summary>  
        internal static ChatMessage DeserializeChatMessage(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string content = default;
        //    MessageKind kind = default;
            ChatRole role = default;
           // Optional<BinaryData> sessionState = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(Encoding.UTF8.GetBytes("content")))
                {
                    content = property.Value.GetString() ?? "";
                    continue;
                }
/*
                if (property.NameEquals("kind"u8))
                {
                    kind = new MessageKind(property.Value.GetString());
                    continue;
                }
*/
                if (property.NameEquals(Encoding.UTF8.GetBytes("role")))
                {
                    role = new ChatRole(property.Value.GetString());
                    continue;
                }
/*
                if (property.NameEquals("sessionState"u8))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    sessionState = BinaryData.FromString(property.Value.GetRawText());
                    continue;
                }
*/
            }
            return new ChatMessage(role, content);
        }


        /*
                /// <summary> Initializes a new instance of ChatMessage. </summary>
                /// <param name="role"> The role associated with the message. </param>
                protected ChatMessage(ChatRole role, string content) : this(role, content, Array.Empty<byte>())
                {
                }
        */
        /// <summary> Initializes a new instance of ChatMessage. </summary>
        /// <param name="kind"> The type of the message. If not specified, the message is assumed to be text. </param>
        /// <param name="role"> The role associated with the message. </param>
        /// <param name="sessionState">
        /// Field that allows the chat app to store and retrieve data, the structure of such data is dependent on the backend
        /// being used. The client must send back the data in this field unchanged in subsequent requests, until the chat app
        /// sends a new one. The data in this field can be used to implement stateful services, such as remembering previous
        /// conversations or user preferences.
        /// </param>
        public ChatMessage(/*MessageKind kind,*/ ChatRole role, string content /*, Byte[] sessionState */)
        {
            Argument.AssertNotNullOrEmpty(content, nameof(content));

            //     Kind = kind;
            Role = role;
            Content = content;
         //   SessionState = sessionState;
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
      //  public Byte[] SessionState { get; set; }

        /// <summary> The text associated with the message. </summary>
        public string Content { get; set; }

        public override string ToString()
        {
            return $"ChatMessage(Role: {Role}, Content: \"{Content}\")";
        }
    }
}
