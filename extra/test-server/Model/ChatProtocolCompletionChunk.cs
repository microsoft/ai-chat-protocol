// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.AI.Chat.SampleService;

public struct ChatProtocolCompletionChunk
{
    [JsonPropertyName("choices")]
    public List<ChoiceProtocolChoiceDelta> Choices { get; set; }
}
