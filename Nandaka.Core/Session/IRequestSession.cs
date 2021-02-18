namespace Nandaka.Core.Session
{
    public interface IRequestSession<TRequestMessage, TSentResult>
        where TRequestMessage: IMessage
        where TSentResult: ISentResult
    {
        TRequestMessage GetNextMessage();
        TSentResult SendRequest(TRequestMessage message);
        void ProcessResponse(IMessage message, TSentResult sentResult);
    }
}