using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    /// <summary>
    /// Returns write only groups ordered by address, if exists.
    /// Else returns read only groups.
    /// </summary>
    public class WriteFirstUpdatePolicy : IRegistersUpdatePolicy
    {
        public WriteFirstUpdatePolicy() { }
        
        public IRegisterMessage GetNextMessage(ForeignDeviceCtx deviceCtx)
        {
            IRegister[] writeRegistersToUpdate = deviceCtx.Registers
                                                          .Where(register => register.RegisterType == RegisterType.WriteRequest)
                                                          .Where(register => !register.IsUpdated)
                                                          .OrderBy(register => register.Address)
                                                          .ToArray();

            if (!writeRegistersToUpdate.IsEmpty())
                return new CommonMessage(deviceCtx.Address, MessageType.Request, OperationType.Write, writeRegistersToUpdate);

            IRegister[] readRegistersToUpdate = deviceCtx.Registers
                                                         .Where(register => register.RegisterType == RegisterType.ReadRequest)
                                                         .OrderBy(register => register.LastUpdateTime)
                                                         .ThenBy(register => register.Address)
                                                         .ToArray();

            if (!readRegistersToUpdate.IsEmpty())
                return new CommonMessage(deviceCtx.Address, MessageType.Request, OperationType.Read, readRegistersToUpdate);

            return new EmptyMessage();
        }
    }
}