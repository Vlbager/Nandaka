using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    /// <summary>
    /// Return groups ordered by version regardless of register type.
    /// </summary>
    public class OldVersionUpdatePolicy : IRegistersUpdatePolicy
    {
        public OldVersionUpdatePolicy() { }
        
        public IRegisterMessage GetNextMessage(ForeignDevice device)
        {
            IRegister[] orderedByVersionRegisters = device.Table
                .OrderBy(register => register.LastUpdateTime)
                .ToArray();

            RegisterType firstRegisterType = orderedByVersionRegisters.First().RegisterType;

            IRegister[] registersToUpdate = orderedByVersionRegisters
                .Where(register => register.RegisterType == firstRegisterType)
                .ToArray();

            OperationType operationType = firstRegisterType == RegisterType.WriteRequest
                ? OperationType.Write
                : OperationType.Read;

            return new CommonMessage(device.Address, MessageType.Request, operationType, registersToUpdate);
        }
    }
}