namespace Nandaka.Core.Table
{
    public interface IValuedRegister<TValue> : IRegister
        where TValue : struct
    {
        TValue Value { get; set; }
    }
}
