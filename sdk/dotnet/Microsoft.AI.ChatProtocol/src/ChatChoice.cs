// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.Text.Json;

    /// <summary> The representation of a single generated completion. </summary>
    public class ChatChoice
    {
        /// <summary>Initializes a new instance of the <see cref="ChatChoice"/> class.</summary>
        /// <param name="index"> The index of the of the chat choice, relative to the other choices in the same completion. </param>
        /// <param name="message"> The chat message for a given chat completion. </param>
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
        /// <param name="finishReason"> The reason this chat completion completed its generation. </param>
        internal ChatChoice(long index, ChatMessage message /*, BinaryData sessionState*/ /*, IReadOnlyDictionary<string, BinaryData> context*/, FinishReason finishReason)
        {
            this.Index = index;
            this.Message = message;

            // this.SessionState = sessionState;
            // this.Context = context;
            this.FinishReason = finishReason;
        }

        /// <summary> Gets the index of the of the chat choice, relative to the other choices in the same completion. </summary>
        public long Index { get; }

        /// <summary>
        /// Gets the chat message for a given chat completion.
        /// </summary>
        public ChatMessage Message { get; }

        /// <summary> Gets the reason this chat completion completed its generation. </summary>
        public FinishReason FinishReason { get; }

        /// <summary> A string representation of the <see cref="ChatChoice"/> object for console or logging printout. </summary>
        /// <returns> A string representation of the <see cref="ChatChoice"/> object. </returns>
        public override string ToString()
        {
            return $"ChatChoice(Index: {this.Index}, FinishReason: {this.FinishReason}, Message: {this.Message})";
        }

        /// <summary> Returns a new ChatChoice object representing the data read from the input JSON element. </summary>
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

            FinishReason finishReason = element.TryGetProperty("finish_reason", out JsonElement jsonFinishReason)
                ? ((jsonFinishReason.GetString() != null) ?
                        new FinishReason(jsonFinishReason.GetString() ?? string.Empty)
                        : throw new Exception("Null `finish_reason` in `choices` element"))
                : throw new Exception("Missing JSON `finish_reason` in `choices` element");

            ChatMessage message = element.TryGetProperty("message", out JsonElement jsonMessage)
                ? ChatMessage.DeserializeChatMessage(jsonMessage)
                : throw new Exception("Missing JSON `message` in `choices` element");

            // BinaryData? sessionState = default;
            // IReadOnlyDictionary<string, BinaryData>? context = default;
//
//            foreach (var property in element.EnumerateObject())
//            {
//                if (property.NameEquals(Encoding.UTF8.GetBytes("index")))
//                {
//                    index = property.Value.GetInt64();
//                    continue;
//                }
//
//                if (property.NameEquals(Encoding.UTF8.GetBytes("message")))
//                {
//                    message = ChatMessage.DeserializeChatMessage(property.Value);
//                    continue;
//                }
//
//
//                if (property.NameEquals(Encoding.UTF8.GetBytes("sessionState")))
//                {
//                    if (property.Value.ValueKind == JsonValueKind.Null)
//                    {
//                        continue;
//                    }
//                    sessionState = BinaryData.FromString(property.Value.GetRawText());
//                    continue;
//                }
//
//
//                if (property.NameEquals(Encoding.UTF8.GetBytes("context")))
//                {
//                    if (property.Value.ValueKind == JsonValueKind.Null)
//                    {
//                        continue;
//                    }
//                    Dictionary<string, BinaryData> dictionary = new Dictionary<string, BinaryData>();
//                    foreach (var property0 in property.Value.EnumerateObject())
//                    {
//                        if (property0.Value.ValueKind == JsonValueKind.Null)
//                        {
//                            dictionary.Add(property0.Name, null);
//                        }
//                        else
//                        {
//                            dictionary.Add(property0.Name, BinaryData.FromString(property0.Value.GetRawText()));
//                        }
//                    }
//                    context = dictionary;
//                    continue;
//                }
//
//                if (property.NameEquals(Encoding.UTF8.GetBytes("finish_reason")))
//                {
//                    finishReason = new FinishReason(property.Value.GetString());
//                    continue;
//                }
//            }
            return new ChatChoice(index, message/*, sessionState */ /*, Optional.ToDictionary(context)*/, finishReason);
        }

        /*
                /// <summary> Deserializes the model from a raw response. </summary>
                /// <param name="response"> The response to deserialize the model from. </param>
                internal static ChatChoice FromResponse(Response response)
                {
                    using var document = JsonDocument.Parse(response.Content);
                    return DeserializeChatChoice(document.RootElement);
                }
        */
        /*
                /// <summary> Initializes a new instance of ChatChoice. </summary>
                /// <param name="index"> The index of the of the chat choice, relative to the other choices in the same completion. </param>
                /// <param name="message"> The chat message for a given chat completion. </param>
                /// <param name="finishReason"> The reason this chat completion completed its generation. </param>
                /// <exception cref="ArgumentNullException"> <paramref name="message"/> is null. </exception>
                internal ChatChoice(long index, ChatMessage message, FinishReason finishReason)
                {
                    Argument.AssertNotNull(message, nameof(message));

                    Index = index;
                    Message = message;
                //    Context = new ChangeTrackingDictionary<string, BinaryData>();
                    FinishReason = finishReason;
                }
        */

        /// <summary>
        /// Field that allows the chat app to store and retrieve data, the structure of such data is dependant on the backend
        /// being used. The client must send back the data in this field unchanged in subsequent requests, until the chat app
        /// sends a new one. The data in this field can be used to implement stateful services, such as remembering previous
        /// conversations or user preferences.
        /// <para>
        /// To assign an object to this property use <see cref="BinaryData.FromObjectAsJson{T}(T, System.Text.Json.JsonSerializerOptions?)"/>.
        /// </para>
        /// <para>
        /// To assign an already formatted json string to this property use <see cref="BinaryData.FromString(string)"/>.
        /// </para>
        /// <para>
        /// Examples:
        /// <list type="bullet">
        /// <item>
        /// <term>BinaryData.FromObjectAsJson("foo")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromString("\"foo\"")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromObjectAsJson(new { key = "value" })</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromString("{\"key\": \"value\"}")</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        // public BinaryData SessionState { get; }

        /// <summary>
        /// Context allows the chat app to receive extra parameters from the client, such as temperature, functions, or
        /// customer_info. These parameters are specific to the chat app and not understood by the generic clients.
        /// <para>
        /// To assign an object to the value of this property use <see cref="BinaryData.FromObjectAsJson{T}(T, System.Text.Json.JsonSerializerOptions?)"/>.
        /// </para>
        /// <para>
        /// To assign an already formatted json string to this property use <see cref="BinaryData.FromString(string)"/>.
        /// </para>
        /// <para>
        /// Examples:
        /// <list type="bullet">
        /// <item>
        /// <term>BinaryData.FromObjectAsJson("foo")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromString("\"foo\"")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromObjectAsJson(new { key = "value" })</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromString("{\"key\": \"value\"}")</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        // public IReadOnlyDictionary<string, BinaryData> Context { get; }
    }
}
