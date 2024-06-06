// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Backend.Model;

public struct AIChatMessage
{
    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("role")]
    public AIChatRole Role { get; set; }

    [JsonPropertyName("context")]
    public BinaryData? Context { get; set; }
}

