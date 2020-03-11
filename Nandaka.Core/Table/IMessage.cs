using System.Collections.Generic;

namespace Nandaka.Core.Table
{
    public interface IMessage
    {
        int DeviceAddress { get; }
        void AddRegister(IRegisterGroup registerGroup);
        void RemoveRegister(IRegisterGroup registerGroup);
        IEnumerable<IRegisterGroup> Registers { get; }
        int RegistersCount { get; }
        MessageType MessageType { get; }
        int ErrorCode { get; }
    }
}
