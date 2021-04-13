namespace Nandaka.Core.Session
{
    public interface IResponseSession<in TResponseMessage>
        where TResponseMessage: IMessage
    {
        void ProcessRequest(TResponseMessage request);
    }
}