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
            string jsonString = "{\"message\":{\"content\":\"some text\",\"role\":\"assistant\",\"session_state\":null},\"context\":null,\"session_state\":null,\"finish_reason\":\"stop\"}";
            using JsonDocument document = JsonDocument.Parse(jsonString);
            ChatCompletion chatCompletion = ChatCompletion.DeserializeChatCompletion(document.RootElement);

            Assert.AreEqual("stop", chatCompletion.FinishReason);
            Assert.AreEqual(ChatFinishReason.Stopped, chatCompletion.FinishReason);
            Assert.AreEqual("some text", chatCompletion.Message.Content);
            Assert.AreEqual("assistant", chatCompletion.Message.Role);
            Assert.AreEqual(ChatRole.Assistant, chatCompletion.Message.Role);
            Assert.IsNull(chatCompletion.Context);
            Assert.IsNull(chatCompletion.SessionState);
        }

        /// <summary>
        /// Test deserialization StreamingChatUpdate (JSON string to C# class objects), with some typical (non null) values.
        /// </summary>
        [TestMethod]
        public void TestChatCompletionUpdateDeserialization()
        {
            string jsonString = "{\"delta\":{\"content\":\"this is the update\",\"role\":\"assistant\",\"session_state\":null},\"context\":null,\"session_state\":null,\"finish_reason\":\"stop\"}";
            using JsonDocument document = JsonDocument.Parse(jsonString);
            ChatCompletionUpdate update = ChatCompletionUpdate.DeserializeStreamingChatUpdate(document.RootElement);

            Assert.AreEqual("stop", update.FinishReason);
            Assert.AreEqual(ChatFinishReason.Stopped, update.FinishReason);
            Assert.AreEqual("this is the update", update.ContentUpdate);
            Assert.AreEqual("assistant", update.Role);
            Assert.AreEqual(ChatRole.Assistant, update.Role);
            Assert.IsNull(update.Context);
            Assert.IsNull(update.SessionState);
        }

        /// <summary>
        /// Test deserialization StreamingChatUpdate (JSON string to C# class objects), with mostly null or empty values.
        /// </summary>
        [TestMethod]
        public void TestChatCompletionUpdateDeserializationWithEmptyAndNullElements()
        {
            string jsonString = "{\"delta\":{\"content\":\"\",\"role\":null,\"session_state\":null},\"context\":null,\"session_state\":null,\"finish_reason\":null}";
            using JsonDocument document = JsonDocument.Parse(jsonString);
            ChatCompletionUpdate update = ChatCompletionUpdate.DeserializeStreamingChatUpdate(document.RootElement);

            Assert.AreEqual(string.Empty, update.ContentUpdate);
            Assert.IsNull(update.FinishReason);
            Assert.IsNull(update.Role);
            Assert.IsNull(update.Context);
            Assert.IsNull(update.SessionState);
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