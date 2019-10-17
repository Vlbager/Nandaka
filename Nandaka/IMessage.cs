using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IMessage
    {
        int DeviceAddress { get; }
        void AddRegister(IRegister register);
        void RemoveRegister(IRegister register);
        IEnumerable<IRegister> Registers { get; }
        int RegistersCount { get; }
        MessageType MessageType { get; }
        int ErrorCode { get; }
    }
}
