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
       // string _key = "";
        string _endpoint = "";

        private void ReadEnvironmentVariables()
        {
           // String? key = Environment.GetEnvironmentVariable("SAMPLE_CHAT_SERVICE_MAAS_KEY");
            String? endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT");

            if (/* String.IsNullOrEmpty(key) || */ String.IsNullOrEmpty(endpoint))
            {
                throw new Exception("Environment variables not defined");
            }

           // _key = key.ToString();
            _endpoint = endpoint.ToString();
        }

        [TestMethod]
        public void TestMethod1()
        {
            ReadEnvironmentVariables();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });


            var options = new ChatProtocolClientOptions(
             // httpHeaders: new Dictionary<string, string> { { "Authorization", "Bearer " + _key } },
                httpHeaders: new Dictionary<string, string> { { "TestHeader1", "TestValue1"}, { "TestHeader2", "TestValue2" }},
                loggerFactory: loggerFactory
            );

            var client = new ChatProtocolClient(new Uri(_endpoint), options);

            HttpResponseMessage response = client.Create(new ChatCompletionOptions(
                messages: new[]
                {
           //         new TextChatMessage(ChatRole.System, "You are an AI assistant that helps people find information"),
             //       new TextChatMessage(ChatRole.Assistant, "Hello, how can I help you?"),
                    new TextChatMessage(ChatRole.User, "How many feet are in a mile?")
                } /*,
                sessionState: Encoding.UTF8.GetBytes("{\"key\":\"value\"}"),
                context: new Dictionary<string, Byte[]>
                {
                    ["key1"] = Encoding.UTF8.GetBytes("value1"),
                    ["key2"] = Encoding.UTF8.GetBytes("value2")
                }*/
            ));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}