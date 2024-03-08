// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.ClientModel;
    using System.ClientModel.Primitives;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Client for the Chat Protocol API.
    /// </summary>
    public class ChatProtocolClient
    {
        private static readonly string VERSION = "1.0.0-beta.1";

        private readonly Uri endpoint;
        private readonly ChatProtocolClientOptions clientOptions; // Do we need save this as class member?

        // private readonly ILogger? logger;
        private readonly ClientPipeline clientPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatProtocolClient"/> class.
        /// </summary>
        /// <param name="endpoint"> The connection URL to use. </param>
        /// <param name="clientOptions"> Additional client options. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="endpoint"/>.</exception>
        public ChatProtocolClient(Uri endpoint, ChatProtocolClientOptions? clientOptions = default)
        {
            Argument.AssertNotNull(endpoint, nameof(endpoint));

            this.endpoint = endpoint;
            this.clientOptions = clientOptions ?? new ChatProtocolClientOptions();

            // Authentication policy instance is created from the user-provided
            // credential and service authentication scheme.
            ApiKeyAuthenticationPolicy authenticationPolicy = ApiKeyAuthenticationPolicy.CreateBearerAuthorizationPolicy("my-credential");

            // Pipeline is created from user-provided options and policies
            // specific to the service client implementation.
            this.clientPipeline = ClientPipeline.Create(
                this.clientOptions,
                perCallPolicies: ReadOnlySpan<PipelinePolicy>.Empty,
                perTryPolicies: new PipelinePolicy[] { authenticationPolicy },
                beforeTransportPolicies: ReadOnlySpan<PipelinePolicy>.Empty);

            /*
                        if (this.clientOptions != null && this.clientOptions.LoggerFactory != null)
                        {
                            this.logger = this.clientOptions.LoggerFactory.CreateLogger<ChatProtocolClient>();
                        }
            */
        }

        /// <summary> Creates a new chat completion. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="requestOptions"> The request options. </param>
        /// <returns> The ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="ClientResultException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        public ClientResult<ChatCompletion> GetChatCompletion(ChatCompletionOptions chatCompletionOptions, RequestOptions? requestOptions = null)
        {
            using PipelineMessage pipelineMessage = this.clientPipeline.CreateMessage();

            this.GetChatCompletionInternal(pipelineMessage, chatCompletionOptions, requestOptions).GetAwaiter().GetResult();

            using PipelineResponse response = pipelineMessage.Response!;

            if (response.IsError)
            {
                throw new ClientResultException(response);
            }

// var task = Task.Run(async () => await this.PrivateGetChatCompletionAsync(chatCompletionOptions, cancellationToken));
            string jsonString = response.Content.ToString();

            using JsonDocument document = JsonDocument.Parse(jsonString);

            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            return ClientResult.FromValue(chatCompletion, response);
        }

        /// <summary> Creates a new chat completion. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <returns> The ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="ClientResultException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        public ClientResult<ChatCompletion> GetChatCompletion(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken)
        {
            RequestOptions requestOptions = new ();
            requestOptions.CancellationToken = cancellationToken;

            return this.GetChatCompletion(chatCompletionOptions, requestOptions);
        }

        /// <summary> Creates a new chat completion. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="requestOptions"> The request options to use. </param>
        /// <returns> A Task that encapsulates the ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="HttpRequestException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        public async Task<ClientResult<ChatCompletion>> GetChatCompletionAsync(ChatCompletionOptions chatCompletionOptions, RequestOptions? requestOptions = null)
        {
            using PipelineMessage pipelineMessage = this.clientPipeline.CreateMessage();

            await this.GetChatCompletionInternal(pipelineMessage, chatCompletionOptions, requestOptions).AsTask();

            using PipelineResponse response = pipelineMessage.Response!;

            if (response.IsError)
            {
                throw new ClientResultException(response);
            }

            string jsonString = response.Content.ToString();

            using JsonDocument document = JsonDocument.Parse(jsonString);

            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            return ClientResult.FromValue(chatCompletion, response);

            /*
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"The request failed with status code: {response.StatusCode}." +
                                $" Reason: {response.ReasonPhrase}");
                        }

                        if (this.logger != null)
                        {
                            this.logger.LogHttpRequest(response.RequestMessage, response.RequestMessage?.Content?.ReadAsStringAsync().Result);
                            this.logger.LogHttpResponse(response, response.Content.ReadAsStringAsync().Result);
                        }

                        string jsonString = response.Content.ReadAsStringAsync().Result;

                        using JsonDocument document = JsonDocument.Parse(jsonString);

                        ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

                        return chatCompletion;
            */
        }

        /// <summary> Creates a new chat completion. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <returns> A Task that encapsulates the ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="HttpRequestException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        public async Task<ClientResult<ChatCompletion>> GetChatCompletionAsync(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken)
        {
            RequestOptions requestOptions = new ();
            requestOptions.CancellationToken = cancellationToken;

            ClientResult<ChatCompletion> result = await this.GetChatCompletionAsync(chatCompletionOptions, requestOptions);

            return result;
        }

        private ValueTask GetChatCompletionInternal(
            PipelineMessage pipelineMessage,
            ChatCompletionOptions chatCompletionOptions,
            RequestOptions? requestOptions)
        {
            Argument.AssertNotNull(chatCompletionOptions, nameof(chatCompletionOptions));

            string jsonBody = chatCompletionOptions.SerializeToJson();

           // pipelineMessage.CancellationToken = requestOptions?.CancellationToken ?? default;
            using PipelineRequest request = pipelineMessage.Request;
            request.Method = "POST";
            request.Uri = this.endpoint;

            // If User-Agent request header does not already exist, set it to default value
            const string userAgentHeader = "User-Agent";
            if (!request.Headers.TryGetValue(userAgentHeader, out _))
            {
                request.Headers.Set(userAgentHeader, $"sdk-csharp-microsoft-ai-chatprotocol/{ChatProtocolClient.VERSION}");
            }

            request.Headers.Set("Content-Type", "application/json");
            request.Headers.Add("Accept", "application/json");
            request.Content = BinaryContent.Create(BinaryData.FromString(jsonBody));

            return /*await*/ this.clientPipeline.SendAsync(pipelineMessage);

            // using HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            // using HttpClient client = new HttpClient();
            /*
                        client.DefaultRequestHeaders.Add("User-Agent", $"sdk-csharp-microsoft-ai-chatprotocol/{ChatProtocolClient.VERSION}");

                        if (this.clientOptions != null && this.clientOptions.HttpRequestHeaders != null)
                        {
                            foreach (var header in this.clientOptions.HttpRequestHeaders)
                            {
                                client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            }
                        }
            */

           // return await client.PostAsync(this.endpoint, content, cancellationToken);
        }
    }
}
