namespace Nandaka.Core.Table
{
    public interface IRoRegister<TValue> : IRegister
        where TValue : struct
    {
        TValue Value { get; }
    }
}