using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    // All inheritors of this exceptions may be catched and handled.
    [Serializable]
    public class NandakaException : NandakaBaseException
    {
        public NandakaException()
        {
        }

        public NandakaException(string message) : base(message)
        {
        }

        public NandakaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NandakaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}