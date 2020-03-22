using System.Collections.Generic;
using System.ComponentModel;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Device
{
    public class SlaveDevice : IDevice
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public IReadOnlyCollection<IRegisterGroup> RegisterGroups { get; }
        public RegisterTable Table { get; }
        public IProtocol Protocol { get; }
        public int Address { get; }
    }
}
