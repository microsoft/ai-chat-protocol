namespace Client;

using Backend.Model;

using Client.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

internal class AiChatProtocolClient(IOptions<AIChatClientOptions> config, IHttpClientFactory factory, IOptions<JsonSerializerOptions> serializerOptions, ILogger<AiChatProtocolClient>? _log = null) : IAiChatProtocolClient
{
    public const string HttpClientName = "AIChatClient";

    private readonly HttpClient _httpClient = ConfigureHttpClient(factory.CreateClient(HttpClientName), config.Value);
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions.Value;

    private static HttpClient ConfigureHttpClient(HttpClient client, AIChatClientOptions config)
    {
        if (client.BaseAddress is null)
        {
            client.BaseAddress = config.ChatEndpointUri;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        return client;
    }

    public async Task<AIChatCompletion?> CompleteAsync(AIChatRequest request, CancellationToken cancellationToken)
    {
        _log?.LogTrace("Sending request to chat endpoint");
        var response = await _httpClient.PostAsync(string.Empty, CreateContent(request, cancellationToken), cancellationToken);
        _log?.LogDebug("Received response from chat endpoint: {StatusCode}", response.StatusCode);

        response.EnsureSuccessStatusCode();

        _log?.LogDebug("Deserializing response from chat endpoint");
        if (_log?.IsEnabled(LogLevel.Trace) is true)
        {
            var respString = await response.Content.ReadAsStringAsync(cancellationToken);
            _log?.LogTrace("Response content: {Content}", respString);

            return JsonSerializer.Deserialize<AIChatCompletion>(respString, _serializerOptions);
        }
        else
        {
            return await response.Content.ReadFromJsonAsync<AIChatCompletion>(_serializerOptions, cancellationToken: cancellationToken);
        }
    }

    private static HttpContent CreateContent(AIChatRequest request, CancellationToken cancellationToken)
    {
        if (request.Messages.Any(message => message.Files?.Count is not null and not 0))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var boundary = $"---Part-{Guid.NewGuid()}";

            // Strip off the Files from each message since we add them as parts to the form
            var c = JsonContent.Create(request with { Messages = request.Messages.Select(message => message with { Files = null }).ToList() });
            c.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "json" };

            var multiPartContent = new MultipartFormDataContent(boundary) { c };

            foreach (var part in request.Messages
                .SelectMany((message, index) => message.Files!.Select((file, fileIndex) =>
                    new
                    {
                        Content = new ReadOnlyMemoryContent(file.Data) { Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) } },
                        Name = $"messages[{index}].files[{fileIndex}]",
                        file.Filename,
                    }))
                .Where(part => part is not null))
            {
                cancellationToken.ThrowIfCancellationRequested();

                multiPartContent.Add(part.Content, part.Name, part.Filename);
            }

            return multiPartContent;
        }

        return JsonContent.Create(request);
    }
}
