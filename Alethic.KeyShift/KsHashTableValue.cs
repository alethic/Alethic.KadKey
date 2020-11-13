using System;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Describes a hashtable value.
    /// </summary>
    public readonly struct KsHashTableValue
    {

        /// <summary>
        /// Initi
        /// </summary>
        /// <param name="data"></param>
        /// <param name="version"></param>
        /// <param name="ttl"></param>
        public KsHashTableValue(byte[] data, ulong version, TimeSpan ttl)
        {
            Data = data;
            Version = version;
            TimeToLive = ttl;
        }

        /// <summary>
        /// Gets the data of the value.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the current version of the value.
        /// </summary>
        public ulong Version { get; }

        /// <summary>
        /// Gets the time at which the value can be removed.
        /// </summary>
        public TimeSpan TimeToLive { get; }

    }

}
