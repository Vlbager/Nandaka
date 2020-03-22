using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class MasterSession : ISession
    {
        private readonly Queue<IRegisterGroup> _readQueue = new Queue<IRegisterGroup>();
        private readonly Queue<IRegisterGroup> _writeQueue = new Queue<IRegisterGroup>();

        // Policy for autoUpdate slave device table. Not using yet.
        // ReSharper disable once NotAccessedField.Local
        private object _updatePolicy;

        public MasterSession(IProtocol protocol, object updatePolicy = null)
        {
            Protocol = protocol;
            _updatePolicy = updatePolicy;
        }

        public IProtocol Protocol { get; }

        //public void EnqueueRegisters(IEnumerable<IRegisterGroup> registerGroups, OperationType operationType)
        //{
        //    Queue<IRegisterGroup> queue;
        //    switch (operationType)
        //    {
        //        case OperationType.Read:
        //            queue = _readQueue;
        //            break;

        //        case OperationType.Write:
        //            queue = _writeQueue;
        //            break;

        //        default:
        //            // todo: Create custom exception.
        //            throw new ArgumentException("Wrong message type");

        //    }

        //    foreach (IRegisterGroup register in registerGroups)
        //    {
        //        if (queue.Contains(register))
        //            continue;

        //        queue.Enqueue(register);
        //    }
        //}

        public void SendMessage(SlaveDevice device)
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

            var message = new CommonMessage(device.Address, MessageType.Request, operationType, queue);

            Protocol.SendMessage(message, out IReadOnlyCollection<IRegisterGroup> sentGroups);

            foreach (IRegisterGroup registerGroup in sentGroups)
            {
                queue.Dequeue(registerGroup);
            }
        }
    }
}
