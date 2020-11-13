using System;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Describes the result of a Freeze operation against a <see cref="IKsStoreEntry{TKey}"/>.
    /// </summary>
    public readonly struct KsStoreFreezeResult
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="forwardUri"></param>
        public KsStoreFreezeResult(string token, Uri forwardUri)
        {
            Token = token;
            ForwardUri = forwardUri;
        }

        /// <summary>
        /// Gets the token associated with the entry.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Gets the forward URI associated with the entry.
        /// </summary>
        public Uri ForwardUri { get; }

    }

}
