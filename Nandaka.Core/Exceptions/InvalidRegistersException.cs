using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidRegistersException : InvalidMessageException
    {
        public InvalidRegistersException() : base(ErrorType.InvalidRegisters)
        {
        }

        public InvalidRegistersException(string message) : base(message, ErrorType.InvalidRegisters)
        {
        }

        public InvalidRegistersException(string message, Exception innerException) : base(message, innerException, ErrorType.InvalidRegisters)
        {
        }

        protected InvalidRegistersException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}