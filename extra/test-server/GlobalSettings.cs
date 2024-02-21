using System.ComponentModel;

namespace Azure.AI.Chat.SampleService;

public enum BackendChatService
{
    AzureOpenAI,
    MaaS,
    Llama2MaaP,
    EndValue // This must always be at the end
}

public static class GlobalSettings
{
    public static BackendChatService backendChatService { get; set; }
}
