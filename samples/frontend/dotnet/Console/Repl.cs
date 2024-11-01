namespace Console;

using Backend.Model;

using Client.Interfaces;

using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

internal class Repl(IAiChatProtocolClient _protocolClient) : IHostedService
{
    private readonly TaskCompletionSource _cts = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var cancelTask = Task.Delay(Timeout.Infinite, cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("Enter your message: ");

            string? message = null;
            do
            {
                var readTask = Task.Run(Console.ReadLine, cancellationToken);
                var completedTask = await Task.WhenAny(readTask, cancelTask);
                if (completedTask == cancelTask)
                {
                    // Cancellation was requested
                    break;
                }

                message = await readTask;
            } while (!cancellationToken.IsCancellationRequested && string.IsNullOrWhiteSpace(message));

            if (cancellationToken.IsCancellationRequested)
            {
                // Cancellation was requested
                break;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("Message cannot be empty.");
                continue;
            }

            var request = new AIChatRequest([new AIChatMessage { Content = message }]);
            var response = await _protocolClient.CompleteAsync(request, cancellationToken);

            Console.WriteLine($"Response: {response.Message.Content}");
            Console.WriteLine();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
