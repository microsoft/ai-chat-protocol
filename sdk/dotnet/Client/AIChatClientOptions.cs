namespace Client;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System;
using System.Text.Json;

using static Backend.Model.AIChatCompletion;

public class AIChatClientOptions() : IConfigureOptions<AIChatClientOptions>, IPostConfigureOptions<AIChatClientOptions>, IValidateOptions<AIChatClientOptions>
{
    public AIChatClientOptions(IConfiguration config, IOptions<JsonSerializerOptions> globalSerializerOptions, IOptions<JsonOptions> httpJsonOptions) : this()
    {
        _config = config;
        _globalSerializerOptions = globalSerializerOptions;
        _httpJsonOptions = httpJsonOptions;
    }

    required public Uri ChatEndpointUri { get; init; }

    private static readonly BinaryDataJsonConverter converter = new();
    private readonly IConfiguration? _config;
    private readonly IOptions<JsonSerializerOptions>? _globalSerializerOptions;
    private readonly IOptions<JsonOptions>? _httpJsonOptions;

    public void Configure(AIChatClientOptions options) => _config?.GetRequiredSection("AiChatProtocol").Bind(options);

    public void PostConfigure(string? name, AIChatClientOptions options)
    {
        _globalSerializerOptions?.Value.Converters.Add(converter);
        _httpJsonOptions?.Value.SerializerOptions.Converters.Add(converter);
    }

    public ValidateOptionsResult Validate(string? name, AIChatClientOptions options) => options.ChatEndpointUri is not null ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail("ChatEndpointUri must be set");
}
