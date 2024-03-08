// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol.Test
{
    using System.ClientModel;
    using System.ClientModel.Primitives;
    using System.Diagnostics;

    /// <summary>
    /// Functional (end-to-end) tests for Microsoft AI Chat Protocol SDK.
    /// </summary>
    [TestClass]
    public class FunctionalTests
    {
        private string endpoint = string.Empty;
        private string? httpRequestHeaderName = null;
        private string? httpRequestHeaderValue = null;

        /// <summary>
        /// Test live chat completion (non-streaming, sync) against a real endpoint.
        /// </summary>
        [TestMethod]
        public void TestGetChatCompletionMultiTurn()
        {
            this.ReadEnvironmentVariables();
/*
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
*/
            Dictionary<string, string> httpHeaders = new Dictionary<string, string> { { "TestHeader1", "TestValue1" }, { "TestHeader2", "TestValue2" } };

            if (!string.IsNullOrEmpty(this.httpRequestHeaderName) && !string.IsNullOrEmpty(this.httpRequestHeaderValue))
            {
                httpHeaders.Add(this.httpRequestHeaderName, this.httpRequestHeaderValue);
            }

            var options = new ChatProtocolClientOptions(httpHeaders, null /* loggerFactory*/);

            var client = new ChatProtocolClient(new Uri(this.endpoint), options);

            var chatCompletionOptions = new ChatCompletionOptions(
                messages: new[]
                {
                // new ChatMessage(ChatRole.System, "You are an AI assistant that helps people find information"),
                // new ChatMessage(ChatRole.Assistant, "Hello, how can I help you?"),
                    new ChatMessage(ChatRole.User, "How many feet are in a mile?"),
                },
                sessionState: "\"12345\"",
                context: "\"67890\"");

                // context: "67890"); // why doesn't this work?
                // context: "{\"key1\":\"value1\",\"key2\":\"value2\"}"); // why doesn't this work?
                // context: "{\"element1\":{\"key1\":\"value1\",\"key2\":\"value2\"},\"element2\":\"value3\"}"); // Why doesn't this work?
            Console.WriteLine(chatCompletionOptions);

            ClientResult<ChatCompletion> clientResult = client.GetChatCompletion(chatCompletionOptions);

            this.PrintResponse(clientResult.GetRawResponse());
            Console.WriteLine(clientResult.Value);

            Assert.AreEqual(1, clientResult.Value.Choices.Count);
            Assert.AreEqual(0, clientResult.Value.Choices[0].Index);
            Assert.AreEqual(FinishReason.Stopped, clientResult.Value.Choices[0].FinishReason);
            Assert.AreEqual(ChatRole.Assistant, clientResult.Value.Choices[0].Message.Role);
            Assert.IsTrue(clientResult.Value.Choices[0].Message.Content.Contains("5280") || clientResult.Value.Choices[0].Message.Content.Contains("5,280"));

            // Console.WriteLine("Request: " + chatCompletion.Response.RequestMessage);
            // Console.WriteLine("Request body: " + chatCompletion.Response.RequestMessage?.Content?.ReadAsStringAsync().Result);
            // Console.WriteLine("Response: " + chatCompletion.Response);
            // Console.WriteLine("Response body: " + chatCompletion.Response.Content.ReadAsStringAsync().Result);
            // Assert.AreEqual(HttpStatusCode.OK, chatCompletion.Response.StatusCode);
            clientResult = client.GetChatCompletion(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, "How many feet are in a mile?"),
                    new ChatMessage(ChatRole.Assistant, clientResult.Value.Choices[0].Message.Content.Trim()),
                    new ChatMessage(ChatRole.User, "And how many feet in one kilometer?"),
                }));

            this.PrintResponse(clientResult.GetRawResponse());
            Console.WriteLine(clientResult.Value);

            Assert.AreEqual(1, clientResult.Value.Choices.Count);
            Assert.AreEqual(0, clientResult.Value.Choices[0].Index);
            Assert.AreEqual(FinishReason.Stopped, clientResult.Value.Choices[0].FinishReason);
            Assert.AreEqual(ChatRole.Assistant, clientResult.Value.Choices[0].Message.Role);
            Assert.IsTrue(clientResult.Value.Choices[0].Message.Content.Contains("3280") || clientResult.Value.Choices[0].Message.Content.Contains("3,280"));
        }

        /// <summary>
        /// Test live chat completion (non-streaming, async) against a real endpoint.
        /// </summary>
        [TestMethod]
        public void TestGetChatCompletionAsync()
        {
            this.ReadEnvironmentVariables();
            /*
                        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                        {
                            builder.AddConsole();
                            builder.SetMinimumLevel(LogLevel.Information);
                        });
            */
            Dictionary<string, string> httpHeaders = new Dictionary<string, string> { { "TestHeader1", "TestValue1" }, { "TestHeader2", "TestValue2" } };

            if (!string.IsNullOrEmpty(this.httpRequestHeaderName) && !string.IsNullOrEmpty(this.httpRequestHeaderValue))
            {
                httpHeaders.Add(this.httpRequestHeaderName, this.httpRequestHeaderValue);
            }

            var options = new ChatProtocolClientOptions(httpHeaders /*, loggerFactory*/);

            var client = new ChatProtocolClient(new Uri(this.endpoint), options);

            Task<ClientResult<ChatCompletion>> task = client.GetChatCompletionAsync(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, "How many feet are in a mile?"),
                }));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!task.IsCompleted)
            {
                Console.WriteLine($"Waiting for task completion ({stopwatch.ElapsedMilliseconds} ms) ...");
                Thread.Sleep(100);
            }

            Console.WriteLine($"Done! ({stopwatch.ElapsedMilliseconds} ms)");
            stopwatch.Stop();

            ChatCompletion chatCompletion = task.Result.Value;
            Console.WriteLine(chatCompletion);
            Assert.AreEqual(1, chatCompletion.Choices.Count);
            Assert.AreEqual(0, chatCompletion.Choices[0].Index);
            Assert.AreEqual(FinishReason.Stopped, chatCompletion.Choices[0].FinishReason);
            Assert.AreEqual(ChatRole.Assistant, chatCompletion.Choices[0].Message.Role);
            Assert.IsTrue(chatCompletion.Choices[0].Message.Content.Contains("5280") || chatCompletion.Choices[0].Message.Content.Contains("5,280"));
        }

        /// <summary>
        /// Helper method to read environment variables (endpoint and custom HTTP header).
        /// </summary>
        private void ReadEnvironmentVariables()
        {
            string? endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT");

            // Override as needed. These are Pamela's endpoints:
            // endpoint = "https://app-backend-5hhse4yls5chk.azurewebsites.net/chat";
            // endpoint = "https://app-backend-j25rgqsibtmlo.azurewebsites.net/chat";
            // endpoint = "https://app-backend-xw55anu4yrb3k.azurewebsites.net/chat";
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception("Environment variables not defined");
            }

            this.endpoint = endpoint.ToString();

            // Optional: Set one HTTP header
            this.httpRequestHeaderName = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_HTTP_REQUEST_HEADER_NAME");
            this.httpRequestHeaderValue = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_HTTP_REQUEST_HEADER_VALUE");
        }

        private void PrintResponse(PipelineResponse response)
        {
            Console.WriteLine($"Response status code: '{response.Status}'.");
            Console.WriteLine("Response headers:");
            foreach (KeyValuePair<string, string> header in response.Headers)
            {
                Console.WriteLine($"\t{header.Key} : {header.Value}");
            }

            Console.WriteLine($"Response body: '{response.Content}'");
        }
    }
}