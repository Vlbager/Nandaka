namespace Nandaka.Core.Table
{
    internal abstract class RegisterGroupBase<TGroupType, TRegisterType> : IRegisterGroup 
        where TGroupType : struct
        where TRegisterType : struct
    {
        public int Address { get; }
        public int Count { get; }
        public RegisterTable<TRegisterType> Table { get; }

        public abstract byte[] GetBytes();
        public abstract TGroupType Value { get; }

        protected RegisterGroupBase(RegisterTable<TRegisterType> table, int address, int count)
        {
            Table = table;
            Address = address;
            Count = count;
        }
    }
}
