using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    /// <summary>
    /// Return groups ordered by version regardless of register type.
    /// </summary>
    public class OldVersionUpdatePolicy : IRegistersUpdatePolicy
    {
        public OldVersionUpdatePolicy() { }
        
        public IRegisterMessage GetNextMessage(NandakaDevice device)
        {
            IRegisterGroup[] orderedByVersionGroups = device.RegisterGroups
                .OrderBy(group => group.Version)
                .ToArray();

            RegisterType firstGroupType = orderedByVersionGroups.First().RegisterType;

            IRegisterGroup[] groupsToUpdate = orderedByVersionGroups
                .Where(group => group.RegisterType == firstGroupType)
                .ToArray();

            OperationType operationType = firstGroupType == RegisterType.WriteOnly
                ? OperationType.Write
                : OperationType.Read;

            return new CommonMessage(device.Address, MessageType.Request, operationType, groupsToUpdate);
        }
    }
}