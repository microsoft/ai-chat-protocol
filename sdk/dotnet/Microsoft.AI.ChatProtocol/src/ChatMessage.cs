// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// A single, role-attributed message within a chat completion interaction.
    /// </summary>
    public class ChatMessage : IUtf8JsonSerializable
    {
/*
        /// <param name="sessionState">
        /// Field that allows the chat app to store and retrieve data, the structure of such data is dependent on the backend
        /// being used. The client must send back the data in this field unchanged in subsequent requests, until the chat app
        /// sends a new one. The data in this field can be used to implement stateful services, such as remembering previous
        /// conversations or user preferences.
        /// </param>
*/

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessage"/> class.
        /// </summary>
        /// <param name="role"> The role associated with the message. </param>
        /// <param name="content"> The message content. </param>
        public ChatMessage(ChatRole role, string content /*, Byte[] sessionState */)
        {
            Argument.AssertNotNullOrEmpty(content, nameof(content));

            this.Role = role;
            this.Content = content;

            // SessionState = sessionState;
        }

        /*
                /// <summary> Initializes a new instance of ChatMessage. </summary>
                /// <param name="role"> The role associated with the message. </param>
                protected ChatMessage(ChatRole role, string content) : this(role, content, Array.Empty<byte>())
                {
                }
        */

        /// <summary> Gets or sets the role associated with the message. </summary>
        public ChatRole Role { get; set; }
/*
        /// <summary>
        /// Gets or sets the field that allows the chat app to store and retrieve data, the structure of such data is dependent on the backend
        /// being used. The client must send back the data in this field unchanged in subsequent requests, until the chat app
        /// sends a new one. The data in this field can be used to implement stateful services, such as remembering previous
        /// conversations or user preferences.
        /// <para>
        /// To assign an object to this property use <see cref="byte[].FromObjectAsJson{T}(T, System.Text.Json.JsonSerializerOptions?)"/>.
        /// </para>
        /// <para>
        /// To assign an already formatted json string to this property use <see cref="byte[].FromString(string)"/>.
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
        // public Byte[] SessionState { get; set; }
*/

        /// <summary> Gets or sets the text of the message. </summary>
        public string Content { get; set; }

        /// <summary> A string representation of the ChatMessage object for console or logging printout. </summary>
        /// <returns> A string representation of the ChatMessage object. </returns>
        public override string ToString()
        {
            return $"ChatMessage(Role: {this.Role}, Content: \"{this.Content}\")";
        }

        /// <summary>
        /// Write the content of this object to a JSON Writer.
        /// </summary>
        /// <param name="writer"> The JSON writer to be updated.</param>
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString(Encoding.UTF8.GetBytes("role"), Encoding.UTF8.GetBytes(this.Role.ToString()));
            writer.WriteString(Encoding.UTF8.GetBytes("content"), Encoding.UTF8.GetBytes(this.Content));
            writer.WriteEndObject();
        }

        /// <summary> Returns a new ChatMessage object representing the data read from the input JSON element. </summary>
        /// <param name="element"> The JSON element to deserialize. </param>
        /// <returns> The deserialized ChatMessage object. </returns>
        internal static ChatMessage DeserializeChatMessage(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                throw new Exception("Null JSON element in `messages`");
            }

            ChatRole role = element.TryGetProperty("role", out JsonElement jsonRole)
                ? new ChatRole(jsonRole.GetString())
                : throw new Exception("Missing JSON `role` in `messages` element");

            string content = element.TryGetProperty("content", out JsonElement jsonContent)
                ? (jsonContent.GetString() ?? throw new Exception("Null `content` in `messages` element"))
                : throw new Exception("Missing JSON `content` in `choices` element");
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

                                    }
                        */
            return new ChatMessage(role, content);
        }
    }
}
