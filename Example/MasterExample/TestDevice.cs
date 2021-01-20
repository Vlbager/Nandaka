using Nandaka.Core.Device;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Example.MasterExample
{
    public class TestDevice : ForeignDeviceCtx
    {
        public IReadOnlyRegister<int> IntRoRegister { get; }     = CreateRoRegister<int>(0x00);
        public IReadOnlyRegister<byte> ByteRoRegister { get; }   = CreateRoRegister<byte>(0x04);   
        public IReadOnlyRegister<short> ShortRoRegister { get; } = CreateRoRegister<short>(0x05);    
        public IReadOnlyRegister<long> LongRoRegister { get; }   = CreateRoRegister<long>(0x07);  
        public IReadOnlyRegister<ulong> UlongRoRegister { get; } = CreateRoRegister<ulong>(0x0F);

        public IRegister<int> IntRwRegister { get; }             = CreateRwRegister<int>(0x80);
        public IRegister<byte> ByteRwRegister { get; }           = CreateRwRegister<byte>(0x84);
        public IRegister<short> ShortRwRegister { get; }         = CreateRwRegister<short>(0x85);
        public IRegister<long> LongRwRegister { get; }           = CreateRwRegister<long>(0x87);
        public IRegister<ulong> UlongRwRegister { get; }         = CreateRwRegister<ulong>(0x8F);

        public override string Name => nameof(TestDevice);

        private TestDevice(int address, IRegistersUpdatePolicy updatePolicy, DeviceState state) : base(address, updatePolicy, state)
        { }
        
        public static TestDevice Create()
        {
            return new TestDevice(0x01, new WriteFirstUpdatePolicy(), DeviceState.Connected);
        }
    }
}