using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidRegistersReceivedException : InvalidMessageReceivedException
    {
        public InvalidRegistersReceivedException() : base(ErrorType.InvalidRegisters)
        {
        }

        public InvalidRegistersReceivedException(string message) : base(message, ErrorType.InvalidRegisters)
        {
        }

        public InvalidRegistersReceivedException(string message, Exception innerException) : base(message, innerException, ErrorType.InvalidRegisters)
        {
        }

        protected InvalidRegistersReceivedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}