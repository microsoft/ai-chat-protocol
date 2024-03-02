using Microsoft.Extensions.Logging;

namespace Microsoft.AI.ChatProtocol
{
    public class ChatProtocolClientOptions
    {
        public Dictionary<string, string>? HttpRequestHeaders { get; internal set; }

        public ILoggerFactory? LoggerFactory { get; internal set; }

        // See article "Logging guidance for .NET library authors"
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors
        public ChatProtocolClientOptions(Dictionary<string, string>? httpRequestHeaders, ILoggerFactory? loggerFactory)
        {
            if (httpRequestHeaders != null)
            {
                foreach(KeyValuePair<string, string> header in httpRequestHeaders)
                {
                    if (string.IsNullOrEmpty(header.Key))
                    {
                        throw new ArgumentException("HTTP request header name cannot be null or empty");
                    }
                    if (string.IsNullOrEmpty(header.Value))
                    {
                        throw new ArgumentException("HTTP request header value cannot be null or empty");
                    }
                }
            }

            HttpRequestHeaders = httpRequestHeaders;
            LoggerFactory = loggerFactory;
        }
    }
}
