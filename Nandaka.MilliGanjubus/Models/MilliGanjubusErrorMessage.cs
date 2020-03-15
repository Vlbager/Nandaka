using Nandaka.Core.Session;

namespace Nandaka.MilliGanjubus.Models
{
    public class MilliGanjubusErrorMessage : ProtocolSpecifiedErrorMessage
    {
        public MilliGanjubusErrorMessage(int slaveDeviceAddress, MessageType type, MilliGanjubusErrorType errorType) 
            : base(slaveDeviceAddress, type, (int)errorType) { }
    }
}
