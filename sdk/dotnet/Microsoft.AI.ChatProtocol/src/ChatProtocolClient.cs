﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.ClientModel;
    using System.ClientModel.Primitives;
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
        private readonly ChatProtocolClientOptions clientOptions = new ();
        private readonly ILogger? logger = null;
        private readonly ClientPipeline clientPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatProtocolClient"/> class.
        /// </summary>
        /// <param name="endpoint"> The connection URL to use. </param>
        /// <param name="clientOptions"> Additional client options. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="endpoint"/>.</exception>
        public ChatProtocolClient(Uri endpoint, ChatProtocolClientOptions? clientOptions = null)
        {
            Argument.AssertNotNull(endpoint, nameof(endpoint));

            this.endpoint = endpoint;

            this.clientOptions = clientOptions ?? new ChatProtocolClientOptions();

            if (this.clientOptions != null && this.clientOptions.LoggerFactory != null)
            {
                this.logger = this.clientOptions.LoggerFactory.CreateLogger<ChatProtocolClient>();
            }

            ApiKeyAuthenticationPolicy authenticationPolicy = ApiKeyAuthenticationPolicy.CreateBearerAuthorizationPolicy("my-credential");

            this.clientPipeline = ClientPipeline.Create(
                this.clientOptions!,
                perCallPolicies: ReadOnlySpan<PipelinePolicy>.Empty,
                perTryPolicies: new PipelinePolicy[] { authenticationPolicy },
                beforeTransportPolicies: ReadOnlySpan<PipelinePolicy>.Empty);
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
            return this.GetChatCompletionAsync(chatCompletionOptions, cancellationToken).GetAwaiter().GetResult();
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
            return this.GetChatCompletionAsync(chatCompletionOptions, requestOptions).GetAwaiter().GetResult();
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
            requestOptions ??= new RequestOptions();

            using PipelineMessage pipelineMessage = this.CreatePipelineMessage(chatCompletionOptions, requestOptions);

            await this.clientPipeline.SendAsync(pipelineMessage).AsTask();

            using PipelineResponse response = pipelineMessage.Response!;

            if (response.IsError)
            {
                if (requestOptions.ErrorOptions == ClientErrorBehaviors.NoThrow)
                {
                    return (ClientResult<ChatCompletion>)ClientResult.FromResponse(response);
                }
                else
                {
                    throw new ClientResultException(response);
                }
            }

            string jsonString = response.Content.ToString();

            using JsonDocument document = JsonDocument.Parse(jsonString);

            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            return ClientResult.FromValue(chatCompletion, response);

            /*
            if (this.logger != null)
            {
                this.logger.LogHttpRequest(response.RequestMessage, response.RequestMessage?.Content?.ReadAsStringAsync().Result);
                this.logger.LogHttpResponse(response, response.Content.ReadAsStringAsync().Result);
            }
            */
        }

        private PipelineMessage CreatePipelineMessage(ChatCompletionOptions chatCompletionOptions, RequestOptions requestOptions)
        {
            PipelineMessage message = this.clientPipeline.CreateMessage();

            PipelineRequest request = message.Request;

            request.Method = "POST";

            request.Uri = this.endpoint;

            const string userAgentHeader = "User-Agent";

            if (!request.Headers.TryGetValue(userAgentHeader, out _))
            {
                request.Headers.Set(userAgentHeader, $"sdk-csharp-microsoft-ai-chatprotocol/{ChatProtocolClient.VERSION}");
            }

            request.Headers.Set("Content-Type", "application/json");

            request.Headers.Add("Accept", "application/json");

            string jsonBody = chatCompletionOptions.SerializeToJson();

            request.Content = BinaryContent.Create(BinaryData.FromString(jsonBody));

            message.Apply(requestOptions);

            return message;
        }
    }
}
