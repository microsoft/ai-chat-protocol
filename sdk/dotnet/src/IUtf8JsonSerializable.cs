// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.Text.Json;

    /// <summary>
    /// Interface for objects that can be serialized to UTF-8 JSON.
    /// </summary>
    internal interface IUtf8JsonSerializable
    {
        /// <summary>
        /// Write the content of this object to a JSON Writer.
        /// </summary>
        /// <param name="writer"> The JSON writer to be updated.</param>
        void Write(Utf8JsonWriter writer);
    }
}
