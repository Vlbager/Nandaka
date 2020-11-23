using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class TooMuchDataRequestedException : InvalidMessageReceivedException
    {
        public TooMuchDataRequestedException() : base(ErrorType.TooMuchDataRequested)
        {
        }

        public TooMuchDataRequestedException(string message) : base(message, ErrorType.TooMuchDataRequested)
        {
        }

        public TooMuchDataRequestedException(string message, Exception innerException) : base(message, innerException, ErrorType.TooMuchDataRequested)
        {
        }

        protected TooMuchDataRequestedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}