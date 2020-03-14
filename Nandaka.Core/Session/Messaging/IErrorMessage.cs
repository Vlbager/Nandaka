namespace Nandaka.Core.Session
{
    public interface IErrorMessage : IFrameworkMessage
    {
        ErrorType ErrorType { get; }
    }
}
