using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidAddressException : InvalidMessageException
    {
        public InvalidAddressException() : base(ErrorType.InvalidAddress)
        {
        }

        public InvalidAddressException(string message) : base(message, ErrorType.InvalidAddress)
        {
        }

        public InvalidAddressException(string message, Exception innerException) : base(message, innerException, ErrorType.InvalidAddress)
        {
        }

        protected InvalidAddressException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}