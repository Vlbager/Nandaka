using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Example
{
    public sealed class ConcreteDevice : NandakaDevice
    {
        private readonly UInt8RegisterGroup _byteGroup;
        private readonly Int32RegisterGroup _intGroup;
        
        public byte TestByte
        {
            get => _byteGroup.Value;
            set => _byteGroup.Value = value;
        }

        public int TestInt
        {
            get => _intGroup.Value;
            set => _intGroup.Value = value;
        }
        
        public override IReadOnlyCollection<IRegisterGroup> RegisterGroups { get; }
        public override string Name => "Test Device";

        private ConcreteDevice(int address, DeviceState state, IRegistersUpdatePolicy updatePolicy, ISpecificMessageHandler specificMessageHandler) 
            : base(address, state, updatePolicy, specificMessageHandler) { }

        private ConcreteDevice(int address, IRegistersUpdatePolicy updatePolicy, DeviceState state) 
            : base(address, updatePolicy, state)
        {
            _byteGroup = new UInt8RegisterGroup(Register<byte>.CreateByte(1, RegisterType.WriteOnly));
            _intGroup = new Int32RegisterGroup(Enumerable.Range(3, 4)
                .Select(register => Register<byte>.
                    CreateByte(register, RegisterType.WriteOnly))
                .ToArray());
            RegisterGroups = new IRegisterGroup[]{_byteGroup, _intGroup};
        }

        public static ConcreteDevice Create()
        {
            return new ConcreteDevice(3, new WriteFirstUpdatePolicy(), DeviceState.Connected);
        }
    }
}