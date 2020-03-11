using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class SlaveSession<T> : ISession<T>
    {
        private readonly Queue<IRegisterGroup> _queue = new Queue<IRegisterGroup>();

        private MessageType _type;

        public SlaveSession(IDevice device, IProtocol<T> protocol)
        {
            Device = device;
            Protocol = protocol;
        }

        public IDevice Device { get; }
        public IProtocol<T> Protocol { get; }
        public void EnqueueRegisters(IEnumerable<IRegisterGroup> registers, MessageType operationType)
        {
            switch (operationType)
            {
                case MessageType.ReadDataResponse:
                case MessageType.WriteDataResponse:
                    _type = operationType;
                    break;

                default:
                    // todo: Create custom exception.
                    throw new ArgumentException("Wrong operation type");
            }

            foreach (var register in registers)
            {
                if (_queue.Contains(register))
                {
                    continue;
                }

                _queue.Enqueue(register);
            }
        }

        public void SendMessage()
        {
            var message = Protocol.GetMessage(_queue, Device.Address, _type);
            var packet = Protocol.PreparePacket(message);
            if (message.RegistersCount != _queue.Count)
            {
                message = Protocol.GetMessage(Enumerable.Empty<IRegisterGroup>(), Device.Address, MessageType.ErrorMessage,
                    (int)ErrorCode.WrongDataAmount);
                packet = Protocol.PreparePacket(message);
            }

            Protocol.SendPacket(packet);
            _queue.Clear();
        }
    }
}
