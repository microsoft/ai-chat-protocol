// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Backend.Model;

public record AIChatCompletion([property: JsonPropertyName("message")] AIChatMessage Message)
{
    [JsonPropertyName("sessionState")]
    public Guid? SessionState;

    [JsonPropertyName("context")]
    public BinaryData? Context;
}
