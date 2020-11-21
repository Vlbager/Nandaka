namespace Nandaka.Core.Session
{
    public class ProtocolSpecifiedErrorMessage : CommonErrorMessage
    {
        public int ErrorCode { get; }

        public ProtocolSpecifiedErrorMessage(int slaveDeviceAddress, MessageType type, int protocolErrorCode) 
            : base(slaveDeviceAddress, type, ErrorType.InternalProtocolError)
        {
            ErrorCode = protocolErrorCode;
        }
    }
}
