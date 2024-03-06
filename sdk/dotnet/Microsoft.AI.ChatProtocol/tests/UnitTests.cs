// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol.Test
{
    using System.Text.Json;

    /// <summary>
    /// Unit tests for Microsoft AI Chat Protocol SDK.
    /// </summary>
    [TestClass]
    public class UnitTests
    {
        /// <summary>
        /// Test deserialization (JSON string to C# class objects).
        /// </summary>
        [TestMethod]
        public void TestParsingJsonResponseBody()
        {
            string jsonString = "{\"choices\":[{\"finish_reason\":\"stop\",\"index\":0,\"message\":{\"content\":\"There are 5,280 feet in a mile.\",\"role\":\"assistant\"}}],\"created\":1795472,\"id\":\"298157ba-853f-4352-8551-dcbdcfb655f3\",\"object\":\"chat.completion\",\"usage\":{\"completion_tokens\":15,\"prompt_tokens\":16,\"total_tokens\":31}}";
            using JsonDocument document = JsonDocument.Parse(jsonString);
            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            Assert.AreEqual(1, chatCompletion.Choices.Count);
            Assert.AreEqual("stop", chatCompletion.Choices[0].FinishReason);
            Assert.AreEqual(FinishReason.Stopped, chatCompletion.Choices[0].FinishReason);
            Assert.AreEqual(0, chatCompletion.Choices[0].Index);
            Assert.AreEqual("There are 5,280 feet in a mile.", chatCompletion.Choices[0].Message.Content);
            Assert.AreEqual("assistant", chatCompletion.Choices[0].Message.Role);
            Assert.AreEqual(ChatRole.Assistant, chatCompletion.Choices[0].Message.Role);
        }

        /// <summary>
        /// Test serialization (C# class object sto JSON strings).
        /// </summary>
        [TestMethod]
        public void TestCreatingJsonRequestBody()
        {
            ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, "user message"),
                    new ChatMessage(ChatRole.Assistant, "assistant message"),
                    new ChatMessage(ChatRole.System, "system message"),
                },
                stream: true);

            string jsonString = chatCompletionOptions.SerializeToJson();

            Assert.AreEqual("{\"messages\":[{\"role\":\"user\",\"content\":\"user message\"},{\"role\":\"assistant\",\"content\":\"assistant message\"},{\"role\":\"system\",\"content\":\"system message\"}],\"stream\":true}", jsonString);
        }
    }
}