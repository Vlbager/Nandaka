namespace Nandaka.Core.Table
{
    public class RawRegister : IRegisterGroup
    {
        public int Address { get; }
        public int Count => 1;

        public byte Value { get; set; }

        public RawRegister(int address, byte value = 0)
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
