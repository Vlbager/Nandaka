using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Helpers;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    /// <summary>
    /// Returns write only groups ordered by version, then by address, if exists.
    /// Else returns read only groups in same order.
    /// </summary>
    public class WriteFirstUpdatePolicy : IRegistersUpdatePolicy
    {
        public WriteFirstUpdatePolicy() { }
        
        public IRegisterMessage GetNextMessage(NandakaDevice device)
        {
            IRegisterGroup[] writeOnlyGroupsToUpdate = device.RegisterGroups
                .Where(group => group.RegisterType == RegisterType.WriteOnly)
                .Where(group => !group.IsUpdated)
                .OrderBy(group => group.Version)
                .ThenBy(group => group.Address)
                .ToArray();

            if (!writeOnlyGroupsToUpdate.IsEmpty())
                return new CommonMessage(device.Address, MessageType.Request, OperationType.Write, writeOnlyGroupsToUpdate);

            IRegisterGroup[] readOnlyGroupsToUpdate = device.RegisterGroups
                .Where(group => group.RegisterType == RegisterType.ReadOnly)
                .OrderBy(group => group.Version)
                .ThenBy(group => group.Address)
                .ToArray();

            return new CommonMessage(device.Address, MessageType.Request, OperationType.Read, readOnlyGroupsToUpdate);
        }
    }
}