namespace Nandaka.Core.Protocol
{
    public interface IRegister
    {
        int Address { get; }

        byte[] GetBytes();
    }
}
