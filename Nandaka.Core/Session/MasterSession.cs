using System;
using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class MasterSession<T> : ISession<T>
    {
        private readonly Queue<IRegisterGroup> _readQueue = new Queue<IRegisterGroup>();
        private readonly Queue<IRegisterGroup> _writeQueue = new Queue<IRegisterGroup>();

        // Policy for autoUpdate slave device table. Not using yet.
        // ReSharper disable once NotAccessedField.Local
        private object _updatePolicy;

        public MasterSession(IDevice slaveDevice, IProtocol<T> protocol, object updatePolicy = null)
        {
            SlaveDevice = slaveDevice;
            Protocol = protocol;
            _updatePolicy = updatePolicy;
        }

        public IDevice SlaveDevice { get; }
        public IProtocol<T> Protocol { get; }

        public void EnqueueRegisters(IEnumerable<IRegisterGroup> registerGroups, OperationType operationType)
        {
            Queue<IRegisterGroup> queue;
            switch (operationType)
            {
                case OperationType.Read:
                    queue = _readQueue;
                    break;

                case OperationType.Write:
                    queue = _writeQueue;
                    break;

                default:
                    // todo: Create custom exception.
                    throw new ArgumentException("Wrong message type");

            }

            foreach (IRegisterGroup register in registerGroups)
            {
                if (queue.Contains(register))
                    continue;

                queue.Enqueue(register);
            }
        }

        public void SendMessage()
        {
            // todo: refactor with update policy
            Queue<IRegisterGroup> queue;
            OperationType operationType;
            if (_writeQueue.Count > 0)
            {
                queue = _writeQueue;
                operationType = OperationType.Write;
            }
            else if (_readQueue.Count > 0)
            {
                queue = _readQueue;
                operationType = OperationType.Read;
            }
            else
            {
                // todo: Create a custom exception.
                throw new ApplicationException("There are no register group in queue to send");
            }

            var message = new CommonMessage(SlaveDevice.Address, MessageType.Request, operationType, queue);

            T packet = Protocol.PreparePacket(message);

            Protocol.SendPacket(packet);

            // todo: refactor this logic.
            // All registerGroups that were in the packet, should be removed from queue.
            queue.Clear();

            foreach (IRegisterGroup registerGroup in message.RegisterGroups)
            {
                queue.Enqueue(registerGroup);
            }
        }
    }
}
