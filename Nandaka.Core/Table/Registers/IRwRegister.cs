namespace Nandaka.Core.Table
{
    public interface IRwRegister<TValue> : IRoRegister<TValue>
        where TValue : struct
    {
        new TValue Value { get; set; }
    }
}
