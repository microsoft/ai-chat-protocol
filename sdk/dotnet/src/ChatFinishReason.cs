// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.ComponentModel;

    /// <summary> Representation of the reason why a chat session has finished processing. </summary>
    public readonly partial struct ChatFinishReason : IEquatable<ChatFinishReason>
    {
        private const string StoppedValue = "stop";
        private const string TokenLimitReachedValue = "length";

        private readonly string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatFinishReason"/> struct.
        /// </summary>
        /// <param name="value"> The finish reason as a string. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is null. </exception>
        public ChatFinishReason(string? value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary> Gets the finish reason that represents a completion ended normally. </summary>
        public static ChatFinishReason Stopped { get; } = new ChatFinishReason(StoppedValue);

        /// <summary> Gets the finish reason that represents a completion exhausted available tokens before generation could complete. </summary>
        public static ChatFinishReason TokenLimitReached { get; } = new ChatFinishReason(TokenLimitReachedValue);

        /// <summary> Converts a string to a <see cref="ChatFinishReason"/>. </summary>
        /// <param name="value"> The finish reason as a string. </param>
        public static implicit operator ChatFinishReason(string value) => new ChatFinishReason(value);

        /// <summary> Determines if two <see cref="ChatFinishReason"/> values are the same. </summary>
        /// <param name="left"> The first finish reason to compare. </param>
        /// <param name="right"> The second finish reason to compare. </param>
        public static bool operator ==(ChatFinishReason left, ChatFinishReason right) => left.Equals(right);

        /// <summary> Determines if two <see cref="ChatFinishReason"/> values are not the same. </summary>
        /// <param name="left"> The first finish reason to compare. </param>
        /// <param name="right"> The second finish reason to compare. </param>
        public static bool operator !=(ChatFinishReason left, ChatFinishReason right) => !left.Equals(right);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj) => obj is ChatFinishReason other && this.Equals(other);

        /// <inheritdoc />
        public bool Equals(ChatFinishReason other) => string.Equals(this.value, other.value, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => this.value?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override string ToString() => this.value;
    }
}
