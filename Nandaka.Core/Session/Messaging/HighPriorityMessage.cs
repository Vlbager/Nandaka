namespace Nandaka.Core.Session
{
    public class HighPriorityMessage : IFrameworkMessage
    {
        public IFrameworkMessage InnerMessage { get; }
        public int SlaveDeviceAddress => InnerMessage.SlaveDeviceAddress;
        public MessageType Type => InnerMessage.Type;

        public HighPriorityMessage(IFrameworkMessage innerMessage)
        {
            InnerMessage = innerMessage;
        }
    }
}