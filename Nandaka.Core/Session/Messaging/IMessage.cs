namespace Nandaka.Core.Session
{
    public interface IMessage
    {
        int SlaveDeviceAddress { get; }
        MessageType Type { get; }
    }
}
