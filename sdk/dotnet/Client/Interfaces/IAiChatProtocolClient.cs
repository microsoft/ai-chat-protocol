namespace Client.Interfaces;

using Backend.Model;

using System.Threading;

public interface IAiChatProtocolClient
{
    Task<AIChatCompletion?> CompleteAsync(AIChatRequest iChatRequest, CancellationToken cancellationToken = default);
}