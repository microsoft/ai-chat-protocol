// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Model;

public record AIChatCompletion([property: JsonPropertyName("message")] AIChatMessage Message)
{
    [JsonInclude, JsonPropertyName("sessionState"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? SessionState;

    [JsonInclude, JsonPropertyName("context"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(BinaryDataJsonConverter))]
    public BinaryData? Context { get; set; }

    public class BinaryDataJsonConverter : JsonConverter<BinaryData>
    {
        public override BinaryData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType is JsonTokenType.String && reader.TryGetBytesFromBase64(out var b64bytes)
                ? BinaryData.FromBytes(b64bytes)
                : new BinaryData(reader.GetString() ?? string.Empty);

        public override void Write(Utf8JsonWriter writer, BinaryData value, JsonSerializerOptions options)
        {
            writer.WriteBase64StringValue(value);
        }
    }
}
