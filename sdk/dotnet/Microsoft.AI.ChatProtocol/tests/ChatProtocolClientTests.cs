// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AI.ChatProtocol;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text;

namespace Microsoft.AI.ChatProtocol.Test
{
    [TestClass]
    public class ChatProtocolClientTests
    {
        string _endpoint = "";
        string? _httpRequestHeaderName;
        string? _httpRequestHeaderValue;

        private void ReadEnvironmentVariables()
        {
            string? endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT");

            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception("Environment variables not defined");
            }

            _endpoint = endpoint.ToString();

            // Optional: Set one HTTP header
            _httpRequestHeaderName = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_HTTP_REQUEST_HEADER_NAME");
            _httpRequestHeaderValue = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_HTTP_REQUEST_HEADER_VALUE");
        }

        [TestMethod]
        public void TestGetChatCompletion()
        {
            ReadEnvironmentVariables();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            Dictionary<string, string> httpHeaders = new Dictionary<string, string> { { "TestHeader1", "TestValue1" }, { "TestHeader2", "TestValue2" } };

            if (!String.IsNullOrEmpty(_httpRequestHeaderName) && !String.IsNullOrEmpty(_httpRequestHeaderValue))
            {
                httpHeaders.Add(_httpRequestHeaderName, _httpRequestHeaderValue);
            }

            var options = new ChatProtocolClientOptions(httpHeaders, loggerFactory);

            var client = new ChatProtocolClient(new Uri(_endpoint), options);

            ChatCompletion chatCompletion = client.GetChatCompletion(new ChatCompletionOptions(
                messages: new[]
                {
           //         new ChatMessage(ChatRole.System, "You are an AI assistant that helps people find information"),
             //       new ChatMessage(ChatRole.Assistant, "Hello, how can I help you?"),
                    new ChatMessage(ChatRole.User, "How many feet are in a mile?")
                } /*,
                sessionState: Encoding.UTF8.GetBytes("{\"key\":\"value\"}"),
                context: new Dictionary<string, Byte[]>
                {
                    ["key1"] = Encoding.UTF8.GetBytes("value1"),
                    ["key2"] = Encoding.UTF8.GetBytes("value2")
                }*/
            ));

            Console.WriteLine(chatCompletion);

        //    Console.WriteLine("Request: " + chatCompletion.Response.RequestMessage);
        //    Console.WriteLine("Request body: " + chatCompletion.Response.RequestMessage?.Content?.ReadAsStringAsync().Result);
        //    Console.WriteLine("Response: " + chatCompletion.Response);
        //    Console.WriteLine("Response body: " + chatCompletion.Response.Content.ReadAsStringAsync().Result);

          //  Assert.AreEqual(HttpStatusCode.OK, chatCompletion.Response.StatusCode);
        }

        [TestMethod]
        public void TestGetChatCompletionAsync()
        {
            ReadEnvironmentVariables();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            Dictionary<string, string> httpHeaders = new Dictionary<string, string> { { "TestHeader1", "TestValue1" }, { "TestHeader2", "TestValue2" } };

            if (!String.IsNullOrEmpty(_httpRequestHeaderName) && !String.IsNullOrEmpty(_httpRequestHeaderValue))
            {
                httpHeaders.Add(_httpRequestHeaderName, _httpRequestHeaderValue);
            }

            var options = new ChatProtocolClientOptions(httpHeaders, loggerFactory);

            var client = new ChatProtocolClient(new Uri(_endpoint), options);

            ChatCompletion chatCompletion = client.GetChatCompletionAsync(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, "How many feet are in a mile?")
                }
            )).Result;

            Console.WriteLine(chatCompletion);
        }
    }
}