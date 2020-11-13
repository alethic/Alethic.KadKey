using System;

namespace Alethic.KeyShift
{

    /// <summary>
    /// Represents errors that occur within KeyShift.
    /// </summary>
    public class KsException : Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public KsException()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public KsException(string message) :
            base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public KsException(string message, Exception innerException) :
            base(message, innerException)
        {

        }

    }

}