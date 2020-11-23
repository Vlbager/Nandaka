using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidMessageToComposeException : NandakaBaseException
    {

        public InvalidMessageToComposeException()
        { }

        public InvalidMessageToComposeException(string message) : base(message)
        { }

        public InvalidMessageToComposeException(string message, Exception innerException) : base(message, innerException)
        { }

        protected InvalidMessageToComposeException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}