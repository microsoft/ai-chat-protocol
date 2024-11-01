namespace Client.Tests;

using Backend.Model;

using Client;

using Microsoft.Extensions.Options;

using Moq;
using Moq.AutoMock;
using Moq.Protected;

using System.Net;
using System.Net.Http.Json;

public class AiChatProtocolClientTests
{
    private readonly AIChatClientOptions _config = new()
    {
        ChatEndpointUri = new Uri("https://api.example.com/chat")
    };

    private static readonly AutoMocker mock = new();

    private static void RegisterFactoryWithMockedHttpResponse(HttpResponseMessage expectedResponse, Action<HttpRequestMessage>? requestValidator = null)
    {
        var handlerMock = mock.GetMock<HttpMessageHandler>(enablePrivate: true);
        handlerMock.Protected().Setup<HttpResponseMessage>("Send", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns((HttpRequestMessage req, CancellationToken _) =>
            {
                requestValidator?.Invoke(req);
                return expectedResponse;
            });

        handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                requestValidator?.Invoke(req);
                return expectedResponse;
            });

        mock.GetMock<IHttpClientFactory>().Setup(f => f.CreateClient(AiChatProtocolClient.HttpClientName))
            .Returns(() => new HttpClient(handlerMock.Object));
    }

    public AiChatProtocolClientTests() => mock.Use(Options.Create(_config));

    [Fact]
    public async Task CompleteAsync_ShouldReturnAIChatCompletion_WhenRequestIsSuccessful()
    {
        var request = new AIChatRequest([]);
        var expectedResponse = new AIChatCompletion(new AIChatMessage() { Content = "hi" });

        RegisterFactoryWithMockedHttpResponse(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(expectedResponse)
        });

        var client = mock.CreateInstance<AiChatProtocolClient>();

        var result = await client.CompleteAsync(request, default);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task CompleteAsync_ShouldThrowException_WhenRequestFails()
    {
        RegisterFactoryWithMockedHttpResponse(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest
        });

        var client = mock.CreateInstance<AiChatProtocolClient>();

        await Assert.ThrowsAsync<HttpRequestException>(() => client.CompleteAsync(new([]), default));
    }

    [Fact]
    public async Task CompleteAsync_WithFilesInRequest_IsSuccessful()
    {
        // Arrange
        var expectedResponse = new AIChatCompletion(new AIChatMessage() { Content = "hi" });
        var chatRequest = new AIChatRequest([
            new()
            {
                Content = "Analyze this file for me",
                Files =
                [
                    new AIChatFile { Data = new([ 1, 2, 3 ]), ContentType = "application/octet-stream", Filename = "test.txt" }
                ]
            }
        ])
        { SessionState = Guid.NewGuid() };

        RegisterFactoryWithMockedHttpResponse(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(expectedResponse)
        }, async (HttpRequestMessage r) =>
        {
            var content = r.Content as MultipartFormDataContent;
            Assert.NotNull(content);
            Assert.Equal(2, content.Count());

            var jsonPart = content.First() as JsonContent;
            Assert.NotNull(jsonPart);
            Assert.Equal(typeof(AIChatRequest), jsonPart.ObjectType);

            // The json value in the multipart form should not contain the files
            Assert.Equivalent(chatRequest with { Messages = chatRequest.Messages.Select(i => i with { Files = null }).ToList() }, jsonPart.Value);

            var fileFormContent = content.Skip(1).First() as ReadOnlyMemoryContent;
            Assert.NotNull(fileFormContent);
            Assert.Equal("application/octet-stream", fileFormContent.Headers.ContentType?.MediaType);

            var disp = fileFormContent.Headers.ContentDisposition;
            Assert.Equal("test.txt", disp?.FileName);
            Assert.Equal("test.txt", disp?.FileNameStar);
            Assert.Equal(@"""messages[0].files[0]""", disp?.Name);
            Assert.Equal(3, disp?.Parameters.Count);
            Assert.Equal([1, 2, 3], await fileFormContent.ReadAsByteArrayAsync());
        });

        var client = mock.CreateInstance<AiChatProtocolClient>();

        var result = await client.CompleteAsync(chatRequest, default);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }
}
