using Nandaka.Core.Session;

namespace Nandaka.Tests.Common
{
    public interface IProtocolErrorMessageFactory
    {
        ErrorMessage Create(int deviceAddress, MessageType messageType, int errorCode);
    }
}