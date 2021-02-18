namespace Nandaka.Core.Session
{
    public interface IResponseSession<in TResponseMessage>
        where TResponseMessage: IMessage
    {
        void ProcessResponse(TResponseMessage message);
    }
}