using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class DeviceNotFoundException : NandakaBaseException
    {
        public DeviceNotFoundException()
        {
        }

        public DeviceNotFoundException(string message) : base(message)
        {
        }

        public DeviceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeviceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}