using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka.Core
{
    public class SlaveSession<T> : ISession<T>
    {
        private readonly Queue<IRegister> _queue = new Queue<IRegister>();

        private MessageType _type;

        public SlaveSession(IDevice device, IProtocol<T> protocol)
        {
            Device = device;
            Protocol = protocol;
        }

        public IDevice Device { get; }
        public IProtocol<T> Protocol { get; }
        public void EnqueueRegisters(IEnumerable<IRegister> registers, MessageType operationType)
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
                message = Protocol.GetMessage(Enumerable.Empty<IRegister>(), Device.Address, MessageType.ErrorMessage,
                    (int)ErrorCode.WrongDataAmount);
                packet = Protocol.PreparePacket(message);
            }

            Protocol.SendPacket(packet);
            _queue.Clear();
        }
    }
}
