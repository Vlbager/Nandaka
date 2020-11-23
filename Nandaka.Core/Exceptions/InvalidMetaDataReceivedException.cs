using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidMetaDataReceivedException : InvalidMessageReceivedException
    {
        public InvalidMetaDataReceivedException() : base(ErrorType.InvalidMetaData)
        {
        }

        public InvalidMetaDataReceivedException(string message) : base(message, ErrorType.InvalidMetaData)
        {
        }

        public InvalidMetaDataReceivedException(string message, Exception innerException) : base(message, innerException, ErrorType.InvalidMetaData)
        {
        }

        protected InvalidMetaDataReceivedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}