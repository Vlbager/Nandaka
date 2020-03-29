namespace Nandaka.Core.Session
{
    public class CommonErrorMessage : IErrorMessage
    {
        public int SlaveDeviceAddress { get; }
        public MessageType Type { get; }
        public ErrorType ErrorType { get; }

        public CommonErrorMessage(int slaveDeviceAddress, MessageType type, ErrorType errorType)
        {
            SlaveDeviceAddress = slaveDeviceAddress;
            Type = type;
            ErrorType = errorType;
        }
    }
}
