// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.ClientModel.Primitives;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents the options used when constructing a <see cref="ChatProtocolClient"/>.
    /// </summary>
    public class ChatProtocolClientOptions : ClientPipelineOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatProtocolClientOptions"/> class.
        /// </summary>
        /// <param name="loggerFactory">Optional logger.
        /// <see href="https://learn.microsoft.com/en-us/dotnet/core/extensions/logging">Logging in C# and .NET</see>.
        /// </param>
        public ChatProtocolClientOptions(ILoggerFactory? loggerFactory = null)
        {
            this.LoggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets or sets the optional Logger factor. <see href="https://learn.microsoft.com/en-us/dotnet/core/extensions/logging">Logging in C# and .NET</see>.
        /// </summary>
        public ILoggerFactory? LoggerFactory { get; set; }
    }
}
