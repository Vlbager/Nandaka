using Nandaka.Core.Device;
using Nandaka.Core.Registers;
using Nandaka.Model.Device;
using Nandaka.Model.Registers;

namespace Nandaka.Tests.Util
{
    internal sealed class TestDevice : ForeignDevice
    {
        public const int TestDeviceAddress = 1;

        private readonly TestRegisters _registers;

        public IReadOnlyRegister<int> RoInt => _registers.RoInt;
        public IReadOnlyRegister<byte> RoByte => _registers.RoByte;
        public IReadOnlyRegister<short> RoShort => _registers.RoShort;
        public IReadOnlyRegister<int> RoInt2 => _registers.RoInt2;
        public IReadOnlyRegister<long> RoLong => _registers.RoLong;

        public IRegister<int> RwInt => _registers.RwInt;
        public IRegister<byte> RwByte => _registers.RwByte;
        public IRegister<short> RwShort => _registers.RwShort;
        public IRegister<int> RwInt2 => _registers.RwInt2;
        public IRegister<long> RwLong => _registers.RwLong;

        public override RegisterTable Table => _registers.Table;
        public override string Name => nameof(TestDevice);
        public override int Address => TestDeviceAddress;

        private TestDevice(TestRegisters registers)
        {
            _registers = registers;
        }

        public TestDevice(IRegisterFactory registerFactory) : this(new TestRegisters(registerFactory))
        {
        }

        public TestDevice() : this(new ForeignDeviceRegisterFactory())
        {
        }
    }
}