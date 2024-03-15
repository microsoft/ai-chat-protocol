// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.Text.Json;

    /// <summary>
    /// A single, role-attributed message within a chat completion interaction.
    /// </summary>
    public class ChatMessage : IUtf8JsonSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessage"/> class.
        /// </summary>
        /// <param name="role"> The role associated with the message. </param>
        /// <param name="content"> The message content. </param>
        public ChatMessage(ChatRole role, string content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(content);
            }

            if (content.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty string.", content);
            }

            this.Role = role;
            this.Content = content;
        }

        /// <summary>
        /// Gets or sets the role associated with the message.
        /// </summary>
        public ChatRole Role { get; set; }

        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// A string representation of the ChatMessage object for console or logging printout.
        /// </summary>
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
            writer.WriteString("role", this.Role.ToString());
            writer.WriteString("content", this.Content);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Returns a new ChatMessage object representing the data read from the input JSON element.
        /// </summary>
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

            return new ChatMessage(role, content);
        }
    }
}
