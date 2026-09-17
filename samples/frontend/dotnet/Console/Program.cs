using Client.Extensions;

using Console;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder b = new(args);
b.Configuration
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables("AICHATPROTOCOL_")
    .AddCommandLine(args);

b.Services
    .AddHostedService<Repl>()
    .AddAiChatProtocolClient();

CancellationTokenSource cancellationTokenSource = new();
System.Console.CancelKeyPress += (_, args) =>
{
    cancellationTokenSource.Cancel();

    args.Cancel = true;
};

await b.Build().RunAsync(cancellationTokenSource.Token);
