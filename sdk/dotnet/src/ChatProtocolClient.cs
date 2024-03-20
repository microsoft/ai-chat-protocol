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

        /// <summary> Creates a new chat completion, with cancellation token. </summary>
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

        /// <summary> Creates a new chat completion, with cancellation token and streaming response. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <returns> The ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="ClientResultException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        /// <remarks> Call this method if the service supports response streaming using the <see href="https://github.com/ndjson/ndjson-spec">Newline Delimited JSON (NDJSON)</see>
        /// or [JSON Lines](https://jsonlines.org/) response formats. NDJSON is identical to JSON Lines, but also allows blank lines. A streaming service will typically respond with
        /// a `Content-Type` request header `application/json-lines`, `application/jsonl`, `application/x-jsonlines` or `application/x-ndjson`.</remarks>
        public StreamingClientResult<ChatCompletionUpdate> GetChatCompletionStreaming(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken)
        {
            return this.GetChatCompletionStreamingAsync(chatCompletionOptions, cancellationToken).GetAwaiter().GetResult();
        }

        /// <summary> Creates a new chat completion with request options. </summary>
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

        /// <summary> Creates a new chat completion, with request options and streaming response. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="requestOptions"> The request options. </param>
        /// <returns> The ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="ClientResultException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        /// <remarks> Call this method if the service supports response streaming using the <see href="https://github.com/ndjson/ndjson-spec">Newline Delimited JSON (NDJSON)</see>
        /// or [JSON Lines](https://jsonlines.org/) response formats. NDJSON is identical to JSON Lines, but also allows blank lines. A streaming service will typically respond with
        /// a `Content-Type` request header `application/json-lines`, `application/jsonl`, `application/x-jsonlines` or `application/x-ndjson`.</remarks>
        public StreamingClientResult<ChatCompletionUpdate> GetChatCompletionStreaming(ChatCompletionOptions chatCompletionOptions, RequestOptions? requestOptions = null)
        {
            return this.GetChatCompletionStreamingAsync(chatCompletionOptions, requestOptions).GetAwaiter().GetResult();
        }

        /// <summary> Creates a new async chat completion with cancellation token. </summary>
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

        /// <summary> Creates a new async chat completion with cancellation token and streaming response. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <returns> A Task that encapsulates the ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="HttpRequestException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        /// <remarks> Call this method if the service supports response streaming using the <see href="https://github.com/ndjson/ndjson-spec">Newline Delimited JSON (NDJSON)</see>
        /// or [JSON Lines](https://jsonlines.org/) response formats. NDJSON is identical to JSON Lines, but also allows blank lines. A streaming service will typically respond with
        /// a `Content-Type` request header `application/json-lines`, `application/jsonl`, `application/x-jsonlines` or `application/x-ndjson`.</remarks>
        public async Task<StreamingClientResult<ChatCompletionUpdate>> GetChatCompletionStreamingAsync(ChatCompletionOptions chatCompletionOptions, CancellationToken cancellationToken)
        {
            RequestOptions requestOptions = new ();
            requestOptions.CancellationToken = cancellationToken;

            StreamingClientResult<ChatCompletionUpdate> result = await this.GetChatCompletionStreamingAsync(chatCompletionOptions, requestOptions);

            return result;
        }

        /// <summary> Creates a new async chat completion with request options. </summary>
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

            await this.clientPipeline.SendAsync(pipelineMessage);

            using PipelineResponse response = pipelineMessage.ExtractResponse() !;

            if (this.logger != null && this.logger.IsEnabled(LogLevel.Information))
            {
                this.logger.LogHttpResponse(this.HttpResponseToString(response));
            }

            if (response.IsError)
            {
                throw new ClientResultException(response);
            }

            if (!response.Headers.TryGetValue("Content-Type", out string? contentType))
            {
                throw new ClientResultException("HTTP response header Content-Type is missing.", response);
            }

            if (!contentType!.Contains("application/json"))
            {
                throw new ClientResultException("Content-Type does not contain application/json.", response);
            }

            string jsonString = response.Content.ToString();

            if (this.logger != null)
            {
                this.logger.LogHttpResponseBody(jsonString);
            }

            using JsonDocument document = JsonDocument.Parse(jsonString);

            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            return ClientResult.FromValue(chatCompletion, response);
        }

        /// <summary> Creates a new async chat completion with request options and streaming response. </summary>
        /// <param name="chatCompletionOptions"> The configuration for a chat completion request. </param>
        /// <param name="requestOptions"> The request options to use. </param>
        /// <returns> A Task that encapsulates the ChatCompletion object containing the chat response from the service. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="chatCompletionOptions"/> is null. </exception>
        /// <exception cref="HttpRequestException"> The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="TaskCanceledException"> The request was canceled. </exception>
        /// <exception cref="InvalidOperationException"> The request URI must be an absolute URI or System.Net.Http.HttpClient.BaseAddress must be set. </exception>
        /// <remarks> Call this method if the service supports response streaming using the <see href="https://github.com/ndjson/ndjson-spec">Newline Delimited JSON (NDJSON)</see>
        /// or [JSON Lines](https://jsonlines.org/) response formats. NDJSON is identical to JSON Lines, but also allows blank lines. A streaming service will typically respond with
        /// a `Content-Type` request header `application/json-lines`, `application/jsonl`, `application/x-jsonlines` or `application/x-ndjson`.</remarks>
        public async Task<StreamingClientResult<ChatCompletionUpdate>> GetChatCompletionStreamingAsync(ChatCompletionOptions chatCompletionOptions, RequestOptions? requestOptions = null)
        {
            requestOptions ??= new RequestOptions();

            chatCompletionOptions.Stream = true;

            /*using*/
            PipelineMessage pipelineMessage = this.CreatePipelineMessage(chatCompletionOptions, requestOptions);
            pipelineMessage.BufferResponse = false;

            await this.clientPipeline.SendAsync(pipelineMessage);

            /*using*/
            PipelineResponse response = pipelineMessage.ExtractResponse() !;

            if (this.logger != null && this.logger.IsEnabled(LogLevel.Information))
            {
                this.logger.LogHttpResponse(this.HttpResponseToString(response));
            }

            // Implementation note checking the response: since there is no standard for streaming JSON lines, this method does not check
            // the value of the HTTP response header Content-Type header, as we do for the non-streaming case.
            // We do, however, expect to see one of the 4 values mentioned in the above remarks.
            if (response.IsError)
            {
                throw new ClientResultException(response);
            }

            ClientResult genericResult = ClientResult.FromResponse(response);

            return StreamingClientResult<ChatCompletionUpdate>.CreateFromResponse(
                genericResult,
                (responseForEnumeration) => JsonLinesAsyncEnumerator.EnumerateFromStream(
                    responseForEnumeration.GetRawResponse().ContentStream,
                    e => ChatCompletionUpdate.DeserializeStreamingChatUpdate(e),
                    this.logger,
                    requestOptions.CancellationToken));
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

            if (!chatCompletionOptions.Stream)
            {
                request.Headers.Set("Accept", "application/json");
            }

            string jsonBody = chatCompletionOptions.SerializeToJson();

            request.Content = BinaryContent.Create(BinaryData.FromString(jsonBody));

            message.Apply(requestOptions);

            if (this.logger != null)
            {
                this.logger.LogHttpRequest(request, jsonBody);
            }

            return message;
        }

        private string HttpResponseToString(PipelineResponse response)
        {
            string responseString = $"Status = {response.ReasonPhrase} ({response.Status}), Headers:\n\t  {{";

            foreach (KeyValuePair<string, string> header in response.Headers)
            {
                responseString += $"\n\t\t{header.Key}: {header.Value}";
            }

            responseString += "\n\t  }";

            return responseString;
        }
    }
}
