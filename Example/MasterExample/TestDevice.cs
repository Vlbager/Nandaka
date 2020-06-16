using System;
using Nandaka.Core.Attributes;
using Nandaka.Core.Device;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Example.MasterExample
{
    public class TestDevice : NandakaDevice
    {
        public IRoRegister<int> IntRoRegister { get; }     = Int32RegisterGroup.CreateNew(0x00);
        public IRoRegister<byte> ByteRoRegister { get; }   = UInt8RegisterGroup.CreateNew(0x04);   
        public IRoRegister<short> ShortRoRegister { get; } = Int16RegisterGroup.CreateNew(0x05);    
        public IRoRegister<long> LongRoRegister { get; }   = Int64RegisterGroup.CreateNew(0x07);  
        public IRoRegister<ulong> UlongRoRegister { get; } = UInt64RegisterGroup.CreateNew(0x0F);
        
        public IRwRegister<int> IntRwRegister { get; }     = Int32RegisterGroup.CreateNew(0x80);
        [RegisterUpdateInterval(2000)]
        public IRwRegister<byte> ByteRwRegister { get; }   = UInt8RegisterGroup.CreateNew(0x84);
        public IRwRegister<short> ShortRwRegister { get; } = Int16RegisterGroup.CreateNew(0x85);
        public IRwRegister<long> LongRwRegister { get; }   = Int64RegisterGroup.CreateNew(0x87);
        public IRwRegister<ulong> UlongRwRegister { get; } = UInt64RegisterGroup.CreateNew(0x8F);

        public override string Name => nameof(TestDevice);

        private TestDevice(int address, IRegistersUpdatePolicy updatePolicy, DeviceState state) : base(address, updatePolicy, state)
        { }
        
        public static TestDevice Create()
        {
            return new TestDevice(0x01, new WriteFirstUpdatePolicy(), DeviceState.Connected);
        }
    }
}