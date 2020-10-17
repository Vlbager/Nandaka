using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Tests.Common
{
    internal class MessageGenerator
    {
        private static readonly IEnumerable<OperationType> OperationTypes = new[] { OperationType.Read, OperationType.Write };
        private static readonly IEnumerable<MessageType> MessageTypes = new[] { MessageType.Request, MessageType.Response };
        
        private readonly RegisterGenerator _registerGenerator;

        public int RegisterSize => _registerGenerator.RegisterSize;
        
        public MessageGenerator(RegisterGenerator registerGenerator)
        {
            _registerGenerator = registerGenerator;
        }

        public IEnumerable<IRegisterMessage> GenerateSingleRegister(int registerAddress, int deviceAddress)
        {
            return GenerateSingleRegister(registerAddress.ToEnumerable(), deviceAddress.ToEnumerable());
        }

        public IEnumerable<IRegisterMessage> GenerateSingleRegister(IEnumerable<int> registerAddresses, IEnumerable<int> deviceAddresses)
        {
            IEnumerable<IRegisterGroup[]> registerBatches = _registerGenerator.Generate(registerAddresses)
                                                                              .Select(register => new[] { register });

            return Generate(registerBatches, deviceAddresses);
        }

        public IEnumerable<IRegisterMessage> Generate(IEnumerable<int> messageSizes, IReadOnlyCollection<int> registerAddressesRange,
                                                      IEnumerable<int> deviceAddresses)
        {
            IEnumerable<IRegisterGroup[]> registerBatches = _registerGenerator.GenerateBatches(messageSizes, registerAddressesRange);
            return Generate(registerBatches, deviceAddresses);
        }

        private IEnumerable<IRegisterMessage> Generate(IEnumerable<IRegisterGroup[]> registerBatches, IEnumerable<int> deviceAddresses)
        {
            return from registerBatch in registerBatches
                   from deviceAddress in deviceAddresses
                   from operationType in OperationTypes
                   from messageType in MessageTypes
                   select new CommonMessage(deviceAddress, messageType, operationType, registerBatch);
        }
    }
}