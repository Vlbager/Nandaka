namespace Nandaka.Core.Session
{
    public interface IErrorMessageHandler
    {
        void OnErrorReceived(ErrorMessage errorMessage);
    }
}