namespace Nandaka.Core.Session
{
    public interface IErrorMessage : IMessage
    {
        ErrorType ErrorType { get; }
    }
}
