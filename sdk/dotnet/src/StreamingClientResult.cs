// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Microsoft.AI.ChatProtocol
{
    using System;
    using System.ClientModel;
    using System.ClientModel.Primitives;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Represents an operation response with streaming content that can be deserialized and enumerated while the response
    /// is still being received.
    /// </summary>
    /// <typeparam name="T"> The data type representative of distinct, streamable items. </typeparam>
    public class StreamingClientResult<T> : IDisposable, IAsyncEnumerable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingClientResult{T}"/> class.
        /// </summary>
        /// <param name="rawResult">The underlying HTTP response.</param>
        /// <param name="asyncEnumerableProcessor">The function that will resolve the provided response into an IAsyncEnumerable.</param>
        private StreamingClientResult(
            ClientResult rawResult,
            Func<ClientResult, IAsyncEnumerable<T>> asyncEnumerableProcessor)
        {
            this.RawResult = rawResult;
            this.AsyncEnumerableSource = asyncEnumerableProcessor.Invoke(rawResult);
        }

        private ClientResult RawResult { get; }

        private IAsyncEnumerable<T> AsyncEnumerableSource { get; }

        private bool DisposedValue { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="PipelineResponse"/> instance that this <see cref="StreamingClientResult{T}"/> may enumerate
        /// over.
        /// </summary>
        /// <returns> The <see cref="PipelineResponse"/> instance attached to this <see cref="StreamingClientResult{T}"/>. </returns>
        public PipelineResponse GetRawResponse() => this.RawResult.GetRawResponse();

        /// <summary>
        /// Gets the asynchronously enumerable collection of distinct, streamable items in the response.
        /// </summary>
        /// <remarks>
        /// <para> The return value of this method may be used with the "await foreach" statement. </para>
        /// <para>
        /// As <see cref="StreamingClientResult{T}"/> explicitly implements <see cref="IAsyncEnumerable{T}"/>, callers may
        /// enumerate a <see cref="StreamingClientResult{T}"/> instance directly instead of calling this method.
        /// </para>
        /// </remarks>
        /// <returns> An asynchronous enumerable collection of distinct, streamable items in the response. </returns>
        public IAsyncEnumerable<T> EnumerateValues() => this;

        /// <inheritdoc/>
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
            => this.AsyncEnumerableSource.GetAsyncEnumerator(cancellationToken);

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new instance of <see cref="StreamingClientResult{T}"/> using the provided underlying HTTP response. The
        /// provided function will be used to resolve the response into an asynchronous enumeration of streamed response
        /// items.
        /// </summary>
        /// <param name="result">The HTTP response.</param>
        /// <param name="asyncEnumerableProcessor">
        /// The function that will resolve the provided response into an IAsyncEnumerable.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="StreamingClientResult{T}"/> that will be capable of asynchronous enumeration of
        /// <typeparamref name="T"/> items from the HTTP response.
        /// </returns>
        internal static StreamingClientResult<T> CreateFromResponse(
            ClientResult result,
            Func<ClientResult, IAsyncEnumerable<T>> asyncEnumerableProcessor)
        {
            return new (result, asyncEnumerableProcessor);
        }

        /// <summary>
        /// Disposes of the resources used by this <see cref="StreamingClientResult{T}"/> instance.
        /// </summary>
        /// <param name="disposing">Whether or not this method is being called from the public <see cref="Dispose()"/> method.</param>
        /// <remarks>
        /// This method is not intended to be called directly. Call the public <see cref="Dispose()"/> method instead.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.DisposedValue)
            {
                if (disposing)
                {
                    this.RawResult?.GetRawResponse()?.Dispose();
                }

                this.DisposedValue = true;
            }
        }
    }
}