using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Utils;

namespace Nandaka.MilliGanjubus.Models
{
    public sealed class MgErrorMessage : ProtocolSpecifiedErrorMessage
    {
        private MgErrorMessage(MgErrorType errorType) : base((int)errorType) { }

        public static ErrorMessage Create(int slaveDeviceAddress, MessageType messageType, MgErrorType errorType)
        {
            var mgError = new MgErrorMessage(errorType);
            
            ErrorType? commonErrorType = errorType.Convert();
            if (commonErrorType.HasValue)
                return ErrorMessage.CreateCommon(slaveDeviceAddress, messageType, commonErrorType.Value, mgError);
            
            return ErrorMessage.CreateFromProtocol(slaveDeviceAddress, messageType, new MgErrorMessage(errorType));
        }

        public override string ToLogLine()
        {
            return $"Milliganjubus error code: '{ErrorCode}'; error type: {(MgErrorType)ErrorCode}";
        }
    }
}
