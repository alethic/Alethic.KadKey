using System;

namespace Alethic.KeyShift
{

    public class KsException : Exception
    {

        public KsException()
        {

        }

        public KsException(string message) :
            base(message)
        {

        }

        public KsException(string message, Exception innerException) :
            base(message, innerException)
        {

        }

    }

}