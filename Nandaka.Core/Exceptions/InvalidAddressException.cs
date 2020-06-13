using System;
using System.Runtime.Serialization;
using Nandaka.Core.Session;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class InvalidAddressException : InvalidMessageException
    {
        public int ReceivedAddress { get; }
        
        public InvalidAddressException(int receivedAddress) : base(ErrorType.InvalidAddress)
        {
            ReceivedAddress = receivedAddress;
        }

        public InvalidAddressException(string message, int receivedAddress) : base(message, ErrorType.InvalidAddress)
        {
            ReceivedAddress = receivedAddress;
        }

        public InvalidAddressException(string message, Exception innerException, int receivedAddress) : base(message, innerException, ErrorType.InvalidAddress)
        {
            ReceivedAddress = receivedAddress;
        }

        protected InvalidAddressException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}