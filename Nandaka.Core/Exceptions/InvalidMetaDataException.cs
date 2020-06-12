using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidMetaDataException : InvalidMessageException
    {
        public InvalidMetaDataException() : base(ErrorType.InvalidMetaData)
        {
        }

        public InvalidMetaDataException(string message) : base(message, ErrorType.InvalidMetaData)
        {
        }

        public InvalidMetaDataException(string message, Exception innerException) : base(message, innerException, ErrorType.InvalidMetaData)
        {
        }

        protected InvalidMetaDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}