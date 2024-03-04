// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable

using System;
using System.ComponentModel;

namespace Microsoft.AI.ChatProtocol
{
    /// <summary> Representation of the reason why a chat session has finished processing. </summary>
    public readonly partial struct FinishReason : IEquatable<FinishReason>
    {
        private readonly string _value;

        /// <summary> Initializes a new instance of <see cref="FinishReason"/>. </summary>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is null. </exception>
        public FinishReason(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private const string StoppedValue = "stop";
        private const string TokenLimitReachedValue = "length";

        /// <summary> Completion ended normally. </summary>
        public static FinishReason Stopped { get; } = new FinishReason(StoppedValue);

        /// <summary> The completion exhausted available tokens before generation could complete. </summary>
        public static FinishReason TokenLimitReached { get; } = new FinishReason(TokenLimitReachedValue);

        /// <summary> Determines if two <see cref="FinishReason"/> values are the same. </summary>
        public static bool operator ==(FinishReason left, FinishReason right) => left.Equals(right);

        /// <summary> Determines if two <see cref="FinishReason"/> values are not the same. </summary>
        public static bool operator !=(FinishReason left, FinishReason right) => !left.Equals(right);

        /// <summary> Converts a string to a <see cref="FinishReason"/>. </summary>
        public static implicit operator FinishReason(string value) => new FinishReason(value);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is FinishReason other && Equals(other);
    
        /// <inheritdoc />
        public bool Equals(FinishReason other) => string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override string ToString() => _value;
    }
}