// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol.Test
{
    using System.ClientModel;
    using System.ClientModel.Primitives;
    using System.Diagnostics;

    /// <summary>
    /// Samples for Microsoft AI Chat Protocol SDK.
    /// </summary>
    [TestClass]
    public class Samples
    {
        /// <summary>
        /// Sample showing how to submit a question to a chat endpoint
        /// - Using default request options
        /// - A synchronous (blocking) call
        /// - Non-streaming response.
        /// </summary>
        [TestMethod]
        public void GetChatCompletion()
        {
            string question = "How many feet are in a mile?";

            string endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT")
                ?? throw new Exception("Missing environment variable");

            ChatProtocolClient client = new ChatProtocolClient(new Uri(endpoint));

            Console.WriteLine($" Question: {question}");

            ClientResult<ChatCompletion> result = client.GetChatCompletion(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, question),
                }));

            Console.WriteLine($" Answer: {result.Value.Message.Content}");
        }

        /// <summary>
        /// Sample showing how submit a question to a chat endpoint,
        /// - Using request options with "Authorization" HTTP request header
        /// - An asynchronous (non-blocking) call
        /// - With non-streaming response.
        /// </summary>
        [TestMethod]
        public void GetChatCompletionAsync()
        {
            string question = "How many feet are in a mile?";

            string endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT")
                ?? throw new Exception("Missing environment variable");

            ChatProtocolClient client = new ChatProtocolClient(new Uri(endpoint));

            var requestOptions = new RequestOptions();
            requestOptions.SetHeader("Authorization", "Bearer <your-key-here>");

            Console.WriteLine($" Question: {question}");

            Task<ClientResult<ChatCompletion>> task = client.GetChatCompletionAsync(
                new ChatCompletionOptions(
                    messages: new[]
                    {
                            new ChatMessage(ChatRole.User, question),
                    }),
                requestOptions);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!task.IsCompleted)
            {
                Console.WriteLine($" Waiting for task completion ({stopwatch.ElapsedMilliseconds}ms) ...");
                Thread.Sleep(50);
            }

            Console.WriteLine($" Task completed ({stopwatch.ElapsedMilliseconds}ms)");
            stopwatch.Stop();

            Console.WriteLine($" Answer: {task.Result.Value.Message.Content}");
        }

        /// <summary>
        /// Sample showing how to submit a question to a chat endpoint
        /// - Using default request options
        /// - A synchronous (blocking) call
        /// - With streaming response.
        /// </summary>
        /// <returns>An asynchronous task.</returns>
        [TestMethod]
        public async Task GetChatCompletionStreaming()
        {
            string question = "Give me three good reasons why I should regularly exercise.";

            string endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_STREAMING_ENDPOINT")
                ?? throw new Exception("Missing environment variable");

            ChatProtocolClient client = new ChatProtocolClient(new Uri(endpoint));

            Console.WriteLine($" Question: {question}");

            StreamingClientResult<ChatCompletionUpdate> result = client.GetChatCompletionStreaming(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, question),
                }));

            string answer = string.Empty;
            await foreach (ChatCompletionUpdate chatUpdate in result)
            {
                answer += chatUpdate.ContentUpdate;
                Console.Write($"\rAnswer: {answer}");
            }
        }

        /// <summary>
        /// Sample showing how to submit a question to a chat endpoint
        /// - Using default request options
        /// - An asynchronous (non-blocking) call
        /// - With streaming response.
        /// </summary>
        /// <returns>An asynchronous task.</returns>
        [TestMethod]
        public async Task GetChatCompletionStreamingAsync()
        {
            string question = "Give me three good reasons why I should regularly exercise.";

            string endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_STREAMING_ENDPOINT")
                ?? throw new Exception("Missing environment variable");

            ChatProtocolClient client = new ChatProtocolClient(new Uri(endpoint));

            Console.WriteLine($" Question: {question}");

            Task<StreamingClientResult<ChatCompletionUpdate>> task = client.GetChatCompletionStreamingAsync(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, question),
                }));

            while (!task.IsCompleted)
            {
                Thread.Sleep(50);
            }

            string answer = string.Empty;
            await foreach (ChatCompletionUpdate chatUpdate in task.Result)
            {
                answer += chatUpdate.ContentUpdate;
                Console.Write($"\rAnswer: {answer}");
            }
        }
    }
}