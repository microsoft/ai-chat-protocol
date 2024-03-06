// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents the options used when constructing a <see cref="ChatProtocolClient"/>.
    /// </summary>
    public class ChatProtocolClientOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatProtocolClientOptions"/> class.
        /// </summary>
        /// <param name="httpRequestHeaders">Optional HTTP request headers and their values.</param>
        /// <param name="loggerFactory">Optional logger.
        /// <see href="https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors">Logging guidance for .NET library authors</see>.
        /// </param>
        /// <exception cref="ArgumentException"><paramref name="httpRequestHeaders"/> is a non-empty dictionary with null or empty keys or values.</exception>
        public ChatProtocolClientOptions(Dictionary<string, string>? httpRequestHeaders, ILoggerFactory? loggerFactory)
        {
            if (httpRequestHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in httpRequestHeaders)
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

            this.HttpRequestHeaders = httpRequestHeaders;
            this.LoggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets the optional HTTP request headers and their values.
        /// </summary>
        public Dictionary<string, string>? HttpRequestHeaders { get; internal set; }

        /// <summary>
        /// Gets the optional Logger factor. <see href="https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors">Logging guidance for .NET library authors</see>.
        /// </summary>
        public ILoggerFactory? LoggerFactory { get; internal set; }
    }
}
