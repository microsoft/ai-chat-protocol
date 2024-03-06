// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.Text.Json;

    /// <summary> Representation of the response to a chat completion request. </summary>
    public class ChatCompletion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatCompletion"/> class. </summary>
        /// <param name="choices"> The collection of generated completions. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="choices"/> is null. </exception>
        internal ChatCompletion(IEnumerable<ChatChoice> choices)
        {
            Argument.AssertNotNull(choices, nameof(choices));

            this.Choices = choices.ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatCompletion"/> class. </summary>
        /// <param name="choices"> The collection of generated completions. </param>
        internal ChatCompletion(IReadOnlyList<ChatChoice> choices)
        {
            this.Choices = choices;
        }

/*
        /// <summary> Gets the HttpResponseMessage. </summary>
        // public HttpResponseMessage Response { get; internal set; }
*/

        /// <summary> Gets the collection of generated completions. </summary>
        public IReadOnlyList<ChatChoice> Choices { get; }

        /// <summary> A string representation of the ChatCompletion object for console or logging printout. </summary>
        /// <returns> A string representation of the ChatCompletion object. </returns>
        public override string ToString()
        {
            string output = $"ChatCompletion: {this.Choices.Count} choices";

            foreach (ChatChoice chatChoice in this.Choices)
            {
                output += $"\n{chatChoice}";
            }

            return output;
        }

        /// <summary> Returns a new ChatCompletion object representing the data read from teh input JSON element. </summary>
        /// <param name="element"> The JSON element to deserialize. </param>
        /// <returns> The deserialized ChatCompletion object. </returns>
        internal static ChatCompletion DeserializeChatCompletion(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                throw new Exception("Null value for JSON element representing service response");
            }

            if (!element.TryGetProperty("choices", out JsonElement jsonChoices))
            {
                throw new Exception("Missing `choices` element in JSON");
            }

            List<ChatChoice> choices = new List<ChatChoice>();
            foreach (JsonElement item in jsonChoices.EnumerateArray())
            {
                choices.Add(ChatChoice.DeserializeChatChoice(item));
            }

            return new ChatCompletion(choices);
        }

/*
        /// <summary> Deserializes the model from a raw response. </summary>
        /// <param name="response"> The response to deserialize the model from. </param>
        internal static ChatCompletion FromResponse(Response response)
        {
            using var document = JsonDocument.Parse(response.Content);
            return DeserializeChatCompletion(document.RootElement);
        }
*/
    }
}
