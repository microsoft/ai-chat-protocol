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
        private readonly string label = "[Test]"; // All console printouts will be prefixed with this label.
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

            var client = new ChatProtocolClient(new Uri(this.chatEndpoint), options);

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

            var client = new ChatProtocolClient(new Uri(this.chatEndpoint), options);

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

            var client = new ChatProtocolClient(new Uri(this.chatEndpoint), options);

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
        /// <returns>A Task.</returns>
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
            var client = new ChatProtocolClient(new Uri(this.chatStreamingEndpoint), options);
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

            StreamingClientResult<ChatCompletionUpdate> streamingClientResult = client.GetChatCompletionStreaming(chatCompletionOptions);

            this.PrintResponseStatusAndHeaders(streamingClientResult.GetRawResponse());
            Assert.IsFalse(streamingClientResult.GetRawResponse().IsError);
            Assert.AreEqual(200, streamingClientResult.GetRawResponse().Status);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await foreach (ChatCompletionUpdate chatUpdate in streamingClientResult)
            {
                Console.WriteLine($"{this.label}[{stopwatch.ElapsedMilliseconds}ms] {chatUpdate}");
            }

            Console.WriteLine($"{this.label} Done! ({stopwatch.ElapsedMilliseconds}ms)");
            stopwatch.Stop();
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
            var client = new ChatProtocolClient(new Uri(this.chatStreamingEndpoint), options);
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

            Task<StreamingClientResult<ChatCompletionUpdate>> task = client.GetChatCompletionStreamingAsync(chatCompletionOptions);

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

            await foreach (ChatCompletionUpdate chatUpdate in streamingClientResult)
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
                throw new Exception("Environment variables not defined");
            }

            this.chatEndpoint = chatEndpoint!;

            string? chatStreamingEndpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_STREAMING_ENDPOINT");
            if (string.IsNullOrEmpty(chatStreamingEndpoint))
            {
                throw new Exception("Environment variables not defined");
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