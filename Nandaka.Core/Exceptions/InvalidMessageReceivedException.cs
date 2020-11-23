using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidMessageReceivedException : NandakaException
    {
        public ErrorType ErrorType { get; }
        
        public InvalidMessageReceivedException(ErrorType errorType)
        {
            ErrorType = errorType;
        }

        public InvalidMessageReceivedException(string message, ErrorType errorType) : base(message)
        {
            ErrorType = errorType;
        }

        public InvalidMessageReceivedException(string message, Exception innerException, ErrorType errorType) : base(message, innerException)
        {
            ErrorType = errorType;
        }

        protected InvalidMessageReceivedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}