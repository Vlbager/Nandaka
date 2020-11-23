using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidAddressReceivedException : InvalidMessageReceivedException
    {
        public int ReceivedAddress { get; }
        
        public InvalidAddressReceivedException(int receivedAddress) : base(ErrorType.InvalidAddress)
        {
            ReceivedAddress = receivedAddress;
        }

        public InvalidAddressReceivedException(string message, int receivedAddress) : base(message, ErrorType.InvalidAddress)
        {
            ReceivedAddress = receivedAddress;
        }

        public InvalidAddressReceivedException(string message, Exception innerException, int receivedAddress) : base(message, innerException, ErrorType.InvalidAddress)
        {
            ReceivedAddress = receivedAddress;
        }

        protected InvalidAddressReceivedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}