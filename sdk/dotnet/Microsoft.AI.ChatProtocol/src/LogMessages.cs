using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AI.ChatProtocol
{
    internal static partial class LogMessages
    {
        [LoggerMessage(
            Message = "Request = {request}\n\t  Request body = {body}",
            Level = LogLevel.Information)]
        internal static partial void LogHttpRequest(
            this ILogger logger,
            HttpRequestMessage? request,
            String body);

        [LoggerMessage(
            Message = "Response = {response}\n\t  Response body = {body}",
            Level = LogLevel.Information)]
        internal static partial void LogHttpResponse(
            this ILogger logger,
            HttpResponseMessage response,
            String body);
    }
}