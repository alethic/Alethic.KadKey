using System;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Entry of the service locator within the DHT.
    /// </summary>
    public readonly struct KsHashTableEntry
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="endpoints"></param>
        public KsHashTableEntry(Uri[] endpoints)
        {
            Endpoints = endpoints;
        }

        /// <summary>
        /// Service URIs that are the current primary and secondaries of the key data.
        /// </summary>
        public Uri[] Endpoints { get; }


    }

}
