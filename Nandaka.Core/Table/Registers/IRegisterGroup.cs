namespace Nandaka.Core.Table
{
    public interface IRegisterGroup
    {
        int Address { get; }
        int Count { get; }
        byte[] GetBytes();
    }
}
