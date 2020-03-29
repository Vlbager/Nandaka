namespace Nandaka.Core.Session
{
    public interface ISpecificMessage : IFrameworkMessage
    {
        int SpecificCode { get; }
        byte[] MessageData { get; }
    }
}
