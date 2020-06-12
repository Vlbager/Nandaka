using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidOperationException : NandakaBaseException
    {
        public InvalidOperationException()
        {
        }

        public InvalidOperationException(string message) : base(message)
        {
        }

        public InvalidOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}