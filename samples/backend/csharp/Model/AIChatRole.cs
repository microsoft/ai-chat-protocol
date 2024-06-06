// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

using Backend.Converters;

namespace Backend.Model;

[JsonConverter(typeof(JsonCamelCaseEnumConverter<AIChatRole>))]
public enum AIChatRole
{
    System,
    Assistant,
    User
}
