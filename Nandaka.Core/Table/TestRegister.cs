using Nandaka.Core.Protocol;

namespace Nandaka.Core.Table
{
    public class TestByteRegister : IRegister
    {
        public int Address { get; }

        public byte Value { get; set; }

        public TestByteRegister(int address, byte value = 0)
        {
            Address = address;
            Value = value;
        }

        public byte[] GetBytes()
        {
            return new [] { Value };
        }
    }
}
