using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class NandakaBaseException : ApplicationException
    {
        public NandakaBaseException()
        {
        }

        public NandakaBaseException(string message) : base(message)
        {
        }

        public NandakaBaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NandakaBaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}