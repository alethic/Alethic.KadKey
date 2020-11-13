using System;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Describes the result of a Get operation against a <see cref="IKsHostClient{TKey}"/>.
    /// </summary>
    public readonly struct KsHostShiftLockResult
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="data"></param>
        /// <param name="forwardUri"></param>
        public KsHostShiftLockResult(string token, byte[] data, Uri forwardUri)
        {
            Token = token;
            Data = data;
            ForwardUri = forwardUri;
        }

        /// <summary>
        /// Gets the token that subsequent requests need to use.
        /// </summary>
        public string Token { get; }

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
