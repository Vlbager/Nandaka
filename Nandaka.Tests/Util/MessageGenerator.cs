using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Nandaka.Tests.Util
{
    internal abstract class MessageGenerator
    {
        protected static readonly IEnumerable<OperationType> OperationTypes = new[] { OperationType.Read, OperationType.Write };
        protected static readonly IEnumerable<MessageType> MessageTypes = new[] { MessageType.Request, MessageType.Response };
        
        public static IEnumerable<ErrorMessage> GenerateCommonErrorMessages(IEnumerable<ErrorType> errorTypes, IEnumerable<int> deviceAddresses, bool responseOnly)
        {
            return from deviceAddress in deviceAddresses
                   from errorType in errorTypes
                   from messageType in responseOnly ? MessageType.Response.ToEnumerable() : MessageTypes
                   select ErrorMessage.CreateCommon(deviceAddress, messageType, errorType);
        }

        public static IEnumerable<ErrorMessage> GenerateProtocolErrorMessages(IEnumerable<int> errorCodes, IEnumerable<int> deviceAddresses, 
                                                                              bool responseOnly)
        {
            return from deviceAddress in deviceAddresses
                   from errorCode in errorCodes
                   from messageType in responseOnly ? MessageType.Response.ToEnumerable() : MessageTypes
                   select ErrorMessage.CreateFromProtocol(deviceAddress, messageType, new ProtocolSpecifiedErrorMessage(errorCode));
        }
    }
    
    internal sealed class MessageGenerator<T> : MessageGenerator
        where T: struct
    {
        private readonly RegisterGenerator<T> _registerGenerator;

        public static int RegisterValueSize => RegisterGenerator<T>.RegisterValueSize;
        
        public MessageGenerator()
        {
            _registerGenerator = new RegisterGenerator<T>();
        }

        public IEnumerable<IRegisterMessage> GenerateSingleRegister(int registerAddress, int deviceAddress)
        {
            return GenerateSingleRegister(registerAddress.ToEnumerable(), deviceAddress.ToEnumerable());
        }

        public IEnumerable<IRegisterMessage> GenerateSingleRegister(IEnumerable<int> registerAddresses, IEnumerable<int> deviceAddresses)
        {
            IEnumerable<IRegister[]> registerBatches = _registerGenerator.Generate(registerAddresses)
                                                                         .Select(register => new[] { register });

            return Generate(registerBatches, deviceAddresses);
        }

        public IEnumerable<IRegisterMessage> Generate(IEnumerable<int> messageSizes, IReadOnlyCollection<int> addressPool,
                                                      IEnumerable<int> deviceAddresses)
        {
            IEnumerable<IRegister[]> registerBatches = _registerGenerator.GenerateBatches(messageSizes, addressPool);
            return Generate(registerBatches, deviceAddresses);
        }

        private IEnumerable<IRegisterMessage> Generate(IEnumerable<IRegister[]> registerBatches, IEnumerable<int> deviceAddresses)
        {
            return from registerBatch in registerBatches
                   from deviceAddress in deviceAddresses
                   from operationType in OperationTypes
                   from messageType in MessageTypes
                   select new CommonMessage(deviceAddress, messageType, operationType, registerBatch);
        }
    }
}