using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class SlaveSession<T> : ISession<T>
    {
        private readonly Queue<IRegisterGroup> _queue = new Queue<IRegisterGroup>();

        private OperationType _type;

        public SlaveSession(IDevice slaveDevice, IProtocol<T> protocol)
        {
            SlaveDevice = slaveDevice;
            Protocol = protocol;
        }

        public IDevice SlaveDevice { get; }
        public IProtocol<T> Protocol { get; }
        public void EnqueueRegisters(IEnumerable<IRegisterGroup> registerGroups, OperationType operationType)
        {
            // todo: refactor slave device design.
            _type = operationType;

            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                if (_queue.Contains(registerGroup))
                    continue;

                _queue.Enqueue(registerGroup);
            }
        }

        public void SendMessage()
        {
            var message = new CommonMessage(SlaveDevice.Address, MessageType.Response, _type);
            T packet = Protocol.PreparePacket(message);

            // todo: refactor this logic.
            if (message.RegisterGroups.Count != _queue.Count)
            {
                var errorMessage = new CommonErrorMessage(SlaveDevice.Address, MessageType.Response, ErrorType.TooMuchDataRequested);
                packet = Protocol.PreparePacket(errorMessage);
            }

            Protocol.SendPacket(packet);
            _queue.Clear();
        }
    }
}
