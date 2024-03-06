﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A partial class to hold logging methods.
    /// </summary>
    internal static partial class LogMessages
    {
        // Message = "Request = {request}\n\t  Request body = {body}",
        [LoggerMessage(
            Message = "Request = {request}",
            Level = LogLevel.Information)]
        internal static partial void LogHttpRequest(
            this ILogger logger,
            HttpRequestMessage? request /*,
            string body*/);

        [LoggerMessage(
            Message = "Response = {response}\n\t  Response body = {body}",
            Level = LogLevel.Information)]
        internal static partial void LogHttpResponse(
            this ILogger logger,
            HttpResponseMessage response,
            string body);
    }
}