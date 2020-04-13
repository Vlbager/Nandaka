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

        private readonly Int16RegisterGroup _shortGroup;
        private readonly UInt8RegisterGroup[] _byteCollection;
        
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

        public short TestShort
        {
            get => _shortGroup.Value;
            set => _shortGroup.Value = value;
        }

        public IEnumerable<byte> ByteCollection => _byteCollection.Select(group => group.Value);

        public override IReadOnlyCollection<IRegisterGroup> RegisterGroups { get; }
        public override string Name => "Test Device";

        private ConcreteDevice(int address, DeviceState state, IRegistersUpdatePolicy updatePolicy, ISpecificMessageHandler specificMessageHandler) 
            : base(address, state, updatePolicy, specificMessageHandler) { }

        private ConcreteDevice(int address, IRegistersUpdatePolicy updatePolicy, DeviceState state) 
            : base(address, updatePolicy, state)
        {
            // todo: hide creation logic in group creation methods.
            // Random unique addresses and values.
            _byteGroup = new UInt8RegisterGroup(Register<byte>.CreateByte(1, RegisterType.WriteOnly));
            
            _intGroup = new Int32RegisterGroup(Enumerable.Range(2, 4)
                .Select(register => Register<byte>.CreateByte(register, RegisterType.WriteOnly))
                .ToArray());
            
            _shortGroup = new Int16RegisterGroup(Enumerable.Range(7, 2)
                .Select(register => Register<byte>.CreateByte(register, RegisterType.ReadOnly))
                .ToArray());
            
            _byteCollection = Enumerable.Range(13, 34)
                .Select(registerAddress => new UInt8RegisterGroup(Register<byte>.CreateByte(registerAddress, RegisterType.ReadOnly)))
                .ToArray();
            
            // very dirty.
            RegisterGroups = new IRegisterGroup[]{_byteGroup, _intGroup, _shortGroup}.Concat(_byteCollection).ToArray();
        }

        public static ConcreteDevice Create()
        {
            return new ConcreteDevice(3, new WriteFirstUpdatePolicy(), DeviceState.Connected);
        }
    }
}