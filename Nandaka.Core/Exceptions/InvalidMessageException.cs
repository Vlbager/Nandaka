using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidMessageException : NandakaException
    {
        public ErrorType ErrorType { get; }
        
        public InvalidMessageException(ErrorType errorType)
        {
            ErrorType = errorType;
        }

        public InvalidMessageException(string message, ErrorType errorType) : base(message)
        {
            ErrorType = errorType;
        }

        public InvalidMessageException(string message, Exception innerException, ErrorType errorType) : base(message, innerException)
        {
            ErrorType = errorType;
        }

        protected InvalidMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}