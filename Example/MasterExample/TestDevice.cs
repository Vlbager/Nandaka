using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Example.MasterExample
{
    public class TestDevice : NandakaDevice
    {
        #region RO
        private readonly IValuedRegister<int> _intRoRegister;
        private readonly IValuedRegister<byte> _byteRoRegister;
        private readonly IValuedRegister<short> _shortRoRegister;
        private readonly IValuedRegister<long> _longRoRegister;
        private readonly IValuedRegister<ulong> _ulongRoRegister;

        public int RoInt => _intRoRegister.Value;
        public byte RoByte => _byteRoRegister.Value;
        public short RoShort => _shortRoRegister.Value;
        public long RoLong => _longRoRegister.Value;
        public ulong RoUlong => _ulongRoRegister.Value;
        #endregion

        #region RW
        private readonly IValuedRegister<int> _intRwRegister;
        private readonly IValuedRegister<byte> _byteRwRegister;
        private readonly IValuedRegister<short> _shortRwRegister;
        private readonly IValuedRegister<long> _longRwRegister;
        private readonly IValuedRegister<ulong> _ulongRwRegister;

        public int RwInt
        {
            get => _intRwRegister.Value;
            set => SetRegisterValue(_intRwRegister, value);
        }

        public byte RwByte
        {
            get => _byteRwRegister.Value;
            set => SetRegisterValue(_byteRwRegister, value);
        }

        public short RwShort
        {
            get => _shortRwRegister.Value;
            set => SetRegisterValue(_shortRwRegister, value);
        }

        public long RwLong
        {
            get => _longRwRegister.Value;
            set => SetRegisterValue(_longRwRegister, value);
        }

        public ulong RwUlong
        {
            get => _ulongRwRegister.Value;
            set => SetRegisterValue(_ulongRwRegister, value);
        }
        #endregion

        public override IReadOnlyCollection<IRegisterGroup> RegisterGroups { get; }
        public override string Name => nameof(TestDevice);

        private TestDevice(int address, IRegistersUpdatePolicy updatePolicy, DeviceState state) : base(address, updatePolicy, state)
        {
            var validator = new RegisterTableValidator();
            
            _intRoRegister   = validator.AddGroup(Int32RegisterGroup.CreateNew(0x00, RegisterType.Read));
            _byteRoRegister  = validator.AddGroup(UInt8RegisterGroup.CreateNew(0x04, RegisterType.Read));
            _shortRoRegister = validator.AddGroup(Int16RegisterGroup.CreateNew(0x05, RegisterType.Read));
            _longRoRegister  = validator.AddGroup(Int64RegisterGroup.CreateNew(0x07, RegisterType.Read));
            _ulongRoRegister = validator.AddGroup(UInt64RegisterGroup.CreateNew(0x0F, RegisterType.Read));
            
            _intRwRegister   = validator.AddGroup(Int32RegisterGroup.CreateNew(0x80, RegisterType.ReadWrite));
            _byteRwRegister  = validator.AddGroup(UInt8RegisterGroup.CreateNew(0x84, RegisterType.ReadWrite));
            _shortRwRegister = validator.AddGroup(Int16RegisterGroup.CreateNew(0x85, RegisterType.ReadWrite));
            _longRwRegister  = validator.AddGroup(Int64RegisterGroup.CreateNew(0x87, RegisterType.ReadWrite));
            _ulongRwRegister = validator.AddGroup(UInt64RegisterGroup.CreateNew(0x8F, RegisterType.ReadWrite));

            RegisterGroups = validator.GetGroups();
        }
        
        public static TestDevice Create()
        {
            return new TestDevice(0x01, new WriteFirstUpdatePolicy(), DeviceState.Connected);
        }
    }
}