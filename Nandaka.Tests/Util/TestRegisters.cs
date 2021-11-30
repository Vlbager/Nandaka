using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Tests.Util
{
    public sealed class TestRegisters
    {
        public RegisterTable Table { get; }

        public IReadOnlyRegister<int> RoInt { get; }
        public IReadOnlyRegister<byte> RoByte { get; }
        public IReadOnlyRegister<short> RoShort { get; }
        public IReadOnlyRegister<int> RoInt2 { get; }
        public IReadOnlyRegister<long> RoLong { get; }

        public IRegister<int> RwInt { get; }
        public IRegister<byte> RwByte { get; }
        public IRegister<short> RwShort { get; }
        public IRegister<int> RwInt2 { get; }
        public IRegister<long> RwLong { get; }

        public TestRegisters(IRegisterFactory factory)
        {
            RoInt = factory.CreateReadOnly<int>(0);
            RoByte = factory.CreateReadOnly<byte>(1);
            RoShort = factory.CreateReadOnly<short>(2);
            RoInt2 = factory.CreateReadOnly<int>(3);
            RoLong = factory.CreateReadOnly<long>(4);

            RwInt = factory.Create<int>(10);
            RwByte = factory.Create<byte>(11);
            RwShort = factory.Create<short>(12);
            RwInt2 = factory.Create<int>(13);
            RwLong = factory.Create<long>(14);

            IRegister[] registers =
            {
                RoInt, RoByte, RoShort, RoInt2, RoLong,
                RwInt, RwByte, RwShort, RwInt2, RwLong
            };
            
            Table = RegisterTable.CreateWithValidation(registers);
        }
    }
}