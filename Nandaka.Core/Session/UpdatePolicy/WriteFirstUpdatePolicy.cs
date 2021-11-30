using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Session
{
    /// <summary>
    /// Returns write only groups ordered by address, if exists.
    /// Else returns read only groups.
    /// </summary>
    public class WriteFirstUpdatePolicy : IRegistersUpdatePolicy
    {
        public IRegisterMessage GetNextMessage(ForeignDevice device)
        {
            IRegister[] writeRegistersToUpdate = device.Table
                                                          .Where(register => register.RegisterType == RegisterType.WriteRequest)
                                                          .Where(register => !register.IsUpdated)
                                                          .OrderBy(register => register.Address)
                                                          .ToArray();

            if (!writeRegistersToUpdate.IsEmpty())
                return new CommonMessage(device.Address, MessageType.Request, OperationType.Write, writeRegistersToUpdate);

            IRegister[] readRegistersToUpdate = device.Table
                                                         .Where(register => register.RegisterType == RegisterType.ReadRequest)
                                                         .OrderBy(register => register.LastUpdateTime)
                                                         .ThenBy(register => register.Address)
                                                         .ToArray();

            if (!readRegistersToUpdate.IsEmpty())
                return new CommonMessage(device.Address, MessageType.Request, OperationType.Read, readRegistersToUpdate);

            return EmptyMessage.Create();
        }
    }
}