using System;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Describes the result of a Get operation against a <see cref="IKsStoreEntry{TKey}"/>.
    /// </summary>
    public readonly struct KsStoreGetResult
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="forwardUri"></param>
        public KsStoreGetResult(byte[] data, Uri forwardUri)
        {
            Data = data;
            ForwardUri = forwardUri;
        }

        /// <summary>
        /// Gets the data associated with the entry.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the forward URI associated with the entry.
        /// </summary>
        public Uri ForwardUri { get; }

    }

}
