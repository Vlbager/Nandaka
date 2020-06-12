using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class DeviceNotRespondException : NandakaException
    {
        public int DeviceAddress { get; }
        
        public DeviceNotRespondException(int deviceAddress)
        {
            DeviceAddress = deviceAddress;
        }

        public DeviceNotRespondException(string message, int deviceAddress) : base(message)
        {
            DeviceAddress = deviceAddress;
        }

        public DeviceNotRespondException(string message, Exception innerException, int deviceAddress) : base(message, innerException)
        {
            DeviceAddress = deviceAddress;
        }

        protected DeviceNotRespondException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}