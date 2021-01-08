namespace Nandaka.Core.Registers
{
    public static class RawRegistersFactory
    {
        public static Register<T> Create<T>(int address, T value = default)
            where T: struct
        {
            return new Register<T>(address, RegisterType.Raw, value);
        }
    }
}