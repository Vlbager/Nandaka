using System;
using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class MasterSession<T> : ISession<T>
    {
        private readonly Queue<IRegister> _readQueue = new Queue<IRegister>();
        private readonly Queue<IRegister> _writeQueue = new Queue<IRegister>();

        // Policy for autoUpdate device table. Not using yet.
        // ReSharper disable once NotAccessedField.Local
        private object _updatePolicy;

        public MasterSession(IDevice device, IProtocol<T> protocol, object updatePolicy = null)
        {
            Device = device;
            Protocol = protocol;
            _updatePolicy = updatePolicy;
        }

        public IDevice Device { get; }
        public IProtocol<T> Protocol { get; }

        public void EnqueueRegisters(IEnumerable<IRegister> registers, MessageType operationType)
        {
            Queue<IRegister> queue;
            switch (operationType)
            {
                case MessageType.ReadDataRequest:
                    queue = _readQueue;
                    break;

                case MessageType.WriteDataRequest:
                    queue = _writeQueue;
                    break;

                default:
                    // todo: Create custom exception.
                    throw new ArgumentException("Wrong operation type");

            }

            foreach (var register in registers)
            {
                if (queue.Contains(register))
                {
                    continue;
                }

                queue.Enqueue(register);
            }
        }

        public void SendMessage()
        {
            IMessage message;
            Queue<IRegister> queue;
            if (_writeQueue.Count > 0)
            {
                queue = _writeQueue;
                message = Protocol.GetMessage(_writeQueue, Device.Address, MessageType.WriteDataRequest);
            }
            else if (_readQueue.Count > 0)
            {
                queue = _readQueue;
                message = Protocol.GetMessage(_readQueue, Device.Address, MessageType.ReadDataRequest);
            }
            else
            {
                // todo: Create a custom exception.
                throw new ApplicationException("There are no register in queue to send");
            }

            var packet = Protocol.PreparePacket(message);

            Protocol.SendPacket(packet);

            // All registers that were in the packet, should be removed from queue.
            queue.Clear();
            foreach (var register in message.Registers)
            {
                queue.Enqueue(register);
            }
        }
    }
}
