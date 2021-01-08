using Nandaka.Core.Device;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Example.MasterExample
{
    public class TestDeviceCtx : ForeignDeviceCtx
    {
        public Register<int> IntRoRegister { get; }     = CreateRoRegister<int>(0x00);
        public Register<byte> ByteRoRegister { get; }   = CreateRoRegister<byte>(0x04);   
        public Register<short> ShortRoRegister { get; } = CreateRoRegister<short>(0x05);    
        public Register<long> LongRoRegister { get; }   = CreateRoRegister<long>(0x07);  
        public Register<ulong> UlongRoRegister { get; } = CreateRoRegister<ulong>(0x0F);

        public Register<int> IntRwRegister { get; }     = CreateRwRegister<int>(0x80);
        public Register<byte> ByteRwRegister { get; }   = CreateRwRegister<byte>(0x84);
        public Register<short> ShortRwRegister { get; } = CreateRwRegister<short>(0x85);
        public Register<long> LongRwRegister { get; }   = CreateRwRegister<long>(0x87);
        public Register<ulong> UlongRwRegister { get; } = CreateRwRegister<ulong>(0x8F);

        public override string Name => nameof(TestDeviceCtx);

        private TestDeviceCtx(int address, IRegistersUpdatePolicy updatePolicy, DeviceState state) : base(address, updatePolicy, state)
        { }
        
        public static TestDeviceCtx Create()
        {
            return new TestDeviceCtx(0x01, new WriteFirstUpdatePolicy(), DeviceState.Connected);
        }
    }
}