using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    /// <summary>
    /// Return groups ordered by last update time regardless of register type.
    /// </summary>
    public class LastTimeUpdatePolicy : IRegistersUpdatePolicy
    {
        public IRegisterMessage GetNextMessage(ForeignDevice device)
        {
            if (device.Table.IsEmpty())
                return EmptyMessage.Create();
            
            RegisterType firstRegisterType = device.Table
                                                   .OrderBy(register => register.LastUpdateTime)
                                                   .First().RegisterType;

            IRegister[] registersToUpdate = device.Table
                                                  .Where(register => register.RegisterType == firstRegisterType)
                                                  .OrderBy(register => register.LastUpdateTime)
                                                  .ThenBy(register => register.Address)
                                                  .ToArray();

            OperationType operationType = firstRegisterType == RegisterType.WriteRequest
                ? OperationType.Write
                : OperationType.Read;

            return new CommonMessage(device.Address, MessageType.Request, operationType, registersToUpdate);
        }
    }
}