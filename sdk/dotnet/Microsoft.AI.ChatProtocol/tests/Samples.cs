// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol.Test
{
    using System.ClientModel;
    using System.Diagnostics;

    /// <summary>
    /// Samples for Microsoft AI Chat Protocol SDK.
    /// </summary>
    [TestClass]
    public class Samples
    {
        /// <summary>
        /// Sample showing how submit a question to a chat endpoint
        /// - Using default request options
        /// - A synchronous (blocking) call
        /// - Non-streaming response.
        /// </summary>
        [TestMethod]
        public void DefaultNonStreamingSyncSample()
        {
            string question = "How many feet are in a mile?";

            string endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT")
                ?? throw new Exception("Missing environment variable");

            ChatProtocolClient client = new ChatProtocolClient(new Uri(endpoint));

            ClientResult<ChatCompletion> result = client.GetChatCompletion(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, question),
                }));

            Console.WriteLine($" Question: {question}");
            Console.WriteLine($" Answer: {result.Value.Choices[0].Message.Content}");
        }

        /// <summary>
        /// Sample showing how submit a question to a chat endpoint,
        /// - Using default request options
        /// - An asynchronous (non-blocking) call
        /// - With non-streaming response.
        /// </summary>
        [TestMethod]
        public void DefaultNonStreamingAsyncSample()
        {
            string question = "How many feet are in a mile?";

            string endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT")
                ?? throw new Exception("Missing environment variable");

            ChatProtocolClient client = new ChatProtocolClient(new Uri(endpoint));

            Task<ClientResult<ChatCompletion>> task = client.GetChatCompletionAsync(new ChatCompletionOptions(
                messages: new[]
                {
                        new ChatMessage(ChatRole.User, question),
                }));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!task.IsCompleted)
            {
                Console.WriteLine($" Waiting for task completion ({stopwatch.ElapsedMilliseconds} ms) ...");
                Thread.Sleep(100);
            }

            Console.WriteLine($" Task completed ({stopwatch.ElapsedMilliseconds} ms)");
            stopwatch.Stop();

            Console.WriteLine($" Question: {question}");
            Console.WriteLine($" Answer: {task.Result.Value.Choices[0].Message.Content}");
        }
    }
}