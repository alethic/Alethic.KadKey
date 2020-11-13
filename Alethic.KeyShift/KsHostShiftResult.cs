using System;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Describes the result of a Shift operation against a <see cref="IKsHostClient{TKey}"/>.
    /// </summary>
    public readonly struct KsHostShiftResult
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="forwardUri"></param>
        public KsHostShiftResult(Uri forwardUri)
        {
            ForwardUri = forwardUri;
        }

        /// <summary>
        /// Gets the forward URI associated with the entry.
        /// </summary>
        public Uri ForwardUri { get; }

    }

}
