namespace Nandaka.Core.Registers
{
    public static class RawRegistersFactory
    {
        public static Register<T> Create<T>(int address, T value)
            where T: struct
        {
            return new Register<T>(address, RegisterType.Raw, value);
        }

        public static Register<T> Create<T>(int address)
            where T: struct
        {
            return new Register<T>(address, RegisterType.RawWithoutValues);
        }
    }
}