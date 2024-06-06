// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Converters;

public class JsonCamelCaseEnumConverter<T> : JsonStringEnumConverter<T> where T : struct, Enum
{
    public JsonCamelCaseEnumConverter() : base(JsonNamingPolicy.CamelCase)
    {
    }
}
