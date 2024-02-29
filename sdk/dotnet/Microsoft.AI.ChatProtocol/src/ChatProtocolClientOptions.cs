using Microsoft.Extensions.Logging;

// See article "Logging guidance for .NET library authors"
// https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors
namespace Microsoft.AI.ChatProtocol
{
    public class ChatProtocolClientOptions
    {
        public Dictionary<string, string>? HttpHeaders { get; set; }

        public ILoggerFactory LoggerFactory { get; set; }

        public ChatProtocolClientOptions(Dictionary<string, string> httpHeaders, ILoggerFactory loggerFactory)
        {
            HttpHeaders = httpHeaders;
            LoggerFactory = loggerFactory;
        }
    }
}
