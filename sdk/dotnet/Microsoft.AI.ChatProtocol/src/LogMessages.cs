// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.ClientModel.Primitives;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A partial class to hold logging methods.
    /// <see href = "https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors" > Logging guidance for .NET library authors</see>.
    /// </summary>
    internal static partial class LogMessages
    {
        [LoggerMessage(
            Message = "Request: {request}\n\t  Request body: {body}",
            Level = LogLevel.Information)]
        internal static partial void LogHttpRequest(
            this ILogger logger,
            PipelineRequest request,
            string body);

        /* If/when PipelineResponse is updated to support a ToString() method, use this instead of the below:
        [LoggerMessage(
            Message = "Response: {response}\n\t  Response body = {body}",
            Level = LogLevel.Information)]
        internal static partial void LogHttpResponse(
            this ILogger logger,
            PipelineResponse response,
            string body);
        */

        [LoggerMessage(
            Message = "Response: {response}\n\t  Response body: {body}",
            Level = LogLevel.Information,
            SkipEnabledCheck = true)]
        internal static partial void LogHttpResponse(
            this ILogger logger,
            string response,
            string body);
    }
}