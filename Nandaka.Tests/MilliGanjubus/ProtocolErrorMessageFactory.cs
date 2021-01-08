using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Models;
using Nandaka.Tests.Common;

namespace Nandaka.Tests.MilliGanjubus
{
    public class ProtocolErrorMessageFactory : IProtocolErrorMessageFactory
    {
        public ErrorMessage Create(int deviceAddress, MessageType messageType, int errorCode)
        {
            return MgErrorMessage.Create(deviceAddress, messageType, (MgErrorType) errorCode);
        }
    }
}