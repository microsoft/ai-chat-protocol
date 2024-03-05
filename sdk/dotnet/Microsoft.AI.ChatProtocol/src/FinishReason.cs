// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.ComponentModel;

    /// <summary> Representation of the reason why a chat session has finished processing. </summary>
    public readonly partial struct FinishReason : IEquatable<FinishReason>
    {
        private const string StoppedValue = "stop";
        private const string TokenLimitReachedValue = "length";

        private readonly string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinishReason"/> struct.
        /// </summary>
        /// <param name="value"> The finish reason as a string. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is null. </exception>
        public FinishReason(string value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary> Gets the finish reason that represents a completion ended normally. </summary>
        public static FinishReason Stopped { get; } = new FinishReason(StoppedValue);

        /// <summary> Gets the finish reason that represents a completion exhausted available tokens before generation could complete. </summary>
        public static FinishReason TokenLimitReached { get; } = new FinishReason(TokenLimitReachedValue);

        /// <summary> Converts a string to a <see cref="FinishReason"/>. </summary>
        /// <param name="value"> The finish reason as a string. </param>
        public static implicit operator FinishReason(string value) => new FinishReason(value);

        /// <summary> Determines if two <see cref="FinishReason"/> values are the same. </summary>
        /// <param name="left"> The first finish reason to compare. </param>
        /// <param name="right"> The second finish reason to compare. </param>
        public static bool operator ==(FinishReason left, FinishReason right) => left.Equals(right);

        /// <summary> Determines if two <see cref="FinishReason"/> values are not the same. </summary>
        /// <param name="left"> The first finish reason to compare. </param>
        /// <param name="right"> The second finish reason to compare. </param>
        public static bool operator !=(FinishReason left, FinishReason right) => !left.Equals(right);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is FinishReason other && this.Equals(other);

        /// <inheritdoc />
        public bool Equals(FinishReason other) => string.Equals(this.value, other.value, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => this.value?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override string ToString() => this.value;
    }
}
