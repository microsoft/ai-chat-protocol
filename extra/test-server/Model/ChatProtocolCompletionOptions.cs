// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.AI.Chat.SampleService;

public struct ChatProtocolCompletionOptions
{
    [JsonPropertyName("messages")]
    public List<ChatProtocolMessage> Messages { get; set; }

    [JsonPropertyName("session_state")]
    public BinaryData? SessionState { get; set; }

    [JsonPropertyName("context")]
    public Dictionary<string, BinaryData>? Context { get; set; }
}
