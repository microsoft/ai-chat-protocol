// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol.Samples
{
    using System.Diagnostics;

    /// <summary>
    /// Chat Protocol SDK samples.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main menu for Chat Protocol SDK samples.
        /// </summary>
        public static void Main()
        {
            ConsoleKeyInfo x;

            do
            {
                Console.WriteLine(string.Empty);
                Console.WriteLine(" Microsoft AI Chat Protocol SDK Samples");
                Console.WriteLine(string.Empty);
                Console.WriteLine(" Please choose one of the following samples:");
                Console.WriteLine(string.Empty);
                Console.WriteLine(" 1. Non-streaming, sync.");
                Console.WriteLine(" 2. Non-streaming, async.");
                Console.WriteLine(" 3. Streaming, sync");
                Console.WriteLine(" 4. Streaming, async.");
                Console.WriteLine(string.Empty);
                Console.Write(" Your choice (or 0 to exit): ");

                x = Console.ReadKey();
                Console.WriteLine(string.Empty);

                switch (x.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        NonStreamingSyncSample();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        NonStreamingAsyncSample();
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        StreamingSyncSample();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        StreamingAsyncSample();
                        break;
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        Console.WriteLine("\n Exiting...");
                        break;
                    default:
                        Console.WriteLine("\n Invalid input, choose again.");
                        break;
                }
            }
            while (x.Key != ConsoleKey.D0);
        }

        /// <summary>
        /// Non-streaming, sync sample.
        /// </summary>
        private static void NonStreamingSyncSample()
        {
/*
            string question = "How many feet are in a mile?";

            string endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT")
                ?? throw new Exception("Missing environment variable");

            ChatProtocolClient client = new ChatProtocolClient(new Uri(endpoint));

            ChatCompletion chatCompletion = client.GetChatCompletion(new ChatCompletionOptions(
                messages: new[]
                {
                    new ChatMessage(ChatRole.User, question),
                }));

            Console.WriteLine($" Question: {question}");
            Console.WriteLine($" Answer: {chatCompletion.Choices[0].Message.Content}");
            Console.WriteLine(" Done!");
*/
        }

        /// <summary>
        /// Non-streaming, async sample.
        /// </summary>
        private static void NonStreamingAsyncSample()
        {
/*
            string question = "How many feet are in a mile?";

            string endpoint = Environment.GetEnvironmentVariable("CHAT_PROTOCOL_ENDPOINT")
                ?? throw new Exception("Missing environment variable");

            ChatProtocolClient client = new ChatProtocolClient(new Uri(endpoint));

            Task<ChatCompletion> task = client.GetChatCompletionAsync(new ChatCompletionOptions(
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
            Console.WriteLine($" Answer: {task.Result.Choices[0].Message.Content}");
            Console.WriteLine(" Done!");
*/
        }

        /// <summary>
        /// Streaming, sync sample.
        /// </summary>
        private static void StreamingSyncSample()
        {
            // TODO
            Console.WriteLine(" Done!");
        }

        /// <summary>
        /// Streaming, async sample.
        /// </summary>
        private static void StreamingAsyncSample()
        {
            // TODO
            Console.WriteLine(" Done!");
        }
    }
}