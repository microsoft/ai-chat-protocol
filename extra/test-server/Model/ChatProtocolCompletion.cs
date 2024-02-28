// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.AI.Chat.SampleService;

public struct ChatProtocolCompletion
{
    [JsonPropertyName("choices")]
    public List<ChatProtocolChoice> Choices { get; set; }
}
