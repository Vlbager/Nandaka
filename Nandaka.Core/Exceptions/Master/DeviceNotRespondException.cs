using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class DeviceNotRespondException : NandakaException
    {
        public DeviceNotRespondException()
        {
        }

        public DeviceNotRespondException(string message) : base(message)
        {
        }

        public DeviceNotRespondException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeviceNotRespondException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}