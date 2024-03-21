// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol.Test
{
    using System.ClientModel;
    using System.ClientModel.Primitives;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Functional (end-to-end) tests for Microsoft AI Chat Protocol SDK.
    /// </summary>
    [TestClass]
    public class FunctionalTests
    {
        // All console printouts from this test code will be prefixed with this label, to
        // distinguish them from client library logging directed to console output.
        private readonly string label = "[Test]";

        private string chatEndpoint = string.Empty;
        private string chatStreamingEndpoint = string.Empty;

        /// <summary>
        /// Test live chat completion (non-streaming, sync) against a real endpoint.
        /// </summary>
        [TestMethod]
        public void TestGetChatCompletionMultiTurn()
        {
            this.ReadEnvironmentVariables();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var options = new ChatProtocolClientOptions(loggerFactory);

            // Create the client with bearer token key authentication, and client logging to console
            var client = new ChatProtocolClient(new Uri(this.chatEndpoint), new ApiKeyCredential("MyToken"), options);

            // Example of how you set additional headers on each request
            var requestOptions = new RequestOptions();
            requestOptions.SetHeader("TestHeader1", "TestValue1");
            requestOptions.SetHeader("TestHeader2", "TestValue2");

            var chatCompletionOptions = new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.System, "You are an AI assistant that helps people find information"),
                    new ChatMessage(ChatRole.User, "How many feet are in a mile?"),
                },
                sessionState: null,
                context: null);

            Console.WriteLine($"{this.label} {chatCompletionOptions}");

            ClientResult<ChatCompletion> clientResult = client.GetChatCompletion(chatCompletionOptions, requestOptions);

            this.PrintResponse(clientResult.GetRawResponse());

            Console.WriteLine($"{this.label} {clientResult.Value}");

            Assert.IsFalse(clientResult.GetRawResponse().IsError);
            Assert.AreEqual(200, clientResult.GetRawResponse().Status);
            Assert.AreEqual(ChatFinishReason.Stopped, clientResult.Value.FinishReason);
            Assert.AreEqual(ChatRole.Assistant, clientResult.Value.Message.Role);
            Assert.IsTrue(clientResult.Value.Message.Content.Contains("5280") || clientResult.Value.Message.Content.Contains("5,280"));

            clientResult = client.GetChatCompletion(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, "How many feet are in a mile?"),
                    new ChatMessage(ChatRole.Assistant, clientResult.Value.Message.Content.Trim()),
                    new ChatMessage(ChatRole.User, "And how many feet in one kilometer?"),
                }));

            this.PrintResponse(clientResult.GetRawResponse());
            Console.WriteLine($"{this.label} {clientResult.Value}");

            Assert.IsFalse(clientResult.GetRawResponse().IsError);
            Assert.AreEqual(200, clientResult.GetRawResponse().Status);
            Assert.AreEqual(ChatFinishReason.Stopped, clientResult.Value.FinishReason);
            Assert.AreEqual(ChatRole.Assistant, clientResult.Value.Message.Role);
            Assert.IsTrue(clientResult.Value.Message.Content.Contains("3280") || clientResult.Value.Message.Content.Contains("3,280"));
        }

        /// <summary>
        /// Test live chat completion (non-streaming, sync) against a real endpoint, with expected service error
        /// response.
        /// </summary>
        [TestMethod]
        public void TestGetChatCompletionWithServiceError()
        {
            this.ReadEnvironmentVariables();

            var client = new ChatProtocolClient(new Uri(this.chatEndpoint));

            var chatCompletionOptions = new ChatCompletionOptions(
                messages: new[]
                {
                    // Use un-supported role to trigger error response from the service
                    new ChatMessage("not-a-valid-role", "anything here..."),
                },
                sessionState: null,
                context: null);

            Console.WriteLine($"{this.label} {chatCompletionOptions}");

            ClientResultException e = Assert.ThrowsException<ClientResultException>(() => client.GetChatCompletion(chatCompletionOptions));

            // Assert response contains error details
            PipelineResponse? pipelineResponse = e.GetRawResponse();
            Assert.IsNotNull(pipelineResponse);
            this.PrintResponseStatusAndHeaders(pipelineResponse);
            Assert.IsTrue(pipelineResponse.IsError);
            Assert.AreEqual(500, pipelineResponse.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pipelineResponse.Content.ToString()));
            Assert.IsTrue(pipelineResponse.Content.ToString().Contains("The request failed with status code: FailedDependency"));

            // Repeat the call, but now configure the request to not throw on error from the service
            ClientResult<ChatCompletion> clientResult = client.GetChatCompletion(
                chatCompletionOptions,
                new RequestOptions() { ErrorOptions = ClientErrorBehaviors.NoThrow });

            // Assert response contains error details
            pipelineResponse = clientResult.GetRawResponse();
            Assert.IsNotNull(pipelineResponse);
            this.PrintResponseStatusAndHeaders(pipelineResponse);
            Assert.IsTrue(pipelineResponse.IsError);
            Assert.AreEqual(500, pipelineResponse.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pipelineResponse.Content.ToString()));
            Assert.IsTrue(pipelineResponse.Content.ToString().Contains("The request failed with status code: FailedDependency"));

            // Assert empty ChatCompletion object returned on error
            ChatCompletion chatCompletion = clientResult.Value;
            Assert.IsNull(chatCompletion.Context);
            Assert.IsNull(chatCompletion.SessionState);
            Assert.AreEqual(string.Empty, chatCompletion.FinishReason);
            Assert.AreEqual(string.Empty, chatCompletion.Message.Content);
            Assert.AreEqual(string.Empty, chatCompletion.Message.Role);
        }

        /// <summary>
        /// Test live chat completion (non-streaming, async) against a real endpoint.
        /// </summary>
        [TestMethod]
        public void TestGetChatCompletionAsync()
        {
            this.ReadEnvironmentVariables();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });

            var options = new ChatProtocolClientOptions(loggerFactory);

            var client = new ChatProtocolClient(new Uri(this.chatEndpoint), null, options);

            Task<ClientResult<ChatCompletion>> task = client.GetChatCompletionAsync(
                new ChatCompletionOptions(
                    messages: new[]
                    {
                        new ChatMessage(ChatRole.User, "How many feet are in a mile?"),
                    }),
                new RequestOptions() { CancellationToken = CancellationToken.None });

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!task.IsCompleted)
            {
                Console.WriteLine($"{this.label} Waiting for task completion ({stopwatch.ElapsedMilliseconds}ms) ...");
                Thread.Sleep(50);
            }

            Console.WriteLine($"{this.label} Done! ({stopwatch.ElapsedMilliseconds}ms)");
            stopwatch.Stop();

            Assert.IsFalse(task.Result.GetRawResponse().IsError);
            Assert.AreEqual(200, task.Result.GetRawResponse().Status);

            ChatCompletion chatCompletion = task.Result.Value;
            Console.WriteLine($"{this.label} {chatCompletion}");

            Assert.AreEqual(ChatFinishReason.Stopped, chatCompletion.FinishReason);
            Assert.AreEqual(ChatRole.Assistant, chatCompletion.Message.Role);
            Assert.IsTrue(chatCompletion.Message.Content.Contains("5280") || chatCompletion.Message.Content.Contains("5,280"));
        }

        /// <summary>
        /// Test live chat completion (non-streaming, async) against a real endpoint, while
        /// using CancellationToken to cancel the on-going async operation.
        /// </summary>
        [TestMethod]
        public void TestGetChatCompletionAsyncWithCancellationToken()
        {
            this.ReadEnvironmentVariables();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var options = new ChatProtocolClientOptions(loggerFactory);

            var client = new ChatProtocolClient(new Uri(this.chatEndpoint), null, options);

            var cancellationTokenSource = new CancellationTokenSource();

            Task<ClientResult<ChatCompletion>> task = client.GetChatCompletionAsync(
                new ChatCompletionOptions(
                    messages: new[]
                    {
                        new ChatMessage(ChatRole.User, "How many feet are in a mile?"),
                    }),
                new RequestOptions() { CancellationToken = cancellationTokenSource.Token });

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!task.IsCompleted)
            {
                Thread.Sleep(50);
                Console.WriteLine($"{this.label} Waiting for task completion ({stopwatch.ElapsedMilliseconds}ms) ...");
                cancellationTokenSource.Cancel();
            }

            Assert.IsTrue(task.IsCanceled);
        }

        /// <summary>
        /// Test live chat completion (streaming, sync) against a real endpoint.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task TestGetChatCompletionStreaming()
        {
            this.ReadEnvironmentVariables();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var options = new ChatProtocolClientOptions(loggerFactory);
            var client = new ChatProtocolClient(new Uri(this.chatStreamingEndpoint), null, options);
            var chatCompletionOptions = new ChatCompletionOptions(
                messages: new[]
                {
                // new ChatMessage(ChatRole.System, "You are an AI assistant that helps people find information"),
                // new ChatMessage(ChatRole.Assistant, "Hello, how can I help you?"),
                    new ChatMessage(ChatRole.User, "Give me three reasons to regularly exercise."),
                },
                sessionState: null,
                context: null);

            Console.WriteLine($"{this.label} {chatCompletionOptions}");

            StreamingClientResult<ChatCompletionDelta> streamingClientResult = client.GetChatCompletionStreaming(chatCompletionOptions);

            this.PrintResponseStatusAndHeaders(streamingClientResult.GetRawResponse());
            Assert.IsFalse(streamingClientResult.GetRawResponse().IsError);
            Assert.AreEqual(200, streamingClientResult.GetRawResponse().Status);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await foreach (ChatCompletionDelta chatUpdate in streamingClientResult)
            {
                Console.WriteLine($"{this.label}[{stopwatch.ElapsedMilliseconds}ms] {chatUpdate}");
            }

            Console.WriteLine($"{this.label} Done! ({stopwatch.ElapsedMilliseconds}ms)");
            stopwatch.Stop();
        }

        /// <summary>
        /// Test live chat completion (streaming, sync) against a real endpoint, with expected
        /// error response from the service.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task TestGetChatCompletionStreamingWithServiceError()
        {
            this.ReadEnvironmentVariables();
            var client = new ChatProtocolClient(new Uri(this.chatStreamingEndpoint));
            var chatCompletionOptions = new ChatCompletionOptions(
                messages: new[]
                {
                    // Use un-supported role to trigger error response from the service
                    new ChatMessage("not-a-valid-role", "anything here..."),
                },
                sessionState: null,
                context: null);

            Console.WriteLine($"{this.label} {chatCompletionOptions}");

            ClientResultException e = Assert.ThrowsException<ClientResultException>(() => client.GetChatCompletionStreaming(chatCompletionOptions));

            // Assert response contains error details
            PipelineResponse? pipelineResponse = e.GetRawResponse();
            Assert.IsNotNull(pipelineResponse);
            this.PrintResponseStatusAndHeaders(pipelineResponse);
            Assert.IsTrue(pipelineResponse.IsError);
            Assert.AreEqual(500, pipelineResponse.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pipelineResponse.Content.ToString()));
            Assert.IsTrue(pipelineResponse.Content.ToString().Contains("The request failed with status code: FailedDependency"));

            // Repeat the call, but now configure the request to not throw on error from the service
            StreamingClientResult<ChatCompletionDelta> streamingClientResult = client.GetChatCompletionStreaming(
                chatCompletionOptions,
                new RequestOptions() { ErrorOptions = ClientErrorBehaviors.NoThrow });

            // Assert response contains error details
            pipelineResponse = streamingClientResult.GetRawResponse();
            Assert.IsNotNull(pipelineResponse);
            this.PrintResponseStatusAndHeaders(pipelineResponse);
            Assert.IsTrue(pipelineResponse.IsError);
            Assert.AreEqual(500, pipelineResponse.Status);

            // If you try to async enumerate the list of ChatCompletionDelta objects, you will get none
            await foreach (ChatCompletionDelta chatUpdate in streamingClientResult)
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        /// Test live chat completion (streaming, async) against a real endpoint.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task TestGetChatCompletionStreamingAsync()
        {
            this.ReadEnvironmentVariables();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var options = new ChatProtocolClientOptions(loggerFactory);
            var client = new ChatProtocolClient(new Uri(this.chatStreamingEndpoint), null, options);
            var chatCompletionOptions = new ChatCompletionOptions(
                messages: new[]
                {
                // new ChatMessage(ChatRole.System, "You are an AI assistant that helps people find information"),
                // new ChatMessage(ChatRole.Assistant, "Hello, how can I help you?"),
                    new ChatMessage(ChatRole.User, "Give me three reasons to regularly exercise."),
                },
                sessionState: null,
                context: null);

            Console.WriteLine($"{this.label} {chatCompletionOptions}");

            Task<StreamingClientResult<ChatCompletionDelta>> task = client.GetChatCompletionStreamingAsync(chatCompletionOptions);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!task.IsCompleted)
            {
                Console.WriteLine($"{this.label} Waiting for task completion ({stopwatch.ElapsedMilliseconds}ms) ...");
                Thread.Sleep(50);
            }

            var streamingClientResult = task.Result;
            this.PrintResponseStatusAndHeaders(streamingClientResult.GetRawResponse());
            Assert.IsFalse(streamingClientResult.GetRawResponse().IsError);
            Assert.AreEqual(200, streamingClientResult.GetRawResponse().Status);

            await foreach (ChatCompletionDelta chatUpdate in streamingClientResult)
            {
                Console.WriteLine($"{this.label}[{stopwatch.ElapsedMilliseconds}ms] {chatUpdate}");
            }

            Console.WriteLine($"{this.label} Done! ({stopwatch.ElapsedMilliseconds}ms)");
            stopwatch.Stop();
        }

        /// <summary>
        /// Helper method to read environment variables (endpoint and custom HTTP header).
        /// </summary>
        private void ReadEnvironmentVariables()
        {
            string? chatEndpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT");
            if (string.IsNullOrEmpty(chatEndpoint))
            {
                throw new Exception("Environment variable not defined");
            }

            this.chatEndpoint = chatEndpoint!;

            string? chatStreamingEndpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_STREAMING_ENDPOINT");
            if (string.IsNullOrEmpty(chatStreamingEndpoint))
            {
                throw new Exception("Environment variable not defined");
            }

            this.chatStreamingEndpoint = chatStreamingEndpoint!;
        }

        private void PrintResponse(PipelineResponse response)
        {
            this.PrintResponseStatusAndHeaders(response);

            Console.WriteLine($"{this.label} Response body: {response.Content}");
        }

        private void PrintResponseStatusAndHeaders(PipelineResponse response)
        {
            Console.WriteLine($"{this.label} Response status: {response.ReasonPhrase} ({response.Status}).");
            Console.WriteLine($"{this.label} Response headers:");
            foreach (KeyValuePair<string, string> header in response.Headers)
            {
                Console.WriteLine($"{this.label}\t{header.Key} : {header.Value}");
            }
        }
    }
}