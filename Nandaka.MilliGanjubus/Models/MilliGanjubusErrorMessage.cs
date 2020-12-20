using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Utils;

namespace Nandaka.MilliGanjubus.Models
{
    public sealed class MilliGanjubusErrorMessage : ProtocolSpecifiedErrorMessage
    {
        private MilliGanjubusErrorMessage(MilliGanjubusErrorType errorType) : base((int)errorType) { }

        public static ErrorMessage Create(int slaveDeviceAddress, MessageType messageType, MilliGanjubusErrorType errorType)
        {
            var mgError = new MilliGanjubusErrorMessage(errorType);
            
            ErrorType? commonErrorType = errorType.Convert();
            if (commonErrorType.HasValue)
                return ErrorMessage.CreateCommon(slaveDeviceAddress, messageType, commonErrorType.Value, mgError);
            
            return ErrorMessage.CreateFromProtocol(slaveDeviceAddress, messageType, new MilliGanjubusErrorMessage(errorType));
        }
    }
}
