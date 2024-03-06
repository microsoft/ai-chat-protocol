// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

// TODO:
// - Add unit-tests to test JSON serialization and deserialization
// - Add support for JSON-L streaming
// - Use class names that are different than AOAI SDK
// - Do we need the Arguments.cs class?
// - Should I expose HttpResponseMessage to the caller?
//   When I updated the test app to get chatCompletion.Response.RequestMessage?.Content?.ReadAsStringAsync().Result
//   I got an exception saying the Content was already disposed. Should I just expose response status code, response
//   headers and raw JSON as sperate properties?
// - Test with CancellationToken
namespace Microsoft.AI.ChatProtocol
{
    using System.Text;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Client for the Chat Protocol API.
    /// </summary>
    public class ChatProtocolClient
    {
        private static readonly string VERSION = "1.0.0-beta.1";

        private readonly Uri endpoint;
        private readonly ChatProtocolClientOptions? clientOptions;
        private readonly ILogger? logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatProtocolClient"/> class.
        /// </summary>
        /// <param name="endpoint"> The connection URL to use. </param>
        /// <param name="clientOptions"> Additional client options. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="endpoint"/>.</exception>
        public ChatProtocolClient(Uri endpoint, ChatProtocolClientOptions? clientOptions = null)
        {
            Argument.AssertNotNull(endpoint, nameof(endpoint));
            Argument.AssertNotNull(clientOptions, nameof(clientOptions));

            this.endpoint = endpoint;
            this.clientOptions = clientOptions;

            if (this.clientOptions != null && this.clientOptions.LoggerFactory != null)
            {
                this.logger = this.clientOptions.LoggerFactory.CreateLogger<ChatProtocolClient>();
            }
        }

        /// <summary> Creates a new chat completion. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <returns> The ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="HttpRequestException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        public ChatCompletion GetChatCompletion(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken = default)
        {
            var task = Task.Run(async () => await this.PrivateGetChatCompletionAsync(chatCompletionOptions, cancellationToken));
            using HttpResponseMessage response = task.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"The request failed with status code: {response.StatusCode}." +
                    $" Reason: {response.ReasonPhrase}");
            }

            if (this.logger != null)
            {
                this.logger.LogHttpRequest(response.RequestMessage /*, response.RequestMessage?.Content?.ReadAsStringAsync().Result*/);
                this.logger.LogHttpResponse(response, response.Content.ReadAsStringAsync().Result);
            }

            string jsonString = response.Content.ReadAsStringAsync().Result;

            using JsonDocument document = JsonDocument.Parse(jsonString);

            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            return chatCompletion;
        }

        /// <summary> Creates a new chat completion. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <returns> A Task that encapsulates the ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="HttpRequestException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        public async Task<ChatCompletion> GetChatCompletionAsync(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await this.PrivateGetChatCompletionAsync(chatCompletionOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"The request failed with status code: {response.StatusCode}." +
                    $" Reason: {response.ReasonPhrase}");
            }

            if (this.logger != null)
            {
                this.logger.LogHttpRequest(response.RequestMessage /*, response.RequestMessage?.Content?.ReadAsStringAsync().Result*/);
                this.logger.LogHttpResponse(response, response.Content.ReadAsStringAsync().Result);
            }

            string jsonString = response.Content.ReadAsStringAsync().Result;

            using JsonDocument document = JsonDocument.Parse(jsonString);

            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            return chatCompletion;
        }

        private async Task<HttpResponseMessage> PrivateGetChatCompletionAsync(
            ChatCompletionOptions chatCompletionOptions,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(chatCompletionOptions, nameof(chatCompletionOptions));

            string jsonBody = chatCompletionOptions.SerializeToJson();
            using HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", $"sdk-csharp-microsoft-ai-chatprotocol/{ChatProtocolClient.VERSION}");

            if (this.clientOptions != null && this.clientOptions.HttpRequestHeaders != null)
            {
                foreach (var header in this.clientOptions.HttpRequestHeaders)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            return await client.PostAsync(this.endpoint, content, cancellationToken);
        }
    }
}
