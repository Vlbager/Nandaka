namespace Nandaka.Core.Table
{
    public interface IValuedRegister<out TValue> : IRegister
        where TValue : struct
    {
        TValue Value { get; }
    }
}
