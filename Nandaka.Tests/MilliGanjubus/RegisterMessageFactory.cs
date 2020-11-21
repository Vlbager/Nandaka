using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Nandaka.Tests.Common;

namespace Nandaka.Tests.MilliGanjubus
{
    public class RegisterMessageFactory
    {
        private readonly int _deviceAddress;

        public RegisterMessageFactory(int deviceAddress)
        {
            _deviceAddress = deviceAddress;
        }

        public IRegisterMessage CreateRange(int firstRegisterAddress, int registersCount, OperationType operationType, MessageType messageType)
        {
            IEnumerable<int> addresses = Enumerable.Range(firstRegisterAddress, registersCount);

            return CreateInternal(addresses, operationType, messageType);
        }

        /// <summary>
        /// Creates message with registers with n + 2 addresses
        /// </summary>
        public IRegisterMessage CreateSeries(int firstRegisterAddress, int registersCount, OperationType operationType, MessageType messageType)
        {
            IEnumerable<int> addresses = Enumerable.Range(firstRegisterAddress, registersCount * 2)
                .SkipEvery(1);

            return CreateInternal(addresses, operationType, messageType);
        }

        private IRegisterMessage CreateInternal(IEnumerable<int> addresses, OperationType operationType, MessageType messageType)
        {
            RegisterType registerType = GetRegisterType(operationType);
            
            UInt8RegisterGroup[] registers = addresses
                .Select(address => UInt8RegisterGroup.CreateNew(address, registerType))
                .ToArray();

            return new CommonMessage(_deviceAddress, messageType, operationType, registers);
        }
        

        private static RegisterType GetRegisterType(OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Read:
                    return RegisterType.Read;
                case OperationType.Write:
                    return RegisterType.ReadWrite;
                default:
                    throw new NandakaBaseException("Wrong operation type");
            }
        }
    }
}