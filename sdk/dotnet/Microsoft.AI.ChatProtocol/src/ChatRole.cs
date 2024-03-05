// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System.ComponentModel;

    /// <summary> A representation of the intended purpose of a message. </summary>
    public readonly struct ChatRole : IEquatable<ChatRole>
    {
        private const string UserValue = "user";
        private const string SystemValue = "system";
        private const string AssistantValue = "assistant";

        private readonly string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRole"/> struct.
        /// </summary>
        /// <param name="value"> The role as a string. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is null. </exception>
        public ChatRole(string value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary> Gets the role that provides input to the completion. </summary>
        public static ChatRole User { get; } = new ChatRole(UserValue);

        /// <summary> Gets the role that instructs or configures the behavior of the assistant. </summary>
        public static ChatRole System { get; } = new ChatRole(SystemValue);

        /// <summary> Gets the role that provides responses to the system-instructed, user-prompted input. </summary>
        public static ChatRole Assistant { get; } = new ChatRole(AssistantValue);

        /// <summary> Converts a string to a <see cref="ChatRole"/>. </summary>
        /// <param name="value"> The role as a string. </param>
        public static implicit operator ChatRole(string value) => new ChatRole(value);

        /// <summary> Determines if two <see cref="ChatRole"/> values are the same. </summary>
        /// <param name="left"> The first role to compare. </param>
        /// <param name="right"> The second role to compare. </param>
        public static bool operator ==(ChatRole left, ChatRole right) => left.Equals(right);

        /// <summary> Determines if two <see cref="ChatRole"/> values are not the same. </summary>
        /// <param name="left"> The first role to compare. </param>
        /// <param name="right"> The second role to compare. </param>
        public static bool operator !=(ChatRole left, ChatRole right) => !left.Equals(right);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is ChatRole other && this.Equals(other);

        /// <inheritdoc />
        public bool Equals(ChatRole other) => string.Equals(this.value, other.value, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => this.value?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override string ToString() => this.value;
    }
}
