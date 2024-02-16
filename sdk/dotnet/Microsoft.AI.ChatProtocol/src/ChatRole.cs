// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace Microsoft.AI.ChatProtocol
{
    /// <summary> A representation of the intended purpose of a message. </summary>
    public readonly struct ChatRole : IEquatable<ChatRole>
    {
        private readonly String _value;

        /// <summary> Initializes a new instance of <see cref="ChatRole"/>. </summary>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is null. </exception>
        public ChatRole(String value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private const String UserValue = "user";
        private const String SystemValue = "system";
        private const String AssistantValue = "assistant";

        /// <summary> The role that provides input to the completion. </summary>
        public static ChatRole User { get; } = new ChatRole(UserValue);

        /// <summary> The role that instructs or configures the behavior of the assistant. </summary>
        public static ChatRole System { get; } = new ChatRole(SystemValue);

        /// <summary> The role that provides responses to the system-instructed, user-prompted input. </summary>
        public static ChatRole Assistant { get; } = new ChatRole(AssistantValue);

        /// <summary> Determines if two <see cref="ChatRole"/> values are the same. </summary>
        public static bool operator ==(ChatRole left, ChatRole right) => left.Equals(right);

        /// <summary> Determines if two <see cref="ChatRole"/> values are not the same. </summary>
        public static bool operator !=(ChatRole left, ChatRole right) => !left.Equals(right);

        /// <summary> Converts a string to a <see cref="ChatRole"/>. </summary>
        public static implicit operator ChatRole(String value) => new ChatRole(value);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is ChatRole other && Equals(other);

        /// <inheritdoc />
        public bool Equals(ChatRole other) => String.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override String ToString() => _value;
    }
}
