using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Helpers;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    /// <summary>
    /// Returns write only groups ordered by address, if exists.
    /// Else returns read only groups.
    /// </summary>
    public class WriteFirstUpdatePolicy : IRegistersUpdatePolicy
    {
        public WriteFirstUpdatePolicy() { }
        
        public IRegisterMessage GetNextMessage(NandakaDevice device)
        {
            IRegisterGroup[] writeOnlyGroupsToUpdate = device.RegisterGroups
                .Where(group => group.RegisterType == RegisterType.ReadWrite)
                .Where(group => !group.IsUpdated)
                .OrderBy(group => group.Address)
                .ToArray();

            if (!writeOnlyGroupsToUpdate.IsEmpty())
                return new CommonMessage(device.Address, MessageType.Request, OperationType.Write, writeOnlyGroupsToUpdate);

            IRegisterGroup[] readOnlyGroupsToUpdate = device.RegisterGroups
                .Where(group => group.RegisterType == RegisterType.Read)
                .OrderBy(group => group.LastUpdateTime)
                .ThenBy(group => group.Address)
                .ToArray();

            if (!readOnlyGroupsToUpdate.IsEmpty())
                return new CommonMessage(device.Address, MessageType.Request, OperationType.Read, readOnlyGroupsToUpdate);

            return new EmptyMessage();
        }
    }
}