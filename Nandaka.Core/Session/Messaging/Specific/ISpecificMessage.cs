namespace Nandaka.Core.Session
{
    public interface ISpecificMessage : IMessage
    {
        int SpecificCode { get; }
        byte[] MessageData { get; }
    }
}
