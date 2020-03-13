namespace Nandaka.Core.Table
{
    public class RawRegister<T> : IRegister
    {
        public int Address { get; }
        public T Value { get; set; }

        public RawRegister(int address, T value = default)
        {
            Address = address;
            Value = value;
        }
    }
}
