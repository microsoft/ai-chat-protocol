// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.AI.Chat.SampleService;

public struct ChoiceProtocolChoiceDelta
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public ChatProtocolMessageDelta? Delta { get; set; }

    [JsonPropertyName("context")]
    public Dictionary<string, BinaryData>? Context { get; set; }

    [JsonPropertyName("session_state")]
    public BinaryData? SessionState { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}