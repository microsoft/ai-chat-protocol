// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Microsoft.AI.ChatProtocol
{
    /// <summary> Representation of the response to a chat completion request. </summary>
    public class ChatCompletion
    {
        internal static ChatCompletion DeserializeChatCompletion(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            IReadOnlyList<ChatChoice> choices = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(Encoding.UTF8.GetBytes("choices")))
                {
                    List<ChatChoice> array = new List<ChatChoice>();
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        array.Add(ChatChoice.DeserializeChatChoice(item));
                    }
                    choices = array;
                    continue;
                }
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
        /// <summary> Initializes a new instance of ChatCompletion. </summary>
        /// <param name="choices"> The collection of generated completions. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="choices"/> is null. </exception>
        internal ChatCompletion(IEnumerable<ChatChoice> choices)
        {
            Argument.AssertNotNull(choices, nameof(choices));

            Choices = choices.ToList();
        }

        /// <summary> Initializes a new instance of ChatCompletion. </summary>
        /// <param name="choices"> The collection of generated completions. </param>
        internal ChatCompletion(IReadOnlyList<ChatChoice> choices)
        {
            Choices = choices;
        }

        /// <summary> Get the HttpResponseMessage </summary>
      //  public HttpResponseMessage Response { get; internal set; }

        /// <summary> The collection of generated completions. </summary>
        public IReadOnlyList<ChatChoice> Choices { get; }

        public override string ToString()
        {
            string output = $"ChatCompletion: {Choices.Count} choices";

            foreach (ChatChoice chatChoice in Choices)
            {
                output += $"\n{chatChoice}";
            }

            return output;
        }
    }
}
