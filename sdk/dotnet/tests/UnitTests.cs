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
        /// Test deserialization  ChatCompletion (JSON string to C# class objects).
        /// </summary>
        [TestMethod]
        public void TestChatCompletionDeserialization()
        {
            string jsonString = "{\"choices\":[{\"finish_reason\":\"stop\",\"index\":0,\"message\":{\"content\":\"There are 5,280 feet in a mile.\",\"role\":\"assistant\"},\"session_state\":\"44967C86-6D52-4F47-B418-82A31C520F3C\",\"context\":{\"overrides\":{\"temperature\":0.5,\"top\":1,\"retrieval_mode\":\"text\"}}}],\"created\":1795472,\"id\":\"298157ba-853f-4352-8551-dcbdcfb655f3\",\"object\":\"chat.completion\",\"usage\":{\"completion_tokens\":15,\"prompt_tokens\":16,\"total_tokens\":31}}";
            using JsonDocument document = JsonDocument.Parse(jsonString);
            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            Assert.AreEqual(1, chatCompletion.Choices.Count);
            Assert.AreEqual("stop", chatCompletion.Choices[0].FinishReason);
            Assert.AreEqual(ChatFinishReason.Stopped, chatCompletion.Choices[0].FinishReason);
            Assert.AreEqual("\"44967C86-6D52-4F47-B418-82A31C520F3C\"", chatCompletion.Choices[0].SessionState);
            Assert.AreEqual("{\"overrides\":{\"temperature\":0.5,\"top\":1,\"retrieval_mode\":\"text\"}}", chatCompletion.Choices[0].Context);
            Assert.AreEqual(0, chatCompletion.Choices[0].Index);
            Assert.AreEqual("There are 5,280 feet in a mile.", chatCompletion.Choices[0].Message.Content);
            Assert.AreEqual("assistant", chatCompletion.Choices[0].Message.Role);
            Assert.AreEqual(ChatRole.Assistant, chatCompletion.Choices[0].Message.Role);
        }

        /// <summary>
        /// Test deserialization StreamingChatUpdate (JSON string to C# class objects), with some typical (non null) values.
        /// </summary>
        [TestMethod]
        public void TestStreamingChatUpdateDeserialization()
        {
            string jsonString = "{\"choices\":[{\"index\":0,\"delta\":{\"content\":\"this is the update\",\"role\":\"assistant\"},\"context\":null,\"session_state\":null,\"finish_reason\":\"stop\"}]}";
            using JsonDocument document = JsonDocument.Parse(jsonString);
            StreamingChatUpdate update = StreamingChatUpdate.DeserializeStreamingChatUpdate(document.RootElement);

            Assert.AreEqual(0, update.ChoiceIndex);
            Assert.AreEqual("stop", update.FinishReason);
            Assert.AreEqual(ChatFinishReason.Stopped, update.FinishReason);
            Assert.AreEqual("this is the update", update.ContentUpdate);
            Assert.AreEqual("assistant", update.Role);
            Assert.AreEqual(ChatRole.Assistant, update.Role);
        }

        /// <summary>
        /// Test deserialization StreamingChatUpdate (JSON string to C# class objects), with mostly null or empty values.
        /// </summary>
        [TestMethod]
        public void TestStreamingChatUpdateDeserializationWithEmptyAndNullElements()
        {
            string jsonString = "{\"choices\":[{\"index\":0,\"delta\":{\"content\":\"\",\"role\":null},\"context\":null,\"session_state\":null,\"finish_reason\":null}]}";
            using JsonDocument document = JsonDocument.Parse(jsonString);
            StreamingChatUpdate update = StreamingChatUpdate.DeserializeStreamingChatUpdate(document.RootElement);

            Assert.AreEqual(0, update.ChoiceIndex);
            Assert.AreEqual(null, update.FinishReason);
            Assert.AreEqual(string.Empty, update.ContentUpdate);
            Assert.AreEqual(null, update.Role);
            Assert.AreEqual(null, update.Role);
        }

        /// <summary>
        /// Test serialization (C# class object into JSON string).
        /// </summary>
        [TestMethod]
        public void TestCreatingJsonRequestBody()
        {
            string sessionState = "\"44967C86-6D52-4F47-B418-82A31C520F3C\"";
            string context = "{\"overrides\":{\"temperature\":0.5,\"top\":1,\"retrieval_mode\":\"text\"}}";

            ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, "user message"),
                    new ChatMessage(ChatRole.Assistant, "assistant message"),
                    new ChatMessage(ChatRole.System, "system message"),
                },
                sessionState: sessionState,
                context: context);

            chatCompletionOptions.Stream = true;

            string jsonString = chatCompletionOptions.SerializeToJson();

            Assert.AreEqual("{\"messages\":[{\"role\":\"user\",\"content\":\"user message\"},{\"role\":\"assistant\",\"content\":\"assistant message\"},{\"role\":\"system\",\"content\":\"system message\"}],\"stream\":true,\"session_state\":\"44967C86-6D52-4F47-B418-82A31C520F3C\",\"context\":{\"overrides\":{\"temperature\":0.5,\"top\":1,\"retrieval_mode\":\"text\"}}}", jsonString);
        }
    }
}