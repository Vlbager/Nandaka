namespace Nandaka.Core.Session
{
    public class HighPriorityMessage : IMessage
    {
        public IMessage InnerMessage { get; }
        public int SlaveDeviceAddress => InnerMessage.SlaveDeviceAddress;
        public MessageType MessageType => InnerMessage.MessageType;

        public HighPriorityMessage(IMessage innerMessage)
        {
            InnerMessage = innerMessage;
        }
    }
}