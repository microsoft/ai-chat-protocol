namespace Client.Extensions;

using Client.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Net.Http.Headers;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAiChatProtocolClient(this IServiceCollection services, IAiChatProtocolClient? client = null)
    {
        services.ConfigureOptions<AIChatClientOptions>();

        if (client is not null)
        {
            services.AddSingleton(client);
        }
        else
        {
            services.AddSingleton<IAiChatProtocolClient, AiChatProtocolClient>();
        }

        services.AddHttpClient(AiChatProtocolClient.HttpClientName, (sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<AIChatClientOptions>>().Value;
            client.BaseAddress = config.ChatEndpointUri;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }
}
